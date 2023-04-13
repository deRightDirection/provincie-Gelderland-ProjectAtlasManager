using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ProjectAtlasManager.Domain;
using ProjectAtlasManager.Services;

namespace UnitTests
{
  [TestClass]
  public class WebMapSynchronizerTest
  {
    private WebMapSynchronizer _synchronizer;
    private string _testdataFolder;

    [TestInitialize]
    public void Setup()
    {
      _synchronizer = new WebMapSynchronizer();
      var path = Assembly.GetExecutingAssembly().Location;
      _testdataFolder = path.Replace("\\UnitTests\\bin\\Debug\\UnitTests.dll", "\\testdata");
    }

    [TestMethod]
    public void Synchronize()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template.json"));
      var operationalLayers = RetrieveLayers(projectTemplateJson);
      var newWebMap = _synchronizer.Synchronize(webmap, operationalLayers);
      var x = JToken.Parse(webmap);
      var x2  = JToken.Parse(newWebMap);
      Assert.IsFalse(JToken.DeepEquals(x,x2));
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
