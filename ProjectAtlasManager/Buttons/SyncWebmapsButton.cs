using ArcGIS.Desktop.Framework.Contracts;
using ProjectAtlasManager.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Buttons
{
  class SyncWebmapsButton : Button
  {
    protected override void OnClick()
    {
      EventSender.Publish(new UpdateGalleryEvent() { UpdateTemplatesGallery = false, UpdateWebmapsGallery = true });
    }
  }
}
