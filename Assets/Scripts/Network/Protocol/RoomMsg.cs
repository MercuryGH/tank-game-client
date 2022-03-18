// 玩家信息数据结构 TODO: modfiy field name
[System.Serializable]
public sealed class PlayerInfo
{
    public string id = "test";  // 账号
    public int team = 0;        // 阵营
    public int win = 0;         // 胜利数
    public int lose = 0;        // 失败数
    public int isOwner = 0;		// 是否是房主
}

// 获取当前所在的房间的玩家信息
// 除了客户端主动获取之外，服务器也会主动广播变化（如检测玩家逃跑）
public sealed class MsgGetRoomInfo : BaseMsg
{
    public MsgGetRoomInfo(int playerCnt)
    {
        protoName = "MsgGetRoomInfo";
        players = new PlayerInfo[playerCnt];
    }

    public MsgGetRoomInfo()
    {
        protoName = "MsgGetRoomInfo";
    }

    // response & push
    public PlayerInfo[] players;
}

// 自己离开房间（其他人离开房间，则会通过MsgGetRoomInfo广播的方式得到）
public sealed class MsgLeaveRoom : BaseMsg
{
    public MsgLeaveRoom() { protoName = "MsgLeaveRoom"; }

    // response status code
    public int result = 0;
}

// 开战
public sealed class MsgStartBattle : BaseMsg
{
    public MsgStartBattle() { protoName = "MsgStartBattle"; }

    // response status code: success: 0, other: 1
    public int result = 0;
}