using System.Collections.Generic;
using UnityEngine;

public class NetControl : MonoBehaviour {
    public static List<GameObject> Players = new List<GameObject>();
    public string Ip;
    public int Port;
    private NetControl mInstance;
    public NetControl Instance
    {
        get
        {
            if (mInstance != null)
                return mInstance;
            else
            {
                var go = GameObject.Find("NetControl") ?? new GameObject("NetControl");
                var control = go.GetComponent<NetControl>() ?? go.AddComponent<NetControl>();
                mInstance = control;
                return mInstance;
            }
        }
    }

}
