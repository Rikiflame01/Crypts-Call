using UnityEngine;

[System.Serializable]
public class DropRarity
{
    public string rarityName;
    [Range(0, 100)]
    public float dropChance;
    public Item[] items;
}
