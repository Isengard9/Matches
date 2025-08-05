using System;
using UnityEngine;
using Core.Events.Level;
using Core.UI;
using MiniGames.Match3.Core;
using MiniGames.Match3.Data;

namespace Core.Managers
{
    [DefaultExecutionOrder(-1)]
    [Serializable]
    public class UIManager : MonoBehaviour, IManager
    {
        [SerializeField] private StartPanelController startPanelController;
        [SerializeField] private EndPanelController endPanelController;
        [SerializeField] private RestartPanelController restartPanelController;
        
        
        [SerializeField] private GamePanelController _match3InGamePanelController;
        [SerializeField] private GamePanelController _runnerInGamePanelController;
        private GamePanelController _currentGamePanelController;
        private UIState currentState = UIState.None;
        
        public void Initialize()
        {
            _match3InGamePanelController?.Hide();
            _runnerInGamePanelController?.Hide();
            SubscribeToEvents();
            SetUIState(UIState.StartPanel);
        }

        private void SubscribeToEvents()
        {
            ManagerContainer.EventManager.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
            ManagerContainer.EventManager.Subscribe<LevelEndedEvent>(OnLevelEnded);
            ManagerContainer.EventManager.Subscribe<LevelFailedEvent>(OnLevelFailed);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            ManagerContainer.EventManager.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);
            ManagerContainer.EventManager.Unsubscribe<LevelEndedEvent>(OnLevelEnded);
            ManagerContainer.EventManager.Unsubscribe<LevelFailedEvent>(OnLevelFailed);
        }

        private void OnLevelLoaded(LevelLoadedEvent levelEvent)
        {
            SetUIState(UIState.StartPanel);
            if (levelEvent.Level as Match3DataSO)
            {
                _currentGamePanelController = _match3InGamePanelController;
            }

            _match3InGamePanelController.Show();
        }

        private void OnLevelEnded(LevelEndedEvent levelEvent)
        {
            SetUIState(UIState.EndPanel);
            if (_currentGamePanelController != null)
            {
                _currentGamePanelController.Hide();
            }
        }

        private void OnLevelFailed(LevelFailedEvent levelEvent)
        {
            SetUIState(UIState.RestartPanel);
            if (_currentGamePanelController != null)
            {
                _currentGamePanelController.Hide();
            }
        }

        private void SetUIState(UIState newState)
        {
            if (currentState == newState) return;
            
            HideCurrentPanel();

            currentState = newState;

            ShowCurrentPanel();
        }

        private void HideCurrentPanel()
        {
            switch (currentState)
            {
                case UIState.StartPanel:
                    startPanelController?.HidePanel();
                    break;
                case UIState.EndPanel:
                    endPanelController?.HidePanel();
                    break;
                case UIState.RestartPanel:
                    restartPanelController?.HidePanel();
                    break;
            }
        }

        private void ShowCurrentPanel()
        {
            switch (currentState)
            {
                case UIState.StartPanel:
                    startPanelController?.ShowPanel();
                    break;
                case UIState.EndPanel:
                    endPanelController?.ShowPanel();
                    break;
                case UIState.RestartPanel:
                    restartPanelController?.ShowPanel();
                    break;
            }
        }
    }
}