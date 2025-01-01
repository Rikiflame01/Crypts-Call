using UnityEngine;
using System.Collections.Generic;

public class ItemDropManager : MonoBehaviour
{
    public static ItemDropManager Instance;

    [Header("Drop Rarities")]
    public List<DropRarity> dropRarities;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void HandleDrop(EnemyDrop enemy)
    {
        foreach (var guaranteedItem in enemy.guaranteedDrops)
        {
            DropItem(guaranteedItem, enemy.transform.position);
        }

        DropRarity rarity = GetRarity(enemy.rarity);
        if (rarity != null)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= rarity.dropChance)
            {
                if (rarity.items.Length > 0)
                {
                    int itemIndex = Random.Range(0, rarity.items.Length);
                    DropItem(rarity.items[itemIndex], enemy.transform.position);
                }
            }
        }
    }

    private DropRarity GetRarity(string rarityName)
    {
        return dropRarities.Find(r => r.rarityName.Equals(rarityName, System.StringComparison.OrdinalIgnoreCase));
    }

    private void DropItem(Item item, Vector3 position)
    {
        if (item.itemPrefab != null)
        {
            Instantiate(item.itemPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"Item prefab for {item.itemName} is not assigned.");
        }
    }
}
