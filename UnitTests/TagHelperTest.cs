using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Services;

namespace UnitTests
{
  [TestClass]
  public class TagHelperTest
  {
    [TestMethod]
    public void RemoveTags()
    {
      var tags = new List<string> {"Mannus", "template", "Bram", "copyoftemplate", "Mark", "ProjectAtLas", "PaT_123456789010"};
      var newTags = UnitTestTagsHelper.UpdateTags(tags);
      newTags.Should().NotContain("template");
      newTags.Should().NotContain("PaT");
    }
  }
}
