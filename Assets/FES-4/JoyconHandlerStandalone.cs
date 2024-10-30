using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class JoyconHandlerStandalone : MonoBehaviour
{
    private Transform m_transform;

    // Values made available via Unity
    public float[] stick;
    public Quaternion orientation;

    private bool[] buttons_down = new bool[13];
    private bool[] buttons_up = new bool[13];
    private bool[] buttons = new bool[13];

    private void Start()
    {
        m_transform = gameObject.transform;
    }

    public void UpdateData(UDPServer.Message message)
    {
        if(message.bytes[0] == 0x02)
        {
            m_transform.rotation = message.ToQuaternion();
        }
        else if(message.bytes[0] == 0x03)
        {
            for (int index = 0; index < 13; index++)
            {
                buttons_down[index] = BitConverter.ToBoolean(message.bytes, 1 + index * sizeof(bool));
            }
            for (int index = 0; index < 13; index++)
            {
                buttons_up[index] = BitConverter.ToBoolean(message.bytes, 14 + index * sizeof(bool));
            }
            for (int index = 0; index < 13; index++)
            {
                buttons[index] = BitConverter.ToBoolean(message.bytes, 27 + index * sizeof(bool));
            }
        }
    }

    public bool GetButtonDown(Joycon.Button b)
    {
        bool result = buttons_down[(int)b];
        buttons_down[(int)b] = false;
        return result;
    }
    public bool GetButton(Joycon.Button b)
    {
        return buttons[(int)b];
    }
    public bool GetButtonUp(Joycon.Button b)
    {
        bool result = buttons_up[(int)b];
        buttons_up[(int)b] = false;
        return result;
    }

    void Update()
    {
        // GetButtonDown checks if a button has been pressed (not held)
        if (GetButtonDown(Joycon.Button.SHOULDER_2))
        {
            Debug.Log("Shoulder button 2 pressed");
  
        }
        // GetButtonDown checks if a button has been released
        if (GetButtonUp(Joycon.Button.SHOULDER_2))
        {
            Debug.Log("Shoulder button 2 released");
        }
        // GetButtonDown checks if a button is currently down (pressed or held)
        if (GetButton(Joycon.Button.SHOULDER_2))
        {
            Debug.Log("Shoulder button 2 held");
        }

        if (GetButtonDown(Joycon.Button.HOME) || GetButtonDown(Joycon.Button.CAPTURE))
        {
            Debug.Log("HOME");
        }

        if (GetButtonDown(Joycon.Button.DPAD_DOWN))
        {
            Debug.Log("DPAD_DOWN");
        }
        if (GetButtonDown(Joycon.Button.DPAD_UP))
        {
            Debug.Log("DPAD_UP");
        }
        if (GetButtonDown(Joycon.Button.DPAD_LEFT))
        {
            Debug.Log("DPAD_LEFT");
        }
        if (GetButtonDown(Joycon.Button.DPAD_RIGHT))
        {
            Debug.Log("DPAD_RIGHT");
        }
    }
}
