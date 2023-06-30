using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ProjectAtlasManager.Dockpanes;
using ProjectAtlasManager.Domain;
using ProjectAtlasManager.Events;
using ProjectAtlasManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectAtlasManager
{
  internal class TemplatesGallery : Gallery
  {
    private bool _isInitialized;
    private bool _galleryBusy;

    public TemplatesGallery()
    {
      EventSender.Subscribe(RenewData, true);
      ActivePortalChangedEvent.Subscribe((args) =>
      {
        Clear();
        LoadItemsAsync();
      });
      PortalSignOnChangedEvent.Subscribe((args) =>
      {
        Clear();
        LoadItemsAsync();
      });
      Initialize();
    }

    private void RenewData(EventBase eventData)
    {
      var updateGalleryEvent = eventData as UpdateGalleryEvent;
      if (updateGalleryEvent != null && updateGalleryEvent.UpdateTemplatesGallery)
      {
        Clear();
        LoadItemsAsync();
      }
    }

    protected override void OnDropDownOpened()
    {
      Initialize();
    }

    private void Initialize()
    {
      if (_isInitialized)
        return;
      _isInitialized = true;
      LoadItemsAsync();
    }

    private async void LoadItemsAsync()
    {
      if (_galleryBusy)
        return;
      _galleryBusy = true;
      LoadingMessage = "Loading templates...";
      FrameworkApplication.State.Activate("TemplatesGallery_Is_Busy_State");
      try
      {
        var portal = ArcGISPortalManager.Current.GetActivePortal();
        if (portal == null)
        {
          Clear();
          LoadingMessage = "Sign on to retrieve web maps";
          return;
        }

        var signedOn = await QueuedTask.Run(() => portal.IsSignedOn());
        if (!signedOn)
        {
          Clear();
          LoadingMessage = "Sign on to retrieve web maps";
          return;
        }

        var portalInfo = await portal.GetPortalInfoAsync();
        var orgId = portalInfo.OrganizationId;
        var username = portal.GetSignOnUsername();
        var query = new PortalQueryParameters(
          $"type:\"Web Map\" AND tags:\"ProjectAtlas\" AND tags:\"Template\" AND orgid:{orgId} owner:\"{username}\"");
        query.SortField = "title";
        query.Limit = 100;
        var results = await portal.SearchForContentAsync(query);
        if (results == null)
        {
          Clear();
          return;
        }

        foreach (var item in results.Results.OfType<PortalItem>().OrderBy(x => x.Title))
        {
          Add(new WebMapItem(item));
        }
      }
      finally
      {
        _galleryBusy = false;
        FrameworkApplication.State.Deactivate("TemplatesGallery_Is_Busy_State");
      }
    }

    protected override void OnClick(object item)
    {
      if (item == null)
        return;
      var clickedWebMapItem = (WebMapItem)item;
      Module1.SelectedProjectTemplate = clickedWebMapItem.ID;
      Module1.SelectedProjectTemplateName = clickedWebMapItem.Title;
      FrameworkApplication.State.Activate("ProjectAtlasManager_Module_ProjectTemplateSelectedState");
      OpenWebMapAsync(item);
      ViewersDockpaneViewModel.ShowOrHide();
    }

    /// <summary>
    /// Opens a web map item in a map pane.
    /// </summary>
    /// <param name="item"></param>
    private async void OpenWebMapAsync(object item)
    {
      if (_galleryBusy)
        return;
      if (item is WebMapItem clickedWebMapItem)
      {
        _galleryBusy = true;
        FrameworkApplication.State.Activate("TemplatesGallery_Is_Busy_State");
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
                  if (map_pane.MapView.Map?.URI == null)
                  {
                    continue;
                  }

                  if (map_pane.MapView.Map.URI == map.URI)
                  {
                    var pane = map_pane as Pane;
                    await FrameworkApplication.Current.Dispatcher.BeginInvoke((Action) (() => pane.Activate()));
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
          _galleryBusy = false;
          FrameworkApplication.State.Deactivate("TemplatesGallery_Is_Busy_State");
          EventSender.Publish(new UpdateGalleryEvent() { UpdateTemplatesGallery = false, UpdateWebmapsGallery = false, UpdateViewersGallery = true });
        }
      }
    }
  }
}
