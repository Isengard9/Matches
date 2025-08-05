using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniGames.Match3.Data;

namespace MiniGames.Match3.Core
{
    /// <summary>
    /// Match3 oyununda eşleşme işlemlerini yöneten sınıf
    /// </summary>
    public class MatchController : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private GridController gridController;

        [Header("Match Settings")] [SerializeField]
        private float matchProcessDelay = 1.0f;

        [SerializeField] private float additionalDelay = 0.2f;
        [SerializeField] private float destructionEffectDelay = 0.05f; // Her cell yok etme arasındaki efekt gecikmesi
        [SerializeField] private float animationStartDelay = 0.1f; // Her animasyon başlatma arasındaki gecikme
        [SerializeField] private float animationDuration = 0.5f; // Match animasyonunun süresi

        private void Awake()
        {
            if (gridController == null)
                gridController = GetComponent<GridController>();
        }

        /// <summary>
        /// Swap yapılan cell'lerden etkilenen eşleşmeleri bulur ve işler
        /// </summary>
        public void ProcessSwapMatches(CellController cell1, CellController cell2)
        {
            HashSet<CellController> allMatchedCells = new HashSet<CellController>();
            
            // Her iki cell'den etkilenen eşleşmeleri bul ve HashSet'e ekle
            AddMatchedCellsToSet(cell1.GridPosition, allMatchedCells);
            AddMatchedCellsToSet(cell2.GridPosition, allMatchedCells);
            
            // Debug log ile eşleşen cell'lerin listesini yazdır
            Debug.Log($"=== SWAP MATCH RESULTS ===");
            Debug.Log($"Swapped cells: {cell1.GridPosition} <-> {cell2.GridPosition}");
            Debug.Log($"Total unique matches: {allMatchedCells.Count}");
            
            if (allMatchedCells.Count > 0)
            {
                // Animasyonları gecikmeyle başlat ve ardından ProcessMatches'i çağır
                StartCoroutine(TriggerMatchAnimationsAndProcess(allMatchedCells));
            }
            
            Debug.Log($"Match processing started for {allMatchedCells.Count} cells affected by swap");
        }

        public void ProcessMatchesAtCell(CellController startCell)
        {
            HashSet<CellController> allMatchedCells = new HashSet<CellController>();
            
            // Cell'den etkilenen eşleşmeleri bul
            AddMatchedCellsToSet(startCell.GridPosition, allMatchedCells);
            
            // Debug log ile eşleşen cell'lerin listesini yazdır
            Debug.Log($"=== MATCH RESULTS ===");
            Debug.Log($"Total unique matches: {allMatchedCells.Count}");
            
            if (allMatchedCells.Count > 0)
            {
                // Animasyonları gecikmeyle başlat ve ardından ProcessMatches'i çağır
                StartCoroutine(TriggerMatchAnimationsAndProcess(allMatchedCells));
            }
            
            Debug.Log($"Match processing started for {allMatchedCells.Count} cells affected by create");
        }
        
        /// <summary>
        /// Belirtilen pozisyondaki eşleşen cell'leri HashSet'e ekler
        /// </summary>
        private void AddMatchedCellsToSet(Vector2Int position, HashSet<CellController> targetSet)
        {
            if (!gridController.IsValidPosition(position)) return;

            CellController cell = gridController.Cells[position.y, position.x];
            if (cell.IsEmpty() || cell.IsWall()) return;

            PieceColorEnum targetColor = cell.CellData.Piece.pieceColor;

            // Birbirine bağlı tüm aynı renkli cell'lerin pozisyonlarını bul
            HashSet<Vector2Int> connectedPositions = new HashSet<Vector2Int>();
            FindConnectedCells(position, targetColor, connectedPositions);

            // 3 veya daha fazla bağlı cell varsa HashSet'e ekle
            if (connectedPositions.Count >= 3)
            {
                foreach (var pos in connectedPositions)
                {
                    targetSet.Add(gridController.Cells[pos.y, pos.x]);
                }
            }
        }
        
        /// <summary>
        /// Belirtilen pozisyonda 3'lü match var mı kontrol eder
        /// </summary>
        public bool CheckForMatches(Vector2Int position)
        {
            if (!gridController.IsValidPosition(position)) return false;

            CellController cell = gridController.Cells[position.y, position.x];
            if (cell.IsEmpty() || cell.IsWall()) return false;

            PieceColorEnum targetColor = cell.CellData.Piece.pieceColor;

            // Birbirine bağlı tüm aynı renkli cell'leri bul (sarmal yapı)
            HashSet<Vector2Int> connectedCells = new HashSet<Vector2Int>();
            FindConnectedCells(position, targetColor, connectedCells);

            // 3 veya daha fazla bağlı cell varsa match
            return connectedCells.Count >= 3;
        }

        /// <summary>
        /// Belirtilen pozisyondaki tüm bağlı eşleşen cell'leri döndürür
        /// </summary>
        private List<CellController> GetMatchedCellsAtPosition(Vector2Int position)
        {
            List<CellController> matchedCells = new List<CellController>();

            if (!gridController.IsValidPosition(position)) return matchedCells;

            CellController cell = gridController.Cells[position.y, position.x];
            if (cell.IsEmpty() || cell.IsWall())
                return matchedCells;

            PieceColorEnum targetColor = cell.CellData.Piece.pieceColor;

            // Birbirine bağlı tüm aynı renkli cell'leri bul
            HashSet<Vector2Int> connectedPositions = new HashSet<Vector2Int>();
            FindConnectedCells(position, targetColor, connectedPositions);

            // 3 veya daha fazla bağlı cell varsa listeye ekle
            if (connectedPositions.Count >= 3)
            {
                foreach (var pos in connectedPositions)
                {
                    matchedCells.Add(gridController.Cells[pos.y, pos.x]);
                }
            }

            return matchedCells;
        }

        /// <summary>
        /// Rekursif olarak bağlı tüm aynı renkli cell'leri bulur
        /// </summary>
        private void FindConnectedCells(Vector2Int position, PieceColorEnum targetColor, HashSet<Vector2Int> visited)
        {
            // Bu pozisyon zaten ziyaret edildiyse veya geçersizse dur
            if (!gridController.IsValidPosition(position) || visited.Contains(position))
                return;

            CellController cell = gridController.Cells[position.y, position.x];

            // Boş, duvar veya farklı renk ise dur
            if (cell.IsEmpty() || cell.IsWall() || !ColorsMatch(cell.CellData.Piece.pieceColor, targetColor))
                return;

            // Bu pozisyonu ziyaret edildi olarak işaretle
            visited.Add(position);

            // 4 yöne de (sağ, sol, yukarı, aşağı) rekursif olarak devam et
            FindConnectedCells(position + Vector2Int.right, targetColor, visited); // Sağ
            FindConnectedCells(position + Vector2Int.left, targetColor, visited); // Sol
            FindConnectedCells(position + Vector2Int.up, targetColor, visited); // Yukarı
            FindConnectedCells(position + Vector2Int.down, targetColor, visited); // Aşağı
        }

        /// <summary>
        /// Eşleşen cell'lerde match animasyonu gecikmeyle tetikler
        /// </summary>
        private IEnumerator TriggerMatchAnimations(HashSet<CellController> matchedCells)
        {
            foreach (var cell in matchedCells)
            {
                cell.TriggerMatchAnimation();
                yield return new WaitForSeconds(animationStartDelay);
            }
        }
        
        /// <summary>
        /// Animasyonları tetikler ve ardından eşleşmeleri işler
        /// </summary>
        private IEnumerator TriggerMatchAnimationsAndProcess(HashSet<CellController> matchedCells)
        {
            // Tüm etkili cell'leri hesapla (normal matches + special piece effects)
            HashSet<CellController> allCellsToDestroy = ProcessAllMatches(matchedCells);
            Debug.Log($"Total cells to destroy: {allCellsToDestroy.Count} (including special piece effects)");
            
            // Önce animasyonları tetikle
            yield return StartCoroutine(TriggerMatchAnimations(allCellsToDestroy));
            
            // Animasyon süresi kadar bekle
            yield return new WaitForSeconds(animationDuration);
            
            // Cell'leri gerçekten yok et
            foreach (var cell in allCellsToDestroy)
            {
                if (cell != null && !cell.IsEmpty())
                {
                    cell.ClearPiece();
                    Debug.Log($"Destroyed piece at {cell.GridPosition}");
                }
            }
            
            Debug.Log($"Cleared {allCellsToDestroy.Count} cells, now creating new pieces...");
            
            // Yeni piece'leri oluştur
            yield return StartCoroutine(gridController.CreateNewPieces());
        }
        
        /// <summary>
        /// Tüm eşleşmeleri ve özel piece etkilerini hesaplar
        /// </summary>
        private HashSet<CellController> ProcessAllMatches(HashSet<CellController> initialMatches)
        {
            HashSet<CellController> allCellsToDestroy = new HashSet<CellController>(initialMatches);
            
            // Özel piece'lerin etkilerini hesapla ve ekle
            HashSet<CellController> specialEffects = CalculateSpecialPieceEffects(initialMatches);
            allCellsToDestroy.UnionWith(specialEffects);
            
            Debug.Log($"Initial matches: {initialMatches.Count}, Special effects: {specialEffects.Count}, Total unique: {allCellsToDestroy.Count}");
            return allCellsToDestroy;
        }
        
        /// <summary>
        /// Özel piece'lerin etkilerini hesaplar
        /// </summary>
        private HashSet<CellController> CalculateSpecialPieceEffects(HashSet<CellController> matchedCells)
        {
            HashSet<CellController> specialEffects = new HashSet<CellController>();
            
            // Her matched cell'i kontrol et, özel piece ise etkilerini hesapla
            foreach (var cell in matchedCells)
            {
                if (cell.CellData?.Piece == null) continue;
                
                var pieceType = cell.CellData.Piece.pieceTypeEnum;
                Vector2Int pos = cell.GridPosition;

                switch (pieceType)
                {
                    case PieceTypeEnum.Bomb:
                        AddBombEffects(pos, specialEffects);
                        break;

                    case PieceTypeEnum.Row:
                        AddRowEffects(pos, specialEffects);
                        break;

                    case PieceTypeEnum.Column:
                        AddColumnEffects(pos, specialEffects);
                        break;
                }
            }
            
            return specialEffects;
        }
        
        /// <summary>
        /// Bomba etkilerini hesaplar (3x3 alan)
        /// </summary>
        private void AddBombEffects(Vector2Int bombPos, HashSet<CellController> effects)
        {
            for (int row = bombPos.y - 1; row <= bombPos.y + 1; row++)
            {
                for (int col = bombPos.x - 1; col <= bombPos.x + 1; col++)
                {
                    Vector2Int pos = new Vector2Int(col, row);
                    if (gridController.IsValidPosition(pos) && !gridController.Cells[pos.y, pos.x].IsWall())
                    {
                        effects.Add(gridController.Cells[pos.y, pos.x]);
                        Debug.Log($"Bomb at {bombPos} affects cell at {pos}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Row piece etkilerini hesaplar (tüm satır)
        /// </summary>
        private void AddRowEffects(Vector2Int rowPos, HashSet<CellController> effects)
        {
            int gridSize = gridController.Cells.GetLength(0);
            
            for (int col = 0; col < gridSize; col++)
            {
                Vector2Int pos = new Vector2Int(col, rowPos.y);
                if (gridController.IsValidPosition(pos) && !gridController.Cells[pos.y, pos.x].IsWall())
                {
                    effects.Add(gridController.Cells[pos.y, pos.x]);
                    Debug.Log($"Row piece at {rowPos} affects cell at {pos}");
                }
            }
        }
        
        /// <summary>
        /// Column piece etkilerini hesaplar (tüm sütun)
        /// </summary>
        private void AddColumnEffects(Vector2Int colPos, HashSet<CellController> effects)
        {
            int gridSize = gridController.Cells.GetLength(0);
            
            for (int row = 0; row < gridSize; row++)
            {
                Vector2Int pos = new Vector2Int(colPos.x, row);
                if (gridController.IsValidPosition(pos) && !gridController.Cells[pos.y, pos.x].IsWall())
                {
                    effects.Add(gridController.Cells[pos.y, pos.x]);
                    Debug.Log($"Column piece at {colPos} affects cell at {pos}");
                }
            }
        }
        
        /// <summary>
        /// Pozisyonların merkez noktasını hesaplar
        /// </summary>
        private Vector2 CalculateCenterPoint(List<Vector2Int> positions)
        {
            if (positions.Count == 0) return Vector2.zero;

            float totalX = 0, totalY = 0;
            foreach (var pos in positions)
            {
                totalX += pos.x;
                totalY += pos.y;
            }

            return new Vector2(totalX / positions.Count, totalY / positions.Count);
        }

        /// <summary>
        /// Belirtilen pozisyonda özel piece oluşturur
        /// </summary>
        private void CreateSpecialPieceAt(Vector2Int position, PieceTypeEnum pieceType)
        {
            if (!gridController.IsValidPosition(position)) return;

            CellController cell = gridController.Cells[position.y, position.x];
            if (cell.IsWall()) return;

            // Rastgele renk seç (özel piece'ler herhangi bir renkte olabilir)
            var availablePieces = gridController.GetComponent<GridController>().AvailablePieces;
            if (availablePieces == null || availablePieces.Count == 0) return;

            var basePiece = availablePieces[Random.Range(0, availablePieces.Count)];

            // Özel piece oluştur
            var specialPieceData = ScriptableObject.CreateInstance<PieceSO>();
            specialPieceData.pieceTypeEnum = pieceType;
            specialPieceData.Color = basePiece.Color;
            specialPieceData.Sprite = basePiece.Sprite; // Burada özel sprite kullanılabilir
            specialPieceData.pieceColor = basePiece.pieceColor;

            var cellData = new CellData
            {
                Piece = specialPieceData,
            };

            cell.SetData(cellData);
            Debug.Log($"Created special {pieceType} piece at {position}");
        }

        /// <summary>
        /// İki rengin eşleşip eşleşmediğini kontrol eder
        /// </summary>
        private bool ColorsMatch(PieceColorEnum color1, PieceColorEnum color2)
        {
            return color1 == color2;
        }
    }
}
