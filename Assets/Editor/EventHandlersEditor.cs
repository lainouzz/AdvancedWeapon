using UnityEditor;
using UnityEngine;
using Events;

[CustomEditor(typeof(EventHandler), true)]
public class EventHandlersEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EventHandler eh = target as EventHandler;
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Event Stacks", EditorStyles.boldLabel);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        foreach (EventHandler.IEvent evt in eh.EventStack)
        {
            string name = "   ¤" + eh.EventStack.IndexOf(evt) + ": " + evt.ToString();
            if (evt is Object obj)
            {
                if (GUILayout.Button(name, evt == eh.CurrentEvent ? EditorStyles.boldLabel : EditorStyles.label))
                {
                    Selection.activeObject = obj;
                }
            }
            else
            {
                EditorGUILayout.LabelField(name, evt == eh.CurrentEvent ? EditorStyles.boldLabel : EditorStyles.label);
            }
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        if (GUILayout.Button("Force Update Event"))
        {
            //eh.UpdateEvents(); // Call the UpdateEvent method on the EventHandler
            Debug.Log("Forced UpdateEvent() called on EventHandler.");
        }
    }
}
