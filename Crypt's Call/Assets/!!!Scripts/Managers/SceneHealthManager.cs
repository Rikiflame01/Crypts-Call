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
    private List<Health> healthComponents = new List<Health>();

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
        if (!healthComponents.Contains(health))
        {
            healthComponents.Add(health);
            health.OnHealthChanged += (current, max) => PlayHurtSound(health);
        }
    }

    public void UnregisterHealthComponent(Health health)
    {
        if (healthComponents.Contains(health))
        {
            healthComponents.Remove(health);
        }
    }

    private void RefreshHealthComponents()
    {
        healthComponents.Clear();
        var foundComponents = FindObjectsByType<Health>(FindObjectsSortMode.None);
        foreach (var health in foundComponents)
        {
            RegisterHealthComponent(health);
        }
    }

    private void PlayHurtSound(Health health)
    {
        if (health.IsDead) return;

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
}
