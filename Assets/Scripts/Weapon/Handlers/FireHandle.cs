using UnityEngine;

public class FireHandle : MonoBehaviour
{
    public M4_Weapon weapon;
    public RecoilHandle recoilHandle;
    public EventStackHandler stackHandler;

    public GameObject vfxMuzzleFlash;
    public GameObject vfx;
    public GameObject vfxBulletHole;

    public bool canFire;

    public void Fire()
    {
        if (canFire)
        {
            if (!stackHandler.hasFiredEvent)
            {
                stackHandler.PushEvent("pushed Firing: " + gameObject.name + " Event");
                stackHandler.hasFiredEvent = true;
            }

            recoilHandle.recoilOffset = new Vector3(Random.Range(-recoilHandle.baseHorizontalRecoil, recoilHandle.baseHorizontalRecoil), 0, 0);
            recoilHandle.recoiling = true;
            recoilHandle.recovering = false;

            GameObject muzzleFlashInstance = Instantiate(vfxMuzzleFlash, weapon.muzzlePosition.position, weapon.muzzlePosition.rotation);
            muzzleFlashInstance.transform.SetParent(weapon.muzzlePosition);
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

            Ray ray = new Ray(weapon.muzzlePosition.transform.position, -weapon.muzzlePosition.transform.forward);

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
        }
    }
}
