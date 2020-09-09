using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletoneGeneric<T> where T : class , new ()
{
    private static object lockobj = new object();
    private static T m_Instance;

    public static T Instance
    {
        get
        {
            lock (lockobj)
            {
                if (m_Instance == null)
                {
                    m_Instance = new T();
                }
                
            }
#if UNITY_EDITOR
            //CreateInstanceObject();
#endif
            return m_Instance;  
        }
    }

#if UNITY_EDITOR
    //public class SingletoneGenerator : UnityEditor.EditorWindow
    //{
    //    [UnityEditor.MenuItem("RoomEscape/Singletone Generator ", false,100)]
    //    public static void CreateWindow()
    //    {
    //        UnityEditor.EditorWindow.GetWindow<SingletoneGenerator>();
    //    }

    //    static List<string> addedList = new List<string>();
    //    public static void AddSingletone(string _name)
    //    {
    //        addedList.Add(_name);
    //    }
    //    void OnEnable()
    //    {
    //    }
    //    private void OnGUI()
    //    {
    //        for (int i = 0; i < addedList.Count; i++)
    //        {
    //            GUILayout.Label(addedList[i]);
    //        }
    //    }
    //}
    //private static GameObject g_goInstanceObject;
    //private static void CreateInstanceObject()
    //{
    //    if (!Application.isPlaying)
    //        return;
    //    //SingletoneGenerator.AddSingletone(typeof(T).FullName);
    //    //return;
    //    //if (g_goInstanceObject == null)
    //    //{
            
    //    //    g_goInstanceObject = new GameObject(typeof(T).FullName);
    //    //    GameObject.DontDestroyOnLoad(g_goInstanceObject);
    //    //}
    //}
#endif
}
