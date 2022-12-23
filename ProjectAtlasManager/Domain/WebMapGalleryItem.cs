
using ArcGIS.Desktop.Core.Portal;

namespace ProjectAtlasManager.Domain
{
  /// <summary>
  /// Represents a web map item
  /// </summary>
  public class WebMapItemGalleryItem
  {

    public WebMapItemGalleryItem(PortalItem portalItem, string token)
    {
      ID = portalItem.ID;
      Title = portalItem.Title;
      Name = portalItem.Name;
      Thumbnail = portalItem.ThumbnailPath + $"?token={token}";
      Snippet = string.IsNullOrEmpty(portalItem.Summary) ? Title : portalItem.Summary;
      Group = portalItem.Owner;
    }

    public string ID { get; set; }

    public string Title { get; set; }

    public string Name { get; set; }
    public string Thumbnail { get; set; }

    public string Snippet { get; set; }
    public string Text { get; set; }
    public string Group { get; set; }

    public override string ToString()
    {
      return Name;
    }
  }
}
