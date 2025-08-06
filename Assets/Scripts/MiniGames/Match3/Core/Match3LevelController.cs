using System;
using Core.Events.Level;
using Core.Managers;
using MiniGames.Base;
using MiniGames.Match3.Data;
using MiniGames.Match3.Events;
using UnityEngine;

namespace MiniGames.Match3.Core
{
    [RequireComponent(typeof(GridController))]
    public class Match3LevelController : LevelController
    {
        public Match3DataSO Match3Data => Data as Match3DataSO;
        public GridController gridController;
        
        private void Start()
        {
            ManagerContainer.EventManager.Publish(new Match3LevelLoaded()
            {
                LevelController = this
            });
        }

        protected override void OnLevelLoaded(LevelLoadedEvent e)
        {
        }

        protected override void OnLevelUnloaded(LevelUnloadedEvent e)
        {
            
        }

        protected override void AddListener()
        {
            ManagerContainer.EventManager.Subscribe<Match3SwapPerformedEvent>(OnSwapPerformed);
            ManagerContainer.EventManager.Subscribe<Match3ScoreEarnedEvent>(OnScoreEarned);
        }

        protected override void RemoveListener()
        {
            ManagerContainer.EventManager.Unsubscribe<Match3SwapPerformedEvent>(OnSwapPerformed);
            ManagerContainer.EventManager.Unsubscribe<Match3ScoreEarnedEvent>(OnScoreEarned);
        }

        private void OnScoreEarned(Match3ScoreEarnedEvent obj)
        {
        }

        public int GetCurrentScore()
        {
            return Match3Data.currentScore;
        }

        public string GetCurrentSwapAttempts()
        {
            return Match3Data.currentSwapAttempts + "/" + Match3Data.maxSwapAttempts;
        }

        private void OnSwapPerformed(Match3SwapPerformedEvent obj)
        {
            Match3Data.currentSwapAttempts++;

            if (Match3Data.maxSwapAttempts > Match3Data.currentSwapAttempts)
                return;

            if (Match3Data.currentScore < Match3Data.TargetScore)
            {
                // If max swap attempts reached and score is not enough, end the level
                ManagerContainer.Instance.GetManager<LevelManager>().LevelFailed();
            }
            else if (Match3Data.currentScore >= Match3Data.TargetScore)
            {
                // If score is enough, end the level
                ManagerContainer.Instance.GetManager<LevelManager>().EndLevel();
                ManagerContainer.Instance.GetManager<SaveLoadManager>().UserSaveData.TotalScore += Match3Data.currentScore;
                ManagerContainer.Instance.GetManager<SaveLoadManager>().SaveUserData();
            }
        }
    }
}