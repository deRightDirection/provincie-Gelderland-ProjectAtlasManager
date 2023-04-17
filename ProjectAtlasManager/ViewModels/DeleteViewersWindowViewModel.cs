using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.ViewModels
{
  class DeleteViewersWindowViewModel : PropertyChangedBase
  {
    public List<string> ItemIds { get; set; }
  }
}
