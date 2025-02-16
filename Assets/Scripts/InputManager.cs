using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager inputInstance {  get; private set; }
    public GameInput gameInput { get; private set; }

    private void Awake()
    {
        if(inputInstance != null && inputInstance != this)
        {
            Destroy(inputInstance);
            return;
        }

        inputInstance = this;

        DontDestroyOnLoad(gameObject);

        gameInput = new GameInput();
        gameInput.Enable();
    }
}
