using UnityEngine;

public class AttachmentSlot : MonoBehaviour
{
    public InspectScript inspectScript;
    public string slotName;
    private AttachmentHandler attachmentHandler;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attachmentHandler = FindAnyObjectByType<AttachmentHandler>();
    }

    private void OnMouseDown()
    {
        if(inspectScript != null && inspectScript.isInspecting)
        {
            Debug.Log($"Clicked on slot: {slotName}");
            
            if (attachmentHandler != null)
            {
                attachmentHandler.CycleAttachment(slotName);
            }
        }
        
    }
}
