using UnityEngine;

namespace MiniGames.Data
{
    [CreateAssetMenu(fileName = "CamaraData", menuName = "NC/CameraData", order = 0)]
    public class CameraDataSO : ScriptableObject
    {
        public Vector3 originalPosition;
        public Vector3 originalRotation;
        public Vector3 originalScale;
        
        public bool UseOrtographic;
    }
}