using UnityEngine;

public class PlayerTriggerStunner : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("Layer mask for detecting enemies.")]
    [SerializeField] private LayerMask enemyLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled)
        return;
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            StunController stunController = other.GetComponent<StunController>();

            if (stunController != null)
            {
                stunController.TriggerStun();
            }
            else
            {
                Debug.LogWarning($"No StunController found on {other.gameObject.name}.");
            }
        }
    }
}
