using System;
using UnityEngine;

namespace MiniGames.Match3.Data
{
    public enum GridSize
    {
        Small = 5,
        Medium = 7,
        Large = 9
    }

    [Serializable]
    public class SerializableCellData
    {
        public CellData[] rows;

        public SerializableCellData(int size)
        {
            rows = new CellData[size * size];
        }

        public CellData this[int row, int col]
        {
            get
            {
                int index = row * (int)Mathf.Sqrt(rows.Length) + col;
                return rows[index];
            }
            set
            {
                int index = row * (int)Mathf.Sqrt(rows.Length) + col;
                rows[index] = value;
            }
        }

        public int GetLength(int dimension)
        {
            return (int)Mathf.Sqrt(rows.Length);
        }
    }

    [Serializable]
    [CreateAssetMenu(menuName = "NC/Match3/GridData")]
    public class GridData : ScriptableObject
    {
        [SerializeField] public GridSize GridSize = GridSize.Small;
        [SerializeField] private SerializableCellData serializableCells;

        public CellData[,] Cells
        {
            get
            {
                if (serializableCells == null) return null;

                int size = serializableCells.GetLength(0);
                CellData[,] result = new CellData[size, size];

                for (int row = 0; row < size; row++)
                {
                    for (int col = 0; col < size; col++)
                    {
                        result[row, col] = serializableCells[row, col];
                    }
                }
                return result;
            }
            set
            {
                if (value == null)
                {
                    serializableCells = null;
                    return;
                }

                int size = value.GetLength(0);
                serializableCells = new SerializableCellData(size);

                for (int row = 0; row < size; row++)
                {
                    for (int col = 0; col < size; col++)
                    {
                        serializableCells[row, col] = value[row, col];
                    }
                }
            }
        }
    }
}