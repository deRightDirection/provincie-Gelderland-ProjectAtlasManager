using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProjectAtlasManager.Domain;
using ProjectAtlasManager.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectAtlasManager
{
  /// <summary>
  /// de gallery met daarin alle webmaps die nog niet een tag "template" hebben
  /// </summary>
  internal class NewTemplateGallery : Gallery
  {
    private bool _isInitialized;
    private bool _galleryBusy;

    public NewTemplateGallery()
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
      Clear();
      LoadItemsAsync();
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
      LoadingMessage = "Loading webmaps...";
      FrameworkApplication.State.Activate("WebmapsGallery_Is_Busy_State");
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
        var query = new PortalQueryParameters($"-tags:\"ProjectAtlas\" type:\"Web Map\" orgid:{orgId} owner:\"{username}\"");
        query.SortField = "title, modified";
        query.Limit = 100;
        var results = await portal.SearchForContentAsync(query);
        if (results == null)
        {
          Clear();
          return;
        }
        if (results.TotalResultsCount > 0)
        {
          foreach (var item in results.Results.OfType<PortalItem>().OrderBy(x => x.Title))
          {
            if (!item.Owner.ToLowerInvariant().StartsWith("esri"))
            {
              Add(new WebMapItem(item));
            }
          }
        }
      }
      finally
      {
        _galleryBusy = false;
        FrameworkApplication.State.Deactivate("WebmapsGallery_Is_Busy_State");
      }
    }

    protected override void OnClick(object item)
    {
      if (item is WebMapItem)
      {
        var clickedWebMapItem = (WebMapItem)item;
        Module1.SelectedWebMapToUpgradeToTemplate = clickedWebMapItem.ID;
      }
      FrameworkApplication.State.Activate("ProjectAtlasManager_Module_WebMapSelectedState");
      base.OnClick(item);
    }
  }
}
