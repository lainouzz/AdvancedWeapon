using JetBrains.Annotations;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Weapon weapon;
    public InspectScript inspectScript;

    public float shakeAmount;

    private Vector3 originalPosition;

    private Camera camera;

    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!inspectScript.isInspecting)
        {
            if (weapon.recoiling && weapon.ammo > 0)
            {
                HandleShake();
            }
            else
            {
                transform.localPosition = originalPosition;
            }
        }
        else if (!weapon.recoiling)
        {
            transform.localPosition = originalPosition;
        }
    }

    public void HandleShake()
    {
        //if (!inspectScript.isInspecting)
        //{
            
        //}
        float randomX = Random.Range(-shakeAmount, shakeAmount);
        float randomY = Random.Range(-shakeAmount, shakeAmount);
        float randomZ = Random.Range(-0.1f, 0.1f);

        Vector3 finalPosition = new Vector3(originalPosition.x + randomX, originalPosition.y + randomY, originalPosition.z + randomZ);
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, finalPosition, 1f);
    }
}
