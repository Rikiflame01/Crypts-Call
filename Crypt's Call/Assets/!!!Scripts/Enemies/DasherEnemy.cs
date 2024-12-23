using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
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

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"[{nameof(DashingEnemy)}] Animator component is missing on {gameObject.name}. Disabling script.");
            this.enabled = false;
            return;
        }
    }

    protected override void Start()
    {
        base.Start();
        extendedState = ExtendedState.Patrol;
    }

    protected override void Update()
    {

        if (isDead)
        return;

        if (agent == null)
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] NavMeshAgent is null on {gameObject.name}. Skipping Update.");
            return;
        }

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
            Debug.LogWarning($"[{nameof(DashingEnemy)}] Animator or NavMeshAgent component is missing on {gameObject.name}.");
        }
    }

    private new void CheckPlayerDetection()
    {
        if (agent == null)
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] NavMeshAgent is null on {gameObject.name}. Skipping player detection.");
            return;
        }

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
        if (agent != null)
        {
            agent.isStopped = false;
            SetNextPatrolDestination();
        }
        else
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] NavMeshAgent is null on {gameObject.name}. Cannot enter Patrol state.");
        }
    }

    protected override void PatrolUpdate()
    {
        if (agent == null)
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] NavMeshAgent is null on {gameObject.name}. Cannot update Patrol state.");
            return;
        }

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
        if (agent != null)
        {
            agent.isStopped = false;
        }
        else
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] NavMeshAgent is null on {gameObject.name}. Cannot enter PlayerDetected state.");
        }
    }

    protected override void PlayerDetectedUpdate()
    {
        if (agent == null)
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] NavMeshAgent is null on {gameObject.name}. Cannot update PlayerDetected state.");
            return;
        }

        if (!detectedPlayer)
        {
            EnterPatrolState();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.transform.position);

        if (distanceToPlayer > desiredMinDistance && agent != null)
        {
            Vector3 directionToPlayer = (detectedPlayer.transform.position - transform.position).normalized;
            Vector3 targetPosition = detectedPlayer.transform.position - directionToPlayer * desiredMinDistance;
            if (!TrySetDestination(targetPosition))
            {
                Debug.LogWarning($"[{nameof(DashingEnemy)}] Failed to set destination towards desiredMinDistance from player.");
            }
        }
        else
        {
            if (!TrySetDestination(transform.position))
            {
                Debug.LogWarning($"[{nameof(DashingEnemy)}] Failed to set destination to current position.");
            }
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
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        else
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] NavMeshAgent is null on {gameObject.name}. Cannot stop agent for PreparingDash state.");
        }

        preDashTimer = preDashWaitTime;

        if (detectedPlayer != null)
        {
            Vector3 targetDir = (detectedPlayer.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
        }
    }

    private void PreparingDashUpdate()
    {
        if (isDead)
        return;
        if (animator != null)
        {
            animator.SetBool("isCharging", true);
        }
        else
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] Animator is null on {gameObject.name}. Cannot set isCharging.");
        }

        preDashTimer -= Time.deltaTime;

        if (detectedPlayer == null)
        {
            dashCooldownTimer = dashCooldown;
            if (animator != null)
            {
                animator.SetBool("isCharging", false);
            }
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
        if (isDead)
        return;
        extendedState = ExtendedState.Dashing;
        if (detectedPlayer != null)
        {
            Vector3 targetDir = (detectedPlayer.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
        }

        dashDirection = transform.forward;
        dashTimer = dashDuration;

        if (animator != null)
        {
            animator.SetBool("isAttacking", true);
        }
        else
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] Animator is null on {gameObject.name}. Cannot set isAttacking.");
        }
    }

    private void DashingUpdate()
    {
        if (isDead)
        return;
        if (agent == null)
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] NavMeshAgent is null on {gameObject.name}. Cannot perform Dashing.");
            return;
        }

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
                if (animator != null)
                {
                    animator.SetBool("isAttacking", false);
                }
                EnterPatrolState();
            }

            if (animator != null)
            {
                animator.SetBool("isAttacking", false);
            }
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


    private bool TrySetDestination(Vector3 destination)
    {
        if (agent != null)
        {
            agent.SetDestination(destination);
            return true;
        }
        else
        {
            return false;
        }
    }

    
}
