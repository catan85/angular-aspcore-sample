using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiDiagnostics.Model
{
    public class CybermesProxy
    {
        public string Name { get; set; }
        public string ManagementAddress { get; set; }

        public string ListeningAddress { get; set; }

        public bool UpdateMode { get; set; }

        public CybermesService LinkedApiMes { get; set; }
    }
}
