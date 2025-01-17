using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private GameObject bloodVFXPrefab;
    [SerializeField] private float vfxSpawnHeight = 1.0f;

    private void Start()
    {
        SubscribeToAllHealthComponents();
    }

    private void SubscribeToAllHealthComponents()
    {
        Health[] healthComponents = Object.FindObjectsByType<Health>(FindObjectsSortMode.None);
        foreach (var health in healthComponents)
        {
            health.OnDied += HandleOnDied;
        }
    }

    public void RegisterNewEnemy(GameObject enemy)
    {
        Health health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.OnDied += HandleOnDied;
        }
        else
        {
            Debug.LogWarning($"The spawned enemy {enemy.name} does not have a Health component.");
        }
    }

    private void OnDestroy()
    {
        Health[] healthComponents = Object.FindObjectsByType<Health>(FindObjectsSortMode.None);
        foreach (var health in healthComponents)
        {
            health.OnDied -= HandleOnDied;
        }
    }

    private void HandleOnDied(GameObject diedObject)
    {
        if (bloodVFXPrefab == null)
        {
            Debug.LogError("Blood VFX Prefab is not assigned in VFXManager!");
            return;
        }

        Vector3 spawnPosition = diedObject.transform.position + Vector3.up * vfxSpawnHeight;

        Instantiate(bloodVFXPrefab, spawnPosition, Quaternion.identity);
    }
}
