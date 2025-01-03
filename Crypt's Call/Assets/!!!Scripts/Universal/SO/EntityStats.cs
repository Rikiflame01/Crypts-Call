using UnityEngine;

[CreateAssetMenu(fileName = "New Entity Stats", menuName = "Game System/Entity Stats")]
public class EntityStats : ScriptableObject
{
    [Header("Health and Stamina")]
    public int maxHealth = 100;
    public int health = 100;
    public int mana = 50;
    public int maxMana = 50;
    public int stamina = 50;
    public int maxStamina = 50;
    public int Gold = 0;
    public int Crystal = 0;

    [Header("Combat Stats")]
    public int damage = 10;

    [Header("Entity Type")]
    public bool isPlayer = false;
    public bool isMelee = true;
    public bool isRanged = false;
    public bool isBoss = false;

    
}
