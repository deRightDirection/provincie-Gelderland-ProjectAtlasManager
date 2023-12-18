using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Services
{
  internal static class MapProjectItemExtension
  {
    internal static string GetItemId(this MapProjectItem projectItem)
    {
      var uri = projectItem.SourceUri;
      if(!string.IsNullOrEmpty(uri))
      {
        var uriParts = uri.Split(new char[] { '/'}, StringSplitOptions.RemoveEmptyEntries);
        var lastPart = uriParts.Last();
        var tryParse = Guid.TryParseExact(lastPart, "N", out Guid result);
        if(tryParse)
        {
          return result.ToString("N");
        }
      }
      return string.Empty;
    }
  }
}
