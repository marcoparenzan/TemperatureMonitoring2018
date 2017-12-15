﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebManagement.Models
{
    public class CreateDeviceViewModel
    {
        public string DeviceId { get; set; }
        public string Location { get; set; }
        public string DeviceType { get; set; }
        public string Installation { get; set; }
    }
}
