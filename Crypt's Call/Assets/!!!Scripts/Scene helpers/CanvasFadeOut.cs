using UnityEngine;
using System.Collections;

public class CanvasFadeOut : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    void Start()
    {
        StartCoroutine(FadeOut());    
    }

    private IEnumerator FadeOut()
    {
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime / 1.5f;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

}
