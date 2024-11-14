using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Net;
using System.Net.Sockets;
#else
using System.IO;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
#endif

public class UDPServer: MonoBehaviour
{
    [SerializeField] private int listenPort = 50000;

    [SerializeField] public event Action<Message> receiveAction;

    private Queue<Message> queue = new Queue<Message>();
    private Message message;

    public void AddReceiveEvent(Action<Message> action)
    {
        receiveAction += action;
    }

#if UNITY_EDITOR
    private IPEndPoint endPoint;
    private UdpClient udpClient;

    private void Awake()
    {
        endPoint = new IPEndPoint(IPAddress.Any, listenPort);
        udpClient = new UdpClient(endPoint);
    }

    private void Update()
    {
        if (udpClient == null) return;

        while (udpClient.Available > 0)
        {
            try
            {
                byte[] bytes = udpClient.Receive(ref endPoint);
                queue.Enqueue(new Message(bytes, System.DateTime.Now));
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        while (queue.Count > 0)
        {
            message = queue.Dequeue();
            receiveAction.Invoke(message);
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
            catch (Exception e)
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
                message = queue.Dequeue();
                receiveAction.Invoke(message);
            }
        }
    }

    async void MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        using (var stream = args.GetDataStream().AsStreamForRead())
        {
            await stream.ReadAsync(buffer, 0, MAX_BUFFER_SIZE);
            lock (lockObject)
            {
                queue.Enqueue(new Message(buffer, System.DateTime.Now));
            }
        }
    }

#endif
}