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
        public string AssemblyPath { get; set; }
        public bool IsRunning { get; set; }
        public bool HasLogFiles { get; set; }
        public bool ContainsException { get; set; }

        public int? ListeningPort { get; set; }
    }
}
