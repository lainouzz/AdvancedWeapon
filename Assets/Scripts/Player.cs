using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("References")]
    public InspectScript inspectScript;
    public Transform camera;
    public Transform playerBody;

    [Header("Mouse Settings")]
    [Range(1f, 100f)]
    public float mouseSensX;
    [Range(1f, 100f)]
    public float mouseSensY;

    [Header("Movement Settings")]
    [Range(0.1f, 5f)]
    public float walkSpeed;
    [Range(5f, 10f)]
    public float runSpeed;

    [Header("State")]
    public bool isWalking;
    public bool isRunning;
    public bool isIdle;

    [HideInInspector]
    public Vector2 moveInput;

    private GameInput gameInput;
    private float verticalRot;

    void Start()
    {
        gameInput = new GameInput();
        gameInput.Enable();
    }

    void Update()
    {
        if (inspectScript.isInspecting) return;

        Vector2 lookInput = gameInput.Player.Look.ReadValue<Vector2>();
        HandleLookX(lookInput.x);
        HandleLookY(lookInput.y);
        HandleMoveCheck();
    }

    private void HandleLookX(float lookX)
    {
        float deltaX = lookX * mouseSensX * Time.deltaTime;
        playerBody.Rotate(Vector3.up * deltaX);
    }

    private void HandleLookY(float lookY)
    {
        float deltaY = lookY * mouseSensY * Time.deltaTime;
        verticalRot -= deltaY;
        verticalRot = Mathf.Clamp(verticalRot, -85f, 85f);
        camera.localRotation = Quaternion.Euler(verticalRot, 0f, 0f);
    }

    public void HandleMoveCheck()
    {
        moveInput = gameInput.Player.Move.ReadValue<Vector2>();
        float runInput = gameInput.Player.Sprint.ReadValue<float>();

        if (moveInput.magnitude < 0.1f)
        {
            isIdle = true;
            isWalking = false;
            isRunning = false;
        }
        else if (moveInput.y > 0.1f && runInput > 0.1f)
        {
            isIdle = false;
            isWalking = false;
            isRunning = true;
        }
        else if (moveInput.y > 0.1f)
        {
            isIdle = false;
            isWalking = true;
            isRunning = false;
        }
    }
}
