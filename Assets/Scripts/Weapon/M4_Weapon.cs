using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class M4_Weapon : MonoBehaviour
{
    #region FIELDS
    [Header("General Components")]
    public Camera camera;
    public TMP_Text ammoText;
    public TMP_Text magCountText;
    public Transform weaponRunPosition;

    [Header("VFX")]
    public GameObject vfx;
    public GameObject vfxBulletHole;
    public GameObject vfxMuzzleFlash;
    public GameObject magazine;

    [Header("Script References")]
    public InspectScript inspectScript;
    public EventStackHandler eventStackHandler;
    public Player player;

    [Header("Muzzle Transform")]
    public Transform muzzlePosition;

    [Header("Sight-In Settings")]
    public Transform sightInPosition;
    public Transform sightInRotation;
    public float sightInSpeed;
    public float sightInThreshold;

    [Header("Attachments")]
    public List<WeaponAttachmentModifier> equippedAttachments = new List<WeaponAttachmentModifier>();

    [Header("Weapon Reload Settings")]
    public int magSize;
    public int ammo;
    public int mag;

    [Header("Weapon Animation Settings")]
    [SerializeField] private float amplitude;
    [SerializeField] private float movementBlend;

    [Header("State")]
    public bool rayHasHit;
    public bool isAiming;
    public bool isReturning;
    public bool isReloading;

    private Vector3 originalPosition;
    private Vector3 originalMagPosition;
    private Quaternion originalMagRotation;
    private Quaternion originalRotation;

    private Rigidbody magazineRigidbody;
    private MeshCollider magazineMeshCollider;

    private GameInput gameInput;
    private FireHandle fireHandle;
    private RecoilHandle recoilHandle;

    private Coroutine movementCoroutine;
    private Coroutine sightCoroutine;
    #endregion

    #region UNITY METHODS
    private void OnEnable()
    {
        gameInput = new GameInput();
        gameInput.Enable();
    }

    private void OnDisable()
    {
        gameInput.Disable();
    }

    void Start()
    {
        InitializeComponents();
        InitializeAmmo();
        StoreInitialTransforms();
        isAiming = false;
    }

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
        fireHandle = GetComponent<FireHandle>();
        recoilHandle = GetComponent<RecoilHandle>();
        magazineRigidbody = magazine.GetComponent<Rigidbody>();
        magazineMeshCollider = magazine.GetComponent<MeshCollider>();
    }

    private void InitializeAmmo()
    {
        ammo = magSize;
    }

    private void StoreInitialTransforms()
    {
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
        else if (fireButton <= 0f)
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
        return fireButton > 0.1f
            && recoilHandle.nextFire <= 0f
            && ammo > 0
            && !inspectScript.isInspecting;
    }

    private bool CanReload(float reloadButton)
    {
        return reloadButton > 0.1f
            && ammo < magSize
            && mag > 0
            && !isReloading;
    }
    #endregion

    #region WEAPON MECHANICS
    private void HandleFire()
    {
        fireHandle.canFire = true;
        recoilHandle.nextFire = 1f / recoilHandle.fireRate;
        ammo--;
        fireHandle.Fire();
    }

    private void ResetFireEvents()
    {
        if (eventStackHandler.hasFiredEvent && !eventStackHandler.hasPoppedEvent)
        {
            eventStackHandler.PopEvent();
            eventStackHandler.hasPoppedEvent = true;
        }
        eventStackHandler.ResetEvent();
    }

    private void Reload()
    {
        isReloading = true;
        fireHandle.canFire = false;
        recoilHandle.recoiling = false;
        mag--;

        magazineRigidbody.isKinematic = false;
        magazineMeshCollider.convex = true;
        magazine.transform.SetParent(null);

        Vector3 targetPos = isAiming ? GetAimedPosition() : originalMagPosition;
        Quaternion targetRot = isAiming ? GetAimedRotation() : originalMagRotation;

        StartCoroutine(MagReloadSequence(targetPos, targetRot));
    }
    #endregion

    #region ANIMATIONS
    private void HandleAnimations()
    {
        if (inspectScript.isInspecting || isReloading || recoilHandle.recoiling || isAiming)
            return;

        if (player == null) return;

        if (player.isRunning)
        {
            HandleRunAnimation();
        }
        else
        {
            HandleWalkAnimation();
        }
    }

    private void HandleWalkAnimation()
    {
        float blendTarget = player.isWalking ? 1f : 0f;
        movementBlend = Mathf.Lerp(movementBlend, blendTarget, Time.deltaTime * 4f);

        float t = Time.time * player.walkSpeed;
        float offsetX = Mathf.Sin(t) * amplitude * movementBlend;
        float offsetY = Mathf.Cos(2f * t) * amplitude * 0.5f * movementBlend;

        transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
    }

    private void HandleRunAnimation()
    {
        movementBlend = Mathf.Lerp(movementBlend, 1f, Time.deltaTime * 10f);

        float t = Time.time * player.runSpeed;
        float offsetX = Mathf.Sin(t) * amplitude * movementBlend;
        Vector3 targetPosition = new Vector3(0.017f, -0.255f, 0.278f) + new Vector3(offsetX, 0f, 0f);
        Quaternion targetRotation = Quaternion.Euler(-34f, 114f, -7.6f);

        if (movementCoroutine == null)
        {
            movementCoroutine = StartCoroutine(MoveToPosition(targetPosition, targetRotation));
        }
    }
    #endregion

    #region AIMING
    private void HandleSightIn()
    {
        float inputKey = gameInput.Player.Attack.ReadValue<float>();

        if (inputKey < 0.1f || isReturning || inspectScript.isInspecting) return;

        if (!isAiming)
        {
            sightCoroutine = StartCoroutine(MoveToSightInPosition(sightInPosition.localPosition, sightInPosition.localRotation));
            isAiming = true;
        }
        else
        {
            sightCoroutine = StartCoroutine(MoveToSightInPosition(originalPosition, originalRotation));
            isAiming = false;
        }
    }
    #endregion

    #region RECOIL
    private void HandleRecoilChecks()
    {
        if (recoilHandle.recoiling && !inspectScript.isInspecting && ammo > 0)
        {
            if (isAiming)
                recoilHandle.AimRecoil();
            else
                recoilHandle.HipRecoil();
        }

        if (recoilHandle.recovering)
        {
            recoilHandle.Recovering();
        }
    }

    private void UpdateRecoilTimer()
    {
        if (recoilHandle.nextFire > 0f)
        {
            recoilHandle.nextFire -= Time.deltaTime;
        }
    }

    public void ApplyRecoil()
    {
        recoilHandle.verticalRecoil = recoilHandle.baseVerticalRecoil;
        recoilHandle.horizontalRecoil = recoilHandle.baseHorizontalRecoil;

        foreach (var attachment in equippedAttachments)
        {
            recoilHandle.verticalRecoil += attachment.VerticalRecoilModifier;
            recoilHandle.horizontalRecoil += attachment.HorizontalRecoilModifier;
        }

        if (recoilHandle.verticalRecoilText != null)
            recoilHandle.verticalRecoilText.text = recoilHandle.verticalRecoil.ToString("F2");
        if (recoilHandle.horizontalRecoilText != null)
            recoilHandle.horizontalRecoilText.text = recoilHandle.horizontalRecoil.ToString("F2");
    }
    #endregion

    #region UI
    private void UpdateUI()
    {
        ammoText.text = $"{ammo}/{magSize}";
        magCountText.text = $"Mag Count: {mag}";

        if (recoilHandle.verticalRecoilText != null)
            recoilHandle.verticalRecoilText.text = $"Vertical: {recoilHandle.verticalRecoil}";
        if (recoilHandle.horizontalRecoilText != null)
            recoilHandle.horizontalRecoilText.text = $"Horizontal: {recoilHandle.horizontalRecoil}";
    }
    #endregion

    #region PUBLIC ACCESSORS
    public Vector3 GetAimedPosition() => sightInPosition.localPosition;
    public Quaternion GetAimedRotation() => sightInRotation.localRotation;
    public Vector3 GetCurrentOriginalPosition() => isAiming ? GetAimedPosition() : originalPosition;
    public Quaternion GetCurrentOriginalRotation() => isAiming ? GetAimedRotation() : originalRotation;
    #endregion

    #region COROUTINES
    private IEnumerator MagReloadSequence(Vector3 targetPos, Quaternion targetRot)
    {
        yield return new WaitForSeconds(1f);

        magazineRigidbody.isKinematic = true;

        float elapsedTime = 0f;
        float duration = 1f;
        Vector3 startPosition = magazine.transform.position;
        Quaternion startRotation = magazine.transform.rotation;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            magazine.transform.rotation = Quaternion.Lerp(startRotation, targetRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        magazine.transform.SetParent(transform);
        magazine.transform.localPosition = originalMagPosition;
        magazine.transform.localRotation = originalMagRotation;

        ammo = magSize;
        fireHandle.canFire = true;
        isReloading = false;
    }

    private IEnumerator MoveToPosition(Vector3 targetPos, Quaternion targetRot)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * 2f;
            transform.SetLocalPositionAndRotation(
                Vector3.Lerp(startPos, targetPos, elapsedTime),
                Quaternion.Lerp(startRot, targetRot, elapsedTime));
            yield return null;
        }

        transform.SetLocalPositionAndRotation(targetPos, targetRot);
        movementCoroutine = null;
    }

    private IEnumerator MoveToSightInPosition(Vector3 targetPos, Quaternion targetRot)
    {
        isReturning = true;

        float elapsedTime = 0f;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        while (elapsedTime < sightInThreshold)
        {
            transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsedTime);
            transform.localRotation = Quaternion.Lerp(startRot, targetRot, elapsedTime);
            elapsedTime += Time.deltaTime * sightInSpeed;
            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localRotation = targetRot;
        isReturning = false;
        sightCoroutine = null;
    }
    #endregion
}