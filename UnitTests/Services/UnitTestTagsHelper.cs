using System.Collections.Generic;

namespace UnitTests.Services
{
  internal class UnitTestTagsHelper
  {
    internal static string UpdateTags(IReadOnlyList<string> tags)
    {
      var tagsToRemove = new List<string>() { "projectatlas", "copyoftemplate", "template" };

      var newSetOfTags = new List<string>();
      foreach (var tag in tags)
      {
        var tagValue = tag.ToLowerInvariant().Trim();
        if (tagsToRemove.Contains(tagValue) || (tagValue.StartsWith("pat_") && tagValue.Length > 10))
        {
          continue;
        }
        newSetOfTags.Add(tag.Trim());
      }
      return string.Join(",", newSetOfTags);
    }

    internal static string ParseTags(IReadOnlyList<string> tags)
    {
      var newSetOfTags = new List<string>();
      foreach (var tag in tags)
      {
        newSetOfTags.Add(tag.Trim());
      }
      return string.Join(",", newSetOfTags);
    }
  }
}
