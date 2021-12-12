using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiDiagnostics.Model
{
    public class CybermesService
    {
        public string Name { get; set; }
        public string Version { get; set; }

        public string Path { get; set; }
        public bool IsRunning { get; set; }
    }
}
