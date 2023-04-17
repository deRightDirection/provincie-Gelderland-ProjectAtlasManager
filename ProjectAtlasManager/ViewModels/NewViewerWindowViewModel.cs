using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectAtlasManager.ViewModels
{
  class NewViewerWindowViewModel : PropertyChangedBase
  {
    private string _name;
    public string Name
    {
      get => _name;
      set
      {
        SetProperty(ref _name, value);
        CmdOk.RaiseCanExecuteChanged();
      }
    }
    public RelayCommand CmdOk => new RelayCommand((proWindow) =>
    {
      (proWindow as ProWindow).DialogResult = true;
      (proWindow as ProWindow).Close();
    }, () => !string.IsNullOrEmpty(Name) && Name.Trim().Length >= 3);

    public ICommand CmdCancel => new RelayCommand((proWindow) =>
    {
      (proWindow as ProWindow).DialogResult = false;
      (proWindow as ProWindow).Close();
    }, () => true);
  }
}
