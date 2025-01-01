using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [Header("Enemy Drop Settings")]
    [Tooltip("Set the rarity of the enemy: Standard, Mythic, Boss")]
    public string rarity;

    [Tooltip("Items that are guaranteed to drop when this enemy dies.")]
    public Item[] guaranteedDrops;

    private void OnDisable()
    {
        if (ItemDropManager.Instance != null)
        {
            Debug.Log("EnemyDrop an item as been spawned");
            ItemDropManager.Instance.HandleDrop(this);
        }
    }
}
