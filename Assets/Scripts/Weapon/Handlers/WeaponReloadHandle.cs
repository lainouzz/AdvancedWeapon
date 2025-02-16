using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class WeaponReloadHandle : MonoBehaviour
{
    private Weapon weapon;
    private bool isReloading;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weapon = GetComponent<Weapon>();
    }

    public void HandleReload()
    {
        if (!isReloading && weapon.CanReload())
        {
            Vector3 targetPos = weapon.isAiming ? weapon.GetAimedPosition() : weapon.originalMagPosition;
            Quaternion targetRot = weapon.isAiming ? weapon.GetAimedRotation() : weapon.originalMagRotation;

            Rigidbody rb = weapon.magazine.GetComponent<Rigidbody>();
            MeshCollider mc = weapon.magazine.GetComponent<MeshCollider>();
            rb.isKinematic = false;
            mc.convex = true;

            weapon.magazine.transform.SetParent(null);

            StartCoroutine(Reload(rb, mc, targetPos, targetRot));
        }
    }

    private IEnumerator Reload(Rigidbody rb, MeshCollider mc, Vector3 targetPos, Quaternion targetRot)
    {
        isReloading = true;
        weapon.canFire = false;
        weapon.recoiling = false;

        float elapsedTime = 0;
        float duration = 1f;

        Vector3 startPosition = weapon.magazine.transform.position;
        Quaternion startRotation = weapon.magazine.transform.rotation;

        yield return new WaitForSeconds(1f);

        rb.isKinematic = true;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            weapon.magazine.transform.rotation = Quaternion.Lerp(startRotation, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        weapon.magazine.transform.position = targetPos;
        weapon.magazine.transform.rotation = targetRot;
        weapon.magazine.transform.SetParent(transform);
        weapon.magazine.transform.localPosition = weapon.originalMagPosition;
        weapon.magazine.transform.localRotation = weapon.originalMagRotation;

        weapon.RefillAmmo();
        weapon.canFire = true;
        isReloading = false;
    }
}
