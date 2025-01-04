#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;

public class EnemyManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyData
    {
        [Tooltip("Unique identifier for this enemy. If empty, one will be generated automatically.")]
        public string UniqueID;

        [Tooltip("Reference to the enemy GameObject in the scene.")]
        public GameObject EnemyObject;
    }

    [Header("All Enemies In Scene")]
    [Tooltip("Populate this array with the known enemies, ensuring each has a unique ID.")]
    public EnemyData[] enemies;

    private void OnValidate()
    {
        bool madeChanges = false;
        foreach (var enemyData in enemies)
        {
            if (enemyData != null && string.IsNullOrEmpty(enemyData.UniqueID))
            {
                enemyData.UniqueID = Guid.NewGuid().ToString();
                Debug.Log($"[EnemyManager] Generated new UniqueID: {enemyData.UniqueID}", this);
                madeChanges = true;
            }
        }

#if UNITY_EDITOR
        if (madeChanges)
        {
            EditorUtility.SetDirty(this);
        }
#endif
    }

    private void Awake()
    {

        foreach (var enemyData in enemies)
        {
            if (enemyData.EnemyObject == null)
            {
                var allSceneObjects = FindObjectsByType<SceneObjectState>(FindObjectsSortMode.None);

                foreach (var state in allSceneObjects)
                {
                    if (state.UniqueID == enemyData.UniqueID)
                    {
                        enemyData.EnemyObject = state.gameObject;
                        break;
                    }
                }
            }

            if (enemyData.EnemyObject == null)
            {
                Debug.LogWarning(
                    $"[EnemyManager] Enemy with UniqueID: {enemyData.UniqueID} not found in scene."
                );
                continue;
            }

            int isDead = PlayerPrefs.GetInt(enemyData.UniqueID, 0); // Default 0 (alive)
            if (isDead == 1)
            {
                enemyData.EnemyObject.SetActive(false);
            }
            else
            {
                Health health = enemyData.EnemyObject.GetComponent<Health>();
                if (health != null)
                {
                    health.OnDied += (go) => OnEnemyDied(go, enemyData.UniqueID);
                }
                else
                {
                    Debug.LogWarning(
                        $"[EnemyManager] {enemyData.EnemyObject.name} has no Health component."
                    );
                }
            }
        }
    }

    private void OnEnemyDied(GameObject enemy, string uniqueID)
    {
        PlayerPrefs.SetInt(uniqueID, 1); // 1 = Dead
        PlayerPrefs.Save();

    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
