using ArcGIS.Desktop.Core.Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProjectAtlasManager.Services
{
  internal static class PortalItemExtension
  {
    internal static bool HasTemplateTags(this PortalItem item)
    {
      var tags = item.ItemTags.Select(x => x.ToLowerInvariant().Trim());
      var one = tags.Contains("projectatlas");
      var two = tags.Contains("template");
      var three = tags.Contains($"pat{item.ID.ToLowerInvariant()}");
      return one && two && three;
    }

    internal static void UpdateTagsForTemplate(this PortalItem item)
    {
      XElement xml = null;
      try
      {
        xml = XElement.Parse(item.GetXml());
      }
      catch (Exception e)
      {
        return;
      }
      if(xml == null)
      {
        return;
      }
      var keysTags = xml.Element("dataIdInfo").Element("searchKeys");
      var tags = item.ItemTags.Select(x => x.ToLowerInvariant().Trim());
      if (keysTags != null)
      {
        var one = tags.Contains("projectatlas");
        if (!one)
        {
          keysTags.Add(new XElement("keyword", "ProjectAtlas"));
        }
        var two = tags.Contains("template");
        if (!two)
        {
          keysTags.Add(new XElement("keyword", "Template"));
        }
        var three = tags.Contains($"pat{item.ID.ToLowerInvariant()}");
        if (!three)
        {
          keysTags.Add(new XElement("keyword", $"PAT{item.ID}"));
        }
      }
      else
      {
        var keysTag = new XElement("searchKeys");
        keysTag.Add(new XElement("keyword", "ProjectAtlas"));
        keysTag.Add(new XElement("keyword", "Template"));
        keysTag.Add(new XElement("keyword", $"PAT{item.ID}"));
        xml.Add(keysTag);
      }
      try
      {
        item.SetXml(xml.ToString());
      }
      catch(InvalidOperationException)
      {

      }
    }
  }
}
