using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ProjectAtlasManager.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectAtlasManager.ViewModels
{
  class DeleteViewersWindowViewModel : PropertyChangedBase
  {
    private IEnumerable<PortalItem> _viewers;
    private ObservableCollection<PortalItem> _viewersToDelete;
    private bool _removeAsItem;

    public DeleteViewersWindowViewModel()
    {
      EventSender.Subscribe(RenewData, true);
      _viewers = new List<PortalItem>();
      _viewersToDelete = new ObservableCollection<PortalItem>();
      FillList();
    }

    private void RenewData(UpdateGalleryEvent eventData)
    {
      if (eventData.UpdateViewersGallery)
      {
        FillList();
      }
    }


    private async void FillList()
    {
      ArcGISPortal portal = ArcGISPortalManager.Current.GetActivePortal();
      var portalInfo = await portal.GetPortalInfoAsync();
      var orgId = portalInfo.OrganizationId;
      var query = new PortalQueryParameters("id:" + Module1.SelectedProjectTemplate);
      query.Limit = 100;
      var results = await ArcGISPortalExtensions.SearchForContentAsync(portal, query);
      var item = results.Results.FirstOrDefault();
      if (item == null)
      {
        return;
      }
      var viewersBasedOnTemplateQuery = new PortalQueryParameters($"type:\"Web Map\" AND tags:\"ProjectAtlas\" AND tags:\"CopyOfTemplate\" AND tags:\"PAT{item.ID}\" AND orgid:{orgId}")
      {
        Limit = 100
      };
      var mapsBasedOnTemplate = await ArcGISPortalExtensions.SearchForContentAsync(portal, viewersBasedOnTemplateQuery);
      Viewers = mapsBasedOnTemplate.Results.OrderBy(x => x.Title);
      CmdOk.RaiseCanExecuteChanged();
    }

    public bool RemoveAsItem
    {
      get => _removeAsItem;
      set => SetProperty(ref _removeAsItem, value);
    }

    public IEnumerable<PortalItem> Viewers
    {
      get => _viewers;
      set => SetProperty(ref _viewers, value);
    }

    public ObservableCollection<PortalItem> ViewersToDelete
    {
      get => _viewersToDelete;
      set => SetProperty(ref _viewersToDelete, value);
    }

    public List<string> ItemIds { get; set; }
    #region Commands

    public RelayCommand CmdOk => new RelayCommand((proWindow) =>
    {
      ItemIds = ViewersToDelete?.Select(x => x.ID).ToList();
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
