using UnityEngine;

namespace Core.UI
{
    public abstract class GamePanelController : MonoBehaviour
    {
        public abstract void Show();
        

        public abstract void Hide();
        
        protected abstract void Reset();
    }
}