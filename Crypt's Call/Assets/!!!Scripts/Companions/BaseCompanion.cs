using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class BaseCompanion : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the Animator for controlling companion animations.")]
    [SerializeField] protected Animator animator;

    protected Health health;

    protected bool isAttacking;

    protected EntityStats entityStats;

    protected float actionCooldown = 1.5f;

    // -------------------------------------------------------------------------
    // Animation Parameter Names
    // -------------------------------------------------------------------------

    protected virtual string WalkBool => "isWalking";

    protected virtual string DeadBool => "isDead";

    protected virtual string QuickSlashTrigger => "QuickSlash";

    // -------------------------------------------------------------------------
    // MonoBehaviour Methods
    // -------------------------------------------------------------------------
    protected virtual void Awake()
    {
        health = GetComponent<Health>();

        if (animator == null)
        {
            Debug.LogError($"[{nameof(BaseCompanion)}] Animator not assigned on '{name}'.");
        }

        health.OnDied += OnCompanionDied;
    }

    protected virtual void OnDestroy()
    {
        if (health != null)
        {
            health.OnDied -= OnCompanionDied;
        }
    }

    protected virtual void Update()
    {
        if (health.IsDead)
        {
            return;
        }

    }

    // -------------------------------------------------------------------------
    // Attack Logic
    // -------------------------------------------------------------------------
    public virtual void PerformBaseAction()
    {
        if (health.IsDead || isAttacking) return;

        animator.SetTrigger(QuickSlashTrigger);

        StartCoroutine(BaseActionRoutine());
    }

    protected virtual IEnumerator BaseActionRoutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(actionCooldown);

        isAttacking = false;
    }

    protected virtual void OnCompanionDied(GameObject companion)
    {
        animator.SetBool(DeadBool, true);
    }

    protected virtual IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(2f);

        this.gameObject.SetActive(false);
    }
}
