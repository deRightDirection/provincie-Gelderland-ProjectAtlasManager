using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ProjectAtlasManager.Dockpanes;

namespace ProjectAtlasManager.Buttons
{
  class ShowViewersPane : Button
  {
    protected override void OnClick()
    {
      var pane = FrameworkApplication.DockPaneManager.Find(ViewersDockpaneViewModel.DockPaneId);
      if (pane == null)
      {
        return;
      }
      pane.Activate();
      pane.IsVisible = true;
    }
  }
}
