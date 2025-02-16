using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    public List<TargetBehaviour> targets = new List<TargetBehaviour>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }

    public void RotateTarget(TargetBehaviour target)
    {
        if (targets.Contains(target))
        {
            target.RotateTarget();
        }
    }
}
