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
    public abstract class BaseController : Controller
    {
        public BaseController(IConfiguration configuration)
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

        protected IConfiguration Configuration { get; }
        private static LocationRef[] __locations;

        protected RegistryManager RM(string location)
        {
            if (location == null) location = __locations.First().Name;
            return RegistryManager.CreateFromConnectionString(Configuration.GetConnectionString(location));
        }

        protected ServiceClient SC(string location)
        {
            if (location == null) location = __locations.First().Name;
            return ServiceClient.CreateFromConnectionString(Configuration.GetConnectionString(location));
        }
    }
}