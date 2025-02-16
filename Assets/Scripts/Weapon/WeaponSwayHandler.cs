using UnityEngine;

public class WeaponSwayHandler : MonoBehaviour
{
    public GameInput gameInput;

    public InspectScript inspect;

    [SerializeField] private float smoothSwayAmount;
    [SerializeField] private float swayMultiplier;

    private void Start()
    {
        gameInput = new GameInput();
        gameInput.Enable();
        gameInput.Player.Look.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if(inspect != null && !inspect.isInspecting)
        {
            HandleWeaponSway();
        }
    }

    void HandleWeaponSway()
    {
        Vector2 mouse = gameInput.Player.Look.ReadValue<Vector2>() * swayMultiplier;

        Quaternion rotationX = Quaternion.AngleAxis(-mouse.y, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouse.x, Vector3.up);
        Quaternion targetRot = rotationX * rotationY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot * Quaternion.Euler(0, 180, 0), smoothSwayAmount * Time.deltaTime);
    }
}
