public sealed class MsgGetText : MsgBase
{
    public MsgGetText() { protoName = "MsgGetText"; }

    // push
    public string text = "";
}

public sealed class MsgSaveText : MsgBase
{
    public MsgSaveText() { protoName = "MsgSaveText"; }

    // request
    public string text = "";

    // response status code （0-成功 1-文字太长）
    public int result = 0;
}
