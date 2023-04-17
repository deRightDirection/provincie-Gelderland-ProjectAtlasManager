using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProjectAtlasManager.Events;
using ProjectAtlasManager.ViewModels;
using ProjectAtlasManager.Web;
using ProjectAtlasManager.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Viewers
{
  class DeleteViewerButton : Button
  {
    private DeleteViewersWindow _deleteViewersWindow = null;
    private List<string> _itemIds;
    private bool _removeAsItem;

    private ProgressDialog progDialog;
    protected override async void OnClick()
    {
      if (_deleteViewersWindow != null)
      {
        return;
      }
      _deleteViewersWindow = new DeleteViewersWindow
      {
        Owner = FrameworkApplication.Current.MainWindow
      };
      _deleteViewersWindow.Closed += (o, e) =>
      {
        var dataContext = _deleteViewersWindow.DataContext as DeleteViewersWindowViewModel;
        _itemIds = dataContext.ItemIds;
        _removeAsItem = dataContext.RemoveAsItem;
        _deleteViewersWindow = null;
      };
      var result = _deleteViewersWindow.ShowDialog();
      if (_itemIds != null && _itemIds.Any())
      {
        progDialog = new ProgressDialog("Verwijder viewers...");
        progDialog.Show();
        await DeleteViewersAsync();
        Thread.Sleep(750);
        EventSender.Publish(new UpdateGalleryEvent() { UpdateTemplatesGallery = false, UpdateWebmapsGallery = true, UpdateViewersGallery = true });
      }
    }

    private Task DeleteViewersAsync()
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      var portalClient = new PortalClient(portal.PortalUri, portal.GetToken());
      return QueuedTask.Run(async () =>
      {
        Parallel.ForEach(_itemIds, async x =>
        {
          var query = PortalQueryParameters.CreateForItemsWithId(x);
          var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
          var item = results.Results.FirstOrDefault();
          if (item == null)
          {
            return;
          }
          if(_removeAsItem)
          {
            await portalClient.Delete(item);
          }
          else
          {
            var tags = UpdateTags(item);
            await portalClient.UpdateTags(item, tags);
          }
        });
        progDialog.Hide();
      });
    }

    private string UpdateTags(PortalItem item)
    {
      var tags = new List<string>();
      foreach(var tag in item.ItemTags)
      {
        if(tag.Equals("ProjectAtlas") || tag.Equals("CopyOfTemplate") || tag.Equals("Template") || tag.StartsWith("PAT"))
        {
          continue;
        }
        tags.Add(tag);
      }
      return string.Join(",", tags);
    }
  }
}
