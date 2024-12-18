using UnityEngine;
using UnityEngine.AI;

public class TownLoader : MonoBehaviour
{
    public Transform specialSpawnPoint;
    private GameObject player;
    private NavMeshAgent agent;
    public float navMeshCheckDistance = 2.0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found!");
            return;
        }

        agent = player.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("Player does not have a NavMeshAgent component!");
            return;
        }

        if (PlayerPrefs.GetInt("HasBeenToTown", 0) == 1)
        {
            Vector3 targetPosition = specialSpawnPoint.position;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(targetPosition, out hit, navMeshCheckDistance, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                Debug.Log("Player repositioned to a valid NavMesh location: " + hit.position);
            }
            else
            {
                Debug.LogWarning("No valid NavMesh position found near the spawn point.");
            }
        }
        else
        {
            Debug.Log("Player is at default position.");
        }
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("HasBeenToTown", 0);
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs reset on application quit.");
    }
}
