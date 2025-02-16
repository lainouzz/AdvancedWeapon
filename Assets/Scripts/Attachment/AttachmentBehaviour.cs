using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Attachment", menuName = "Attachment/AttachmentType")]
public class AttachmentBehaviour : ScriptableObject
{
    public string attachmentType;
    public string attachmentName;

    public GameObject attachmentPrefab;

    public void ChangeRecoilValue(float amount)
    {
        // if grip1 is attach, make recoil have more side recoil, and less vertical for example
    }
}
