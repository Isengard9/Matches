using Core.Managers;
using MiniGames.Match3.Core;
using MiniGames.Match3.Events;
using TMPro;

namespace Core.UI
{
    public class Match3InGamePanelController : GamePanelController
    {
        public TMP_Text ScoreText;
        
        public TMP_Text SwapAttemptsText;

        public Match3LevelController CurrentLevelController;
        
        

        private void OnEnable()
        {
            ManagerContainer.EventManager.Subscribe<Match3SwapPerformedEvent>(OnSwapPerformed);
            ManagerContainer.EventManager.Subscribe<Match3ScoreEarnedEvent>(OnScoreEarned);
            ManagerContainer.EventManager.Subscribe<Match3LevelLoaded>(OnLevelLoaded);
        }
        
        private void OnDisable()
        {
            ManagerContainer.EventManager.Unsubscribe<Match3SwapPerformedEvent>(OnSwapPerformed);
            ManagerContainer.EventManager.Unsubscribe<Match3ScoreEarnedEvent>(OnScoreEarned);
            ManagerContainer.EventManager.Unsubscribe<Match3LevelLoaded>(OnLevelLoaded);
        }
        
        void OnLevelLoaded(Match3LevelLoaded e)
        {
            CurrentLevelController = e.LevelController;
            SwapAttemptsText.text = CurrentLevelController.GetCurrentSwapAttempts();
            ScoreText.text = "Score: " + CurrentLevelController.GetCurrentScore();
        }

        void OnSwapPerformed(Match3SwapPerformedEvent e)
        {
            SwapAttemptsText.text= CurrentLevelController.GetCurrentSwapAttempts();
        }
        
        void OnScoreEarned(Match3ScoreEarnedEvent e)
        {
            ScoreText.text = "Score: " + CurrentLevelController.GetCurrentScore();
        }

        public override void Show()
        {
            gameObject.SetActive(true);
            Reset();
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
            Reset();
        }

        protected override void Reset()
        {
            ScoreText.text = "Score: 0";
            SwapAttemptsText.text = "0/0";
        }
    }
}