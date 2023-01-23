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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
      await QueuedTask.Run(async () =>
      {
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        var item = results.Results.FirstOrDefault();
        if (item == null)
        {
          return;
        }
        var tags = string.Join(",", item.ItemTags);
        tags = tags.Replace("Template", string.Empty);
        tags = tags.Replace("ProjectAtlas", string.Empty);
        tags = tags.Replace($"PAT{item.ID}", string.Empty);
        tags = tags.Replace(",,", ",");
        if (tags.StartsWith(","))
        {
          tags = tags.Substring(1);
        }
        var uri = $"{item.PortalUri}sharing/rest/content/users/{item.Owner}/{item.FolderID}/items/{item.ItemID}/update?f=json&token=" + portal.GetToken();
        var httpClient = new EsriHttpClient();
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(tags), "tags");
        formContent.Add(new StringContent("" + true), "clearEmptyFields");
        var response = await httpClient.PostAsync(uri, formContent);
      });
      FrameworkApplication.State.Deactivate("ProjectAtlasManager_Module_ProjectTemplateSelectedState");
      FrameworkApplication.State.Activate("ProjectAtlasManager_Module_UpdateWebMapGalleryState");
    }
  }
}
