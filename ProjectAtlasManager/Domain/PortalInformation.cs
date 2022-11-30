using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Domain
{
  class PortalInformation
  {
    public Uri PortalUri { get; set; }
    public string Username { get; set; }
    public string PortalName { get; set; }
    public bool IsSignedOn { get; set; }
    public bool IsActive { get; set; }
  }
}
