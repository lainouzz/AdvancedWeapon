using UnityEngine;

public class RPGMissile : MonoBehaviour
{
    RPGWeapon rpgWeapon;

    public GameObject VFX;

    public float speed;
    public float rotationSpeed;

    private float originalSpeed;
    private float originalRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalSpeed = speed;
        originalRotation = rotationSpeed;

        rpgWeapon = FindAnyObjectByType<RPGWeapon>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rpgWeapon != null && rpgWeapon.hasFired)
        {
            HandleMissileMovement();
        }

        if (rpgWeapon.isReloading)
        {
            speed = 0;
            rotationSpeed = 0;
        }
        else
        {
            speed = originalSpeed;
            rotationSpeed = originalRotation;
        }
    }

    private void HandleMissileMovement()
    {
      
        //TODO: ADD PHYSICS
        transform.SetParent(null);
        transform.position -= transform.forward * speed * Time.deltaTime;
        transform.Rotate(0, 0, rotationSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = Instantiate(VFX, transform.position, Quaternion.identity);

        Debug.LogWarning("RPG HIT SOMETHING");
        Destroy(go, 2.5f);
        Destroy(gameObject);
    }
}
