using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebManagement.Models
{
    public class DeviceInfoViewModel
    {
        public string DeviceId { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string ConnectionState { get; set; }
    }
}
