using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
    public bool isHit;

    private Quaternion originalRotation;

    private void Start()
    {
        TargetManager.Instance.targets.Add(this);
        originalRotation = transform.rotation;
    }

    public void Update()
    {
        if (isHit)
        {
            transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            StartCoroutine(ResetTargets());
        }
    }

    public void RotateTarget()
    {
        isHit = true;
    }

    IEnumerator ResetTargets()
    {
        yield return new WaitForSeconds(1f);

        float elapsedTime = 0f;
        float duration = 1;

        Quaternion startRotation = transform.rotation;

        while (elapsedTime < duration)
        {
            float f = elapsedTime / duration;
            transform.rotation = Quaternion.Lerp(startRotation, originalRotation, f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
        isHit = false;
    }
}
