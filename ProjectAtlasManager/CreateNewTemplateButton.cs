using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProjectAtlasManager.Events;
using ProjectAtlasManager.Services;
using ProjectAtlasManager.Web;
using System.Linq;
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
      var query = new PortalQueryParameters("id:" + Module1.SelectedWebMapToUpgradeToTemplate)
      {
        Limit = 100
      };
      await QueuedTask.Run(async () =>
      {
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        var item = results.Results.FirstOrDefault();
        if (item == null)
        {
          return;
        }
        item.Refresh();
        var tags = TagsHelper.ParseTags(item.ItemTags);
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
        await portalClient.UpdateTemplate(item, true);
        await portalClient.UpdateTags(item, tags);
        FrameworkApplication.State.Deactivate("ProjectAtlasManager_Module_WebMapSelectedState");
        EventSender.Publish(new UpdateGalleryEvent { TemplateAdded = true, Template = item });
      });
    }
  }
}
