using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private GameObject bloodVFXPrefab;
    [SerializeField] private GameObject stunVFXPrefab;
    [SerializeField] private GameObject poisonVFXPrefab;
    [SerializeField] private float vfxSpawnHeight = 1.0f;

    private void Start()
    {
        SubscribeToAllHealthComponents();
        EventManager.OnStunApplied += HandleOnStunApplied;
        EventManager.OnPoisonApplied += HandleOnPoisonApplied;
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

        EventManager.OnStunApplied -= HandleOnStunApplied;
        EventManager.OnPoisonApplied -= HandleOnPoisonApplied;
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

    private void HandleOnStunApplied(GameObject player)
    {
        if (stunVFXPrefab == null)
        {
            Debug.LogError("Stun VFX Prefab is not assigned in VFXManager!");
            return;
        }

        GameObject stunVFX = Instantiate(stunVFXPrefab, player.transform);
        stunVFX.transform.localPosition = Vector3.up * 1.5f;

        Destroy(stunVFX, 3f);
    }

    private void HandleOnPoisonApplied(GameObject player)
    {
        if (poisonVFXPrefab == null)
        {
            Debug.LogError("Poison VFX Prefab is not assigned in VFXManager!");
            return;
        }

        GameObject poisonVFX = Instantiate(poisonVFXPrefab, player.transform);
        poisonVFX.transform.localPosition = Vector3.up * -0.1f;

        Destroy(poisonVFX, 3f);
    }
}
