using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProjectAtlasManager.Events;
using ProjectAtlasManager.Services;
using ProjectAtlasManager.ViewModels;
using ProjectAtlasManager.Web;
using ProjectAtlasManager.Windows;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Viewers
{
  [Obsolete("is in dockpane verwerkt")]
  internal class NewViewerButton : Button
  {
    private NewViewerWindow _newviewerwindow = null;
    private string _name;
    private ProgressDialog progDialog;

    protected override async void OnClick()
    {
      if (_newviewerwindow != null)
      {
        return;
      }
      _newviewerwindow = new NewViewerWindow
      {
        Owner = FrameworkApplication.Current.MainWindow
      };
      _newviewerwindow.Closed += (o, e) =>
      {
        var dataContext = _newviewerwindow.DataContext as NewViewerWindowViewModel;
        _name = dataContext.Name;
        _newviewerwindow = null;
      };
      var result = _newviewerwindow.ShowDialog();
      if (result.Value)
      {
        progDialog = new ProgressDialog($"Aanmaken viewer [{_name}]...");
        progDialog.Show();
        await CreateNewViewerFromTemplate();
        progDialog.Hide();
      }
    }

    private async Task CreateNewViewerFromTemplate()
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      var query = new PortalQueryParameters("id:" + Module1.SelectedProjectTemplate);
      query.Limit = 1;
      await QueuedTask.Run(async () =>
      {
        var results = await portal.SearchForContentAsync(query);
        var item = results.Results.FirstOrDefault();
        if (item == null)
        {
          return;
        }
        var tags = TagsHelper.ParseTags(item);
        if (tags.Contains("Template"))
        {
          tags = tags.Replace("Template", string.Empty);
        }
        tags += ",CopyOfTemplate";
        if (!tags.Contains("ProjectAtlas"))
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
        tags = tags.Replace(",,", ",");
        var portalClient = new PortalClient(item.PortalUri, portal.GetToken());
        await portalClient.UpdateTemplate(item, true);
        await portalClient.CreateViewerFromTemplate(item, _name, tags);
      });
      Thread.Sleep(750);
      EventSender.Publish(new UpdateGalleryEvent() { UpdateTemplatesGallery = false, UpdateWebmapsGallery = false, UpdateViewersGallery = true });
    }
  }
}
