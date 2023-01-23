using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ProjectAtlasManager.Domain
{
  public class OperationalLayer
  {
    public string Id { get; set; }
    public JObject JsonDefinition { get; set; }
  }
}
