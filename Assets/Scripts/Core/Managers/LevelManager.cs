using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants;
using Core.Events.Level;
using UnityEngine;

namespace Core.Managers
{
    public class LevelManager : MManager
    {
        [SerializeField] private LevelsContainerSO levelsContainer;
        private List<Level> levels => levelsContainer.Levels.ConvertAll(x => x.Level);
        private Level currentLevel;
        public Level CurrentLevel => currentLevel;
        private int currentLevelIndex = 0;

        public override void Initialize()
        {
            levelsContainer = Resources.Load<LevelsContainerSO>(ResourcePaths.LevelsContainer);
            if (levelsContainer == null)
            {
                Debug.LogError("LevelsContainerSO not found in Resources/Data/Level/LevelsContainer");
            }

            WaitForFirstStart();
        }

        private async void WaitForFirstStart()
        {
            await Task.Delay(1000);
            currentLevelIndex = ManagerContainer.Instance.GetManager<SaveLoadManager>().UserSaveData.CurrentLevelIndex;

            LoadLevel();
        }

        public void StartLevel()
        {
            if (currentLevel == null)
            {
                return;
            }

            ManagerContainer.EventManager.Publish(new LevelStartedEvent() { Level = currentLevel });
        }


        public void LoadLevel()
        {
            var loadLevel = levels[currentLevelIndex];
            this.currentLevel = loadLevel;
            this.currentLevel.Load();
            ManagerContainer.EventManager.Publish(new LevelLoadedEvent() { Level = this.currentLevel });
            ManagerContainer.Instance.GetManager<SaveLoadManager>().UserSaveData.CurrentLevelIndex = currentLevelIndex;
            ManagerContainer.Instance.GetManager<SaveLoadManager>().SaveUserData();
        }


        public void LoadNextLevel()
        {
            UnloadLevel();

            currentLevelIndex++;

            if (currentLevelIndex < levels.Count)
            {
                currentLevel = levels[currentLevelIndex];
            }
            else
            {
                currentLevelIndex = 0;
                Debug.Log("All levels completed.");
            }

            LoadLevel();
        }

        public void EndLevel()
        {
            if (currentLevel == null)
                return;

            ManagerContainer.EventManager.Publish(new LevelEndedEvent() { Level = currentLevel });
        }

        public void UnloadLevel()
        {
            if (currentLevel == null)
                return;

            currentLevel?.Unload();
            ManagerContainer.EventManager.Publish(new LevelUnloadedEvent() { Level = currentLevel });
        }

        public void LevelFailed()
        {
            ManagerContainer.EventManager.Publish(new LevelFailedEvent() { Level = currentLevel });
        }

        public void RestartLevel()
        {
            if (currentLevel == null)
                return;
            UnloadLevel();
            ManagerContainer.EventManager.Publish(new LevelRestartedEvent() { Level = currentLevel });
            LoadLevel();
        }
    }
}