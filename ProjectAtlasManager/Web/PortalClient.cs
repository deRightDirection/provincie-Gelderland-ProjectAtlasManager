using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectAtlasManager.Domain;
using ProjectAtlasManager.Services;
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
    private readonly EsriHttpClient _http;
    public PortalClient(Uri portalUri, string token)
    {
      _portalUri = portalUri;
      _token = token;
      _http = new EsriHttpClient();
    }

    internal Task UpdateTags(PortalItem item, string tags)
    {
      var uri = $"{_portalUri}sharing/rest/content/users/{item.Owner}/{item.FolderID}/items/{item.ItemID}/update?f=json&token=" + _token;
      var formContent = new MultipartFormDataContent
      {
        { new StringContent(tags), "tags" }
      };
      if (string.IsNullOrEmpty(tags))
      {
        formContent.Add(new StringContent(true.ToString()), "clearEmptyFields");
      }
      return _http.PostAsync(uri, formContent);
    }
    internal async Task<string> GetDataFromItem(PortalItem item)
    {
      var uri = $"{_portalUri}sharing/rest/content/items/{item.ID}/data?f=json&token={_token}";
      var response = await _http.GetAsync(uri).ConfigureAwait(false);
      return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
    internal Task Delete(PortalItem item)
    {
      var uri = $"{_portalUri}sharing/rest/content/users/{item.Owner}/items/{item.ID}/delete?f=json&token={_token}";
      return uri.PostAsync();
    }
    internal async Task CreateViewerFromTemplate(PortalItem item, string title, string tags)
    {
      var uri = $"{_portalUri}sharing/rest/content/users/{item.Owner}/addItem?f=json&token={_token}";
      var data = await GetDataFromItem(item).ConfigureAwait(false);
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
      await _http.PostAsync(uri, formContent).ConfigureAwait(false);
    }

    internal Task<EsriHttpResponseMessage> UpdateData(PortalItem webmap, string webmapData)
    {
      var uri = $"{_portalUri}sharing/rest/content/users/{webmap.Owner}/{webmap.FolderID}/items/{webmap.ID}/update?f=json&token={_token}";
      var formContent = new MultipartFormDataContent
      {
        { new StringContent(webmapData), "text" }
      };
      return _http.PostAsync(uri, formContent);
    }

    public async Task UpdateTemplate(PortalItem item, bool addPATToLayerId)
    {
      var data = await GetDataFromItem(item).ConfigureAwait(false);
      var layers = data.RetrieveLayers();
      foreach(var layer in layers)
      {
        var newId = "";
        if (!layer.Id.StartsWith("PAT_") && addPATToLayerId)
        {
          newId = $"PAT_{layer.Id}";
        }
        if (layer.Id.StartsWith("PAT_") && !addPATToLayerId)
        {
          newId = layer.Id.Replace("PAT_", string.Empty);
        }
        if(!string.IsNullOrEmpty(newId))
        {
          data = data.Replace(layer.Id, newId);
        }
      }
      await UpdateData(item, data).ConfigureAwait(false);
    }
  }
}
