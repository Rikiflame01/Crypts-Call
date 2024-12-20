using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    public System.Action<GameObject> onPlayerHit;

    private void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            onPlayerHit?.Invoke(player.gameObject);
        }
    }
}
