using ProjectAtlasManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectAtlasManager.Windows
{
  /// <summary>
  /// Interaction logic for NewViewerWindow.xaml
  /// </summary>
  public partial class NewViewerWindow : ArcGIS.Desktop.Framework.Controls.ProWindow
  {
    private readonly NewViewerWindowViewModel VM = new NewViewerWindowViewModel();
    public NewViewerWindow()
    {
      InitializeComponent();
      DataContext = VM;
    }
  }
}
