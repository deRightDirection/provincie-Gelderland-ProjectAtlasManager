using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CommunityToolkit.Mvvm.Input;
using ProjectAtlasManager.Domain;
using ProjectAtlasManager.Events;
using ProjectAtlasManager.Services;
using ProjectAtlasManager.ViewModels;
using ProjectAtlasManager.Web;
using ProjectAtlasManager.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ProjectAtlasManager.Dockpanes
{
  class ViewersDockpaneViewModel : DockPane
  {
    private const string DockPaneId = "viewers_Dockpanel";
    private ObservableCollection<WebMapItem> _viewers;
    private bool _viewerIsSelected;
    private bool _galleryBusy;
    private string _loadingMessage;
    private WebMapItem _selectedViewer;
    private string _template;
    private NewViewerWindow _newviewerwindow;
    private string _name;
    private DeleteViewersWindow _deleteViewersWindow;
    private List<string> _itemIds;
    private bool _removeAsItem;
    private ProgressDialog _progDialog;

    protected override Task InitializeAsync()
    {
      BindingOperations.EnableCollectionSynchronization(Viewers, Module1._lock);
      return base.InitializeAsync();
    }

    internal static void ShowOrHide()
    {
      var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId);
      if (pane == null)
      {
        return;
      }
      pane.Activate();
      pane.IsVisible = true;
    }

    public ViewersDockpaneViewModel()
    {
      _viewers = new ObservableCollection<WebMapItem>();
      EventSender.Subscribe(RenewData, true);
      ActivePortalChangedEvent.Subscribe((args) =>
      {
        lock (Module1._lock)
        {
          _galleryBusy = false;
          _viewers.Clear();
          LoadItemsAsync();
        }
      });
      PortalSignOnChangedEvent.Subscribe((args) =>
      {
        lock (Module1._lock)
        {
          _galleryBusy = false;
          _viewers.Clear();
          LoadItemsAsync();
        }
      });
      NewViewerCommand = new AsyncRelayCommand(NewViewer, CanNewViewer);
      OpenViewerCommand = new AsyncRelayCommand(OpenViewer);
      DeleteViewerCommand = new AsyncRelayCommand(DeleteViewer, CanDeleteViewer);
    }

    private bool CanDeleteViewer()
    {
      lock (Module1._lock)
      {
        return !string.IsNullOrEmpty(Module1.SelectedProjectTemplate) && Viewers.Any();
      }
    }

    private bool CanNewViewer()
    {
      return !string.IsNullOrEmpty(Module1.SelectedProjectTemplate);
    }

    private async Task DeleteViewer()
    {
      if (_deleteViewersWindow != null)
      {
        return;
      }
      _deleteViewersWindow = new DeleteViewersWindow
      {
        Owner = FrameworkApplication.Current.MainWindow
      };
      var dc = _deleteViewersWindow.DataContext as DeleteViewersWindowViewModel;
      dc.SetViewers(Viewers);
      _deleteViewersWindow.Closed += (o, e) =>
      {
        var dataContext = _deleteViewersWindow.DataContext as DeleteViewersWindowViewModel;
        _itemIds = dataContext.ItemIds;
        _removeAsItem = dataContext.RemoveAsItem;
        _deleteViewersWindow = null;
      };
      var result = _deleteViewersWindow.ShowDialog();
      if (_itemIds != null && _itemIds.Any())
      {
        _progDialog = new ProgressDialog("Verwijder afgeleiden...");
        _progDialog.Show();
        await DeleteViewersAsync().ConfigureAwait(false);
        _progDialog.Hide();
      }
    }

    private async Task OpenViewer()
    {
      if (_galleryBusy)
        return;
      if (SelectedViewer is WebMapItem clickedWebMapItem)
      {
        _galleryBusy = true;
        FrameworkApplication.State.Activate("ViewersGallery_Is_Busy_State");
        try
        {
          await QueuedTask.Run(async () =>
          {
            //Open WebMap
            var currentItem = ItemFactory.Instance.Create(clickedWebMapItem.ID, ItemFactory.ItemType.PortalItem);
            var mapTitle = clickedWebMapItem.Title;
            var mapProjectItems = Project.Current.GetItems<MapProjectItem>();
            if (!string.IsNullOrEmpty(mapTitle))
            {
              var mapsWithSameTitleAsPortalItem = mapProjectItems.Where(x => !string.IsNullOrEmpty(x.Title) && x.Title.Equals(mapTitle, StringComparison.CurrentCultureIgnoreCase));
              Project.Current.RemoveItems(mapsWithSameTitleAsPortalItem);
              var newMap = MapFactory.Instance.CreateMapFromItem(currentItem);
              await FrameworkApplication.Panes.CreateMapPaneAsync(newMap).ConfigureAwait(false);
            }
          }).ConfigureAwait(false);
        }
        finally
        {
          _galleryBusy = false;
          FrameworkApplication.State.Deactivate("ViewersGallery_Is_Busy_State");
        }
        SelectedViewer = null;
      }
    }

    private async Task NewViewer()
    {
      if (_newviewerwindow != null)
      {
        return;
      }
      _newviewerwindow = new NewViewerWindow
      {
        Owner = FrameworkApplication.Current.MainWindow
      };
      _newviewerwindow.Closed += (o, e) =>
      {
        var dataContext = _newviewerwindow.DataContext as NewViewerWindowViewModel;
        _name = dataContext.Name;
        _newviewerwindow = null;
      };
      var result = _newviewerwindow.ShowDialog();
      if (result.Value)
      {
        _progDialog = new ProgressDialog($"Aanmaken afgeleide [{_name}]...");
        _progDialog.Show();
        await CreateNewViewerFromTemplate().ConfigureAwait(false);
        _progDialog.Hide();
      }
    }

    public IAsyncRelayCommand NewViewerCommand { get; }
    public IAsyncRelayCommand OpenViewerCommand { get; }
    public IAsyncRelayCommand DeleteViewerCommand { get; }
    public string LoadingMessage
    {
      get => _loadingMessage;
      set => SetProperty(ref _loadingMessage, value);
    }
    public bool ViewerIsSelected
    {
      get => _viewerIsSelected;
      set => SetProperty(ref _viewerIsSelected, value);
    }
    public ObservableCollection<WebMapItem> Viewers
    {
      get
      {
        return _viewers;
      }
      set
      {
        SetProperty(ref _viewers, value);
      }
    }
    public WebMapItem SelectedViewer
    {
      get => _selectedViewer;
      set
      {
        SetProperty(ref _selectedViewer, value);
        ViewerIsSelected = value != null;
      }
    }
    public string Template
    {
      get => _template;
      set => SetProperty(ref _template, value);
    }

    private void RenewData(EventBase eventData)
    {
      var updateGalleryEvent = eventData as UpdateGalleryEvent;
      if (updateGalleryEvent != null)
      {
        if (updateGalleryEvent.TemplateAdded)
        {
          // niets doen
          return;
        }
        if (updateGalleryEvent.ViewerDeleted)
        {
          // niets doen
          return;
        }
        lock (Module1._lock)
        {
          _viewers.Clear();
          if (updateGalleryEvent.TemplateSelected)
          {
            LoadItemsAsync();
          }
          if (updateGalleryEvent.TemplateDeleted)
          {
            Template = string.Empty;
            FrameworkApplication.Current.Dispatcher.Invoke(() =>
            {
              NewViewerCommand.NotifyCanExecuteChanged();
            });
          }
        }
      }
    }

    private async void LoadItemsAsync()
    {
      if (_galleryBusy)
        return;
      if (string.IsNullOrEmpty(Module1.SelectedProjectTemplate))
      {
        Template = "";
        lock (Module1._lock)
        {
          _viewers.Clear();
          var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId);
          if (pane != null)
          {
            try
            {
              pane.IsVisible = false;
            }
            catch (InvalidOperationException e)
            {
            }
          }
        }
        return;
      }
      _galleryBusy = true;
      LoadingMessage = "Inladen afgeleiden...";
      Template = Module1.SelectedProjectTemplateName;
      FrameworkApplication.State.Activate("ViewersGallery_Is_Busy_State");
      var portal = ArcGISPortalManager.Current.GetActivePortal();
      if (portal == null)
      {
        lock (Module1._lock)
        {
          _viewers.Clear();
          LoadingMessage = "Sign on to retrieve web maps";
        }
        return;
      }
      var signedOn = await QueuedTask.Run(() => portal.IsSignedOn()).ConfigureAwait(false);
      if (!signedOn)
      {
        lock (Module1._lock)
        {
          _viewers.Clear();
          LoadingMessage = "Sign on to retrieve web maps";
        }
        return;
      }
      var portalInfo = await portal.GetPortalInfoAsync().ConfigureAwait(false);
      var orgId = portalInfo.OrganizationId;
      var username = portal.GetSignOnUsername();
      var query = new PortalQueryParameters($"type:\"Web Map\" AND tags:\"ProjectAtlas\" AND tags:\"PAT{Module1.SelectedProjectTemplate}\" AND tags:\"CopyOfTemplate\" AND orgid:{orgId} owner:\"{username}\"");
      query.SortField = "title";
      query.Limit = 100;
      var results = await portal.SearchForContentAsync(query).ConfigureAwait(false);
      if (results == null)
      {
        lock (Module1._lock)
        {
          Viewers.Clear();
        }
        return;
      }
      foreach (var item in results.Results.OrderBy(x => x.Title))
      {
        lock (Module1._lock)
        {
          Viewers.Add(new WebMapItem(item));
        }
      }
      LoadingMessage = string.Empty;
      _galleryBusy = false;
      FrameworkApplication.State.Deactivate("ViewersGallery_Is_Busy_State");
      FrameworkApplication.Current.Dispatcher.Invoke(() =>
      {
        NewViewerCommand.NotifyCanExecuteChanged();
        DeleteViewerCommand.NotifyCanExecuteChanged();
      });
    }

    private async Task<PortalItem> FindItem(string itemId)
    {
      var portal = ArcGISPortalManager.Current.GetActivePortal();
      var query = new PortalQueryParameters("id:" + itemId);
      query.Limit = 1;
      var results = await portal.SearchForContentAsync(query).ConfigureAwait(false);
      return results.Results.FirstOrDefault();
    }

    private async Task CreateNewViewerFromTemplate()
    {
      var portal = ArcGISPortalManager.Current.GetActivePortal();
      var item = await FindItem(Module1.SelectedProjectTemplate);
      if (item == null)
      {
        return;
      }
      var tags = TagsHelper.ParseTags(item.ItemTags);
      if (tags.Contains("Template"))
      {
        tags = tags.Replace("Template", string.Empty);
      }
      tags += ",CopyOfTemplate";
      if (!tags.Contains("ProjectAtlas"))
      {
        tags += ",ProjectAtlas";
      }
      if (!tags.Contains($"PAT{item.ID}"))
      {
        tags += $",PAT{item.ID}";
      }
      if (tags.StartsWith(","))
      {
        tags = tags.Substring(1);
      }
      tags = tags.Replace(",,", ",");
      var portalClient = new PortalClient(item.PortalUri, portal.GetToken());
      await portalClient.UpdateTemplate(item, true).ConfigureAwait(false);
      var newViewer = await portalClient.CreateViewerFromTemplate(item, _name, tags).ConfigureAwait(false);
      _viewers.InsertInPlace(newViewer, x => x.Title);
      FrameworkApplication.Current.Dispatcher.Invoke(() =>
      {
        DeleteViewerCommand.NotifyCanExecuteChanged();
      });
    }

    private async Task DeleteViewersAsync()
    {
      var portal = ArcGISPortalManager.Current.GetActivePortal();
      var portalClient = new PortalClient(portal.PortalUri, portal.GetToken());
      foreach (var itemid in _itemIds)
      {
        var item = await FindItem(itemid);
        if (item == null)
        {
          continue;
        }
        if (_removeAsItem)
        {
          await portalClient.Delete(item).ConfigureAwait(false);
        }
        else
        {
          var tags = TagsHelper.UpdateTags(item.ItemTags);
          await portalClient.UpdateTags(item, tags).ConfigureAwait(false);
        }
        var viewerToRemove = Viewers.FirstOrDefault(x => x.ID.Equals(itemid));
        if (viewerToRemove != null)
        {
          _viewers.Remove(viewerToRemove);
        }
        if (!_removeAsItem)
        {
          EventSender.Publish(new UpdateGalleryEvent() { ViewerDeleted = true, Template = item });
        }
      }
    }
  }
}
