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
using ProjectAtlasManager.Events;
using ProjectAtlasManager.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectAtlasManager
{
  internal class DeleteTemplateButton : Button
  {
    protected override void OnClick()
    {
      RemoveTagsFromTemplateAsync();
    }

    private async Task RemoveTagsFromTemplateAsync()
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      var query = new PortalQueryParameters("id:" + Module1.SelectedProjectTemplate);
      query.Limit = 100;
      await QueuedTask.Run(async () =>
      {
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        var item = results.Results.FirstOrDefault();
        if (item == null)
        {
          return;
        }
        var tags = UpdateTags(item);
        var portalClient = new PortalClient(portal.PortalUri, portal.GetToken());
        await portalClient.UpdateTags(item, tags);
        var viewersBasedOnTemplateQuery = new PortalQueryParameters($"type:\"Web Map\" AND tags:\"ProjectAtlas\" AND tags:\"CopyOfTemplate\" AND tags:\"PAT{item.ID}\" AND orgid:0123456789ABCDEF")
        {
          Limit = 100
        };
        var mapsBasedOnTemplate = await ArcGISPortalExtensions.SearchForContentAsync(portal, viewersBasedOnTemplateQuery);
        foreach(var viewer in mapsBasedOnTemplate.Results)
        {
          var tags2 = UpdateTags(viewer);
          await portalClient.UpdateTags(viewer, tags2);
        }
      });
      Thread.Sleep(750);
      EventSender.Publish(new UpdateGalleryEvent());
      FrameworkApplication.State.Deactivate("ProjectAtlasManager_Module_ProjectTemplateSelectedState");
    }

    private string UpdateTags(PortalItem item)
    {
      var tags = new List<string>();
      foreach (var tag in item.ItemTags)
      {
        if (tag.Equals("ProjectAtlas") || tag.Equals("CopyOfTemplate") || tag.Equals("Template") || tag.StartsWith("PAT"))
        {
          continue;
        }
        tags.Add(tag);
      }
      return string.Join(",", tags);
    }
  }
}
