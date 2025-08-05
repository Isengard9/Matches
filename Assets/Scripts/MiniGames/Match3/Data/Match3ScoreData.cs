using UnityEngine;

[CreateAssetMenu(fileName = "Match3 Score Data", menuName = "NC/Matches/Match3 Score Data", order = 1)]
public class Match3ScoreData : ScriptableObject
{
    public int defaultPieceScore = 1;
    public int bombPieceScore = 3;
    public int rowColumnPieceScore = 2;
}

