using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentHandler : MonoBehaviour
{
    public M4_Weapon weapon;

    public Transform sightTransform;
    public Transform gripTransform;
    public Transform muzzleTransform;
    public Transform sideTransform;

    public List<GameObject> availableSights;
    public List<GameObject> availableGrips;
    public List<GameObject> availableMuzzles;
    public List<GameObject> availableSides;

    private Dictionary<string, List<GameObject>> attachmentOptions;
    private Dictionary<string, WeaponAttachmentModifier> equippedAttachmentData = new Dictionary<string, WeaponAttachmentModifier>();
    private Dictionary<string, GameObject> equippedAttachments = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> savedAttachments = new Dictionary<string, GameObject>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attachmentOptions = new Dictionary<string, List<GameObject>>()
        {
            {"Sight", availableSights },
            {"Grip", availableGrips },
            {"Muzzle", availableMuzzles },
            {"Side", availableSides }
        };
    }

    public void SaveAttachment()
    {
        savedAttachments["Sight"] = sightTransform.childCount > 0 ? sightTransform.GetChild(0).gameObject : null;
        savedAttachments["Grip"] = gripTransform.childCount > 0 ? gripTransform.GetChild(0).gameObject : null;
        savedAttachments["Muzzle"] = muzzleTransform.childCount > 0 ? muzzleTransform.GetChild(0).gameObject : null;
        savedAttachments["Side"] = sideTransform.childCount > 0 ? sideTransform.GetChild(0).gameObject : null;
    }

    public void LoadAttachment()
    {
        EquipAttachment(sightTransform, savedAttachments.ContainsKey("Sight") ? savedAttachments["Sight"] : null, "Sight");
        EquipAttachment(gripTransform, savedAttachments.ContainsKey("Grip") ? savedAttachments["Grip"] : null, "Grip");
        EquipAttachment(muzzleTransform, savedAttachments.ContainsKey("Muzzle") ? savedAttachments["Muzzle"] : null, "Muzzle");
        EquipAttachment(sideTransform, savedAttachments.ContainsKey("Side") ? savedAttachments["Side"] : null, "Side");
    }

    public void CycleAttachment(string slotName)
    {
        Transform slotTransform = GetSlotTransform(slotName);

        if (slotTransform == null || !attachmentOptions.ContainsKey(slotName)) return;

        List<GameObject> options = attachmentOptions[slotName];
        if (options.Count == 0) return;

        GameObject currentAttachment = equippedAttachments.ContainsKey(slotName) ? equippedAttachments[slotName] : null;

        int currentIndex = -1;
        if (currentAttachment != null)
        {
            currentIndex = options.IndexOf(currentAttachment);
        }

        int nextIndex = (currentIndex + 1) % options.Count;

        // Debugging output
        Debug.Log($"Current attachment in {slotName}: {currentAttachment}, Index: {currentIndex}");
        Debug.Log($"Next attachment to equip: {options[nextIndex].name}");
        Debug.Log($"Next index: {nextIndex}");

        EquipAttachment(slotTransform, options[nextIndex], slotName);
        //weapon.ApplyRecoil();
        equippedAttachments[slotName] = options[nextIndex];
    }

    private Transform GetSlotTransform(string slotName)
    {
        return slotName switch
        {
            "Sight" => sightTransform,
            "Muzzle" => muzzleTransform,
            "Grip" => gripTransform,
            "Side" => sideTransform,
            _ => null,
        };
    }

    private void EquipAttachment(Transform slot, GameObject attachmentPrefab, string slotName)
    {
        if(slot.childCount > 0)
        {
            Destroy(slot.GetChild(0).gameObject);
        }

        if (attachmentPrefab == null) return;

        GameObject newAttachment = Instantiate(attachmentPrefab, slot.position, slot.rotation, slot);

        newAttachment.transform.localPosition = Vector3.zero;
        newAttachment.transform.localScale = new Vector3(10, 10, 10);
        newAttachment.transform.localRotation = Quaternion.Euler(0, 180,0);

        AttachmentComponent attachmentDataComponent = newAttachment.GetComponent<AttachmentComponent>();

        if (attachmentDataComponent == null)
        {
            Debug.LogError($"Attachment {attachmentPrefab.name} does NOT have an AttachmentComponent!");
            return;
        }

        if (attachmentDataComponent.attachmentData == null)
        {
            Debug.LogError($"AttachmentComponent on {attachmentPrefab.name} is missing an AttachmentData reference!");
            return;
        }

        equippedAttachmentData[slotName] = attachmentDataComponent.attachmentData;
        weapon.ApplyRecoil();

        equippedAttachments[slotName] = newAttachment;
    }
}
