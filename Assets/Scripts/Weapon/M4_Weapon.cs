using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class M4_Weapon : MonoBehaviour
{
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
    public WeaponSightIn sightIn;
    public InspectScript inspectScript;
    public EventStackHandler eventStackHandler;
    public Player player;
    [Space(3)]

    [Header("Muzzle Transform")]
    public Transform muzzlePosition;
    [Space(3)]

    [Header("Recoil Settings")]
    [Range(0f, 1f)]
    public float recoilPercent = 0.3f;
    [Range(0f, 2f)]
    public float recoverPercent = 0.7f;
    public float baseVerticalRecoil = 1f;
    public float recoilBack = 0f;
    public float baseHorizontalRecoil = 0.1f;
    public float recoilLenght;
    public float verticalRecoil;
    public float horizontalRecoil;

    [Header("Sight in Settings")]
    public Transform sightInPosition;
    public Transform sightInRotation;
    public float sightInSpeed;
    public float sightInThreshold;

    public List<WeaponAttachmentModifier> equipedAttachment = new List<WeaponAttachmentModifier>();
    [Space]

    [Header("Weapon Fire Settings")]
    public float nextFire;
    public float fireRate;

    [Header("Weapon Reload Settings")]
    public int magSize;
    public int ammo;
    public int mag;

    [Header("Weapon animation Settings")]
    [SerializeField] private float amplitude;
    [SerializeField] private float movementBlend;

    //private variables
    private Vector3 originalPosition;
    private Vector3 originalMagPosition;
    private Quaternion originalMagRotation;
    private Quaternion originalRotation;
    private Vector3 recoilVelocity = Vector3.zero;
    public Vector3 recoilOffset;
    private Vector3 finalPosition;

    [Header("Booleans")]
    public bool rayHasHit;
    public bool isAiming;
    public bool isReturning;

    public bool isReloading;
    public bool recovering;
    public float recoverLenght;

    private GameInput gameInput;
    private FireHandle fireHandle;
    private RecoilHandle recoilHandle;
    private void OnEnable()
    {
        gameInput = new GameInput();
        gameInput.Enable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fireHandle = GetComponent<FireHandle>();
        recoilHandle = GetComponent<RecoilHandle>();

        //store position/rotation
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        originalMagPosition = magazine.transform.localPosition;
        originalMagRotation = magazine.transform.localRotation;

        isAiming = false;

        ammo = magSize;
        ammoText.text = ammo + " / " + magSize;
        magCountText.text = "Mag Count: " + mag.ToString();

        recoilLenght = 1 / fireRate * recoilPercent;
        recoverLenght = 1 / fireRate * recoverPercent;
    }

    // Update is called once per frame
    void Update()
    {
        if(recoilHandle.nextFire > 0)
        {
            recoilHandle.nextFire -= Time.deltaTime;
        }

        HandleSightIn();

        float fireButton = gameInput.Player.Fire.ReadValue<float>();
        float reloadButton = gameInput.Player.Reload.ReadValue<float>();

        //fire button pressed and not out of ammo/mag?
        if (fireButton > 0.1 && recoilHandle.nextFire <= 0 && ammo > 0 && !inspectScript.isInspecting)
        {
            fireHandle.canFire = true;
            recoilHandle.nextFire = 1 / recoilHandle.fireRate;

            ammo -= 1;
            ammoText.text = ammo + "/" + magSize;

            fireHandle.Fire();
        }
        else if(fireButton <= 0)//fire button released
        {
            if(eventStackHandler.hasFiredEvent && !eventStackHandler.hasPoppedEvent)
            { 
                eventStackHandler.PopEvent();
                eventStackHandler.hasPoppedEvent = true;
            }
            
            eventStackHandler.ResetEvent();
        }

        // is recoiling and not inspecting?
        if (recoilHandle.recoiling && !inspectScript.isInspecting && ammo > 0)
        {
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

        if(reloadButton > 0.1 && ammo < magSize  && mag >= 0 && !isReloading)
        {        
            Reload();
            
        }

        if(!inspectScript.isInspecting && !isReloading && !recoilHandle.recoiling && !isAiming)
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

    public void ApplyRecoil()
    {
        foreach (var attachment in equipedAttachment)
        {
            recoilHandle.verticalRecoil += attachment.VerticalRecoilModifier;
            recoilHandle.horizontalRecoil += attachment.HorizontalRecoilModifier;
        }
    }

    private void Reload()
    {
        if (mag > 0 && ammo < magSize && !isReloading)
        {     
            isReloading = true;
            fireHandle.canFire = false;
            recoilHandle.recoiling = false;

            mag--;
            
            Vector3 targetPos = isAiming ? GetAimedPosition() : originalMagPosition;
            Quaternion targetRot = isAiming ? GetAimedRotation() : originalMagRotation;

            Rigidbody rb = magazine.GetComponent<Rigidbody>();
            MeshCollider mc = magazine.GetComponent<MeshCollider>();
            rb.isKinematic = false;
            mc.convex = true;
            
            magazine.transform.SetParent(null);

            StartCoroutine(MagReloadMagic(rb, mc, targetPos, targetRot));
        }
    }

    private void HandleSightIn()
    {
        float inputKey = gameInput.Player.Attack.ReadValue<float>();

        if (inputKey >= 0.1f && !isAiming && !isReturning && !inspectScript.isInspecting)
        {
            StartCoroutine(MoveToSightInPosition(sightInPosition.localPosition, sightInPosition.localRotation));
            isAiming = true;
        }

        if (inputKey >= 0.1f && isAiming && !isReturning)
        {
            StartCoroutine(MoveToSightInPosition(originalPosition, originalRotation));
            isAiming = false;
        }
    }

    public Vector3 GetAimedPosition()
    {
        return sightInPosition.localPosition;
    }
    public Quaternion GetAimedRotation()
    {
        return sightInRotation.localRotation;
    }
    public Vector3 GetCurrentOirignalPosition()
    {
        return isAiming ? GetAimedPosition() : originalPosition;
    }
    public Quaternion GetCurrentOirignalRotation()
    {
        return isAiming ? GetAimedRotation() : originalRotation;
    }
    private IEnumerator MagReloadMagic(Rigidbody rb, MeshCollider mc, Vector3 targetPos, Quaternion targetRot)
    {
        float elapsedTime = 0;
        float duration = 1f;

        Vector3 startPosition = magazine.transform.position;
        Quaternion startRotation = magazine.transform.rotation;

        yield return new WaitForSeconds(1f);

        rb.isKinematic = true;

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
}
