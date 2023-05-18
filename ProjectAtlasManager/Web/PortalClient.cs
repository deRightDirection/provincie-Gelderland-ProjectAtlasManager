using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectAtlasManager.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

    internal Task UpdateTags(PortalItem item, string tags)
    {
      var uri = $"{_portalUri}sharing/rest/content/users/{item.Owner}/{item.FolderID}/items/{item.ItemID}/update?f=json&token=" + _token;
      // TODO thumbnail toevoegen indien die nog niet aanwezig is
      // TODO opzoeken van item die de template informatie bevat
      var formContent = new MultipartFormDataContent();
      formContent.Add(new StringContent(tags), "tags");
      if (string.IsNullOrEmpty(tags))
      {
        formContent.Add(new StringContent(true.ToString()), "clearEmptyFields");
      }
      return _http.PostAsync(uri, formContent);
    }

    internal async Task<string> GetDataFromItem(PortalItem item)
    {
      var uri = $"{_portalUri}sharing/rest/content/items/{item.ID}/data?f=json&token={_token}";
      var response = await _http.GetAsync(uri);
      return await response.Content.ReadAsStringAsync();
    }

    internal async Task Delete(PortalItem item)
    {
      var uri = $"{_portalUri}sharing/rest/content/users/{item.Owner}/items/{item.ID}/delete?f=json&token={_token}";
      await uri.PostAsync();
    }
    internal async Task CreateViewerFromTemplate(PortalItem item, string title, string tags)
    {
      var uri = $"{_portalUri}sharing/rest/content/users/{item.Owner}/addItem?f=json&token={_token}";
      var data = await GetDataFromItem(item);
      var formContent = new MultipartFormDataContent
      {
        { new StringContent(tags), "tags" },
        { new StringContent(title), "title" },
        { new StringContent(data), "text" },
        { new StringContent(item.Type), "type" },
        { new StringContent(item.TypeKeywords), "typeKeywords" },
        { new StringContent(string.IsNullOrEmpty(item.Description) ? string.Empty : item.Description), "description" },
        { new StringContent(string.IsNullOrEmpty(item.Summary) ? string.Empty : item.Summary), "snippet" }
      };
      var extent = $"{item.XMin},{item.YMin},{item.XMax},{item.YMax}";
      formContent.Add(new StringContent(extent), "extent");
      var json = JsonConvert.SerializeObject(item.ItemCategories);
      formContent.Add(new StringContent(json), "categories");
      await _http.PostAsync(uri, formContent);
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
