using UnityEngine;
using TMPro;

public class RecoilHandle : MonoBehaviour
{
    [Header("References")]
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
    public float recoilBack;
    public float baseHorizontalRecoil = 0.1f;
    public float verticalRecoil;
    public float horizontalRecoil;

    [Header("Weapon Fire Settings")]
    public float nextFire;
    public float fireRate;

    [Header("State")]
    public bool recoiling;
    public bool recovering;

    public Vector3 recoilOffset;

    private float recoilLength;
    private float recoverLength;
    private Vector3 recoilVelocity = Vector3.zero;
    private Vector3 hipRecoilTarget;
    private Vector3 originalPosition;

    private const float RecoilThreshold = 0.01f;

    void Start()
    {
        originalPosition = transform.localPosition;

        verticalRecoil = baseVerticalRecoil;
        horizontalRecoil = baseHorizontalRecoil;

        recoilLength = 1f / fireRate * recoilPercent;
        recoverLength = 1f / fireRate * recoverPercent;
    }

    public void HipRecoil()
    {
        hipRecoilTarget = originalPosition + new Vector3(0f, baseVerticalRecoil, -recoilBack);
        ApplyRecoilMovement(hipRecoilTarget, recoilLength);
    }

    public void AimRecoil()
    {
        Vector3 aimedPosition = weapon.GetAimedPosition();
        Vector3 target = aimedPosition + recoilOffset + new Vector3(0f, baseVerticalRecoil, -recoilBack);
        ApplyRecoilMovement(target, recoilLength);
    }

    public void Recovering()
    {
        Vector3 target = weapon.GetCurrentOriginalPosition();
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target, ref recoilVelocity, recoverLength);

        if (Vector3.Distance(transform.localPosition, target) < RecoilThreshold)
        {
            recoiling = false;
            recovering = false;
        }
    }

    private void ApplyRecoilMovement(Vector3 target, float smoothTime)
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target, ref recoilVelocity, smoothTime);

        if (Vector3.Distance(transform.localPosition, target) < RecoilThreshold)
        {
            recoiling = false;
            recovering = true;
        }
    }
}
