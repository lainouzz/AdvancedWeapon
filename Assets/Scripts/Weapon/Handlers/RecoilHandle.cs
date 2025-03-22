using UnityEngine;
using TMPro;

public class RecoilHandle : MonoBehaviour
{
    public M4_Weapon weapon;
    public EventStackHandler stackHandler;
    public TMP_Text verticalRecoilText;
    public TMP_Text horizontalRecoilText;


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
    public float recoverLenght;

    [Header("Weapon Fire Settings")]
    public float nextFire;
    public float fireRate;

    public bool recoiling;
    public bool recovering;

    private Vector3 recoilVelocity = Vector3.zero;
    public Vector3 recoilOffset;
    private Vector3 finalPosition;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        verticalRecoil = baseVerticalRecoil;
        horizontalRecoil = baseHorizontalRecoil;

        recoilLenght = 1 / fireRate * recoilPercent;
        recoverLenght = 1 / fireRate * recoverPercent;
    }

    public void HipRecoil()
    {
        finalPosition = new Vector3(originalPosition.x, originalPosition.y + baseVerticalRecoil, originalPosition.z - recoilBack);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoilLenght);

        if (Vector3.Distance(transform.localPosition, finalPosition) < 0.01f)
        {
            recoiling = false;
            recovering = true;
        }
    }

    public void AimRecoil()
    {
        Vector3 aimedPosition = weapon.GetAimedPosition();
        Vector3 finalPosition = aimedPosition + recoilOffset + new Vector3(0, baseVerticalRecoil, -recoilBack);

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoilLenght);

        if (Vector3.Distance(transform.localPosition, finalPosition) < 0.01f)
        {
            recoiling = false;
            recovering = true;
        }
    }



    public void Recovering()
    {
        Vector3 finalPosition = weapon.GetCurrentOriginalPosition();

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoverLenght);

        if (Vector3.Distance(transform.localPosition, finalPosition) < 0.01f)
        {
            recoiling = false;
            recovering = false;
        }
    }
}
