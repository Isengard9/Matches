using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Managers
{
    public class ManagerContainer : MonoBehaviour
    {
        public static ManagerContainer Instance { get; private set; }
        public List<IManager> _managers = new List<IManager>();
        public static EventManager EventManager { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateManagers();
            InitializeManagers();
        }

        private void CreateManagers()
        {
            AddManager(new EventManager());
            AddManager(new LevelManager());
            // Diğer yöneticiler buraya eklenecek
        }

        private void AddManager(IManager manager)
        {
            _managers.Add(manager);
        }

        private void InitializeManagers()
        {
            foreach (var manager in _managers)
            {
                manager.Initialize();
            }
            
            EventManager = GetManager<EventManager>();
        }

        public T GetManager<T>() where T : class, IManager
        {
            return _managers.FirstOrDefault(m => m is T) as T;
        }
    }
}
