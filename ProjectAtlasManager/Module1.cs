using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
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
using System.Windows.Input;

namespace ProjectAtlasManager
{
  internal class Module1 : Module
  {
    private static Module1 _this = null;
    private static string ModuleID = "ProjectAtlasManager_Module";

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule(ModuleID));
      }
    }
    /// <summary>
    /// geselecteerde viewer
    /// </summary>
    public static string SelectedViewer { get; internal set; }
    /// <summary>
    /// het geselecteerde template
    /// </summary>
    internal static string SelectedProjectTemplate { get; set; }
    /// <summary>
    /// de webmap die opgewaardeerd kan worden naar een template
    /// </summary>
    internal static string SelectedWebMapToUpgradeToTemplate { get; set; }

    protected override bool CanUnload()
    {
      return true;
    }
  }
}
