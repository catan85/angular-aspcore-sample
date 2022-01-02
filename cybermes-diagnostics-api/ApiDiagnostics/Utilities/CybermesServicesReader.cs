using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceProcess;
using Microsoft.Win32;
using System.IO;
using ApiDiagnostics.Model;
using System.IO.Compression;
using ApiDiagnostics.Utilities;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                var serviceImage = GetServiceImage(service);
                var assemblyPath = GetAssemblyPathFromServiceImage(serviceImage);
                var path = GetDirectoryPathOfServiceImage(serviceImage);
                var vers = GetVersion(path + "\\ReleaseLog\\ReleaseLog.en-US.txt");
                var containsException = LastLogFileContainsException(path);
                var hasLogFiles = HasLogFiles(path);

                int? listeningPort = null;
                if (name.Contains("ApiMes"))
                {
                    string apiMesCfgFilePath = path + "\\appsettings.json";
                    if (File.Exists(apiMesCfgFilePath))
                    {
                        var apiMesConfiguration = JsonConvert.DeserializeObject<JObject>(System.IO.File.ReadAllText(apiMesCfgFilePath));
                        var configuredPort = apiMesConfiguration["SettingsUrl"]["Port"];
                        if (configuredPort != null)
                        {
                            listeningPort = Convert.ToInt32(configuredPort);
                        }
                        
                    }
                }

                yield return new CybermesService()
                {
                    Name = name,
                    IsRunning = isRunning,
                    Path = path,
                    AssemblyPath = assemblyPath,
                    Version = vers,
                    ContainsException = containsException,
                    HasLogFiles = hasLogFiles,
                    ListeningPort = listeningPort
                };
            }
        }

        public (MemoryStream data,bool isZip) GetLogFiles(string applicationBasePath)
        {
            List<string> foundLogFiles = new List<string>();

            // Il path del servizio potrebbe non esistere se fosse stato eliminato ma non disinstallato
            if(Directory.Exists(applicationBasePath))
            {
                foreach (string file in Directory.EnumerateFiles(applicationBasePath, "*.txt", SearchOption.AllDirectories))
                {
                    string filename = System.IO.Path.GetFileName(file);
                    bool filenameContainsLog = filename.IndexOf("log", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (filenameContainsLog)
                        foundLogFiles.Add(file);
                }

                if (foundLogFiles.Count > 1)
                {
                    // zip files
                    byte[] archiveFile;
                    using (var archiveStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                        {
                            foreach (var file in foundLogFiles)
                            {
                                // Passiamo solo il file name per appiattire la lista (tutti i log nella root dello zip
                                var zipArchiveEntry = archive.CreateEntry(System.IO.Path.GetFileName(file), CompressionLevel.Fastest);
                                using (var zipStream = zipArchiveEntry.Open())
                                {
                                    byte[] fileContent = null;
                                    using (System.IO.FileStream fs = System.IO.File.Open(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                                    {
                                        int numBytesToRead = Convert.ToInt32(fs.Length);
                                        fileContent = new byte[(numBytesToRead)];
                                        fs.Read(fileContent, 0, numBytesToRead);
                                    }

                                    zipStream.Write(fileContent, 0, fileContent.Length);
                                }
                            }
                        }

                        return (archiveStream, true);
                    }
                }
                else if (foundLogFiles.Count == 1)
                {
                    // download directly
                    MemoryStream ms = new MemoryStream();

                    using (FileStream file = new FileStream(foundLogFiles[0], FileMode.Open, FileAccess.Read))
                        file.CopyTo(ms);

                    return (ms, false);

                }
            }
            
            return (null,false);
        }

        private bool HasLogFiles(string applicationBasePath)
        {
            List<string> foundLogFiles = new List<string>();

            // Il path del servizio potrebbe non esistere se fosse stato eliminato ma non disinstallato
            if (Directory.Exists(applicationBasePath))
            {
                foreach (string file in Directory.EnumerateFiles(applicationBasePath, "*.txt", SearchOption.AllDirectories))
                {
                    string filename = System.IO.Path.GetFileName(file);
                    bool filenameContainsLog = filename.IndexOf("log", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (filenameContainsLog)
                        foundLogFiles.Add(file);
                }

                if (foundLogFiles.Count >= 1)
                {
                    return true;
                }
            }
            return false;
        }

        private bool LastLogFileContainsException(string applicationBasePath)
        {
            List<string> lastLogFile = new List<string>();

            // Il path del servizio potrebbe non esistere se fosse stato eliminato ma non disinstallato
            if (Directory.Exists(applicationBasePath))
            {
                foreach (string file in Directory.EnumerateFiles(applicationBasePath, "*.txt", SearchOption.TopDirectoryOnly))
                {
                    string filename = System.IO.Path.GetFileName(file);

                    // Ricerca di file txt contenenti la parola log nel nome file
                    bool filenameContainsLog = StringContainsIgnoreCase(filename, "log");

                    // I file di release log vanno esclusi
                    bool filenameContainsRelease = StringContainsIgnoreCase(filename, "release");
                    
                    // L'ultimo file di log non contiene numeri
                    bool filenameContainsDigits = filename.Any(char.IsDigit);


                    if (filenameContainsLog && !filenameContainsRelease && !filenameContainsDigits)
                        lastLogFile.Add(file);
                }
            }

            if (lastLogFile.Count == 1)
            {
                List<string> lines = new List<string>();
                using (StreamReader reader = new StreamReader(new FileStream(lastLogFile[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    // Use while not null pattern in while loop.
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
                lines.Reverse();
                for (int i = 0; i<500 && i<lines.Count; i++)
                {
                    var containsError = LineContainsError(lines[i]);
                    if (containsError)
                    {
                        return true;
                    }
                }
            }
            else if (lastLogFile.Count > 1)
            {
                Console.WriteLine("Too many log files found");
            }
            return false;
        }

        private bool LineContainsError(string line)
        {
            bool containsException = StringContainsIgnoreCase(line, "exception");
            bool containsError = StringContainsIgnoreCase(line, "error");
            return containsError || containsException;
        }

        private bool StringContainsIgnoreCase(string str, string searchString)
        {
            return str.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string GetServiceImage(ServiceController service)
        {
            var key = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\" + service.ServiceName;
            var val = @"ImagePath";

            return Registry.GetValue(key, val, string.Empty).ToString();
        }

        private string GetAssemblyPathFromServiceImage(string serviceImage)
        {
            var parts = Regex.Matches(serviceImage, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();
            if (parts.Count>0)
            {
                return parts[0].Trim('\"');
            }else
            {
                return null;
            }    
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
