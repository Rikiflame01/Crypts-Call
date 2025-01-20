using UnityEngine;
using System.Collections;

public class Despawn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DespawnCoRoutine());
    }

    private IEnumerator DespawnCoRoutine(){
        yield return new WaitForSeconds(1);
        Destroy(this.gameObject);
    }
}
