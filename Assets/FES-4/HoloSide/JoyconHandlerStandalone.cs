using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class JoyconHandlerStandalone : MonoBehaviour
{
    [SerializeField, Tooltip("�|�[�g�ԍ�")] private int port = 50000;
    [SerializeField, Tooltip("IP�A�h���X")] private string ip = "192.168.20.14";

    [Header("Button Eventt")]
    [SerializeField] private UnityEvent PressedUpButtonEvents;
    [SerializeField] private UnityEvent PressedDownButtonEvents;
    [SerializeField] private UnityEvent PressedLeftButtonEvents;
    [SerializeField] private UnityEvent PressedRightButtonEvents;
    [SerializeField] private UnityEvent PressedShoulder1ButtonEvents;
    [SerializeField] private UnityEvent PressedShoulder2ButtonEvents;

    // �ʐM�p�ϐ�
    private TCPCliant _tcpCcliant = null;
    private Queue<Message> _queue = new Queue<Message>();
    private Message _message;
    private byte[] request_bytes = { 0x01 };
    private int quaternion_size = 4 * sizeof(float);
    private int puket_number = 0;
    private bool isChecked = true; // �f�[�^�g�p�ς݁H

    // �Q��
    private Transform m_transform;

    // Values made available via Unity
    private float[] stick;
    private Quaternion orientation = Quaternion.identity;
    // �{�^���̏��
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
        _tcpCcliant = new TCPCliant(port, ip);
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

        // �{�^���̏�Ԃ��`�F�b�N���ăC�x���g����������B
        CheckButtons();

        // ���g�̉�]���X�V����
        m_transform.rotation = orientation;
    }


    /// <summary>
    /// �f�[�^����M�����Ƃ��ɍs���B��M�����f�[�^��queue�ɋl�߂�B
    /// </summary>
    /// <param name="buffer">��M�����f�[�^</param>
    public void ReceivedMessage(byte[] buffer)
    {
        lock (_queue)
        {
            _queue.Enqueue(new Message(buffer, System.DateTime.Now));
        }
    }


    /// <summary>
    /// ��M�����f�[�^��ǂݍ��ށB
    /// </summary>
    /// <param name="message">��M�����f�[�^</param>
    private void ReadMessage(Message message)
    {
        // ���������Ƀf�R�[�h
        // �X���f�[�^
        orientation.x = BitConverter.ToSingle(message.bytes, 0 * sizeof(float));
        orientation.y = BitConverter.ToSingle(message.bytes, 1 * sizeof(float));
        orientation.z = BitConverter.ToSingle(message.bytes, 2 * sizeof(float));
        orientation.w = BitConverter.ToSingle(message.bytes, 3 * sizeof(float));
        // �{�^���̏�ԃf�[�^
        for (int i = 0; i < buttons.Length; i++)
        {
            bool button_tmp = BitConverter.ToBoolean(message.bytes, quaternion_size + i);
            // ���o����܂ł͘_���a
            buttons[i] = isChecked ? button_tmp : (buttons[i] | button_tmp);
        }
        isChecked = false;
        // Debug.Log(BitConverter.ToInt32(message.bytes, quaternion_size + buttons.Length));
    }


    /// <summary>
    /// �{�^���̏�Ԃ��`�F�b�N���ăC�x���g����������B
    /// </summary>
    private void CheckButtons()
    {
        // �������u�ԁA�b�����u�Ԃ𔻒�
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

        // ��ԃ`�F�b�N
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
