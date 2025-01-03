using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BreathingEffect : MonoBehaviour
{
    [Header("Breathing Settings")]
    [Tooltip("Duration of one full breath cycle (in seconds).")]
    public float breathDuration = 2f;

    [Tooltip("Maximum intensity multiplier when fading to white.")]
    [Range(0f, 10f)]
    public float maxEmissionIntensity = 1f;

    [Header("Shader Property Settings")]
    [Tooltip("Name of the emission color property in the shader (usually _EmissionColor).")]
    public string emissionColorProperty = "_EmissionColor";

    private Color originalEmissionColor;
    private Renderer objRenderer;
    private float timer;
    private MaterialPropertyBlock propBlock;
    private int emissionColorID;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();

        emissionColorID = Shader.PropertyToID(emissionColorProperty);

        if (objRenderer.sharedMaterial.HasProperty(emissionColorProperty))
        {
            originalEmissionColor = objRenderer.sharedMaterial.GetColor(emissionColorProperty);
        }
        else
        {
            enabled = false;
            return;
        }

        objRenderer.sharedMaterial.EnableKeyword("_EMISSION");

        propBlock.SetColor(emissionColorID, originalEmissionColor);
        objRenderer.SetPropertyBlock(propBlock);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float lerpFactor = (Mathf.Sin((timer / breathDuration) * Mathf.PI * 2) + 1f) / 2f;

        Color targetEmission = Color.Lerp(originalEmissionColor, Color.white, lerpFactor * maxEmissionIntensity);

        propBlock.SetColor(emissionColorID, targetEmission);
        objRenderer.SetPropertyBlock(propBlock);

        if (timer > breathDuration)
        {
            timer -= breathDuration;
        }
    }

    void OnDisable()
    {
        if (propBlock != null && objRenderer != null)
        {
            propBlock.SetColor(emissionColorID, originalEmissionColor);
            objRenderer.SetPropertyBlock(propBlock);
        }
    }
}
