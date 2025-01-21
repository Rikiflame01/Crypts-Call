using UnityEngine;

public class CollisionHandling : MonoBehaviour
{
    [SerializeField] private int GoldAmount;
    [SerializeField] private int CrystalAmount;
    [SerializeField] private int HealthAmount;
    [SerializeField] private int ManaAmount;

    [SerializeField] private bool isHealthItem = false;
    [SerializeField] private bool isManaItem = false;
    [SerializeField] private bool isGoldItem = false;
    [SerializeField] private bool isCrystalItem = false;
    [SerializeField] private bool isKey = false;

    [SerializeField] private GameObject healingVFXPrefab;
    [SerializeField] private Vector3 healingVFXOffset = new Vector3(0f, -1f, 0f);

    public GenericEventSystem eventSystem;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (isKey)
            {
                Debug.Log("Key Collision is working");
                eventSystem.RaiseEvent("ItemDrop", "Key");
            }
            if (isHealthItem)
            {
                eventSystem.RaiseEvent("Health", "Change", HealthAmount);
                PlayHealingVFX(collision.gameObject);
            }
            if (isManaItem)
            {
                eventSystem.RaiseEvent("Mana", "Change", ManaAmount);
            }
            if (isGoldItem)
            {
                eventSystem.RaiseEvent("Gold", "Change", GoldAmount);
            }
            if (isCrystalItem)
            {
                eventSystem.RaiseEvent("Crystal", "Change", CrystalAmount);
            }
            
            Destroy(gameObject);
        }
    }

    private void PlayHealingVFX(GameObject player)
    {
        if (healingVFXPrefab != null)
        {
            GameObject vfxInstance = Instantiate(healingVFXPrefab, player.transform.position + healingVFXOffset, Quaternion.identity);
            vfxInstance.transform.SetParent(player.transform);
        }
        else
        {
            Debug.LogWarning("Healing VFX Prefab is not assigned.");
        }
    }
}
