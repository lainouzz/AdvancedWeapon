using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    public int magSize;
    public int maxAmmo;

    public float fireRate;

    public float sightInSpeed;
    public float sightInThreshold;

    public float baseVerticalRecoil;
    public float baseHorizontalRecoil;

    public float verticalRecoil;
    public float horizontalRecoil;

    public float recoilBack = 0f;
    public float recoilLenght;


    public Vector3 recoilVelocity = Vector3.zero;

}
