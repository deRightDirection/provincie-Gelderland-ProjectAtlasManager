using FluentAssertions;
using UnitTests.Services;
using System.Reflection;

namespace UnitTests
{
  [TestClass]
  public class MapExtensionTest
  {
    private string _testdataFolder;
    [TestInitialize]
    public void Setup()
    {
      var path = Assembly.GetExecutingAssembly().Location;
      _testdataFolder = path.Replace("\\Tests\\bin\\Debug\\net8.0\\Tests.dll", "\\testdata");
    }
    [TestMethod]
    public void SetSummary()
    {
      var xml = File.ReadAllText(Path.Combine(_testdataFolder, "metadata.xml"));
      var result = xml.UpdateSummary("testmethod");
      result.Should().Contain("testmethod");
    }
  }
}
