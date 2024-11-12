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
    // 受信時イベント
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
        // サーバに接続する。
        Thread receivedThread = new Thread(() =>
        {
            ConnectTcpServer(port, ip);
        });
        receivedThread.Start();
    }


    /// <summary>
    /// サーバに接続する。
    /// </summary>
    private void ConnectTcpServer(int port, string ip)
    {
        Debug.Log($"TcpClient Request port:{port}");
        try
        {
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

            // 受信開始
            MessageReceivTask();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    /// <summary>
    /// データを送信する。
    /// </summary>
    /// <param name="bytes">送信するしたデータ</param>
    public void SendMessage(byte[] bytes)
    {
        // 接続中なら
        if (_tcpClient != null && _tcpClient.Connected)
        {
            Thread sendThread = new Thread(() =>
            {
                // データを送信
                _stream.Write(bytes, 0, bytes.Length);
                _stream.Flush();
                //Debug.Log("TcpClient SendMessage");
            });
            sendThread.Start();
        }
    }

    /// <summary>
    /// 接続が切れるまで受信を繰り返す。非同期処理。受信時に登録したイベントを呼び出す。
    /// </summary>
    private async void MessageReceivTask()
    {
        // 接続が切れるまで受信を繰り返す
        while (_tcpClient != null && _tcpClient.Connected)
        {
            try
            {
                // データを受信
                byte[] buffer = new byte[_tcpClient.ReceiveBufferSize];
                await _stream.ReadAsync(buffer, 0, buffer.Length);
                //Debug.Log("TcpClient ReceivedMessage");

                // 受信時イベントを呼び出す。
                onReceiveEvents.Invoke(buffer);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                break;
            }
            // クライアントの接続が切れたら
            if (_tcpClient.Client.Poll(1000, SelectMode.SelectRead) && (_tcpClient.Client.Available == 0))
            {
                Debug.Log("TcpClient Disconnect: " + _tcpClient.Client.RemoteEndPoint);
                break;
            }
        }
        // 接続を閉じる
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
                // TcpClientを作成し、サーバーと接続する
                // 接続完了するまでブロッキングする
                _socket = new StreamSocket();
                await _socket.ConnectAsync(new HostName(ip), port.ToString());
                UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log($"TcpClient Connect"); }, true);
                // 入出力Streamを取得する
                _inputStream = _socket.InputStream.AsStreamForRead();
                _outputStream = _socket.OutputStream.AsStreamForWrite();
                // 受信開始
                MessageReceivTask();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        });

    }

    
    /// <summary>
    /// データを送信する。
    /// </summary>
    /// <param name="bytes">送信するしたデータ</param>
    public void SendMessage(byte[] bytes)
    {
        // 接続中なら
        if (_outputStream != null)
        {
            Task.Run(async () => {
                // データを送信
                await _outputStream.WriteAsync(bytes);
                await _outputStream.FlushAsync();
                // UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log("TcpClient SendMessage"); }, true);
            });
        }
    }
        
    /// <summary>
    /// 接続が切れるまで受信を繰り返す。非同期処理。受信時に登録したイベントを呼び出す。
    /// </summary>
    private async void MessageReceivTask()
    {
        // 接続が切れるまで送受信を繰り返す
        while (_inputStream != null && !_isStopMessageReceivedTask)
        {

            try
            {
                // データを受信
                await _inputStream.ReadAsync(_buffer, 0, MAX_BUFFER_SIZE);
                // UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log("TcpClient ReceivedMessage"); }, true);

                // 受信時イベントを呼び出す。
                onReceiveEvents.Invoke(_buffer);
            }
            catch (Exception e)
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log(e.ToString()); }, true);
                break;
            }
        }
        // 接続を閉じる
        _socket?.Dispose();
        _inputStream?.Close();
        _outputStream?.Close();
        UnityEngine.WSA.Application.InvokeOnAppThread(() => { Debug.Log("TcpClient Close"); }, true);
    }
#endif
}