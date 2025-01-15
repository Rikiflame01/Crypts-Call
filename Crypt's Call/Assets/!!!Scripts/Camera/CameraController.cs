using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Enable movement on the X axis.")]
    public bool enableX = true;

    [Tooltip("Enable movement on the Z axis.")]
    public bool enableZ = true;

    [Tooltip("Target to follow.")]
    public Transform target;

    [Tooltip("Speed of camera movement.")]
    public float followSpeed = 5f;

    [Header("Obstruction Settings")]
    [Tooltip("Layer mask for objects that can obstruct the view.")]
    public LayerMask obstructionLayer;

    [Tooltip("Enable object transparency.")]
    public bool enableObjectTransparency = true;

    [Tooltip("Transparency value for obstructing objects (0 = fully transparent, 1 = fully opaque).")]
    [Range(0.1f, 1f)]
    public float transparencyLevel = 0.3f;

    [Header("Screen Shake Settings")]
    [Tooltip("Duration of the screen shake in seconds.")]
    public float shakeDuration = 0.5f;

    [Tooltip("Amplitude of the shake. Higher values mean more intense shake.")]
    public float shakeAmplitude = 0.3f;

    [Tooltip("Frequency of the shake. Higher values mean faster shake.")]
    public float shakeFrequency = 25f;

    [Header("Layer Settings for Screen Shake")]
    [Tooltip("Layers to monitor for health changes.")]
    public string[] targetLayersForShake = { "Enemy", "Player" };

    [Tooltip("Time interval to scan for new enemies or players.")]
    public float scanInterval = 1f;

    [Header("Event Listeners")]
    public UnityEvent onEnableX;
    public UnityEvent onEnableZ;

    private Vector3 offset;
    private Renderer lastHiddenRenderer;
    private Material originalMaterial;
    private Material transparentMaterial;

    private float currentShakeDuration = 0f;
    private Vector3 shakeOffset = Vector3.zero;
    private bool isShaking = false;

    private List<IHealth> subscribedHealthComponents = new List<IHealth>();

    void Start()
    {
        if (target != null)
        {
            offset = transform.position - target.position;
        }
        else
        {
            Debug.LogError("CameraController: No target assigned!");
        }

        ScanForHealthComponents();
        StartCoroutine(PeriodicScan());
    }

    void Update()
    {
        if (target == null)
            return;

        FollowTarget();
        HandleObstructions();
        ApplyShakeOffset();
    }

    void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset;

        if (!enableX)
            desiredPosition.x = transform.position.x;

        if (!enableZ)
            desiredPosition.z = transform.position.z;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }

    void HandleObstructions()
    {
        if (!enableObjectTransparency || target == null)
            return;

        Vector3 directionToTarget = target.position - transform.position;

        if (Physics.Linecast(transform.position, target.position, out RaycastHit hit, obstructionLayer))
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer != lastHiddenRenderer)
            {
                RestoreLastObject();
                MakeObjectTransparent(renderer);
            }
        }
        else
        {
            RestoreLastObject();
        }
    }

    void MakeObjectTransparent(Renderer renderer)
    {
        if (renderer == null)
            return;

        originalMaterial = renderer.material;

        transparentMaterial = new Material(originalMaterial);

        transparentMaterial.SetFloat("_Surface", 1);
        transparentMaterial.SetFloat("_Blend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transparentMaterial.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transparentMaterial.SetFloat("_ZWrite", 0);
        transparentMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        transparentMaterial.renderQueue = 3000;

        Color color = transparentMaterial.color;
        color.a = transparencyLevel;
        transparentMaterial.color = color;

        renderer.material = transparentMaterial;
        lastHiddenRenderer = renderer;
    }

    void RestoreLastObject()
    {
        if (lastHiddenRenderer != null)
        {
            lastHiddenRenderer.material = originalMaterial;
            lastHiddenRenderer = null;
            originalMaterial = null;
        }
    }

    private void ScanForHealthComponents()
    {
        foreach (string layerName in targetLayersForShake)
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

            shakeOffset = new Vector3(xShake, yShake, 0f);

            currentShakeDuration -= Time.deltaTime;

            yield return null;
        }

        shakeOffset = Vector3.zero;
        isShaking = false;
    }

    private void ApplyShakeOffset()
    {
        transform.position += shakeOffset;
    }

    #region Event Listeners
    public void EnableXMovement()
    {
        enableX = true;
        onEnableX?.Invoke();
    }

    public void DisableXMovement()
    {
        enableX = false;
    }

    public void EnableZMovement()
    {
        enableZ = true;
        onEnableZ?.Invoke();
    }

    public void DisableZMovement()
    {
        enableZ = false;
    }
    #endregion

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
