using System.Collections.Generic;
using UnityEngine;

public class SceneStateManager : MonoBehaviour
{
    private Dictionary<string, Dictionary<string, SavedObjectState>> sceneStates
        = new Dictionary<string, Dictionary<string, SavedObjectState>>();

    public static SceneStateManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSceneState(string sceneName, Dictionary<string, SavedObjectState> objectsState)
    {
        sceneStates[sceneName] = objectsState;
    }

    public Dictionary<string, SavedObjectState> GetSceneState(string sceneName)
    {
        if (sceneStates.TryGetValue(sceneName, out var savedStates))
        {
            return savedStates;
        }
        return new Dictionary<string, SavedObjectState>();
    }

    public void SaveSingleObject(string sceneName, string objectID, SavedObjectState state)
    {
        if (!sceneStates.ContainsKey(sceneName))
        {
            sceneStates[sceneName] = new Dictionary<string, SavedObjectState>();
        }

        sceneStates[sceneName][objectID] = state;

    }

}

public class SavedObjectState
{
    public Vector3 Position;
    public Quaternion Rotation;
    public bool IsDestroyed;

}
