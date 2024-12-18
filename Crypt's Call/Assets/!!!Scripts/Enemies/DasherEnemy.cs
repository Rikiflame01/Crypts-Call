using UnityEngine;
using UnityEngine.AI;

public class DashingEnemy : BaseEnemy
{
    [Header("Dash Attack")]
    public float dashSpeed = 10f;
    public float dashDuration = 1f;
    public float dashCooldown = 2f;
    public float dashStartDistance = 5f;
    public float preDashWaitTime = 1f;

    [Header("Standoff Distance")]
    public float desiredMinDistance = 8f;

    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private float preDashTimer = 0f;
    private Vector3 dashDirection;

    private enum ExtendedState
    {
        Idle,
        Patrol,
        PlayerDetected,
        PreparingDash,
        Dashing
    }

    private ExtendedState extendedState = ExtendedState.Idle;

    protected override void Start()
    {
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
            case ExtendedState.PreparingDash:
                PreparingDashUpdate();
                break;
            case ExtendedState.Dashing:
                DashingUpdate();
                break;
        }

        CheckPlayerDetection();
    }

    private new void CheckPlayerDetection()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hits.Length > 0)
        {
            detectedPlayer = hits[0].gameObject;
            if (extendedState != ExtendedState.PlayerDetected
                && extendedState != ExtendedState.Dashing
                && extendedState != ExtendedState.PreparingDash)
            {
                EnterPlayerDetectedState();
            }
        }
        else
        {
            detectedPlayer = null;
            if (extendedState == ExtendedState.PlayerDetected
                || extendedState == ExtendedState.Dashing
                || extendedState == ExtendedState.PreparingDash)
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
        SetRandomPatrolDestination();
    }

    protected override void PatrolUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTime -= Time.deltaTime;
            if (patrolWaitTime <= 0f)
            {
                SetRandomPatrolDestination();
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

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (dashCooldownTimer <= 0f && distanceToPlayer <= dashStartDistance)
        {
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
            dashCooldownTimer = dashCooldown;
            if (detectedPlayer != null)
            {
                EnterPlayerDetectedState();
            }
            else
            {
                EnterPatrolState();
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        //start distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashStartDistance);

        //min distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, desiredMinDistance);
    }
}
