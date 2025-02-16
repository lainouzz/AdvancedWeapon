using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitch : MonoBehaviour
{
    GameInput gameInput;

    public bool hasSwitched;
    public bool canSwitch;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        gameInput = new GameInput();
        gameInput.Enable();
    }

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float switchInput = gameInput.Player.Switch.ReadValue<float>();
        if (switchInput >= 0.5f && !canSwitch)
        {
            canSwitch = true;
            if (!hasSwitched)
            {
                HandleSwitchInput();
            }
                    
        }      

        if (switchInput < 0.5f)
        {
            canSwitch = false;
        }
    }

    private void HandleSwitchInput()
    {
        hasSwitched = true;
        foreach (Transform weapon in transform)
        {
            weapon.gameObject.SetActive(!weapon.gameObject.activeSelf);  
        }
        hasSwitched = false;
    }
}
