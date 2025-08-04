using System;
using UnityEngine;

namespace Core
{
    [Serializable]
    public abstract class Level : ScriptableObject, ILevel
    {
        [SerializeField] private bool isLoaded;
        public bool IsLoaded => isLoaded;
        
        public virtual void Load()
        {
            if (isLoaded) return;
            isLoaded = true;
        }
        
        public virtual void Unload()
        {
            if (!isLoaded) return;
            isLoaded = false;
        }
        
    }
}
