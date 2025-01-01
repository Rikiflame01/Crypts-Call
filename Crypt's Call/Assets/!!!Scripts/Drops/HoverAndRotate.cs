using UnityEngine;

public class HoverAndRotate : MonoBehaviour
{
    // Hover parameters
    [Header("Hover Settings")]
    public float hoverHeight = 0.5f;      
    public float hoverSpeed = 2f;         
    public float hoverAmplitude = 0.1f;  

    // Rotation parameters
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 90f;         

    // Floor detection parameters
    [Header("Floor Detection Settings")]
    public LayerMask floorLayers;         // Layers considered as floor
    public float detectionDistance = 0.1f;
    public Vector3 rayOriginOffset = Vector3.zero;

    // Internal variables
    private Vector3 initialPosition;
    private float currentHoverHeight;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        DetectFloorAndAdjustHover();
        Hover();
        Rotate();
    }

    void DetectFloorAndAdjustHover()
    {
        Vector3 rayOrigin = transform.position + rayOriginOffset;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, detectionDistance, floorLayers))
        {
            float distanceToFloor = hit.distance;

            currentHoverHeight = Mathf.Max(hoverHeight, distanceToFloor);

            Debug.DrawRay(rayOrigin, Vector3.down * hit.distance, Color.green);
        }
        else
        {
            currentHoverHeight = hoverHeight;

            Debug.DrawRay(rayOrigin, Vector3.down * detectionDistance, Color.red);
        }
    }

    void Hover()
    {
        float newY = initialPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        Vector3 newPosition = new Vector3(initialPosition.x, newY + currentHoverHeight, initialPosition.z);
        transform.localPosition = newPosition;
    }

    void Rotate()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 rayOrigin = transform.position + rayOriginOffset;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * detectionDistance);
    }
}
