using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class JoyconHandlerStandalone : MonoBehaviour
{
    [SerializeField, Tooltip("JoyconID")] private int id = 0;

    [Header("Button Eventt")]
    [SerializeField] private UnityEvent PressedUpButtonEvents;
    [SerializeField] private UnityEvent PressedDownButtonEvents;
    [SerializeField] private UnityEvent PressedLeftButtonEvents;
    [SerializeField] private UnityEvent PressedRightButtonEvents;
    [SerializeField] private UnityEvent PressedShoulder1ButtonEvents;
    [SerializeField] private UnityEvent PressedShoulder2ButtonEvents;

    // 通信用変数
    private TCPCliant _tcpCcliant = null;
    private Queue<Message> _queue = new Queue<Message>();
    private Message _message;
    private byte[] request_bytes = { 0x01 };
    private int quaternion_size = 4 * sizeof(float);
    private int puket_number = 0;
    private bool isChecked = true; // データ使用済み？

    // 参照
    private Transform m_transform;

    // Values made available via Unity
    private float[] stick;
    private Quaternion orientation = Quaternion.identity;
    // ボタンの状態
    private bool[] down_ = new bool[13];
    private bool[] buttons_down = new bool[13];
    private bool[] buttons_up = new bool[13];
    private bool[] buttons = new bool[13];

    private bool GetButtonDown(Joycon.Button b)
    {
        return buttons_down[(int)b];
    }
    private bool GetButton(Joycon.Button b)
    {
        return buttons[(int)b];
    }
    private bool GetButtonUp(Joycon.Button b)
    {
        return buttons_up[(int)b];
    }

    private void Start()
    {
        m_transform = gameObject.transform;
        // _tcpCcliant = new TCPCliant(port, ip);
        if (TCPCliantManagaer.instance.tcpCcliants.Count - 1 < id)
        {
            Debug.Log("クライアントが存在しません: " + id);
            return;
        }
        _tcpCcliant = TCPCliantManagaer.instance.tcpCcliants[id];
        _tcpCcliant.AddReceiveEvent(ReceivedMessage);
    }

    private void Update()
    {
        _tcpCcliant.SendMessage(request_bytes);

        while (_queue.Count > 0)
        {
            lock (_queue)
            {
                _message = _queue.Dequeue();
                ReadMessage(_message);
            }
        }

        // ボタンの状態をチェックしてイベントを処理する。
        CheckButtons();

        // 自身の回転を更新する
        m_transform.rotation = orientation;
    }


    /// <summary>
    /// データを受信したときに行う。受信したデータをqueueに詰める。
    /// </summary>
    /// <param name="buffer">受信したデータ</param>
    public void ReceivedMessage(byte[] buffer)
    {
        lock (_queue)
        {
            _queue.Enqueue(new Message(buffer, System.DateTime.Now));
        }
    }


    /// <summary>
    /// 受信したデータを読み込む。
    /// </summary>
    /// <param name="message">受信したデータ</param>
    private void ReadMessage(Message message)
    {
        // いい感じにデコード
        // 傾きデータ
        orientation.x = BitConverter.ToSingle(message.bytes, 0 * sizeof(float));
        orientation.y = BitConverter.ToSingle(message.bytes, 1 * sizeof(float));
        orientation.z = BitConverter.ToSingle(message.bytes, 2 * sizeof(float));
        orientation.w = BitConverter.ToSingle(message.bytes, 3 * sizeof(float));
        // ボタンの状態データ
        for (int i = 0; i < buttons.Length; i++)
        {
            bool button_tmp = BitConverter.ToBoolean(message.bytes, quaternion_size + i);
            // 検出するまでは論理和
            buttons[i] = isChecked ? button_tmp : (buttons[i] | button_tmp);
        }
        isChecked = false;
        // Debug.Log(BitConverter.ToInt32(message.bytes, quaternion_size + buttons.Length));
    }


    /// <summary>
    /// ボタンの状態をチェックしてイベントを処理する。
    /// </summary>
    private void CheckButtons()
    {
        // 押した瞬間、話した瞬間を判定
        int index = 0;
        for (index = 0; index < buttons.Length; index++)
        {
            buttons_up[index] = (down_[index] & !buttons[index]);
            buttons_down[index] = (!down_[index] & buttons[index]);
        }
        for (index = 0; index < buttons.Length; index++)
        {
            down_[index] = buttons[index];
        }

        // 状態チェック
        if (GetButtonDown(Joycon.Button.DPAD_UP))
        {
            Debug.Log("Up button pressed");
            PressedUpButtonEvents?.Invoke();
        }
        if (GetButtonDown(Joycon.Button.DPAD_DOWN))
        {
            Debug.Log("Down button pressed");
            PressedDownButtonEvents?.Invoke();
        }
        if (GetButtonDown(Joycon.Button.DPAD_LEFT))
        {
            Debug.Log("Left button pressed");
            PressedLeftButtonEvents?.Invoke();
        }
        if (GetButtonDown(Joycon.Button.DPAD_RIGHT))
        {
            Debug.Log("Right button pressed");
            PressedRightButtonEvents?.Invoke();
        }
        if (GetButtonDown(Joycon.Button.SHOULDER_1))
        {
            Debug.Log("Shoulder button 1 pressed");
            PressedShoulder1ButtonEvents?.Invoke();
        }
        if (GetButtonDown(Joycon.Button.SHOULDER_2))
        {
            Debug.Log("Shoulder button 2 pressed");
            PressedShoulder2ButtonEvents?.Invoke();
        }
        isChecked = true;
    }

}
