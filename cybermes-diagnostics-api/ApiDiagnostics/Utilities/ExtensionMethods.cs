using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiDiagnostics.Utilities
{
    public static class ExtensionMethods
    {
        public static bool IpEquals(this string firstString, string secondString)
        {
            var string1 = firstString.ToLower().Trim().Replace("localhost", "127.0.0.1");
            var string2 = secondString.ToLower().Trim().Replace("localhost", "127.0.0.1");

            return string1 == string2;
        }
    }
}
