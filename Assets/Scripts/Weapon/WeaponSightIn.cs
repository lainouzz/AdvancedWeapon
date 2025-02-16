using System.Collections;
using UnityEngine;

public class WeaponSightIn : MonoBehaviour
{
    public InspectScript inspectScript;

    private GameInput gameInput;

    public Transform sightInPosition;
    public Transform sightInRotation;

    public float speed;
    public float threshold;

    public bool isAiming;
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
        originalRotation = transform.localRotation;
        originalPosition = transform.localPosition;

        isAiming = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleSightIn();
    }

    void HandleSightIn()
    {
        float inputKey = gameInput.Player.Attack.ReadValue<float>();

        if (inputKey >= 0.1f && !isAiming && !isReturning && !inspectScript.isInspecting)
        {
            StartCoroutine(MoveToPosition(sightInPosition.localPosition, sightInPosition.localRotation));
            isAiming = true;
        }

        if (inputKey >= 0.1f && isAiming && !isReturning)
        {
            StartCoroutine(MoveToPosition(originalPosition, originalRotation));
            isAiming = false;
        }
    }

    public Vector3 GetAimedPosition()
    {
        return sightInPosition.localPosition;
    }
    public Quaternion GetAimedRotation()
    {
        return sightInRotation.localRotation;
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
