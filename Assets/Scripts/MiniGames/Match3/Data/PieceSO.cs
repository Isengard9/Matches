using UnityEngine;

namespace MiniGames.Match3.Data
{
    public enum PieceType
    {
        Default,
        Bomb,
        Row,
        Column,
        Wall,
    }
    [CreateAssetMenu(fileName = "Match3Piece", menuName = "NC/Match3/Objects/Piece", order = 0)]
    public class PieceSO : ScriptableObject
    {
        public Sprite Sprite;
        public Color Color = Color.white;
        public PieceType Type = PieceType.Default;
        
    }
}