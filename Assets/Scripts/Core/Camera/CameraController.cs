using UnityEngine;
using Core.Events.Level;
using Core.Managers;
using MiniGames.Data;
using MiniGames.RunnerCube.Core;
using MiniGames.RunnerCube.Data;

namespace Core.Controllers.Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera cameraComponent;
        
        [Header("Player Follow Settings")]
        [SerializeField] private Vector3 followOffset = new Vector3(0, 5, -10);
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private bool smoothFollow = true;
        
        private bool followPlayer = false;
        private Transform playerTransform;
        
        private void Awake()
        {
            if (cameraComponent == null)
            {
                cameraComponent = GetComponent<UnityEngine.Camera>();
            }
        }

        private void Start()
        {
            ManagerContainer.EventManager.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
        }

        private void OnDestroy()
        {
            ManagerContainer.EventManager.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);
        }

        private void Update()
        {
            if (followPlayer && playerTransform != null)
            {
                FollowPlayer();
            }
        }
        
        private void OnLevelLoaded(LevelLoadedEvent @event)
        {
            var levelData = @event.Level as Level;
            if (levelData != null && levelData.CameraData != null)
            {
                
                
                if (levelData is RunnerDataSO)
                {
                    followPlayer = true;
                    FindPlayer();
                    ApplyCameraSettings(levelData.CameraData as RunnerCameraDataSO);
                }
                else
                {
                    followPlayer = false;
                    playerTransform = null;
                    ApplyCameraSettings(levelData.CameraData);
                }
            }
            
        }
        
        private void FindPlayer()
        {
            // RunnerPlayerController'ı sahneде bul
            var playerController = FindObjectOfType<RunnerPlayerController>().playerObject;
            if (playerController != null)
            {
                playerTransform = playerController.transform;
            }
        }

        private void ApplyCameraSettings(CameraDataSO cameraData)
        {
            if (cameraComponent == null) return;

            transform.position = cameraData.originalPosition;
            transform.rotation = Quaternion.Euler(cameraData.originalRotation);
            transform.localScale = cameraData.originalScale;

            cameraComponent.orthographic = cameraData.UseOrtographic;
        }

        private void ApplyCameraSettings(RunnerCameraDataSO cameraData)
        {
            if (cameraComponent == null) return;

            transform.position = cameraData.originalPosition;
            transform.rotation = Quaternion.Euler(cameraData.originalRotation);
            transform.localScale = cameraData.originalScale;

            cameraComponent.orthographic = cameraData.UseOrtographic;
            followOffset = cameraData.followOffset;
            followSpeed = cameraData.followSpeed;
            smoothFollow = cameraData.smoothFollow;
            
        }


        private void FollowPlayer()
        {
            if (playerTransform == null) return;
            
            Vector3 targetPosition = playerTransform.position + followOffset;
            Vector3 currentPosition = transform.position;
            
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y, targetPosition.z);
            
            if (smoothFollow)
            {
                transform.position = Vector3.Lerp(currentPosition, newPosition, followSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = newPosition;
            }
        }
        
    }
}