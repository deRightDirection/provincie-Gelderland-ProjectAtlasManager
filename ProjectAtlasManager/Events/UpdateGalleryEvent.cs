using ArcGIS.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Events
{
  class UpdateGalleryEvent : EventBase
  {
    public bool UpdateWebmapsGallery { get; set; }
    public bool UpdateTemplatesGallery { get; set; }
  }
}
