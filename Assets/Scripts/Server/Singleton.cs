using UnityEngine;

public class Singleton2<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                T obj;
                obj = FindObjectOfType<T>();
                if (obj == null)
                {
                    var gameObj = new GameObject(typeof(T).Name);
                    _instance = gameObj.AddComponent<T>();
                }
                else
                {
                    _instance = obj;
                }
            }
            return _instance;
        }
    }

    public static bool instanceExists = false;

    public virtual void Awake()
    {
        instanceExists = true;
        DontDestroyOnLoad(gameObject);
    }
}

