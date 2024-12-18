using UnityEngine;

public class DisablePreviousFloor : MonoBehaviour
{
    public GameObject[] previousFloor;
    private float playerEntryY;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerEntryY = other.transform.position.y;
            Debug.Log("Player entered at Y: " + playerEntryY);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            float playerExitY = other.transform.position.y;

            if (playerExitY < playerEntryY)
            {
                foreach (GameObject floor in previousFloor)
                {
                    floor.SetActive(false);
                }
                Debug.Log("Player exited lower: Previous floors disabled.");
            }
            else if (playerExitY > playerEntryY)
            {
                foreach (GameObject floor in previousFloor)
                {
                    floor.SetActive(true);
                }
                Debug.Log("Player exited higher: Previous floors enabled.");
            }
        }
    }
}
