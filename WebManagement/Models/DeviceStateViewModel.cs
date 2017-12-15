using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebManagement.Models
{
    public class DeviceStateViewModel
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }
        [JsonProperty("deviceType")]
        public string DeviceType { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("value")]
        public int CurrentValue { get; set; }
        public bool Active { get; set; }
    }
}
