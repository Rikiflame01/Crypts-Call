using UnityEngine;
using UnityEngine.Events;

public class DynamicFollow : MonoBehaviour
{
    [Header("Axis Movement Settings")]
    [Tooltip("Enable movement on the X axis.")]
    public bool enableX = true;

    [Tooltip("Enable movement on the Z axis.")]
    public bool enableZ = true;

    [Header("Target to Follow")]
    [Tooltip("The target the camera will follow.")]
    public Transform target;

    [Header("Event Listeners")]
    public UnityEvent onEnableX;
    public UnityEvent onEnableZ;

    [Tooltip("Speed of camera movement.")]
    public float followSpeed = 5f;

    private Vector3 offset;

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
