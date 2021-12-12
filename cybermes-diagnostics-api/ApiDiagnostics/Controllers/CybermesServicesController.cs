using ApiDiagnostics.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ApiDiagnostics.Controllers
{
    public class CybermesServicesController : Controller
    {
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("/CybermesServicesController/GetAllServices")]
        public IEnumerable<CybermesService> GetAllServices()
        {
            CybermesServicesReader reader = new CybermesServicesReader();
            return reader.GetInstalledServices();
        }
    }
}
