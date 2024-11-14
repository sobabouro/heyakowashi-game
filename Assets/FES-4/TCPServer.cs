using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;

public class TCPServer
{
    // ��M���C�x���g
    public event Action<byte[]> onReceiveEvents;
    public void AddReceiveEvent(Action<byte[]> action)
    {
        onReceiveEvents += action;
    }

    private TcpListener _tcpListener = null;
    private TcpClient _tcpClient = null;
    private NetworkStream _stream = null;


    public TCPServer(int port)
    {
        _tcpListener = new TcpListener(IPAddress.Any, port);
        _tcpListener.Start();
        //�R�[���o�b�N�ݒ�@�������̓R�[���o�b�N�֐��ɓn�����
        _tcpListener.BeginAcceptSocket(DoAcceptTcpClientCallback, _tcpListener);
        Debug.Log($"TCPServer Wait port:{port}");
    }


    /// <summary>
    /// �N���C�A���g����̐ڑ�����
    /// </summary>
    private void DoAcceptTcpClientCallback(IAsyncResult ar)
    {
        // �n���ꂽ���̂����o��
        TcpListener tcpListener = (TcpListener)ar.AsyncState;
        _tcpClient = tcpListener.EndAcceptTcpClient(ar);
        Debug.Log("TCPServer Connect: " + _tcpClient.Client.RemoteEndPoint);

        // �ڑ������l�Ƃ̃l�b�g���[�N�X�g���[�����擾
        _stream = _tcpClient.GetStream();
        // �ǂݎ��A�������݂̃^�C���A�E�g��10�b�ɂ���
        //�f�t�H���g��Infinite�ŁA�^�C���A�E�g���Ȃ�
        //(.NET Framework 2.0�ȏオ�K�v)
        _stream.ReadTimeout = 10000;
        _stream.WriteTimeout = 10000;

        // ��M�J�n
        Thread receivedThread = new Thread(() =>
        {
            MessageReceivedTask();
        });
        receivedThread.Start();
    }

    /// <summary>
    /// �f�[�^�𑗐M����B
    /// </summary>
    /// <param name="bytes">���M���邵���f�[�^</param>
    public void SendMessage(byte[] bytes)
    {
        // �ڑ����Ȃ�
        if (_tcpClient != null && _tcpClient.Connected)
        {
            Thread sendThread = new Thread(() =>
            {
                // �f�[�^�𑗐M
                _stream.Write(bytes, 0, bytes.Length);
                _stream.Flush();
                //Debug.Log("TCPServer SendMessage");
            });
            sendThread.Start();
        }
    }

    /// <summary>
    /// �ڑ����؂��܂Ŏ�M���J��Ԃ��B�񓯊������B��M���ɓo�^�����C�x���g���Ăяo���B
    /// </summary>
    private async void MessageReceivedTask()
    {
        // �ڑ����؂��܂ő���M���J��Ԃ�
        while (_tcpClient != null && _tcpClient.Connected)
        {
            try
            {
                // �f�[�^����M
                byte[] buffer = new byte[_tcpClient.ReceiveBufferSize];
                await _stream.ReadAsync(buffer, 0, buffer.Length);
                //Debug.Log("TCPServer ReceivedMessage");

                // ��M���C�x���g���Ăяo���B
                onReceiveEvents.Invoke(buffer);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                break;
            }
            // �N���C�A���g�̐ڑ����؂ꂽ��
            if (_tcpClient.Client.Poll(1000, SelectMode.SelectRead) && (_tcpClient.Client.Available == 0))
            {
                Debug.Log("TCPServer Disconnect: " + _tcpClient.Client.RemoteEndPoint);
                break;
            }
        }

        // �ڑ������
        _stream?.Dispose();
        _tcpListener?.Stop();
        _tcpClient?.Close();
        Debug.Log("TcpServer Close");
    }
}