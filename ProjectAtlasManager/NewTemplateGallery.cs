using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProjectAtlasManager.Domain;
using ProjectAtlasManager.Events;
using System.Collections.Generic;
using System.Linq;

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
        LoadItemsAsync();
      });
      PortalSignOnChangedEvent.Subscribe((args) =>
      {
        LoadItemsAsync();
      });
      Initialize();
    }

    private void RenewData(EventBase eventData)
    {
      var updateGalleryEvent = eventData as UpdateGalleryEvent;
      if (updateGalleryEvent != null)
      {
        // als er een template is aangemaakt moet de lijst met webmaps worden verkleind
        if (updateGalleryEvent.TemplateAdded)
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
        // als er een template is verwijderd moet de lijst met webmaps worden uitgebreid
        // of er is een nieuwe viewer aangemaakt en vervolgens los gekoppeld van een template
        if (updateGalleryEvent.TemplateDeleted || updateGalleryEvent.ViewerDeleted)
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
        if(updateGalleryEvent.DataRefreshed)
        {
          lock (Module1._lock)
          {
            LoadItemsAsync();
          }
        }
        if (updateGalleryEvent.TemplateSelected)
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
      Clear();
      _galleryBusy = true;
      LoadingMessage = "Loading webmaps...";
      FrameworkApplication.State.Activate("WebmapsGallery_Is_Busy_State");
      try
      {
        var portal = ArcGISPortalManager.Current.GetActivePortal();
        if (portal == null)
        {
          LoadingMessage = "Sign on to retrieve web maps";
          return;
        }
        var signedOn = await QueuedTask.Run(() => portal.IsSignedOn());
        if (!signedOn)
        {
          LoadingMessage = "Sign on to retrieve web maps";
          return;
        }
        var portalInfo = await portal.GetPortalInfoAsync();
        var orgId = portalInfo.OrganizationId;
        var username = await QueuedTask.Run(() => portal.GetSignOnUsername());
        var query = new PortalQueryParameters($"-tags:\"ProjectAtlas\" type:\"Web Map\" orgid:{orgId} owner:\"{username}\"")
        {
          SortField = "title, modified",
          Limit = 100
        };
        var results = await portal.SearchForContentAsync(query);
        if (results == null)
        {
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
