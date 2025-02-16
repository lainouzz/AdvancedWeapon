using UnityEngine;

public class Player : MonoBehaviour
{
    public InspectScript inspectScript;

    public Transform camera;
    public Transform playerBody;
    private GameInput gameInput;

    [Range(1f, 100f)]
    public float mouseSensX;
    [Range(1f, 100f)]
    public float mouseSensY;

    [Range(0.1f, 5f)]
    public float walkSpeed;
    [Range(5f, 10f)]
    public float runSpeed;

    public bool isWalking;
    public bool isRunning;
    public bool isIdle;

    float runInput;

    public Vector2 moveInput;

    private float verticalRot;
    private float mouseX;
    private float mouseY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameInput = new GameInput();
        gameInput.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 lookInput = gameInput.Player.Look.ReadValue<Vector2>();

        if (!inspectScript.isInspecting)
        {
            HandleLookX(lookInput.x);
            HandleLookY(lookInput.y);
            HandleMoveCheck();
        }
    }

    private void HandleLookX(float lookX)
    {
        mouseX = lookX * mouseSensX * Time.deltaTime;
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleLookY(float lookY)
    {
        mouseY = lookY * mouseSensY * Time.deltaTime;
        verticalRot -= mouseY;
        verticalRot = Mathf.Clamp(verticalRot, -85f, 85f);

        camera.localRotation = Quaternion.Euler(verticalRot, 0, 0);
    }

    public void HandleMoveCheck()
    {
        moveInput = gameInput.Player.Move.ReadValue<Vector2>();
        runInput = gameInput.Player.Sprint.ReadValue<float>();
        if (moveInput.magnitude < 0.1f && runInput < 0.1f)
        {
            isIdle = true;
            isWalking = false;
            isRunning = false;
        }
        else if (moveInput.y > 0.1f && runInput < 0.1f)
        {
            isWalking = true;
            isRunning = false;
        }else if(moveInput.y > 0.1f && runInput > 0.1f)
        {
            isWalking = false;
            isRunning = true;
        }
    }
}
