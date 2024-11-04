using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPCliant
{
    private UdpClient udpclient;

    public UDPCliant(int port = 8000)
    {
        try
        {
            udpclient = new UdpClient();
            udpclient.EnableBroadcast = true;
            udpclient.Connect(new IPEndPoint(IPAddress.Broadcast, port));
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public void SendMessage(Quaternion q)
    {
        Message mes = new Message(0x02, q, System.DateTime.Now);
        Thread thread = new Thread(() =>
        {
            udpclient.Send(mes.bytes, mes.bytes.Length);
        });
        thread.Start();
    }
    public void SendMessage(bool[] boolean_data)
    {
        Message mes = new Message(0x03, boolean_data, System.DateTime.Now);
        Thread thread = new Thread(() =>
        {
            udpclient.Send(mes.bytes, mes.bytes.Length);
        });
        thread.Start();
    }
}