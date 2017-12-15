using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using WebManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System.Text;
using Microsoft.WindowsAzure.Storage;

namespace WebManagement.Controllers
{
    [Authorize]
    public class DevicesController : BaseController
    {
        public DevicesController(IConfiguration configuration): base(configuration)
        {
        }

        // GET: Devices
        public async Task<ActionResult> Index(string location)
        {
            if (string.IsNullOrWhiteSpace(location)) return RedirectToAction("Index", "Locations", new { returnUrl = $"{Request.Path}{Request.QueryString}" });

            var registryManager = RM(location);
            var query = registryManager.CreateQuery($"SELECT deviceId, tags.DeviceType, tags.Installation FROM devices"); //
            var items = (await query.GetNextAsJsonAsync())
                .Select(xx => JsonConvert.DeserializeObject<DeviceRef>(xx))
               .ToList();

            return View(new DevicePage {
                Location = location,
                Items = items
            });
        }

        // GET: Devices/Create
        public async Task<ActionResult> Create(string location)
        {
            var viewModel = new CreateDeviceViewModel
            {
                Location = location
            };
            return View(viewModel);
        }

        // POST: Devices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateDeviceViewModel viewModel)
        {
            try
            {
                var registryManager = RM(viewModel.Location);
                var newDevice = new Device(viewModel.DeviceId)
                {
                    Status = DeviceStatus.Enabled
                };

                var device = await registryManager.AddDeviceAsync(newDevice);
                var twin = await registryManager.GetTwinAsync(device.Id);
                twin.Tags["DeviceType"] = viewModel.DeviceType;
                twin.Tags["Installation"] = viewModel.Installation;
                await registryManager.UpdateTwinAsync(device.Id, twin, twin.ETag);

                return RedirectToAction(nameof(Index), new { location = viewModel.Location });
            }
            catch(Exception ex)
            {
                return View(viewModel);
            }
        }

        // GET: Devices/Info
        public async Task<DeviceInfoViewModel> Info(string location, string id)
        {
            var registryManager = RM(location);
            var device = await registryManager.GetDeviceAsync(id);

            var viewModel = new DeviceInfoViewModel
            {
                Location = location,
                DeviceId = id,
                Status = device.Status.ToString(),
                ConnectionState = device.ConnectionState.ToString()
            };

            return viewModel;
        }

        // GET: Devices/Create
        public async Task<ActionResult> Edit(string location, string id)
        {
            var registryManager = RM(location);
            var device = await registryManager.GetDeviceAsync(id);
            var twin = await registryManager.GetTwinAsync(id);

            var viewModel = new EditDeviceViewModel
            {
                Location = location,
                DeviceId = id,
                DeviceType = twin.Tag("DeviceType"),
                Installation = twin.Tag("Installation")
            };

            return View(viewModel);
        }

        // POST: Devices/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CreateDeviceViewModel viewModel)
        {
            try
            {
                var registryManager = RM(viewModel.Location);
                var device = await registryManager.GetDeviceAsync(viewModel.DeviceId);
                var twin = await registryManager.GetTwinAsync(device.Id);
                twin.Tags["DeviceType"] = viewModel.DeviceType;
                twin.Tags["Installation"] = viewModel.Installation;
                await registryManager.UpdateTwinAsync(device.Id, twin, twin.ETag);

                return RedirectToAction(nameof(Index), new { location = viewModel.Location });
            }
            catch (Exception ex)
            {
                return View(viewModel);
            }

        }

        // GET: Devices/Create
        public async Task<FileContentResult> Deploy(string id, string location)
        {
            var registryManager = RM(location);
            var device = await registryManager.GetDeviceAsync(id);
            var twin = await registryManager.GetTwinAsync(id);

            var connectionInfo =
                Configuration.GetConnectionString(location).Split(";")
                .Select(xx => xx.Split("="))
                .ToDictionary(xx => xx[0].Trim(), xx => xx[1].Trim());

            var sb = new StringBuilder();
            sb.AppendLine($"{Configuration[$"AppSettings:launch{twin.Tag("DeviceType")}device"]} {connectionInfo["HostName"]} {id} {device.Authentication.SymmetricKey.PrimaryKey}");

            var result = new FileContentResult(
                Encoding.UTF8.GetBytes(sb.ToString()),
                "text/plain")
            {
            };
            result.FileDownloadName = $"{id}_{location}.bat";

            return result;
        }

        // GET: Devices/Create
        public async Task<ActionResult> Delete(string id, string location)
        {
            return View(new DeleteDeviceViewModel
            {
                DeviceId = id,
                Location = location
            });
        }

        // POST: Devices/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(DeleteDeviceViewModel viewModel)
        {
            try
            {
                var registryManager = RM(viewModel.Location);
                var device = await registryManager.GetDeviceAsync(viewModel.DeviceId);
                await registryManager.RemoveDeviceAsync(device);

                return RedirectToAction(nameof(Index), new { location = viewModel.Location });
            }
            catch
            {
                return View(viewModel);
            }
        }


        // GET: Devices/Create
        public async Task<ActionResult> Disable(string id, string location)
        {
            return View(new DisableDeviceViewModel
            {
                DeviceId = id,
                Location = location
            });
        }

        // POST: Devices/Disable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disable(DisableDeviceViewModel viewModel)
        {
            try
            {
                var registryManager = RM(viewModel.Location);
                var device = await registryManager.GetDeviceAsync(viewModel.DeviceId);
                device.Status = DeviceStatus.Disabled;
                device = await registryManager.UpdateDeviceAsync(device, true);

                return RedirectToAction(nameof(Index), new { location = viewModel.Location });
            }
            catch
            {
                return View(viewModel);
            }
        }


        // GET: Devices/Create
        public async Task<ActionResult> Enable(string id, string location)
        {
            return View(new EnableDeviceViewModel
            {
                DeviceId = id,
                Location = location
            });
        }

        // POST: Devices/Enable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Enable(EnableDeviceViewModel viewModel)
        {
            try
            {
                var registryManager = RM(viewModel.Location);
                var device = await registryManager.GetDeviceAsync(viewModel.DeviceId);
                device.Status = DeviceStatus.Enabled;
                device = await registryManager.UpdateDeviceAsync(device, true);

                return RedirectToAction(nameof(Index), new { location = viewModel.Location });
            }
            catch
            {
                return View(viewModel);
            }
        }
    }
}