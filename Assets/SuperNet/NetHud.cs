using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetHud : MonoBehaviour
{
    private string mIp = "127.0.0.1";
    void OnGUI()
    {
        if (GUILayout.Button("Create Host"))
        {
            NetClient.Instance.CreateClient();
            NetServer.Instance.CreateServer(null);
            NetClient.Instance.Connect(NetServer.Instance.Ip);
        }

        if (GUILayout.Button("Create Server"))
        {
            NetServer.Instance.CreateServer(null);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Connect Client"))
        {
            NetClient.Instance.CreateClient();
            NetClient.Instance.Connect(new IPEndPoint(IPAddress.Parse(mIp),NetConst.Port));
        }
        mIp = GUILayout.TextField(mIp);
        GUILayout.EndHorizontal();
    }

}
