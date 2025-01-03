using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private EntityStats playerStats;
    [SerializeField] private GenericEventSystem GenericEventSystem;

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI crystalText;

    private void Awake()
    {
        goldText.text = playerStats.Gold.ToString();
        crystalText.text = playerStats.Crystal.ToString();
    }

    public void CrystalToGoldTrade()
    {
        if (playerStats.Crystal< 20)
        {
            Debug.Log("not enough Crystal to trade");
            return;
        }
        playerStats.Gold += 10;
        playerStats.Crystal -= 20;

        RefreshCanvases();
    }

    public void HealPlayer()
    {
        if (playerStats.health <=0 || playerStats.Gold <10 || playerStats.health == playerStats.maxHealth)
        {
            Debug.Log("Cannot afford to heal");
            return;
        }
        playerStats.Gold -= 10;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Health health = player.GetComponent<Health>();

        health.Heal(playerStats.maxHealth-playerStats.health);
        RefreshCanvases();
    }

    public void ReplenishMana()
    {
        if (playerStats.Gold < 10 || playerStats.mana == playerStats.maxMana)
        {
            Debug.Log("Cannot afford to replenish Mana");
            return;
        }
        playerStats.Gold -= 10;

        float replenishAmnt = (playerStats.maxMana - playerStats.mana);
        GenericEventSystem.RaiseEvent("Mana", "Change", (int)replenishAmnt);

        RefreshCanvases();
    }

    private void RefreshCanvases()
    {
        goldText.text = playerStats.Gold.ToString();
        crystalText.text = playerStats.Crystal.ToString();
    }
}
