using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json;
using ProjectAtlasManager.Events;
using ProjectAtlasManager.Services;
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
  internal class CreateNewTemplateButton : Button
  {
    protected override void OnUpdate()
    {
    }
    protected override void OnClick()
    {
        SetTagsForNewTemplateAsync();
    }

    private async Task SetTagsForNewTemplateAsync()
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      var query = new PortalQueryParameters("id:" + Module1.SelectedWebMapToUpgradeToTemplate);
      query.Limit = 100;
      await QueuedTask.Run(async () =>
      {
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        var item = results.Results.FirstOrDefault();
        if (item == null)
        {
          return;
        }
        var tags = TagsHelper.ParseTags(item);
        if(!tags.Contains("Template"))
        {
          tags += ",Template";
        }
        if(!tags.Contains("ProjectAtlas"))
        {
          tags += ",ProjectAtlas";
        }
        if (!tags.Contains($"PAT{item.ID}"))
        {
          tags += $",PAT{item.ID}";
        }
        if (tags.StartsWith(","))
        {
          tags = tags.Substring(1);
        }
        var portalClient = new PortalClient(item.PortalUri, portal.GetToken());
        await portalClient.UpdateTags(item, tags);
      });
      FrameworkApplication.State.Deactivate("ProjectAtlasManager_Module_WebMapSelectedState");
      Thread.Sleep(750);
      EventSender.Publish(new UpdateGalleryEvent());
    }
  }
}
