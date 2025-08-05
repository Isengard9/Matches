using System;
using Core.Events.Level;
using Core.Managers;
using MiniGames.Match3.Data;
using UnityEngine;
namespace MiniGames.Match3.Core
{
    [RequireComponent(typeof(GridController))]
    public class Match3LevelController : MonoBehaviour
    {
        public Match3DataSO Match3Data;
        public GridController gridController;


        private void OnEnable()
        {
            ManagerContainer.EventManager.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
            ManagerContainer.EventManager.Subscribe<LevelUnloadedEvent>(OnLevelUnloaded);
        }
        
        private void OnDisable()
        {
            ManagerContainer.EventManager.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);
            ManagerContainer.EventManager.Unsubscribe<LevelUnloadedEvent>(OnLevelUnloaded);
        }
        
        
        private void OnLevelLoaded(LevelLoadedEvent e)
        {
            
        }
        
        private void OnLevelUnloaded(LevelUnloadedEvent e)
        {
            
        }
    }
}