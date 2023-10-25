using System.Xml.Linq;
using System;

namespace UnitTests.Services
{
  internal static class UnitTestMapExtension
  {
    internal static string UpdateSummary(this string mapMetadata, string summary)
    {
      if (string.IsNullOrEmpty(mapMetadata))
      {
        return string.Empty;
      }
      XElement xml = null;
      try
      {
        xml = XElement.Parse(mapMetadata);
      }
      catch (Exception e)
      {
        return string.Empty;
      }
      if (xml == null)
      {
        return string.Empty;
      }
      var summaryElement = xml.Element("dataIdInfo")?.Element("idPurp");
      if (summaryElement != null)
      {
        summaryElement.Value = summary;
      }

      return xml.ToString();
    }
  }
}
