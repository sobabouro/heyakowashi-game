using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;
using UnityEngine.Events;


#if UNITY_EDITOR
using System.Net;
using System.Net.Sockets;
#else
using System.IO;
using Windows.Networking.Sockets;
using System.Threading.Tasks;
#endif

[System.Serializable]
public class ReceiveEvent : UnityEvent<UDPServer.Message>
{
}
public class UDPServer : MonoBehaviour
{
    [SerializeField]
    private int listenPort = 8000;

    [SerializeField, Tooltip("UDPメッセージ受信時実行処理")]
    private ReceiveEvent receiveEvent;

    private Queue<Message> queue = new Queue<Message>();
    private Message msg;

    public struct Message
    {
        public byte[] bytes;
        System.DateTime time;

        public Message(byte[] b, System.DateTime t)
        {
            bytes = b;
            time = t;
        }
        public Message(Quaternion q, System.DateTime t)
        {
            bytes = new byte[sizeof(float) * 4];
            Array.Copy(BitConverter.GetBytes(q.x), 0, bytes, 0 * sizeof(float), sizeof(float));
            Array.Copy(BitConverter.GetBytes(q.y), 0, bytes, 1 * sizeof(float), sizeof(float));
            Array.Copy(BitConverter.GetBytes(q.z), 0, bytes, 2 * sizeof(float), sizeof(float));
            Array.Copy(BitConverter.GetBytes(q.w), 0, bytes, 3 * sizeof(float), sizeof(float));
            time = t;
        }

        public override string ToString()
        {
            string temp = Encoding.UTF8.GetString(bytes);
            return temp;
        }

        public Quaternion ToQuaternion()
        {
            Quaternion q = Quaternion.identity;
            q.x = BitConverter.ToSingle(bytes, 0 * sizeof(float));
            q.y = BitConverter.ToSingle(bytes, 1 * sizeof(float));
            q.z = BitConverter.ToSingle(bytes, 2 * sizeof(float));
            q.w = BitConverter.ToSingle(bytes, 3 * sizeof(float));
            return q;
        }
    }

    public void OnMessage(Message msg)
    {
        // ここで適当に処理する
        Debug.LogFormat(msg.ToQuaternion().ToString());
    }

#if UNITY_EDITOR
    private UdpClient udpClient;
    private IPEndPoint endPoint;

    private void Start()
    {
        endPoint = new IPEndPoint(IPAddress.Any, listenPort);
        udpClient = new UdpClient(endPoint);
    }

    private void Update()
    {
        while (udpClient.Available > 0)
        {
            byte[] bytes = udpClient.Receive(ref endPoint);
            queue.Enqueue(new Message(bytes, System.DateTime.Now));
        }

        while (queue.Count > 0)
        {
            msg = queue.Dequeue();
            receiveEvent.Invoke(msg);
        }   
    }

#else

    private DatagramSocket socket;
    private object lockObject = new object();

    private const int MAX_BUFFER_SIZE = 1024;
    private byte[] buffer = new byte[MAX_BUFFER_SIZE];

    private void Start()
    {
        Task.Run(async () => {
            try {
                socket = new DatagramSocket();
                socket.MessageReceived += MessageReceived;
                await socket.BindServiceNameAsync(listenPort.ToString());
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        });
    }

    private void Update()
    {
        lock (lockObject) {
            while (queue.Count > 0)
            {
                msg = queue.Dequeue();
                receiveEvent.Invoke(msg);
            }
        }
    }

    async void MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        using (var stream = args.GetDataStream().AsStreamForRead()) {
            await stream.ReadAsync(buffer, 0, MAX_BUFFER_SIZE);
            lock (lockObject) {
                queue.Enqueue(new Message(buffer, System.DateTime.Now));
            }
        }
    }

#endif
}