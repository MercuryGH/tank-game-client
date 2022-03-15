public sealed class MsgRegister : MsgBase
{
    public MsgRegister() { protoName = "MsgRegister"; }
    //客户端发
    public string id = "";
    public string pw = "";
    //服务端回（0-成功，1-失败）
    public int result = 0;
}

public sealed class MsgLogin : MsgBase
{
    public MsgLogin() { protoName = "MsgLogin"; }
    //客户端发
    public string id = "";
    public string pw = "";
    //服务端回（0-成功，1-失败）
    public int result = 0;
}

public sealed class MsgKick : MsgBase
{
    public MsgKick() { protoName = "MsgKick"; }
    //原因（0-其他人登陆同一账号）
    public int reason = 0;
}