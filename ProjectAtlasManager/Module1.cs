using ArcGIS.Desktop.Framework;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Module = ArcGIS.Desktop.Framework.Contracts.Module;

namespace ProjectAtlasManager
{
  internal class Module1 : Module
  {
    private static Module1 _this = null;
    private static string ModuleID = "ProjectAtlasManager_Module";
    internal static readonly object _lock = new object();

    protected override bool Initialize()
    {
      var logLocation = GetAddinFolder();
      var filePath = Path.Combine(logLocation, "projectatlasmanager-log-.txt");
      Log.Logger = new LoggerConfiguration()
        .WriteTo.File(filePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5)
        .MinimumLevel.Verbose()
        .CreateLogger();
      Log.Logger.Information("addin gestart");
      return base.Initialize();
    }

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

    public string GetAddinFolder()
    {
      var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      var arcGisProLocation = Path.Combine(myDocs, "ArcGIS", "AddIns", "ArcGISPro");
      var attribute = (GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0];
      var proAddinFolder = $"{{{attribute.Value}}}";
      var addinFolder = Path.Combine(arcGisProLocation, proAddinFolder);
      return addinFolder;
    }

    protected override bool CanUnload()
    {
      return true;
    }
  }
}
