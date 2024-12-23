#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using UnityEngine;
using System;

public interface IHealth
{
    void TakeDamage(float damage);
    void Heal(float amount);
    float CurrentHealth { get; }
    float MaxHealth { get; }

    bool IsDead { get; }
}

[RequireComponent(typeof(Animator))]
public class Health : MonoBehaviour, IHealth
{
    public event Action<float, float> OnHealthChanged;

    public event Action OnHealthDepleted;

    public event Action<GameObject> OnDied;

    [Header("Entity Stats")]
    [SerializeField] private EntityStats entityStats;

    [Header("Floating Damage")]
    [Tooltip("Floating damage prefab to display damage taken.")]
    [SerializeField] private GameObject floatingDamagePrefab;

    private float currentHealth;

    public float MaxHealth => entityStats != null ? entityStats.maxHealth : 0;
    public float CurrentHealth => currentHealth;

    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        if (entityStats == null)
        {
            Debug.LogWarning("No EntityStats assigned to " + gameObject.name);
            return;
        }

        currentHealth = entityStats.health;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0 || entityStats == null) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        Debug.Log($"{gameObject.name} took {damage} damage.");

        if (floatingDamagePrefab != null)
        {
            ShowFloatingDamage(damage);
        }

        if (currentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} has died.");
            OnHealthDepleted?.Invoke();
            HandleDeath();
        }
    }

    private void ShowFloatingDamage(float damage)
    {
        GameObject damageTextInstance = Instantiate(floatingDamagePrefab, transform.position, Quaternion.identity);

        FloatingDamage floatingDamage = damageTextInstance.GetComponent<FloatingDamage>();
        if (floatingDamage != null)
        {
            floatingDamage.SetDamageText(damage);
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0 || entityStats == null) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    protected virtual void HandleDeath()
    {
        OnDied?.Invoke(gameObject);

    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    public void EditorDamage(float damage)
    {
        TakeDamage(damage);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Health))]
public class HealthEditor : Editor
{
    private float damageAmount = 10f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Health health = (Health)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Options", EditorStyles.boldLabel);

        damageAmount = EditorGUILayout.FloatField("Damage Amount", damageAmount);

        if (GUILayout.Button("Apply Damage"))
        {
            health.EditorDamage(damageAmount);
            EditorUtility.SetDirty(health);
        }
    }
}
#endif
