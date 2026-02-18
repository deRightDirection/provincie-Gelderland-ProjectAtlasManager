using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core.Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Events;

class UpdateGalleryEvent : EventBase
{
  public bool ViewerDeleted { get; set; }
  public bool TemplateDeleted { get; set; }
  public PortalItem Template { get; set; }
  public bool TemplateAdded { get; set; }
  public bool TemplateSelected { get; set; }
  public bool DataRefreshed { get; set; }
}
