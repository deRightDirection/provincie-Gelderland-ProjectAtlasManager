using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProjectAtlasManager.ViewModels;
using ProjectAtlasManager.Web;
using ProjectAtlasManager.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager
{
  class DeleteViewerButton : Button
  {
    private DeleteViewersWindow _deleteViewersWindow = null;
    private List<string> _itemIds;

    private ProgressDialog progDialog;
    protected override async void OnClick()
    {
      if (_deleteViewersWindow != null)
      {
        return;
      }
      _deleteViewersWindow = new DeleteViewersWindow
      {
        Owner = FrameworkApplication.Current.MainWindow
      };
      _deleteViewersWindow.Closed += (o, e) =>
      {
        var dataContext = _deleteViewersWindow.DataContext as DeleteViewersWindowViewModel;
        _itemIds = dataContext.ItemIds;
        _deleteViewersWindow = null;
      };
      var result = _deleteViewersWindow.ShowDialog();
      if (_itemIds != null && _itemIds.Any())
      {
        progDialog = new ProgressDialog("Verwijder viewers...");
        progDialog.Show();
        await DeleteViewersAsync(_itemIds);
      }
    }

    private Task DeleteViewersAsync(IEnumerable<string> viewers)
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      var query = PortalQueryParameters.CreateForItemsWithId(Module1.SelectedProjectTemplate);
      var portalClient = new PortalClient(portal.PortalUri, portal.GetToken());
      return QueuedTask.Run(async () =>
      {
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        var item = results.Results.FirstOrDefault();
        if (item == null)
        {
          return;
        }
        Parallel.ForEach(viewers, async x =>
        {
          await DeleteViewer(x, portalClient);
        });
        progDialog.Hide();
      });
    }

    private async Task DeleteViewer(string viewerItemId, PortalClient portalClient)
    {
      //ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      //var query = PortalQueryParameters.CreateForItemsWithId(viewerItemId);
      //var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
      //var webmap = results.Results.FirstOrDefault();
      //var webmapData = await portalClient.GetDataFromItem(webmap);
      //webmapData = webmapSynchronizer.Synchronize(webmapData, layersFromTemplate);
      //var result = await portalClient.UpdateData(webmap, webmapData);
    }
  }
}
