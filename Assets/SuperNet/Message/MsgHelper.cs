using System;
using System.Reflection;
using SWX;

public class MsgHelper {
    protected SerializerType mType;
    protected class FieldStruct
    {
        public FieldInfo FieldInfo;
        public object Obj;
    }
    protected bool NotValueType(Type type)
    {
        return !type.IsValueType && type != typeof(string);
    }
}
