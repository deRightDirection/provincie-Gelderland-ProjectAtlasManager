using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
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
    // template heeft een laag aangevinkt en een extra laag
    // na sync is de viewer helemaal verprutst, niet gereproduceerd in deze sync
    public void Issue_1_17_1()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas29.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template29.json"));
      var xx = JToken.Parse(projectTemplateJson);
      var xx2 = JToken.Parse(webmap);
      xx["operationalLayers"].Children().Count().Should().Be(13);
      xx2["operationalLayers"].Children().Count().Should().Be(12);
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      x["operationalLayers"].Children().Count().Should().Be(13);
      x2["operationalLayers"].Children().Count().Should().Be(13);
    }

    [TestMethod]
    public void WithoutOrdering()
    {
      Assert.Inconclusive("waarschijnlijk niet meer goed omdat re-ordering is uitgezet");
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas28.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template28.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }


    [TestMethod]
    // in template zit een laag en een groepslaag met visibility op true, in template zitten zelfde lagen
    // met visibility = false, na sync moeten die ook op visibility = true staan
    public void VisibilityOfLayerAndGroupLayer()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas27.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template27.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }

    [TestMethod]
    // in template zitten drie lagen minder dan in viewer en die worden verwijderd uit viewer
    // hierna is viewer op 1 laag na gelijk aan template
    public void RemoveLayersFromTemplate2()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas26.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template26.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsFalse(JToken.DeepEquals(x, x2));
    }

    [TestMethod]
    // in template zitten drie lagen minder dan in viewer en die worden verwijderd uit viewer
    // hierna is viewer gelijk aan template
    public void RemoveLayersFromTemplate()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas25.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template25.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }

    [TestMethod]
    // template heeft alle grouplayers een ander id dan die in de viewer
    public void SynchronizeAllLevelsOfWebMap_With_Changing_Id()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas24.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template24.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }
    [TestMethod]
    // template heeft voor de grouplayer een ander id dan die in de viewer
    public void GroupLayer_With_Changing_Id()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas23.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template23.json"));
      var jsonTemplate = JToken.Parse(projectTemplateJson);
      var jsonOldWebmap = JToken.Parse(webmap);
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var jsonNewWebmap = JToken.Parse(newWebMap);
      Assert.IsFalse(JToken.DeepEquals(jsonTemplate, jsonOldWebmap));
      Assert.IsTrue(JToken.DeepEquals(jsonTemplate, jsonNewWebmap));
    }

    [TestMethod]
    // template en viewer hebben exact dezelfde lagen en geneste groepslagen
    // op alle niveuas zijn er lagen verwisseld
    public void Reorder5()
    {
      Assert.Inconclusive("waarschijnlijk niet meer goed omdat re-ordering is uitgezet");
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas22.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template22.json"));
      var jsonTemplate = JToken.Parse(projectTemplateJson);
      var jsonOldWebmap = JToken.Parse(webmap);
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var jsonNewWebmap = JToken.Parse(newWebMap);
      Assert.IsFalse(JToken.DeepEquals(jsonTemplate, jsonOldWebmap));
      Assert.IsTrue(JToken.DeepEquals(jsonTemplate, jsonNewWebmap));
    }

    [TestMethod]
    // template en viewer hebben exact dezelfde lagen maar viewer heeft in groepslaag 1 extra laag
    // waarbij die extra laag in het midden van de 2 andere lagen zitten in de groepslaag die in de template verwisseld zijn
    // daarnaast zijn de normale laag en groepslaag ook verwisseld
    public void Reorder4()
    {
      Assert.Inconclusive("waarschijnlijk niet meer goed omdat re-ordering is uitgezet");
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas21.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template21.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var jsonTemplate = JToken.Parse(projectTemplateJson);
      var jsonOldWebmap = JToken.Parse(webmap);
      var jsonNewWebmap = JToken.Parse(newWebMap);
      var firstOperationLayer = jsonTemplate["operationalLayers"].First();
      var lastOperationLayer = jsonTemplate["operationalLayers"].Last();
      var oldFirstOperationLayer = jsonOldWebmap["operationalLayers"].First();
      var newFirstOperationLayer = jsonNewWebmap["operationalLayers"].First();
      var newLastOperationLayer = jsonNewWebmap["operationalLayers"].Last();
      firstOperationLayer["id"].ToString().Should().Be(jsonOldWebmap["operationalLayers"].Last["id"].ToString());
      firstOperationLayer["id"].ToString().Should().Be(newFirstOperationLayer["id"].ToString());
      jsonTemplate["operationalLayers"].Last["id"].ToString().Should().Be(oldFirstOperationLayer["id"].ToString());
      jsonTemplate["operationalLayers"].Last["id"].ToString().Should().Be(jsonNewWebmap["operationalLayers"].Last["id"].ToString());
      lastOperationLayer["layers"].First["id"].ToString().Should().Be(oldFirstOperationLayer["layers"].Last["id"].ToString());
      lastOperationLayer["layers"].First["id"].ToString().Should().Be(newLastOperationLayer["layers"].First["id"].ToString());
    }

    [TestMethod]
    // template en viewer hebben exact dezelfde lagen maar viewer heeft er 1 extra
    // waarbij die extra laag in het midden van de 2 andere lagen zitten die in de template verwisseld zijn
    public void Reorder3()
    {
      Assert.Inconclusive("waarschijnlijk niet meer goed omdat re-ordering is uitgezet");
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas20.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template20.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var jsonTemplate = JToken.Parse(projectTemplateJson);
      var jsonOldWebmap = JToken.Parse(webmap);
      var jsonNewWebmap = JToken.Parse(newWebMap);
      jsonTemplate["operationalLayers"].First["id"].ToString().Should().Be(jsonOldWebmap["operationalLayers"].Last["id"].ToString());
      jsonTemplate["operationalLayers"].First["id"].ToString().Should().Be(jsonNewWebmap["operationalLayers"].First["id"].ToString());
      jsonTemplate["operationalLayers"].Last["id"].ToString().Should().Be(jsonOldWebmap["operationalLayers"].First["id"].ToString());
      jsonTemplate["operationalLayers"].Last["id"].ToString().Should().Be(jsonNewWebmap["operationalLayers"].Last["id"].ToString());
    }
    [TestMethod]
    // template en viewer hebben exact dezelfde lagen maar viewer heeft er 1 extra
    // maar in een andere volgorde
    public void Reorder2()
    {
      Assert.Inconclusive("waarschijnlijk niet meer goed omdat re-ordering is uitgezet");
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas19.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template19.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var jsonTemplate = JToken.Parse(projectTemplateJson);
      var jsonOldWebmap = JToken.Parse(webmap);
      var jsonNewWebmap = JToken.Parse(newWebMap);
      jsonTemplate["operationalLayers"].First["id"].ToString().Should().NotBe(jsonOldWebmap["operationalLayers"].First["id"].ToString());
      jsonTemplate["operationalLayers"].First["id"].ToString().Should().Be(jsonNewWebmap["operationalLayers"].First["id"].ToString());
    }

    [TestMethod]
    // template en viewer hebben exact dezelfde lagen
    // maar in een andere volgorde
    public void Reorder()
    {
      Assert.Inconclusive("waarschijnlijk niet meer goed omdat re-ordering is uitgezet");
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas18.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template18.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var jsonTemplate = JToken.Parse(projectTemplateJson);
      var jsonNewWebmap = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(jsonTemplate, jsonNewWebmap));
    }

    [TestMethod]
    public void SyncTemplate()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas17.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template17.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      newWebMap.Should().Contain("Ziekenhuizen en buitenpoliklinieken gdb - Ziekenhuizen");
    }
    [TestMethod]
    public void ReplaceExistingLayer()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas16.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template16.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }
    [TestMethod]
    public void AddAllLayers()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas15.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template15.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }
    [TestMethod]
    public void SetLayerIndices()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "layerindices.json"));
      var layers = _synchronizer.RetrieveLayers(projectTemplateJson);
      layers.Count(x => x.Level == 1).Should().Be(3);
      layers.Count(x => x.Level == 0).Should().Be(3);
    }
    [TestMethod]
    public void SynchronizeAllLevelsOfWebMap3()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas14.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template14.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      newWebMap.Should().Contain("1882d4dce67-layer-12");
      newWebMap.Should().Contain("1899d4dce67-layer-14");
      var x2 = JToken.Parse(newWebMap);
      Assert.IsFalse(JToken.DeepEquals(x, x2));
    }
    [TestMethod]
    public void SynchronizeAllLevelsOfWebMap2()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas13.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template13.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      newWebMap.Should().Contain("1882d4dce67-layer-14");
      newWebMap.Should().Contain("1882d4dce67-layer-15");
      var x2 = JToken.Parse(newWebMap);
      Assert.IsFalse(JToken.DeepEquals(x, x2));
    }
    [TestMethod]
    public void SynchronizeAllLevelsOfWebMap()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas12.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template12.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }
    [TestMethod]
    public void SynchronizeLowerLevelOfWebMap2()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas11.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template11.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }
    [TestMethod]
    public void SynchronizeLowerLevelOfWebMap()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas10.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template10.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }
    [TestMethod]
    public void SynchronizeLowerGroupsLevelOfWebMap()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas9.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template9.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }
    [TestMethod]
    public void SynchronizeTopLevelOfWebMap()
    {
      var projectTemplateJson = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas.json"));
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
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
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
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
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
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
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
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
      var layers = _synchronizer.RetrieveLayers(projectTemplateJson);
      layers.Count(xx => xx.Level == 0).Should().Be(3);
      var webmap = File.ReadAllText(Path.Combine(_testdataFolder, "projectatlas copy van template5.json"));
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
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
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
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
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
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
      var newWebMap = _synchronizer.Synchronize(webmap, projectTemplateJson);
      var x = JToken.Parse(projectTemplateJson);
      var x2 = JToken.Parse(newWebMap);
      Assert.IsTrue(JToken.DeepEquals(x, x2));
    }
  }
}
