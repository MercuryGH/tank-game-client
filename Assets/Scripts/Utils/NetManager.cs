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

    private static Queue<ByteBuffer> writeQueue; // 写入队列

    private static bool isConnecting = false;
    private static bool isClosing = false;

    private static List<MsgBase> msgList = new List<MsgBase>();
    private static int msgCount = 0;
    private const int MAX_MESSAGE_FIRE = 10;

    public static bool usePingPong = true;
    public static int pingInterval = 30;
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
        writeQueue = new Queue<ByteBuffer>();
        isConnecting = false;
        isClosing = false;
        msgList = new List<MsgBase>();
        msgCount = 0;
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

        if (writeQueue.Count > 0)
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

        lock (writeQueue) // 上锁
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }

        // send
        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length,
                0, SendCallback, socket);
        }
    }

    public static void SendCallback(IAsyncResult ar)
    {
        // 获取state、EndSend的处理
        Socket socket = (Socket)ar.AsyncState;
        if (socket == null || !socket.Connected)
        {
            return;
        }

        //EndSend
        int count = socket.EndSend(ar);

        // 获取写入队列第一条数据
        ByteBuffer ba;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }

        // 完整发送
        ba.readIdx += count;
        if (ba.CurSize == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                ba = writeQueue.First();
            }
        }

        if (ba != null) // 继续发送
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
            //获取接收数据长度
            int count = socket.EndReceive(ar);
            readBuff.writeIdx += count;
            //处理二进制消息
            OnReceiveData();
            //继续接收数据
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

    //数据处理
    public static void OnReceiveData()
    {
        //消息长度
        if (readBuff.CurSize <= 2)
        {
            return;
        }
        //获取消息体长度
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (readBuff.CurSize < bodyLength)
            return;
        readBuff.readIdx += 2;
        //解析协议名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
        if (protoName == "")
        {
            Debug.Log("OnReceiveData MsgBase.DecodeName fail");
            return;
        }
        readBuff.readIdx += nameCount;
        //解析协议体
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        //添加到消息队列
        lock (msgList)
        {
            msgList.Add(msgBase);
            msgCount++;
        }
        //继续读取消息
        if (readBuff.CurSize > 2)
        {
            OnReceiveData();
        }
    }

    public static void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

    public static void MsgUpdate()
    {
        if (msgCount == 0)
        {
            return;
        }

        // 重复处理消息
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            //获取第一条消息
            MsgBase msgBase = null;
            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            //分发消息
            if (msgBase != null)
            {
                InvokeMsgListener(msgBase.protoName, msgBase);
            }
            //没有消息了
            else
            {
                break;
            }
        }
    }

    //发送PING协议
    private static void PingUpdate()
    {
        if (!usePingPong)
        {
            return;
        }

        // 发送PING
        if (Time.time - lastPingTime > pingInterval)
        {
            MsgPing msgPing = new MsgPing();
            Send(msgPing);
            lastPingTime = Time.time;
        }

        // 检测PONG时间
        if (Time.time - lastPongTime > pingInterval * 4)
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
