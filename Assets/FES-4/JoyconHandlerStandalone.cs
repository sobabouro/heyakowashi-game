using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class JoyconHandlerStandalone : MonoBehaviour
{
    [Header("Button Eventt")]
    [SerializeField] private UnityEvent UpPushEventt;
    [SerializeField] private UnityEvent DownPushEventt;
    [SerializeField] private UnityEvent LeftPushEventt;
    [SerializeField] private UnityEvent RightPushEventt;
    [SerializeField] private UnityEvent Shoulder1PushEventt;
    [SerializeField] private UnityEvent Shoulder2PushEventt;

    private Transform m_transform;

    // Values made available via Unity
    private float[] stick;
    private Quaternion orientation;

    // ボタンの状態
    private bool[] buttons_down = new bool[13];
    private bool[] buttons_up = new bool[13];
    private bool[] buttons = new bool[13];

    private void Start()
    {
        m_transform = gameObject.transform;
        UDPServer server = GetComponent<UDPServer>();
        if (server != null) server.AddReceiveEvent(GetMessage);
    }

    // UDPで受信したデータを取り出す
    public void GetMessage(Message message)
    {
        // 傾きデータなら
        if (message.bytes[0] == 0x02)
        {
            orientation = message.ToQuaternion();
        }
        // ボタンの状態データなら
        else if (message.bytes[0] == 0x03)
        {
            // いい感じにデコード
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

    private bool GetButtonDown(Joycon.Button b)
    {
        bool result = buttons_down[(int)b];
        buttons_down[(int)b] = false;
        return result;
    }
    private bool GetButton(Joycon.Button b)
    {
        return buttons[(int)b];
    }
    private bool GetButtonUp(Joycon.Button b)
    {
        bool result = buttons_up[(int)b];
        buttons_up[(int)b] = false;
        return buttons_up[(int)b];
    }

    private void Update()
    {
        if (GetButtonDown(Joycon.Button.DPAD_UP))
        {
            Debug.Log("Up button pressed");
            UpPushEventt.Invoke();
        }
        if (GetButtonDown(Joycon.Button.DPAD_DOWN))
        {
            Debug.Log("Down button pressed");
            DownPushEventt.Invoke();
        }
        if (GetButtonDown(Joycon.Button.DPAD_LEFT))
        {
            Debug.Log("Left button pressed");
            LeftPushEventt.Invoke();
        }
        if (GetButtonDown(Joycon.Button.DPAD_RIGHT))
        {
            Debug.Log("Right button pressed");
            RightPushEventt.Invoke();
        }
        if (GetButtonDown(Joycon.Button.SHOULDER_1))
        {
            Debug.Log("Shoulder button 1 pressed");
            Shoulder1PushEventt.Invoke();
        }
        if (GetButtonDown(Joycon.Button.SHOULDER_2))
        {
            Debug.Log("Shoulder button 2 pressed");
            Shoulder2PushEventt.Invoke();
        }

        m_transform.rotation = orientation;
    }

}
