using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProjectAtlasManager.Domain;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectAtlasManager;

class BackstageViewModel : BackstageTab
{
  private ObservableCollection<PortalInformation> _portals = new ObservableCollection<PortalInformation>();
  private PortalInformation _selectedPortal;
  private string _logFilePath;

  public BackstageViewModel()
  {
    LogFilePath = Module1.Current.GetAddinFolder();
  }
  protected override async Task InitializeAsync()
  {
    await base.InitializeAsync();
    var allPortals = ArcGISPortalManager.Current.GetPortals();
    if (allPortals.Count() != _portals.Count)
    {
      var portals = new ObservableCollection<PortalInformation>();
      foreach (var portal in allPortals)
      {
        var portalData = new PortalInformation();
        portalData.PortalUri = portal.PortalUri;
        portalData.IsSignedOn = await QueuedTask.Run(() => portal.IsSignedOn());
        portalData.IsActive = portal.IsActivePortal();
        var info = await portal.GetPortalInfoAsync();
        portalData.PortalName = info.PortalName;
        portalData.Username = await QueuedTask.Run(() => portal.GetSignOnUsername());
        portals.Add(portalData);
      }
      Portals = portals;
      if (Portals.Count > 0)
      {
        var uriFromSettings = Properties.Settings.Default.PortalUri;
        SelectedPortal = Portals.FirstOrDefault(x => x.PortalUri.Equals(uriFromSettings));
        if (SelectedPortal == null)
        {
          SelectedPortal = Portals.First();
        }
      }
    }
  }

  protected override Task UninitializeAsync()
  {
    return base.UninitializeAsync();
  }

  public PortalInformation SelectedPortal
  {
    get => _selectedPortal;
    set
    {
      SetProperty(ref _selectedPortal, value, () => SelectedPortal);
      if (value != null)
      {
        Properties.Settings.Default.PortalUri = value.PortalUri;
        Properties.Settings.Default.Save();
      }
    }
  }

  public string LogFilePath
  {
    get => _logFilePath;
    set => SetProperty(ref _logFilePath, value, () => LogFilePath);
  }

  public ObservableCollection<PortalInformation> Portals
  {
    get => _portals;
    set => SetProperty(ref _portals, value, () => Portals);
  }
}
