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


public class TCPCliant : MonoBehaviour
{
    [SerializeField] private int port = 50000;
    [SerializeField] private string ip = "192.168.20.14";
    [SerializeField] private byte[] request_bytes = { 0x01 };

    public event Action<Message> receiveAction;
    private Queue<Message> _queue = new Queue<Message>();
    private Message _message;

    private object _lockObject = new object();
    public void AddReceiveEvent(Action<Message> action)
    {
        receiveAction += action;
    }

#if UNITY_EDITOR
    private TcpClient _tcpClient = null;
    private NetworkStream _stream = null;

    private void Start()
    {
        try
        {
            Debug.Log($"TcpClient Request port:{port}");
            // TcpClientを作成し、サーバーと接続する
            // 接続完了するまでブロッキングする
            _tcpClient = new TcpClient(ip, port);
            // NetworkStreamを取得する
            _stream = _tcpClient.GetStream();

            // 読み取り、書き込みのタイムアウトを10秒にする
            //デフォルトはInfiniteで、タイムアウトしない
            //(.NET Framework 2.0以上が必要)
            _stream.ReadTimeout = 10000;
            _stream.WriteTimeout = 10000;

            Thread sendthread = new Thread(() =>
            {
                MessageReceivedTask();
            });
            sendthread.Start();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void Update()
    {
        SendMessage(request_bytes);

        while (_queue.Count > 0)
        {
            lock (_lockObject)
            {
                _message = _queue.Dequeue();
                receiveAction.Invoke(_message);
            }
        }

    }

    public void SendMessage(byte[] bytes)
    {
        if (_tcpClient != null && _tcpClient.Connected)
        {
            Thread sendthread = new Thread(() =>
            {
                // データを送信
                _stream.Write(bytes, 0, bytes.Length);
                _stream.Flush();
                //Debug.Log("SendMessage");
            });
            sendthread.Start();
        }
    }
    private async void MessageReceivedTask()
    {
        // 接続が切れるまで送受信を繰り返す
        while (_tcpClient != null && _tcpClient.Connected)
        {
            try
            {
                // データを受信
                byte[] buffer = new byte[_tcpClient.ReceiveBufferSize];
                await _stream.ReadAsync(buffer, 0, buffer.Length);
                
                lock (_lockObject)
                {
                    _queue.Enqueue(new Message(buffer, System.DateTime.Now));
                }
                //Debug.Log("MessageReceived");
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            // クライアントの接続が切れたら
            if (_tcpClient.Client.Poll(1000, SelectMode.SelectRead) && (_tcpClient.Client.Available == 0))
            {
                Debug.Log("Disconnect: " + _tcpClient.Client.RemoteEndPoint);
                _tcpClient.Close();
                _stream = null;
            }
        }
        Debug.Log("TcpClient Close"); 
    }
#else
    private StreamSocket _socket = null;
    private Stream _inputStream = null;
    private Stream _outputStream = null;

    private const int MAX_BUFFER_SIZE = 1024;
    private byte[] _buffer = new byte[MAX_BUFFER_SIZE];
    private bool isStopMessageReceivedTask = false;
    private void Start()
    {
        Task.Run(async () =>
        {
            _socket = new StreamSocket();
            UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log($"TcpClient Request port:{port}"); }, true);
            await _socket.ConnectAsync(new HostName(ip), port.ToString());
            UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log($"TcpClient Connect"); }, true);
            _inputStream = _socket.InputStream.AsStreamForRead();
            _outputStream = _socket.OutputStream.AsStreamForWrite();
            MessageReceivedTask(_inputStream);
        });

    }

    private void Update()
    {
        SendMessage(request_bytes); 

        lock (_lockObject)
        {
            while (_queue.Count > 0)
            {
                _message = _queue.Dequeue();
                receiveAction.Invoke(_message);
            }
        }
    }

    public void SendMessage(byte[] bytes)
    {
        if (_outputStream != null) Task.Run(async () =>
        {
            await _outputStream.WriteAsync(bytes);
            await _outputStream.FlushAsync();
            // UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log("SendMessage"); }, true);
        });
    }

    private async void MessageReceivedTask(Stream inputStream)
    {
        while (!isStopMessageReceivedTask)
        {
            if (inputStream == null) break;

            try
            {
                await inputStream.ReadAsync(_buffer, 0, MAX_BUFFER_SIZE);
                lock (_lockObject)
                {
                    _queue.Enqueue(new Message(_buffer, System.DateTime.Now));
                }
                // UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log("MessageReceived"); }, true);
            }
            catch (Exception e)
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log(e.ToString()); }, true);
                break;
            }
        }
        UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log("TcpClient Close"); }, true);
    }
    public void OnDestroy()
    {
        isStopMessageReceivedTask = true;
        _socket?.Dispose();
        _inputStream?.Close();
    }
#endif
}