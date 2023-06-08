using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ProjectAtlasManager.Domain;
using theRightDirection.Library;

namespace ProjectAtlasManager.Services
{
  class WebMapManager
  {
    internal string Synchronize(string webmapData, string templateData)
    {
      var layersInTemplate = RetrieveLayers(templateData);
      var layersInWebMap = RetrieveLayers(webmapData);
      var levelsInTemplate = layersInTemplate.Select(x => x.Level).Distinct().OrderBy(x => x);
      var layersToAdd = new List<OperationalLayer>();
      var layersToReplace = new List<OperationalLayer>();
      foreach (var level in levelsInTemplate)
      {
        var layersOnLevel = layersInTemplate.Where(x => x.Level == level);
        foreach (var templateLayer in layersOnLevel)
        {
          var foundInWebMap = layersInWebMap.FirstOrDefault(x => x.Level == level && x.Id.Equals(templateLayer.Id));
          if (foundInWebMap == null)
          {
            layersToAdd.Add(templateLayer);
          }
          else
          {
            layersToReplace.Add(templateLayer);
          }
        }
      }
      if (layersToAdd.Any())
      {
        webmapData = InsertLayersFromTemplate(webmapData, templateData, layersToAdd);
      }
      layersToReplace = layersToReplace.Where(x => !x.LayerType.Equals("GroupLayer")).ToList();
      if (layersToReplace.Any())
      {
        webmapData = InsertLayersFromTemplate(webmapData, templateData, layersToReplace);
      }
      layersInWebMap = RetrieveLayers(webmapData);
      var newOrder = MakeIndicesLayersEqual(layersInTemplate, layersInWebMap, 0, null);
      var json = CreateNewOperationalLayerJsonObject(newOrder, new JArray());
      var webmap = JObject.Parse(webmapData);
      webmap["operationalLayers"] = json;
      return webmap.ToString();
    }
    /// <summary>
    /// maak een nieuw json object van de bijgewerkte lagen
    /// </summary>
    private JArray CreateNewOperationalLayerJsonObject(IEnumerable<OperationalLayer> layers, JArray rootObject)
    {
      foreach (var webmapLayer in layers)
      {
        var newObject = webmapLayer.JsonDefinition;
        if (webmapLayer.LayerType.Equals("GroupLayer"))
        {
          newObject["layers"] = CreateNewOperationalLayerJsonObject(webmapLayer.SubLayers, new JArray());
        }
        rootObject.Add(newObject);
      }
      return rootObject;
    }
    /// <summary>
    /// herrangschik de lagen indien ze van volgorde zijn veranderd
    /// </summary>
    private IEnumerable<OperationalLayer> MakeIndicesLayersEqual(IEnumerable<OperationalLayer> layersInTemplate, IEnumerable<OperationalLayer> layersInWebMap, int level, string parent)
    {
      var templateLayers = layersInTemplate.Where(x => x.Level == level && x.Parent == parent);
      var webmapLayers = layersInWebMap.Where(x => x.Level == level && x.Parent == parent);
      int indexSkip = 0;
      foreach (var webmapLayer in webmapLayers)
      {
        if (webmapLayer.LayerType.Equals("GroupLayer"))
        {
          var newSubLayers = MakeIndicesLayersEqual(layersInTemplate, layersInWebMap, webmapLayer.Level + 1, webmapLayer.Id);
          webmapLayer.SubLayers = newSubLayers.ToList();
        }
        var templateLayer = templateLayers.FirstOrDefault(x => x.Id.Equals(webmapLayer.Id));
        webmapLayer.NewIndex = templateLayer?.Index ?? webmapLayer.Index;
      }
      var newList = new List<OperationalLayer>();
      foreach (var webmapLayer in webmapLayers.OrderBy(x => x.NewIndex))
      {
        var templateLayer = templateLayers.FirstOrDefault(x => x.Id.Equals(webmapLayer.Id));
        if (templateLayer == null)
        {
          if (newList.Count >= webmapLayer.Index)
          {
            newList.Insert(webmapLayer.Index, webmapLayer);
          }
          else
          {
            newList.Add(webmapLayer);
          }
        }
        else
        {
          newList.Add(webmapLayer);
        }
      }
      return newList;
    }

    /// <summary>
    /// voeg lagen toe aan de copy-webmap
    /// </summary>
    private string InsertLayersFromTemplate(string webmapData, string templateData, List<OperationalLayer> layersToAdd)
    {
      var webmap = JObject.Parse(webmapData);
      var template = JObject.Parse(templateData);
      foreach (var layerToAdd in layersToAdd)
      {
        var filter = $"..*[?(@.id == '{layerToAdd.Id}')]";
        var templateLayer = (JObject)template.SelectToken(filter);
        if (templateLayer != null)
        {
          var parentInWebMap = GetParentFromWebMap(webmap, layerToAdd.Parent);
          if (parentInWebMap == null)
          {
            continue;
          }
          InsertOrReplaceLayer(layerToAdd, parentInWebMap);
        }
      }
      return webmap.ToString();
    }

    private void InsertOrReplaceLayer(OperationalLayer layerToAdd, JToken parentInWebMap)
    {
      var tokenName = layerToAdd.Parent == null ? "operationalLayers" : "layers";
      var layersInParent = parentInWebMap[tokenName] as JArray;
      var layerFilter = $"..*[?(@.id == '{layerToAdd.Id}')]";
      var layerExist = layersInParent.SelectToken(layerFilter);
      if (layerExist == null)
      {
        layersInParent.Insert(layerToAdd.Index, layerToAdd.JsonDefinition);
      }
      else
      {
        var index = layersInParent.IndexOf(layerExist);
        layersInParent[index] = layerToAdd.JsonDefinition;
      }
    }

    /// <summary>
    /// haal een token met layers-token op uit de webmap
    /// </summary>
    private JToken GetParentFromWebMap(JObject webmap, string parent)
    {
      if (parent == null)
      {
        return webmap;
      }
      var parentFilter = $"..*[?(@.id == '{parent}')]";
      try
      {
        return webmap.SelectToken(parentFilter);
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// haal alle lagen op die in de webmap, het level bepaalt hoe diep in de boom
    /// </summary>
    internal IEnumerable<OperationalLayer> RetrieveLayers(string json)
    {
      var webmap = JToken.Parse(json);
      var result = new List<OperationalLayer>();
      RetrieveLayers(webmap, "operationalLayers", result, -1, null);
      return result;
    }
    private void RetrieveLayers(JToken jsonObject, string tokenName, List<OperationalLayer> result, int level, string parent)
    {
      var operationalLayers = jsonObject[tokenName];
      var index = 0;
      var currentLevel = level + 1;
      foreach (var layer in operationalLayers)
      {
        var operationalLayer = new OperationalLayer();
        operationalLayer.Id = layer["id"].ToString();
        operationalLayer.JsonDefinition = layer as JObject;
        operationalLayer.Level = currentLevel;
        operationalLayer.Index = index;
        operationalLayer.LayerType = layer["layerType"].ToString();
        operationalLayer.Parent = parent;
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
