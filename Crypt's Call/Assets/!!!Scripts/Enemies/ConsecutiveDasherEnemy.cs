using UnityEngine;
using UnityEngine.AI;

public class ConsecutiveDasherEnemy : BaseEnemy
{
    [Header("Dash Attack")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;
    public float dashStartDistance = 5f;
    public float preDashWaitTime = 1f;

    [Header("Multiple Dashes Settings")]
    public int dashesPerCycle = 3;
    public float timeBetweenDashes = 1.8f;

    [Header("Standoff Distance")]
    public float desiredMinDistance = 8f;

    private float preDashTimer = 0f;
    private int dashesRemaining = 0;
    private float dashTimer = 0f;
    private float dashIntervalTimer = 0f;
    private Vector3 dashDirection;

    private enum ExtendedState
    {
        Idle,
        Patrol,
        PlayerDetected,
        PreparingDash,
        Dashing,
        WaitingForNextDash
    }

    private ExtendedState extendedState = ExtendedState.Idle;

    protected override void Start()
    {
        animator = GetComponent<Animator>();
        base.Start();
        extendedState = ExtendedState.Patrol;
    }

    protected override void Update()
    {
        if (isDead)
            return;
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
            case ExtendedState.PreparingDash:
                PreparingDashUpdate();
                break;
            case ExtendedState.Dashing:
                DashingUpdate();
                break;
            case ExtendedState.WaitingForNextDash:
                WaitingForNextDashUpdate();
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
                && extendedState != ExtendedState.Dashing
                && extendedState != ExtendedState.PreparingDash
                && extendedState != ExtendedState.WaitingForNextDash)
            {
                EnterPlayerDetectedState();
            }
        }
        else
        {
            detectedPlayer = null;
            if (extendedState == ExtendedState.PlayerDetected
                || extendedState == ExtendedState.Dashing
                || extendedState == ExtendedState.PreparingDash
                || extendedState == ExtendedState.WaitingForNextDash)
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

        if (distanceToPlayer <= dashStartDistance && dashesRemaining == 0)
        {
            dashesRemaining = dashesPerCycle;
            EnterPreparingDashState();
        }
    }

    private void EnterPreparingDashState()
    {
        extendedState = ExtendedState.PreparingDash;
        agent.isStopped = true;
        agent.ResetPath();
        preDashTimer = preDashWaitTime;

        if (detectedPlayer != null)
        {
            Vector3 targetDir = (detectedPlayer.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
        }
    }

    private void PreparingDashUpdate()
    {
        preDashTimer -= Time.deltaTime;

        if (detectedPlayer == null)
        {
            dashesRemaining = 0;
            EnterPatrolState();
            return;
        }

        if (preDashTimer <= 0f)
        {
            EnterDashingState();
        }
    }

    private void EnterDashingState()
    {
        extendedState = ExtendedState.Dashing;

        if (detectedPlayer != null)
        {
            Vector3 targetDir = (detectedPlayer.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
        }

        dashDirection = transform.forward;
        dashTimer = dashDuration;
    }

    private void DashingUpdate()
    {
        transform.position += dashDirection * dashSpeed * Time.deltaTime;
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            dashesRemaining--;
            if (dashesRemaining > 0)
            {
                EnterWaitingForNextDashState();
            }
            else
            {
                EnterPlayerDetectedState();
            }
        }
    }

    private void EnterWaitingForNextDashState()
    {
        extendedState = ExtendedState.WaitingForNextDash;
        dashIntervalTimer = timeBetweenDashes;
    }

    private void WaitingForNextDashUpdate()
    {
        dashIntervalTimer -= Time.deltaTime;
        if (dashIntervalTimer <= 0f)
        {
            EnterDashingState();
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashStartDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, desiredMinDistance);
    }
}
