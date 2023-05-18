using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
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
using ProjectAtlasManager.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ProjectAtlasManager.Viewers
{
  internal class ViewersGallery : Gallery
  {
    private bool _isInitialized;

    public ViewersGallery()
    {
      EventSender.Subscribe(RenewData, true);
      ActivePortalChangedEvent.Subscribe((args) =>
      {
        CheckStatus();
      });
      PortalSignOnChangedEvent.Subscribe((args) =>
      {
        CheckStatus();
      });
      Initialize();
    }

    private void CheckStatus()
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      if (portal != null && portal.IsSignedOn())
      {
        EventSender.Subscribe(RenewData, true);
      }
      else
      {
        SetItemCollection(new ObservableCollection<object>());
      }
    }
    private void RenewData(UpdateGalleryEvent eventData)
    {
      if(eventData.UpdateViewersGallery)
      {
        LoadItemsAsync(true);
      }
    }

    protected override void OnDropDownOpened()
    {
      LoadItemsAsync(true);
    }
    private async void Initialize()
    {
      await LoadItemsAsync();
    }
    private async Task LoadItemsAsync(bool renew = false)
    {
      if (_isInitialized && !renew)
      {
        return;
      }
      var activePortal = ArcGISPortalManager.Current.GetActivePortal();
      if (activePortal == null || activePortal.IsSignedOn() == false)
      {
        SetItemCollection(new ObservableCollection<object>());
        return;
      }
      var items = await GetWebMapsAsync();
      SetItemCollection(new ObservableCollection<object>(items));
      _isInitialized = true;
    }

    private async Task<List<WebMapItemGalleryItem>> GetWebMapsAsync()
    {
      var lstWebmapItems = new List<WebMapItemGalleryItem>();
      await QueuedTask.Run(async () =>
      {
        ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
        var username = portal.GetSignOnUsername();
        var query = new PortalQueryParameters($"type:\"Web Map\" AND tags:\"ProjectAtlas\" AND tags:\"PAT{Module1.SelectedProjectTemplate}\" AND tags:\"CopyOfTemplate\" AND orgid:{Module1.OrgId} owner:\"{username}\"");
        query.SortField = "title";
        query.Limit = 100;
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        if (results == null)
        {
          return;
        }
        foreach (var item in results.Results.OfType<PortalItem>())
        {
          lstWebmapItems.Add(new WebMapItemGalleryItem(item, portal.GetToken()));
        }
      });
      return lstWebmapItems;
    }

    protected override async void OnClick(object item)
    {
      if (item is WebMapItemGalleryItem)
      {
        var clickedWebMapItem = (WebMapItemGalleryItem)item;
        Module1.SelectedViewer = clickedWebMapItem.ID;
        ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
        var query = PortalQueryParameters.CreateForItemsWithId(Module1.SelectedViewer);
        PortalQueryResultSet<PortalItem> results = await portal.SearchForContentAsync(query);
        var result = results.Results.FirstOrDefault();
        if (result == null)
        {
          return;
        }
        QueuedTask.Run(() =>
        {
          if (MapFactory.Instance.CanCreateMapFrom(result))
          {
            var newMap = MapFactory.Instance.CreateMapFromItem(result);
            ProApp.Panes.CreateMapPaneAsync(newMap);
          }
        });
      }
      base.OnClick(item);
    }
  }
}
