using UnityEngine;

public static class EventManager
{
    public static System.Action<string, int> OnAbilityUsed;

    public static void TriggerAbilityUsed(string abilityName, int cooldownTime)
    {
        OnAbilityUsed?.Invoke(abilityName, cooldownTime);
    }
}
