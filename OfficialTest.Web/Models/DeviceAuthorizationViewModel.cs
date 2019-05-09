using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficialTest.Web.Models
{
    public class DeviceAuthorizationViewModel : ConsentViewModel
    {
        public string UserCode { get; set; }
        public bool ConfirmUserCode { get; set; }
    }
}

