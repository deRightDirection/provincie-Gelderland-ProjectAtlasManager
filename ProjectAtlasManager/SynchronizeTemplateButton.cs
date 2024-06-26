using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProjectAtlasManager.Services;
using ProjectAtlasManager.ViewModels;
using ProjectAtlasManager.Web;
using ProjectAtlasManager.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ProjectAtlasManager
{
  internal class SynchronizeTemplateButton : Button
  {
    private SyncViewersFromTemplateWindow _syncviewersfromtemplatewindow = null;
    private List<string> _itemIds;

    private ProgressDialog progDialog;

    protected override async void OnClick()
    {
      if (_syncviewersfromtemplatewindow != null)
      {
        return;
      }
      _syncviewersfromtemplatewindow = new SyncViewersFromTemplateWindow
      {
        Owner = FrameworkApplication.Current.MainWindow
      };
      _syncviewersfromtemplatewindow.Closed += (o, e) =>
      {
        var dataContext = _syncviewersfromtemplatewindow.DataContext as SyncViewersFromTemplateViewModel;
        _itemIds = dataContext.ItemIds;
        _syncviewersfromtemplatewindow = null;
      };
      var result = _syncviewersfromtemplatewindow.ShowDialog();
      if (_itemIds != null && _itemIds.Any())
      {
        progDialog = new ProgressDialog("Synchroniseren webmaps...");
        progDialog.Show();
        await SynchronizeTemplatesAsync(_itemIds);
        progDialog.Hide();
      }
    }

    private Task SynchronizeTemplatesAsync(IEnumerable<string> viewers)
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      var query = PortalQueryParameters.CreateForItemsWithId(Module1.SelectedProjectTemplate);
      return QueuedTask.Run(async () =>
      {
        var portalClient = new PortalClient(portal.PortalUri, portal.GetToken());
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        var projectTemplate = results.Results.FirstOrDefault();
        if (projectTemplate == null)
        {
          return;
        }
        await portalClient.UpdateTemplate(projectTemplate, true);
        var templateWebmapData = await portalClient.GetDataFromItem(projectTemplate);
        var webmapSynchronizer = new WebMapManager(templateWebmapData);
        Parallel.ForEach(viewers, async x =>
        {
          await SynchronizeWebMap(x, portalClient, templateWebmapData, webmapSynchronizer);
        });
        progDialog.Hide();
      });
    }

    private async Task SynchronizeWebMap(string viewerItemId, PortalClient portalClient, string templateJson, WebMapManager webmapSynchronizer)
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      var query = PortalQueryParameters.CreateForItemsWithId(viewerItemId);
      var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
      var webmap = results.Results.FirstOrDefault();
      var webmapData = await portalClient.GetDataFromItem(webmap);
      webmapData = webmapSynchronizer.Synchronize(webmapData, templateJson);
      var result = await portalClient.UpdateData(webmap, webmapData);
      await result.Content.ReadAsStringAsync();
    }
  }
}
