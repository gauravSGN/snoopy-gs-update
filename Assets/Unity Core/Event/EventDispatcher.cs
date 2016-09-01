using System;
using Service;
using System.Collections.Generic;
using HandlerDict = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<object>>;

namespace Event
{
    public class EventDispatcher : EventService
    {
        private int dispatchesInProgress;

        private readonly HandlerDict pools = new HandlerDict();
        private readonly List<HandlerDict> handlerDictList = new List<HandlerDict>();
        private readonly HashSet<List<object>> handlerListsToClean = new HashSet<List<object>>();

        public EventDispatcher()
        {
            foreach (var handlerDictType in EnumExtensions.GetValues<HandlerDictType>())
            {
                handlerDictList.Insert((int)handlerDictType, new HandlerDict());
            }
        }

        public void Reset()
        {
            handlerDictList[(int)HandlerDictType.Transient].Clear();
        }

        public void AddEventHandler<T>(Action<T> handler) where T : GameEvent
        {
            AddEventHandler<T>(handler, HandlerDictType.Transient);
        }

        public void AddEventHandler<T>(Action<T> handler, HandlerDictType handlerDictType) where T : GameEvent
        {
            DictionaryInsert(handlerDictList[(int)handlerDictType], typeof(T), handler);
        }

        public void RemoveEventHandler<T>(Action<T> handler) where T : GameEvent
        {
            var eventType = typeof(T);

            foreach (var handlers in handlerDictList)
            {
                if (handlers.ContainsKey(eventType))
                {
                    var handlerList = handlers[eventType];
                    var index = handlerList.IndexOf(handler);

                    if (index >= 0)
                    {
                        handlerList[index] = null;
                        handlerListsToClean.Add(handlerList);
                    }
                }
            }
        }

        public void Dispatch<T>(T gameEvent) where T : GameEvent
        {
            var eventType = typeof(T);

            dispatchesInProgress++;

            foreach (var handlers in handlerDictList)
            {
                if (handlers.ContainsKey(eventType))
                {
                    var handlerList = handlers[eventType];

                    for (int handlerIndex = 0, maxIndex = handlerList.Count; handlerIndex < maxIndex; handlerIndex++)
                    {
                        if (handlerList[handlerIndex] != null)
                        {
                            (handlerList[handlerIndex] as Action<T>).Invoke(gameEvent);
                        }
                    }
                }
            }

            dispatchesInProgress--;
            CleanModifiedHandlerLists();
        }

        public void DispatchPooled<T>(T gameEvent) where T : GameEvent
        {
            Dispatch(gameEvent);
            AddPooledEvent(gameEvent);
        }

        public T GetPooledEvent<T>() where T : GameEvent
        {
            var eventType = typeof(T);
            T gameEvent;

            if (pools.ContainsKey(eventType) && (pools[eventType].Count > 0))
            {
                gameEvent = (T)pools[eventType][0];
                pools[eventType].RemoveAt(0);
            }
            else
            {
                gameEvent = (T)Activator.CreateInstance(eventType);
            }

            return gameEvent;
        }

        public void AddPooledEvent<T>(T gameEvent) where T : GameEvent
        {
            DictionaryInsert(pools, typeof(T), gameEvent);
        }

        private void DictionaryInsert<K, V>(Dictionary<K, List<V>> dictionary, K key, V item)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, new List<V>());
            }

            var list = dictionary[key];
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }

        private void CleanModifiedHandlerLists()
        {
            if ((dispatchesInProgress == 0) && (handlerListsToClean.Count > 0))
            {
                foreach (var handlerList in handlerListsToClean)
                {
                    handlerList.RemoveAll(h => (h == null));
                }

                handlerListsToClean.Clear();
            }
        }
    }
}
