using UnityEngine;
using System.Collections.Generic;
using MiniGames.Match3.Data;

namespace MiniGames.Match3.Core
{
    [RequireComponent(typeof(Match3LevelController))]
    public class GridController : MonoBehaviour
    {
        public Match3LevelController levelController;
        public CellController CellPrefab;

        public CellController[,] Cells;

        [SerializeField] private float padding = 1.2f;

        [ContextMenu("Generate Grid")]
        public void CreateGrid()
        {
            if (levelController == null || levelController.Match3Data == null ||
                levelController.Match3Data.GridData == null)
            {
                Debug.LogError("Match3Data or GridData is not set.");
                return;
            }

            var gridData = levelController.Match3Data.GridData;
            int gridSize = (int)gridData.GridSize;

            // Cells array'ini initialize et
            Cells = new CellController[gridSize, gridSize];

            // Mevcut grid'i temizle (Editor güvenli)
            ClearExistingGrid();

            // GridData'dan cells'i initialize et
            if (gridData.Cells == null)
            {
                Debug.LogError("GridData.Cells is not initialized.");
                return;
            }

            // Yeni grid oluştur
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    // World position: col = x, row = y (ama row'u ters çevir çünkü Unity'de Y yukarı doğru pozitif)
                    Vector3 position = new Vector3(col * padding, -row * padding, 0);
                    var cellObj = Instantiate(CellPrefab.gameObject, position, Quaternion.identity, transform);
                    cellObj.name = $"Cell_{row}_{col}";

                    CellController cellController = cellObj.GetComponent<CellController>();
                    // Grid position: x = col, y = row (0,0 = sol üst, 4,4 = sağ alt)
                    cellController.GridPosition = new Vector2Int(col, row);

                    // GridData'dan CellData'yı al ve set et
                    var cellData = gridData.Cells[row, col];
                    cellController.SetData(cellData);

                    // Cells array'ine kaydet: [row, col] = [y, x]
                    Cells[row, col] = cellController;
                }
            }
        }

        public void ClearExistingGrid()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
#if UNITY_EDITOR
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
#else
        Destroy(child.gameObject);
#endif
            }
        }
        
        /// <summary>
        /// Swap işlemini dener. Tüm kuralları kontrol eder.
        /// </summary>
        public bool TrySwap(CellController fromCell, Vector2Int direction)
        {
            if (fromCell == null) return false;
            
            // Hedef pozisyonu hesapla
            Vector2Int fromPos = fromCell.GridPosition;
            Vector2Int toPos = fromPos + direction;
            
            // Grid sınırları içinde mi kontrol et
            if (!IsValidPosition(toPos))
            {
                Debug.Log($"Swap failed: Target position {toPos} is out of bounds");
                return false;
            }
            
            CellController toCell = Cells[toPos.y, toPos.x];
            
            // Swap kurallarını kontrol et
            if (!CanSwapCells(fromCell, toCell))
            {
                Debug.Log($"Swap failed: Cannot swap {fromPos} with {toPos}");
                return false;
            }
            
            // Geçici swap yap ve match kontrol et
            SwapCellData(fromCell, toCell);
            
            bool hasMatches = CheckForMatches(fromPos) || CheckForMatches(toPos);
            
            if (hasMatches)
            {
                Debug.Log($"Swap successful: {fromPos} <-> {toPos}");
                
                // Animasyonlu swap gerçekleştir
                PerformAnimatedSwap(fromCell, toCell);
                
                // Sadece swap yapılan cell'lerden etkilenen eşleşmeleri bul ve animasyon tetikle
                TriggerMatchAnimationsForSwappedCells(fromCell, toCell);
                
                return true;
            }
            else
            {
                // Match bulunamadı, swap'i geri al
                SwapCellData(fromCell, toCell);
                
                // Swap yapılmaya çalışılan cell'lerde WrongMatch animasyonu tetikle
                fromCell.TriggerWrongMatchAnimation();
                toCell.TriggerWrongMatchAnimation();
                
                Debug.Log($"Swap failed: No matches found after swapping {fromPos} with {toPos}");
                return false;
            }
        }
        
        /// <summary>
        /// Sadece swap yapılan cell'lerden etkilenen eşleşmeleri bulur ve animasyon tetikler
        /// </summary>
        private void TriggerMatchAnimationsForSwappedCells(CellController cell1, CellController cell2)
        {
            List<CellController> allMatchedCells = new List<CellController>();
            
            // Cell1'den etkilenen eşleşmeleri bul
            List<CellController> cell1Matches = GetMatchedCellsAtPosition(cell1.GridPosition);
            foreach (var cell in cell1Matches)
            {
                if (!allMatchedCells.Contains(cell))
                {
                    allMatchedCells.Add(cell);
                }
            }
            
            // Cell2'den etkilenen eşleşmeleri bul
            List<CellController> cell2Matches = GetMatchedCellsAtPosition(cell2.GridPosition);
            foreach (var cell in cell2Matches)
            {
                if (!allMatchedCells.Contains(cell))
                {
                    allMatchedCells.Add(cell);
                }
            }
            
            // Debug log ile eşleşen cell'lerin listesini yazdır
            Debug.Log($"=== SWAP MATCH RESULTS ===");
            Debug.Log($"Swapped cells: {cell1.GridPosition} <-> {cell2.GridPosition}");
            Debug.Log($"Cell1 matches: {cell1Matches.Count}");
            Debug.Log($"Cell2 matches: {cell2Matches.Count}");
            Debug.Log($"Total unique matches: {allMatchedCells.Count}");
            
            string matchList = "Matched positions: ";
            foreach (var cell in allMatchedCells)
            {
                matchList += $"{cell.GridPosition} ";
            }
            Debug.Log(matchList);
            
            // Eşleşen tüm cell'lerde Match animasyonu tetikle
            foreach (var cell in allMatchedCells)
            {
                cell.TriggerMatchAnimation();
            }
            
            Debug.Log($"Match animations triggered for {allMatchedCells.Count} cells affected by swap");
        }
        
        /// <summary>
        /// Grid'deki tüm eşleşmeleri bulur ve Match animasyonu tetikler
        /// </summary>
        private void TriggerMatchAnimationsForAllMatches()
        {
            List<CellController> matchedCells = new List<CellController>();
            
            if (Cells == null) return;
            
            int gridSize = Cells.GetLength(0);
            
            // Tüm grid'i tara ve eşleşmeleri bul
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    Vector2Int position = new Vector2Int(col, row);
                    
                    if (CheckForMatches(position))
                    {
                        // Bu pozisyondaki eşleşmeleri bul ve listeye ekle
                        List<CellController> positionMatches = GetMatchedCellsAtPosition(position);
                        
                        foreach (var cell in positionMatches)
                        {
                            if (!matchedCells.Contains(cell))
                            {
                                matchedCells.Add(cell);
                            }
                        }
                    }
                }
            }
            
            // Eşleşen tüm cell'lerde Match animasyonu tetikle
            foreach (var cell in matchedCells)
            {
                cell.TriggerMatchAnimation();
            }
            
            Debug.Log($"Match animations triggered for {matchedCells.Count} cells");
        }
        
        /// <summary>
        /// Belirtilen pozisyonda 3'lü match var mı kontrol eder (Sarmal/Dallanma yapısı)
        /// </summary>
        private bool CheckForMatches(Vector2Int position)
        {
            if (!IsValidPosition(position)) return false;
            
            CellController cell = Cells[position.y, position.x];
            if (cell.IsEmpty() || cell.IsWall()) return false;
            
            Color targetColor = cell.CellData.Piece.Color;
            
            // Birbirine bağlı tüm aynı renkli cell'leri bul (sarmal yapı)
            HashSet<Vector2Int> connectedCells = new HashSet<Vector2Int>();
            FindConnectedCells(position, targetColor, connectedCells);
            
            // 3 veya daha fazla bağlı cell varsa match
            return connectedCells.Count >= 3;
        }
        
        /// <summary>
        /// Rekursif olarak bağlı tüm aynı renkli cell'leri bulur (sarmal yapı)
        /// </summary>
        private void FindConnectedCells(Vector2Int position, Color targetColor, HashSet<Vector2Int> visited)
        {
            // Bu pozisyon zaten ziyaret edildiyse veya geçersizse dur
            if (!IsValidPosition(position) || visited.Contains(position))
                return;
            
            CellController cell = Cells[position.y, position.x];
            
            // Boş, duvar veya farklı renk ise dur
            if (cell.IsEmpty() || cell.IsWall() || !ColorsMatch(cell.CellData.Piece.Color, targetColor))
                return;
            
            // Bu pozisyonu ziyaret edildi olarak işaretle
            visited.Add(position);
            
            // 4 yöne de (sağ, sol, yukarı, aşağı) rekursif olarak devam et
            FindConnectedCells(position + Vector2Int.right, targetColor, visited); // Sağ
            FindConnectedCells(position + Vector2Int.left, targetColor, visited);  // Sol
            FindConnectedCells(position + Vector2Int.up, targetColor, visited);    // Yukarı
            FindConnectedCells(position + Vector2Int.down, targetColor, visited);  // Aşağı
        }
        
        /// <summary>
        /// Belirtilen pozisyondaki tüm bağlı eşleşen cell'leri döndürür (sarmal yapı)
        /// </summary>
        private List<CellController> GetMatchedCellsAtPosition(Vector2Int position)
        {
            List<CellController> matchedCells = new List<CellController>();
            
            if (!IsValidPosition(position)) return matchedCells;
            
            CellController cell = Cells[position.y, position.x];
            if (cell.IsEmpty() || cell.IsWall()) 
                return matchedCells;
            
            Color targetColor = cell.CellData.Piece.Color;
            
            // Birbirine bağlı tüm aynı renkli cell'leri bul
            HashSet<Vector2Int> connectedPositions = new HashSet<Vector2Int>();
            FindConnectedCells(position, targetColor, connectedPositions);
            
            // 3 veya daha fazla bağlı cell varsa listeye ekle
            if (connectedPositions.Count >= 3)
            {
                foreach (var pos in connectedPositions)
                {
                    matchedCells.Add(Cells[pos.y, pos.x]);
                }
            }
            
            return matchedCells;
        }
        
        /// <summary>
        /// İki cell arasında animasyonlı swap gerçekleştirir
        /// </summary>
        private void PerformAnimatedSwap(CellController cell1, CellController cell2)
        {
            Vector3 cell1Position = cell1.transform.position;
            Vector3 cell2Position = cell2.transform.position;
            
            // Her iki cell'i de karşı pozisyona animasyonla götür
            cell1.MoveTo(cell2Position);
            cell2.MoveTo(cell1Position);
        }
        
        /// <summary>
        /// İki cell'in swap edilip edilemeyeceğini kontrol eder
        /// </summary>
        private bool CanSwapCells(CellController fromCell, CellController toCell)
        {
            // Null kontrolleri
            if (fromCell == null || toCell == null) return false;
            
            // Wall kontrolü - Duvar ile hiçbir şey swap edilemez
            if (fromCell.IsWall() || toCell.IsWall())
            {
                return false;
            }
            
            // En az bir cell'de piece olmalı
            if (fromCell.IsEmpty() && toCell.IsEmpty())
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// İki cell'in data'larını swap eder
        /// </summary>
        private void SwapCellData(CellController cell1, CellController cell2)
        {
            var cell1Position = cell1.GridPosition;
            var cell2Position = cell2.GridPosition;
           
            // Grid'deki cell'lerin pozisyonlarını güncelle
            Cells[cell1Position.y, cell1Position.x] = cell2;
            Cells[cell2Position.y, cell2Position.x] = cell1;
            // Cell'lerin grid pozisyonlarını güncelle
            cell1.GridPosition = cell2Position;
            cell2.GridPosition = cell1Position;
        }
        
        /// <summary>
        /// İki rengin eşleşip eşleşmediğini kontrol eder
        /// </summary>
        private bool ColorsMatch(Color color1, Color color2)
        {
            // Renk karşılaştırması için epsilon değeri kullan
            float threshold = 0.1f;
            return Mathf.Abs(color1.r - color2.r) < threshold &&
                   Mathf.Abs(color1.g - color2.g) < threshold &&
                   Mathf.Abs(color1.b - color2.b) < threshold;
        }
        
        /// <summary>
        /// Pozisyonun grid sınırları içinde olup olmadığını kontrol eder
        /// </summary>
        private bool IsValidPosition(Vector2Int position)
        {
            if (Cells == null) return false;
            
            int gridSize = Cells.GetLength(0);
            return position.x >= 0 && position.x < gridSize && 
                   position.y >= 0 && position.y < gridSize;
        }
    }
}

