using System.Net;
using System.Threading;
using System.Net.Sockets;
using UnityEngine;

public class UDPCliant
{
    private UdpClient udpClient;

    public UDPCliant(int port)
    {
        try
        {
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            udpClient.Connect(new IPEndPoint(IPAddress.Broadcast, port));
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public void SendMessage(byte[] bytes)
    {
        Message mes = new Message(bytes, System.DateTime.Now);
        Thread thread = new Thread(() =>
        {
            udpClient.Send(mes.bytes, mes.bytes.Length);
        });
        thread.Start();
    }
}