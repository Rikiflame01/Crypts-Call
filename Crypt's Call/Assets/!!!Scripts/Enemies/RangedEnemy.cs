using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Combat Settings")]
    public float desiredDistance = 10f;
    public GameObject projectilePrefab;            
    public Transform firePoint;                    
    public float projectileForce = 20f;             

    [Header("Retreat Settings")]
    public float maxRetreatTime = 3f;               
    private float retreatTimer = 0f;                

    [Header("Burst Firing Settings")]
    public int shotsPerBurst = 3;                  
    public float timeBetweenShotsInBurst = 0.5f;   
    public float burstInterval = 3f;                

    private float nextBurstTimer = 0f;              
    private int shotsRemainingInBurst = 0;          
    private float shotTimer = 0f;                   

    [Header("Visibility Check")]
    [Tooltip("Assign the SkinnedMeshRenderer of this enemy here for visibility checks.")]
    public SkinnedMeshRenderer skinnedMeshRenderer;

    private enum ExtendedState
    {
        Idle,
        Patrol,
        PlayerDetected
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
        if (isDead) return;

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

    private bool IsVisibleToCamera()
    {
        return IsFullyVisible(skinnedMeshRenderer);
    }

    private void CheckPlayerDetection()
    {
        if (!IsVisibleToCamera()) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hits.Length > 0)
        {
            detectedPlayer = hits[0].gameObject;
            if (extendedState != ExtendedState.PlayerDetected)
            {
                EnterPlayerDetectedState();
            }
        }
        else
        {
            detectedPlayer = null;
            if (extendedState == ExtendedState.PlayerDetected)
            {
                EnterPatrolState();
            }
        }
    }

private bool IsFullyVisible(SkinnedMeshRenderer rend)
{
    if (!rend || !Camera.main) return false;

    Bounds bounds = rend.bounds;
    Vector3 center = bounds.center;
    Vector3 extents = bounds.extents;

    //Calculate all 8 corners of the bounding box
    Vector3[] corners = new Vector3[8];
    corners[0] = center + new Vector3(+extents.x, +extents.y, +extents.z);
    corners[1] = center + new Vector3(+extents.x, +extents.y, -extents.z);
    corners[2] = center + new Vector3(+extents.x, -extents.y, +extents.z);
    corners[3] = center + new Vector3(+extents.x, -extents.y, -extents.z);
    corners[4] = center + new Vector3(-extents.x, +extents.y, +extents.z);
    corners[5] = center + new Vector3(-extents.x, +extents.y, -extents.z);
    corners[6] = center + new Vector3(-extents.x, -extents.y, +extents.z);
    corners[7] = center + new Vector3(-extents.x, -extents.y, -extents.z);

    //Convert each corner to viewport space and check if it is within [0..1]
    foreach (var corner in corners)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(corner);

        //If any corner is off-screen or behind the camera, return false
        if (viewportPos.z < 0f ||
            viewportPos.x < 0f || viewportPos.x > 1f ||
            viewportPos.y < 0f || viewportPos.y > 1f)
        {
            return false;
        }
    }

    //All corners are fully within the camera's view
    return true;
}


    protected override void IdleUpdate()
    {
        EnterPatrolState();
    }

    protected override void PatrolUpdate()
    {
        if (agent != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTime -= Time.deltaTime;
            if (patrolWaitTime <= 0f)
            {
                SetNextPatrolDestination();
            }
        }
    }

    private void EnterPlayerDetectedState()
    {
        extendedState = ExtendedState.PlayerDetected;
        agent.isStopped = false;

        nextBurstTimer = 0f;
        shotsRemainingInBurst = 0;
        shotTimer = 0f;
        retreatTimer = 0f;
    }

    protected override void PlayerDetectedUpdate()
    {
        if (!detectedPlayer)
        {
            EnterPatrolState();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.transform.position);

        if (distanceToPlayer < desiredDistance)
        {
            if (retreatTimer == 0f)
            {
                retreatTimer = maxRetreatTime;
            }

            if (retreatTimer > 0f)
            {
                retreatTimer -= Time.deltaTime;
                RetreatFromPlayer(distanceToPlayer);
            }
            else if (isDead == false)
            {
                agent.SetDestination(transform.position);
                FacePlayer();
                HandleFiring();
            }
        }
        else if (isDead == false)
        {
            retreatTimer = 0f;

            agent.SetDestination(transform.position);
            FacePlayer();
            HandleFiring();
        }
    }

    private void RetreatFromPlayer(float distanceToPlayer)
    {
        Vector3 awayFromPlayer = (transform.position - detectedPlayer.transform.position).normalized;
        float neededDistance = desiredDistance - distanceToPlayer;
        Vector3 retreatPosition = transform.position + awayFromPlayer * neededDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatPosition, out hit, desiredDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            agent.SetDestination(transform.position);
        }
    }

    private void FacePlayer()
    {
        if (isDead) return;

        if (detectedPlayer != null)
        {
            Vector3 directionToPlayer = (detectedPlayer.transform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero && !isDead)
            {
                transform.rotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            }
        }
    }

    private void HandleFiring()
    {
        if (!IsVisibleToCamera()) return;

        if (shotsRemainingInBurst <= 0)
        {
            nextBurstTimer -= Time.deltaTime;
            if (nextBurstTimer <= 0f)
            {
                shotsRemainingInBurst = shotsPerBurst;
                shotTimer = 0f;
                animator.SetBool("isAttacking", true);
            }
        }
        else
        {
            shotTimer -= Time.deltaTime;
            if (shotTimer <= 0f)
            {
                FireProjectile();
                shotsRemainingInBurst--;

                if (shotsRemainingInBurst > 0)
                {
                    shotTimer = timeBetweenShotsInBurst;
                }
                else
                {
                    nextBurstTimer = burstInterval;
                    animator.SetBool("isAttacking", false);
                }
            }
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * projectileForce, ForceMode.VelocityChange);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, desiredDistance);
    }

    protected override void HandleEnemyDeath(GameObject deadEnemy)
    {
        if (deadEnemy == this.gameObject)
        {
            isDead = true;
            DisableMovement();
        }
    }

    private void DisableMovement()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        else
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] NavMeshAgent is null on {gameObject.name}. Skipping agent disable.");
        }

        if (animator != null)
        {
            animator.SetBool("isDead", true);
            StartCoroutine(WaitForDeathAnimation());
        }
        else
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] Animator is null on {gameObject.name}. Disabling script immediately.");
            this.enabled = false;
        }
    }

    private IEnumerator WaitForDeathAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning($"[{nameof(DashingEnemy)}] Animator is null on {gameObject.name}. Cannot wait for death animation.");
            yield break;
        }

        string deathStateName = "Death";

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(deathStateName))
        {
            yield return null;
        }

        while (animator.GetCurrentAnimatorStateInfo(0).IsName(deathStateName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        this.enabled = false;
    }

}
