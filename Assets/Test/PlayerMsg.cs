public class PlayerMsg : NetMsg
{
    public static ushort Code = 1;
    public override ushort MsgCode()
    {
        return Code;
    }

    public string GameObjectName;
    public bool IsActive = false;

    public override void Serializer(WriterMsg writer)
    {
        writer.Write(GameObjectName);
        writer.Write(IsActive);
    }

    public override void Derializer(ReaderMsg reader)
    {
        GameObjectName = reader.ReadString();
        IsActive = reader.ReadBool();
    }
}
