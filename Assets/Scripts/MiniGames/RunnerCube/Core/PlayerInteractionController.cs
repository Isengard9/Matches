using UnityEngine;
using MiniGames.RunnerCube.Data;
using Core.Events.RunnerCube;
using Core.Managers;

namespace MiniGames.RunnerCube.Core
{
    public class PlayerInteractionController : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private Transform targetObject;
        [SerializeField] private Vector3 boxCastSize = new Vector3(1f, 1f, 1f);
        [SerializeField] private float detectionDistance = 2f;
        [SerializeField] private LayerMask interactableLayerMask = -1;
        
        [Header("Debug")]
        [SerializeField] private bool showGizmos = true;
        
        private void FixedUpdate()
        {
            if (targetObject == null) return;
            
            DetectInteractables();
        }
        
        private void DetectInteractables()
        {
            // Cast a box in front of the target object
            Vector3 origin = targetObject.position;
            Vector3 direction = targetObject.forward;
            
            // Perform box cast
            if (Physics.BoxCast(origin, boxCastSize * 0.5f, direction, out RaycastHit hit, 
                targetObject.rotation, detectionDistance, interactableLayerMask))
            {
                // Check if hit object has InteractableObjectController
                InteractableObjectController interactable = hit.collider.GetComponent<InteractableObjectController>();
                
                if (interactable != null && interactable.Data != null)
                {
                    HandleInteraction(interactable);
                }
            }
        }
        
        private void HandleInteraction(InteractableObjectController interactable)
        {
            // Check the type and publish appropriate event
            switch (interactable.Data.InteractableType)
            {
                case InteractableTypeEnum.Obstacle:
                    ManagerContainer.EventManager.Publish(new ObstacleTriggeredEvent 
                    { 
                        ObstacleObject = interactable 
                    });
                    break;
                    
                case InteractableTypeEnum.Collectible:
                    ManagerContainer.EventManager.Publish(new CollectibleTriggeredEvent 
                    { 
                        CollectibleObject = interactable 
                    });
                    break;
                    
                case InteractableTypeEnum.FinishLine:
                    ManagerContainer.EventManager.Publish(new FinishLineTriggeredEvent 
                    { 
                        FinishLineObject = interactable 
                    });
                    break;
                    
                case InteractableTypeEnum.None:
                default:
                    // Do nothing for None type or unknown types
                    break;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!showGizmos || targetObject == null) return;
            
            // Draw the box cast area
            Gizmos.color = Color.yellow;
            Vector3 origin = targetObject.position;
            Vector3 direction = targetObject.forward;
            Vector3 endPosition = origin + direction * detectionDistance;
            
            // Draw the box at origin
            Gizmos.matrix = Matrix4x4.TRS(origin, targetObject.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxCastSize);
            
            // Draw the box at end position
            Gizmos.matrix = Matrix4x4.TRS(endPosition, targetObject.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxCastSize);
            
            // Draw the cast direction
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, endPosition);
        }
    }
}