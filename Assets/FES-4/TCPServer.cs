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
    private TcpListener _tcpListener = null;
    private TcpClient _tcpClient = null;
    private NetworkStream _stream = null;

    private const int MAX_BUFFER_SIZE = 1024;
    private byte[] _bytes = new byte[MAX_BUFFER_SIZE];

    public bool send_flag = false;
    public TCPServer(int port)
    {
        _tcpListener = new TcpListener(IPAddress.Any, port);
        _tcpListener.Start();
        //コールバック設定　第二引数はコールバック関数に渡される
        _tcpListener.BeginAcceptSocket(DoAcceptTcpClientCallback, _tcpListener);
        Debug.Log($"TCPServer Wait port:{port}");
    }

    // クライアントからの接続処理
    private void DoAcceptTcpClientCallback(IAsyncResult ar)
    {
        // 渡されたものを取り出す
        TcpListener tcpListener = (TcpListener)ar.AsyncState;
        _tcpClient = tcpListener.EndAcceptTcpClient(ar);
        Debug.Log("Connect: " + _tcpClient.Client.RemoteEndPoint);

        // 接続した人とのネットワークストリームを取得
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
                //Debug.Log($"ReceivedMessage {buffer[0]}");

                // データを送信
                await _stream.WriteAsync(_bytes, 0, _bytes.Length);
                _stream.Flush();
                send_flag = true;
                //Debug.Log("SendMessage");
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
            }
        }
        Debug.Log("TcpServer Close");
    }


    public void WrightMessage(byte[] bytes)
    {
        send_flag = false;
        _bytes = bytes;
    }

    // 終了処理
    protected virtual void OnApplicationQuit()
    {
        _stream?.Dispose();
        _tcpListener?.Stop();
        _tcpClient?.Close();
    }
}