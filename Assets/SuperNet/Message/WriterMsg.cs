using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SWX;
using UnityEngine;

public class WriterMsg :MsgHelper
{
    private WriteHelper mWrite;
    private WriteHelper mWriteHelper
    {
        set { mWrite = value; }
        get { return mWrite ?? (mWrite = new WriteHelper()); }
    }

    public WriterMsg(ushort code)
    {
        Write(code);
    }

    private FieldStruct GetField(object obj, string propName)
    {
        string[] nameParts = propName.Split('.');
        if (nameParts.Length == 1)
        {
            var field = obj.GetType().GetField(propName);
            return new FieldStruct { FieldInfo = field, Obj = obj };
        }
        FieldInfo info = null;
        foreach (string part in nameParts)
        {
            if (obj == null) { return null; }

            Type type = obj.GetType();
            info = type.GetField(part);
            if (info == null) { return null; }
            if (info.FieldType.IsClass && info.FieldType != typeof(String))
                obj = info.GetValue(obj);
        }
        return new FieldStruct { FieldInfo = info, Obj = obj };
    }

    public byte[] GetBytes()
    {
        return mWriteHelper.Bytes;
    }

    public void Write(object obj)
    {
        if (!mWriteHelper.Write(obj))
            WriteMsg(obj);
    }

    public void WriteMsg<T>(T obj) where T : NetMsg
    {
        var type = obj.GetType();
        var serMethod = type.GetMethod("Serializer");
        if(serMethod.DeclaringType == type)
            obj.Serializer(this);
        else
            WriteMsg(obj);
    }

    private void WriteMsg(object obj)
    {
        Serializer(obj);
    }

    private void Serializer(object o, SerializerType type = SerializerType.All)
    {
        using (var stream = new MemoryStream())
        {
            var write = mWriteHelper;
            mWriteHelper = new WriteHelper();
            StartWrite(o, type);
            write.Write(mWriteHelper.Length);
            write.Write(mWriteHelper.Bytes);
            mWriteHelper = write;
        }
    }

    private void StartWrite(object o, SerializerType type = SerializerType.All)
    {
        mWriteHelper.Write((byte)type);
        mType = type;
        Serializer(o, "");
    }

    private void Serializer(object o, string nested)
    {
        var oType = o.GetType();
        var fields = oType.GetFields();
        if (mType == SerializerType.All)
        {
            int length = fields.Length;
            for (int i = 0; i < length; i++)
            {
                var field = fields[i];
                if (field.FieldType.IsGenericType)
                {
                    IDictionary dictionary = null;
                    var fieldStruct = GetField(o, nested + field.Name);
                    if (field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var list = fieldStruct.FieldInfo.GetValue(fieldStruct.Obj) as IList;
                        mWriteHelper.Write(nested + field.Name);
                        mWriteHelper.Write(list.Count);
                        var type = list.GetType().GetGenericArguments()[0];
                        if (NotValueType(type))
                        {
                            for (int z = 0; z < list.Count; z++)
                            {
                                Serializer(list[z]);
                            }
                        }
                        else
                        {
                            for (int z = 0; z < list.Count; z++)
                            {
                                mWriteHelper.Write(list[z]);
                            }
                        }
                        continue;
                    }
                    else if ((dictionary = fieldStruct.FieldInfo.GetValue(fieldStruct.Obj) as IDictionary) != null)
                    {
                        mWriteHelper.Write(nested + field.Name);
                        mWriteHelper.Write(dictionary.Count);
                        foreach (var node in dictionary.Keys)
                        {
                            var key = dictionary.GetType().GetGenericArguments()[0];
                            var value = dictionary.GetType().GetGenericArguments()[1];
                            if (NotValueType(key))
                                Serializer(node);
                            else
                                mWriteHelper.Write(node);
                            if (NotValueType(value))
                                Serializer(dictionary[node]);
                            else
                                mWriteHelper.Write(dictionary[node]);
                        }
                        continue;
                    }
                }
                else if (field.FieldType.IsArray)
                {
                    var arrayType = field.FieldType.GetElementType();
                    Array array = (Array)field.GetValue(o);
                    mWriteHelper.Write(nested + field.Name);
                    var count = array.Length;
                    mWriteHelper.Write(count);
                    if (NotValueType(arrayType))
                    {
                        for (int j = 0; j < count; j++)
                            Serializer(field.GetValue(o));
                    }
                    else
                    {
                        for (int j = 0; j < count; j++)
                            mWriteHelper.Write(array.GetValue(j));
                    }
                    continue;
                }
                else if (field.FieldType.IsClass && field.FieldType != typeof(String))
                {
                    Serializer(field.GetValue(o), field.Name + ".");
                    continue;
                }

                mWriteHelper.Write(nested + field.Name);
                mWriteHelper.Write(field.GetValue(o));
            }
        }
    }
}
