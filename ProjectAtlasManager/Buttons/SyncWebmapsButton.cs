using ArcGIS.Desktop.Framework.Contracts;
using ProjectAtlasManager.Events;

namespace ProjectAtlasManager.Buttons
{
  class SyncWebmapsButton : Button
  {
    protected override void OnClick()
    {
      EventSender.Publish(new UpdateGalleryEvent { DataRefreshed = true });
    }
  }
}
