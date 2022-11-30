using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ProjectAtlasManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager
{
  class ShowTemplates : Button
  {
    private ShowTemplatesView _showTemplates = null;
    private const string _disabledTooltip = "niet ingelogd in het ArcGIS Enterprise-portaal van de provincie Gelderland";
    public ShowTemplates()
    {
      Enabled = false;
      DisabledTooltip = _disabledTooltip;
    }

    protected override void OnUpdate()
    {
      var portal = ArcGISPortalManager.Current.GetActivePortal();
      if (portal.IsSignedOn())
      {
        Enabled = portal.PortalUri.Equals(new Uri("https://www.therightdirectionserver.nl/portal"));
      }
    }

  protected override void OnClick()
    {
      //already open?
      if (_showTemplates != null)
        return;
      _showTemplates = new ShowTemplatesView();
      _showTemplates.Owner = FrameworkApplication.Current.MainWindow;
      _showTemplates.Closed += (o, e) => { _showTemplates = null; };
      _showTemplates.ShowDialog();
    }

  }
}
