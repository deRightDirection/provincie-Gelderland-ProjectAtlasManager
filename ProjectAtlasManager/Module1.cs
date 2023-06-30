using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

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
    internal static string SelectedProjectTemplateName { get; set; }
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
