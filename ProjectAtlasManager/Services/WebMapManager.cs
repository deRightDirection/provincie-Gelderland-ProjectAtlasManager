using Newtonsoft.Json.Linq;
using ProjectAtlasManager.Domain;
using System.Collections.Generic;
using System.Linq;

namespace ProjectAtlasManager.Services
{
  class WebMapManager
  {
    private readonly IEnumerable<OperationalLayer> _layersInTemplate;

    public WebMapManager(string templateJson)
    {
      _layersInTemplate = templateJson.RetrieveLayers();
    }
    internal string Synchronize(string webmapData, string templateData)
    {
      var layersInWebMap = webmapData.RetrieveLayers();
      webmapData = MakeGroupLayerIndicesEqual(_layersInTemplate, layersInWebMap, webmapData);
      var levelsInTemplate = _layersInTemplate.Select(x => x.Level).Distinct().OrderBy(x => x);
      var layersToAdd = new List<OperationalLayer>();
      var layersToReplace = new List<OperationalLayer>();
      var layersToRemove = new List<OperationalLayer>();
      DetermineLayersToAddOrReplace(levelsInTemplate, _layersInTemplate, layersInWebMap, layersToAdd, layersToReplace);
      DetermineLayersToRemove(levelsInTemplate, _layersInTemplate, layersInWebMap, layersToRemove);
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
      layersInWebMap = webmapData.RetrieveLayers();
      var newOrder = MakeIndicesLayersEqual(_layersInTemplate, layersInWebMap, 0, null);
      var json = CreateNewOperationalLayerJsonObject(newOrder, new JArray());
      json = DetermineGroupLayersToBeRemoved(templateData, json);
      var webmap = JObject.Parse(webmapData);
      webmap["operationalLayers"] = json;
      return webmap.ToString();
    }


    /// <summary>
    /// Het kan in sommige gevallen voorkomen dat grouplayers in de viewer geen PAT-id krijgen.
    /// In de template is dan de gehele grouplayer verdwenen maar in de viewer blijft dan
    /// een lege grouplayer achter. De lagen in de grouplayer zijn wel verwijderd uit de viewer.
    /// </summary>
    private JArray DetermineGroupLayersToBeRemoved(string templateData, JArray viewerJson)
    {
      var groupLayerFilter = "..*[?(@.layerType == 'GroupLayer')]";
      var groupLayersInViewer = viewerJson.SelectTokens(groupLayerFilter);
      var emptyGroupLayersInViewer = groupLayersInViewer.Where(x => !x["layers"].Children().Any());
      if (emptyGroupLayersInViewer.Any())
      {
        return RemoveGroupLayersWithoutLayersFromViewer(emptyGroupLayersInViewer, templateData, viewerJson);
      }
      return viewerJson;
    }

    /// <summary>
    /// verwijder grouplayers zonder layers uit de viewer
    /// </summary>
    private JArray RemoveGroupLayersWithoutLayersFromViewer(IEnumerable<JToken> emptyGroupLayersInViewer, string templateData, JArray viewerJson)
    {
      var groupLayerFilter = "..*[?(@.layerType == 'GroupLayer')]";
      var template = JObject.Parse(templateData);
      var groupLayersInTemplate = template.SelectTokens(groupLayerFilter);
      var groupLayerTitlesInTemplate = groupLayersInTemplate.Select(x => x["title"].ToString());
      var newViewerJson = viewerJson.DeepClone();
      foreach (var emptyGroupLayer in emptyGroupLayersInViewer)
      {
        var title = emptyGroupLayer["title"].ToString();
        if (!groupLayerTitlesInTemplate.Contains(title))
        {
          var layerToBeRemovedFilter = "..*[?(@.layerType == 'GroupLayer')]";
          var layerToBeRemoved = newViewerJson.SelectTokens(layerToBeRemovedFilter).FirstOrDefault(x => x["id"].Equals(emptyGroupLayer["id"]));
          layerToBeRemoved?.Remove();
        }
      }
      return newViewerJson as JArray;
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
        // het kan zijn dat er een grouplayer is verwijderd, dan zijn de volgende layers uit de lijst van layersToRemove mogelijk null
        webmapLayer?.Remove();
      }
      return webmap.ToString();
    }

    /// <summary>
    /// bepaal welke lagen in de webmap vervangen moeten worden of er aan toegevoegd vanuit het template
    /// </summary>
    private void DetermineLayersToAddOrReplace(IOrderedEnumerable<int> levelsInTemplate, IEnumerable<OperationalLayer> layersInTemplate,
      IEnumerable<OperationalLayer> layersInWebMap, List<OperationalLayer> layersToAdd, ICollection<OperationalLayer> layersToReplace)
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
    private void DetermineLayersToRemove(IEnumerable<int> levelsInTemplate, IEnumerable<OperationalLayer> layersInTemplate,
      IEnumerable<OperationalLayer> layersInWebMap, ICollection<OperationalLayer> layersToRemove)
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
  }
}
