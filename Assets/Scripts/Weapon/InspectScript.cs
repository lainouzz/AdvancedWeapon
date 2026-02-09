using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectScript : MonoBehaviour
{
    private GameInput gameInput;

    public CameraShake cameraShake;
    [SerializeField] private EventStackHandler eventStackHandler;

    public AttachmentHandler attachmentHandler;
    public Transform inspectPosition;

    public float speed;
    public float threshold;

    public bool isInspecting;
    public bool isReturning;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        gameInput = new GameInput();
        gameInput.Enable();
    }

    private void Start()
    {
        //store position/rotation
        originalRotation = transform.localRotation;
        originalPosition = transform.localPosition;

        isInspecting = false;
    }

    // Update is called once per frame
    void Update()
    {
        InspectWeapon();
    }

    void InspectWeapon()
    {
        float inputKey = gameInput.Player.Interact.ReadValue<float>();

        //move from original pos to inspect pos, push event
        if (inputKey >= 0.1f && !isInspecting && !isReturning)
        {
            StartCoroutine(MoveToPosition(inspectPosition.localPosition, inspectPosition.localRotation));
            isInspecting = true;
            cameraShake.enabled = false; 
            eventStackHandler.PushEvent("Pushed Inspecting event");
            Cursor.lockState = CursorLockMode.None;
        }
        //return from inspect pos to original pos, save attachment and pop event
        if (inputKey >= 0.1f && isInspecting && !isReturning)
        {
            StartCoroutine(MoveToPosition(originalPosition, originalRotation));
            isInspecting = false;
            attachmentHandler.SaveAttachments();
            eventStackHandler.PopEvent();
            Cursor.lockState = CursorLockMode.Locked;
            
        }
        if (transform.localPosition == originalPosition)
        {
            cameraShake.enabled = true;
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPos, Quaternion targetRot)
    {
        isReturning = true;

        float elapsedTime = 0f;
        Vector3 originalPosition = transform.localPosition;
        Quaternion originalRotation = transform.localRotation;

        while (elapsedTime < 1f)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, targetPos, elapsedTime);
            transform.localRotation = Quaternion.Lerp(originalRotation, targetRot, elapsedTime);

            elapsedTime += Time.deltaTime * speed;

            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localRotation = targetRot;
        isReturning = false;      
    }
}
