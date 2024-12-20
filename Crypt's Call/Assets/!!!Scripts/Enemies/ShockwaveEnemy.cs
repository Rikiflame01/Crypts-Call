using UnityEngine;
using UnityEngine.AI;

public class ShockwaveEnemy : BaseEnemy
{
    [Header("Shockwave Attack")]
    public float desiredMinDistance = 5f;
    public float shockwaveRadius = 8f;
    public float shockwaveDamage = 20f;
    public float preShockwaveWaitTime = 1f;
    public float maintainDistanceTime = 3f;
    public float shockwaveCooldown = 2f;     
    public float retreatMultiplier = 2f;

    private float preShockwaveTimer = 0f;
    private float maintainDistanceTimer = 0f;
    private float cooldownTimer = 0f;
    private bool shockwaveReady = true;

    private Vector3 retreatPoint;

    private enum ExtendedState
    {
        Idle,
        Patrol,
        PlayerDetected,
        PreparingShockwave,
        ShockwaveActive,
        MaintainingDistance,
        AttackCooldown
    }

    private ExtendedState extendedState = ExtendedState.Idle;

    private Animator animator;
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        base.Start();
        extendedState = ExtendedState.Patrol;
    }

    protected override void Update()
    {
        switch (extendedState)
        {
            case ExtendedState.Idle:
                IdleUpdate();
                break;
            case ExtendedState.Patrol:
                PatrolUpdate();
                break;
            case ExtendedState.PlayerDetected:
                PlayerDetectedUpdate();
                break;
            case ExtendedState.PreparingShockwave:
                PreparingShockwaveUpdate();
                break;
            case ExtendedState.ShockwaveActive:
                ShockwaveUpdate();
                break;
            case ExtendedState.MaintainingDistance:
                MaintainingDistanceUpdate();
                break;
            case ExtendedState.AttackCooldown:
                AttackCooldownUpdate();
                break;
        }
        UpdateWalkingAnimation();
        CheckPlayerDetection();
    }

    private void UpdateWalkingAnimation()
    {
        if (animator != null && agent != null)
        {
            bool isWalking = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isWalking);
        }
        else
        {
            Debug.LogWarning("Animator or NavMeshAgent component is missing.");
        }
    }
    
    private new void CheckPlayerDetection()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hits.Length > 0)
        {
            detectedPlayer = hits[0].gameObject;
            if (extendedState != ExtendedState.PlayerDetected 
                && extendedState != ExtendedState.PreparingShockwave
                && extendedState != ExtendedState.ShockwaveActive
                && extendedState != ExtendedState.MaintainingDistance
                && extendedState != ExtendedState.AttackCooldown)
            {
                EnterPlayerDetectedState();
            }
        }
        else
        {
            detectedPlayer = null;
            if (extendedState == ExtendedState.PlayerDetected 
                || extendedState == ExtendedState.PreparingShockwave
                || extendedState == ExtendedState.ShockwaveActive
                || extendedState == ExtendedState.MaintainingDistance
                || extendedState == ExtendedState.AttackCooldown)
            {
                EnterPatrolState();
            }
        }
    }

    protected override void IdleUpdate()
    {
        EnterPatrolState();
    }
    private new void EnterPatrolState()
    {
        extendedState = ExtendedState.Patrol;
        agent.isStopped = false;
        SetNextPatrolDestination();
    }

    protected override void PatrolUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTime -= Time.deltaTime;
            if (patrolWaitTime <= 0f)
            {
                SetNextPatrolDestination();
            }
        }
    }

    private new void EnterPlayerDetectedState()
    {
        extendedState = ExtendedState.PlayerDetected;
        agent.isStopped = false;
    }

    protected override void PlayerDetectedUpdate()
    {
        if (!detectedPlayer)
        {
            EnterPatrolState();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.transform.position);

        if (distanceToPlayer > desiredMinDistance)
        {
            Vector3 directionToPlayer = (detectedPlayer.transform.position - transform.position).normalized;
            Vector3 targetPosition = detectedPlayer.transform.position - directionToPlayer * desiredMinDistance;
            agent.SetDestination(targetPosition);
        }
        else
        {
            agent.SetDestination(transform.position);
        }

        if (shockwaveReady && distanceToPlayer <= desiredMinDistance + 0.5f)
        {
            EnterPreparingShockwaveState();
        }
    }

    private void EnterPreparingShockwaveState()
    {
        extendedState = ExtendedState.PreparingShockwave;
        agent.isStopped = true;
        agent.ResetPath();
        preShockwaveTimer = preShockwaveWaitTime;

        if (detectedPlayer != null)
        {
            Vector3 targetDir = (detectedPlayer.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
        }
    }

    private void PreparingShockwaveUpdate()
    {
        preShockwaveTimer -= Time.deltaTime;

        if (detectedPlayer == null)
        {
            shockwaveReady = true;
            EnterPatrolState();
            return;
        }

        if (preShockwaveTimer <= 0f)
        {
            EnterShockwaveState();
        }
    }

    private void EnterShockwaveState()
    {
        extendedState = ExtendedState.ShockwaveActive;
        PerformShockwave();
    }

    private void ShockwaveUpdate()
    {
        EnterMaintainingDistanceState();
    }

    private void PerformShockwave()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, shockwaveRadius, playerLayer);
        foreach (Collider hit in hits)
        {
            IHealth health = hit.GetComponent<IHealth>();
            if (health != null)
            {
                health.TakeDamage(shockwaveDamage);
            }
        }

        shockwaveReady = false;
    }

    private void EnterMaintainingDistanceState()
    {
        extendedState = ExtendedState.MaintainingDistance;
        maintainDistanceTimer = maintainDistanceTime;
        agent.isStopped = false;

        if (detectedPlayer != null)
        {
            Vector3 awayFromPlayer = (transform.position - detectedPlayer.transform.position).normalized;
            float retreatDistance = desiredMinDistance * retreatMultiplier;
            retreatPoint = transform.position + awayFromPlayer * retreatDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(retreatPoint, out hit, retreatDistance, NavMesh.AllAreas))
            {
                retreatPoint = hit.position;
            }

            agent.SetDestination(retreatPoint);
        }
    }

    private void MaintainingDistanceUpdate()
    {
        maintainDistanceTimer -= Time.deltaTime;

        if (detectedPlayer == null)
        {
            EnterPatrolState();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            agent.SetDestination(transform.position);
        }

        if (maintainDistanceTimer <= 0f)
        {
            EnterAttackCooldownState();
        }
    }

    private void EnterAttackCooldownState()
    {
        extendedState = ExtendedState.AttackCooldown;
        cooldownTimer = shockwaveCooldown;
    }

    private void AttackCooldownUpdate()
    {
        cooldownTimer -= Time.deltaTime;

        if (detectedPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.transform.position);

            if (distanceToPlayer > desiredMinDistance)
            {
                Vector3 directionToPlayer = (detectedPlayer.transform.position - transform.position).normalized;
                Vector3 targetPosition = detectedPlayer.transform.position - directionToPlayer * desiredMinDistance;
                agent.SetDestination(targetPosition);
            }
            else
            {
                agent.SetDestination(transform.position);
            }
        }
        else
        {
            EnterPatrolState();
            return;
        }

        if (cooldownTimer <= 0f)
        {
            shockwaveReady = true;
            EnterPlayerDetectedState();
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, desiredMinDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(retreatPoint, 0.5f);
    }
}
