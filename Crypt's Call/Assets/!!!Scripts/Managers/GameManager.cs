using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public GenericEventSystem eventSystem;
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
    }

    public void OnStaminaChange(object payload)
    {
        if (playerStats.stamina > playerStats.maxStamina)
        {
            return;
        }
        if (payload is int staminaChange)
        {
            if (playerStats.stamina == playerStats.maxStamina && staminaChange > 0)
            {
                return;
            }

            playerStats.stamina += staminaChange;
        }
        else
        {
            Debug.LogWarning($"Stamina change event received a non-int payload: {payload?.GetType()}");
        }
        
        eventSystem.RaiseEvent("UI", "updateStaminaUI");
    }

    public void OnManaChange(object payload)
        {
        if (playerStats.mana > playerStats.maxMana)
        {
            return;
        }
        if (payload is int manaChange)
            {
                playerStats.mana += manaChange;
            }
            else
            {
                Debug.LogWarning($"Mana change event received a non-int payload: {payload?.GetType()}");
            }
            eventSystem.RaiseEvent("UI", "updateManaUI");
        }

    public void OnGoldChange(object payload)
    {
        if (payload is int goldChange)
        {
            playerStats.Gold += goldChange;
        }
        else
        {
            Debug.LogWarning($"Mana change event received a non-int payload: {payload?.GetType()}");
        }
        eventSystem.RaiseEvent("UI", "updateManaUI");
    }

    public void OnCrystalChange(object payload)
    {
        if (payload is int crystalChange)
        {
            playerStats.Crystal += crystalChange;
        }
        else
        {
            Debug.LogWarning($"Mana change event received a non-int payload: {payload?.GetType()}");
        }
        eventSystem.RaiseEvent("UI", "updateManaUI");
    }

    public void OnHealthChange(object payload) 
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Health health = player.GetComponent<Health>();

        if (payload is int healthChange)
        {
            health.Heal(healthChange);
        }
    }
    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ChallengeScene(){
        SceneManager.LoadScene("Challenge");
    }

    public void ExitApplication(){
        Application.Quit();
    }
}
