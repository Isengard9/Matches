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
            // Collider2D yoksa ekle
            if (GetComponent<Collider2D>() == null)
            {
                var collider = gameObject.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
            }
            
            // Animator referansını al
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        public void SetData(CellData data)
        {
            CellData = data;
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
            return CellData?.Piece?.Type == PieceType.Wall;
        }
        
        public bool IsEmpty()
        {
            return CellData == null || CellData.Piece == null;
        }
        
        public bool CanSwap()
        {
            return !IsWall() && !IsEmpty();
        }
        
        // Animasyon için pozisyon değiştirme metodu
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
        
        public void MoveLeft()
        {
            
        }
        
        public void MoveRight()
        {
            
        }
        
        public void MoveUp()
        {
            
        }
        
        public void MoveDown()
        {
            
        }
        
        /// <summary>
        /// Match animasyonu tetikler
        /// </summary>
        public void TriggerMatchAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("Match");
                Debug.Log($"Match animation triggered for cell at {GridPosition}");
            }
        }
        
        /// <summary>
        /// Wrong match animasyonu tetikler
        /// </summary>
        public void TriggerWrongMatchAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("WrongMatch");
                Debug.Log($"WrongMatch animation triggered for cell at {GridPosition}");
            }
        }
    }
}