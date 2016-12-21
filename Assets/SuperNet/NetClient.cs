using System;
using System.Net;
using System.Net.Sockets;

public class NetClient :NetBase<NetClient> {
    internal UdpClient Client;
    public void Send(ushort sign, object msg)
    {
        var write = new WriterMsg(sign);
        write.Write(msg);
        var bytes = write.GetBytes();
        if (Client != null)
        {
            if(Client.Client.Connected)
                Client.Send(bytes, bytes.Length);
        }
    }
    public void CreateClient()
    {
        Client = new UdpClient(NetConst.Port);
        Client.BeginReceive(Callback, null);
    }

    private void Callback(IAsyncResult ar)
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, NetConst.Port);
        byte[] receiveBytes = Client.EndReceive(ar, ref ip);
        var reader = new ReaderMsg(receiveBytes);
        ReadHeader(reader);
        Client.BeginReceive(Callback, null);
    }

    public void Connect(IPEndPoint ip)
    {
        Client.Connect(ip);
    }
}
