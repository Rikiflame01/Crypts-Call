using UnityEngine;

public class Damager : MonoBehaviour
{
    [Header("Entity Stats")]
    [SerializeField] private EntityStats entityStats;

    private PlayerController playerController;
    [SerializeField] private bool isPlayerWeapon =false;

    void Start()
    {
        if (isPlayerWeapon == true)
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        if (entityStats == null)
        {
            Debug.LogWarning("EntityStats not assigned to Damager on " + gameObject.name);
            return;
        }

        IHealth health = other.gameObject.GetComponent<IHealth>();

        if (health != null && isPlayerWeapon != true)
        {
            Debug.Log($"{gameObject.name} dealt {entityStats.damage} damage to {other.gameObject.name}");
            health.TakeDamage(entityStats.damage);
        }
        else if (health != null && isPlayerWeapon == true && playerController.isPlayerAttacking == true)
        {
            Debug.Log($"{gameObject.name} dealt {entityStats.damage} damage to {other.gameObject.name}");
            health.TakeDamage(entityStats.damage);
        }
        else
        {
            Debug.Log($"{other.gameObject.name} does not have an IHealth component.");
        }
    }
}
