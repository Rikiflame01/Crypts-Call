using UnityEngine;
using UnityEngine.Events;

public class DynamicFollow : MonoBehaviour
{
    [Header("Axis Movement Settings")]
    public bool enableX = true;
    public bool enableZ = true;

    [Header("Target to Follow")]
    public Transform target;

    [Header("Event Listeners")]
    public UnityEvent onEnableX;
    public UnityEvent onEnableZ;

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

    private Vector3 offset;
    private Renderer lastHiddenRenderer;
    private Material originalMaterial;
    private Material transparentMaterial;

    void Start()
    {
        if (target != null)
        {
            offset = transform.position - target.position;
        }
        else
        {
            Debug.LogError("CameraFollowAxis: No target assigned!");
        }
    }

    void Update()
    {
        if (target == null)
            return;

        FollowTarget();
        HandleObstructions();
    }

    void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset;

        if (!enableX)
            desiredPosition.x = transform.position.x;

        if (!enableZ)
            desiredPosition.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
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
}
