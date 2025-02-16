using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class Weapon : MonoBehaviour
{
    [Header("General Component")]
    public Camera camera;
    public TMP_Text ammoText;
    public TMP_Text magCountText;
    public TMP_Text verticalText;
    public TMP_Text horizontalText;
    public Transform weaponRunPosition;
    public WeaponData weaponData;
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
    public Vector3 originalMagPosition;
    public Quaternion originalMagRotation;
    private Quaternion originalRotation;
    public Vector3 recoilVelocity = Vector3.zero;
    public Vector3 recoilOffset;
    public Vector3 finalPosition;

    [Header("Booleans")]
    public bool recoiling;
    public bool rayHasHit;
    public bool canFire;
    public bool isAiming;
    public bool isReturning;

    private bool isReloading;
    public bool recovering;
    private float recoverLenght;

    private GameInput gameInput;
    private WeaponFireHandle weaponFireHandle;
    private WeaponReloadHandle weaponReloadHandle;
    private WeaponAnimationHandler weaponAnimationHandler;
    private void OnEnable()
    {
        gameInput = new GameInput(); //use inputManagers gameInput
        gameInput.Enable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //store position/rotation
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        originalMagPosition = magazine.transform.localPosition;
        originalMagRotation = magazine.transform.localRotation;

        weaponData.verticalRecoil = weaponData.baseVerticalRecoil;
        weaponData.horizontalRecoil = weaponData.baseHorizontalRecoil;

        isAiming = false;

        weaponFireHandle = GetComponent<WeaponFireHandle>();
        weaponReloadHandle = GetComponent<WeaponReloadHandle>();
        weaponAnimationHandler = GetComponent<WeaponAnimationHandler>();

        ammo = magSize;
        ammoText.text = ammo + " / " + magSize;
        magCountText.text = "Mag Count: " + mag.ToString();

        recoilLenght = 1 / fireRate * recoilPercent;
        recoverLenght = 1 / fireRate * recoverPercent;
    }

    // Update is called once per frame
    void Update()
    {
        float inputKey = gameInput.Player.Attack.ReadValue<float>();
        float fireButton = gameInput.Player.Fire.ReadValue<float>();
        float reloadButton = gameInput.Player.Reload.ReadValue<float>();

        if (nextFire > 0)
        {
            nextFire -= Time.deltaTime;
        }

        if(inputKey >= 0.1f && !isAiming && !isReturning && !inspectScript.isInspecting)
        {
            weaponAnimationHandler.HandleSightIn(sightInPosition.localPosition, sightInPosition.localRotation);
            isAiming = true;
        }

        if(inputKey >= 0.1f && isAiming && !isReturning)
        {
            weaponAnimationHandler.HandleSightIn(originalPosition, originalRotation);
            isAiming = false;
        }
        //HandleSightIn();


        //fire button pressed and not out of ammo/mag?
        if (fireButton > 0.1 && nextFire <= 0 && ammo > 0 && !inspectScript.isInspecting)
        {
            canFire = true;
            nextFire = 1 / fireRate;

            ammo -= 1;
            ammoText.text = ammo + "/" + magSize;

            weaponFireHandle.HandleFire();

            //Fire(); old function
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
        if (recoiling && !inspectScript.isInspecting && ammo > 0)
        {
            if (isAiming)
            {
                weaponAnimationHandler.HandleAimRecoil();
               // AimRecoil();
            }
            else
            {
                weaponAnimationHandler.HandleHipFire();
                //HipRecoil();
            }
        }

        //has recoiled?
        if (recovering)
        {
            Recovering();
        }

        if(reloadButton > 0.1 && ammo < magSize  && mag >= 0 && !isReloading)
        {    
            weaponReloadHandle.HandleReload();
           // Reload();
            
        }

        if(!inspectScript.isInspecting && !isReloading && !recoiling && !isAiming)
        {
            HandleWalkAnimation();
            HandleRunAnimation();
        }     
    }

    public void ConsumeAmmo()
    {
        ammo--;
        UpdateUI();
    }

    public bool CanReload()
    {
        return ammo < weaponData.magSize && mag > 0;
    }

    public void RefillAmmo()
    {
        ammo = weaponData.magSize;
        mag--;
        UpdateUI();
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
    private void HipRecoil()
    {
        finalPosition = new Vector3(originalPosition.x, originalPosition.y + baseVerticalRecoil, originalPosition.z - recoilBack);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoilLenght);

        if(Vector3.Distance(transform.localPosition, finalPosition) < 0.01f)
        {
            recoiling = false;
            recovering = true;
        }
    }

    private void AimRecoil()
    {      
        Vector3 aimedPosition = GetAimedPosition();
        Vector3 finalPosition = aimedPosition + recoilOffset + new Vector3(0, baseVerticalRecoil, -recoilBack);

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoilLenght);

        if (Vector3.Distance(transform.localPosition, finalPosition) < 0.01f)
        {
            recoiling = false;
            recovering = true;
        }
    }

    public void ApplyRecoil()
    {
        foreach (var attachment in equipedAttachment)
        {
            weaponData.verticalRecoil += attachment.VerticalRecoilModifier;
            weaponData.horizontalRecoil += attachment.HorizontalRecoilModifier;
        }
    }

    private void Recovering()
    {
        Vector3 finalPosition = GetCurrentOirignalPosition();

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoverLenght);

        if (Vector3.Distance(transform.localPosition, finalPosition) < 0.01f)
        {
            recoiling = false;
            recovering = false;
        }
    }

    public void UpdateUI()
    {
        ammoText.text = $"{ammo} / {weaponData.magSize}";
        magCountText.text = $"Mag: {mag}";
        verticalText.text = $"Vertical: {weaponData.verticalRecoil}";
        horizontalText.text = $"Horizontal: {weaponData.horizontalRecoil}";
    }
    
    public Vector3 GetAimedPosition()
    {
        return sightInPosition.localPosition;
    }
    public Quaternion GetAimedRotation()
    {
        return sightInRotation.localRotation;
    }
    private Vector3 GetCurrentOirignalPosition()
    {
        return isAiming ? GetAimedPosition() : originalPosition;
    }
    private Quaternion GetCurrentOirignalRotation()
    {
        return isAiming ? GetAimedRotation() : originalRotation;
    }


    private IEnumerator MoveToPosition(Vector3 targetPos, Quaternion targetRot) // player run animation
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
}