using UnityEngine;
using UnityEngine.AI;

public class ClerkCompanion : BaseCompanion
{
    public new EntityStats entityStats;

    [Header("Health/Recovery Settings")]
    [SerializeField] private float companionHealthThreshold = 30f;
    [SerializeField] private float recoveryTime = 3f;

    [Header("Healing Settings")]
    [SerializeField] private float playerDamageThreshold = 20f;

    [Header("Movement Settings")]
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float moveSpeed = 3.5f;

    private Transform playerTransform;
    private NavMeshAgent agent;

    private bool isResting = false;
    private float restingTimer = 0f;

    private void Start()
    {
        actionCooldown = 25f;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player ? player.transform : null;

        agent = GetComponent<NavMeshAgent>();
        if (!agent)
        {
            Debug.LogError("[ClerkCompanion] No NavMeshAgent found on this GameObject.");
            return;
        }

        agent.speed = moveSpeed;
    }

    protected override void Update()
    {
        base.Update();

        if (actionCooldown > 0)
        {
            actionCooldown -= Time.deltaTime;
            if (actionCooldown < 0) actionCooldown = 0f;
        }

        if (health.IsDead)
        {
            agent.isStopped = true;
            return;
        }

        bool needsHealing = DoesPlayerNeedHealing();

        if (needsHealing && actionCooldown <= 0f)
        {
            PerformBaseAction();
            return;
        }

        FollowPlayer();
    }

    private bool DoesPlayerNeedHealing()
    {
        if (!playerTransform) return false;
        Health playerHealth = playerTransform.GetComponent<Health>();
        if (!playerHealth) return false;
        float missingHealth = playerHealth.MaxHealth - playerHealth.CurrentHealth;
        return missingHealth >= playerDamageThreshold;
    }

    private void FollowPlayer()
    {
        if (!playerTransform)
        {
            if (agent) agent.isStopped = true;
            animator.SetBool(WalkBool, false);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= followDistance)
        {
            if (agent) agent.isStopped = true;
            animator.SetBool(WalkBool, false);
        }
        else
        {
            if (agent)
            {
                agent.isStopped = false;
                agent.speed = moveSpeed;
                agent.SetDestination(playerTransform.position);
            }
            animator.SetBool(WalkBool, true);
        }
    }

    public override void PerformBaseAction()
    {
        if (actionCooldown > 0f) return;
        StartCoroutine(PerformHealAction());
    }

    private System.Collections.IEnumerator PerformHealAction()
    {
        if (agent) agent.isStopped = true;

        base.PerformBaseAction();
        entityStats.stamina -= 5;
        entityStats.mana -= 5;

        if (playerTransform)
        {
            Health playerHealth = playerTransform.GetComponent<Health>();
            if (playerHealth) playerHealth.Heal(20f);
        }

        actionCooldown = 25f;

        yield return new WaitForSeconds(1.5f);

        if (agent) agent.isStopped = false;
    }
}
