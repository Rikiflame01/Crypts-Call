using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IEnemy
{
    public bool IsAttacking { get; protected set; }

    void Start()
    {
        IsAttacking = true;
    }
}
