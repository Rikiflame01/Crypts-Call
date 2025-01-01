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
        Debug.Log("OnStaminaChange was invoked!");
        if (payload is int staminaChange)
        {
            playerStats.stamina += staminaChange;
            Debug.Log($"Stamina changed by {staminaChange}. New value = {playerStats.stamina}.");
        }
        else
        {
            Debug.LogWarning($"Stamina change event received a non-int payload: {payload?.GetType()}");
        }
        eventSystem.RaiseEvent("UI", "updateStaminaUI");
    }

    public void OnManaChange(object payload)
        {
            Debug.Log("OnManaChange was invoked!");
            if (payload is int manaChange)
            {
                playerStats.mana += manaChange;
                Debug.Log($"Mana changed by {manaChange}. New value = {playerStats.mana}.");
            }
            else
            {
                Debug.LogWarning($"Mana change event received a non-int payload: {payload?.GetType()}");
            }
            eventSystem.RaiseEvent("UI", "updateManaUI");
        }

    public void OnGoldChange(object payload)
    {
        Debug.Log("OnManaChange was invoked!");
        if (payload is int goldChange)
        {
            playerStats.Gold += goldChange;
            Debug.Log($"Gold changed by {goldChange}. New value = {playerStats.Gold}.");
        }
        else
        {
            Debug.LogWarning($"Mana change event received a non-int payload: {payload?.GetType()}");
        }
        eventSystem.RaiseEvent("UI", "updateManaUI");
    }

    public void OnCrystalChange(object payload)
    {
        Debug.Log("OnManaChange was invoked!");
        if (payload is int crystalChange)
        {
            playerStats.Crystal += crystalChange;
            Debug.Log($"Gold changed by {crystalChange}. New value = {playerStats.Crystal}.");
        }
        else
        {
            Debug.LogWarning($"Mana change event received a non-int payload: {payload?.GetType()}");
        }
        eventSystem.RaiseEvent("UI", "updateManaUI");
    }
}
