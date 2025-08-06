using Core;
using MiniGames.RunnerCube.Core;
using UnityEngine;

namespace MiniGames.RunnerCube.Data
{
    [CreateAssetMenu(fileName = "Runner Level Data", menuName = "NC/Matches/Runner Level Data", order = 0)]
    public class RunnerDataSO : Level
    {
        public RoadData RoadData;
        public GameObject LevelPrefab;
        private RunnerLevelController createdLevel;
        
        public override void Load()
        {
            base.Load();
            
            if (LevelPrefab != null)
            {
                createdLevel = Instantiate(LevelPrefab).GetComponent<RunnerLevelController>();
                createdLevel.SetData(this);
            }
            else
            {
                Debug.LogError("LevelPrefab is not assigned in RunnerDataSO.");
            }
        }
        
        public override void Unload()
        {
            base.Unload();
            
            if (createdLevel != null)
            {
                Destroy(createdLevel.gameObject);
                createdLevel = null;
            }
            else
            {
                Debug.LogError("Created level is null in RunnerDataSO.");
            }
        }
    }
}