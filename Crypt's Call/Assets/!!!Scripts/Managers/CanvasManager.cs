using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
        staminaSlider.maxValue = playerStats.maxStamina;
        staminaSlider.value = playerStats.stamina;

        manaSlider.maxValue = playerStats.maxMana;
        manaSlider.value = playerStats.mana;
    }

    public void DisableCanvases()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        if (canvases.Length < 0) {return;}
        foreach (Canvas canvas in canvases)
        {
            canvas.enabled = false;
        }
    }

    public void EnableCanvas(string canvasName){

        Canvas canvas = GameObject.Find(canvasName).GetComponent<Canvas>();
        if (canvas == null) {return;}
        canvas.enabled = true;
    }

    public void DisableCanvas(string canvasName){
        Canvas canvas = GameObject.Find(canvasName).GetComponent<Canvas>();
        if (canvas == null) {return;}
        canvas.enabled = false;
    }
    public void FadeInCanvas(string canvasName)
    {
        Canvas canvas = GameObject.Find(canvasName).GetComponent<Canvas>();
        if (canvas == null) {return;}
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
        if (group == null && canvas != null)
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
        StartCoroutine(LoadTown());
    }

    private IEnumerator LoadTown(){
        yield return new WaitForSeconds(5f);
        SaveCurrentScene();
        eventSystem.RaiseEvent("SceneManagement", "LoadTown");
    }

    private void SaveCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        SceneObjectState[] objectsToSave = FindObjectsByType<SceneObjectState>(FindObjectsSortMode.None);
        var newData = new Dictionary<string, SavedObjectState>();
        foreach (var objState in objectsToSave)
        {
            newData[objState.UniqueID] = objState.GetCurrentState();
        }

        var oldData = SceneStateManager.Instance.GetSceneState(currentSceneName);

        foreach (var kvp in oldData)
        {
            if (!newData.ContainsKey(kvp.Key))
            {
                newData[kvp.Key] = kvp.Value;
            }
        }

        SceneStateManager.Instance.SaveSceneState(currentSceneName, newData);
    }
    public void UpdateStaminaUI(){
        staminaSlider.value = playerStats.stamina;
    }

    public void UpdateManaUI(){
        manaSlider.value = playerStats.mana;
    }

}


