using UnityEngine;
public interface IHealth
{
    void TakeDamage(float damage);
    void Heal(float amount);
    float CurrentHealth { get; }
    float MaxHealth { get; }
}

public class Health : MonoBehaviour, IHealth
{
    [Header("Entity Stats")]
    [SerializeField] private EntityStats entityStats;

    [Header("Floating Damage")]
    [Tooltip("Floating damage prefab to display damage taken.")]
    [SerializeField] private GameObject floatingDamagePrefab;

    private float currentHealth;

    public float MaxHealth => entityStats != null ? entityStats.maxHealth : 0;
    public float CurrentHealth => currentHealth;

    public delegate void HealthEvent(float currentHealth, float maxHealth);
    public event HealthEvent OnHealthChanged;
    public event HealthEvent OnHealthDepleted;

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
            OnHealthDepleted?.Invoke(currentHealth, MaxHealth);
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
        gameObject.SetActive(false);
    }
}
