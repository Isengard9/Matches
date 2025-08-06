using System;
using MiniGames.Data;
using UnityEngine;

namespace Core
{
    [Serializable]
    public abstract class Level : ScriptableObject, ILevel
    {
        public CameraDataSO CameraData;
        [SerializeField] private bool isLoaded;
        public bool IsLoaded => isLoaded;
        
        public virtual void Load()
        {
            if (isLoaded) return;
            isLoaded = true;
            //SetCameraValues();
        }
        
        public virtual void Unload()
        {
            if (!isLoaded) return;
            isLoaded = false;
        }

        public void SetCameraValues()
        {
            
            Camera.main.transform.position = CameraData.originalPosition;
            Camera.main.transform.rotation = Quaternion.Euler(CameraData.originalRotation);
            Camera.main.transform.localScale = CameraData.originalScale;
            Camera.main.orthographic = CameraData.UseOrtographic;
        }
        
    }
}
