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
using System.Linq;

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
      FrameworkApplication.State.Deactivate("TemplatesGallery_Is_Busy_State");
      var updateGalleryEvent = eventData as UpdateGalleryEvent;
      if(updateGalleryEvent != null)
      {
        // als er een template is aangemaakt moet de lijst met templates worden uitgebreid
        if (updateGalleryEvent.TemplateAdded)
        {
          lock (Module1._lock)
          {
            Add(new WebMapItem(updateGalleryEvent.Template));
            var newList = new List<WebMapItem>();
            foreach (var item in ItemCollectionCopy)
            {
              newList.Add(item as WebMapItem);
            }
            var orderedList = newList.OrderBy(x => x.Title);
            Clear();
            foreach (var item in orderedList)
            {
              Add(item);
            }
          }
        }
        // als er een template is verwijderd moet de lijst met temapltes worden ingekort
        if (updateGalleryEvent.TemplateDeleted)
        {
          lock (Module1._lock)
          {
            var itemToRemove = ItemCollection.FirstOrDefault(x => ((WebMapItem)x).ID.Equals(updateGalleryEvent.Template.ID));
            if (itemToRemove != null)
            {
              Remove(itemToRemove);
            }
          }
        }
        if(updateGalleryEvent.TemplateSelected)
        {
          // niets doen
        }
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
              // TODO description uit portaal ophalen
              map.UpdateSummary(clickedWebMapItem.Snippet);
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
            // TODO description uit portaal ophalen
            newMap.UpdateSummary(clickedWebMapItem.Snippet);
            await FrameworkApplication.Panes.CreateMapPaneAsync(newMap);
          }
        });
        FrameworkApplication.State.Deactivate("TemplatesGallery_Is_Busy_State");
        EventSender.Publish(new UpdateGalleryEvent() { TemplateSelected = true });
        _galleryBusy = false;
      }
    }
  }
}
