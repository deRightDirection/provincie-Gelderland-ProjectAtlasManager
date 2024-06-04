using FluentAssertions;
using UnitTests.Services;

namespace UnitTests
{
  [TestClass]
  public class TagHelperTest
  {
    [TestMethod]
    public void RemoveTags()
    {
      var tags = new List<string> {"Mannus", "template", "Bram", "copyoftemplate", "Mark", "ProjectAtLas", "PaT12345678901011121314151617181920"};
      var newTags = UnitTestTagsHelper.UpdateTags(tags);
      newTags.Should().NotContain("template");
      newTags.Should().NotContain("PaT");
    }

    [TestMethod]
    public void ParseTags()
    {
      var tags = new List<string> { "Mannus", "template", "Bram", "copyoftemplate", "Mark", "ProjectAtLas", "PaT12345678901011121314151617181920" };
      var newTags = UnitTestTagsHelper.ParseTags(tags);
      newTags.Should().Contain("template");
      newTags.Should().NotContain("PaT");
    }
  }
}
