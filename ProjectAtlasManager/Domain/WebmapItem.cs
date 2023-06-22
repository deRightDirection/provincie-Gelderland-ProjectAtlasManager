using ArcGIS.Desktop.Core.Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Domain
{
  public class WebMapItem
  {
    private string _id;
    private string _title;
    private string _name;

    public WebMapItem(PortalItem portalItem)
    {
      _id = portalItem.ID;
      _title = portalItem.Title;
      _name = portalItem.Name;
      Snippet = string.IsNullOrEmpty(portalItem.Summary) ? _title : portalItem.Summary;
      Group = portalItem.Owner;
    }

    public string ID => _id;

    public string Title => _title;

    public string Name => _name;

    public string Snippet { get; }

    public string Group { get; }

    public override string ToString()
    {
      return Name;
    }
  }
}
