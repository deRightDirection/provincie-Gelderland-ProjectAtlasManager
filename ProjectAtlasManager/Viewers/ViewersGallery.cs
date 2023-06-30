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
  [Obsolete("is in dockpane verwerkt")]
  internal class ViewersGallery : Gallery
  {
    private bool _isInitialized;
    private bool _galleryBusy;

    public ViewersGallery()
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
    private void RenewData(UpdateGalleryEvent eventData)
    {
      if(eventData.UpdateViewersGallery)
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
      if(string.IsNullOrEmpty(Module1.SelectedProjectTemplate))
      {
        Clear();
        return;
      }
      _galleryBusy = true;
      LoadingMessage = "Loading viewers...";
      FrameworkApplication.State.Activate("ViewersGallery_Is_Busy_State");
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
        var query = new PortalQueryParameters($"type:\"Web Map\" AND tags:\"ProjectAtlas\" AND tags:\"PAT{Module1.SelectedProjectTemplate}\" AND tags:\"CopyOfTemplate\" AND orgid:{orgId} owner:\"{username}\"");
        query.SortField = "title";
        query.Limit = 100;
        var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
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
        FrameworkApplication.State.Deactivate("ViewersGallery_Is_Busy_State");
      }
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
      if (_galleryBusy)
        return;
      if (item is WebMapItem clickedWebMapItem)
      {
        _galleryBusy = true;
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
              var mapsWithSameTitleAsPortalItem = mapProjectItems.Where(x => !string.IsNullOrEmpty(x.Title) && x.Title.Equals(mapTitle, StringComparison.CurrentCultureIgnoreCase));
              Project.Current.RemoveItems(mapsWithSameTitleAsPortalItem);
              var newMap = MapFactory.Instance.CreateMapFromItem(currentItem);
              await FrameworkApplication.Panes.CreateMapPaneAsync(newMap);
            }
          });
        }
        finally
        {
          _galleryBusy = false;
          FrameworkApplication.State.Deactivate("ViewersGallery_Is_Busy_State");
        }
        SelectedItem = null;
      }
    }
  }
}
