using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "NC/Matches/Level Data", order = 0)]
    public class LevelDataSO : ScriptableObject
    {
        public string LevelName;
        public Level Level;
    }
}