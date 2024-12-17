using UnityEngine;
using UnityEngine.SceneManagement;

public class EnableTorch : MonoBehaviour
{
    public GameObject torch;
    void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Level1")
        {
            torch.SetActive(true);
        }
    }
}
