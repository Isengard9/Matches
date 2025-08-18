using Core.Events;
using Core.Managers;
using UnityEngine;

namespace Test
{
    public class EventsTest : MonoBehaviour
    {
        private EventManager _eventManager;

        private void Start()
        {
            _eventManager = ManagerContainer.Instance.GetManager<EventManager>();
            SetupEventTests();
        }

        private void OnDestroy()
        {
            TeardownEventTests();
        }

        #region Event Tests

        private void SetupEventTests()
        {
            _eventManager.Subscribe<TestEvent>(HandleTestEvent);
            _eventManager.Subscribe<TestEventClass>(HandleTestEventClass);

            var testEvent = new TestEvent { testlog = "Bu bir struct test mesajıdır." };
            _eventManager.Publish(testEvent);

            var testEventClass = new TestEventClass { testlog = "Bu bir class test mesajıdır." };
            _eventManager.Publish(testEventClass);
        }

        private void TeardownEventTests()
        {
            if (_eventManager == null) return;
            _eventManager.Unsubscribe<TestEvent>(HandleTestEvent);
            _eventManager.Unsubscribe<TestEventClass>(HandleTestEventClass);
        }

        private void HandleTestEvent(TestEvent testEvent)
        {
            Debug.Log($"Struct Event alındı: {testEvent.testlog}");
        }

        private void HandleTestEventClass(TestEventClass testEvent)
        {
            Debug.Log($"Class Event alındı: {testEvent.testlog}");
        }

        #endregion

        #region Level Tests

        [ContextMenu("Start Level Test")]
        public void StartLevelTest()
        {
            var levelManager = ManagerContainer.Instance.GetManager<LevelManager>();
            if (levelManager == null)
            {
                Debug.LogError("LevelManager not found.");
                return;
            }

            levelManager.StartLevel();
            levelManager.LoadLevel();
        }

        [ContextMenu("End Level Test")]
        public void EndLevelTest()
        {
            var levelManager = ManagerContainer.Instance.GetManager<LevelManager>();
            if (levelManager == null)
            {
                Debug.LogError("LevelManager not found.");
                return;
            }
            levelManager.EndLevel();
            Debug.Log("Level ended.");
        }

        #endregion
    }
}
