using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ProjectAtlasManager
{
  internal class NewTemplateGallery : Gallery
  {
    private bool _isInitialized;

    protected override void OnDropDownOpened()
    {
      Initialize();
    }

    private void Initialize()
    {
      if (_isInitialized)
        return;

      for (int i = 0; i < 30; i++)
      {
        string name = string.Format("Item {0}", i);
        Add(new GalleryItem(name, this.LargeImage != null ? ((ImageSource)this.LargeImage).Clone() : null, name));
      }
      _isInitialized = true;

    }

    protected override void OnClick(GalleryItem item)
    {
      Module1.SelectedWebMapToUpgradeToTemplate = item;
      // TODO set state
    }
  }
}
