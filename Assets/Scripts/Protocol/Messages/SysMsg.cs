public sealed class MsgPing : MsgBase
{
    public MsgPing() { protoName = "MsgPing"; }
}

public sealed class MsgPong : MsgBase
{
    public MsgPong() { protoName = "MsgPong"; }
}