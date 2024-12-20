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

    private enum ExtendedState
    {
        Idle,
        Patrol,
        PlayerDetected
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
        }

        CheckPlayerDetection();
    }

    private new void CheckPlayerDetection()
    {
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
            else
            {
                agent.SetDestination(transform.position);
                FacePlayer();
                HandleFiring();
            }
        }
        else
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
        if (detectedPlayer != null)
        {
            Vector3 directionToPlayer = (detectedPlayer.transform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            }
        }
    }

    private void HandleFiring()
    {
        if (shotsRemainingInBurst <= 0)
        {
            nextBurstTimer -= Time.deltaTime;
            if (nextBurstTimer <= 0f)
            {
                shotsRemainingInBurst = shotsPerBurst;
                shotTimer = 0f;
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
}
