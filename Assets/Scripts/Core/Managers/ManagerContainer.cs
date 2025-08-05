using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Managers
{
    [DefaultExecutionOrder(-2)]
    public class ManagerContainer : MonoBehaviour
    {
        public static ManagerContainer Instance { get; private set; }
        [SerializeField] public List<IManager> _managers = new List<IManager>();
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
            AddManager(new SaveLoadManager());
            AddManager(new EventManager());
            AddManager(new LevelManager());
            AddManager(GetComponent<UIManager>());
            // Diğer yöneticiler buraya eklenecek
        }

        private void AddManager(IManager manager)
        {
            _managers.Add(manager);
        }

        private void InitializeManagers()
        {
            EventManager = GetManager<EventManager>();
            foreach (var manager in _managers)
            {
                manager.Initialize();
            }
            
           
        }

        public T GetManager<T>() where T : class, IManager
        {
            return _managers.FirstOrDefault(m => m is T) as T;
        }
    }
}
