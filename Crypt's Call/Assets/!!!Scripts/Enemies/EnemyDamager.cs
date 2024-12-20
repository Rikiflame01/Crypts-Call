using UnityEngine;

public class EnemyDamager : MonoBehaviour
{
    [Header("Entity Stats")]
    [SerializeField] private EntityStats entityStats;

    [Header("Damage Settings")]
    [SerializeField] private float damageInterval = 2.0f;

    private float lastDamageTime = -Mathf.Infinity;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnCollisionStay(Collision collision)
    {
        if (entityStats == null)
        {
            Debug.LogWarning($"EntityStats not assigned to EnemyDamager on {gameObject.name}");
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            IHealth playerHealth = collision.gameObject.GetComponent<IHealth>();
            
            if (playerHealth != null)
            {
                if (Time.time - lastDamageTime >= damageInterval)
                {
                    animator.SetBool("isAttacking", true);
                    
                    Debug.Log($"{gameObject.name} dealt {entityStats.damage} damage to {collision.gameObject.name}");
                    playerHealth.TakeDamage(entityStats.damage);
                    
                    lastDamageTime = Time.time;
                }
            }
            else
            {
                Debug.LogWarning($"Player object does not have an IHealth component: {collision.gameObject.name}");
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            animator.SetBool("isAttacking", false);
            lastDamageTime = -Mathf.Infinity;
        }
    }
}
