using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public GenericEventSystem eventSystem;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (isKey == true) {
                Debug.Log("Key Collision is working");
                eventSystem.RaiseEvent("ItemDrop","Key");
            }
            if (isHealthItem == true)
            {
                eventSystem.RaiseEvent("Health", "Change", HealthAmount);
            }
            if (isManaItem == true)
            {
                eventSystem.RaiseEvent("Mana", "Change", ManaAmount);
            }
            if (isGoldItem == true)
            {
                eventSystem.RaiseEvent("Gold", "Change", GoldAmount);
            }
            if (isCrystalItem == true)
            {
                eventSystem.RaiseEvent("Crystal", "Change", CrystalAmount);
            }

            SceneObjectState state = GetComponent<SceneObjectState>();

            if (state != null) { 
                state.MarkDestroyedAndSave(state.UniqueID);
            }
            else
            {
                Destroy(gameObject);
            }


            }
    }
}
