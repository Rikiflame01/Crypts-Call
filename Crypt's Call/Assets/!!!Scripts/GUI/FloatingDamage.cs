using UnityEngine;
using TMPro;

public class FloatingDamage : MonoBehaviour
{
    [Header("Floating Text Settings")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float wiggleSpeed = 30f;
    [SerializeField] private float wiggleAmount = 2f;

    private TextMeshProUGUI textMesh;
    private Color textColor;
    private float timer;

    private Vector3 startPosition;

    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textColor = textMesh.color;
        startPosition = transform.position;
    }

    public void SetDamageText(float damage)
    {
        textMesh.text = damage.ToString("F0");
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        float wiggleOffset = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount;
        transform.position = new Vector3(startPosition.x + wiggleOffset, 
        transform.position.y, startPosition.z);

        timer += Time.deltaTime;
        float fadeAmount = 1 - (timer / lifetime);
        textColor.a = fadeAmount;
        textMesh.color = textColor;

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
