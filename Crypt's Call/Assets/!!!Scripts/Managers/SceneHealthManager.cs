using System.Collections.Generic;
using UnityEngine;

public class SceneHealthManager : MonoBehaviour
{
    public static SceneHealthManager Instance { get; private set; }

    [Header("Hurt Sounds")]
    [SerializeField] private AudioClip playerHurtSound;
    [SerializeField] private AudioClip normalEnemyHurtSound;
    [SerializeField] private AudioClip bossEnemyHurtSound;

    private AudioSource audioSource;
    private Dictionary<Health, float> previousHealthValues = new Dictionary<Health, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        RefreshHealthComponents();
    }

    public void RegisterHealthComponent(Health health)
    {
        if (!previousHealthValues.ContainsKey(health))
        {
            previousHealthValues[health] = health.CurrentHealth;
            health.OnHealthChanged += (current, max) => PlayHurtSound(health, current);
        }
    }

    public void UnregisterHealthComponent(Health health)
    {
        if (previousHealthValues.ContainsKey(health))
        {
            previousHealthValues.Remove(health);
        }
    }

    private void RefreshHealthComponents()
    {
        previousHealthValues.Clear();
        var foundComponents = FindObjectsByType<Health>(FindObjectsSortMode.None);
        foreach (var health in foundComponents)
        {
            RegisterHealthComponent(health);
        }
    }

    private void PlayHurtSound(Health health, float currentHealth)
    {
        if (health.IsDead) return;

        if (previousHealthValues.TryGetValue(health, out float previousHealth) && currentHealth < previousHealth)
        {
            AudioClip hurtSound = null;
            float volume = 0.5f;

            if (health.CompareTag("Player"))
            {
                hurtSound = playerHurtSound;
                volume = 1.2f;
            }
            else if (health.CompareTag("BossEnemy"))
            {
                hurtSound = bossEnemyHurtSound;
            }
            else if (health.CompareTag("NormalEnemy"))
            {
                hurtSound = normalEnemyHurtSound;
            }

            if (hurtSound != null)
            {
                audioSource.PlayOneShot(hurtSound, volume);
            }
        }

        previousHealthValues[health] = currentHealth;
    }
}
