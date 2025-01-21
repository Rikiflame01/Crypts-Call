using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Music Settings")]
    public AudioSource musicSource;
    public AudioSource queuedMusicSource;
    public AudioClip[] musicPlaylist;

    [SerializeField] private AudioClip[] deathSoundSFX;
    [SerializeField] private AudioSource deathAudioSource;
    public int currentTrackIndex = 0;
    public float maxVolume = 1f;
    public float minVolume = 0f;
    public float fadeDuration = 2f;

    [Header("Distance Settings")]
    public float playableDistance = 50f;
    public float pauseDistance = 100f;
    public float yOffset = 1f;

    [Header("References")]
    public Transform playerTransform;
    public LayerMask enemyLayer;

    [Header("SFX Settings")]
    public List<SFX> sfxList;

    private Coroutine volumeCoroutine;
    private Coroutine fadeCoroutine;
    private Coroutine trackMonitorCoroutine;
    private bool isMusicPlaying = false;
    private bool isFading = false;

    void Awake()
    {
        // Implement Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize SFX AudioSources
        InitializeSFX();
    }

    void Start()
    {
        if (musicSource == null)
        {
            Debug.LogWarning("Music Source is not assigned.");
            return;
        }

        if (queuedMusicSource == null)
        {
            Debug.LogWarning("Queued Music Source is not assigned.");
            return;
        }

        if (musicPlaylist == null || musicPlaylist.Length == 0)
        {
            Debug.LogWarning("Music Playlist is empty.");
            return;
        }

        musicSource.clip = musicPlaylist[currentTrackIndex];
        musicSource.volume = 0f;
        musicSource.Play();
        StartFade(musicSource, 0f, maxVolume);
        isMusicPlaying = true;

        StartTrackMonitor();
    }

    void Update()
    {
        UpdateMusicState();
    }

    #region Music

    void UpdateMusicState()
    {
        float closestDistance = GetClosestEnemyDistance();

        if (closestDistance == Mathf.Infinity)
        {
            if (isMusicPlaying)
            {
                StartFadeOutAndQueue();
                isMusicPlaying = false;
            }
        }
        else
        {
            if (!isMusicPlaying && queuedMusicSource.clip != null)
            {
                PlayQueuedMusic();
                isMusicPlaying = true;
            }

            float targetVolume = Mathf.Lerp(minVolume, maxVolume, Mathf.Clamp01((pauseDistance - closestDistance) / (pauseDistance - playableDistance)));
            if (volumeCoroutine != null)
                StopCoroutine(volumeCoroutine);
            volumeCoroutine = StartCoroutine(ChangeVolume(musicSource, targetVolume));
        }
    }

    float GetClosestEnemyDistance()
    {
        Collider[] enemies = Physics.OverlapSphere(playerTransform.position, pauseDistance, enemyLayer);
        float minDistance = Mathf.Infinity;

        foreach (Collider enemy in enemies)
        {
            float verticalDifference = enemy.transform.position.y - playerTransform.position.y;

            if (verticalDifference < -yOffset)
            {
                continue;
            }

            Vector3 playerPos = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
            Vector3 enemyPos = new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z);
            float distance = Vector3.Distance(playerPos, enemyPos);

            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    IEnumerator ChangeVolume(AudioSource source, float targetVolume)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        source.volume = targetVolume;
    }

    void StartFade(AudioSource source, float startVol, float endVol)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(Fade(source, startVol, endVol));
    }

    IEnumerator Fade(AudioSource source, float startVol, float endVol)
    {
        isFading = true;
        source.volume = startVol;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            source.volume = Mathf.Lerp(startVol, endVol, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        source.volume = endVol;
        isFading = false;
    }

    void StartFadeOutAndQueue()
    {
        if (isFading)
            StopCoroutine(fadeCoroutine);

        if (trackMonitorCoroutine != null)
            StopCoroutine(trackMonitorCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutAndQueueCoroutine());
    }

    IEnumerator FadeOutAndQueueCoroutine()
    {
        isFading = true;
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = maxVolume;

        QueueNextTrack();

        isFading = false;

        if (trackMonitorCoroutine != null)
        {
            StopCoroutine(trackMonitorCoroutine);
            trackMonitorCoroutine = null;
        }
    }

    void QueueNextTrack()
    {
        currentTrackIndex++;

        if (currentTrackIndex >= musicPlaylist.Length)
        {
            currentTrackIndex = 0;
        }

        if (musicPlaylist.Length > 1 && musicPlaylist[currentTrackIndex] == musicSource.clip)
        {
            currentTrackIndex = (currentTrackIndex + 1) % musicPlaylist.Length;
            Debug.Log("Next track is same as current. Skipping to avoid repeat.");
        }

        queuedMusicSource.clip = musicPlaylist[currentTrackIndex];
        queuedMusicSource.volume = 0f;
    }

    void PlayQueuedMusic()
    {
        if (queuedMusicSource.clip == null)
        {
            return;
        }

        AudioClip nextClip = queuedMusicSource.clip;
        queuedMusicSource.clip = null;

        musicSource.clip = nextClip;
        musicSource.Play();

        StartFade(musicSource, 0f, maxVolume);

        StartTrackMonitor();

        QueueNextTrack();
    }

    public void OnPlayerEngageEnemies()
    {
        if (!isMusicPlaying && queuedMusicSource.clip != null)
        {
            Debug.Log("Player engaged with enemies. Playing queued music.");
            PlayQueuedMusic();
            isMusicPlaying = true;
        }
    }

    void StartTrackMonitor()
    {
        if (trackMonitorCoroutine != null)
            StopCoroutine(trackMonitorCoroutine);

        trackMonitorCoroutine = StartCoroutine(MonitorTrack());
    }

    IEnumerator MonitorTrack()
    {
        while (musicSource.isPlaying)
        {
            yield return null;
        }

        Debug.Log($"Track ended: {musicSource.clip.name}");

        float closestDistance = GetClosestEnemyDistance();

        if (closestDistance != Mathf.Infinity)
        {
            PlayQueuedMusic();
            isMusicPlaying = true;
        }
        else
        {
            QueueNextTrack();
            isMusicPlaying = false;
        }
    }

    #endregion

    #region SFX

    [System.Serializable]
    public class SFX
    {
        public string name;     
        public AudioClip clip;     
        [HideInInspector]
        public AudioSource source; 
    }

    void InitializeSFX()
    {
        foreach (var sfx in sfxList)
        {
            if (sfx.clip == null || string.IsNullOrEmpty(sfx.name))
            {
                Debug.LogWarning("SFX entry is missing a name or AudioClip.");
                continue;
            }

            GameObject sfxObject = new GameObject($"SFX_{sfx.name}");
            sfxObject.transform.parent = this.transform;

            sfx.source = sfxObject.AddComponent<AudioSource>();
            sfx.source.clip = sfx.clip;
            sfx.source.playOnAwake = false;
            sfx.source.spatialBlend = 0f;

        }
    }

    public void PlaySFX(string sfxName)
    {
        SFX sfx = sfxList.Find(s => s.name.Equals(sfxName, System.StringComparison.OrdinalIgnoreCase));
        if (sfx != null && sfx.source != null)
        {
            sfx.source.Play();
        }
        else
        {
            Debug.LogWarning($"SFX '{sfxName}' not found or AudioSource is missing.");
        }
    }

    public void PlaySFX(string sfxName, float volume)
    {
        SFX sfx = sfxList.Find(s => s.name.Equals(sfxName, System.StringComparison.OrdinalIgnoreCase));
        if (sfx != null && sfx.source != null)
        {
            sfx.source.volume = Mathf.Clamp01(volume);
            sfx.source.Play();
        }
        else
        {
            Debug.LogWarning($"SFX '{sfxName}' not found or AudioSource is missing.");
        }
    }


    public void PlaySFX(string sfxName, Vector3 position)
    {
        SFX sfx = sfxList.Find(s => s.name.Equals(sfxName, System.StringComparison.OrdinalIgnoreCase));
        if (sfx != null && sfx.clip != null)
        {
            AudioSource.PlayClipAtPoint(sfx.clip, position);
        }
        else
        {
            Debug.LogWarning($"SFX '{sfxName}' not found or AudioClip is missing.");
        }
    }

    public void PlayDeathSound(GameObject target)
    {
        if (target.layer == LayerMask.NameToLayer("Enemy") && target.CompareTag("NormalEnemy"))
        {
            if (deathSoundSFX.Length > 0 && deathAudioSource != null)
            {
                AudioClip randomClip = deathSoundSFX[Random.Range(0, deathSoundSFX.Length)];
                deathAudioSource.PlayOneShot(randomClip, 2);
            }
            else
            {
                Debug.LogWarning("Death sound SFX array is empty or AudioSource is not assigned.");
            }
        }
    }

    #endregion
}
