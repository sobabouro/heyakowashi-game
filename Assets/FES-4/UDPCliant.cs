using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;

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
        UDPServer.Message mes = new UDPServer.Message(q, System.DateTime.Now);
        Thread thread = new Thread(() =>
        {
            udpclient.Send(mes.bytes, mes.bytes.Length);
        });
        thread.Start();
    }
}