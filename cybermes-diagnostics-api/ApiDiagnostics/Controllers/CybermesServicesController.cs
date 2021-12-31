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
        private CybermesServicesReader cybermesServicesReader = null;

        public CybermesServicesController(CybermesServicesReader reader)
        {
            cybermesServicesReader = reader;
        }

        // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("/CybermesServicesController/GetAllServices")]
        public IEnumerable<CybermesService> GetAllServices()
        {

            return cybermesServicesReader.GetInstalledServices();
        }

        // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("/CybermesServicesController/DownloadAllLogFiles")]
        public FileResult DownloadAllLogFiles([FromQuery] string applicationPath)
        {
            CybermesServicesReader reader = new CybermesServicesReader();
            var logs = reader.GetLogFiles(applicationPath);
            var appName = new System.IO.DirectoryInfo(applicationPath).Name;
            if (logs.data != null)
            {
                if (logs.isZip)
                {
                    return File(logs.data.ToArray(), "application/octet-stream", $"{appName}.zip");
                }
                else
                {
                    return File(logs.data.ToArray(), "application/octet-stream", $"{appName}.txt");
                }
            }
            else
            {
                return null;
            }
            
        }
    }
}
