using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTests.Domain
{
  public class OperationalLayer
  {
    [JsonIgnore]
    public int Index { get; set; }
    public string Id { get; set; }
    public JObject JsonDefinition { get; set; }
  }
}
