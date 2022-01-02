using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiDiagnostics.Controllers
{
    public class UtilitiesController : Controller
    {

        [HttpGet]
        [Route("/UtilitiesController/GetApiMesUrlClients")]
        public string? GetApiMesUrlClients()
        {
            return ReadEnvironmentVariableValue("ApiMesUrl");
        }

        [HttpGet]
        [Route("/UtilitiesController/GetApiMesUrlServices")]
        public string? GetApiMesUrlServices()
        {
            return ReadEnvironmentVariableValue("ApiMesUrlServices");
        }

        [HttpGet]
        [Route("/UtilitiesController/GetApiMesUrlReports")]
        public string? GetApiMesUrlReports()
        {
            return ReadEnvironmentVariableValue("ApiMesUrlReports");
        }

        private string? ReadEnvironmentVariableValue(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName);
        }
    }
}
