using UnityEngine;
using System.Collections;

public class PlayRandomSound : MonoBehaviour
{
    public AudioClip soundEffect;
    public AudioSource audioSource;
    public float playChance = 0.02f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        StartCoroutine(PlaySoundWithChance());
    }

    private IEnumerator PlaySoundWithChance()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (Random.value <= playChance)
            {
                PlaySound();
            }
        }
    }

    private void PlaySound()
    {
        if (soundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundEffect);
        }
        else
        {
            Debug.LogWarning("AudioSource or SoundEffect is not assigned!");
        }
    }
}
