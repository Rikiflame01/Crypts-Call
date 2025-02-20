using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DemonFly : BaseEnemy
{
    [Header("DemonFly Settings")]
    public float dashSpeed = 10f;
    public float attackDuration = 1f;
    public int attackCount = 3; 
    public float retreatDuration = 3f;
    public float dashStartDistance = 5f;
    public GameObject attackColliderObject;

    [Header("Attack Cooldown Settings")]
    [Tooltip("Time in seconds between consecutive attack sets.")]
    public float attackCooldown = 5f;

    [Header("Retreat Settings")]
    [Tooltip("Minimum distance to maintain from the player when retreating.")]
    public float retreatMinDistance = 8f;

    private int remainingAttacks;
    private float attackTimer;
    private PlayerController playerController;
    private float dashTimer;
    private Vector3 dashDirection;
    private float cooldownTimer;
    private float retreatTimer;

    private enum ExtendedState
    {
        None,
        Dashing,
        Attacking,
        Cooldown,
        Retreating
    }

    private ExtendedState extendedState = ExtendedState.None;

    public override bool IsAttacking
    {
        get => base.IsAttacking;
        protected set => base.IsAttacking = value;
    }

    PlayerTriggerStunner playerTriggerStunner;
    GameObject player;

    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerTriggerStunner = player.GetComponent<PlayerTriggerStunner>();

        animator = GetComponent<Animator>();
        base.Start();
        remainingAttacks = attackCount;

        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(false);
            AttackCollider attackColliderScript = attackColliderObject.GetComponent<AttackCollider>();
            if (attackColliderScript != null)
            {
                attackColliderScript.onPlayerHit += HandlePlayerHit;
            }
        }
    }

    protected override void Update()
    {
        if (isDead || isStunned == true)
            return;
        base.Update(); 
        switch (extendedState)
        {
            case ExtendedState.Dashing:
                IsAttacking = false;
                DashingUpdate();
                break;
            case ExtendedState.Attacking:
                IsAttacking = true;
                AttackPlayer();
                break;
            case ExtendedState.Cooldown:
                IsAttacking = false;
                CooldownUpdate();
                break;
            case ExtendedState.Retreating:
                IsAttacking = false;
                RetreatUpdate();
                break;
            default:
                break;
        }

        UpdateWalkingAnimation();
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


    protected override void PatrolUpdate()
    {
        base.PatrolUpdate();

    }

    protected override void PlayerDetectedUpdate()
    {
        if (detectedPlayer == null)
        {
            base.PlayerDetectedUpdate();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.transform.position);

        if (extendedState == ExtendedState.None && distanceToPlayer <= dashStartDistance && remainingAttacks == attackCount && cooldownTimer <= 0f)
        {
            EnterDashingState();
        }
        else if (extendedState == ExtendedState.None)
        {
            base.PlayerDetectedUpdate();
        }
    }

    private void EnterDashingState()
    {
        extendedState = ExtendedState.Dashing;
        agent.isStopped = true;

        if (detectedPlayer != null)
        {
            Vector3 targetDir = (detectedPlayer.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
            dashDirection = transform.forward;
        }

        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(true);
        }

        dashTimer = 0.5f;
    }

    private void DashingUpdate()
    {
        transform.position += dashDirection * dashSpeed * Time.deltaTime;

        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0f && extendedState == ExtendedState.Dashing)
        {
            if (attackColliderObject != null)
            {
                attackColliderObject.SetActive(false);
            }

            extendedState = ExtendedState.None;
            agent.isStopped = false;
        }
    }

    private void HandlePlayerHit(GameObject player)
    {
        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(false);
        }
        EnterAttackingState(player);
    }

    private void EnterAttackingState(GameObject player)
    {
        if (playerTriggerStunner.enabled == true) {return;}
        extendedState = ExtendedState.Attacking;
        agent.isStopped = true;

        playerController = player.GetComponent<PlayerController>();

        if (playerController != null)
        {
            Animator playerAnimator = player.GetComponent<Animator>();
            playerAnimator.SetBool("isStunned", true);
            EventManager.TriggerStunApplied(player);
            EventManager.TriggerPoisonApplied(player);
            playerController.enabled = false;
        }

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null && playerTriggerStunner.enabled != true)
        {
            playerRb.constraints = RigidbodyConstraints.FreezePosition;
            playerRb.isKinematic = true;
        }

        attackTimer = attackDuration; 
        remainingAttacks = attackCount;
    }

    private void AttackPlayer()
    {
        if (playerTriggerStunner.enabled == true) {return;}
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f && playerTriggerStunner.enabled != true)
        {
            IHealth health = detectedPlayer ? detectedPlayer.GetComponent<IHealth>() : null;
            health?.TakeDamage(10);

            remainingAttacks--;
            attackTimer = attackDuration;

            if (remainingAttacks > 0)
            {
                attackTimer = attackDuration;
            }
            else
            {
                EnterCooldownState();
            }
        }
    }

    private void EnterCooldownState()
    {
        if (playerTriggerStunner.enabled == true) {return;}
        extendedState = ExtendedState.Cooldown;
        cooldownTimer = attackCooldown;

        if (detectedPlayer != null)
        {
            Animator playerAnimator = player.GetComponent<Animator>();
            playerAnimator.SetBool("isStunned", false);
            Rigidbody playerRb = detectedPlayer.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.constraints = RigidbodyConstraints.None;
                playerRb.isKinematic=false;
            }

            if (playerController != null && playerTriggerStunner.enabled != true)
            {
                playerController.enabled = true;
                playerController = null;
            }
        }

        agent.isStopped = false;
    }

    private void CooldownUpdate()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            cooldownTimer = 0f;
            remainingAttacks = attackCount;
            extendedState = ExtendedState.None;

            if (detectedPlayer != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.transform.position);
                if (distanceToPlayer < retreatMinDistance)
                {
                    EnterRetreatingState();
                }
            }
        }
    }

    private void EnterRetreatingState()
    {
        extendedState = ExtendedState.Retreating;
        agent.isStopped = true;
        retreatTimer = retreatDuration;

        if (detectedPlayer != null)
        {
            Vector3 directionAway = (transform.position - detectedPlayer.transform.position).normalized;
            Vector3 retreatPosition = transform.position + directionAway * retreatMinDistance;
            agent.SetDestination(retreatPosition);
        }

        if (detectedPlayer != null)
        {
            Rigidbody playerRb = detectedPlayer.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.constraints = RigidbodyConstraints.None;
            }

            if (playerController != null)
            {
                playerController.enabled = true;
                playerController = null;
            }
        }
    }

    private void RetreatUpdate()
    {
        if (detectedPlayer == null)
        {
            extendedState = ExtendedState.None;
            agent.isStopped = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.transform.position);

        if (distanceToPlayer >= retreatMinDistance)
        {
            extendedState = ExtendedState.None;
            agent.isStopped = false;
        }
        else
        {
            retreatTimer -= Time.deltaTime;
            if (retreatTimer <= 0f)
            {
                extendedState = ExtendedState.None;
                agent.isStopped = false;
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashStartDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retreatMinDistance);
    }

    protected override void OnDisable()
    {
        if (!isStunned || health.CurrentHealth <= 0){
            Animator playerAnimator = player.GetComponent<Animator>();
            playerAnimator.SetBool("isStunned", false);
            if (health != null)
            {
                health.OnDied -= HandleEnemyDeath;
            }        
            gameObject.SetActive(false);
        
        }
        playerController.enabled = true;
    }
}
