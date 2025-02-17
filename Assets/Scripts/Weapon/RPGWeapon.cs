using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RPGWeapon : MonoBehaviour
{
    public M4_Weapon weapon;
    public EventStackHandler eventStackHandler;
    public GameObject missilePrefab;
    public GameObject currentMissilePrefab;

    [Header("General Components")]
    public TMP_Text ammoText;

    [Header("Recoil Settings")]
    [Range(0f, 1f)]
    public float recoilPercent = 0.3f;
    [Range(0f, 2f)]
    public float recoverPercent = 0.7f;
    public float baseVerticalRecoil = 1f;
    public float recoilBack = 0f;
    public float recoilLenght;
    public float verticalRecoil;
    private float recoverLenght;

    [Header("Weapon Fire Settings")]
    public float nextFire;
    public float fireRate;

    [Header("Weapon Ammo Settings")]
    public int currentAmmo;
    public int maxAmmo;

    [Header("Transforms")]
    public Transform muzzlePosition;

    [Header("Booleans(for testing)")]
    public bool recoiling;
    public bool canFire;
    public bool hasFired;
    public bool isAiming;
    public bool isReturning;
    public bool isReloading;
    private bool recovering;

    private Vector3 recoilVelocity = Vector3.zero;
    private Vector3 recoilOffset;


    private Vector3 originalMissilePos;
    private Vector3 originalPosition;
    private Quaternion originalMissileRot;
    private Quaternion originalRotation;

    private GameInput gameInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameInput = InputManager.inputInstance.gameInput;

        //store position/rotation
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalMissilePos = missilePrefab.transform.localPosition;
        originalMissileRot = missilePrefab.transform.localRotation;

        SpawnMissile();

        currentAmmo = maxAmmo;

        recoilLenght = 1 / fireRate * recoilPercent;
        recoverLenght = 1 / fireRate * recoverPercent;
    }

    // Update is called once per frame
    void Update()
    {     
        if(gameObject.activeSelf)
        {

            if (nextFire > 0)
            {
                nextFire -= Time.deltaTime;
            }

            float fireButton = gameInput.Player.Fire.ReadValue<float>();
            float reloadButton = gameInput.Player.Reload.ReadValue<float>();

            //fire button pressed
            if (fireButton > 0.1 && nextFire <= 0 && currentAmmo > 0)
            {
                canFire = true;
                hasFired = true;
                nextFire = 1 / fireRate;

                currentAmmo -= 1;
                //ammoText.text = currentAmmo + "/" + maxAmmo;

                HandleFire();
            }
            else if (fireButton <= 0)//fire button released
            {
                
                if (eventStackHandler.hasFiredEvent && !eventStackHandler.hasPoppedEvent)
                {
                    eventStackHandler.PopEvent();
                    eventStackHandler.hasPoppedEvent = true;
                }

                eventStackHandler.ResetEvent();
            }

            if (reloadButton > 0.1 && currentAmmo < maxAmmo && maxAmmo >= 0 && !isReloading)
            {
                HandleReload();

            }
        }
    }

    private void HandleFire()
    {
        if (canFire)
        {
            //event not fired?
            if (!eventStackHandler.hasFiredEvent)
            {
                eventStackHandler.PushEvent("pushed Firing RPG: " + gameObject.name + " Event");
                eventStackHandler.hasFiredEvent = true;
            }
            recoiling = true;
            recovering = false;

        }
    }

    private void SpawnMissile()
    {
        if(missilePrefab != null)
        {
            currentMissilePrefab = Instantiate(missilePrefab, transform);
            currentMissilePrefab.transform.localPosition = originalMissilePos;
            currentMissilePrefab.transform.localRotation = originalMissileRot;
        }
    }

    private void HandleReload()
    {
        if (currentAmmo > 0 && currentAmmo < maxAmmo && !isReloading)
        {
            isReloading = true;
            canFire = false;
            recoiling = false;

            Vector3 targetPos = originalMissilePos;
            Quaternion targetRot = originalMissileRot;

            StartCoroutine(MagReloadMagic(targetPos, targetRot));
        }
    }

    private void HandleRecoil()
    {

    }

    private void Recovering()
    {

    }

    private void HandleSightIn()
    {

    }

    private IEnumerator MagReloadMagic(Vector3 targetPos, Quaternion targetRot)
    {
        yield return new WaitForSeconds(1f);

        if(missilePrefab != null)
        {
            currentMissilePrefab = Instantiate (missilePrefab, transform);
            currentMissilePrefab.transform.localPosition = originalMissilePos;
            currentMissilePrefab.transform.localRotation = originalMissileRot;
        }

        canFire = true;
        isReloading = false;
        hasFired = false;
        //ammoText.text = ammo + "/" + magSize; 
    }
}
