using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebManagement.Models
{
    public class DeviceRef
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }
        [JsonProperty("DeviceType")]
        public string DeviceType { get; set; }
        [JsonProperty("Installation")]
        public string Installation { get; set; }
    }
}
