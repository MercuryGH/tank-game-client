public sealed class MsgRegister : MsgBase
{
    public MsgRegister() { protoName = "MsgRegister"; }

    // request
    public string id = "";
    public string pw = "";

    // response status code（0-成功，1-失败）
    public int result = 0;
}

public sealed class MsgLogin : MsgBase
{
    public MsgLogin() { protoName = "MsgLogin"; }

    // request
    public string id = "";
    public string pw = "";

    // response status code （0-成功，1-失败）
    public int result = 0;
}

public sealed class MsgKick : MsgBase
{
    public MsgKick() { protoName = "MsgKick"; }

    // push 被踢原因（0-其他人登陆同一账号）
    public int reason = 0;
}