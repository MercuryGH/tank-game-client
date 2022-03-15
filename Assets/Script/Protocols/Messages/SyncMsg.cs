//同步坦克信息
public sealed class MsgSyncTank : MsgBase
{
    public MsgSyncTank() { protoName = "MsgSyncTank"; }
    //位置、旋转、炮塔旋转
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
    public float ex = 0f;
    public float ey = 0f;
    public float ez = 0f;
    public float turretY = 0f;
    public float gunX = 0f;
    //服务端补充
    public string id = "";		//哪个坦克
}

//开火
public sealed class MsgFire : MsgBase
{
    public MsgFire() { protoName = "MsgFire"; }
    //炮弹初始位置、旋转
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
    public float ex = 0f;
    public float ey = 0f;
    public float ez = 0f;
    //服务端补充
    public string id = "";		//哪个坦克
}

//击中
public sealed class MsgHit : MsgBase
{
    public MsgHit() { protoName = "MsgHit"; }
    //击中谁
    public string targetId = "";
    //击中点	
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
    //服务端补充
    public string id = "";      //哪个坦克
    public int hp = 0;          //被击中坦克血量
    public int damage = 0;		//受到的伤害
}