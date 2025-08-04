using System;
using UnityEngine;

namespace MiniGames.Match3.Core
{
    [Serializable]
    public class CellData
    {
        public Vector2Int Position;
        public SpriteRenderer Sprite;
        public PieceSO Piece;
    }
}