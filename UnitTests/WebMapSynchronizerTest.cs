using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using UnitTests.Domain;
using UnitTests.Templates;

namespace UnitTests
{
  [TestClass]
  public class WebMapSynchronizerTest
  {
    private UnitTestWebMapManager _synchronizer;
    private string _testdataFolder;

    [TestInitialize]
    public void Setup()
    {
      _synchronizer = new UnitTestWebMapManager();
      var path = Assembly.GetExecutingAssembly().Location;
      _testdataFolder = path.Replace("\\UnitTests\\bin\\Debug\\UnitTests.dll", "\\testdata");
    }

    [TestMethod]
    public void SynchronizeTopLevelOfWebMap()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template.json"));
      var operationalLayers = RetrieveLayers(projectTemplateJson);
      var newWebMap = _synchronizer.Synchronize(webmap, operationalLayers);
      var x = JToken.Parse(webmap);
      var x2  = JToken.Parse(newWebMap);
      Assert.IsFalse(JToken.DeepEquals(x,x2));
    }

    [TestMethod]
    // projectatlas heeft 3-lagen
    // copy heeft er 2 waarbij de middelste ontbreekt
    // na synchronize is in het midden de nieuwe toegevoegd
    public void SynchronizeTopLevelOfWebMap2()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas2.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template2.json"));
      var operationalLayers = RetrieveLayers(projectTemplateJson);
      var newWebMap = _synchronizer.Synchronize(webmap, operationalLayers);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }

    [TestMethod]
    // projectatlas heeft 3-lagen
    // copy heeft er 2 waarbij de eerste ontbreekt
    // na synchronize is aan het begin de nieuwe toegevoegd
    public void SynchronizeTopLevelOfWebMap3()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas3.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template3.json"));
      var operationalLayers = RetrieveLayers(projectTemplateJson);
      var newWebMap = _synchronizer.Synchronize(webmap, operationalLayers);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }

    [TestMethod]
    // projectatlas heeft 3-lagen
    // copy heeft er 2 waarbij de laatste ontbreekt
    // na synchronize is de laatste toegevoegd
    public void SynchronizeTopLevelOfWebMap4()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas4.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template4.json"));
      var operationalLayers = RetrieveLayers(projectTemplateJson);
      var newWebMap = _synchronizer.Synchronize(webmap, operationalLayers);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }

    [TestMethod]
    // projectatlas heeft 3-groepslagen
    // copy heeft er 2 waarbij de middelste ontbreekt
    // na synchronize is de middelste toegevoegd
    public void SynchronizeTopLevelOfWebMap5()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas5.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template5.json"));
      var operationalLayers = RetrieveLayers(projectTemplateJson);
      var newWebMap = _synchronizer.Synchronize(webmap, operationalLayers);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }

    [TestMethod]
    // projectatlas heeft 3-groepslagen
    // in groep 2 ontbreekt de eerste laag
    // na synchronize is dit toegevoegd
    public void SynchronizeGrouplayerOfWebMap1()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas6.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template6.json"));
      var operationalLayers = RetrieveLayers(projectTemplateJson);
      var newWebMap = _synchronizer.Synchronize(webmap, operationalLayers);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }

    [TestMethod]
    // projectatlas heeft 3-groepslagen
    // in groep 2 ontbreekt de middelste laag
    // na synchronize is dit toegevoegd
    public void SynchronizeGrouplayerOfWebMap2()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas7.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template7.json"));
      var operationalLayers = RetrieveLayers(projectTemplateJson);
      var newWebMap = _synchronizer.Synchronize(webmap, operationalLayers);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }

    [TestMethod]
    // projectatlas heeft 3-groepslagen
    // in groep 2 ontbreekt de laatste laag
    // na synchronize is dit toegevoegd
    public void SynchronizeGrouplayerOfWebMap3()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas8.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template8.json"));
      var operationalLayers = RetrieveLayers(projectTemplateJson);
      var newWebMap = _synchronizer.Synchronize(webmap, operationalLayers);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }

    private IEnumerable<OperationalLayer> RetrieveLayers(string json)
    {
      var webmap = JToken.Parse(json);
      var operationalLayers = webmap["operationalLayers"];
      var result = new List<OperationalLayer>();
      foreach (var layer in operationalLayers)
      {
        var operationalLayer = new OperationalLayer();
        operationalLayer.Id = layer["id"].ToString();
        operationalLayer.JsonDefinition = layer as JObject;
        result.Add(operationalLayer);
      }
      return result;
    }
  }
}
