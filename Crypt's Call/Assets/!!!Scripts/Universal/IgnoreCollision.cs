using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    public int layerToIgnore;

    void Start()
    {
        Physics.IgnoreLayerCollision(gameObject.layer, layerToIgnore);
    }
}
