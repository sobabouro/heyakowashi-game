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
    // 受信時イベント
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
        //コールバック設定　第二引数はコールバック関数に渡される
        _tcpListener.BeginAcceptSocket(DoAcceptTcpClientCallback, _tcpListener);
        Debug.Log($"TCPServer Wait port:{port}");
    }


    /// <summary>
    /// クライアントからの接続処理
    /// </summary>
    private void DoAcceptTcpClientCallback(IAsyncResult ar)
    {
        // 渡されたものを取り出す
        TcpListener tcpListener = (TcpListener)ar.AsyncState;
        _tcpClient = tcpListener.EndAcceptTcpClient(ar);
        Debug.Log("TCPServer Connect: " + _tcpClient.Client.RemoteEndPoint);

        // 接続した人とのネットワークストリームを取得
        _stream = _tcpClient.GetStream();
        // 読み取り、書き込みのタイムアウトを10秒にする
        //デフォルトはInfiniteで、タイムアウトしない
        //(.NET Framework 2.0以上が必要)
        _stream.ReadTimeout = 10000;
        _stream.WriteTimeout = 10000;

        // 受信開始
        Thread receivedThread = new Thread(() =>
        {
            MessageReceivedTask();
        });
        receivedThread.Start();
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
                //Debug.Log("TCPServer SendMessage");
            });
            sendThread.Start();
        }
    }

    /// <summary>
    /// 接続が切れるまで受信を繰り返す。非同期処理。受信時に登録したイベントを呼び出す。
    /// </summary>
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
                //Debug.Log("TCPServer ReceivedMessage");

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
                Debug.Log("TCPServer Disconnect: " + _tcpClient.Client.RemoteEndPoint);
                break;
            }
        }

        // 接続を閉じる
        _stream?.Dispose();
        _tcpListener?.Stop();
        _tcpClient?.Close();
        Debug.Log("TcpServer Close");
    }
}