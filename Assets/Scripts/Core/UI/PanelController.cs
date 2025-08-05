using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public abstract class PanelController : MonoBehaviour, IMainButton
    {
        [SerializeField] protected Button mainButton;
        [SerializeField] protected GameObject panel;

        protected virtual void Awake()
        {
            if (mainButton != null)
            {
                mainButton.onClick.AddListener(OnMainButtonClicked);
            }
        }

        protected virtual void OnDestroy()
        {
            if (mainButton != null)
            {
                mainButton.onClick.RemoveListener(OnMainButtonClicked);
            }
        }

        public virtual void ShowPanel()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        public virtual void HidePanel()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public virtual void OnMainButtonClicked()
        {
            HidePanel();
        }
    }
}
