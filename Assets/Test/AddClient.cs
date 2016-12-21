public class AddClient : NetMsg {
    public override ushort MsgCode()
    {
        return 2;
    }

    public string Ip;
}
