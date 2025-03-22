using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System;

public class M4_Weapon : MonoBehaviour
{
    #region FIELDS
    [Header("General Component")]
    public Camera camera;
    public TMP_Text ammoText;
    public TMP_Text magCountText;
    public Transform weaponRunPosition;
    [Space(3)]

    [Header("VFX")]
    public GameObject vfx;
    public GameObject vfxBulletHole;
    public GameObject vfxMuzzleFlash;
    public GameObject magazine;
    [Space(3)]

    [Header("Script references")]
    public InspectScript inspectScript;
    public EventStackHandler eventStackHandler;
    public Player player;
    [Space(3)]

    [Header("Muzzle Transform")]
    public Transform muzzlePosition;
    [Space(3)]

    [Header("Sight in Settings")]
    public Transform sightInPosition;
    public Transform sightInRotation;
    public float sightInSpeed;
    public float sightInThreshold;

    public List<WeaponAttachmentModifier> equipedAttachment = new List<WeaponAttachmentModifier>();
    [Space]

    [Header("Weapon Reload Settings")]
    public int magSize;         // Maximum ammo per magazine
    public int ammo;            // Current ammo in the magazine
    public int mag;             // Number of magazines remaining

    [Header("Weapon animation Settings")]
    [SerializeField] private float amplitude;       // Strength of weapon sway during movement
    [SerializeField] private float movementBlend;   // Smooths transition between movement states

    //private variables
    private Vector3 originalPosition;
    private Vector3 originalMagPosition;
    private Quaternion originalMagRotation;
    private Quaternion originalRotation;


    [Header("Booleans")]
    public bool rayHasHit;
    public bool isAiming;
    public bool isReturning;
    public bool isReloading;

    private GameInput gameInput;
    private FireHandle fireHandle;
    private RecoilHandle recoilHandle;
    #endregion

    #region UNITY METHODS
    private void OnEnable()
    {
        gameInput = new GameInput();
        gameInput.Enable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeComponents();
        InitializeAmmo();
        StoreInitialTransforms();

        isAiming = false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRecoilTimer();
        HandleSightIn();
        ProcessInput();
        HandleRecoilChecks();
        HandleAnimations();
        UpdateUI();
    }
    #endregion

    #region INITIALIZATION
    private void InitializeComponents()
    {
        //Get components
        fireHandle = GetComponent<FireHandle>();
        recoilHandle = GetComponent<RecoilHandle>();
    }
    private void InitializeAmmo()
    {
        ammo = magSize;
        UpdateAmmoUI();
    }
    private void StoreInitialTransforms()
    {
        // Store starting positions and rotations for reset purposes
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalMagPosition = magazine.transform.localPosition;
        originalMagRotation = magazine.transform.localRotation;
    }
    #endregion

    #region INPUT HANDLING
    private void ProcessInput()
    {
        float fireButton = gameInput.Player.Fire.ReadValue<float>();
        float reloadButton = gameInput.Player.Reload.ReadValue<float>();

        if (CanFire(fireButton))
        {
            HandleFire();
        }
        else if (fireButton <= 0)
        {
            ResetFireEvents();
        }

        if (CanReload(reloadButton))
        {
            Reload();
        }
    }
    private bool CanFire(float fireButton)
    {
        // Check if firing is allowed based on input, timing, ammo, and inspection state
        return fireButton > 0.1f &&
               recoilHandle.nextFire <= 0 &&
               ammo > 0 &&
               !inspectScript.isInspecting;
    }
    private bool CanReload(float reloadButton)
    {
        // Conditions to allow reloading
        return reloadButton > 0.1f &&
               ammo < magSize &&
               mag >= 0 &&
               !isReloading;
    }
    #endregion

    #region WEAPON MECHANICS
    private void HandleFire()
    {
        fireHandle.canFire = true;
        recoilHandle.nextFire = 1f / recoilHandle.fireRate; // Set delay between shots
        ammo--;
        UpdateAmmoUI();
        fireHandle.Fire();
    }
    private void ResetFireEvents()
    {
        // Manage firing event stack for clean state transitions
        if (eventStackHandler.hasFiredEvent && !eventStackHandler.hasPoppedEvent)
        {
            eventStackHandler.PopEvent();
            eventStackHandler.hasPoppedEvent = true;
        }
        eventStackHandler.ResetEvent();
    }
    private void Reload()
    {
        if (mag > 0 && ammo < magSize && !isReloading)
        {
            isReloading = true;
            fireHandle.canFire = false;
            recoilHandle.recoiling = false;
            mag--;

            // Decide target position/rotation based on aiming state
            Vector3 targetPos = isAiming ? GetAimedPosition() : originalMagPosition;
            Quaternion targetRot = isAiming ? GetAimedRotation() : originalMagRotation;

            Rigidbody rb = magazine.GetComponent<Rigidbody>();
            MeshCollider mc = magazine.GetComponent<MeshCollider>();
            rb.isKinematic = false;
            mc.convex = true;

            magazine.transform.SetParent(null); // Detach magazine for animation

            StartCoroutine(MagReloadMagic(rb, mc, targetPos, targetRot));
        }
    }
    #endregion

    #region ANIMATIONS
    private void HandleAnimations()
    {
        // Only animate if not inspecting, reloading, recoiling, or aiming
        if (!inspectScript.isInspecting && !isReloading && !recoilHandle.recoiling && !isAiming)
        {
            HandleWalkAnimation();
            HandleRunAnimation();
        }
    }
    private void HandleWalkAnimation()
    {
        if (player == null) return;

        float blendTarget = player.isWalking ? 1.0f : 0.0f;
        movementBlend = Mathf.Lerp(movementBlend, blendTarget, Time.deltaTime * 4f);

        // Simulate weapon sway based on walk speed
        float t = Time.time * player.walkSpeed;
        float offsetX = Mathf.Sin(t) * amplitude * movementBlend;
        float offsetY = Mathf.Cos(2 * t) * amplitude * 0.5f * movementBlend;

        transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);
    }
    private void HandleRunAnimation()
    {

        if (player == null) return;

        float blendTarget = player.isRunning ? 1.0f : 0.0f;

        movementBlend = Mathf.Lerp(movementBlend, blendTarget, Time.deltaTime * 10f);

        float t = Time.time * player.runSpeed;
        float offsetX = Mathf.Sin(t) * amplitude * movementBlend;
        Vector3 targetPosition = new Vector3(0.017f, -0.255f, 0.278f) + new Vector3(offsetX, 0, 0);
        Quaternion targetRotation = Quaternion.Euler(-34, 114, -7.6f);

        if (player.isRunning)
        {
            StartCoroutine(MoveToPosition(targetPosition, targetRotation));
        }
    }
    #endregion

    #region AIMING
    private void HandleSightIn()
    {
        // Toggle aiming state based on input
        float inputKey = gameInput.Player.Attack.ReadValue<float>();

        if (inputKey >= 0.1f && !isAiming && !isReturning && !inspectScript.isInspecting)
        {
            StartCoroutine(MoveToSightInPosition(sightInPosition.localPosition, sightInPosition.localRotation));
            isAiming = true;
        }
        else if (inputKey >= 0.1f && isAiming && !isReturning)
        {
            StartCoroutine(MoveToSightInPosition(originalPosition, originalRotation));
            isAiming = false;
        }
    }
    #endregion

    #region RECOIL UTILITY
    private void HandleRecoilChecks()
    {
        if (recoilHandle.recoiling && !inspectScript.isInspecting && ammo > 0)
        {
            // Apply different recoil types based on aiming
            if (isAiming)
            {
                recoilHandle.AimRecoil();
            }
            else
            {
                recoilHandle.HipRecoil();
            }
        }

        //has recoiled?
        if (recoilHandle.recovering)
        {
            recoilHandle.Recovering();
        }
    }
    private void UpdateRecoilTimer()
    {
        // Countdown to next allowed shot
        if (recoilHandle.nextFire > 0)
        {
            recoilHandle.nextFire -= Time.deltaTime;
        }
    }
    public void ApplyRecoil()
    {
        // Add recoil modifiers from equipped attachments
        foreach (var attachment in equipedAttachment)
        {
            recoilHandle.verticalRecoil += attachment.VerticalRecoilModifier;
            recoilHandle.horizontalRecoil += attachment.HorizontalRecoilModifier;
        }
    }
    public void UpdateUI()
    {
        UpdateAmmoUI();
        recoilHandle.verticalRecoilText.text = $"Vertical: {recoilHandle.verticalRecoil}";
        recoilHandle.horizontalRecoilText.text = $"Horizontal: {recoilHandle.horizontalRecoil}";
    }
    private void UpdateAmmoUI()
    {
        ammoText.text = $"{ammo}/{magSize}";
        magCountText.text = $"Mag Count: {mag}";
    }

    public Vector3 GetAimedPosition() => sightInPosition.localPosition;
    public Quaternion GetAimedRotation() => sightInRotation.localRotation;
    public Vector3 GetCurrentOriginalPosition() => isAiming ? GetAimedPosition() : originalPosition;
    public Quaternion GetCurrentOriginalRotation() => isAiming ? GetAimedRotation() : originalRotation;
    #endregion

    #region COROUTINES
    private IEnumerator MagReloadMagic(Rigidbody rb, MeshCollider mc, Vector3 targetPos, Quaternion targetRot)
    {
        float elapsedTime = 0;
        float duration = 1f;

        Vector3 startPosition = magazine.transform.position;
        Quaternion startRotation = magazine.transform.rotation;

        yield return new WaitForSeconds(1f);    // Delay before magazine reattaches

        rb.isKinematic = true;
        // Smoothly move magazine back to its slot
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            magazine.transform.rotation = Quaternion.Lerp(startRotation, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        magazine.transform.position = targetPos;
        magazine.transform.rotation = targetRot;
        magazine.transform.SetParent(transform);
        magazine.transform.localPosition = originalMagPosition;
        magazine.transform.localRotation = originalMagRotation;

        ammo = magSize;
        fireHandle.canFire = true;
        isReloading = false;
        ammoText.text = ammo + "/" + magSize;
        magCountText.text = "Mag Count: " + mag.ToString();
    }
    private IEnumerator MoveToPosition(Vector3 targetPos, Quaternion targetRot)
    {
        float elapsedTime = 0f;
        Vector3 originalPosition = transform.localPosition;
        Quaternion originalRotation = transform.localRotation;

        // Lerp to target position and rotation smoothly
        while (elapsedTime < 1f)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, targetPos, elapsedTime);
            transform.localRotation = Quaternion.Lerp(originalRotation, targetRot, elapsedTime);

            elapsedTime += Time.deltaTime * 2f;

            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localRotation = targetRot;
    }
    private IEnumerator MoveToSightInPosition(Vector3 targetPos, Quaternion targetRot)
    {
        isReturning = true;

        float elapsedTime = 0f;
        Vector3 originalPosition = transform.localPosition;
        Quaternion originalRotation = transform.localRotation;

        // Move to sight-in position with customizable speed
        while (elapsedTime < sightInThreshold)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, targetPos, elapsedTime);
            transform.localRotation = Quaternion.Lerp(originalRotation, targetRot, elapsedTime);

            elapsedTime += Time.deltaTime * sightInSpeed;

            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localRotation = targetRot;
        isReturning = false;
    }
    #endregion
}