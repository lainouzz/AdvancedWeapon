using System.Collections.Generic;
using UnityEngine;

public class AttachmentHandler : MonoBehaviour
{
    [Header("References")]
    public M4_Weapon weapon;

    [Header("Slot Transforms")]
    public Transform sightTransform;
    public Transform gripTransform;
    public Transform muzzleTransform;
    public Transform sideTransform;

    [Header("Available Attachments")]
    public List<GameObject> availableSights;
    public List<GameObject> availableGrips;
    public List<GameObject> availableMuzzles;
    public List<GameObject> availableSides;

    private Dictionary<string, List<GameObject>> attachmentOptions;
    private Dictionary<string, GameObject> equippedPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, WeaponAttachmentModifier> equippedModifiers = new Dictionary<string, WeaponAttachmentModifier>();
    private Dictionary<string, GameObject> savedPrefabs = new Dictionary<string, GameObject>();

    private static readonly string[] SlotNames = { "Sight", "Grip", "Muzzle", "Side" };

    void Start()
    {
        attachmentOptions = new Dictionary<string, List<GameObject>>
        {
            { "Sight", availableSights },
            { "Grip", availableGrips },
            { "Muzzle", availableMuzzles },
            { "Side", availableSides }
        };
    }

    public void SaveAttachments()
    {
        foreach (string slot in SlotNames)
        {
            equippedPrefabs.TryGetValue(slot, out GameObject prefab);
            savedPrefabs[slot] = prefab;
        }
    }

    public void LoadAttachments()
    {
        foreach (string slot in SlotNames)
        {
            savedPrefabs.TryGetValue(slot, out GameObject prefab);
            EquipAttachment(GetSlotTransform(slot), prefab, slot);
        }
    }

    public void CycleAttachment(string slotName)
    {
        Transform slotTransform = GetSlotTransform(slotName);
        if (slotTransform == null) return;

        if (!attachmentOptions.TryGetValue(slotName, out List<GameObject> options) || options.Count == 0)
            return;

        equippedPrefabs.TryGetValue(slotName, out GameObject currentPrefab);
        int currentIndex = currentPrefab != null ? options.IndexOf(currentPrefab) : -1;
        int nextIndex = (currentIndex + 1) % options.Count;

        EquipAttachment(slotTransform, options[nextIndex], slotName);
    }

    private Transform GetSlotTransform(string slotName)
    {
        return slotName switch
        {
            "Sight" => sightTransform,
            "Grip" => gripTransform,
            "Muzzle" => muzzleTransform,
            "Side" => sideTransform,
            _ => null,
        };
    }

    private void EquipAttachment(Transform slot, GameObject attachmentPrefab, string slotName)
    {
        if (slot.childCount > 0)
        {
            Destroy(slot.GetChild(0).gameObject);
        }

        equippedPrefabs[slotName] = attachmentPrefab;
        equippedModifiers.Remove(slotName);

        if (attachmentPrefab == null)
        {
            SyncModifiersToWeapon();
            return;
        }

        GameObject instance = Instantiate(attachmentPrefab, slot);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localScale = Vector3.one * 10f;
        instance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        AttachmentComponent attachmentData = instance.GetComponent<AttachmentComponent>();
        if (attachmentData == null)
        {
            Debug.LogError($"Attachment {attachmentPrefab.name} is missing an AttachmentComponent!");
            return;
        }

        if (attachmentData.attachmentData == null)
        {
            Debug.LogError($"AttachmentComponent on {attachmentPrefab.name} has no AttachmentData assigned!");
            return;
        }

        equippedModifiers[slotName] = attachmentData.attachmentData;
        SyncModifiersToWeapon();
    }

    private void SyncModifiersToWeapon()
    {
        weapon.equippedAttachments.Clear();
        foreach (var modifier in equippedModifiers.Values)
        {
            weapon.equippedAttachments.Add(modifier);
        }
        weapon.ApplyRecoil();
    }
}
