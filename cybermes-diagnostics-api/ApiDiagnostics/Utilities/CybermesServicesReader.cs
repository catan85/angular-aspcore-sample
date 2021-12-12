using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceProcess;
using Microsoft.Win32;
using System.IO;
using ApiDiagnostics.Model;

namespace ApiDiagnostics
{
    public class CybermesServicesReader
    {
        public IEnumerable<CybermesService> GetInstalledServices()
        {
            
            var mesKeywords = new string[]
            {
                "postgres",
                "rabbit",
                "camozzi",
                "ccc"
            };

            var services = ServiceController.GetServices();

            var filteredServices = new List<ServiceController>();

            foreach (var service in services)
            {
                foreach (var keyword in mesKeywords)
                {
                    if ((service.ServiceName.ToLower() + service.DisplayName.ToLower()).Contains(keyword.ToLower()))
                    {
                        filteredServices.Add(service);
                    }
                }
            }


            foreach (var service in filteredServices)
            {
                var isRunning = service.Status == ServiceControllerStatus.Running;

                var name = service.ServiceName;
                var runn = isRunning ? "[RUNNING]" : service.Status.ToString();
                var imagePath = GetImagePath(service);
                var path = GetDirectoryPathOfServiceImage(imagePath);
                var vers = GetVersion(path + "\\ReleaseLog\\ReleaseLog.en-US.txt");

                Console.ForegroundColor = isRunning
                    ? ConsoleColor.Green
                    : ConsoleColor.Gray;

                Console.WriteLine();
                Console.WriteLine($" name : {name}");
                Console.WriteLine($" runn : {runn}");
                Console.WriteLine($" path : {path}");
                Console.WriteLine($" vers : {vers}");

                yield return new CybermesService()
                {
                    Name = name,
                    IsRunning = isRunning,
                    Path = path,
                    Version = vers
                };
            }
        }

        private string GetImagePath(ServiceController service)
        {
            var key = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\" + service.ServiceName;
            var val = @"ImagePath";

            return Registry.GetValue(key, val, string.Empty).ToString();
        }

        private static string GetDirectoryPathOfServiceImage(string path)
        {
            path = path.Trim('\"');
            path = ToUpperOnlyFirst(path);
            path = path.Split(new string[] { ".exe" }, StringSplitOptions.RemoveEmptyEntries).First() + ".exe";

            return Path.GetDirectoryName(path);
        }

        private static string ToUpperOnlyFirst(string s)
        {
            if (s.Length >= 2) return char.ToUpper(s[0]) + s.Substring(1).ToLower();
            if (s.Length == 1) return s.ToUpper();

            return string.Empty;
        }

        /// <summary>
        /// return the version in the indicated file, reading the pattern "*** version x.x.x.x ***"
        /// and indicating if is not the first expression in file
        /// </summary>
        public static string GetVersion(string versionFile = null)
        {

            if (System.IO.File.Exists(versionFile))
            {
                using (var stream = new StreamReader(versionFile))
                {
                    var line = stream.ReadLine();

                    var version = VersionFromRow(line);

                    // return the version, if it's the first line in file
                    if (version != null)
                    {
                        return $"{version}";
                    }

                    while ((line = stream.ReadLine()) != null)
                    {
                        version = VersionFromRow(line);

                        // return the version, but it's not the first line in file (maybe a non-wanted release)
                        if (version != null)
                        {
                            return $"{version} [+ stages]";
                        }
                    }
                }
            }
            else
            {
                return "[non present]";
            }

            return "[unknown]";
        }

        private static string VersionFromRow(string line)
        {
            if (line.Contains("*** version"))
            {
                return line
                    .Replace("***", "")
                    .Replace("version", "")
                    .Trim();
            }

            return null;
        }
    }
}
