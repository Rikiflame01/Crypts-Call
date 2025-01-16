using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IEnemy
{
    public bool IsAttacking { get; protected set; }
    public bool isStunned { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    void Start()
    {
        IsAttacking = true;
    }
}
