using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class SceneObjectState : MonoBehaviour
{
    [SerializeField]
    private string uniqueID;

    private bool isDestroyed = false;

    public string UniqueID => uniqueID;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = Guid.NewGuid().ToString();
        }
    }
    public SavedObjectState GetCurrentState()
        {
            return new SavedObjectState
            {
                Position = transform.position,
                Rotation = transform.rotation,
                IsDestroyed = isDestroyed
            };
        }
    public void MarkDestroyedAndSave(string Identity)
    {
        isDestroyed = true;

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        var myState = new SavedObjectState
        {
            Position = transform.position,
            Rotation = transform.rotation,
            IsDestroyed = isDestroyed
        };
        SceneStateManager.Instance.SaveSingleObject(sceneName, uniqueID, myState);

        Destroy(gameObject);
    }

    private void OnEnable()
    {
        RestoreState();
    }

    private void RestoreState()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        var savedStates = SceneStateManager.Instance.GetSceneState(sceneName);


        if (savedStates.TryGetValue(uniqueID, out SavedObjectState savedState))
        {
            ApplyState(savedState);
        }
        else
        {
            Debug.Log($"[RestoreState] No entry found for ID {uniqueID} in savedStates");
        }
    }


    private void ApplyState(SavedObjectState state)
    {
        transform.position = state.Position;
        transform.rotation = state.Rotation;
        isDestroyed = state.IsDestroyed;

        if (isDestroyed)
        {
            Destroy(gameObject);
        }
    }

}

