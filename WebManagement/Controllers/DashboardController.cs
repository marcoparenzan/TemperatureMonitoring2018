using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebManagement.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace WebManagement.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(IConfiguration configuration): base(configuration)
        {
        }

        // GET: Home
        public async Task<ActionResult> Properties(string location)
        {
            if (string.IsNullOrWhiteSpace(location)) return RedirectToAction("Index", "Locations", new { returnUrl = $"{Request.Path}{Request.QueryString}" });
            var list = await Data(location);
            return View(list);
        }

        // GET: Home
        public async Task<ActionResult> State(string location)
        {
            if (string.IsNullOrWhiteSpace(location)) return RedirectToAction("Index", "Locations", new { returnUrl = $"{Request.Path}{Request.QueryString}" });
            var list = await Data(location);
            return View(list);
        }

        // GET: Home
        public async Task<List<DevicePropertiesViewModel>> Data(string location)
        {
            var list = new List<DevicePropertiesViewModel>();
            var query_westus = RM(location).CreateQuery("SELECT * FROM devices");
            list.AddRange((await query_westus.GetNextAsTwinAsync()).Select(xx => new DevicePropertiesViewModel
            {
                DeviceId = xx.DeviceId,
                DeviceType = xx.Tag("DeviceType"),
                Location = location,
                DesiredValue = xx.Desired<int>("desiredValue"),
                State = xx.Reported<string>("state"),
                Version = xx.Reported<string>("version")
            }));

            return list;
        }

        // GET: Devices/State
        public async Task<DevicePropertiesViewModel> PropertiesData(string location, string id)
        {
            var rm = RM(location);
            var twin = await rm.GetTwinAsync(id);

            DevicePropertiesViewModel viewModel = new DevicePropertiesViewModel
            {
                Location = location,
                DeviceId = id,
                DeviceType = twin.Tag("DeviceType"),
                DesiredValue = twin.Desired<int>("desiredValue"),
                State = twin.Reported<string>("state"),
                Version = twin.Reported<string>("version")
            };

            return viewModel;
        }

        // GET: Devices/State
        public async Task<DeviceStateViewModel> StateData(string location, string id)
        {
            var rm = RM(location);
            var twin = await rm.GetTwinAsync(id);

            DeviceStateViewModel viewModel = new DeviceStateViewModel
            {
                Location = location,
                DeviceId = id,
                DeviceType = twin.Tag("DeviceType"),
                Active = false
            };

            try
            {
                var storageAccount = CloudStorageAccount.Parse(Configuration.GetConnectionString("Storage"));
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("devicestates");
                var blobRef = container.GetBlockBlobReference($"{location}_{id}.json");

                var json = await blobRef.DownloadTextAsync();
                viewModel = JsonConvert.DeserializeObject<DeviceStateViewModel>(json);
                viewModel.Active = true;
            }
            catch
            {
            }

            return viewModel;
        }

        public class Req
        {
            public string deviceId { get; set; }
            public string location { get; set; }
        }

        public async Task Increase([FromBody] Req req)
        {
            var rm = RM(req.location);
            var twin = await rm.GetTwinAsync(req.deviceId);
            var desiredValue = twin.Properties.Desired.Contains("desiredValue") ? (int)twin.Properties.Desired["desiredValue"] : 0;
            var newTwin = new Twin();
            newTwin.Properties.Desired["desiredValue"] = desiredValue + 1;
            await rm.UpdateTwinAsync(req.deviceId, newTwin, twin.ETag);
        }

        public async Task Decrease([FromBody] Req req)
        {
            var rm = RM(req.location);
            var twin = await rm.GetTwinAsync(req.deviceId);
            var desiredValue = twin.Properties.Desired.Contains("desiredValue") ? (int)twin.Properties.Desired["desiredValue"] : 0;
            var newTwin = new Twin();
            newTwin.Properties.Desired["desiredValue"] = desiredValue - 1;
            await rm.UpdateTwinAsync(req.deviceId, newTwin, twin.ETag);
        }

        public async Task Reset([FromBody] Req req)
        {
            var sc = SC(req.location);
            await sc.InvokeDeviceMethodAsync(req.deviceId, new CloudToDeviceMethod("reset"));
        }
    }
}