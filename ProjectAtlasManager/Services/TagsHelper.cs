using ArcGIS.Desktop.Core.Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Services
{
  internal class TagsHelper
  {
    internal static string UpdateTags(PortalItem item)
    {
      var tags = new List<string>();
      foreach (var tag in item.ItemTags)
      {
        var tagValue = tag.ToLowerInvariant().Trim();
        if (tagValue.Equals("projectatlas") || tag.Equals("copyoftemplate") || tag.Equals("template") || tag.StartsWith("pat"))
        {
          continue;
        }
        tags.Add(tag.Trim());
      }
      return string.Join(",", tags);
    }

    internal static string ParseTags(PortalItem item)
    {
      var tags = new List<string>();
      foreach (var tag in item.ItemTags)
      {
        tags.Add(tag.Trim());
      }
      return string.Join(",", tags);
    }
  }
}
