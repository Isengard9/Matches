using Core.Events.Level;
using MiniGames.Base;
using MiniGames.RunnerCube.Data;
using UnityEngine;

namespace MiniGames.RunnerCube.Core
{
    public class RunnerLevelController : LevelController
    {
        public RoadController RoadController;
        public RunnerDataSO LevelData;
        

[ContextMenu("Save Data")]
        public void SaveData()
        {
#if UNITY_EDITOR
            var data = RoadController.GetData();
            LevelData.RoadData = data;
            UnityEditor.EditorUtility.SetDirty(LevelData);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public void SetData(RunnerDataSO levelData)
        {
            LevelData = levelData;
            if (RoadController != null)
            {
                RoadController.SetData(LevelData.RoadData);
            }
            else
            {
                UnityEngine.Debug.LogError("RoadController is not assigned in RunnerLevelController.");
            }
        }


        protected override void OnLevelLoaded(LevelLoadedEvent e)
        {
            
        }

        protected override void OnLevelUnloaded(LevelUnloadedEvent e)
        {
        }

        protected override void AddListener()
        {
        }

        protected override void RemoveListener()
        {
        }
    }
}