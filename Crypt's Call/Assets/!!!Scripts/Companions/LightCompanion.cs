using UnityEngine;
using System.Collections;

public class LightCompanion : MonoBehaviour
{
    private Light lightComponent;

    [Header("Idle Light Settings")]
    public float idleMinRange = 1f;
    public float idleMaxRange = 5f;
    public float idleMinIntensity = 1f;
    public float idleMaxIntensity = 5f;

    [Header("Active Light Settings")]
    public float activeMinRange = 10f;
    public float activeMaxRange = 15f;
    public float activeMinIntensity = 15f;
    public float activeMaxIntensity = 20f;

    public float lightLerpSpeed = 1f;

    [Header("Hover Settings")]
    public float hoverHeight = 2f;
    public float hoverSpeed = 5f;
    public Vector3 idleHoverOffset = Vector3.zero;

    [Header("Oscillation Settings")]
    public float oscillationSpeed = 1f;
    public float oscillationAmount = 0.5f;

    [Header("Detection Settings")]
    public float playerDetectionRadius = 10f;
    public LayerMask playerLayer;
    public float enemyDetectionRadius = 15f;
    public LayerMask enemyLayer;

    [Header("Safety Settings")]
    public float maxDistanceFromPlayer = 20f;

    [Header("Y-Height Settings")]
    public float yHeightTolerance = 0.5f;

    private Transform playerTransform;
    private Transform currentTarget;
    private bool isFollowingEnemy = false;
    private Rigidbody rb;
    private Vector3 initialIdlePosition;

    private enum State { Idle, FollowingPlayer, FollowingEnemy }
    private State currentState = State.Idle;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        if (lightComponent == null)
        {
            Debug.LogError("LightCompanion requires a Light component.");
            enabled = false;
            return;
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("LightCompanion requires a Rigidbody component.");
            enabled = false;
            return;
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        initialIdlePosition = transform.position + idleHoverOffset;

        lightComponent.range = idleMinRange;
        lightComponent.intensity = idleMinIntensity;

        StartCoroutine(FluctuateLight());

        Debug.Log("LightCompanion initialized and hovering at initial position.");
    }

    void FixedUpdate()
    {
        DetectPlayer();

        SafetyTeleportBack();

        switch (currentState)
        {
            case State.Idle:
                Hover(initialIdlePosition);
                break;

            case State.FollowingPlayer:
                if (playerTransform != null)
                {
                    Vector3 targetPosition = playerTransform.position + Vector3.up * hoverHeight;
                    Hover(targetPosition);
                    DetectEnemies();
                }
                break;

            case State.FollowingEnemy:
                if (currentTarget != null)
                {
                    Vector3 targetPosition = currentTarget.position + Vector3.up * hoverHeight;
                    Hover(targetPosition);

                    if (!currentTarget.gameObject.activeInHierarchy)
                    {
                        Debug.Log("Enemy disabled. Returning to FollowingPlayer state.");
                        isFollowingEnemy = false;
                        currentState = State.FollowingPlayer;
                        currentTarget = null;
                    }
                }
                else
                {
                    Debug.Log("No current target. Reverting to FollowingPlayer state.");
                    currentState = State.FollowingPlayer;
                }
                break;
        }
    }

    void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, playerDetectionRadius, playerLayer);
        if (hits.Length > 0)
        {
            if (playerTransform == null)
            {
                playerTransform = hits[0].transform;
                Debug.Log("Player detected and assigned to LightCompanion.");
            }

            if (currentState != State.FollowingEnemy)
            {
                if (currentState != State.FollowingPlayer)
                {
                    Debug.Log("Switching to FollowingPlayer state.");
                }
                currentState = State.FollowingPlayer;
                currentTarget = playerTransform;
            }
        }
        else
        {
            if (currentState != State.FollowingEnemy)
            {
                if (currentState != State.Idle)
                {
                    Debug.Log("No player detected. Switching to Idle state.");
                }
                currentState = State.Idle;
                currentTarget = null;
            }
        }
    }

    void DetectEnemies()
    {
        if (isFollowingEnemy)
            return;

        if (playerTransform == null)
            return;

        Collider[] enemies = Physics.OverlapSphere(transform.position, enemyDetectionRadius, enemyLayer);
        foreach (Collider enemy in enemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                float yDifference = Mathf.Abs(enemy.transform.position.y - playerTransform.position.y);
                if (yDifference <= yHeightTolerance)
                {
                    currentTarget = enemy.transform;
                    currentState = State.FollowingEnemy;
                    isFollowingEnemy = true;
                    Debug.Log($"Enemy detected and followed: {enemy.gameObject.name}");
                    break;
                }
                else
                {
                    Debug.Log($"Enemy {enemy.gameObject.name} ignored due to Y-height difference: {yDifference}");
                }
            }
        }
    }

    IEnumerator FluctuateLight()
    {
        while (true)
        {
            float targetRange;
            float targetIntensity;

            if (currentState == State.Idle)
            {
                targetRange = Random.Range(idleMinRange, idleMaxRange);
                targetIntensity = Random.Range(idleMinIntensity, idleMaxIntensity);
            }
            else
            {
                targetRange = Random.Range(activeMinRange, activeMaxRange);
                targetIntensity = Random.Range(activeMinIntensity, activeMaxIntensity);
            }

            float initialRange = lightComponent.range;
            float initialIntensity = lightComponent.intensity;
            float elapsed = 0f;

            while (elapsed < lightLerpSpeed)
            {
                lightComponent.range = Mathf.Lerp(initialRange, targetRange, elapsed / lightLerpSpeed);
                lightComponent.intensity = Mathf.Lerp(initialIntensity, targetIntensity, elapsed / lightLerpSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            lightComponent.range = targetRange;
            lightComponent.intensity = targetIntensity;

            yield return new WaitForSeconds(1f);
        }
    }

    void Hover(Vector3 targetPosition)
    {
        Vector3 oscillation = Vector3.up * Mathf.Sin(Time.time * oscillationSpeed) * oscillationAmount;

        Vector3 finalTarget = targetPosition + oscillation;

        Vector3 newPosition = Vector3.Lerp(rb.position, finalTarget, hoverSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
    }

    void SafetyTeleportBack()
    {
        if (currentState == State.Idle)
            return;

        if (playerTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        Debug.Log($"SafetyTeleportBack: Distance to player is {distance}.");

        if (distance > maxDistanceFromPlayer)
        {
            Vector3 teleportPosition = playerTransform.position + Vector3.up * hoverHeight;
            rb.MovePosition(teleportPosition);

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Debug.Log("LightCompanion teleported back to the player due to exceeding max distance.");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyDetectionRadius);

        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, maxDistanceFromPlayer);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
        }
    }
}
