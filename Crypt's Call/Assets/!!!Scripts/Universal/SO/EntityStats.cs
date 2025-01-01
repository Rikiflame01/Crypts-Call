using UnityEngine;

[CreateAssetMenu(fileName = "New Entity Stats", menuName = "Game System/Entity Stats")]
public class EntityStats : ScriptableObject
{
    [Header("Health and Stamina")]
    public float maxHealth = 100f;
    public float health = 100f;
    public float mana = 50f;
    public float maxMana = 50f;
    public float stamina = 50f;
    public float maxStamina = 50f;
    public int Gold = 0;
    public int Crystal = 0;

    [Header("Combat Stats")]
    public float damage = 10f;

    [Header("Entity Type")]
    public bool isPlayer = false;
    public bool isMelee = true;
    public bool isRanged = false;
    public bool isBoss = false;

    
}
