using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [Header("Enemy Drop Settings")]
    [Tooltip("Set the rarity of the enemy: Standard, Mythic, Boss")]
    public string rarity;

    [Tooltip("Items that are guaranteed to drop when this enemy dies.")]
    public Item[] guaranteedDrops;

    public void HandleDrop(){
        if (ItemDropManager.Instance != null)
        {
            ItemDropManager.Instance.HandleDrop(this);
        }
    }

}
