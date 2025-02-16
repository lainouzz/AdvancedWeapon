using UnityEngine;

public class GameEvent : MonoBehaviour
{
    [SerializeField] private EventStackHandler eventStackHandler;
    [SerializeField] private InspectScript inspectScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eventStackHandler.PushEvent("GameStart");
    }

    public void EnterMainMenu()
    {
        eventStackHandler.PushEvent("Entered MainMenu");
    }

    public void ExitMainMenu()
    {
        eventStackHandler.PushEvent("Exited MainMenu");
    }

    public void EnterOptionsMenu()
    {
        eventStackHandler.PushEvent("Entered OptionsMenu");
    }

    public void ExitOptionsMenu()
    {
        eventStackHandler.PushEvent("Exited OptionsMenu");
    }

    public void GameQuit()
    {
        eventStackHandler.PushEvent("Game Quit");
        Application.Quit();
    }
}
