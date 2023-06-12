using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ProjectAtlasManager.Domain
{
  public class OperationalLayer
  {
    [JsonIgnore]
    public int Index { get; set; }
    public string Id { get; set; }
    public JObject JsonDefinition { get; set; }
    public string LayerType { get; set; }
    public int Level { get; set; }
    public string Parent { get; set; }
    [JsonIgnore]
    public int NewIndex { get; set; }
    [JsonIgnore]
    public List<OperationalLayer> SubLayers { get; set; }
    [JsonIgnore]
    public bool IsPATLayer => Id.StartsWith("PAT_");
    public string Title { get; set; }
    [JsonIgnore]
    public bool IsGroupLayer => LayerType.Equals("GroupLayer");
  }
}
