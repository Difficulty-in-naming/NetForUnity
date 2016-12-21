using UnityEngine;
public class ManagerController<T> where T : new(){
    private static T mInstance;
    public static T Instance { get { return mInstance != null ? mInstance : (mInstance = new T()); } }
}
