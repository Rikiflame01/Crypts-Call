using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : MonoBehaviour
{
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
    public float patrolWaitTime;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        EnterPatrolState();
    }

    protected virtual void Update()
    {
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
        
        CheckPlayerDetection();
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
        EnterPatrolState();
    }

    private void EnterPatrolState()
    {
        currentState = State.Patrol;
        agent.isStopped = false;
        SetRandomPatrolDestination();
    }

    protected virtual void PatrolUpdate()
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

    public void SetRandomPatrolDestination()
    {
        Vector3 randomPoint = patrolAreaCenter + Random.insideUnitSphere * patrolAreaRadius;
        randomPoint.y = patrolAreaCenter.y;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolAreaRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        patrolWaitTime = Random.Range(minPatrolWait, maxPatrolWait);
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

    //Gizmos to visualize detection and patrol area
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(patrolAreaCenter, patrolAreaRadius);
    }
}
