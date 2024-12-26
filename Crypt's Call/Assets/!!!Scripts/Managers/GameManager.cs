using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public EntityStats playerStats;

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ResetPlayerStats()
    {
        playerStats.health = playerStats.maxHealth;
        playerStats.mana = 50;
        playerStats.stamina = 50;
        Debug.Log("Player stats reset.");
    }
}
