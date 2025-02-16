using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

[CreateAssetMenu(fileName = "EventStackHandler", menuName = "EventSystem/EventStackHandler")]

public class EventStackHandler : ScriptableObject
{
    public bool hasFiredEvent;
    public bool hasPoppedEvent;

    private Stack<string> eventStack = new Stack<string>();
    private List<(string message, Color color)> eventLogs = new List<(string message, Color color)>();

    public void PushEvent(string newEvent)
    {
        eventStack.Push(newEvent);
        Debug.Log($"Event Pushed: {newEvent}");
        eventLogs.Add(($"Event Pushed: {newEvent}", Color.green));
        TrimLogs();
    }

    public string PopEvent()
    {
        if (eventStack.Count > 0)
        {
            string poppedEvent = eventStack.Pop();
            Debug.Log($"Event Popped: {poppedEvent}");
            eventLogs.Add(($"Event Popped: {poppedEvent}", Color.red));
            TrimLogs();
            return poppedEvent;
        }
        Debug.Log("Event stack empty");
        eventLogs.Add(("Event Stack Empty", Color.red));
        TrimLogs();
        return null;
    }

    public string Peak()
    {
        return eventStack.Count > 0 ? eventStack.Peek() : "No event";
    }

    public List<string> GetStack()
    {
        return new List<string>(eventStack);
    }
    public List<(string, Color)> GetLogs()
    {
        return new List<(string, Color)> (eventLogs);
    }

    public void ResetEvent()
    {
        hasFiredEvent = false;
        hasPoppedEvent = false;
    }

    private void TrimLogs()
    {
       if(eventLogs.Count > 3)
       {
        eventLogs.RemoveAt(0);
       }
    }
}
