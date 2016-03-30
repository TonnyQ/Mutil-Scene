using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {    
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<T>();
                    singleton.name = typeof(T).ToString();

                    DontDestroyOnLoad(singleton);
                }

                return _instance;
            }
        }
    }

    private static bool applicationIsQuitting = false;
   
    public void OnDestroy()
    {
        applicationIsQuitting = true;
    }
}
