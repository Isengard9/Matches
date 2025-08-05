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
        
        [Header("Piece Generation")]
        [SerializeField] private List<PieceSO> availablePieces = new List<PieceSO>();

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
        /// Sadece swap yapılan cell'lerden etkilenen eşleşmeleri bulur ve match işlemlerini başlatır
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
            
            if (allMatchedCells.Count > 0)
            {
                // Eşleşen cell'lerde match animasyonu tetikle
                foreach (var cell in allMatchedCells)
                {
                    cell.TriggerMatchAnimation();
                }
                
                // Match işlemlerini başlat (animasyon + yok etme + yaratma)
                StartCoroutine(ProcessMatches(allMatchedCells));
            }
            
            Debug.Log($"Match processing started for {allMatchedCells.Count} cells affected by swap");
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
            
            PieceColorEnum targetColor = cell.CellData.Piece.pieceColor;
            
            // Birbirine bağlı tüm aynı renkli cell'leri bul (sarmal yapı)
            HashSet<Vector2Int> connectedCells = new HashSet<Vector2Int>();
            FindConnectedCells(position, targetColor, connectedCells);
            
            // 3 veya daha fazla bağlı cell varsa match
            return connectedCells.Count >= 3;
        }
        
        /// <summary>
        /// Rekursif olarak bağlı tüm aynı renkli cell'leri bulur (sarmal yapı)
        /// </summary>
        private void FindConnectedCells(Vector2Int position, PieceColorEnum targetColor, HashSet<Vector2Int> visited)
        {
            // Bu pozisyon zaten ziyaret edildiyse veya geçersizse dur
            if (!IsValidPosition(position) || visited.Contains(position))
                return;
            
            CellController cell = Cells[position.y, position.x];
            
            // Boş, duvar veya farklı renk ise dur
            if (cell.IsEmpty() || cell.IsWall() || !ColorsMatch(cell.CellData.Piece.pieceColor, targetColor))
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
            
            PieceColorEnum targetColor = cell.CellData.Piece.pieceColor;
            
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
        private bool ColorsMatch(PieceColorEnum color1, PieceColorEnum color2)
        {
            return color1 == color2;
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
        
        /// <summary>
        /// Eşleşen cell'leri işler: animasyon bekler, yok eder, yenilerini yaratır
        /// </summary>
        private System.Collections.IEnumerator ProcessMatches(List<CellController> matchedCells)
        {
            // 1. Match animasyonunun bitmesini bekle (1 saniye)
            yield return new WaitForSeconds(1.0f);
            
            // 2. Ek bekleme süresi (0.1 saniye)
            yield return new WaitForSeconds(0.2f);
            
            // 3. Özel piece'lerin etkilerini hesapla ve yok edilecek cell'leri genişlet
            HashSet<Vector2Int> cellsToDestroy = CalculateDestructionArea(matchedCells);
            
            // 4. Cell'leri yok et
            DestroyCells(cellsToDestroy);
            
            // 5. Boş alanları doldur (cascade kontrolü CreateNewPieces içinde yapılıyor)
            yield return StartCoroutine(CreateNewPieces());
        }
        
        /// <summary>
        /// Özel piece'lerin etkilerini hesaplar ve yok edilecek alanı genişletir
        /// </summary>
        private HashSet<Vector2Int> CalculateDestructionArea(List<CellController> matchedCells)
        {
            HashSet<Vector2Int> destructionArea = new HashSet<Vector2Int>();
            
            // Önce normal eşleşen cell'leri ekle
            foreach (var cell in matchedCells)
            {
                destructionArea.Add(cell.GridPosition);
            }
            
            // Özel piece'lerin etkilerini hesapla
            foreach (var cell in matchedCells)
            {
                
                var pieceType = cell.CellData.Piece.pieceTypeEnum;
                Vector2Int pos = cell.GridPosition;
                
                switch (pieceType)
                {
                    case PieceTypeEnum.Bomb:
                        AddBombDestructionArea(pos, destructionArea);
                        break;
                        
                    case PieceTypeEnum.Row:
                        AddRowDestructionArea(pos, destructionArea);
                        break;
                        
                    case PieceTypeEnum.Column:
                        AddColumnDestructionArea(pos, destructionArea);
                        break;
                }
            }
            
            Debug.Log($"Destruction area calculated: {destructionArea.Count} cells will be destroyed");
            return destructionArea;
        }
        
        /// <summary>
        /// Bomba için 2 birim genişliğinde kare alan ekler
        /// </summary>
        private void AddBombDestructionArea(Vector2Int bombPos, HashSet<Vector2Int> destructionArea)
        {
            // 2 birim genişlik = bombPos'dan her yöne 2 birim
            for (int row = bombPos.y - 2; row <= bombPos.y + 2; row++)
            {
                for (int col = bombPos.x - 2; col <= bombPos.x + 2; col++)
                {
                    Vector2Int pos = new Vector2Int(col, row);
                    if (IsValidPosition(pos) && !Cells[pos.y, pos.x].IsWall())
                    {
                        destructionArea.Add(pos);
                    }
                }
            }
            Debug.Log($"Bomb at {bombPos} added 5x5 destruction area");
        }
        
        /// <summary>
        /// Row piece için tüm satırı ekler
        /// </summary>
        private void AddRowDestructionArea(Vector2Int rowPos, HashSet<Vector2Int> destructionArea)
        {
            int gridSize = Cells.GetLength(0);
            
            // Tüm satırdaki cell'leri ekle
            for (int col = 0; col < gridSize; col++)
            {
                Vector2Int pos = new Vector2Int(col, rowPos.y);
                if (IsValidPosition(pos) && !Cells[pos.y, pos.x].IsWall())
                {
                    destructionArea.Add(pos);
                }
            }
            Debug.Log($"Row piece at {rowPos} added entire row {rowPos.y}");
        }
        
        /// <summary>
        /// Column piece için tüm sütunu ekler
        /// </summary>
        private void AddColumnDestructionArea(Vector2Int colPos, HashSet<Vector2Int> destructionArea)
        {
            int gridSize = Cells.GetLength(0);
            
            // Tüm sütundaki cell'leri ekle
            for (int row = 0; row < gridSize; row++)
            {
                Vector2Int pos = new Vector2Int(colPos.x, row);
                if (IsValidPosition(pos) && !Cells[pos.y, pos.x].IsWall())
                {
                    destructionArea.Add(pos);
                }
            }
            Debug.Log($"Column piece at {colPos} added entire column {colPos.x}");
        }
        
        /// <summary>
        /// Belirtilen pozisyonlardaki cell'leri yok eder
        /// </summary>
        private void DestroyCells(HashSet<Vector2Int> positionsToDestroy)
        {
            foreach (var pos in positionsToDestroy)
            {
                if (IsValidPosition(pos))
                {
                    CellController cell = Cells[pos.y, pos.x];
                    if (!cell.IsEmpty() && !cell.IsWall())
                    {
                        // Cell'i boş yap
                        cell.ClearPiece();
                        Debug.Log($"Destroyed cell at {pos}");
                    }
                }
            }
            
            Debug.Log($"Destroyed {positionsToDestroy.Count} cells");
        }
        
        /// <summary>
        /// Boş alanları availablePieces listesinden seçilen piece'lerle doldurur
        /// </summary>
        private System.Collections.IEnumerator CreateNewPieces()
        {
            if (Cells == null || availablePieces == null || availablePieces.Count == 0) 
            {
                Debug.LogWarning("Cannot create new pieces: availablePieces list is empty or null");
                yield break;
            }
            
            int gridSize = Cells.GetLength(0);
            bool hasNewPieces = false;
            
            // Her sütunu aşağıdan yukarıya doğru kontrol et
            for (int col = 0; col < gridSize; col++)
            {
                for (int row = gridSize - 1; row >= 0; row--)
                {
                    Vector2Int pos = new Vector2Int(col, row);
                    CellController cell = Cells[pos.y, pos.x];
                    
                    // Boş cell ve duvar değilse yeni piece oluştur
                    if (cell.IsEmpty() && !cell.IsWall())
                    {
                        CreateNewPieceAt(pos);
                        hasNewPieces = true;
                    }
                }
            }
            
            if (hasNewPieces)
            {
                Debug.Log("New pieces created to fill empty spaces");
                // Yeni piece'lerin yerleşmesi için kısa bekleme
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        /// <summary>
        /// Belirtilen pozisyonda availablePieces listesinden rastgele bir piece oluşturur
        /// </summary>
        private void CreateNewPieceAt(Vector2Int position)
        {
            if (!IsValidPosition(position)) return;
            
            CellController cell = Cells[position.y, position.x];
            if (cell.IsWall()) return;
            
            if (availablePieces == null || availablePieces.Count == 0)
            {
                Debug.LogWarning("availablePieces list is empty. Cannot create new piece.");
                return;
            }
            
            // availablePieces listesinden rastgele bir piece seç
            PieceSO selectedPiece = availablePieces[Random.Range(0, availablePieces.Count)];
            
            // Piece'in kopyasını oluştur (ScriptableObject referansını korumak için)
            var newPieceData = ScriptableObject.CreateInstance<PieceSO>();
            newPieceData.pieceTypeEnum = selectedPiece.pieceTypeEnum;
            newPieceData.Color = selectedPiece.Color;
            newPieceData.Sprite = selectedPiece.Sprite;
            newPieceData.pieceColor = selectedPiece.pieceColor;
            
            var cellData = new CellData
            {
                Piece = newPieceData,
            };
            // Cell'e yeni piece'i set et
            cell.SetData(cellData);
            
            //Debug.Log($"Created new {newPieceData.pieceTypeEnum} piece with color {newPieceData.Color} at {position}");
        }
        
        /// <summary>
        /// Yeni piece'ler oluşturulduktan sonra cascade eşleşmeleri kontrol eder
        /// </summary>
        private System.Collections.IEnumerator CheckForCascadingMatches()
        {
            // Tüm grid'deki yeni eşleşmeleri bul
            List<CellController> newMatches = GetAllMatchedCells();
            
            if (newMatches.Count > 0)
            {
                Debug.Log($"Cascading matches found: {newMatches.Count} cells");
                
                // Yeni eşleşmelerde animasyon tetikle
                foreach (var cell in newMatches)
                {
                    cell.TriggerMatchAnimation();
                }
                
                // Yeni eşleşmeleri tekrar işle (recursive)
                yield return StartCoroutine(ProcessMatches(newMatches));
            }
            else
            {
                Debug.Log("No cascading matches found. Match processing complete.");
            }
        }
        
        /// <summary>
        /// Grid'deki tüm eşleşmeleri bulur ve döndürür (cascade kontrolü için)
        /// </summary>
        private List<CellController> GetAllMatchedCells()
        {
            List<CellController> allMatchedCells = new List<CellController>();
            HashSet<Vector2Int> processedPositions = new HashSet<Vector2Int>();
            
            if (Cells == null) return allMatchedCells;
            
            int gridSize = Cells.GetLength(0);
            
            // Tüm grid'i tara
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    Vector2Int position = new Vector2Int(col, row);
                    
                    // Bu pozisyon zaten işlendiyse atla
                    if (processedPositions.Contains(position))
                        continue;
                    
                    // Bu pozisyonda eşleşme var mı kontrol et
                    if (CheckForMatches(position))
                    {
                        // Bu pozisyondaki tüm bağlı eşleşmeleri bul
                        List<CellController> positionMatches = GetMatchedCellsAtPosition(position);
                        
                        foreach (var cell in positionMatches)
                        {
                            if (!allMatchedCells.Contains(cell))
                            {
                                allMatchedCells.Add(cell);
                                processedPositions.Add(cell.GridPosition);
                            }
                        }
                    }
                }
            }
            
            return allMatchedCells;
        }
    }
}
