using System;
using Core;
using MiniGames.Match3.Core;
using UnityEngine;

namespace MiniGames.Match3.Data
{
    [CreateAssetMenu(fileName = "Match3 Data", menuName = "NC/Matches/Match3 Level Data", order = 0)]
    public class Match3DataSO : Level
    {
        public GameObject LevelPrefab;
        private GameObject createdLevel;
        public GridData GridData;

        private void OnValidate()
        {
            if(GridData != null)
                return;
            
            GridData = new GridData();
        }

        public override void Load()
        {
            base.Load();
            if (LevelPrefab != null)
            {
                createdLevel = Instantiate(LevelPrefab);
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
                Destroy(createdLevel);
                createdLevel = null;
            }
            else
            {
                Debug.LogWarning("No level to unload in Match3DataSO.");
            }
        }
    }
}