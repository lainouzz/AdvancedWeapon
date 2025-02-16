using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EventStackHandler))]

public class EventStackHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EventStackHandler eventStackHandler = (EventStackHandler)target;

        EditorGUILayout.LabelField("Event Stack", EditorStyles.boldLabel);
        var stack = eventStackHandler.GetStack();

        if (stack.Count == 0)
        {
            EditorGUILayout.LabelField("No event in stack");
        }
        else
        {
            foreach (var evt in stack)
            {
                EditorGUILayout.LabelField(evt);
            }
        }

        if(GUILayout.Button("Push Test Event"))
        {
            eventStackHandler.PushEvent($"TestEvent_{Random.Range(1, 100)}");
        }

        if (GUILayout.Button("Pop Event"))
        {
            eventStackHandler.PopEvent();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
