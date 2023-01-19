using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProjectAtlasManager
{
  /// <summary>
  /// de gallery met daarin alle webmaps die nog niet een tag "template" hebben
  /// </summary>
  internal class NewTemplateGallery : Gallery
  {
    private bool _isInitialized;

    protected override void OnDropDownOpened()
    {
      LoadItemsAsync();
    }

    protected override void OnUpdate()
    {
      if(FrameworkApplication.State.Contains("ProjectAtlasManager_Module_UpdateWebMapGalleryState"))
      {
          LoadItemsAsync(true);
      }
    }

    private async Task LoadItemsAsync(bool renew = false)
    {
      if(_isInitialized && !renew)
      {
        return;
      }
      var activePortal = ArcGISPortalManager.Current.GetActivePortal();
      if (activePortal == null)
      {
        return;
      }
      var items = await GetWebMapsAsync();
      SetItemCollection(new ObservableCollection<object>(items));
      _isInitialized = true;
      if(renew)
      {
        FrameworkApplication.State.Activate("ProjectAtlasManager_Module_ProjectTemplateGalleryState");
        FrameworkApplication.State.Deactivate("ProjectAtlasManager_Module_UpdateWebMapGalleryState");
      }
    }

    protected override void OnClick(object item)
    {
      if (item is WebMapItemGalleryItem)
      {
        var clickedWebMapItem = (WebMapItemGalleryItem)item;
        Module1.SelectedWebMapToUpgradeToTemplate = clickedWebMapItem.ID;
      }
      FrameworkApplication.State.Activate("ProjectAtlasManager_Module_WebMapSelectedState");
      base.OnClick(item);
    }

    /// <summary>
    /// Gets a collection of web map items from ArcGIS Online
    /// </summary>
    /// <returns></returns>
    private async Task<List<WebMapItemGalleryItem>> GetWebMapsAsync()
    {
      var lstWebmapItems = new List<WebMapItemGalleryItem>();
      try
      {
        await QueuedTask.Run(async () =>
        {
          ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
          var query = new PortalQueryParameters($"-tags:\"Template\" -tags:\"ProjectAtlas\" type:\"Web Map\" orgid:0123456789ABCDEF", 100);
          var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
          if (results == null)
            return;
          if(results.TotalResultsCount > 0)
          {
            foreach (var item in results.Results.OfType<PortalItem>())
            {
              if (!item.Owner.ToLowerInvariant().StartsWith("esri"))
              {
                lstWebmapItems.Add(new WebMapItemGalleryItem(item, portal.GetToken()));
              }
            }
          }
        });
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
      return lstWebmapItems;
    }
  }
}
