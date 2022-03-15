// 查询玩家个人信息
public sealed class MsgGetAchieve : MsgBase
{
    public MsgGetAchieve() { protoName = "MsgGetAchieve"; }

    // query with no request

    // response
    public int win = 0;
    public int lost = 0;
}

// 房间信息数据结构
[System.Serializable]
public sealed class RoomInfo
{
    // response
    public int id = 0;      // 房间id
    public int count = 0;   // 人数
    public int status = 0;	// 状态 0-准备中 1-战斗中
}

// 查询房间列表
public sealed class MsgGetRoomList : MsgBase
{
    public MsgGetRoomList() { protoName = "MsgGetRoomList"; }

    // response
    public RoomInfo[] rooms;
}

// 创建房间
public sealed class MsgCreateRoom : MsgBase
{
    public MsgCreateRoom() { protoName = "MsgCreateRoom"; }

    // response status code
    public int result = 0;
}

// 进入房间
public sealed class MsgEnterRoom : MsgBase
{
    public MsgEnterRoom() { protoName = "MsgEnterRoom"; }

    // request
    public int id = 0;
    // response
    public int result = 0;
}