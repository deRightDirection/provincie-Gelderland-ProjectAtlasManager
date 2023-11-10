using ArcGIS.Desktop.Core.Portal;
using Newtonsoft.Json;
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

    public override string ToString()
    {
      return Name;
    }
  }
}
