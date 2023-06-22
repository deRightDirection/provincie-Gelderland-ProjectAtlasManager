using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
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

namespace ProjectAtlasManager.Viewers
{
  internal class ViewersGallery : Gallery
  {
    private bool _isInitialized;
    private bool _gallery_busy;

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
        var portalInfo = await portal.GetPortalInfoAsync();
        var orgId = portalInfo.OrganizationId;
        var username = portal.GetSignOnUsername();
        var query = new PortalQueryParameters($"type:\"Web Map\" AND tags:\"ProjectAtlas\" AND tags:\"PAT{Module1.SelectedProjectTemplate}\" AND tags:\"CopyOfTemplate\" AND orgid:{orgId} owner:\"{username}\"");
        query.SortField = "title";
        query.Limit = 100;
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
        if (results == null)
        {
          return;
        }
        foreach (var item in results.Results.OfType<PortalItem>().OrderBy(x => x.Title))
        {
          lstWebmapItems.Add(new WebMapItemGalleryItem(item, portal.GetToken()));
        }
      });
      return lstWebmapItems;
    }

    protected override void OnClick(object item)
    {
      if (item == null)
        return;
      OpenWebMapAsync(item);
    }

    /// <summary>
    /// Opens a web map item in a map pane.
    /// </summary>
    /// <param name="item"></param>
    private async void OpenWebMapAsync(object item)
    {
      if (_gallery_busy)
        return;
      if (item is WebMapItem clickedWebMapItem)
      {
        _gallery_busy = true;
        FrameworkApplication.State.Activate("ViewersGallery_Is_Busy_State");
        try
        {
          await QueuedTask.Run(async () =>
          {
            //Open WebMap
            var currentItem = ItemFactory.Instance.Create(clickedWebMapItem.ID, ItemFactory.ItemType.PortalItem);
            var mapTitle = clickedWebMapItem.Title;
            var mapProjectItems = Project.Current.GetItems<MapProjectItem>();
            if (!string.IsNullOrEmpty(mapTitle))
            {
              var mapsWithSameTitleAsPortalItem = mapProjectItems.Where(
                x => !string.IsNullOrEmpty(x.Name) && x.Name.Equals(
                  mapTitle, StringComparison.CurrentCultureIgnoreCase));
              var mapItem = mapsWithSameTitleAsPortalItem.FirstOrDefault();
              if (mapItem != null)
              {
                var map = mapItem.GetMap();
                //is this map already active?
                if (MapView.Active?.Map?.URI == map.URI)
                  return;
                //has this map already been opened?
                var map_panes =
                  FrameworkApplication.Panes.OfType<IMapPane>();
                foreach (var map_pane in map_panes)
                {
                  if (map_pane.MapView.Map.URI == map.URI)
                  {
                    var pane = map_pane as Pane;
                      await FrameworkApplication.Current.Dispatcher.BeginInvoke((Action)(() => pane.Activate()));
                    return;
                  }
                }
                //open a new pane
                await FrameworkApplication.Panes.CreateMapPaneAsync(map);
                return;
              }
            }
            //open a new pane
            if (MapFactory.Instance.CanCreateMapFrom(currentItem))
            {
              var newMap = MapFactory.Instance.CreateMapFromItem(currentItem);
              await FrameworkApplication.Panes.CreateMapPaneAsync(newMap);
            }

          });
        }
        finally
        {
          _gallery_busy = false;
          FrameworkApplication.State.Deactivate("ViewersGallery_Is_Busy_State");
        }
      }
    }
  }
}
