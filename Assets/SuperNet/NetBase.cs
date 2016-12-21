using System;
using System.Collections.Generic;
using UnityEngine;

public class NetBase<T> : ManagerController<T> where T : new()
{
    protected Dictionary<ushort, Action<ReaderMsg>> Dict = new Dictionary<ushort, Action<ReaderMsg>>();
    public virtual void RegisterReceive(ushort code, Action<ReaderMsg> msg)
    {
        if(msg == null)
            Debug.LogError("注册的协议号为:" + code + "读取字段不能为空");
        if (Dict.ContainsKey(code))
        {
            Debug.LogError("注册的协议号为:" + code + "已经存在读取委托.重写该委托");
            Dict.Remove(code);
        }
        Dict.Add(code,msg);
    }

    public virtual void UnRegisterReceive(ushort code)
    {
        if (Dict.ContainsKey(code))
        {
            Dict.Remove(code);
        }
    }

    protected void ReadHeader(ReaderMsg reader)
    {
        var code = reader.ReadUShort();
        Dict[code](reader);
    }
}
