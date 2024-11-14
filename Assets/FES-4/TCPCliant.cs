using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Text;


#if UNITY_EDITOR
using System.Net.Sockets;
#else
using System.Threading.Tasks;
using System.IO;
using Windows.Networking;
using Windows.Networking.Sockets;
#endif


public class TCPCliant
{
    // ��M���C�x���g
    public event Action<byte[]> onReceiveEvents;

    public void AddReceiveEvent(Action<byte[]> action)
    {
        onReceiveEvents += action;
    }

#if UNITY_EDITOR
    private TcpClient _tcpClient = null;
    private NetworkStream _stream = null;

    public TCPCliant(int port, string ip)
    {
        // �T�[�o�ɐڑ�����B
        Thread receivedThread = new Thread(() =>
        {
            ConnectTcpServer(port, ip);
        });
        receivedThread.Start();
    }


    /// <summary>
    /// �T�[�o�ɐڑ�����B
    /// </summary>
    private void ConnectTcpServer(int port, string ip)
    {
        Debug.Log($"TcpClient Request port:{port}");
        try
        {
            // TcpClient���쐬���A�T�[�o�[�Ɛڑ�����
            // �ڑ���������܂Ńu���b�L���O����
            _tcpClient = new TcpClient(ip, port);
            // NetworkStream���擾����
            _stream = _tcpClient.GetStream();

            // �ǂݎ��A�������݂̃^�C���A�E�g��10�b�ɂ���
            //�f�t�H���g��Infinite�ŁA�^�C���A�E�g���Ȃ�
            //(.NET Framework 2.0�ȏオ�K�v)
            _stream.ReadTimeout = 10000;
            _stream.WriteTimeout = 10000;

            // ��M�J�n
            MessageReceivTask();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
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
                //Debug.Log("TcpClient SendMessage");
            });
            sendThread.Start();
        }
    }

    /// <summary>
    /// �ڑ����؂��܂Ŏ�M���J��Ԃ��B�񓯊������B��M���ɓo�^�����C�x���g���Ăяo���B
    /// </summary>
    private async void MessageReceivTask()
    {
        // �ڑ����؂��܂Ŏ�M���J��Ԃ�
        while (_tcpClient != null && _tcpClient.Connected)
        {
            try
            {
                // �f�[�^����M
                byte[] buffer = new byte[_tcpClient.ReceiveBufferSize];
                await _stream.ReadAsync(buffer, 0, buffer.Length);
                //Debug.Log("TcpClient ReceivedMessage");

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
                Debug.Log("TcpClient Disconnect: " + _tcpClient.Client.RemoteEndPoint);
                break;
            }
        }
        // �ڑ������
        _stream?.Close();
        _tcpClient?.Close();
        Debug.Log("TcpClient Close"); 
    }
#else
    private StreamSocket _socket = null;
    private Stream _inputStream = null;
    private Stream _outputStream = null;

    private const int MAX_BUFFER_SIZE = 1024;
    private byte[] _buffer = new byte[MAX_BUFFER_SIZE];
    private bool _isStopMessageReceivedTask = false;

    public TCPCliant(int port, string ip)
    {
        Task.Run(async () =>
        {
            try
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log($"TcpClient Request port:{port}, Now waiting..."); }, true);
                // TcpClient���쐬���A�T�[�o�[�Ɛڑ�����
                // �ڑ���������܂Ńu���b�L���O����
                _socket = new StreamSocket();
                await _socket.ConnectAsync(new HostName(ip), port.ToString());
                UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log($"TcpClient Connect"); }, true);
                // ���o��Stream���擾����
                _inputStream = _socket.InputStream.AsStreamForRead();
                _outputStream = _socket.OutputStream.AsStreamForWrite();
                // ��M�J�n
                MessageReceivTask();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        });

    }

    
    /// <summary>
    /// �f�[�^�𑗐M����B
    /// </summary>
    /// <param name="bytes">���M���邵���f�[�^</param>
    public void SendMessage(byte[] bytes)
    {
        // �ڑ����Ȃ�
        if (_outputStream != null)
        {
            Task.Run(async () => {
                // �f�[�^�𑗐M
                await _outputStream.WriteAsync(bytes);
                await _outputStream.FlushAsync();
                // UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log("TcpClient SendMessage"); }, true);
            });
        }
    }
        
    /// <summary>
    /// �ڑ����؂��܂Ŏ�M���J��Ԃ��B�񓯊������B��M���ɓo�^�����C�x���g���Ăяo���B
    /// </summary>
    private async void MessageReceivTask()
    {
        // �ڑ����؂��܂ő���M���J��Ԃ�
        while (_inputStream != null && !_isStopMessageReceivedTask)
        {

            try
            {
                // �f�[�^����M
                await _inputStream.ReadAsync(_buffer, 0, MAX_BUFFER_SIZE);
                // UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log("TcpClient ReceivedMessage"); }, true);

                // ��M���C�x���g���Ăяo���B
                onReceiveEvents.Invoke(_buffer);
            }
            catch (Exception e)
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log(e.ToString()); }, true);
                break;
            }
        }
        // �ڑ������
        _socket?.Dispose();
        _inputStream?.Close();
        _outputStream?.Close();
        UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log("TcpClient Close"); }, true);
    }
#endif
}