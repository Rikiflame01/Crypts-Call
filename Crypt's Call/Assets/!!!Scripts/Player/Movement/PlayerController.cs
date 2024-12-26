using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class SceneNames
{
    public const string Town = "Town";
}

public class PlayerController : MonoBehaviour
{
    private PlayerMovement controls;
    private PlayerInput playerInput;

    private float leftClickStartTime;

    private bool inventoryOpened = false;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float holdThreshold = 0.2f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.5f;

    public bool isPlayerAttacking = false;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    private NavMeshAgent agent;
    private Camera mainCamera;
    private Vector2 movementInput;

    private Animator animator;
    private string currentSceneName = "";

    private delegate void MovementHandler();
    private MovementHandler HandleCurrentMovement;

    private bool isLeftClickPressed = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        if (agent == null) Debug.LogError("NavMeshAgent component missing from the player.");
        if (animator == null) Debug.LogError("Animator component missing from the player.");
        
        controls = new PlayerMovement();
        InitializeInputActions();
        SceneManager.sceneLoaded += OnSceneLoaded;

        agent.speed = moveSpeed;
    }

    private void OnEnable() => controls.controls.Enable();
    private void OnDisable() => controls.controls.Disable();

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found.");
        }

        currentSceneName = SceneManager.GetActiveScene().name;
        SetMovementHandler(currentSceneName);
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Update()
    {
        HandleCurrentMovement?.Invoke();
        UpdateAnimations();
        UpdateCooldownTimers();
    }

    private void InitializeInputActions()
    {
        controls.controls.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        controls.controls.Move.canceled += ctx => movementInput = Vector2.zero;

        controls.controls.LeftClick.started += ctx => OnLeftClickStarted();
        controls.controls.LeftClick.canceled += ctx => OnLeftClickCanceled();

        controls.controls.RightClick.performed += ctx => AttemptDash();

        controls.controls.OpenInventory.performed += ctx => ToggleInventory();
        controls.controls.SetupCamp.performed += ctx => SetupCamp();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        SetMovementHandler(currentSceneName);
    }

    private void SetMovementHandler(string sceneName)
    {
        if (sceneName.Equals(SceneNames.Town, System.StringComparison.OrdinalIgnoreCase))
        {
            HandleCurrentMovement = HandleStationaryCameraMovement;
            Debug.Log("Switched to Town Movement (Stationary Camera Relative)");
        }
        else
        {
            HandleCurrentMovement = HandleCameraRelativeMovement;
            Debug.Log("Switched to Normal Movement (Camera Relative)");
        }
    }

    private void HandleStationaryCameraMovement()
    {
        if (movementInput == Vector2.zero)
        {
            agent.ResetPath();              
            agent.velocity = Vector3.zero; 
            animator.SetBool("isWalking", false);
            return;
        }

        MoveAgentRelativeToCamera();
    }


    private void HandleCameraRelativeMovement()
    {
        if (movementInput == Vector2.zero)
        {
            agent.ResetPath();              
            agent.velocity = Vector3.zero; 
            animator.SetBool("isWalking", false);
            return;
        }

        MoveAgentRelativeToCamera();
    }


    private void MoveAgentRelativeToCamera()
    {
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * movementInput.y + right * movementInput.x).normalized;

        Vector3 targetPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        agent.SetDestination(targetPosition);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

private void UpdateAnimations()
{
    bool isWalking = movementInput != Vector2.zero && !agent.isStopped;
    animator.SetBool("isWalking", isWalking);
}


    private void OnLeftClickStarted()
    {
        leftClickStartTime = Time.time; 
    }
    private void OnLeftClickCanceled()
    {
        float clickDuration = Time.time - leftClickStartTime;

        if (clickDuration < holdThreshold)
        {
            TriggerQuickSlash();
        }
        else
        {
            Debug.Log("Hold detected - Heavy attack implemented here soon-ish?");
        }
    }

    private void TriggerQuickSlash()
    {
        if (animator.GetBool("isWalking"))
        {
            Debug.Log("Cannot attack while walking.");
            return;
        }

        animator.SetTrigger("QuickSlash");
        Debug.Log("Quick Slash Triggered!");
        StartCoroutine(PlayerIsAttacking());
    }

    private IEnumerator PlayerIsAttacking()
    {
        isPlayerAttacking = true;
        yield return new WaitForSeconds(1.5f);
        isPlayerAttacking = false;
    }

    private void UpdateCooldownTimers()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
    private void AttemptDash()
    {
        if (isDashing || dashCooldownTimer > 0) return;

        Vector3 dashDirection = GetMouseDirection();
        if (dashDirection != Vector3.zero)
        {
            StartCoroutine(PerformDash(dashDirection));
        }
    }

        private IEnumerator PerformDash(Vector3 dashDirection)
    {
        isDashing = true;
        float startTime = Time.time;

        agent.ResetPath();
        agent.isStopped = true;

        while (Time.time < startTime + dashDuration)
        {
            agent.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        agent.isStopped = false;
        isDashing = false;

        dashCooldownTimer = dashCooldown;
    }

    private Vector3 GetMouseDirection()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = (hit.point - transform.position).normalized;
            direction.y = 0f;
            return direction;
        }
        return Vector3.zero;
    }

    private void ToggleInventory()
    {
        inventoryOpened = !inventoryOpened;
        Debug.Log($"Inventory Opened: {inventoryOpened}");
    }

    private void SetupCamp()
    {
        Debug.Log("Setup Camp Initiated");
    }

    public void HandleDeath()
    {
        animator.SetBool("isDead", true);
    }
}
