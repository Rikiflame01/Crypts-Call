using UnityEngine;
using UnityEngine.InputSystem;

public class AimArrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    
    [Header("Settings")]
    [SerializeField] private float distanceFromPlayer = 2f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (!mainCamera)
        {
            Debug.LogWarning("No main camera found! Make sure to set this if not using Camera.main.");
        }
    }

    private void Update()
    {
        if (!mainCamera || playerTransform == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, playerTransform.position.y, 0));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector3 direction = (hitPoint - playerTransform.position).normalized;
            direction.y = 0f;

            transform.position = playerTransform.position + direction * distanceFromPlayer;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

                Vector3 euler = transform.eulerAngles;
                euler.x = 90f;
                euler.z = 0f; 
                transform.eulerAngles = euler;
            }
        }
    }
}
