using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public AudioSource soundEffect;
    private Coroutine fadeInCoroutine;
    private Coroutine cameraZoomCoroutine;
    private bool isFadingOut = false;

    public GameObject Canvas;
    public CanvasGroup canvasGroup;

    public Transform zoomTarget;
    public float zoomDuration = 1.5f;
    public float zoomedFOV = 30f;

    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private float originalCameraFOV;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
            originalCameraFOV = mainCamera.fieldOfView;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
        if (soundEffect != null){
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.Play();
            }

            if (fadeInCoroutine == null)
            {
                fadeInCoroutine = StartCoroutine(FadeInAndLoadScene());
            }
            if (cameraZoomCoroutine != null)
            {
                StopCoroutine(cameraZoomCoroutine);
            }
            cameraZoomCoroutine = StartCoroutine(ZoomCameraIn());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (soundEffect != null){
                soundEffect.Stop();  
            }

            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }
            if (!isFadingOut)
            {
                StartCoroutine(FadeOut());
            }
            if (cameraZoomCoroutine != null)
            {
                StopCoroutine(cameraZoomCoroutine);
            }
            cameraZoomCoroutine = StartCoroutine(ZoomCameraOut());
        }
    }

private IEnumerator FadeInAndLoadScene()
{
    Canvas.SetActive(true);

    while (canvasGroup.alpha < 1f)
    {
        canvasGroup.alpha += Time.deltaTime / zoomDuration;
        yield return null;
    }

    canvasGroup.alpha = 1f;
    yield return new WaitForSeconds(0.5f);

    if (SceneManager.GetActiveScene().name == "Level 1")
    {
        PlayerPrefs.SetInt("HasBeenToTown", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Town");
    }
    else
    {
        SceneManager.LoadScene("Level 1");
    }
}

    private IEnumerator FadeOut()
    {
        isFadingOut = true;

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime / zoomDuration;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        Canvas.SetActive(false);
        isFadingOut = false;
    }

    private IEnumerator ZoomCameraIn()
    {
        float elapsedTime = 0f;

        Vector3 startPosition = mainCamera.transform.position;
        float startFOV = mainCamera.fieldOfView;

        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;

            mainCamera.transform.position = Vector3.Lerp(startPosition, zoomTarget.position, elapsedTime / zoomDuration);
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, zoomedFOV, elapsedTime / zoomDuration);

            yield return null;
        }

        mainCamera.transform.position = zoomTarget.position;
        mainCamera.fieldOfView = zoomedFOV;
    }

    private IEnumerator ZoomCameraOut()
    {
        float elapsedTime = 0f;

        Vector3 startPosition = mainCamera.transform.position;
        float startFOV = mainCamera.fieldOfView;

        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;

            mainCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, elapsedTime / zoomDuration);
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, originalCameraFOV, elapsedTime / zoomDuration);

            yield return null;
        }

        mainCamera.transform.position = originalCameraPosition;
        mainCamera.fieldOfView = originalCameraFOV;
    }
}
