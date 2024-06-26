using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProjectAtlasManager.Web;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ProjectAtlasManager.ViewModels
{
  class SyncViewersFromTemplateViewModel : PropertyChangedBase
  {
    private IEnumerable<PortalItem> _viewers;
    private ObservableCollection<PortalItem> _viewersToSynchronize;

    public SyncViewersFromTemplateViewModel()
    {
      _viewers = new List<PortalItem>();
      _viewersToSynchronize = new ObservableCollection<PortalItem>();
      FillList();
    }

    private async void FillList()
    {
      var portal = ArcGISPortalManager.Current.GetActivePortal();
      var portalInfo = await portal.GetPortalInfoAsync();
      var orgId = portalInfo.OrganizationId;
      var query = new PortalQueryParameters("id:" + Module1.SelectedProjectTemplate)
      {
        Limit = 100
      };
      var results = await portal.SearchForContentAsync(query);
      var item = results.Results.FirstOrDefault();
      if (item == null)
      {
        return;
      }
      var portalClient = await QueuedTask.Run(() => new PortalClient(item.PortalUri, portal.GetToken()));
      await portalClient.UpdateTemplate(item, true);
      var mapsBasedOnTemplateQuery = new PortalQueryParameters($"type:\"Web Map\" AND tags:\"ProjectAtlas\" AND tags:\"CopyOfTemplate\" AND tags:\"PAT{item.ID}\" AND orgid:{orgId}")
      {
        Limit = 100
      };
      var mapsBasedOnTemplate = await portal.SearchForContentAsync(mapsBasedOnTemplateQuery);
      Viewers = mapsBasedOnTemplate.Results.OrderBy(x => x.Title);
      CmdOk.RaiseCanExecuteChanged();
    }

    public IEnumerable<PortalItem> Viewers
    {
      get => _viewers;
      set => SetProperty(ref _viewers, value);
    }

    public ObservableCollection<PortalItem> ViewersToSynchronize
    {
      get => _viewersToSynchronize;
      set => SetProperty(ref _viewersToSynchronize, value);
    }

    public List<string> ItemIds {get;set;}
    #region Commands

    public RelayCommand CmdOk => new RelayCommand((proWindow) =>
    {
      ItemIds = ViewersToSynchronize?.Select(x => x.ID).ToList();
      (proWindow as ProWindow).DialogResult = true;
      (proWindow as ProWindow).Close();
    }, () => Viewers.Any());

    public ICommand CmdCancel => new RelayCommand((proWindow) =>
    {
      (proWindow as ProWindow).DialogResult = false;
      (proWindow as ProWindow).Close();
    }, () => true);

    #endregion
  }
}
