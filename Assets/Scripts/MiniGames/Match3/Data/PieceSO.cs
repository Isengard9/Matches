using UnityEngine;
using UnityEngine.Serialization;

namespace MiniGames.Match3.Data
{
    public enum PieceTypeEnum
    {
        Default,
        Bomb,
        Row,
        Column,
        Wall,
    }
    
    public enum PieceColorEnum
    {
        White,
        Red,
        Green,
        Blue,
        Yellow,
        Purple,
    }
    
    [CreateAssetMenu(fileName = "Match3Piece", menuName = "NC/Matches/Games/Match3/Piece Data", order = 0)]
    public class PieceSO : ScriptableObject
    {
        public Sprite Sprite;
        public Color Color = Color.white;
        public PieceTypeEnum pieceTypeEnum = PieceTypeEnum.Default;
        public PieceColorEnum pieceColor = PieceColorEnum.White;
        
    }
}