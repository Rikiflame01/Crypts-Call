using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Damager : MonoBehaviour
{
    [Header("Entity Stats")]
    [SerializeField] private EntityStats entityStats;
    public GameObject[] VFX;
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public AudioClip[] projectileClips;

    private PlayerController playerController;
    [SerializeField] private bool isPlayerWeapon = false;
    [SerializeField] private bool isProjectile = false;
    [SerializeField] private float damageCooldown = 2f;
    private bool canDealDamage = true;

    public float knockbackForce = 7f;
    public float knockbackDuration = 0.5f;

    void Start()
    {
        if (isProjectile)
        {
            audioSource = GetComponent<AudioSource>();
            if (projectileClips.Length > 0)
            {
                audioSource.clip = projectileClips[0];
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("No projectile clips assigned.");
            }
        }

        if (isPlayerWeapon)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogWarning("PlayerController component not found on Player.");
                }
            }
            else
            {
                Debug.LogWarning("Player object with tag 'Player' not found.");
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (entityStats == null)
        {
            Debug.LogWarning("EntityStats not assigned to Damager.");
            return;
        }

        IHealth health = other.gameObject.GetComponent<IHealth>();

        if (!canDealDamage)
            return;

        if (!isPlayerWeapon)
        {
            IEnemy enemy = this.gameObject.GetComponent<IEnemy>();

            if (enemy != null && health != null && enemy.IsAttacking && other.gameObject.CompareTag("Player"))
            {
                DealDamage(health, other.gameObject);
            }
        }
        else if (health != null && playerController != null && playerController.isPlayerAttacking)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                DealDamage(health, other.gameObject);
                PlaySparkVFX(other);
            }
        }

        if (isProjectile)
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private void DealDamage(IHealth health, GameObject target)
    {
        health.TakeDamage(entityStats.damage);
        ApplyKnockback(target);
        StartCoroutine(StartDamageCooldown());
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
        Vector3 knockbackMovement = direction * knockbackForce;

        while (elapsed < knockbackDuration)
        {
            agent.transform.position += knockbackMovement * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = false;
    }

    private IEnumerator StartDamageCooldown()
    {
        canDealDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canDealDamage = true;
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private void PlaySparkVFX(Collision other)
    {
        if (audioClips.Length > 0)
        {
            AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
            audioSource.PlayOneShot(randomClip);
        }
        else
        {
            Debug.LogWarning("No audio clips assigned for VFX.");
        }

        if (VFX.Length > 0)
        {
            Vector3 hitPoint = other.GetContact(0).point;
            Vector3 hitNormal = other.GetContact(0).normal;
            GameObject randomVFX = VFX[Random.Range(0, VFX.Length)];

            GameObject sparkVFX = Instantiate(randomVFX, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(sparkVFX, 1f);
        }
        else
        {
            Debug.LogWarning("No VFX prefabs assigned.");
        }
    }
}
