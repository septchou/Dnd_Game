using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Singleton<T> : MonoBehaviour where T : Component {
    private static T instance;
    protected static bool DontDestroy = false;

    static Singleton() {
        // Subscribe to sceneLoaded to reset instance on scene reload
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static T GetInstance() {
        if (instance == null) {
            instance = FindObjectOfType<T>();
            if (instance == null) {
                GameObject obj = new GameObject();
                obj.name = typeof(T).Name;
                instance = obj.AddComponent<T>();
            }
        }
        return instance;
    }

    protected virtual void Awake() {
        if (instance == null) {
            instance = this as T;
            if (DontDestroy) DontDestroyOnLoad(gameObject);
        } else if (instance != this as T) {
            Destroy(gameObject);
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Reset the instance to force reinitialization in the new scene
        instance = null;
    }
}