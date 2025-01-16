using System.Collections;
using UnityEngine;
using UnityEngine.AI;


#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class StunController : MonoBehaviour
{
    [Header("Visual Effect Settings")]
    [Tooltip("Assign the Visual Effect prefab to be instantiated above the GameObject when stunned.")]
    [SerializeField] private GameObject vfxPrefab;

    private Animator animator;
    private bool isStunned;
    private MonoBehaviour[] disabledScripts;
    private EnemyDamager enemyDamager;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing on the same GameObject as StunController.");
        }

        enemyDamager = GetComponent<EnemyDamager>();
    }

    public void TriggerStun()
    {
        if (!isStunned)
        {
            StartCoroutine(StunCoroutine());
        }
        else
        {
            Debug.LogWarning("Stun is already active on this object.");
        }
    }

    private IEnumerator StunCoroutine()
    {
        isStunned = true;
        Debug.Log("Stun started.");
        animator.SetBool("isStunned", true);

        IEnemy[] enemyScripts = GetComponents<IEnemy>();
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.enabled = false;
        foreach (var enemy in enemyScripts)
        {
            enemy.isStunned = true;

            if (enemy is MonoBehaviour mb)
            {

                mb.enabled = false;
            }
        }

        if (enemyDamager != null)
        {
            enemyDamager.enabled = false;
        }

        GameObject vfxInstance = null;
        if (vfxPrefab)
        {
            Vector3 vfxPosition = transform.position + Vector3.up * 2f;
            vfxInstance = Instantiate(vfxPrefab, vfxPosition, Quaternion.identity, transform);
        }

        yield return new WaitForSeconds(3f);
        animator.SetBool("isStunned", false);

        navMeshAgent.enabled = true;
        foreach (var enemy in enemyScripts)
        {
            if (enemy is MonoBehaviour mb)
            {
                mb.enabled = true;
            }

            enemy.isStunned = false;
        }

        if (enemyDamager != null)
        {
            enemyDamager.enabled = true;
        }

        if (vfxInstance) Destroy(vfxInstance);

        isStunned = false;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StunController))]
    public class StunControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            StunController stunController = (StunController)target;
            if (GUILayout.Button("Test Stun"))
            {
                stunController.TriggerStun();
            }
        }
    }
#endif
}
