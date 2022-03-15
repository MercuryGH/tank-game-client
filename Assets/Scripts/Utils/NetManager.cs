using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Linq;
public static class NetManager
{
    private static Socket socket;
    private static ByteBuffer readBuff;

    private static Queue<ByteBuffer> sendQueue; // 发送的消息的队列。确定了成功发送则出队

    private static bool isConnecting = false;
    private static bool isClosing = false;

    private static Queue<MsgBase> receiveQueue; // 接收的消息的队列。处理后出队
    private static int receiveQueueSize = 0; // 相当于 receiveQueue.size()
    private const int MAX_MESSAGE_HANDLE = 10; // 一帧最多处理多少条消息

    public static bool usePingPong = true;
    public const int PING_INTERVAL = 30;
    private static float lastPingTime = 0;
    private static float lastPongTime = 0;

    // 网络事件
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }

    // 回调函数签名
    public delegate void EventListener(string arg);
    // {事件类型: 回调}。每一种事件类型可对应多个回调，当事件发生时，依次调用
    private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

    // 添加事件监听
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        // 添加事件
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        // 新增事件
        else
        {
            eventListeners[netEvent] = listener;
        }
    }
    // 删除事件监听
    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
        }
    }
    // 调用事件监听回调
    private static void InvokeEventListener(NetEvent netEvent, string arg)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](arg);
        }
    }

    // 消息委托类型
    public delegate void MsgListener(MsgBase msgBase);
    // 消息监听列表
    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        else
        {
            msgListeners[msgName] = listener;
        }
    }
    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
        }
    }
    private static void InvokeMsgListener(string msgName, MsgBase msgBase)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }

    public static void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
        {
            Debug.Log("Connect fail, already connected!");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("Connect fail, isConnecting");
            return;
        }

        InitState();

        socket.NoDelay = true; // 不使用Nagle算法，增加实时性 
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    private static void InitState()
    {
        socket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        readBuff = new ByteBuffer();
        sendQueue = new Queue<ByteBuffer>();
        isConnecting = false;
        isClosing = false;
        receiveQueue = new Queue<MsgBase>();
        receiveQueueSize = 0;
        lastPingTime = Time.time;
        lastPongTime = Time.time;

        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
    }

    // Connect回调
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ ");
            InvokeEventListener(NetEvent.ConnectSucc, "");
            isConnecting = false;
            //开始接收
            socket.BeginReceive(
                readBuff.bytes,
                readBuff.writeIdx,
                readBuff.RemainSize,
                0,
                ReceiveCallback,
                socket
            );
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Connect fail " + ex.ToString());
            InvokeEventListener(NetEvent.ConnectFail, ex.ToString());
            isConnecting = false;
        }
    }

    public static void Close()
    {
        //状态判断
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }

        if (sendQueue.Count > 0)
        {
            isClosing = true; // CLOSE_WAIT ready
        }
        else // 没有数据要发了，Send CLOSE
        {
            socket.Close();
            InvokeEventListener(NetEvent.Close, "");
        }
    }

    /**
     * 游戏模块均可调用的 Send 方法。向服务器发送数据包
     */
    public static void Send(MsgBase msg)
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if (isClosing)
        {
            return;
        }

        // 数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];

        // 小端组装长度
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);

        // 组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        // 组装消息体
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        // 写入队列
        ByteBuffer ba = new ByteBuffer(sendBytes);
        int count = 0;  // writeQueue的长度

        lock (sendQueue) // 上锁
        {
            sendQueue.Enqueue(ba);
            count = sendQueue.Count;
        }

        // send
        if (count >= 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length,
                0, SendCallback, socket);
        }
    }

    public static void SendCallback(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        if (socket == null || !socket.Connected)
        {
            return;
        }

        // EndSend 返回已经发送成功的字节数
        int count = socket.EndSend(ar);

        // 获取写入队列第一条数据
        ByteBuffer ba;
        lock (sendQueue)
        {
            ba = sendQueue.First();
        }

        // 完整发送了该数据
        if (ba.readIdx + count == ba.writeIdx)
        {
            lock (sendQueue)
            {
                sendQueue.Dequeue();
                ba = sendQueue.First();
            }
        }

        if (ba != null) // 居然没发送成功，或者队列里居然还有数据，则继续发送
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.CurSize,
                0, SendCallback, socket);
        }
        else if (isClosing) //正在关闭
        {
            socket.Close();
        }
    }

    public static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;

            // 获取接收数据长度
            int count = socket.EndReceive(ar);
            readBuff.writeIdx += count;

            // 处理二进制消息
            OnReceiveData();

            // 继续接收数据
            if (readBuff.RemainSize < 8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.CurSize * 2);
            }
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx,
                    readBuff.RemainSize, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }

    // 处理readBuff的数据
    public static void OnReceiveData()
    {
        if (readBuff.CurSize <= 2)
        {
            return;
        }

        // 获取消息体长度
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (readBuff.CurSize < bodyLength)
        {
            return;
        }
        readBuff.readIdx += 2;

        // 解析协议名
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out int cnt);
        if (protoName == "")
        {
            Debug.Log("OnReceiveData MsgBase.DecodeName fail");
            return;
        }
        readBuff.readIdx += cnt;

        // 解析协议体
        int bodyCount = bodyLength - cnt;
        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();

        // 添加到消息队列
        lock (receiveQueue)
        {
            receiveQueue.Enqueue(msgBase);
            receiveQueueSize++;
        }

        // 继续读取消息
        if (readBuff.CurSize > 2)
        {
            OnReceiveData();
        }
    }

    // 不断处理接收消息队列里的消息，由GameMain.Update每帧调用
    public static void Update() 
    {
        MsgUpdate(); // 尝试接收消息
        PingUpdate(); // 发送 ping
    }

    public static void MsgUpdate()
    {
        if (receiveQueueSize == 0)
        {
            return;
        }

        // 一次 Update 最多接收 MAX_MESSAGE_FIRE 条消息
        for (int i = 0; i < MAX_MESSAGE_HANDLE; i++) 
        {
            // 获取第一条消息
            MsgBase msgBase = null;
            lock (receiveQueue)
            {
                if (receiveQueue.Count > 0)
                {
                    msgBase = receiveQueue.First();
                    receiveQueue.Dequeue();
                    receiveQueueSize--;
                }
            }
            // 调用该消息的回调
            if (msgBase != null)
            {
                InvokeMsgListener(msgBase.protoName, msgBase);
            }
            else // 没有消息则结束
            {
                break;
            }
        }
    }

    // 发送PING
    private static void PingUpdate()
    {
        if (!usePingPong)
        {
            return;
        }

        // 发送PING
        if (Time.time - lastPingTime > PING_INTERVAL)
        {
            MsgPing msgPing = new MsgPing();
            Send(msgPing);
            lastPingTime = Time.time;
        }

        // 检测PONG时间
        if (Time.time - lastPongTime > PING_INTERVAL * 4)
        {
            Close();
        }
    }

    // 监听PONG协议
    private static void OnMsgPong(MsgBase msgBase)
    {
        lastPongTime = Time.time;
    }
}
