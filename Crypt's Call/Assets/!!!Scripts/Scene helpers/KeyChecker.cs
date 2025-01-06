using UnityEngine;
using UnityEngine.EventSystems;

public class KeyChecker : MonoBehaviour
{
    public GenericEventSystem eventSystem;

    private void OnTriggerEnter(Collider other)
    {
        GameObject[] Keys = GameObject.FindGameObjectsWithTag("Key");
        foreach (var key in Keys)
        {
            eventSystem.RaiseEvent("ItemDrop", "Key");
            Destroy(key);
        }
    }
}
