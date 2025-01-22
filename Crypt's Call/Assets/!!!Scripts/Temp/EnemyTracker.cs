using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyTracker : MonoBehaviour
{
    [SerializeField] private GameObject victoryCanvas;
    [SerializeField] private PlayerController playerController;
    
    private List<GameObject> enemies = new List<GameObject>();

    private void Start()
    {
        RegisterAllEnemies();
    }

    private void RegisterAllEnemies()
    {
        GameObject[] enemyObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject enemy in enemyObjects)
        {
            if (enemy.layer == LayerMask.NameToLayer("Enemy"))
            {
                Health enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemies.Add(enemy);
                    enemyHealth.OnDied += HandleEnemyDeath;
                }
            }
        }
    }

    private void HandleEnemyDeath(GameObject enemy)
    {
        enemies.Remove(enemy);
        enemy.GetComponent<Health>().OnDied -= HandleEnemyDeath;
        
        if (enemies.Count == 0)
        {
            ActivateVictoryCanvas();
            DisablePlayerControls();
        }
    }

    private void ActivateVictoryCanvas()
    {
        if (victoryCanvas != null)
        {
            victoryCanvas.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Victory canvas is not assigned!");
        }
    }

    public void DeactivateVictoryCanvas()
    {
        if (victoryCanvas != null)
        {
            victoryCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Victory canvas is not assigned!");
        }
    }

    private void DisablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        else
        {
            Debug.LogWarning("PlayerController is not assigned!");
        }
    }

    public void EnablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        else
        {
            Debug.LogWarning("PlayerController is not assigned!");
        }
    }

}