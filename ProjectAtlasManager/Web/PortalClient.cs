using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectAtlasManager.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Web
{
  class PortalClient
  {
    private readonly Uri _portalUri;
    private readonly string _token;
    private EsriHttpClient _http;
    public PortalClient(Uri portalUri, string token)
    {
      _portalUri = portalUri;
      _token = token;
      _http = new EsriHttpClient();
    }

    internal Task AddTags(PortalItem item, string tags)
    {
      var uri = $"{_portalUri}sharing/rest/content/users/{item.Owner}/{item.FolderID}/items/{item.ItemID}/update?f=json&token=" + _token;
      // TODO thumbnail toevoegen indien die nog niet aanwezig is
      // TODO opzoeken van item die de template informatie bevat
      var formContent = new MultipartFormDataContent();
      formContent.Add(new StringContent(tags), "tags");
      return _http.PostAsync(uri, formContent);
    }

    internal async Task<string> GetDataFromItem(PortalItem item)
    {
      var uri = $"{_portalUri}sharing/rest/content/items/{item.ID}/data?f=json&token={_token}";
      var response = await _http.GetAsync(uri);
      var json = await response.Content.ReadAsStringAsync();
      return json;
    }

    internal async Task<IEnumerable<OperationalLayer>> RetrieveLayers(PortalItem item)
    {
      var json = await GetDataFromItem(item);
      var webmap = JToken.Parse(json);
      var operationalLayers = webmap["operationalLayers"];
      var result = new List<OperationalLayer>();
      foreach(var layer in operationalLayers)
      {
        var operationalLayer = new OperationalLayer();
        operationalLayer.Id = layer["id"].ToString();
        operationalLayer.JsonDefinition = layer as JObject;
        result.Add(operationalLayer);
      }
      return result;
    }

    internal Task<EsriHttpResponseMessage> UpdateData(PortalItem webmap, string webmapData)
    {
      var uri = $"{_portalUri}sharing/rest/content/users/{webmap.Owner}/{webmap.FolderID}/items/{webmap.ID}/update?token={_token}";
      var formContent = new MultipartFormDataContent();
      formContent.Add(new StringContent(webmapData), "text");
      return _http.PostAsync(uri, formContent);
    }
  }
}
