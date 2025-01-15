using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScreenShake script to be attached to the Main Camera.
/// It listens for health changes in all enemies and players and triggers a screen shake effect.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    [Header("Screen Shake Settings")]
    [Tooltip("Duration of the screen shake in seconds.")]
    public float shakeDuration = 0.5f;

    [Tooltip("Amplitude of the shake. Higher values mean more intense shake.")]
    public float shakeAmplitude = 0.3f;

    [Tooltip("Frequency of the shake. Higher values mean faster shake.")]
    public float shakeFrequency = 25f;

    [Header("Layer Settings")]
    [Tooltip("Layers to monitor for health changes.")]
    public string[] targetLayers = { "Enemy", "Player" };

    private float currentShakeDuration = 0f;
    private Vector3 initialPosition;
    private bool isShaking = false;

    private List<IHealth> subscribedHealthComponents = new List<IHealth>();

    [Tooltip("Time interval to scan for new enemies or players.")]
    public float scanInterval = 1f;

    private void Start()
    {
        initialPosition = transform.localPosition;
        ScanForHealthComponents();
        StartCoroutine(PeriodicScan());
    }

    private void ScanForHealthComponents()
    {
        foreach (string layerName in targetLayers)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
            {
                Debug.LogWarning($"Layer '{layerName}' does not exist. Please add it to the project layers.");
                continue;
            }

            GameObject[] objectsInLayer = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in objectsInLayer)
            {
                if (obj.layer != layer)
                    continue;

                IHealth healthComponent = obj.GetComponent<IHealth>();
                if (healthComponent != null && !subscribedHealthComponents.Contains(healthComponent))
                {
                    SubscribeToHealthEvents(healthComponent);
                    subscribedHealthComponents.Add(healthComponent);
                }
            }
        }
    }

    private void SubscribeToHealthEvents(IHealth health)
    {
        health.OnHealthChanged += HandleHealthChanged;
        health.OnDied += HandleEntityDeath;
    }

    private void UnsubscribeFromHealthEvents(IHealth health)
    {
        health.OnHealthChanged -= HandleHealthChanged;
        health.OnDied -= HandleEntityDeath;
    }

    private IEnumerator PeriodicScan()
    {
        while (true)
        {
            ScanForHealthComponents();
            yield return new WaitForSeconds(scanInterval);
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        TriggerShake();
    }

    private void HandleEntityDeath(GameObject deadEntity)
    {
        IHealth healthComponent = deadEntity.GetComponent<IHealth>();
        if (healthComponent != null)
        {
            UnsubscribeFromHealthEvents(healthComponent);
            subscribedHealthComponents.Remove(healthComponent);
        }
    }

    private void TriggerShake()
    {
        currentShakeDuration = shakeDuration;
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine());
        }
    }

    private IEnumerator ShakeCoroutine()
    {
        isShaking = true;
        while (currentShakeDuration > 0)
        {
            float xShake = UnityEngine.Random.Range(-1f, 1f) * shakeAmplitude;
            float yShake = UnityEngine.Random.Range(-1f, 1f) * shakeAmplitude;

            transform.localPosition = new Vector3(initialPosition.x + xShake, initialPosition.y + yShake, initialPosition.z);

            currentShakeDuration -= Time.deltaTime;

            yield return null;
        }

        transform.localPosition = initialPosition;
        isShaking = false;
    }

    private void OnDestroy()
    {
        foreach (IHealth health in subscribedHealthComponents)
        {
            if (health != null)
            {
                UnsubscribeFromHealthEvents(health);
            }
        }
    }
}
