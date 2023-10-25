using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Services;

namespace UnitTests
{
  [TestClass]
  public class MapExtensionTest
  {
    [TestMethod]
    public void SetSummary()
    {
      var xml = File.ReadAllText("metadata.xml");
      var result = xml.UpdateSummary("testmethod");
      result.Should().Contain("testmethod");
    }
  }
}
