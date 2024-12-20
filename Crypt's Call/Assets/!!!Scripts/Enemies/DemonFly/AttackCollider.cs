using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    public System.Action<GameObject> onPlayerHit;
    public Animator animator;
    private void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            animator.SetBool("isAttacking", true);
            onPlayerHit?.Invoke(player.gameObject);
        }
    }
    void OnDisable()
    {
        animator.SetBool("isAttacking", false);
    }
}
