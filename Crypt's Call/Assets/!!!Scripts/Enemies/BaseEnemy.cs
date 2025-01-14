using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public interface IEnemy
{
    bool IsAttacking { get; }
}


public class BaseEnemy : MonoBehaviour, IEnemy
{

    protected bool isAttacking = false;
    protected bool isDead = false;

    Rigidbody rb;
    [Header("Detection")]
    public float detectionRadius = 10f;
    public LayerMask playerLayer;

    [Header("Patrol")]
    public Vector3 patrolAreaCenter;
    public float patrolAreaRadius = 20f;
    public float minPatrolWait = 2f;
    public float maxPatrolWait = 5f;

    protected NavMeshAgent agent;
    protected GameObject detectedPlayer;

    private enum State
    {
        Idle,
        Patrol,
        PlayerDetected
    }

    private State currentState = State.Idle;
    private bool hasStartedPatrolling = false;
    public float patrolWaitTime;

    private Vector3[] patrolPoints;

    public Health health;
    public Animator animator;

    public virtual bool IsAttacking { get; protected set; }


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();

        health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError("BaseEnemy requires a Health component on the same GameObject.");
        }

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        InitializePatrolPoints();
    }

    protected virtual void OnEnable()
    {
        if (health != null)
        {
            health.OnDied += HandleEnemyDeath;
        }
    }

    protected virtual void Start()
    {
        EnterPatrolState();
        hasStartedPatrolling = true;
    }

    protected virtual void Update()
    {
        if (isDead)
        return;

        switch (currentState)
        {
            case State.Idle:
                IdleUpdate();
                break;
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.PlayerDetected:
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

    private void CheckPlayerDetection()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hits.Length > 0)
        {
            detectedPlayer = hits[0].gameObject;
            if (currentState != State.PlayerDetected)
            {
                EnterPlayerDetectedState();
            }
        }
        else
        {
            detectedPlayer = null;
            if (currentState == State.PlayerDetected)
            {
                EnterPatrolState();
            }
        }
    }

    protected virtual void IdleUpdate()
    {
        if (!hasStartedPatrolling)
        {
            EnterPatrolState();
            hasStartedPatrolling = true;
        }
    }

    public void EnterPatrolState()
    {
        currentState = State.Patrol;
        agent.isStopped = false;
        SetNextPatrolDestination();
    }

    protected virtual void PatrolUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (patrolWaitTime > 0f)
            {
                patrolWaitTime -= Time.deltaTime;
            }
            else
            {
                SetNextPatrolDestination();
            }
        }
    }

    private void InitializePatrolPoints()
    {
        float halfSide = patrolAreaRadius;
        Vector3 topLeft = patrolAreaCenter + new Vector3(-halfSide, 0f, halfSide);
        Vector3 topRight = patrolAreaCenter + new Vector3(halfSide, 0f, halfSide);
        Vector3 bottomLeft = patrolAreaCenter + new Vector3(-halfSide, 0f, -halfSide);
        Vector3 bottomRight = patrolAreaCenter + new Vector3(halfSide, 0f, -halfSide);
        Vector3 center = patrolAreaCenter;

        patrolPoints = new Vector3[] { center, topLeft, topRight, bottomLeft, bottomRight };
    }

    public void SetNextPatrolDestination()
    {
        int[] indices = {0, 1, 2, 3, 4};
        Shuffle(indices);

        Vector3 currentPos = transform.position;

        foreach (int i in indices)
        {
            Vector3 point = patrolPoints[i];
            NavMeshHit hit;
            if (NavMesh.SamplePosition(point, out hit, 2f, NavMesh.AllAreas))
            {
                if (!Physics.CheckSphere(hit.position, 0.5f, LayerMask.GetMask("Obstacle")) &&
                    Vector3.Distance(currentPos, hit.position) > 2f)
                {
                    NavMeshPath path = new NavMeshPath();
                    if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.SetDestination(hit.position);
                        patrolWaitTime = UnityEngine.Random.Range(minPatrolWait, maxPatrolWait);
                        return;
                    }
                }
            }
        }

        Debug.LogWarning("Failed to find a valid patrol point from the predefined points.");
        patrolWaitTime = UnityEngine.Random.Range(minPatrolWait, maxPatrolWait);
    }

    private void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    private void EnterPlayerDetectedState()
    {
        currentState = State.PlayerDetected;
    }

    protected virtual void PlayerDetectedUpdate()
    {
        if (detectedPlayer)
        {
            agent.isStopped = false;
            agent.SetDestination(detectedPlayer.transform.position);
        }
        else
        {
            EnterPatrolState();
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(patrolAreaCenter, new Vector3(patrolAreaRadius * 2, 0.1f, patrolAreaRadius * 2));

        if (Application.isPlaying && patrolPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (var p in patrolPoints)
            {
                Gizmos.DrawWireSphere(p, 0.5f);
            }
        }
    }

    protected virtual void HandleEnemyDeath(GameObject deadEnemy)
    {
        if (deadEnemy == this.gameObject)
        {
            Debug.Log($"{gameObject.name}: Detected own death. Disabling movement script.");
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
    
    animator.SetBool("isDead", true);
    StartCoroutine(WaitForDeathAnimation());

}

private IEnumerator WaitForDeathAnimation()
{
    string deathStateName = "Death";
    
    while (!animator.GetCurrentAnimatorStateInfo(0).IsName(deathStateName))
    {
        yield return null;
    }

    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    float animationLength = stateInfo.length;
    float normalizedTime = stateInfo.normalizedTime;

    while (animator.GetCurrentAnimatorStateInfo(0).IsName(deathStateName) &&
           animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
    {
        yield return null;
    }

    yield return new WaitForSeconds(2f);

    this.enabled = false;
}


    protected virtual void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= HandleEnemyDeath;
        }        
        gameObject.SetActive(false);

    }
}
