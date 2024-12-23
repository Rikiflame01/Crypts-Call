using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LockToTarget : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The Transform to which the sword will be locked.")]
    public Transform target;

    [Header("Lock Settings")]
    [Tooltip("Lock the sword's position to the target.")]
    public bool lockPosition = true;
    [Tooltip("Lock the sword's rotation to the target.")]
    public bool lockRotation = true;

    [Header("Smoothing Options")]
    [Tooltip("Enable to smoothly interpolate to the target's position and rotation.")]
    public bool enableSmoothing = false;
    [Tooltip("Speed of the position smoothing.")]
    public float positionSmoothingSpeed = 20f;
    [Tooltip("Speed of the rotation smoothing.")]
    public float rotationSmoothingSpeed = 20f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("LockToTarget requires a Rigidbody component.");
            return;
        }

        rb.isKinematic = false;

        if (target == null)
        {
            Debug.LogError("LockToTarget script requires a target Transform to lock to.");
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        if (lockPosition)
        {
            if (enableSmoothing)
            {
                Vector3 newPosition = Vector3.Lerp(transform.position, target.position, Time.deltaTime * positionSmoothingSpeed);
                transform.position = newPosition;
            }
            else
            {
                transform.position = target.position;
            }
        }

        if (lockRotation)
        {
            if (enableSmoothing)
            {
                Quaternion newRotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * rotationSmoothingSpeed);
                transform.rotation = newRotation;
            }
            else
            {
                transform.rotation = target.rotation;
            }
        }
    }

    public void SetNewTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
