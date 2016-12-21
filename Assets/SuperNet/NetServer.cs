using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class NetServer : NetBase<NetServer> {
    private List<string> mClientList = new List<string>();
    public IPEndPoint Ip;
    internal UdpClient Server;
    public void Send(ushort sign, object msg)
    {
        var write = new WriterMsg(sign);
        write.Write(msg);
        var bytes = write.GetBytes();
        if (Server != null)
        {
            for (int i = 0; i < mClientList.Count; i++)
            {
                Server.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Parse(mClientList[i]),NetConst.Port));
            }
        }
    }
    public void CreateServer(IPAddress address)
    {
        if (address == null)
            address = IPAddress.Parse("127.0.0.1");
        Ip = new IPEndPoint(address, NetConst.Port);
        Server = new UdpClient(Ip);
        Server.BeginReceive(Callback, null);
    }

    private void Callback(IAsyncResult ar)
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, NetConst.Port);
        byte[] receiveBytes = Server.EndReceive(ar,ref ip);
        var reader = new ReaderMsg(receiveBytes);
        ReadHeader(reader);
        Server.BeginReceive(Callback, null);
    }
}
