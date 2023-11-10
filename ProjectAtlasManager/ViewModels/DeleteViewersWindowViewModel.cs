using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ProjectAtlasManager.Domain;
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
    private IEnumerable<WebMapItem> _viewers;
    private ObservableCollection<WebMapItem> _viewersToDelete;
    private bool _removeAsItem;

    public DeleteViewersWindowViewModel()
    {
      _viewers = new List<WebMapItem>();
      _viewersToDelete = new ObservableCollection<WebMapItem>();
    }

    public bool RemoveAsItem
    {
      get => _removeAsItem;
      set => SetProperty(ref _removeAsItem, value);
    }

    public IEnumerable<WebMapItem> Viewers
    {
      get => _viewers;
      set => SetProperty(ref _viewers, value);
    }

    public ObservableCollection<WebMapItem> ViewersToDelete
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

    internal void SetViewers(ObservableCollection<WebMapItem> viewers)
    {
      Viewers = viewers.ToList();
    }
    #endregion
  }
}
