using Core;
using Core.Events.Level;
using Core.Managers;
using UnityEngine;

namespace MiniGames.Base
{
    public abstract class LevelController : MonoBehaviour
    {
        public object Data;
        private void OnEnable()
        {
            ManagerContainer.EventManager.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
            ManagerContainer.EventManager.Subscribe<LevelUnloadedEvent>(OnLevelUnloaded);
            AddListener();
        }

        private void OnDisable()
        {
            ManagerContainer.EventManager.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);
            ManagerContainer.EventManager.Unsubscribe<LevelUnloadedEvent>(OnLevelUnloaded);
            RemoveListener();
        }
        
        protected abstract void OnLevelLoaded(LevelLoadedEvent e);
        protected abstract void OnLevelUnloaded(LevelUnloadedEvent e);

        protected abstract void AddListener();
        protected abstract void RemoveListener();
    }
}