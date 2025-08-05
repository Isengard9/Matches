using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniGames.Match3.Data;

namespace MiniGames.Match3.Core
{
    /// <summary>
    /// Manages matching operations in the Match-3 game.
    /// </summary>
    public class MatchController : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("References")]
        [SerializeField] private GridController gridController;

        [Header("Match Settings")]
        [SerializeField] private float animationStartDelay = 0.1f;
        [SerializeField] private float animationDuration = 0.5f;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            if (gridController == null)
                gridController = GetComponent<GridController>();
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Finds and processes matches affected by swapped cells.
        /// </summary>
        public void ProcessSwapMatches(CellController cell1, CellController cell2)
        {
            HashSet<CellController> allMatchedCells = new HashSet<CellController>();
            
            AddMatchedCellsToSet(cell1.GridPosition, allMatchedCells);
            AddMatchedCellsToSet(cell2.GridPosition, allMatchedCells);
            
            if (allMatchedCells.Count > 0)
            {
                StartCoroutine(TriggerMatchAnimationsAndProcess(allMatchedCells));
            }
        }

        /// <summary>
        /// Processes matches starting from a specific cell.
        /// </summary>
        public void ProcessMatchesAtCell(CellController startCell)
        {
            HashSet<CellController> allMatchedCells = new HashSet<CellController>();
            
            AddMatchedCellsToSet(startCell.GridPosition, allMatchedCells);
            
            if (allMatchedCells.Count > 0)
            {
                StartCoroutine(TriggerMatchAnimationsAndProcess(allMatchedCells));
            }
        }
        
        /// <summary>
        /// Checks if a match of 3 or more exists at the specified position.
        /// </summary>
        public bool CheckForMatches(Vector2Int position)
        {
            if (!gridController.IsValidPosition(position)) return false;

            CellController cell = gridController.Cells[position.y, position.x];
            if (cell.IsEmpty() || cell.IsWall()) return false;

            PieceColorEnum targetColor = cell.CellData.Piece.pieceColor;
            HashSet<Vector2Int> connectedCells = new HashSet<Vector2Int>();
            FindConnectedCells(position, targetColor, connectedCells);

            return connectedCells.Count >= 3;
        }
        
        #endregion

        #region Private Match Logic
        
        /// <summary>
        /// Adds matched cells at a given position to the target set.
        /// </summary>
        private void AddMatchedCellsToSet(Vector2Int position, HashSet<CellController> targetSet)
        {
            if (!gridController.IsValidPosition(position)) return;

            CellController cell = gridController.Cells[position.y, position.x];
            if (cell.IsEmpty() || cell.IsWall()) return;

            PieceColorEnum targetColor = cell.CellData.Piece.pieceColor;
            HashSet<Vector2Int> connectedPositions = new HashSet<Vector2Int>();
            FindConnectedCells(position, targetColor, connectedPositions);

            if (connectedPositions.Count >= 3)
            {
                foreach (var pos in connectedPositions)
                {
                    targetSet.Add(gridController.Cells[pos.y, pos.x]);
                }
            }
        }

        /// <summary>
        /// Recursively finds all connected cells of the same color.
        /// </summary>
        private void FindConnectedCells(Vector2Int position, PieceColorEnum targetColor, HashSet<Vector2Int> visited)
        {
            if (!gridController.IsValidPosition(position) || visited.Contains(position))
                return;

            CellController cell = gridController.Cells[position.y, position.x];

            if (cell.IsEmpty() || cell.IsWall() || !ColorsMatch(cell.CellData.Piece.pieceColor, targetColor))
                return;

            visited.Add(position);

            // Check in all 4 directions
            FindConnectedCells(position + Vector2Int.right, targetColor, visited);
            FindConnectedCells(position + Vector2Int.left, targetColor, visited);
            FindConnectedCells(position + Vector2Int.up, targetColor, visited);
            FindConnectedCells(position + Vector2Int.down, targetColor, visited);
        }

        /// <summary>
        /// Checks if two piece colors are the same.
        /// </summary>
        private bool ColorsMatch(PieceColorEnum color1, PieceColorEnum color2)
        {
            return color1 == color2;
        }
        
        #endregion

        #region Animation & Processing
        
        /// <summary>
        /// Triggers match animations for the given cells with a delay between each.
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
        /// Triggers animations and then processes the clearing of matched cells.
        /// </summary>
        private IEnumerator TriggerMatchAnimationsAndProcess(HashSet<CellController> matchedCells)
        {
            HashSet<CellController> allCellsToDestroy = ProcessAllMatches(matchedCells);
            
            yield return StartCoroutine(TriggerMatchAnimations(allCellsToDestroy));
            yield return new WaitForSeconds(animationDuration);
            
            foreach (var cell in allCellsToDestroy)
            {
                if (cell != null && !cell.IsEmpty() && !cell.IsWall())
                {
                    cell.ClearPiece();
                }
            }
            
            yield return StartCoroutine(gridController.CreateNewPieces());
        }
        
        /// <summary>
        /// Gathers all cells to be destroyed, including initial matches and effects from special pieces.
        /// </summary>
        private HashSet<CellController> ProcessAllMatches(HashSet<CellController> initialMatches)
        {
            HashSet<CellController> allCellsToDestroy = new HashSet<CellController>(initialMatches);
            HashSet<CellController> specialEffects = CalculateSpecialPieceEffects(initialMatches);
            allCellsToDestroy.UnionWith(specialEffects);
            
            return allCellsToDestroy;
        }
        
        #endregion

        #region Special Piece Effects
        
        /// <summary>
        /// Calculates the area of effect for any special pieces in the matched set.
        /// </summary>
        private HashSet<CellController> CalculateSpecialPieceEffects(HashSet<CellController> matchedCells)
        {
            HashSet<CellController> specialEffects = new HashSet<CellController>();
            
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
        /// Adds cells in a 3x3 area around the bomb to the effects set.
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
                    }
                }
            }
        }
        
        /// <summary>
        /// Adds all cells in the same row to the effects set.
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
                }
            }
        }
        
        /// <summary>
        /// Adds all cells in the same column to the effects set.
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
                }
            }
        }
        
        #endregion
    }
}
