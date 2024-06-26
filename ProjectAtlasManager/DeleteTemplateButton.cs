using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProjectAtlasManager.Events;
using ProjectAtlasManager.Services;
using ProjectAtlasManager.Web;
using System.Linq;
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
      var portal = ArcGISPortalManager.Current.GetActivePortal();
      var query = new PortalQueryParameters("id:" + Module1.SelectedProjectTemplate);
      Module1.SelectedProjectTemplate = string.Empty;
      Module1.SelectedProjectTemplateName = string.Empty;
      var results = await portal.SearchForContentAsync(query);
      var item = results.Results.FirstOrDefault();
      if (item == null)
      {
        return;
      }
      var tags = TagsHelper.UpdateTags(item.ItemTags);
      var portalClient = await QueuedTask.Run(() => new PortalClient(item.PortalUri, portal.GetToken()));
      await portalClient.UpdateTags(item, tags);
      await portalClient.UpdateTemplate(item, false);
      await Task.Delay(1500);
      EventSender.Publish(new UpdateGalleryEvent { TemplateDeleted = true, Template = item });
      FrameworkApplication.State.Deactivate("ProjectAtlasManager_Module_ProjectTemplateSelectedState");
    }
  }
}
