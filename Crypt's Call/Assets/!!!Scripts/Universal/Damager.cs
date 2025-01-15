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
    private bool canDealDamage = true;
    [SerializeField] private float damageCooldown = 2f;
    
    public float knockbackForce = 7f;
    public float knockbackDuration = 0.5f;

    void Start()
    {
        if (isProjectile == true)
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = projectileClips[0];
            audioSource.Play();
        }
        if (isPlayerWeapon == true)
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
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

        if (!isPlayerWeapon)
        {
            IEnemy enemy = this.gameObject.GetComponent<IEnemy>();

            if (enemy != null && health != null && enemy.IsAttacking && other.gameObject.CompareTag("Player"))
            {
                health.TakeDamage(entityStats.damage);
                ApplyKnockback(other.gameObject);
                StartCoroutine(StartDamageCooldown());
            }
        }
        else if (health != null && playerController.isPlayerAttacking)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                PlaySparkVFX(other);
                health.TakeDamage(entityStats.damage);
                ApplyKnockback(other.gameObject);
                StartCoroutine(StartDamageCooldown());
            }
        }
        if (isProjectile == true) {StartCoroutine(DestroyAfterDelay());}
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
        AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
        audioSource.PlayOneShot(randomClip);
        Vector3 hitPoint = other.GetContact(0).point;
        Vector3 hitNormal = other.GetContact(0).normal;
        GameObject randomVFX = VFX[Random.Range(0, VFX.Length)];

        GameObject sparkVFX = Instantiate(randomVFX, hitPoint, Quaternion.LookRotation(hitNormal));
        Destroy(sparkVFX, 1f);
    }
}
