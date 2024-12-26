using UnityEngine;
using System;
using System.Collections.Generic;

public class GenericEventListener : MonoBehaviour
{
    [Header("Reference to the central GenericEventSystem")]
    [SerializeField] private GenericEventSystem eventSystem;

    [Header("List of all category-based events this object wants to listen to")]
    [SerializeField] private List<CategoryEventSubscription> eventSubscriptions;

    private void OnEnable()
    {
        if (eventSystem == null) return;

        foreach (var subscription in eventSubscriptions)
        {
            eventSystem.RegisterListener(subscription.category, subscription.eventName, subscription.InvokeWithPayload);
        }
    }

    private void OnDisable()
    {
        if (eventSystem == null) return;

        foreach (var subscription in eventSubscriptions)
        {
            eventSystem.UnregisterListener(subscription.category, subscription.eventName, subscription.InvokeWithPayload);
        }
    }
}
