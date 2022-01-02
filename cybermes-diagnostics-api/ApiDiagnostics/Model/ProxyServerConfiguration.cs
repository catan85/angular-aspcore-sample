using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiDiagnostics.Model
{
    public class ProxyServerConfiguration
    {
        public string Name { get; set; }
        public string ManagementAddress { get; set; }
        public string ListeningAddress { get; set; }
        public string ForwardingProductionAddress { get; set; }
        public string ForwardingUpdatingAddress { get; set; }

    }
}
