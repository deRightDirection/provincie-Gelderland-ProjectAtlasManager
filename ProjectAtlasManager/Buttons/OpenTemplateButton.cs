using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Buttons
{
  class OpenTemplateButton : Button
  {
    protected override async void OnClick()
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      var query = PortalQueryParameters.CreateForItemsWithId(Module1.SelectedProjectTemplate);
      PortalQueryResultSet<PortalItem> results = await portal.SearchForContentAsync(query);
      var result = results.Results.FirstOrDefault();
      if(result == null)
      {
        return;
      }
      QueuedTask.Run(() =>
      {
        if (MapFactory.Instance.CanCreateMapFrom(result))
        {
          var newMap = MapFactory.Instance.CreateMapFromItem(result);
          ProApp.Panes.CreateMapPaneAsync(newMap);
        }
      });
    }
  }
}
