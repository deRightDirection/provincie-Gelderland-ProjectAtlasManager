using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using theRightDirection.Library;
using UnitTests.Domain;

namespace UnitTests.Templates
{
  class UnitTestWebMapManager
  {
    internal string Synchronize(string webmapData, IEnumerable<OperationalLayer> layersFromTemplate)
    {
      SetIndices(layersFromTemplate);
      var jsonObject = JToken.Parse(webmapData);
      foreach (var operationalLayer in layersFromTemplate)
      {
        var layerInWebMap = FindLayerInWebMap(jsonObject, operationalLayer);
        if (!layerInWebMap)
        {
          var operationalLayersInWebMap = jsonObject["operationalLayers"] as JArray;
          operationalLayersInWebMap?.Insert(operationalLayer.Index, operationalLayer.JsonDefinition);
        }
      }
      return jsonObject.ToString();
    }

    /// <summary>
    /// voeg een index toe aan alle lagen zodat ik kan bijhouden waar ik de nieuwe laag moet invoegen
    /// </summary>
    private void SetIndices(IEnumerable<OperationalLayer> layersFromTemplate)
    {
      var index = 0;
      layersFromTemplate.ForEach(x =>
      {
        x.Index = index;
        index++;
      });
    }

    /// <summary>
    /// opzoeken van een laag en eventueel vervangen, bij true de laag is gevonden en vervangen
    /// </summary>
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
