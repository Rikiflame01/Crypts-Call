using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public static class SceneNames
{
    public const string Town = "Town";
}

public class PlayerController : MonoBehaviour
{
    public GenericEventSystem eventSystem;
    [SerializeField] private EntityStats playerStats;
    private PlayerMovement controls;
    private PlayerInput playerInput;

    public GameObject dashEffect;
    private float leftClickStartTime;

    private bool inventoryOpened = false;

    [Header("Enemy Targeting Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask enemyLayer;
    private Transform nearestEnemy;

    [Header("Movement Settings")]
    [SerializeField] private float defaultMoveSpeed = 5f;
    private float currentMoveSpeed;

    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float holdThreshold = 0.2f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.6f;
    [SerializeField] private float dashCooldown = 1.5f;

    [Header("Quick Slash Dash Settings")]
    [SerializeField] private float attackDashSpeed = 5f;
    [SerializeField] private float quickSlashDashCooldown = 2f;
    private float quickSlashDashCooldownTimer = 0f;

    private float quickSlashCooldownTimer = 0f;
    [SerializeField] private float quickSlashCooldown = 1f;

    [Header("Heavy Attack Settings")]
    [SerializeField] private float heavyAttackCooldown = 10f;
    private float heavyAttackCooldownTimer = 0f;

    [SerializeField] private float heavyAttackStaminaCost = 2f;
    [SerializeField] private float heavyAttackSpeedMultiplier = 0.5f;
    [SerializeField] private float heavyAttackShakeAmount = 0.1f;
    [SerializeField] private float heavyAttackShakeDuration = 0.5f;

    [Header("Heavy Attack Dash Damage Settings")]
    [SerializeField] private float heavyAttackDashDamageRadius = 3f;
    [SerializeField] private int heavyAttackDashDamageAmount = 20;

    [SerializeField] private float heavyAttackDashSpeed = 25f;
    [SerializeField] private float heavyAttackDashDuration = 0.3f;

    public bool isPlayerAttacking = false;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    private Camera mainCamera;
    private Vector2 movementInput;

    private Animator animator;
    private string currentSceneName = "";

    private delegate void MovementHandler();
    private MovementHandler HandleCurrentMovement;

    private bool isLeftClickPressed = false;
    private Coroutine heavyAttackAnticipationCoroutine;
    private Coroutine currentShakeCoroutine;

    public AudioSource dashSound;
    private PlayerAttacks playerAttack;

    public int ComboStep => comboStep;
    private int comboStep = 0; 
    [SerializeField] private float comboResetTime = 1f;
    private float lastAttackTime = 0f;

    private PlayerTriggerStunner playerTriggerStunner;

    public TrailRenderer trailRenderer;

    [Header("Heavy Attack Charge UI")]
    [Tooltip("Assign a UI Slider that will fill up when holding LMB for a heavy attack.")]
    public Slider heavyAttackChargeSlider;
    [Tooltip("How quickly the slider drains back to zero if heavy attack is not triggered.")]
    public float sliderResetSpeed = 8f;

    private Coroutine heavyAttackSliderCoroutine;
    private bool heavyAttackFullyCharged = false;


    private void Awake()
    {
        currentMoveSpeed = defaultMoveSpeed;
        playerTriggerStunner = GetComponent<PlayerTriggerStunner>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        if (animator == null) Debug.LogError("Animator component missing from the player.");

        controls = new PlayerMovement();
        InitializeInputActions();
        SceneManager.sceneLoaded += OnSceneLoaded;

        playerAttack = GetComponent<PlayerAttacks>();
        if (playerAttack == null)
        {
            Debug.LogError("PlayerAttack component missing from the player.");
        }

        if (heavyAttackChargeSlider != null)
        {
            heavyAttackChargeSlider.value = 0f;
            heavyAttackChargeSlider.gameObject.SetActive(false);
        }
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

        FindNearestEnemy();
        RotateTowardsEnemy();
    }

    private void InitializeInputActions()
    {
        controls.controls.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        controls.controls.Move.canceled += ctx => movementInput = Vector2.zero;

        controls.controls.LeftClick.started += ctx => OnLeftClickStarted();
        controls.controls.LeftClick.canceled += ctx => OnLeftClickCanceled();

        controls.controls.Spacebar.performed += ctx => AttemptDash();

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
        }
        else
        {
            HandleCurrentMovement = HandleCameraRelativeMovement;
        }
    }

    private void HandleStationaryCameraMovement()
    {
        if (movementInput == Vector2.zero)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        MovePlayerRelativeToCamera();
    }

    private void HandleCameraRelativeMovement()
    {
        if (movementInput == Vector2.zero)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        MovePlayerRelativeToCamera();
    }

    private void MovePlayerRelativeToCamera()
    {
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * movementInput.y + right * movementInput.x).normalized;

        if (!isDashing)
        {
            transform.position += moveDirection * currentMoveSpeed * Time.deltaTime;

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void UpdateAnimations()
    {
        bool isWalking = (movementInput != Vector2.zero && !isDashing);
        animator.SetBool("isWalking", isWalking);
    }

    private void FindNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);

        if (enemies.Length > 0)
        {
            nearestEnemy = enemies
                .Select(e => e.transform)
                .OrderBy(e => Vector3.Distance(transform.position, e.position))
                .FirstOrDefault();
        }
        else
        {
            nearestEnemy = null;
        }
    }

    private void RotateTowardsEnemy()
    {
        if (nearestEnemy == null) return;

        Vector3 direction = (nearestEnemy.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnLeftClickStarted()
    {
        leftClickStartTime = Time.time;
        isLeftClickPressed = true;

        if (heavyAttackCooldownTimer <= 0f)
        {
            heavyAttackFullyCharged = false;
            if (heavyAttackChargeSlider != null)
            {
                heavyAttackChargeSlider.gameObject.SetActive(true);
                heavyAttackChargeSlider.value = 0f;
            }

            heavyAttackSliderCoroutine = StartCoroutine(ChargeHeavyAttackSlider());
        }

        heavyAttackAnticipationCoroutine = StartCoroutine(HeavyAttackAnticipation());
    }

    private IEnumerator ChargeHeavyAttackSlider()
    {
        float elapsed = 0f;

        while (isLeftClickPressed && !heavyAttackFullyCharged && heavyAttackChargeSlider != null)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / holdThreshold);
            heavyAttackChargeSlider.value = fill;

            if (fill >= 1f)
            {
                heavyAttackChargeSlider.value = 1f;
                heavyAttackFullyCharged = true;
            }
            yield return null;
        }

    }

    private void OnLeftClickCanceled()
    {
        float clickDuration = Time.time - leftClickStartTime;
        isLeftClickPressed = false;

        if (heavyAttackSliderCoroutine != null)
        {
            StopCoroutine(heavyAttackSliderCoroutine);
            heavyAttackSliderCoroutine = null;
        }
        if (heavyAttackAnticipationCoroutine != null)
        {
            StopCoroutine(heavyAttackAnticipationCoroutine);
            heavyAttackAnticipationCoroutine = null;
        }


        bool sliderWasFullyCharged = heavyAttackFullyCharged;

        RestorePlayerSpeed();
        StopShake();

        if (sliderWasFullyCharged && clickDuration >= holdThreshold)
        {
            AttemptHeavyAttack();
        }
        else
        {
            if (clickDuration < holdThreshold && playerStats.stamina > 0)
            {
                TriggerQuickSlash();
            }
            StartCoroutine(ResetHeavyAttackSlider());
        }
    }

    private IEnumerator ResetHeavyAttackSlider()
    {
        if (heavyAttackChargeSlider == null) yield break;

        float currentValue = heavyAttackChargeSlider.value;
        while (currentValue > 0f)
        {
            currentValue -= Time.deltaTime * sliderResetSpeed;
            heavyAttackChargeSlider.value = Mathf.Clamp01(currentValue);
            yield return null;
        }

        heavyAttackChargeSlider.value = 0f;
        heavyAttackChargeSlider.gameObject.SetActive(false);
    }

    private IEnumerator HeavyAttackAnticipation()
    {
        yield return new WaitForSeconds(holdThreshold);

        if (isLeftClickPressed)
        {
            currentMoveSpeed *= heavyAttackSpeedMultiplier;
            StartShake();
        }
    }

    private void RestorePlayerSpeed()
    {
        currentMoveSpeed = defaultMoveSpeed;
    }

    private void StartShake()
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }
        currentShakeCoroutine = StartCoroutine(ShakePlayer());
    }

    private void StopShake()
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            currentShakeCoroutine = null;
        }
    }

    private IEnumerator ShakePlayer()
    {
        float elapsed = 0f;
        Vector3 originalPosition = transform.position;

        while (elapsed < heavyAttackShakeDuration)
        {
            float x = Random.Range(-heavyAttackShakeAmount, heavyAttackShakeAmount);
            float y = Random.Range(-heavyAttackShakeAmount, heavyAttackShakeAmount);
            transform.position = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        currentShakeCoroutine = null;
    }

    private void TriggerQuickSlash()
    {
        if (quickSlashCooldownTimer > 0f) return;

        EnableTrail();
        eventSystem.RaiseEvent("Stamina", "Change", -1);
        EventManager.TriggerAbilityUsed("standard", (int)quickSlashCooldown);
        quickSlashCooldownTimer = quickSlashCooldown;
        StartCoroutine(PlayerIsAttacking());

        AttemptQuickSlashDash();

        comboStep++;
        lastAttackTime = Time.time;

        if (comboStep > 3)
        {
            comboStep = 1;
            DisableTrail();
        }

        switch (comboStep)
        {
            case 1:
                animator.SetTrigger("QuickSlash");
                break;
            case 2:
                animator.SetTrigger("Attack2");
                break;
            case 3:
                animator.SetTrigger("Attack3");
                break;
            default:
                animator.SetTrigger("QuickSlash");
                break;
        }
    }

    private IEnumerator PlayerIsAttacking()
    {
        isPlayerAttacking = true;

        if (playerAttack != null)
        {
            playerAttack.PerformQuickSlash();
        }

        yield return new WaitForSeconds(comboResetTime);
        isPlayerAttacking = false;
    }

    private void AttemptHeavyAttack()
    {
        if (heavyAttackChargeSlider != null)
        {
            heavyAttackChargeSlider.value = 1f;
            heavyAttackChargeSlider.gameObject.SetActive(false);
        }

        if (isDashing || heavyAttackCooldownTimer > 0f)
        {
            Debug.Log("Heavy Attack is on cooldown.");
            return;
        }

        if (playerStats.mana < 1)
        {
            Debug.Log("Not enough mana for Heavy Attack.");
            return;
        }

        if (playerStats.stamina < heavyAttackStaminaCost)
        {
            Debug.Log("Not enough stamina for Heavy Attack.");
            return;
        }

        EnableTrail();
        eventSystem.RaiseEvent("Mana", "Change", -1);
        eventSystem.RaiseEvent("Stamina", "Change", (int)-heavyAttackStaminaCost);
        EventManager.TriggerAbilityUsed("heavy", (int)heavyAttackCooldown);

        Vector3 dashDirection = GetMouseDirection();
        if (dashDirection == Vector3.zero)
        {
            dashDirection = transform.forward;
        }

        playerTriggerStunner.enabled = true;
        dashSound.Play();
        foreach (var ps in dashEffect.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }

        StartCoroutine(PerformHeavyAttackDash(dashDirection, heavyAttackDashSpeed, heavyAttackDashDuration));
        heavyAttackCooldownTimer = heavyAttackCooldown;
        DisableTrail();
    }

    private void AttemptDash()
    {
        if (isDashing || dashCooldownTimer > 0 || playerStats.mana <= 0) return;

        Vector3 dashDirection = GetMouseDirection();
        if (dashDirection != Vector3.zero)
        {
            playerTriggerStunner.enabled = true;
            dashSound.Play();
            foreach (var ps in dashEffect.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Play();
            }

            StartCoroutine(PerformDash(dashDirection, dashSpeed));
            dashCooldownTimer = dashCooldown;
            EventManager.TriggerAbilityUsed("dash", 0);
        }
    }

    private void AttemptQuickSlashDash()
    {
        if (isDashing || quickSlashDashCooldownTimer > 0 || playerStats.mana <= 0) return;

        Vector3 dashDirection;
        if (nearestEnemy != null)
        {
            dashDirection = (nearestEnemy.position - transform.position).normalized;
            dashDirection.y = 0f;
        }
        else
        {
            dashDirection = transform.forward;
        }

        if (dashDirection == Vector3.zero)
        {
            dashDirection = transform.forward;
        }

        playerAttack.ApplyHeavyAttackDamage(transform.position, new HashSet<Transform>());
        dashSound.Play();

        StartCoroutine(PerformDash(dashDirection, attackDashSpeed));
        quickSlashDashCooldownTimer = quickSlashDashCooldown;
    }

    private IEnumerator PerformDash(Vector3 dashDirection, float dashSpeedM)
    {
        isDashing = true;
        float startTime = Time.time;

        Light dashLight = dashEffect.GetComponentInChildren<Light>();
        if (dashLight != null && dashSpeedM < 15)
        {
            dashLight.intensity = 15f;
        }

        Vector3 normalizedDashDirection = dashDirection.normalized;
        if (normalizedDashDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(normalizedDashDirection, Vector3.up);
            transform.rotation = targetRotation;
        }

        while (Time.time < startTime + dashDuration)
        {
            transform.position += normalizedDashDirection * dashSpeedM * Time.deltaTime;
            yield return null;
        }

        if (dashLight != null && dashSpeedM < 15)
        {
            dashLight.intensity = 0f;
        }

        isDashing = false;
        StartCoroutine(stunTimerCoRoutine());

        eventSystem.RaiseEvent("Mana", "Change", -1);
    }

    private IEnumerator PerformHeavyAttackDash(Vector3 dashDirection, float dashSpeedH, float dashDurationH)
    {
        isDashing = true;
        float startTime = Time.time;

        Light dashLight = dashEffect.GetComponentInChildren<Light>();
        if (dashLight != null)
        {
            dashLight.intensity = 15f;
        }

        Vector3 normalizedDashDirection = dashDirection.normalized;
        if (normalizedDashDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(normalizedDashDirection, Vector3.up);
            transform.rotation = targetRotation;
        }

        HashSet<Transform> hitEnemies = new HashSet<Transform>();

        animator.SetBool("isHeavyAttacking", true);
        float originalAnimatorSpeed = animator.speed;
        animator.speed = 2f;

        float damageInterval = 0.1f;
        float nextDamageTime = startTime;

        while (Time.time < startTime + dashDurationH)
        {
            transform.position += normalizedDashDirection * dashSpeedH * Time.deltaTime;

            if (Time.time >= nextDamageTime)
            {
                playerAttack.ApplyHeavyAttackDamage(transform.position, hitEnemies);
                nextDamageTime += damageInterval;
            }

            yield return null;
        }

        animator.SetBool("isHeavyAttacking", false);
        animator.speed = originalAnimatorSpeed;

        if (dashLight != null)
        {
            dashLight.intensity = 0f;
        }

        isDashing = false;
        StartCoroutine(stunTimerCoRoutine());

        RestorePlayerSpeed();
    }

    private IEnumerator stunTimerCoRoutine()
    {
        yield return new WaitForSeconds(1f);
        playerTriggerStunner.enabled = false;
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

    private void UpdateCooldownTimers()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (quickSlashDashCooldownTimer > 0)
        {
            quickSlashDashCooldownTimer -= Time.deltaTime;
        }

        if (quickSlashCooldownTimer > 0)
        {
            quickSlashCooldownTimer -= Time.deltaTime;
        }

        if (heavyAttackCooldownTimer > 0)
        {
            heavyAttackCooldownTimer -= Time.deltaTime;
        }

        if (comboStep > 0 && Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }
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

    private IEnumerator Shake()
    {
        float elapsed = 0f;
        Vector3 originalPosition = transform.localPosition;

        while (elapsed < heavyAttackShakeDuration)
        {
            float x = Random.Range(-heavyAttackShakeAmount, heavyAttackShakeAmount);
            float y = Random.Range(-heavyAttackShakeAmount, heavyAttackShakeAmount);
            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    private void EnableTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }
    }

    private void DisableTrail()
    {
        if (trailRenderer != null)
        {
            StartCoroutine(FadeOutTrail());
        }
    }

    private IEnumerator FadeOutTrail()
    {
        yield return new WaitForSeconds(5f);
        trailRenderer.enabled = false;
    }
}
