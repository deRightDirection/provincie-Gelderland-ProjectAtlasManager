using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ProjectAtlasManager.Domain;
using ProjectAtlasManager.Templates;
using ProjectAtlasManager.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager
{
  internal class SynchronizeTemplateButton : Button
  {
    private ProgressDialog progDialog;
    protected override async void OnClick()
    {
      progDialog = new ProgressDialog("Synchroniseren webmaps...");
      progDialog.Show();
      await SynchronizeTemplatesAsync();
    }

    private Task SynchronizeTemplatesAsync()
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      var query = new PortalQueryParameters("id:" + Module1.SelectedProjectTemplate);
      var portalClient = new PortalClient(portal.PortalUri, portal.GetToken());
      return QueuedTask.Run(async () =>
      {
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        var item = results.Results.FirstOrDefault();
        if (item == null)
        {
          return;
        }
        var layersFromTemplate = await portalClient.RetrieveLayers(item);
        var mapsBasedOnTemplateQuery = new PortalQueryParameters($"type:\"Web Map\" AND tags:\"ProjectAtlas\" AND tags:\"Template\" AND tags:\"PAT{item.ID}\" AND orgid:0123456789ABCDEF");
        var mapsBasedOnTemplate = await ArcGISPortalExtensions.SearchForContentAsync(portal, mapsBasedOnTemplateQuery);
        var webmapSynchronizer = new WebMapSynchronizer();
        Parallel.ForEach(mapsBasedOnTemplate.Results, async x =>
        {
          await SynchronizeWebMap(x, portalClient, layersFromTemplate, webmapSynchronizer);
        });
        progDialog.Hide();
      });
    }

    private async Task SynchronizeWebMap(PortalItem webmap, PortalClient portalClient, IEnumerable<OperationalLayer> layersFromTemplate, WebMapSynchronizer webmapSynchronizer)
    {
      var webmapData = await portalClient.GetDataFromItem(webmap);
      webmapData = webmapSynchronizer.Synchronize(webmapData, layersFromTemplate);
      await portalClient.UpdateData(webmap, webmapData);
    }
  }
}
