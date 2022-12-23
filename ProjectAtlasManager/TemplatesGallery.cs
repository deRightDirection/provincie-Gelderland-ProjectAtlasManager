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

namespace ProjectAtlasManager
{
  internal class TemplatesGallery : Gallery
  {
    private bool _isInitialized;

    public TemplatesGallery()
    {
      LoadItems();
    }

    private async Task LoadItems()
    {
      if (_isInitialized)
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
    }

    private async Task<List<WebMapItemGalleryItem>> GetWebMapsAsync()
    {
      var lstWebmapItems = new List<WebMapItemGalleryItem>();
      try
      {
        await QueuedTask.Run(async () =>
        {
          ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
          var query = new PortalQueryParameters($"type:\"Web map\" AND tags:\"ProjectAtlas\" AND tags:\"Template\" AND orgid:0123456789ABCDEF");
          var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
          if (results == null)
            return;

          foreach (var item in results.Results.OfType<PortalItem>())
          {
            lstWebmapItems.Add(new WebMapItemGalleryItem(item, portal.GetToken()));
          }
        });
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
      return lstWebmapItems;
    }

    protected override void OnClick(GalleryItem item)
    {
      //TODO - insert your code to manipulate the clicked gallery item here
      System.Diagnostics.Debug.WriteLine("Remove this line after adding your custom behavior.");
      base.OnClick(item);
    }
  }
}
