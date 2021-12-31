using ApiDiagnostics.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiDiagnostics.Controllers
{
    public class CybermesProxiesController : Controller
    {
        private CybermesServicesReader servicesReader = null;
        private CybermesProxiesManager proxiesManager = null;
        private readonly IConfiguration configuration;


        public CybermesProxiesController(CybermesServicesReader servicesReader, CybermesProxiesManager proxiesManager, IConfiguration config)
        {
            this.servicesReader = servicesReader;
            this.proxiesManager = proxiesManager;
            this.configuration = config;
        }

   
        [HttpGet]
        [Route("/CybermesProxiesController/GetAllProxies")]
        public IEnumerable<CybermesProxy> GetAllProxies()
        {
            var services = servicesReader.GetInstalledServices();
            var proxies = configuration.GetSection("Proxies").Get<List<string>>();
            return proxiesManager.GetAllProxies(services, proxies);
        }

        [HttpPost]
        [Route("/CybermesProxiesController/TurnUpdateModeOn")]
        public bool TurnUpdateModeOn([FromBody]CybermesProxy proxy)
        {
            return proxiesManager.TurnApiMesUpdateModeOn(proxy);
        }


        [HttpPost]
        [Route("/CybermesProxiesController/TurnUpdateModeOff")]
        public bool TurnUpdateModeOff([FromBody] CybermesProxy proxy)
        {
            return proxiesManager.TurnApiMesUpdateModeOff(proxy);
        }
        
    }
}
