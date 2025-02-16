using System.Collections.Generic;
using UnityEngine;

public class EventLogDisplay : MonoBehaviour
{
    [SerializeField] EventStackHandler eventStackHandler;

    private GUIStyle whiteStyle;
    private GUIStyle greenStyle;
    private GUIStyle redStyle;

    void Start()
    {
        whiteStyle = new GUIStyle { fontSize = 14, normal = { textColor = Color.white } };
        greenStyle = new GUIStyle { fontSize = 14, normal = { textColor = Color.green } };
        redStyle = new GUIStyle { fontSize = 14, normal = { textColor = Color.red } };
    }

    private void OnGUI()
    {
        if (eventStackHandler == null) return;

        List<(string message, Color color)> logs = eventStackHandler.GetLogs();
        float y = 0;

        foreach (var log in logs)
        {
            GUIStyle style = log.color == Color.green ? greenStyle : redStyle;
            GUI.Label(new Rect(10f, y, 500f, 20f), log.message, style);
            y += 20;
        }

        GUI.Label(new Rect(10f, y, 500f, 20f), $"Current Stack Count: {eventStackHandler.Peak()}", whiteStyle);
    }
}
