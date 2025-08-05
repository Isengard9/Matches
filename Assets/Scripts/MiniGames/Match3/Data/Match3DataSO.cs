using System;
using Core;
using MiniGames.Match3.Core;
using UnityEngine;

namespace MiniGames.Match3.Data
{
    [CreateAssetMenu(fileName = "Match3 Data", menuName = "NC/Matches/Match3 Level Data", order = 0)]
    public class Match3DataSO : Level
    {
        public Match3ScoreData scoreData;
        public int maxSwapAttempts = 10;
        public int currentSwapAttempts;
        
        public int currentScore;
        public int TargetScore = 20;
        
        public Match3LevelController LevelPrefab;
        private Match3LevelController createdLevel;
        
        public GridData GridData;

        private void OnValidate()
        {
            if(GridData != null)
                return;
            
            GridData = CreateInstance<GridData>();
        }

        public override void Load()
        {
            base.Load();
            
            currentScore = 0;
            currentSwapAttempts = 0;
            
            if (LevelPrefab != null)
            {
                createdLevel = Instantiate(LevelPrefab);
                createdLevel.gridController.CreateGrid();
            }
            else
            {
                Debug.LogError("LevelPrefab is not assigned in Match3DataSO.");
            }
        }
        public override void Unload()
        {
            base.Unload();
            if (createdLevel != null)
            {
                createdLevel.gridController.ClearExistingGrid();
                Destroy(createdLevel.gameObject);
                createdLevel = null;
                currentScore = currentSwapAttempts = 0;
            }
            else
            {
                Debug.LogWarning("No level to unload in Match3DataSO.");
            }
        }

        private void OnDestroy()
        {
            Unload();
        }
    }
}