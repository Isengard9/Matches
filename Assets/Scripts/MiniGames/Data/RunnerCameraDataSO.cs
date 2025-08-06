using UnityEngine;

namespace MiniGames.Data
{
    
    [CreateAssetMenu(fileName = "RunnerCamaraData", menuName = "NC/Matches/Camera/RunnerCameraData", order = 0)]
    public class RunnerCameraDataSO : CameraDataSO
    {
        [Header("Player Follow Settings")]
        public Vector3 followOffset = new Vector3(0, 5, -10);
        public float followSpeed = 5f;
        public bool smoothFollow = true;
    }
}