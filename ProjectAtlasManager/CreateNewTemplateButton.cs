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
    protected override void OnClick()
    {
      var control = FrameworkApplication.GetPlugInWrapper("newTemplateGallery");
      SetTagsForNewTemplate();
    }

    private async Task SetTagsForNewTemplate()
    {
      await QueuedTask.Run(async () =>
      {
        ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
        var query = new PortalQueryParameters("id:" + Module1.SelectedWebMapToUpgradeToTemplate);
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        var item = results.Results.FirstOrDefault();
        if(item == null)
        {
          return;
        }
        item.Tags.Insert(0, "Template");
        item.Tags.Insert(0, "ProjectAtlas");
        var json = JsonConvert.SerializeObject(item);
        var uri = $"{item.PortalUri}sharing/rest/content/users/{item.Owner}/{item.FolderID}/items/{item.ItemID}/update?token=" + portal.GetToken();
        var httpClient = new EsriHttpClient();

      // https://www.therightdirectionserver.nl/portal/sharing/rest/content/users/portaladmin//items/8e07a36e38ef4ffcb4b8d940a5dd772c/update?token=iPQ8CLqH_Rpm76bSm279zuBRYlaYEh7dbc_0YhQh0wb6kf2mywgSKAp73PliQI8ycz0jDgUM3eUdeb0PLMQNuYyYrP3o40VAwdkE2n7R3otngr-MEeKTdWBfdtV4C6U5SDAM-qESOgGGcqmO4Lmn6oX_F94_G7MdINvVZAQm4zAOEGLURh4JvZEOAq46xgNcxF_yOr46W8vuRUG5jwV0jU8RPTzQCXFhp83mr-WwjsgM69ehpbIWu_RLyGTwsy2n
      // uitzoeken met console app hoe de url en ook de body er moet zien, via pro testen en ontwikkelen duurt te lang
        await httpClient.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"));
      });
    }
  }
}
