using System.Collections;
using UnityEngine;

public class Damager : MonoBehaviour
{
    [Header("Entity Stats")]
    [SerializeField] private EntityStats entityStats;
    public GameObject[] VFX;
    public AudioSource audioSource;
    public AudioClip[] audioClips;

    public AudioClip[] projectileClips;
    private PlayerController playerController;
    [SerializeField] private bool isPlayerWeapon =false;
    [SerializeField] private bool isProjectile = false;

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
        if (isProjectile == true)
        {
            audioSource.clip = projectileClips[1];
            audioSource.Play();
            StartCoroutine(DestroyAfterDelay());
        }
        if (entityStats == null)
        {
            Debug.LogWarning("EntityStats not assigned to Damager on " + gameObject.name);
            return;
        }
        IHealth health = other.gameObject.GetComponent<IHealth>();

        if (health != null && other.gameObject.CompareTag("Player") && isPlayerWeapon == false)
        {
            Debug.Log($"{gameObject.name} dealt {entityStats.damage} damage to {other.gameObject.name}");
            health.TakeDamage(entityStats.damage);
        }
        else if (health != null && isPlayerWeapon == true && playerController.isPlayerAttacking == true)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                PlaySparkVFX(other);
                Debug.Log($"{gameObject.name} dealt {entityStats.damage} damage to {other.gameObject.name}");
                health.TakeDamage(entityStats.damage);
            }
        }
        else
        {
           return;
        }

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
