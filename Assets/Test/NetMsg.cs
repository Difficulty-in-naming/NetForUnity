public abstract class NetMsg
{
    public abstract ushort MsgCode();
    public virtual void Serializer(WriterMsg writer) { }
    public virtual void Derializer(ReaderMsg reader) { }
}
