﻿using UnityEngine;
using System;
using System.Collections.Generic;

public class EventDispatcher : MonoBehaviour
{
    public static EventDispatcher Instance { get { return instance; } }

    private static EventDispatcher instance;

    private Dictionary<Type, List<object>> handlers = new Dictionary<Type, List<object>>();

    public void OnEnable()
    {
        instance = this;
    }

    public void OnDestroy()
    {
        handlers.Clear();
    }

    public void AddEventHandler<T>(Action<T> handler) where T : IGameEvent
    {
        var eventType = typeof(T);
        List<object> handlerList;

        if (handlers.ContainsKey(eventType))
        {
            handlerList = handlers[eventType];
        }
        else
        {
            handlerList = new List<object>();
            handlers.Add(eventType, handlerList);
        }

        handlerList.Add(handler);
    }

    public void RemoveEventHandler<T>(Action<T> handler) where T : IGameEvent
    {
        var eventType = typeof(T);

        if (handlers.ContainsKey(eventType))
        {
            var handlerList = handlers[eventType];

            if (handlerList.Contains(handler))
            {
                handlerList.Remove(handler);
            }
        }
    }

    public void Dispatch<T>(T gameEvent) where T : IGameEvent
    {
        var eventType = typeof(T);

        if (handlers.ContainsKey(eventType))
        {
            var handlerList = handlers[eventType];

            foreach (var handler in handlerList)
            {
                (handler as Action<T>).Invoke(gameEvent);
            }
        }
    }
}
