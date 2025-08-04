using UnityEngine;
using System;

namespace MiniGames.Match3.Core
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private GridController gridController;
        
        private Vector2 startTouchPosition;
        private Vector2 endTouchPosition;
        private bool isTouching = false;
        private CellController selectedCell;
        
        private void Start()
        {
            if (gridController == null)
                gridController = FindObjectOfType<GridController>();
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            // Mouse/Touch input handling
            if (Input.GetMouseButtonDown(0))
            {
                startTouchPosition = Input.mousePosition;
                isTouching = true;
                
                // Hangi cell'e dokunulduğunu bul
                selectedCell = GetCellAtPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
            else if (Input.GetMouseButtonUp(0) && isTouching)
            {
                endTouchPosition = Input.mousePosition;
                isTouching = false;
                
                DetectSwipe();
            }
        }
        
        private CellController GetCellAtPosition(Vector3 worldPosition)
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            if (hit.collider != null)
            {
                return hit.collider.GetComponent<CellController>();
            }
            return null;
        }
        
        private void DetectSwipe()
        {
            if (selectedCell == null) return;
            
            Vector2 swipeVector = endTouchPosition - startTouchPosition;
            float swipeDistance = swipeVector.magnitude;
            
            if (swipeDistance < minSwipeDistance) return;
            
            Vector2Int swipeDirection = GetSwipeDirection(swipeVector);
            
            // GridController'a swipe isteği gönder
            if (gridController != null)
            {
                gridController.TrySwap(selectedCell, swipeDirection);
            }
        }
        
        private Vector2Int GetSwipeDirection(Vector2 swipeVector)
        {
            Vector2 normalizedSwipe = swipeVector.normalized;
            
            if (Mathf.Abs(normalizedSwipe.x) > Mathf.Abs(normalizedSwipe.y))
            {
                // Horizontal swipe
                return normalizedSwipe.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                // Vertical swipe - Unity screen koordinatlarında Y yukarı pozitif
                // Ama bizim grid sistemimizde Y aşağı pozitif
                return normalizedSwipe.y > 0 ? Vector2Int.down : Vector2Int.up;
            }
        }
    }
}