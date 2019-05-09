using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficialTest.Web.Models
{
    public class DeviceAuthorizationInputModel : ConsentInputModel
    {
        public string UserCode { get; set; }
    }
}
