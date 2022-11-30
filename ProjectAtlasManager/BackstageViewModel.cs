using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager
{
  class BackstageViewModel : BackstageTab
  {
    protected override async Task InitializeAsync()
    {
      await InitializeAsync();
    }

    protected override Task UninitializeAsync()
    {
      return UninitializeAsync();
    }

    private Uri _portalUri;
    public Uri PortalUri
    {
      get { return _portalUri; }
      set { SetProperty(ref _portalUri, value, () => PortalUri); }
    }
  }
}
