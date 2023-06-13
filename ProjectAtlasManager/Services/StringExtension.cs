using Newtonsoft.Json.Linq;
using ProjectAtlasManager.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Services
{
  internal static class StringExtension
  {
    /// <summary>
    /// haal alle lagen op die in de webmap, het level bepaalt hoe diep in de boom
    /// </summary>
    internal static IEnumerable<OperationalLayer> RetrieveLayers(this string json)
    {
      var webmap = JToken.Parse(json);
      var result = new List<OperationalLayer>();
      RetrieveLayers(webmap, "operationalLayers", result, -1, null);
      return result;
    }

    private static void RetrieveLayers(JToken jsonObject, string tokenName, List<OperationalLayer> result, int level, string parent)
    {
      var operationalLayers = jsonObject[tokenName];
      var index = 0;
      var currentLevel = level + 1;
      foreach (var layer in operationalLayers)
      {
        var operationalLayer = new OperationalLayer
        {
          Id = layer["id"].ToString(),
          JsonDefinition = layer as JObject,
          Level = currentLevel,
          Index = index,
          Title = layer["title"].ToString(),
          LayerType = layer["layerType"].ToString(),
          Parent = parent
        };
        result.Add(operationalLayer);
        index++;
        if (operationalLayer.LayerType.Equals("GroupLayer"))
        {
          RetrieveLayers(operationalLayer.JsonDefinition, "layers", result, currentLevel, operationalLayer.Id);
        }
      }
    }
  }
}
