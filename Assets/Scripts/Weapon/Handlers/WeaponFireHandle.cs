using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class WeaponFireHandle : MonoBehaviour
{
    public Transform muzzleTransform;
    public GameObject vfxMuzzleFlash;
    public GameObject vfx;
    public GameObject vfxBulletHole;

    private Weapon weapon;

    private float nextFire;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weapon = GetComponent<Weapon>();
    }

    public void HandleFire()
    {
        if (Time.time >= nextFire && weapon.canFire)
        {
            nextFire = Time.time + 1 / weapon.weaponData.fireRate;
            weapon.ConsumeAmmo();
            Fire();
        }
    }

    public void Fire()
    {
        if (weapon.canFire)
        {
            //event not fired?
            if (!weapon.eventStackHandler.hasFiredEvent)
            {
                weapon.eventStackHandler.PushEvent("pushed Firing: " + gameObject.name + " Event");
                weapon.eventStackHandler.hasFiredEvent = true;
            }
            weapon.recoilOffset = new Vector3(Random.Range(-weapon.weaponData.baseHorizontalRecoil, weapon.weaponData.baseHorizontalRecoil), 0, 0);
            weapon.recoiling = true;
            weapon.recovering = false;

            //instantiate vfx
            GameObject muzzleFlashInstance = Instantiate(vfxMuzzleFlash, muzzleTransform.position, muzzleTransform.rotation);
            muzzleFlashInstance.transform.SetParent(muzzleTransform);
            ParticleSystem ps = muzzleFlashInstance.GetComponent<ParticleSystem>();

            //can play vfx?
            if (!weapon.inspectScript.isInspecting && weapon.ammo > 0 && weapon.mag >= 0)
            {
                ps.Play();
                Destroy(muzzleFlashInstance, ps.main.duration - 0.8f);
            }
            else
            {
                Destroy(muzzleFlashInstance);
            }

            Ray ray = new Ray(muzzleTransform.transform.position, -muzzleTransform.transform.forward);

            RaycastHit hit;

            if (Physics.Raycast(ray.origin, ray.direction, out hit, 100f))
            {
                Vector3 impactOffset = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                Vector3 impactPosition = hit.point + impactOffset;

                Instantiate(vfx, impactPosition, Quaternion.identity);
                Instantiate(vfxBulletHole, impactPosition, Quaternion.identity);

                TargetBehaviour target = hit.collider.GetComponent<TargetBehaviour>();
                if (target || hit.collider)
                {
                    target.isHit = true;
                    TargetManager.Instance.RotateTarget(target);
                }
            }
            weapon.ApplyRecoil();
        }
    }
}
