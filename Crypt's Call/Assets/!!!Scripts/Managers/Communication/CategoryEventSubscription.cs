using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class ObjectUnityEvent : UnityEvent<object>
{
    
}

[Serializable]
public class CategoryEventSubscription
{
    [Tooltip("Group events logically (e.g. 'Resources', 'UI', 'Enemies').")]
    public string category;

    [Tooltip("Unique name for the event within this category (e.g. 'ResourcePickedUp', 'PlayerDeath').")]
    public string eventName;

    [Tooltip("Invoked when the event is raised. The parameter is the optional payload.")]
    public ObjectUnityEvent onEventRaised;

    public void InvokeWithPayload(object payload)
    {
        onEventRaised.Invoke(payload);
    }
}
