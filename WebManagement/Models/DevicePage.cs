using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebManagement.Models
{
    public class DevicePage
    {
        public string Location { get; set; }
        public List<DeviceRef> Items { get; set; }
    }
}
