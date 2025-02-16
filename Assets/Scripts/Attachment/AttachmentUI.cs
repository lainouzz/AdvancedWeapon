using Mono.Cecil.Cil;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttachmentUI : MonoBehaviour
{
    public InspectScript inspectScript;

    public Transform gripSlot;
    public Transform sightSlot;
    public Transform sideSlot;
    public Transform muzzleSlot;

    private GameObject gripObject;
    private GameObject sightObject;
    private GameObject sideObject;
    private GameObject muzzleObject;

    private void Start()
    {
        if (gripSlot) gripObject = gripSlot.gameObject;
        if (sightSlot) sightObject = sightSlot.gameObject;
        if (sideSlot) sideObject = sideSlot.gameObject;
        if (muzzleSlot) muzzleObject = muzzleSlot.gameObject;
    }

    public void Update()
    {
        if (inspectScript.isInspecting)
        {
            ToggleSlots(true);
        }
        else
        {
            ToggleSlots(false);
        }
    }

    public void ToggleSlots(bool state)
    {
        if (gripObject) gripObject.SetActive(state);
        if (sightObject) sightObject.SetActive(state);
        if (sideObject) sideObject.SetActive(state);
        if (muzzleObject) muzzleObject.SetActive(state);
    }
}
