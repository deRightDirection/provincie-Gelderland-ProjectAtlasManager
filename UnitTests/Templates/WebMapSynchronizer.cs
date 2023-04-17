using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ProjectAtlasManager.Domain;

namespace ProjectAtlasManager.Services
{
  class WebMapSynchronizer
  {
    internal string Synchronize(string webmapData, IEnumerable<OperationalLayer> layersFromTemplate)
    {
      var jsonObject = JToken.Parse(webmapData);
      foreach (var operationalLayer in layersFromTemplate)
      {
        var layerInWebMap = FindLayerInWebMap(jsonObject, operationalLayer);
        if (!layerInWebMap)
        {
          var operationalLayersInWebMap = jsonObject["operationalLayers"] as JArray;
          operationalLayersInWebMap?.Add(operationalLayer.JsonDefinition);
        }
      }
      return jsonObject.ToString();
    }

    private bool FindLayerInWebMap(JToken jsonObject, OperationalLayer layerFromTemplate)
    {
      var operationalLayersInWebMap = jsonObject["operationalLayers"];
      foreach (var operationalLayer in operationalLayersInWebMap)
      {
        if (operationalLayer["id"].ToString().Equals(layerFromTemplate.Id))
        {
          operationalLayer.Replace(layerFromTemplate.JsonDefinition);
          return true;
        }
      }

      return false;
    }
  }
}
