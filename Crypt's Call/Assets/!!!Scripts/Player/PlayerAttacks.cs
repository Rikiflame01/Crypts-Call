using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f; 
    [SerializeField] private float attackAngle = 90f;
    [SerializeField] private int maxTargets = 2; 
    [SerializeField] private int damage = 10; 

    [Header("Visual and Audio Effects")]
    [SerializeField] private GameObject[] hitVFXPrefabs; 
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private AudioClip[] hitClips; 

    [Header("Layer Settings")]
    [SerializeField] private LayerMask enemyLayer;

    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController component not found on the player.");
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioSource component not found. Please assign it in the inspector.");
            }
        }

        if (hitVFXPrefabs.Length == 0)
        {
            Debug.LogWarning("No hit VFX prefabs assigned to PlayerAttack.");
        }

        if (hitClips.Length == 0)
        {
            Debug.LogWarning("No hit audio clips assigned to PlayerAttack.");
        }
    }

    public void PerformQuickSlash()
    {
        if (playerController == null || !playerController.isPlayerAttacking)
        {
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        if (hitColliders.Length == 0)
        {
            return;
        }

        List<Transform> validTargets = new List<Transform>();

        Vector3 playerForward = transform.forward;

        foreach (Collider collider in hitColliders)
        {
            Vector3 directionToEnemy = (collider.transform.position - transform.position).normalized;
            float angleToEnemy = Vector3.Angle(playerForward, directionToEnemy);

            if (angleToEnemy <= attackAngle / 2)
            {
                validTargets.Add(collider.transform);
            }
        }

        if (validTargets.Count == 0)
        {
            return;
        }

        validTargets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));

        int targetsToHit = Mathf.Min(maxTargets, validTargets.Count);

        for (int i = 0; i < targetsToHit; i++)
        {
            Transform enemy = validTargets[i];
            ApplyDamage(enemy);
            PlayHitEffects(enemy);
        }
    }

    private void ApplyDamage(Transform enemy)
    {
        IHealth enemyHealth = enemy.GetComponent<IHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Debug.Log($"QuickSlash dealt {damage} damage to {enemy.name}");
        }
    }

    private void PlayHitEffects(Transform enemy)
    {
        if (hitVFXPrefabs.Length > 0)
        {
            GameObject vfx = hitVFXPrefabs[Random.Range(0, hitVFXPrefabs.Length)];
            Instantiate(vfx, enemy.position, Quaternion.identity);
        }

        if (hitClips.Length > 0 && audioSource != null)
        {
            AudioClip clip = hitClips[Random.Range(0, hitClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2, 0) * transform.forward * attackRange;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2, 0) * transform.forward * attackRange;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
