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

   
        /// <summary>
        /// Gets a list of the found proxy installed on the server
        /// The proxy should be configured in the ApiDiagnostic configuration
        /// Combined to the proxy it is listed also the connected apiMes (according to
        /// the listening port of the apiMes).
        /// </summary>
        [HttpGet]
        [Route("/CybermesProxiesController/GetAllProxies")]
        public IEnumerable<CybermesProxy> GetAllProxies()
        {
            var services = servicesReader.GetInstalledServices();
            var proxies = configuration.GetSection("Proxies").Get<List<ProxyServerConfiguration>>();
            return proxiesManager.GetAllProxies(services, proxies);
        }


        /// <summary>
        /// Switch the ApiMes connected to a proxy in update mode.
        /// Update mode means that the production service will be stopped and a temporary service will be
        /// started on a differente port. The proxy will switch the calls to the temporary service, then 
        /// you will be able to update the production service
        /// </summary>
        [HttpPost]
        [Route("/CybermesProxiesController/TurnUpdateModeOn")]
        public bool TurnUpdateModeOn([FromBody]CybermesProxy proxy)
        {
            return proxiesManager.TurnApiMesUpdateModeOn(proxy);
        }

        /// <summary>
        /// Switch the ApiMes connected to a proxy in production mode.
        /// It stops the temporary apiMes and turn on again the production one. 
        /// Then the proxy is changed to forward the calls to the production proxy
        /// </summary>
        [HttpPost]
        [Route("/CybermesProxiesController/TurnUpdateModeOff")]
        public bool TurnUpdateModeOff([FromBody] CybermesProxy proxy)
        {
            return proxiesManager.TurnApiMesUpdateModeOff(proxy);
        }
        
    }
}
