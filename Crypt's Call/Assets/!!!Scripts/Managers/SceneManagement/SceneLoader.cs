using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class SceneLoader : MonoBehaviour
{
    public GameObject Canvas;
    public CanvasGroup canvasgroup;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            StartCoroutine(LoadScene(other));
        }
    }

    private IEnumerator LoadScene(Collider other)
    {
        Canvas.SetActive(true);
        while (canvasgroup.alpha < 1)
        {
            canvasgroup.alpha += Time.deltaTime / 5;
            yield return null;
        }
        SceneManager.LoadScene("Level 1");
    }
}
