using System.Dynamic;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class StaminaRecovery : MonoBehaviour
{
    [Header("UI Components")]
    public Slider staminaSlider;

    public GameObject restCanvas;

    private bool isRecovering = false;

    public GenericEventSystem eventManager;
    [SerializeField] private EntityStats playerStats;

    private void Awake()
    {
        staminaSlider.maxValue = playerStats.maxStamina;
        staminaSlider.value = playerStats.stamina;
    }

    private void OnTriggerEnter(Collider other)
    {
        restCanvas.SetActive(true);
        isRecovering = true;
        StartCoroutine(StaminaCoRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        restCanvas.SetActive(false);
        isRecovering = false;
    }

    private IEnumerator StaminaCoRoutine()
    {
        while (isRecovering == true)
        {
            yield return new WaitForSeconds(1);
            eventManager.RaiseEvent("Stamina", "Change", 1);
            staminaSlider.maxValue = playerStats.maxStamina;
            staminaSlider.value = playerStats.stamina;
        }
    }
}