using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourFeedbackManager : MonoBehaviour
{
    [SerializeField] private float flashDuration = 0.2f;

    [SerializeField] private Material flashRedMaterial;
    [SerializeField] private Material flashWhiteMaterial;

    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> enemies = new List<GameObject>();

    private bool isFlashing = false;

    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    void Awake()
    {
        RegisterEntities();
    }

    private void RegisterEntities()
    {
        players.Clear();
        players.AddRange(GameObject.FindGameObjectsWithTag("Player"));

    }

    public void FlashObjectHandler(object payload)
    {
        if (payload is GameObject gameObject)
        {
            if (gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                StartCoroutine(FlashMaterialCoroutine(gameObject, flashWhiteMaterial, flashDuration));
            }
            else if (gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                StartCoroutine(FlashMaterialCoroutine(gameObject, flashRedMaterial, flashDuration));
            }
            else
            {
                Debug.LogWarning($"GameObject {gameObject.name} is neither Player nor Enemy.");
            }
        }
        else
        {
            Debug.LogWarning("FlashObjectHandler received a payload that is not a GameObject.");
        }
    }

    private IEnumerator FlashMaterialCoroutine(GameObject target, Material flashMat, float duration)
    {
        if (isFlashing)
        {
            yield break;
        }

        isFlashing = true;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            if (!originalMaterials.ContainsKey(renderer))
            {
                originalMaterials[renderer] = renderer.materials;
            }

            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = flashMat;
            }
            renderer.materials = newMaterials;
        }

        yield return new WaitForSeconds(duration);

        foreach (var renderer in renderers)
        {
            if (originalMaterials.ContainsKey(renderer))
            {
                renderer.materials = originalMaterials[renderer];
            }
        }

        isFlashing = false;
    }

}
