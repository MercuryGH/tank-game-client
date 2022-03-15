// 坦克信息
[System.Serializable]
public sealed class TankInfo
{
    public string id = "";  // 玩家id
    public int camp = 0;    // 阵营    TODO: modify name
    public int hp = 0;      // 生命值

    public float x = 0;     // 位置
    public float y = 0;
    public float z = 0;
    public float ex = 0;    // 旋转
    public float ey = 0;
    public float ez = 0;
}


// 进入战场
public sealed class MsgEnterBattle : MsgBase
{
    public MsgEnterBattle() { protoName = "MsgEnterBattle"; }

    // push
    public TankInfo[] tanks; // 初始化所有坦克的阵营、位置等
    public int mapId = 1;	 // 地图id
}

// 战斗结果
public sealed class MsgBattleResult : MsgBase
{
    public MsgBattleResult() { protoName = "MsgBattleResult"; }

    // push
    public int winCamp = 0;	 // 获胜的阵营
}

// 玩家退出
public sealed class MsgLeaveBattle : MsgBase
{
    public MsgLeaveBattle() { protoName = "MsgLeaveBattle"; }

    // push
    public string id = "";
}