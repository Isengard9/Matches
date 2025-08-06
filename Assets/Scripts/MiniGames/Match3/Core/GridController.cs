using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Core.Managers;
using MiniGames.Match3.Data;
using MiniGames.Match3.Events;

namespace MiniGames.Match3.Core
{
    [RequireComponent(typeof(Match3LevelController))]
    public class GridController : MonoBehaviour
    {
        #region Fields

        public Match3LevelController levelController;
        public CellController CellPrefab;

        public CellController[,] Cells;

        [SerializeField] private float padding = 1.2f;

        [Header("Piece Generation")] [SerializeField]
        private List<PieceSO> availablePieces = new List<PieceSO>();

        public List<PieceSO> AvailablePieces => availablePieces;

        [SerializeField] private float pieceGenerationDelay = 0.1f; // Delay between each piece generation
        [SerializeField] private int maxGenerationAttempts = 10; // Maximum attempts to find a piece without matches
        [SerializeField] private bool preventAutoMatches = true; // Prevent automatic matches on generation

        [Header("Match Controller")] [SerializeField]
        private MatchController matchController;

        [Header("Special Piece Chance")] [SerializeField]
        private float specialPieceSpawnChance = 0.1f; // Chance of generating special pieces (0-1)

        [SerializeField] private bool limitSpecialPiecesOnInit = true; // Limit special pieces at game start

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (matchController == null)
                matchController = GetComponent<MatchController>();
        }

        #endregion

        #region Grid Management

        [ContextMenu("Generate Grid")]
        public void CreateGrid()
        {
            if (levelController == null || levelController.Match3Data == null ||
                levelController.Match3Data.girdDataSo == null)
            {
                Debug.LogError("Match3Data or GirdDataSO is not set.");
                return;
            }

            var gridData = levelController.Match3Data.girdDataSo;
            int gridSize = (int)gridData.GridSize;

            // Initialize the cells array
            Cells = new CellController[gridSize, gridSize];

            // Clear existing grid (Editor safe)
            ClearExistingGrid();

            // Initialize cells from GirdDataSO
            if (gridData.Cells == null)
            {
                Debug.LogError("GirdDataSO.Cells is not initialized.");
                return;
            }

            // Create new grid
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    // World position: col = x, row = y (but invert row since Y is positive upwards in Unity)
                    Vector3 position = new Vector3(col * padding, -row * padding, 0);
                    var cellObj = Instantiate(CellPrefab.gameObject, position, Quaternion.identity, transform);
                    cellObj.name = $"Cell_{row}_{col}";

                    CellController cellController = cellObj.GetComponent<CellController>();
                    // Grid position: x = col, y = row (0,0 = top left, 4,4 = bottom right)
                    cellController.GridPosition = new Vector2Int(col, row);

                    // Get CellData from GirdDataSO and set it
                    var cellData = gridData.Cells[row, col];
                    cellController.SetData(cellData);

                    // Save to Cells array: [row, col] = [y, x]
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

        #endregion

        #region Swapping

        /// <summary>
        /// Attempts to swap pieces. Checks all rules.
        /// </summary>
        public bool TrySwap(CellController fromCell, Vector2Int direction)
        {
            if (fromCell == null) return false;
            ManagerContainer.EventManager.Publish(new Match3SwapPerformedEvent());
            // Calculate target position
            Vector2Int fromPos = fromCell.GridPosition;
            Vector2Int toPos = fromPos + direction;

            // Check if within grid bounds
            if (!IsValidPosition(toPos))
            {
                Debug.Log($"Swap failed: Target position {toPos} is out of bounds");
                return false;
            }

            CellController toCell = Cells[toPos.y, toPos.x];

            // Check swap rules
            if (!CanSwapCells(fromCell, toCell))
            {
                Debug.Log($"Swap failed: Cannot swap {fromPos} with {toPos}");
                return false;
            }

            // Temporarily swap and check for matches
            SwapCellData(fromCell, toCell);

            bool hasMatches = CheckForMatches(fromPos) || CheckForMatches(toPos);

            if (hasMatches)
            {
                Debug.Log($"Swap successful: {fromPos} <-> {toPos}");

                // Perform animated swap
                PerformAnimatedSwap(fromCell, toCell);

                // Find and trigger match animations for affected cells by the swap
                TriggerMatchAnimationsForSwappedCells(fromCell, toCell);

                return true;
            }
            else
            {
                // No match found, revert the swap
                SwapCellData(fromCell, toCell);

                // Trigger WrongMatch animation on the cells that were attempted to be swapped
                fromCell.TriggerWrongMatchAnimation();
                toCell.TriggerWrongMatchAnimation();


                Debug.Log($"Swap failed: No matches found after swapping {fromPos} with {toPos}");
                return false;
            }
        }

        /// <summary>
        /// Finds matches affected by swapped cells and initiates match processing
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
        /// Checks if there's a 3-match at the specified position (delegated from MatchController)
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
        /// Performs an animated swap between two cells
        /// </summary>
        private void PerformAnimatedSwap(CellController cell1, CellController cell2)
        {
            Vector3 cell1Position = cell1.transform.position;
            Vector3 cell2Position = cell2.transform.position;

            // Animate both cells to their opposite positions
            cell1.MoveTo(cell2Position);
            cell2.MoveTo(cell1Position);
        }

        /// <summary>
        /// Checks if two cells can be swapped
        /// </summary>
        private bool CanSwapCells(CellController fromCell, CellController toCell)
        {
            // Null checks
            if (fromCell == null || toCell == null) return false;

            // Wall check - Nothing can be swapped with a wall
            if (fromCell.IsWall() || toCell.IsWall())
            {
                return false;
            }

            // At least one cell must have a piece
            if (fromCell.IsEmpty() && toCell.IsEmpty())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Swaps the data between two cells
        /// </summary>
        private void SwapCellData(CellController cell1, CellController cell2)
        {
            var cell1Position = cell1.GridPosition;
            var cell2Position = cell2.GridPosition;

            // Update cells in the grid
            Cells[cell1Position.y, cell1Position.x] = cell2;
            Cells[cell2Position.y, cell2Position.x] = cell1;

            // Update grid positions of the cells
            cell1.GridPosition = cell2Position;
            cell2.GridPosition = cell1Position;
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Checks if two colors match
        /// </summary>
        private bool ColorsMatch(PieceColorEnum color1, PieceColorEnum color2)
        {
            return color1 == color2;
        }

        /// <summary>
        /// Checks if the position is within grid bounds
        /// </summary>
        public bool IsValidPosition(Vector2Int position)
        {
            if (Cells == null) return false;

            int gridSize = Cells.GetLength(0);
            return position.x >= 0 && position.x < gridSize &&
                   position.y >= 0 && position.y < gridSize;
        }

        #endregion

        #region Piece Generation

        /// <summary>
        /// Fills empty spaces with pieces selected from the availablePieces list
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
            // Check each column from bottom to top
            for (int col = 0; col < gridSize; col++)
            {
                for (int row = gridSize - 1; row >= 0; row--)
                {
                    Vector2Int pos = new Vector2Int(col, row);
                    CellController cell = Cells[pos.y, pos.x];

                    // Create new piece if cell is empty and not a wall
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
                // Short wait for new pieces to settle
                yield return new WaitForSeconds(0.2f);

                // Check for cascading matches
                yield return StartCoroutine(CheckForCascadingMatches(createdPieces));
            }
        }

        /// <summary>
        /// Creates a random piece at the specified position from the availablePieces list
        /// Prevents automatic matches by checking match rules
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
                // Try to find a piece that won't create a match
                selectedPiece = FindNonMatchingPiece(position);
            }

            // If no non-matching piece was found or preventAutoMatches is false, select randomly
            if (selectedPiece == null)
            {
                selectedPiece = availablePieces[Random.Range(0, availablePieces.Count)];
            }

            // Create a copy of the piece (to preserve the ScriptableObject reference)
            var newPieceData = ScriptableObject.CreateInstance<PieceSO>();
            newPieceData.pieceTypeEnum = selectedPiece.pieceTypeEnum;
            newPieceData.Color = selectedPiece.Color;
            newPieceData.Sprite = selectedPiece.Sprite;
            newPieceData.pieceColor = selectedPiece.pieceColor;

            var cellData = new CellData
            {
                Piece = newPieceData,
            };
            // Set the new piece to the cell
            cell.SetData(cellData);
        }

        /// <summary>
        /// Attempts to find a piece that won't create a match at the specified position
        /// </summary>
        private PieceSO FindNonMatchingPiece(Vector2Int position)
        {
            // Save the target cell temporarily
            CellController targetCell = Cells[position.y, position.x];

            // Create a list containing only normal pieces (for performance)
            List<PieceSO> normalPieces = new List<PieceSO>();
            foreach (var piece in availablePieces)
            {
                if (piece.pieceTypeEnum == PieceTypeEnum.Default)
                {
                    normalPieces.Add(piece);
                }
            }

            // If no normal pieces, use the entire list
            if (normalPieces.Count == 0)
            {
                normalPieces = availablePieces;
            }

            for (int attempt = 0; attempt < maxGenerationAttempts; attempt++)
            {
                PieceSO candidatePiece;

                // Determine whether to select a special piece or normal piece
                bool selectSpecialPiece = Random.value < specialPieceSpawnChance && !limitSpecialPiecesOnInit;

                if (selectSpecialPiece)
                {
                    // Chance to select a special piece
                    candidatePiece = availablePieces[Random.Range(0, availablePieces.Count)];
                }
                else
                {
                    // Select a normal piece
                    candidatePiece = normalPieces[Random.Range(0, normalPieces.Count)];
                }

                // Temporarily place this piece
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

                // Check if this piece would create a match
                bool wouldCreateMatch = CheckForMatches(position);

                // If it doesn't create a match, accept this piece
                if (!wouldCreateMatch)
                {
                    // Clear temporary piece (real piece will be created in CreateNewPieceAt)
                    targetCell.ClearPiece();
                    return candidatePiece;
                }

                // Clear temporary piece and try another piece
                targetCell.ClearPiece();
            }

            // Maximum attempts reached, select a normal piece

            Debug.Log(
                $"Could not find non-matching piece for position {position} after {maxGenerationAttempts} attempts");

            // Return a default normal piece
            foreach (var piece in availablePieces)
            {
                if (piece.pieceTypeEnum == PieceTypeEnum.Default)
                {
                    return piece;
                }
            }

            return availablePieces[0]; // If no normal pieces, return the first piece
        }

        #endregion

        #region Match Processing

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

        #endregion
    }
}