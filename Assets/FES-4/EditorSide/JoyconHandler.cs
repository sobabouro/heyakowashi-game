using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Http;
using UnityEngine;

public class JoyconHandler : MonoBehaviour
{
    [SerializeField, Tooltip("���ʔԍ�")] public int jc_ind = 0;
    [SerializeField, Tooltip("�|�[�g�ԍ�")] public int port = 50000;

    // �Q��
    private List<Joycon> joycons;
    private Joycon joycon = null;
    private Transform m_transform;

    // Values made available via Unity
    public float[] stick;
    private bool[] buttons = new bool[13];
    public Quaternion orientation;

    // �ʐM�p�ϐ�
    private TCPServer tcpServer;
    private int qs = 4 * sizeof(float);
    private byte[] send_bytes = new byte[4 * sizeof(float) + 13 + sizeof(int)];
    private int puket_number = 0;
    private bool isSent = true; // ���M�ς݃f�[�^������H

    private void Start()
    {
        // get the public Joycon array attached to the JoyconManager in scene
        joycons = JoyconManager.Instance.j;
        if (joycons.Count < jc_ind + 1)
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log($"jc_ind: {jc_ind}");
        joycon = joycons[jc_ind];
        m_transform = gameObject.transform;

        // �ʐM�p�T�[�o���N��
        tcpServer = new TCPServer(port);
        // ��M���C�x���g��o�^
        tcpServer.AddReceiveEvent(SendMessage);

        // �����ڂ���������
        Color[] colers = new Color[] { Color.blue, Color.red, Color.green };
        gameObject.GetComponent<Renderer>().material.color = colers[jc_ind];
    }

    // Update is called once per frame
    void Update()
    {
        // make sure the Joycon only gets checked if attached
        if (joycon == null) return;

        if (joycon.GetButton(Joycon.Button.SHOULDER_2))
        {
            foreach (Joycon joycon in joycons)
            {
                joycon.Recenter();
            }
        }

        if (joycon.GetButtonDown(Joycon.Button.DPAD_DOWN))
        {
            Debug.Log("Rumble");

            // Rumble for 200 milliseconds, with low frequency rumble at 160 Hz and high frequency rumble at 320 Hz. For more information check:
            // https://github.com/dekuNukem/Nintendo_Switch_Reverse_Engineering/blob/master/rumble_data_table.md

            joycon.SetRumble(160, 320, 0.6f, 200);

            // The last argument (time) in SetRumble is optional. Call it with three arguments to turn it on without telling it when to turn off.
            // (Useful for dynamically changing rumble values.)
            // Then call SetRumble(0,0,0) when you want to turn it off.
        }

        // stick = j.GetStick();

        // buttons
        // ���M����܂ł͘_���a
        bool[] buttons_tmp = joycon.GetButtons();
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = isSent ? buttons_tmp[i] : (buttons[i] | buttons_tmp[i]);
        }
        isSent = false;
        // orientation
        orientation = joycon.GetVector();
        // Apply
        m_transform.rotation = orientation;
    }

    /// <summary>
    /// Joycon�̃f�[�^��byte�z��ɕϊ�����
    /// </summary>
    private void CreateJoyconMessage()
    {
        Array.Copy(BitConverter.GetBytes(orientation.x), 0, send_bytes, 0 * sizeof(float), sizeof(float));
        Array.Copy(BitConverter.GetBytes(orientation.y), 0, send_bytes, 1 * sizeof(float), sizeof(float));
        Array.Copy(BitConverter.GetBytes(orientation.z), 0, send_bytes, 2 * sizeof(float), sizeof(float));
        Array.Copy(BitConverter.GetBytes(orientation.w), 0, send_bytes, 3 * sizeof(float), sizeof(float));

        for (int i = 0; i < buttons.Length; i++)
        {
            send_bytes[qs + i] = Convert.ToByte(buttons[i]);
        }
        Array.Copy(BitConverter.GetBytes(puket_number), 0, send_bytes, qs + buttons.Length, sizeof(int));
        puket_number = (puket_number + 1) % 10000;  // puket_number��9999�𒴂�����0�ɖ߂�

    }

    /// <summary>
    /// �f�[�^�𑗐M����B�N���C�A���g���烊�N�G�X�g���������ꍇ�ɌĂяo���B
    /// </summary>
    public void SendMessage(byte[] receiveBytes)
    {
        // ���ɑ��M����f�[�^���쐬
        CreateJoyconMessage();
        tcpServer.SendMessage(send_bytes);
        isSent = true;
    }
}