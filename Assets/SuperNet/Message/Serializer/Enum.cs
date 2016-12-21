using System;

namespace SWX
{
    public enum SerializerType
    {
        All = 1 << 1,
        WithContract = 1 << 2,
    }
    public class SerializerTypeAttribute : Attribute
    {
        public SerializerType Type;

        public SerializerTypeAttribute(SerializerType type)
        {
            Type = type;
        }
    }

    public class SerializerContract : Attribute
    {
        public int Tag;

        public SerializerContract(int tag)
        {
            Tag = tag;
        }
    }
}
