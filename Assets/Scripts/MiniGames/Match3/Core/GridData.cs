using System;
using UnityEngine;

namespace MiniGames.Match3.Core
{
    public enum GridSize
    {
        Small = 5,
        Medium = 7,
        Large = 9
    }
    [Serializable]
    
    [CreateAssetMenu(menuName = "Match3/GridData")]
    public class GridData : ScriptableObject
    {
        public GridSize GridSize = GridSize.Small; // Default to Small size
        public CellData[,] Cells;

        private void OnValidate()
        {
            Cells = new CellData[(int)GridSize, (int)GridSize];
        }
    }
}