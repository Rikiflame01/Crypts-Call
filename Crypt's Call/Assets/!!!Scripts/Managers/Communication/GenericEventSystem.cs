using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Events/Generic Event System")]
public class GenericEventSystem : ScriptableObject
{
    private Dictionary<string, Dictionary<string, List<Action<object>>>> categoryEventMap 
        = new Dictionary<string, Dictionary<string, List<Action<object>>>>();

    public void RegisterListener(string category, string eventName, Action<object> listener)
    {
        if (!categoryEventMap.ContainsKey(category))
        {
            categoryEventMap[category] = new Dictionary<string, List<Action<object>>>();
        }

        if (!categoryEventMap[category].ContainsKey(eventName))
        {
            categoryEventMap[category][eventName] = new List<Action<object>>();
        }

        var listeners = categoryEventMap[category][eventName];
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    public void UnregisterListener(string category, string eventName, Action<object> listener)
    {
        if (categoryEventMap.ContainsKey(category) &&
            categoryEventMap[category].ContainsKey(eventName))
        {
            var listeners = categoryEventMap[category][eventName];
            if (listeners.Contains(listener))
            {
                listeners.Remove(listener);
            }
        }
    }

    public void RaiseEvent(string category, string eventName, object payload = null)
    {
        if (categoryEventMap.ContainsKey(category) &&
            categoryEventMap[category].ContainsKey(eventName))
        {
            var listeners = categoryEventMap[category][eventName];

            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i]?.Invoke(payload);
            }
        }
        else
        {
            Debug.LogWarning($"No listeners found for Category: {category}, Event: {eventName}");
        }
    }
}
