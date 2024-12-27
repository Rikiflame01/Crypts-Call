using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private GenericEventSystem eventSystem;
    [SerializeField] private EntityStats playerStats;

    public Slider staminaSlider;
    public Image staminaFill;

    public Slider manaSlider;
    public Image manaFill;

    void Start()
    {
        staminaSlider.maxValue = playerStats.stamina;
        staminaSlider.value = playerStats.stamina;

        manaSlider.maxValue = playerStats.mana;
        manaSlider.value = playerStats.mana;
    }

    public void DisableCanvases()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            canvas.enabled = false;
        }
    }

    public void EnableCanvas(string canvasName){
        Debug.Log("Enabling canvas: " + canvasName);
        Canvas canvas = GameObject.Find(canvasName).GetComponent<Canvas>();
        canvas.enabled = true;
    }

    public void DisableCanvas(string canvasName){
        Canvas canvas = GameObject.Find(canvasName).GetComponent<Canvas>();
        canvas.enabled = false;
    }
    public void FadeInCanvas(string canvasName)
    {
        Canvas canvas = GameObject.Find(canvasName).GetComponent<Canvas>();
        canvas.enabled = true;

        CanvasGroup group = canvas.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = canvas.gameObject.AddComponent<CanvasGroup>();
        }

        float duration = 1f;
        StartCoroutine(FadeInCanvasRoutine(group, duration));
    }

    private IEnumerator FadeInCanvasRoutine(CanvasGroup group, float duration)
    {
        float startAlpha = 0f;
        float endAlpha = 1f;
        float startTime = Time.time;
        float endTime = startTime + duration;
        group.alpha = startAlpha;

        Debug.Log("Fading in canvas group: " + group.name);

        while (Time.time < endTime)
        {
            float elapsedTime = Time.time - startTime;
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        group.alpha = endAlpha;
    }

    public void FadeOutCanvas(string canvasName)
    {
        Canvas canvas = GameObject.Find(canvasName).GetComponent<Canvas>();
        CanvasGroup group = canvas.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = canvas.gameObject.AddComponent<CanvasGroup>();
        }

        float duration = 1f;
        StartCoroutine(FadeOutCanvasRoutine(group, duration));

    }

    private IEnumerator FadeOutCanvasRoutine(CanvasGroup group, float duration)
    {
        yield return new WaitForSeconds(3f);
        float startAlpha = 1f;
        float endAlpha = 0f;
        float startTime = Time.time;
        float endTime = startTime + duration;
        group.alpha = startAlpha;

        Debug.Log("Fading out canvas group: " + group.name);

        while (Time.time < endTime)
        {
            float elapsedTime = Time.time - startTime;
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        group.alpha = endAlpha;
    }

    public void HandlePlayerDeath(){
        DisableCanvases();
        FadeInCanvas("PlayerDeath");
        FadeOutCanvas("PlayerDeath");

        StartCoroutine(LoadTown());

    }

    private IEnumerator LoadTown(){
        yield return new WaitForSeconds(3f);
        eventSystem.RaiseEvent("SceneManagement", "LoadTown");
    }

    public void UpdateStaminaUI(){
        staminaSlider.value = playerStats.stamina;
    }

    public void UpdateManaUI(){
        manaSlider.value = playerStats.mana;
    }

}


