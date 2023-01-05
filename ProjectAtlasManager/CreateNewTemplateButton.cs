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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager
{
  internal class CreateNewTemplateButton : Button
  {
    protected override void OnUpdate()
    {
//      FrameworkApplication.State.Contains
      // check for state
    }
    protected override void OnClick()
    {
      var t = Task.Run(async () =>
      {
        await SetTagsForNewTemplate();
      });
      t.Wait();
      FrameworkApplication.State.Deactivate("ProjectAtlasManager_Module_WebMapSelectedState");
      FrameworkApplication.State.Activate("ProjectAtlasManager_Module_UpdateWebMapGalleryState");
    }

    private async Task SetTagsForNewTemplate()
    {
        ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
        var query = new PortalQueryParameters("id:" + Module1.SelectedWebMapToUpgradeToTemplate);
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        var item = results.Results.FirstOrDefault();
        if (item == null)
        {
          return;
        }
        var tags = string.Join(",", item.ItemTags);
        tags += ",Template,ProjectAtlas";
        if(tags.StartsWith(","))
        {
          tags = tags.Substring(1);
        }
        var uri = $"{item.PortalUri}sharing/rest/content/users/{item.Owner}/{item.FolderID}/items/{item.ItemID}/update?f=json&token=" + portal.GetToken();
        // TODO thumbnail toevoegen indien die nog niet aanwezig is
        var httpClient = new EsriHttpClient();
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(tags), "tags");
        var response = await httpClient.PostAsync(uri, formContent);
    }
  }
}
