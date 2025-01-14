using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float baseAttackRange = 2f; 
    [SerializeField] private float baseAttackAngle = 90f;
    [SerializeField] private int baseMaxTargets = 2; 
    [SerializeField] private int baseDamage = 10; 

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
            Debug.LogWarning("No hit VFX prefabs assigned to PlayerAttacks.");
        }

        if (hitClips.Length == 0)
        {
            Debug.LogWarning("No hit audio clips assigned to PlayerAttacks.");
        }
    }

    public void PerformQuickSlash()
    {
        if (playerController == null || !playerController.isPlayerAttacking)
        {
            return;
        }

        int currentCombo = playerController.ComboStep;

        float attackRange = baseAttackRange;
        float attackAngle = baseAttackAngle;
        int maxTargets = baseMaxTargets;
        int damage = baseDamage;

        switch (currentCombo)
        {
            case 1:
                attackRange = baseAttackRange;
                attackAngle = baseAttackAngle;
                maxTargets = baseMaxTargets;
                damage = baseDamage;
                break;
            case 2:
                attackRange = baseAttackRange + 1f;
                attackAngle = baseAttackAngle + 90f;
                maxTargets = 3;
                damage = baseDamage + 2;
                break;
            case 3:
                attackRange = baseAttackRange + 2f;
                attackAngle = baseAttackAngle + 270f;
                maxTargets = 8;
                damage = baseDamage + 4;
                break;
            default:
                attackRange = baseAttackRange;
                attackAngle = baseAttackAngle;
                maxTargets = baseMaxTargets;
                damage = baseDamage;
                break;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        if (hitColliders.Length == 0)
        {
            Debug.Log("No enemies within attack range.");
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
            else
            {
                Debug.Log($"Enemy {collider.name} is outside attack angle ({angleToEnemy} degrees).");
            }
        }

        if (validTargets.Count == 0)
        {
            return;
        }

        validTargets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));

        int targetsToHit = Mathf.Min(maxTargets, validTargets.Count);
        Debug.Log($"Targets to hit: {targetsToHit}");

        for (int i = 0; i < targetsToHit; i++)
        {
            Transform enemy = validTargets[i];
            ApplyDamage(enemy, damage);
            PlayHitEffects(enemy);
        }
    }

    private void ApplyDamage(Transform enemy, int damageAmount)
    {
        IHealth enemyHealth = enemy.GetComponent<IHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damageAmount);
            Debug.Log($"Attack dealt {damageAmount} damage to {enemy.name}");
        }
        else
        {
            Debug.LogWarning($"Enemy {enemy.name} does not have an IHealth component.");
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
            Debug.Log($"Played hit audio clip: {clip.name}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, baseAttackRange + (playerController != null ? (playerController.ComboStep - 1) * 1f : 0f));

        float currentAttackAngle = baseAttackAngle + ((playerController != null && playerController.ComboStep > 0) ? (playerController.ComboStep - 1) * 90f : 0f);

        Vector3 leftBoundary = Quaternion.Euler(0, -currentAttackAngle / 2, 0) * transform.forward * (baseAttackRange + (playerController != null ? (playerController.ComboStep - 1) * 1f : 0f));
        Vector3 rightBoundary = Quaternion.Euler(0, currentAttackAngle / 2, 0) * transform.forward * (baseAttackRange + (playerController != null ? (playerController.ComboStep - 1) * 1f : 0f));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
