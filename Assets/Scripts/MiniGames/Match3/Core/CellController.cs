using MiniGames.Match3.Data;
using UnityEngine;

namespace MiniGames.Match3.Core
{
    [RequireComponent(typeof(Collider2D))]
    public class CellController : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer;
        public Vector2Int GridPosition;
        public CellData CellData;
        
        [SerializeField] private Animator animator;

        private void Awake()
        {
            // Add Collider2D if it doesn't exist
            if (GetComponent<Collider2D>() == null)
            {
                var collider = gameObject.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
            }
            
            // Get Animator reference
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        public void SetData(CellData data,bool trying = false)
        {
            CellData = data;
            if(trying)
                return;
            animator.SetTrigger("Idle");
            if (data != null && data.Piece != null)
            {
                SpriteRenderer.sprite = data.Piece.Sprite;
                SpriteRenderer.color = data.Piece.Color;
            }
            else
            {
                SpriteRenderer.sprite = null; // Clear sprite if no piece is assigned
            }
        }
        
        public bool IsWall()
        {
            return CellData?.Piece?.pieceTypeEnum == PieceTypeEnum.Wall;
        }
        
        public bool IsEmpty()
        {
            return CellData == null || CellData.Piece == null;
        }
        
        public bool CanSwap()
        {
            return !IsWall() && !IsEmpty();
        }
        
        // Method for changing position for animation
        public void MoveTo(Vector3 targetPosition, float duration = 0.3f)
        {
            StartCoroutine(MoveCoroutine(targetPosition, duration));
        }
        
        private System.Collections.IEnumerator MoveCoroutine(Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = transform.position;
            float elapsedTime = 0;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            
            transform.position = targetPosition;
        }
        
              
        /// <summary>
        /// Triggers match animation
        /// </summary>
        public void TriggerMatchAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("Match");
            }
        }
        
        /// <summary>
        /// Triggers wrong match animation
        /// </summary>
        public void TriggerWrongMatchAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("WrongMatch");
            }
        }

        public void ClearPiece()
        {
            CellData = null;
        }
    }
}