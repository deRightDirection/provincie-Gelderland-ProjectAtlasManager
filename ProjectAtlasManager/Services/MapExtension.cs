using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProjectAtlasManager.Services
{
  internal static class MapExtension
  {
    internal static void UpdateSummary(this Map map, string summary)
    {
      var mapMetadata = map.GetMetadata();
      if (string.IsNullOrEmpty(mapMetadata))
      {
        return;
      }
      XElement xml = null;
      try
      {
        xml = XElement.Parse(mapMetadata);
      }
      catch (Exception e)
      {
        return;
      }
      if (xml == null)
      {
        return;
      }
      var summaryElement = xml.Element("dataIdInfo")?.Element("idPurp");
      if (summaryElement != null)
      {
        summaryElement.Value = summary;
      }
      map.SetMetadata(xml.ToString());
    }
  }
}
