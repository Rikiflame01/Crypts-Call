using UnityEngine;
using System;

public static class EventManager
{
    public static Action<string, int> OnAbilityUsed;
    public static Action<GameObject> OnStunApplied;
    public static Action<GameObject> OnPoisonApplied;

    public static void TriggerAbilityUsed(string abilityName, int cooldownTime)
    {
        OnAbilityUsed?.Invoke(abilityName, cooldownTime);
    }

    public static void TriggerStunApplied(GameObject player)
    {
        OnStunApplied?.Invoke(player);
    }

    public static void TriggerPoisonApplied(GameObject player)
    {
        OnPoisonApplied?.Invoke(player);
    }
}
