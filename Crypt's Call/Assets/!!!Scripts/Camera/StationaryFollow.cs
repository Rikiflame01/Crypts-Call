using UnityEngine;

public class StationaryFollow : MonoBehaviour
{
    [Tooltip("Assign the player's Transform here. If left empty, the script will try to find the player by tag.")]
    public Transform player;

    [Tooltip("Optional offset to adjust the camera's look direction.")]
    public Vector3 offset = Vector3.zero;

    [Tooltip("Smoothing factor for camera rotation. Higher values make the camera rotate faster.")]
    public float rotationSpeed = 5f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("CameraLookAtPlayer: No GameObject with tag 'Player' found. Please assign the player Transform in the Inspector.");
            }
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPosition = player.position + offset;

            Vector3 direction = targetPosition - transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
