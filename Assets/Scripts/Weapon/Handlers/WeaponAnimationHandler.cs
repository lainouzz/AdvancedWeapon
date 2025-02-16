using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Net.WebSockets;
using TMPro;

public class WeaponAnimationHandler : MonoBehaviour
{
    Weapon weapon;
    GameInput gameInput;

    private Vector3 originalPosition;
    private Vector3 recoilOffset;
    private Vector3 finalPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameInput = new GameInput();
        weapon = GetComponent<Weapon>();
        originalPosition = transform.localPosition;
    }

    public void HandleHipFire()
    {
        finalPosition = new Vector3(originalPosition.x, originalPosition.y + weapon.weaponData.baseVerticalRecoil, originalPosition.z - weapon.weaponData.recoilBack);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref weapon.weaponData.recoilVelocity, weapon.recoilLenght);

        if (Vector3.Distance(transform.localPosition, finalPosition) < 0.01f)
        {
            weapon.recoiling = false;
            weapon.recovering = true;
        }
    }

    public void HandleAimRecoil()
    {
        Vector3 aimedPosition = weapon.GetAimedPosition();
        finalPosition = aimedPosition + weapon.recoilOffset + new Vector3(0, weapon.weaponData.baseVerticalRecoil, -weapon.weaponData.recoilBack);

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref weapon.recoilVelocity, weapon.recoilLenght);

        if (Vector3.Distance(transform.localPosition, finalPosition) < 0.01f)
        {
            weapon.recoiling = false;
            weapon.recovering = true;
        }
    }

    public void HandleSightIn(Vector3 targetPos, Quaternion targetRot)
    {
        StartCoroutine(MoveToPosition(targetPos, targetRot));
    }

    private IEnumerator MoveToPosition(Vector3 targetPos, Quaternion targetRot)
    {
        weapon.isReturning = true;

        float elapsedTime = 0f;
        Vector3 originalPosition = transform.localPosition;
        Quaternion originalRotation = transform.localRotation;

        while (elapsedTime < weapon.weaponData.sightInThreshold)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, targetPos, elapsedTime);
            transform.localRotation = Quaternion.Lerp(originalRotation, targetRot, elapsedTime);

            elapsedTime += Time.deltaTime * weapon.weaponData.sightInSpeed;

            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localRotation = targetRot;
        weapon.isReturning = false;
    }
}
