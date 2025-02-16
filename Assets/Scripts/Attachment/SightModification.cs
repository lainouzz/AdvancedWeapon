using System.Collections;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.Rendering;

public class SightModification : MonoBehaviour
{
    private GameInput gameInput;

    public  bool hasPressed;

    private Transform findPart;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameInput = new GameInput();
        gameInput.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        ModifySight();
    }

    void ModifySight()
    {
        float inputKey = gameInput.Player.AttachmentModification.ReadValue<float>();
        if (inputKey >= 0.1f && !hasPressed)
        {
            StartCoroutine(SightNumerator(true));
        }
        else if (inputKey >= 0.1f && hasPressed)
        {
            StartCoroutine(SightNumerator(false));
        }
    }

    IEnumerator SightNumerator(bool isModified)
    {
        float elapsedTime = 0;
        float duration = 1;
        findPart = transform.Find("Optic_L_Part");

        Quaternion startRot = findPart.rotation;
        Quaternion targetRot = isModified ? Quaternion.Euler(0, 180, -90) : Quaternion.Euler(0, 180, 0);

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            findPart.rotation = Quaternion.Slerp(startRot, targetRot, elapsedTime / duration);

            yield return null;
        }

        findPart.rotation = targetRot;
        hasPressed = isModified;
    }
}
