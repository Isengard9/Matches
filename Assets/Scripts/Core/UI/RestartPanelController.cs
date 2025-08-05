using Core.Managers;

namespace Core.UI
{
    public class RestartPanelController : PanelController
    {
        public override void OnMainButtonClicked()
        {
            base.OnMainButtonClicked();
            ManagerContainer.Instance.GetManager<LevelManager>().RestartLevel();
        }
    }
}
