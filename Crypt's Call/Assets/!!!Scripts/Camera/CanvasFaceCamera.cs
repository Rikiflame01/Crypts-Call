using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasFaceCamera : MonoBehaviour
{
    private Camera targetCamera;

    void Awake()
    {
        targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogWarning("No Main Camera found in the scene. Please assign a Camera with the 'MainCamera' tag.");
        }
    }

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward,
            targetCamera.transform.rotation * Vector3.up);
        }
    }
}
