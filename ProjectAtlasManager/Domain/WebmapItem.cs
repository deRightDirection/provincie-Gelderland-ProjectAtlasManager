using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json;
using ProjectAtlasManager.Services;
using ProjectAtlasManager.Web;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Domain
{
  public class WebMapItem
  {
    private string _title;
    private readonly Uri _portalUri;

    public WebMapItem()
    {
    }
    public WebMapItem(PortalItem portalItem)
    {
      _portalUri = portalItem.PortalUri;
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

    [JsonProperty("snippet")]
    public string Snippet { get; set; }

    public string Group { get; }

    public async Task Open()
    {
      //Open WebMap
      var currentItem = ItemFactory.Instance.Create(ID, ItemFactory.ItemType.PortalItem);
      var mapTitle = Title;
      if (!string.IsNullOrEmpty(mapTitle))
      {
        MapProjectItem mapItem = FindProjectItem(mapTitle);
        if (mapItem != null)
        {
          var map = mapItem.GetMap();
          var map_panes = FrameworkApplication.Panes.OfType<IMapPane>();
          foreach (var map_pane in map_panes)
          {
            var contentId = map_pane.ViewState?.ViewableObjectPath;
            if (contentId.Equals(map.URI))
            {
              var pane = map_pane as Pane;
              await FrameworkApplication.Current.Dispatcher.BeginInvoke((Action)(() => pane.Close()));
            }
          }
          Project.Current.RemoveItem(mapItem);
        }
      }
      var portal = ArcGISPortalManager.Current.GetActivePortal();
      if (portal != null)
      {
        var portalClient = await QueuedTask.Run(() => new PortalClient(portal.PortalUri, portal.GetToken()));
        var newItemData = await portalClient.GetItem(ID);
        var newItem = JsonConvert.DeserializeObject<WebMapItem>(newItemData);
        Snippet = newItem.Snippet;
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

    private MapProjectItem FindProjectItem(string mapTitle)
    {
      var mapProjectItems = Project.Current.GetItems<MapProjectItem>();
      var mapsWithSameTitleAsPortalItem = mapProjectItems.Where(
        x => !string.IsNullOrEmpty(x.Name)
              && x.Name.Equals(mapTitle, StringComparison.CurrentCultureIgnoreCase)
              && x.GetItemId().Equals(ID, StringComparison.CurrentCultureIgnoreCase));
      var mapItem = mapsWithSameTitleAsPortalItem.FirstOrDefault();
      return mapItem;
    }

    public override string ToString()
    {
      return Name;
    }
  }
}
