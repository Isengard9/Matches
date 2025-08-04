using MiniGames.Match3.Data;
using UnityEngine;
namespace MiniGames.Match3.Core
{
    [RequireComponent(typeof(GridController))]
    public class Match3LevelController : MonoBehaviour
    {
        public Match3DataSO Match3Data;
        public GridController gridController;

    }
}