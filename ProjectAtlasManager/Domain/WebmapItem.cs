using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json;
using ProjectAtlasManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Domain
{
  public class WebMapItem
  {
    private string _title;

    public WebMapItem()
    {
    }
    public WebMapItem(PortalItem portalItem)
    {
      ID = portalItem.ID;
      Title = portalItem.Title;
      Name = portalItem.Name;
      Snippet = string.IsNullOrEmpty(portalItem.Summary) ? Title : portalItem.Summary;
      Group = portalItem.Owner;
    }

    public string ID { get; set; }

    public string Title
    {
      get
      {
        if (string.IsNullOrEmpty(_title))
        {
          return Name;
        }
        return _title;
      }
      set
      {
        _title = value;
      }
    }

    [JsonProperty("title")]
    public string Name { get; set; }

    public string Snippet { get; }

    public string Group { get; }

    public async Task Open()
    {
      //Open WebMap
      var currentItem = ItemFactory.Instance.Create(ID, ItemFactory.ItemType.PortalItem);
      var mapTitle = Title;
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
          map.UpdateSummary(Snippet);
          //is this map already active?
          if (MapView.Active?.Map?.URI == map.URI)
            return;
          //has this map already been opened?
          var map_panes = FrameworkApplication.Panes.OfType<IMapPane>();
          foreach (var map_pane in map_panes)
          {
            var contentId = map_pane.ViewState?.ViewableObjectPath;
            if(contentId.Equals(map.URI))
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
        newMap.UpdateSummary(Snippet);
        await FrameworkApplication.Panes.CreateMapPaneAsync(newMap);
      }
    }

    public override string ToString()
    {
      return Name;
    }
  }
}
