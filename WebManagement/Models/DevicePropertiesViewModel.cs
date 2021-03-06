﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebManagement.Models
{
    public class DevicePropertiesViewModel
    {
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string Location { get; set; }
        public int DesiredValue { get; set; }
        public string State { get; set; }
        public string Version { get; set; }
    }
}
