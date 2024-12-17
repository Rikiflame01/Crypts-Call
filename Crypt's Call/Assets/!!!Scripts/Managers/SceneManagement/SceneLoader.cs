using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    private Coroutine fadeInCoroutine;
    private bool isFadingOut = false;

    public GameObject Canvas;
    public CanvasGroup canvasGroup;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (fadeInCoroutine == null)
            {
                fadeInCoroutine = StartCoroutine(FadeInAndLoadScene());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }

            if (!isFadingOut)
            {
                StartCoroutine(FadeOut());
            }
        }
    }

    private IEnumerator FadeInAndLoadScene()
    {
        Canvas.SetActive(true);

        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime / 1.5f;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("Level 1");
    }

    private IEnumerator FadeOut()
    {
        isFadingOut = true;

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime / 1.5f;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        Canvas.SetActive(false);
        isFadingOut = false;
    }
}
