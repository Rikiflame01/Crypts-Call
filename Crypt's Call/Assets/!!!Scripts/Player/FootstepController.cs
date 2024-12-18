using UnityEngine;
using UnityEngine.AI;

public class FootstepController : MonoBehaviour
{
    public AudioClip[] footstepClips;
    public AudioSource audioSource;
    public float stepInterval = 0.5f;

    private NavMeshAgent agent;
    private float stepTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (IsAgentMoving())
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private bool IsAgentMoving()
    {
        return agent.velocity.magnitude > 0.1f && !agent.isStopped;
    }

    private void PlayFootstepSound()
    {
        if (footstepClips.Length > 0)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}
