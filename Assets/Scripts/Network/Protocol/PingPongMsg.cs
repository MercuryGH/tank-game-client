public sealed class MsgPing : BaseMsg
{
    public MsgPing() { protoName = "MsgPing"; }
}

public sealed class MsgPong : BaseMsg
{
    public MsgPong() { protoName = "MsgPong"; }
}