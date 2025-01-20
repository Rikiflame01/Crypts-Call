using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooldownUIManager : MonoBehaviour
{
    [SerializeField] private Image standardAttackImage;
    [SerializeField] private Image heavyAttackImage;
    [SerializeField] private Image dashImage;
    [SerializeField] private Image standardOverlay;
    [SerializeField] private Image heavyOverlay;
    [SerializeField] private Image dashOverlay;
    [SerializeField] private float growAmount = 1.1f;
    [SerializeField] private float shakeAmount = 0.05f;

    private Dictionary<string, Coroutine> activeCooldowns = new Dictionary<string, Coroutine>();

    private void OnEnable()
    {
        EventManager.OnAbilityUsed += StartCooldown;
    }

    private void OnDisable()
    {
        EventManager.OnAbilityUsed -= StartCooldown;
    }

    private void StartCooldown(string abilityName, int cooldownTime)
    {
        if (activeCooldowns.ContainsKey(abilityName))
        {
            StopCoroutine(activeCooldowns[abilityName]);
        }

        switch (abilityName.ToLower())
        {
            case "standard":
                activeCooldowns[abilityName] = StartCoroutine(HandleCooldown(standardAttackImage, standardOverlay, cooldownTime));
                break;
            case "heavy":
                activeCooldowns[abilityName] = StartCoroutine(HandleCooldown(heavyAttackImage, heavyOverlay, cooldownTime));
                break;
            case "dash":
                activeCooldowns[abilityName] = StartCoroutine(HandleCooldown(dashImage, dashOverlay, cooldownTime));
                break;
            default:
                Debug.LogWarning("Unknown ability name: " + abilityName);
                break;
        }
    }

    private IEnumerator HandleCooldown(Image abilityImage, Image overlayImage, int cooldownTime)
    {
        yield return StartCoroutine(ShakeAndGrow(abilityImage, overlayImage));
        
        if (cooldownTime == 0)
        {
            float transitionTime = 0.5f;
            float elapsedTime = 0f;
            while (elapsedTime < transitionTime)
            {
                elapsedTime += Time.deltaTime;
                abilityImage.fillAmount = Mathf.Lerp(0f, 1f, elapsedTime / transitionTime);
                yield return null;
            }
            abilityImage.fillAmount = 1f;
            yield break;
        }

        float elapsed = 0f;
        abilityImage.fillAmount = 0f;

        while (elapsed < cooldownTime)
        {
            elapsed += Time.deltaTime;
            abilityImage.fillAmount = Mathf.Clamp01(elapsed / cooldownTime);
            yield return null;
        }

        abilityImage.fillAmount = 1f;
    }

    private IEnumerator ShakeAndGrow(Image abilityImage, Image overlayImage)
    {
        Vector3 originalScale = abilityImage.transform.localScale;
        Vector3 targetScale = originalScale * growAmount;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(elapsed * 50) * shakeAmount;
            Vector3 shakeVector = new Vector3(shake, shake, 0);
            abilityImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration) + shakeVector;
            overlayImage.transform.localScale = abilityImage.transform.localScale;
            yield return null;
        }
        abilityImage.transform.localScale = originalScale;
        overlayImage.transform.localScale = originalScale;
    }
}
