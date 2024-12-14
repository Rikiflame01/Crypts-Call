using UnityEngine;

public class StationaryFollow : MonoBehaviour
{
    public Transform player;

    public Vector3 offset = Vector3.zero;

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
