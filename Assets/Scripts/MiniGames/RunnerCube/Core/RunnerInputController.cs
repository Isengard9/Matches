using UnityEngine;
using Core.Events.Level;
using Core.Managers;

namespace MiniGames.RunnerCube.Core
{
    public class RunnerInputController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private Vector2 horizontalLimits = new Vector2(-5f, 5f); // Left and right boundaries
        [SerializeField] private float movementSensitivity = 1f;
        
        [Header("Target GameObject")]
        [SerializeField] private Transform targetObject;
        
        [SerializeField] private bool canMove;
        private Vector2 lastTouchPosition;
        private bool isTouching;
        
        private void Start()
        {
            ManagerContainer.EventManager.Subscribe<LevelStartedEvent>(OnLevelStarted);
        }
        
        private void OnDestroy()
        {
            ManagerContainer.EventManager.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
        }
        
        private void OnLevelStarted(LevelStartedEvent e)
        {
            canMove = true;
        }
        
        private void LateUpdate()
        {
            if (!canMove || targetObject == null) return;
            
            HandleTouchInput();
        }
        
        private void HandleTouchInput()
        {
            // Mouse/Touch input handling
            if (Input.GetMouseButtonDown(0))
            {
                isTouching = true;
                lastTouchPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isTouching = false;
            }
            else if (Input.GetMouseButton(0) && isTouching)
            {
                Vector2 currentTouchPosition = Input.mousePosition;
                Vector2 deltaPosition = currentTouchPosition - lastTouchPosition;
                
                // Movement on X axis
                float horizontalMovement = deltaPosition.x * movementSensitivity * Time.deltaTime;
                
                // Get current position and calculate new position
                Vector3 currentPosition = targetObject.position;
                float newXPosition = currentPosition.x + horizontalMovement;
                
                // Check boundaries
                newXPosition = Mathf.Clamp(newXPosition, horizontalLimits.x, horizontalLimits.y);
                
                // Apply new position
                targetObject.position = new Vector3(newXPosition, currentPosition.y, currentPosition.z);
                
                lastTouchPosition = currentTouchPosition;
            }
        }
    }
}