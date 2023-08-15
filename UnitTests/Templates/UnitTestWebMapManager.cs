using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using theRightDirection.Library;
using UnitTests.Domain;

namespace UnitTests.Templates
{
  class UnitTestWebMapManager
  {
    internal string Synchronize(string webmapData, string templateData)
    {
      var layersInTemplate = RetrieveLayers(templateData);
      var layersInWebMap = RetrieveLayers(webmapData);
      webmapData = MakeGroupLayerIndicesEqual(layersInTemplate, layersInWebMap, webmapData);
      var levelsInTemplate = layersInTemplate.Select(x => x.Level).Distinct().OrderBy(x => x);
      var layersToAdd = new List<OperationalLayer>();
      var layersToReplace = new List<OperationalLayer>();
      var layersToRemove = new List<OperationalLayer>();
      DetermineLayersToAddOrReplace(levelsInTemplate, layersInTemplate, layersInWebMap, layersToAdd, layersToReplace);
      DetermineLayersToRemove(levelsInTemplate, layersInTemplate, layersInWebMap, layersToRemove);
      if (layersToRemove.Any())
      {
        webmapData = RemoveLayersFromTemplate(webmapData, layersToRemove);
      }
      if (layersToAdd.Any())
      {
        webmapData = InsertLayersFromTemplate(webmapData, templateData, layersToAdd);
      }
      var groupLayersToUpdate = layersToReplace.Where(x => x.IsGroupLayer).ToList();
      layersToReplace = layersToReplace.Where(x => !x.IsGroupLayer).ToList();
      if (layersToReplace.Any())
      {
        webmapData = InsertLayersFromTemplate(webmapData, templateData, layersToReplace);
      }
      if (groupLayersToUpdate.Any())
      {
        webmapData = UpdateGrouplayerInformation(webmapData, templateData, groupLayersToUpdate);
      }
      layersInWebMap = RetrieveLayers(webmapData);
      var newOrder = MakeIndicesLayersEqual(layersInTemplate, layersInWebMap, 0, null);
      var json = CreateNewOperationalLayerJsonObject(newOrder, new JArray());
      // 23-06-2023: op verzoek van Mark uitgezet
      //var json = CreateNewOperationalLayerJsonObject(layersInWebMap, new JArray());
      var webmap = JObject.Parse(webmapData);
      webmap["operationalLayers"] = json;
      return webmap.ToString();
    }

    /// <summary>
    /// zorg ervoor dat de visibility-instelling van een grouplayer wordt overgenomen in de webmap vanuit het template
    /// </summary>
    private string UpdateGrouplayerInformation(string webmapData, string templateData, List<OperationalLayer> groupLayersToUpdate)
    {
      var webmap = JObject.Parse(webmapData);
      var template = JObject.Parse(templateData);
      foreach (var grouplayerToUpdate in groupLayersToUpdate)
      {
        var filter = $"..*[?(@.id == '{grouplayerToUpdate.Id}')]";
        var templateLayer = (JObject)template.SelectToken(filter);
        if (templateLayer != null)
        {
          var webmapLayer = (JObject)webmap.SelectToken(filter);
          if (webmapLayer != null)
          {
            var visibilityValue = templateLayer["visibility"];
            if (visibilityValue != null)
            {
              webmapLayer["visibility"] = visibilityValue;
            }
          }
        }
      }
      return webmap.ToString();
    }

    /// <summary>
    /// verwijder lagen uit de webmap
    /// </summary>
    private string RemoveLayersFromTemplate(string webmapData, List<OperationalLayer> layersToRemove)
    {
      var webmap = JObject.Parse(webmapData);
      foreach (var layerToRemove in layersToRemove)
      {
        var filter = $"..*[?(@.id == '{layerToRemove.Id}')]";
        var webmapLayer = (JObject)webmap.SelectToken(filter);
        webmapLayer.Remove();
      }
      return webmap.ToString();
    }

    /// <summary>
    /// bepaal welke lagen in de webmap vervangen moeten worden of er aan toegevoegd vanuit het template
    /// </summary>
    private void DetermineLayersToAddOrReplace(IOrderedEnumerable<int> levelsInTemplate, IEnumerable<OperationalLayer> layersInTemplate,
      IEnumerable<OperationalLayer> layersInWebMap, List<OperationalLayer> layersToAdd, List<OperationalLayer> layersToReplace)
    {
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
    }

    /// <summary>
    /// bepaal welke lagen in de webmap die verwijderd moeten worden
    /// </summary>
    private void DetermineLayersToRemove(IOrderedEnumerable<int> levelsInTemplate, IEnumerable<OperationalLayer> layersInTemplate,
      IEnumerable<OperationalLayer> layersInWebMap, List<OperationalLayer> layersToRemove)
    {
      foreach (var level in levelsInTemplate)
      {
        var layersOnLevel = layersInWebMap.Where(x => x.Level == level && x.IsPATLayer);
        foreach (var webmapLayer in layersOnLevel)
        {
          var foundInTemplate = layersInTemplate.FirstOrDefault(x => x.Level == level && x.Id.Equals(webmapLayer.Id));
          if (foundInTemplate == null)
          {
            layersToRemove.Add(webmapLayer);
          }
        }
      }
    }

    /// <summary>
    /// in ArcGIS Pro worden bij het opslaan van een webmap de id's van een grouplayer veranderd, voor de logica
    /// maken we de layerid's dan in de webmap/viewer gelijk waarbij de match nu wordt gelegd op title
    /// </summary>
    private string MakeGroupLayerIndicesEqual(IEnumerable<OperationalLayer> layersInTemplate, IEnumerable<OperationalLayer> layersInWebMap, string webmapData)
    {
      foreach (var templateGroupLayer in layersInTemplate.Where(x => x.IsGroupLayer))
      {
        var webmapLayer = layersInWebMap.FirstOrDefault(x => LayerFound(templateGroupLayer, x));
        if (webmapLayer != null)
        {
          webmapData = webmapData.Replace(webmapLayer.Id, templateGroupLayer.Id);
          var json = webmapLayer.JsonDefinition.ToString();
          json = json.Replace(webmapLayer.Id, templateGroupLayer.Id);
          webmapLayer.JsonDefinition = JObject.Parse(json);
          webmapLayer.Id = templateGroupLayer.Id;
        }
      }
      return webmapData;
    }

    private bool LayerFound(OperationalLayer webmapLayer, OperationalLayer templateLayer)
    {
      if (webmapLayer.Level != templateLayer.Level)
      {
        return false;
      }
      if (templateLayer.Id.Equals(webmapLayer.Id))
      {
        return true;
      }
      if (templateLayer.IsGroupLayer)
      {
        return webmapLayer.Title.ToLowerInvariant().Equals(templateLayer.Title.ToLowerInvariant());
      }
      return false;
    }

    private JArray CreateNewOperationalLayerJsonObject(IEnumerable<OperationalLayer> layers, JArray rootObject)
    {
      foreach (var webmapLayer in layers)
      {
        var newObject = webmapLayer.JsonDefinition;
        if (webmapLayer.IsGroupLayer)
        {
          newObject["layers"] = CreateNewOperationalLayerJsonObject(webmapLayer.SubLayers, new JArray());
        }
        rootObject.Add(newObject);
      }
      return rootObject;
    }

    private IEnumerable<OperationalLayer> MakeIndicesLayersEqual(IEnumerable<OperationalLayer> layersInTemplate, IEnumerable<OperationalLayer> layersInWebMap, int level, string parent)
    {
      var templateLayers = layersInTemplate.Where(x => x.Level == level && x.Parent == parent);
      var webmapLayers = layersInWebMap.Where(x => x.Level == level && x.Parent == parent);
      int indexSkip = 0;
      foreach (var webmapLayer in webmapLayers)
      {
        if (webmapLayer.IsGroupLayer)
        {
          var newSubLayers = MakeIndicesLayersEqual(layersInTemplate, layersInWebMap, webmapLayer.Level + 1, webmapLayer.Id);
          webmapLayer.SubLayers = newSubLayers.ToList();
        }
        // 23-06-2023: op verzoek van Mark uitgezet
        //        var templateLayer = templateLayers.FirstOrDefault(x => x.Id.Equals(webmapLayer.Id));
        //        webmapLayer.NewIndex = templateLayer?.Index ?? webmapLayer.Index;
      }

      return webmapLayers;
      

      //var newList = new List<OperationalLayer>();
      //foreach (var webmapLayer in webmapLayers.OrderBy(x => x.NewIndex))
      //{
      //  var templateLayer = templateLayers.FirstOrDefault(x => x.Id.Equals(webmapLayer.Id));
      //  if (templateLayer == null)
      //  {
      //    if(newList.Count >= webmapLayer.Index)
      //    {
      //      newList.Insert(webmapLayer.Index, webmapLayer);
      //    }
      //    else
      //    {
      //      newList.Add(webmapLayer);
      //    }
      //  }
      //  else
      //  {
      //    newList.Add(webmapLayer);
      //  }
      //}
      //return newList;
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
        operationalLayer.Title = layer["title"].ToString();
        result.Add(operationalLayer);
        index++;
        if (operationalLayer.IsGroupLayer)
        {
          RetrieveLayers(operationalLayer.JsonDefinition, "layers", result, currentLevel, operationalLayer.Id);
        }
      }
    }

  }
}
