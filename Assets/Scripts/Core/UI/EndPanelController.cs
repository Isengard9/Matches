using Core.Managers;

namespace Core.UI
{
    public class EndPanelController : PanelController
    {
        public override void OnMainButtonClicked()
        {
            base.OnMainButtonClicked();
            ManagerContainer.Instance.GetManager<LevelManager>().LoadNextLevel();
        }
    }
}
