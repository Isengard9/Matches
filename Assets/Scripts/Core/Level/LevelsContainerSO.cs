using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "LevelsContainer", menuName = "NC/Matches/Level Container", order = 0)]
    public class LevelsContainerSO : ScriptableObject
    {
        [SerializeField] public List<LevelDataSO> Levels = new List<LevelDataSO>();
    }
}