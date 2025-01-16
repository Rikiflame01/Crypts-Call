using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDamager : MonoBehaviour
{
    [Header("Entity Stats")]
    [SerializeField] private EntityStats entityStats;

    [Header("Damage Settings")]
    [SerializeField] private float damageInterval = 2.0f;

    private float lastDamageTime = -Mathf.Infinity;
    
    public float knockbackForce = 7f;
    public float knockbackDuration = 0.5f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnCollisionStay(Collision collision)
    {
        if (!enabled)
            return;

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
                    ApplyKnockback(collision.gameObject);
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

    private void ApplyKnockback(GameObject target)
    {
        Vector3 knockbackDirection = target.transform.position - transform.position;
        knockbackDirection.y = 0;
        knockbackDirection.Normalize();

        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            StartCoroutine(KnockbackAgent(agent, knockbackDirection));
        }
        else
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
            else
            {
                Debug.LogWarning("The object to knock back does not have a NavMeshAgent or Rigidbody component.");
            }
        }
    }

    private IEnumerator KnockbackAgent(NavMeshAgent agent, Vector3 direction)
    {
        if (agent == null)
            yield break;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        float elapsed = 0f;
        Vector3 initialPosition = agent.transform.position;
        Vector3 knockbackMovement = direction * knockbackForce;

        while (elapsed < knockbackDuration)
        {
            agent.transform.position += knockbackMovement * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            animator.SetBool("isAttacking", false);
        }
    }
}
