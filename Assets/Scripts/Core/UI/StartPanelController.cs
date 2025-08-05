using Core.Managers;

namespace Core.UI
{
    public class StartPanelController : PanelController
    {
        public override void OnMainButtonClicked()
        {
            base.OnMainButtonClicked();
            ManagerContainer.Instance.GetManager<LevelManager>().StartLevel();
        }
    }
}
