using UnityEngine;
using Core.Events.Level;
using Core.Events.RunnerCube;
using Core.Managers;
using System.Collections;

namespace MiniGames.RunnerCube.Core
{
    public class RunnerPlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private Transform playerObject;
        [SerializeField] private float forwardSpeed = 5f;
        [SerializeField] private float slowDownDuration = 2f;
        
        [Header("Scale Animation Settings")]
        [SerializeField] private float scaleUpAmount = 0.3f;
        [SerializeField] private float scaleDownAmount = 0.2f;
        [SerializeField] private float scaleAnimationDuration = 0.5f;
        
        [Header("Game Rules")]
        [SerializeField] private float minimumScale = 1f;
        
        private bool isMoving;
        private Vector3 originalScale;
        private Coroutine currentScaleCoroutine;
        private Coroutine slowDownCoroutine;
        
        private void Start()
        {
            // Subscribe to events
            ManagerContainer.EventManager.Subscribe<LevelStartedEvent>(OnLevelStarted);
            ManagerContainer.EventManager.Subscribe<ObstacleTriggeredEvent>(OnObstacleTriggered);
            ManagerContainer.EventManager.Subscribe<CollectibleTriggeredEvent>(OnCollectibleTriggered);
            ManagerContainer.EventManager.Subscribe<FinishLineTriggeredEvent>(OnFinishLineTriggered);
            
            // Store original scale
            if (playerObject != null)
            {
                originalScale = playerObject.localScale;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            ManagerContainer.EventManager.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
            ManagerContainer.EventManager.Unsubscribe<ObstacleTriggeredEvent>(OnObstacleTriggered);
            ManagerContainer.EventManager.Unsubscribe<CollectibleTriggeredEvent>(OnCollectibleTriggered);
            ManagerContainer.EventManager.Unsubscribe<FinishLineTriggeredEvent>(OnFinishLineTriggered);
            
            // Stop coroutines
            if (currentScaleCoroutine != null)
                StopCoroutine(currentScaleCoroutine);
            if (slowDownCoroutine != null)
                StopCoroutine(slowDownCoroutine);
        }
        
        private void Update()
        {
            if (isMoving && playerObject != null)
            {
                // Move forward continuously
                playerObject.Translate(Vector3.forward * (forwardSpeed * Time.deltaTime));
                
                // Check if scale dropped below minimum
                CheckScaleFailure();
            }
        }
        
        private void OnLevelStarted(LevelStartedEvent e)
        {
            isMoving = true;
        }
        
        private void OnObstacleTriggered(ObstacleTriggeredEvent e)
        {
            if (playerObject == null) return;
            
            // Scale down the player
            Vector3 newScale = playerObject.localScale;
            newScale.y -= scaleDownAmount;
            
            AnimateScale(newScale);
        }
        
        private void OnCollectibleTriggered(CollectibleTriggeredEvent e)
        {
            if (playerObject == null) return;
            
            // Scale up the player
            Vector3 newScale = playerObject.localScale;
            newScale.y += scaleUpAmount;
            
            AnimateScale(newScale);
            
            // Destroy the collectible object
            if (e.CollectibleObject != null)
            {
                Destroy(e.CollectibleObject.gameObject);
            }
        }
        
        private void OnFinishLineTriggered(FinishLineTriggeredEvent e)
        {
            // Gradually slow down and stop
            SlowDownAndStop();
            
            // Publish level ended event
            ManagerContainer.EventManager.Publish(new LevelEndedEvent());
        }
        
        private void AnimateScale(Vector3 targetScale)
        {
            if (playerObject == null) return;
            
            // Stop current scale animation if running
            if (currentScaleCoroutine != null)
                StopCoroutine(currentScaleCoroutine);
            
            // Start new scale animation
            currentScaleCoroutine = StartCoroutine(ScaleAnimation(targetScale));
        }
        
        private IEnumerator ScaleAnimation(Vector3 targetScale)
        {
            Vector3 startScale = playerObject.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < scaleAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / scaleAnimationDuration;
                
                // Apply easing (OutBounce effect simulation)
                float easedProgress = EaseOutBounce(progress);
                
                playerObject.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);
                yield return null;
            }
            
            playerObject.localScale = targetScale;
            currentScaleCoroutine = null;
        }
        
        private void SlowDownAndStop()
        {
            if (!isMoving) return;
            
            // Stop current slow down if running
            if (slowDownCoroutine != null)
                StopCoroutine(slowDownCoroutine);
                
            // Start slow down animation
            slowDownCoroutine = StartCoroutine(SlowDownAnimation());
        }
        
        private IEnumerator SlowDownAnimation()
        {
            float startSpeed = forwardSpeed;
            float elapsedTime = 0f;
            
            while (elapsedTime < slowDownDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / slowDownDuration;
                
                // Apply easing (OutQuart effect)
                float easedProgress = EaseOutQuart(progress);
                
                forwardSpeed = Mathf.Lerp(startSpeed, 0f, easedProgress);
                yield return null;
            }
            
            forwardSpeed = 0f;
            isMoving = false;
            slowDownCoroutine = null;
            
            // Reset speed for next level
            forwardSpeed = startSpeed;
        }
        
        private void CheckScaleFailure()
        {
            if (playerObject != null && playerObject.localScale.y < minimumScale)
            {
                // Stop movement
                isMoving = false;
                
                // Stop all animations
                if (currentScaleCoroutine != null)
                    StopCoroutine(currentScaleCoroutine);
                if (slowDownCoroutine != null)
                    StopCoroutine(slowDownCoroutine);
                
                // Publish level failed event
                ManagerContainer.EventManager.Publish(new LevelFailedEvent());
            }
        }
        
        // Custom easing functions
        private float EaseOutBounce(float t)
        {
            if (t < 1f / 2.75f)
            {
                return 7.5625f * t * t;
            }
            else if (t < 2f / 2.75f)
            {
                return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            }
            else if (t < 2.5f / 2.75f)
            {
                return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            }
            else
            {
                return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
            }
        }
        
        private float EaseOutQuart(float t)
        {
            return 1f - Mathf.Pow(1f - t, 4f);
        }
    }
}