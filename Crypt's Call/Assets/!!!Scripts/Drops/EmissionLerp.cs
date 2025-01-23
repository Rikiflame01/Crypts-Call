using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
public class EmissionLerp : MonoBehaviour
{
    public float lerpSpeed = 1f;

    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    private Dictionary<Material, Color> originalEmissionColors = new Dictionary<Material, Color>();
    
    void Start()
    {
        meshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());

        foreach (MeshRenderer renderer in meshRenderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");

                    if (!originalEmissionColors.ContainsKey(mat))
                    {
                        originalEmissionColors.Add(mat, mat.GetColor("_EmissionColor"));
                    }
                }
                else
                {
                    Debug.LogWarning($"Material {mat.name} does not have an _EmissionColor property.");
                }
            }
        }
    }

    void Update()
    {
        float lerpFactor = (Mathf.Sin(Time.time * lerpSpeed) + 0.1f) / 1.5f;

        foreach (MeshRenderer renderer in meshRenderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (originalEmissionColors.ContainsKey(mat))
                {
                    Color targetColor = Color.Lerp(originalEmissionColors[mat], Color.white, lerpFactor);
                    mat.SetColor("_EmissionColor", targetColor);
                }
            }
        }
    }

    void OnDisable()
    {
        foreach (MeshRenderer renderer in meshRenderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (originalEmissionColors.ContainsKey(mat))
                {
                    mat.SetColor("_EmissionColor", originalEmissionColors[mat]);
                }
            }
        }
    }
}
