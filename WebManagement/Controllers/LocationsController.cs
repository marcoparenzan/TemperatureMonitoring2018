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
    public class LocationsController : Controller
    {
        public LocationsController(IConfiguration configuration)
        {
            Configuration = configuration;
            if (__locations == null)
            {
                __locations = Configuration["AppSettings:locations"].Split(',').Select(xx => new LocationRef
                {
                    Name = xx,
                    DisplayName = xx
                }).ToArray();
            }
        }

        public IConfiguration Configuration { get; }
        public static LocationRef[] __locations;

        private RegistryManager RM(string location)
        {
            if (location == null) location = __locations.First().Name;
            return RegistryManager.CreateFromConnectionString(Configuration.GetConnectionString(location));
        }

        // GET: Devices
        public async Task<ActionResult> Index(string returnUrl)
        {
            ViewData["returnUrl"] = returnUrl;
            return View(__locations);
        }
    }
}