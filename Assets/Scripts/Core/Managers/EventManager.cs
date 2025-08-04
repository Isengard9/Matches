using System;
using System.Collections.Generic;

namespace Core.Managers
{
    public class EventManager : MManager
    {
        private readonly Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();

        public override void Initialize()
        {
            // EventManager için başlangıç ayarları
        }

        public void Subscribe<T>(Action<T> listener)
        {
            var eventType = typeof(T);
            if (_events.TryGetValue(eventType, out var existingDelegate))
            {
                _events[eventType] = (Action<T>)existingDelegate + listener;
            }
            else
            {
                _events[eventType] = listener;
            }
        }

        public void Unsubscribe<T>(Action<T> listener)
        {
            var eventType = typeof(T);
            if (_events.TryGetValue(eventType, out var existingDelegate))
            {
                var newDelegate = (Action<T>)existingDelegate - listener;
                if (newDelegate == null)
                {
                    _events.Remove(eventType);
                }
                else
                {
                    _events[eventType] = newDelegate;
                }
            }
        }

        public void Publish<T>(T eventToPublish)
        {
            var eventType = typeof(T);
            if (_events.TryGetValue(eventType, out var existingDelegate))
            {
                (existingDelegate as Action<T>)?.Invoke(eventToPublish);
            }
        }
    }
}
