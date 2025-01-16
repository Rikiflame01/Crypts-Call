#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using UnityEngine;
using System;

public interface IHealth
{
    void TakeDamage(int damage);
    void Heal(int amount);
    int CurrentHealth { get; }
    int MaxHealth { get; }

    bool IsDead { get; }

    event Action<int, int> OnHealthChanged;

    event Action<GameObject> OnDied;
}

[RequireComponent(typeof(Animator))]
public class Health : MonoBehaviour, IHealth
{
    [SerializeField] private GenericEventSystem eventSystem;

    public event Action<int, int> OnHealthChanged;

    public event Action OnHealthDepleted;

    public event Action<GameObject> OnDied;

    [Header("Entity Stats")]
    [SerializeField] private EntityStats entityStats;

    [Header("Floating Damage")]
    [Tooltip("Floating damage prefab to display damage taken.")]
    [SerializeField] private GameObject floatingDamagePrefab;

    EnemyDrop enemyDrop;

    private int currentHealth;

    public int MaxHealth => entityStats != null ? entityStats.maxHealth : 0;
    public int CurrentHealth => currentHealth;

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

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || entityStats == null || IsDead == true) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        eventSystem.RaiseEvent("Colour","Change", this.gameObject);

        if (floatingDamagePrefab != null)
        {
            ShowFloatingDamage(damage);
        }

        if (currentHealth <= 0)
        {
            OnHealthDepleted?.Invoke();
            HandleDeath();
        }
    }

    private void ShowFloatingDamage(int damage)
    {
        GameObject damageTextInstance = Instantiate(floatingDamagePrefab, transform.position, Quaternion.identity);

        FloatingDamage floatingDamage = damageTextInstance.GetComponent<FloatingDamage>();
        if (floatingDamage != null)
        {
            floatingDamage.SetDamageText(damage);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || entityStats == null) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        if (currentHealth > MaxHealth)
        {
            currentHealth = MaxHealth;
        }
        entityStats.health = currentHealth;

        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    protected virtual void HandleDeath()
    {
        if (gameObject.CompareTag("BossEnemy"))
        {
            eventSystem.RaiseEvent("BossDeath", "DisplayEndOfDemo");
        }
        OnDied?.Invoke(gameObject);

        if (gameObject.name == "Player")
        {
            GameObject[] Keys = GameObject.FindGameObjectsWithTag("Key");
            if (Keys.Length >0)
            {
                foreach (var key in Keys)
                {
                    eventSystem.RaiseEvent("ItemDrop", "Key");
                    Destroy(key);
                } 
            }
            eventSystem.RaiseEvent("Player", "PlayerDeath");
            eventSystem.RaiseEvent("PlayerUI", "DeathCanvas");
            eventSystem.RaiseEvent("Player", "PlayerStatsReset");
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    public void EditorDamage(int damage)
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
            health.EditorDamage((int)damageAmount);
            EditorUtility.SetDirty(health);
        }
    }
}
#endif
