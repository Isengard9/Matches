using System.Collections;
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

        [Header("Piece Generation")] [SerializeField]
        private List<PieceSO> availablePieces = new List<PieceSO>();

        public List<PieceSO> AvailablePieces => availablePieces;
        [SerializeField] private float pieceGenerationDelay = 0.1f; // Her piece oluşturma arasındaki bekleme

        [SerializeField]
        private int maxGenerationAttempts = 10; // Eşleşme olmayan piece bulmak için maksimum deneme sayısı

        [SerializeField] private bool preventAutoMatches = true; // Otomatik eşleşmeleri engelle

        [Header("Match Controller")] [SerializeField]
        private MatchController matchController;

        [Header("Special Piece Chance")]
        [SerializeField] private float specialPieceSpawnChance = 0.1f; // Özel piece'lerin oluşma olasılığı (0-1 arası)
        [SerializeField] private bool limitSpecialPiecesOnInit = true; // Oyun başlangıcında özel piece'leri sınırlandır

        private void Awake()
        {
            if (matchController == null)
                matchController = GetComponent<MatchController>();
        }

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
        /// Sadece swap yapılan cell'lerden etkilenen eşleşmeleri bulır ve match işlemlerini başlatır
        /// </summary>
        private void TriggerMatchAnimationsForSwappedCells(CellController cell1, CellController cell2)
        {
            if (matchController != null)
            {
                matchController.ProcessSwapMatches(cell1, cell2);
            }
            else
            {
                Debug.LogError("MatchController is not assigned!");
            }
        }

        /// <summary>
        /// Belirtilen pozisyonda 3'lü match var mı kontrol eder (MatchController'dan delege edilir)
        /// </summary>
        private bool CheckForMatches(Vector2Int position)
        {
            if (matchController != null)
            {
                return matchController.CheckForMatches(position);
            }

            return false;
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
        public bool IsValidPosition(Vector2Int position)
        {
            if (Cells == null) return false;

            int gridSize = Cells.GetLength(0);
            return position.x >= 0 && position.x < gridSize &&
                   position.y >= 0 && position.y < gridSize;
        }

        /// <summary>
        /// Boş alanları availablePieces listesinden seçilen piece'lerle doldurur
        /// </summary>
        public IEnumerator CreateNewPieces()
        {
            if (Cells == null || availablePieces == null || availablePieces.Count == 0)
            {
                Debug.LogWarning("Cannot create new pieces: availablePieces list is empty or null");
                yield break;
            }

            int gridSize = Cells.GetLength(0);
            bool hasNewPieces = false;
            var createdPieces = new List<Vector2Int>();
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
                        createdPieces.Add(pos);
                        hasNewPieces = true;
                    }
                }
            }

            if (hasNewPieces)
            {
                Debug.Log("New pieces created to fill empty spaces");
                // Yeni piece'lerin yerleşmesi için kısa bekleme
                yield return new WaitForSeconds(0.2f);

                // Cascade eşleşmeleri kontrol et
                //yield return StartCoroutine(CheckForCascadingMatches());
                yield return StartCoroutine(CheckForCascadingMatches(createdPieces));
            }
        }

        /// <summary>
        /// Belirtilen pozisyonda availablePieces listesinden rastgele bir piece oluşturur
        /// Eşleşme kontrolü yaparak otomatik match'leri engeller
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

            PieceSO selectedPiece = null;

            if (preventAutoMatches)
            {
                // Eşleşme yapmayacak piece bulmaya çalış
                selectedPiece = FindNonMatchingPiece(position);
            }

            // Eğer eşleşmeyen piece bulunamadıysa veya preventAutoMatches false ise rastgele seç
            if (selectedPiece == null)
            {
                selectedPiece = availablePieces[Random.Range(0, availablePieces.Count)];
            }

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
        /// Belirtilen pozisyonda eşleşme yapmayacak bir piece bulmaya çalışır
        /// </summary>
        private PieceSO FindNonMatchingPiece(Vector2Int position)
        {
            // Mevcut cell'i geçici olarak kaydet
            CellController targetCell = Cells[position.y, position.x];

            // Sadece normal piece'leri içeren bir liste oluştur (performans için)
            List<PieceSO> normalPieces = new List<PieceSO>();
            foreach (var piece in availablePieces)
            {
                if (piece.pieceTypeEnum == PieceTypeEnum.Default)
                {
                    normalPieces.Add(piece);
                }
            }

            // Normal piece yoksa, tüm listeyi kullan
            if (normalPieces.Count == 0)
            {
                normalPieces = availablePieces;
            }

            for (int attempt = 0; attempt < maxGenerationAttempts; attempt++)
            {
                PieceSO candidatePiece;

                // Özel piece mi yoksa normal piece mı seçileceğini belirle
                bool selectSpecialPiece = Random.value < specialPieceSpawnChance && !limitSpecialPiecesOnInit;

                if (selectSpecialPiece)
                {
                    // Özel piece seçme şansı var
                    candidatePiece = availablePieces[Random.Range(0, availablePieces.Count)];
                }
                else
                {
                    // Normal piece seç
                    candidatePiece = normalPieces[Random.Range(0, normalPieces.Count)];
                }

                // Geçici olarak bu piece'i yerleştir
                var tempPieceData = ScriptableObject.CreateInstance<PieceSO>();
                tempPieceData.pieceTypeEnum = selectSpecialPiece ? candidatePiece.pieceTypeEnum : PieceTypeEnum.Default;
                tempPieceData.Color = candidatePiece.Color;
                tempPieceData.Sprite = candidatePiece.Sprite;
                tempPieceData.pieceColor = candidatePiece.pieceColor;

                var tempCellData = new CellData
                {
                    Piece = tempPieceData,
                };

                targetCell.SetData(tempCellData, true);

                // Bu piece ile eşleşme oluşur mu kontrol et
                bool wouldCreateMatch = CheckForMatches(position);

                // Eğer eşleşme oluşmazsa bu piece'i kabul et
                if (!wouldCreateMatch)
                {
                    // Geçici piece'i temizle (gerçek piece CreateNewPieceAt'da oluşturulacak)
                    targetCell.ClearPiece();
                    return candidatePiece;
                }

                // Geçici piece'i temizle ve başka bir piece dene
                targetCell.ClearPiece();
            }

            // Maksimum deneme sayısına ulaşıldı, normal piece seç
            Debug.Log($"Could not find non-matching piece for position {position} after {maxGenerationAttempts} attempts");

            // Default olarak normal piece döndür
            foreach (var piece in availablePieces)
            {
                if (piece.pieceTypeEnum == PieceTypeEnum.Default)
                {
                    return piece;
                }
            }

            return availablePieces[0]; // Hiç normal piece yoksa ilk piece'i döndür
        }
        
        

        private IEnumerator CheckForCascadingMatches(List<Vector2Int> createdPieces)
        {
            yield return new WaitForSeconds(Time.deltaTime * createdPieces.Count * 10);
            for (int i = 0; i < createdPieces.Count; i++)
            {
                if (CheckForMatches(createdPieces[i]))
                {
                    matchController.ProcessMatchesAtCell(Cells[createdPieces[i].y, createdPieces[i].x]);
                    break;
                }
            }
        }
    }
}