using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletoneMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool g_bIsApplicationClose = false;
    public static bool IsApplicationClose { get { return g_bIsApplicationClose; } }
    private static object lockobj = new object();
    protected static T m_Instance;
    public static T Instance
    {
        get
        {
            if (g_bIsApplicationClose)
                return null;
            lock (lockobj)
            {

                CreateInstance();

            }           

            return m_Instance;
        }
    }

    protected static void CreateInstance()
    {        
        if (m_Instance == null)
        {
            Debug.Log("Instance : " + typeof(T).Name);
            T[] objs = FindObjectsOfType<T>();

            if (objs.Length > 0)
                m_Instance = objs[0];

            if (objs.Length > 1)
                Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");

            if (m_Instance == null)
            {
                string goName = typeof(T).ToString();
                GameObject go = GameObject.Find(goName);
                if (go == null)
                    go = new GameObject(goName);
                m_Instance = go.AddComponent<T>();
            }
        }
    }
    protected virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (m_Instance == null)
        {
            m_Instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnApplicationQuit()
    {
        g_bIsApplicationClose = true;
    }
}
