using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SWX;

public class ReaderMsg :MsgHelper
{
    private byte[] mBytes;
    private ReadHelper mReadHelper;

    public ReaderMsg(byte[] bytes)
    {
        mBytes = bytes;
        mReadHelper = new ReadHelper(mBytes);
    }

    private void SetPropValue(object obj, string propName)
    {
        string[] nameParts = propName.Split('.');
        if (nameParts.Length == 1)
        {
            var field = obj.GetType().GetProperty(propName);
            field.SetValue(obj, mReadHelper.Read(field.PropertyType), null);
            return;
        }
        PropertyInfo info = null;

        foreach (string part in nameParts)
        {
            if (obj == null) { return; }
            Type type = obj.GetType();
            info = type.GetProperty(part);
            if (info == null) { return; }
            if (info.PropertyType.IsClass && info.PropertyType != typeof(String))
                obj = info.GetValue(obj, null);
        }
        if (info != null) info.SetValue(obj, mReadHelper.Read(info.PropertyType), null);
    }

    private void SetFieldValue(object obj, string propName)
    {
        string[] nameParts = propName.Split('.');
        if (nameParts.Length == 1)
        {
            var field = obj.GetType().GetField(propName);
            if (field.FieldType.IsGenericType)
            {
                //IDictionary dictionary = null;
                if (field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    object value = mReadHelper.Read(field.FieldType);
                    IList list = value as IList;
                    int count = (int)mReadHelper.Read(typeof(int));
                    Type args = value.GetType().GetGenericArguments()[0];
                    if (NotValueType(args))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var test = ReadMsg(args);
                            list.Add(test);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            list.Add(mReadHelper.Read(args));
                        }
                    }
                    field.SetValue(obj, value);
                    return;
                }
                else if (field.GetValue(obj) is IDictionary)
                {
                    object dict = mReadHelper.Read(field.FieldType);
                    IDictionary list = dict as IDictionary;
                    int count = (int)mReadHelper.Read(typeof(int));
                    Type args = dict.GetType().GetGenericArguments()[0];
                    Type args2 = dict.GetType().GetGenericArguments()[1];
                    object key, value;
                    for (int i = 0; i < count; i++)
                    {
                        key = NotValueType(args) ? ReadMsg(args) : mReadHelper.Read(args);
                        value = NotValueType(args2) ? ReadMsg(args2) : mReadHelper.Read(args2);
                        list.Add(key, value);
                    }
                    field.SetValue(obj, dict);
                    return;
                }
            }
            else if (field.FieldType.IsArray)
            {
                int count = (int)mReadHelper.Read(typeof(int));
                Type args = field.FieldType.GetElementType();
                Array list = Array.CreateInstance(args, count);
                if (NotValueType(args))
                {
                    for (int i = 0; i < count; i++)
                    {
                        var test = ReadMsg(args);
                        list.SetValue(test, i);
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        list.SetValue(mReadHelper.Read(args), i);
                    }
                }
                field.SetValue(obj, list);
                return;
            }
            field.SetValue(obj, mReadHelper.Read(field.FieldType));
            return;
        }
        FieldInfo info = null;
        Type type = null;
        foreach (string part in nameParts)
        {
            if (obj == null) { return; }
            type = obj.GetType();
            info = type.GetField(part);
            if (info == null) { return; }
            if (info.FieldType.IsClass && info.FieldType != typeof(String))
                obj = info.GetValue(obj);
        }
        if (info != null)
        {
            info.SetValue(obj, mReadHelper.Read(info.FieldType));
        }
    }

    private int ReadLength()
    {
        return (int)mReadHelper.Read(typeof(int));
    }


    public T Read<T>()
    {
        var newT = Activator.CreateInstance<T>();
        var length = ReadLength();
        SerializerType serType = (SerializerType)mReadHelper.ReadByte();
        string varName;
        long pos = mReadHelper.Position - 1;
        if (serType == SerializerType.All)
        {
            for (long i = 0; i < length;)
            {
                varName = (string)mReadHelper.Read(typeof(string));
                SetFieldValue(newT, varName);
                i = mReadHelper.Position - pos;
            }
        }
        return newT;
    }

    public T ReadMsg<T>() where T : NetMsg
    {
        var type = typeof (T);
        var newT = Activator.CreateInstance<T>();
        var derMethod = type.GetMethod("Derializer");
        if (derMethod.DeclaringType == type)
            newT.Derializer(this);
        else
            Read<T>();
        return newT;
    }
    public object ReadMsg(Type type)
    {
        long length = ReadLength();
        SerializerType serType = (SerializerType)mReadHelper.ReadByte();
        var newT = Activator.CreateInstance(type);
        string varName;
        long pos = mReadHelper.Position - 1;
        for (long i = 0; i < length;)
        {
            varName = (string)mReadHelper.Read(typeof(string));
            SetFieldValue(newT, varName);
            i = mReadHelper.Position - pos;
        }
        return newT;
    }

    #region Read
    public float ReadFloat()
    {
        return mReadHelper.ReadFloat();
    }

    public short ReadShort()
    {
        return mReadHelper.ReadShort();
    }

    public uint ReadUint()
    {
        return mReadHelper.ReadUint();
    }

    public ulong ReadUlong()
    {
        return mReadHelper.ReadUlong();
    }

    public ushort ReadUShort()
    {
        return mReadHelper.ReadUShort();
    }

    public int ReadInt()
    {
        return mReadHelper.ReadInt();
    }

    public string ReadString()
    {
        return mReadHelper.ReadString();
    }

    public long ReadLong()
    {
        return mReadHelper.ReadLong();
    }

    public bool ReadBool()
    {
        return mReadHelper.ReadByte() == 1;
    }

    public byte ReadByte()
    {
        return mReadHelper.ReadByte();
    }

    public double ReadDouble()
    {
        return mReadHelper.ReadDouble();
    }
    #endregion
}
