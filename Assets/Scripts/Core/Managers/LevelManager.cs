using System.Collections.Generic;
using System.Linq;
using Constants;
using Core.Events.Level;
using UnityEngine;

namespace Core.Managers
{
    public class LevelManager : MManager
    {
        [SerializeField] private LevelsContainerSO levelsContainer;
        private List<Level> levels => levelsContainer.Levels.ConvertAll(x => x.Level);
        private Level CurrentLevel;
        private int currentLevelIndex = 0;

        public override void Initialize()
        {
            levelsContainer = Resources.Load<LevelsContainerSO>(ResourcePaths.LevelsContainer);
            if (levelsContainer == null)
            {
                Debug.LogError("LevelsContainerSO not found in Resources/Data/Level/LevelsContainer");
            }
        }

        public void StartLevel()
        {
            var currentLevel = levels[currentLevelIndex];
            if (currentLevel == null)
            {
                return;
            }

            CurrentLevel = currentLevel;

            ManagerContainer.EventManager.Publish(new LevelStartedEvent() { Level = currentLevel });
        }

        public void LoadLevel()
        {
            if (CurrentLevel == null)
                return;

            CurrentLevel.Load();

            ManagerContainer.EventManager.Publish(new LevelLoadedEvent() { Level = CurrentLevel });
        }

        public void EndLevel()
        {
            if (CurrentLevel == null)
                return;

            ManagerContainer.EventManager.Publish(new LevelEndedEvent() { Level = CurrentLevel });
            currentLevelIndex++;

            if (currentLevelIndex < levels.Count)
            {
                CurrentLevel = levels[currentLevelIndex];
            }
            else
            {
                Debug.Log("All levels completed.");
            }
        }

        public void UnloadLevel()
        {
            if (CurrentLevel == null)
                return;

            var oldLevel = levels[currentLevelIndex - 1];
            if (oldLevel == null) return;

            oldLevel?.Unload();
            ManagerContainer.EventManager.Publish(new LevelUnloadedEvent() { Level = oldLevel });
        }
    }
}