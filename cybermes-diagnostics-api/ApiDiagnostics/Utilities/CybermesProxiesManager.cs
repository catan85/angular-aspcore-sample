using ApiDiagnostics.Model;
using ApiDiagnostics.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace ApiDiagnostics
{
    public class CybermesProxiesManager
    {

        const int MAX_API_ATTEMPTS = 20;
        public IEnumerable<CybermesProxy> GetAllProxies(IEnumerable<CybermesService> foundServices, List<ProxyServerConfiguration> configuredProxies)
        {
            Dictionary<int,CybermesService> foundApi = new Dictionary<int,CybermesService>();
            foreach (var service in foundServices)
            {
                if (StringContainsIgnoreCase(service.Name, "apiMes") && service.ListeningPort != null)
                {
                    foundApi.Add(service.ListeningPort.Value, service);
                }
            }

            foreach (var configuredProxy in configuredProxies)
            {
                var cybermesProxy = new CybermesProxy();

                cybermesProxy.Name = configuredProxy.Name;
                cybermesProxy.ListeningAddress = configuredProxy.ListeningAddress;
                cybermesProxy.ManagementAddress = configuredProxy.ManagementAddress;
                
                var proxyServerData = GetProxyServerData(configuredProxy.ManagementAddress);

                if (proxyServerData != null)
                {
                    if (proxyServerData.LinkedApiAddress.IpEquals(configuredProxy.ForwardingProductionAddress))
                    {
                        cybermesProxy.UpdateMode = false;
                    }
                    else if (proxyServerData.LinkedApiAddress.IpEquals(configuredProxy.ForwardingUpdatingAddress))
                    {
                        cybermesProxy.UpdateMode = true;
                    }

                    var linkedApiPort = Convert.ToInt32(configuredProxy.ForwardingProductionAddress.Split(":")[1]);
                    if (foundApi.ContainsKey(linkedApiPort))
                    {
                        cybermesProxy.LinkedApiMes = foundApi[linkedApiPort];
                    }

                    yield return cybermesProxy;
                }
            }
        }

        public bool TurnApiMesUpdateModeOn(CybermesProxy proxy)
        {
            // Copy apiMes files and folders in a duplicate version
            string temporaryFolderPath = $"{Path.GetTempPath()}CamozziApiDiagnostics\\TMP_{proxy.LinkedApiMes.Name}";
            if (System.IO.Directory.Exists(temporaryFolderPath))
                System.IO.Directory.Delete(temporaryFolderPath,true);

            System.IO.Directory.CreateDirectory(temporaryFolderPath);
            CopyFilesRecursively(
                new DirectoryInfo(proxy.LinkedApiMes.Path), new DirectoryInfo(temporaryFolderPath));

            // Modify the listening port of the copied api Mes
            string configFilePath = temporaryFolderPath + "\\appsettings.json";
            var configJsonString = System.IO.File.ReadAllText(configFilePath);
            var configObject = JsonConvert.DeserializeObject<JObject>(configJsonString);
            string currentAddress = configObject["SettingsUrl"]["Ip"].Value<string>();
            int currentPort = configObject["SettingsUrl"]["Port"].Value<int>();
            int newPort = currentPort + 100;
            configObject["SettingsUrl"]["Port"] = newPort;
            File.WriteAllText(configFilePath, configObject.ToString());

            // Run the new apiMes
            string apiMesExePath = temporaryFolderPath + "\\apiMes.exe";
            string commandText = $"/C {apiMesExePath} -instance:999999";
            var launchedProcess = System.Diagnostics.Process.Start("CMD.exe", commandText);

            // Check if the new apiMes is reachable
            var newApiIsAlive = CheckApiAlive("http://" + currentAddress + ":" + newPort);

            if (newApiIsAlive)
            {
                // Change the proxy to point to the copied ApiMes
                SetProxyForwardingToAddress(proxy.ManagementAddress, currentAddress + ":" + newPort);

                // Stop the apiMes service
                ServiceController sc = new ServiceController(proxy.LinkedApiMes.Name);
                sc.Stop();

                return true;
            }
            else
            {
                return false;
            }
        }


        public bool TurnApiMesUpdateModeOff(CybermesProxy proxy)
        {
            // Run the new apiMesService
            ServiceController sc = new ServiceController(proxy.LinkedApiMes.Name);
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                sc.Start();
            }
            
            // Check if the apiMes is reachable
            string configFilePath = proxy.LinkedApiMes.Path + "\\appsettings.json";
            var configJsonString = System.IO.File.ReadAllText(configFilePath);
            var configObject = JsonConvert.DeserializeObject<JObject>(configJsonString);
            string productionAddress = configObject["SettingsUrl"]["Ip"].Value<string>();
            int productionPort = configObject["SettingsUrl"]["Port"].Value<int>();

            var productionApiIsAlive = CheckApiAlive("http://" + productionAddress + ":" + productionPort);

            if (productionApiIsAlive)
            {
                SetProxyForwardingToAddress(proxy.ManagementAddress, productionAddress + ":" + productionPort);

                var tempApiPath = $"{Path.GetTempPath()}CamozziApiDiagnostics\\TMP_{proxy.LinkedApiMes.Name}\\apiMes.exe";

                KillProcessByPath(tempApiPath);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }


        private bool CheckApiAlive(string apiAddress)
        {
            var client = new RestClient(apiAddress);
            client.Timeout = 1000;
            var request = new RestRequest("API/IsAlive", Method.GET);

            var apiAlive = false;
            var notAliveCounter = 0;

            while (!apiAlive)
            {
                try
                {
                    var apiAliveResult = client.Execute<bool>(request).Data;
                    if (apiAliveResult)
                    {
                        apiAlive = true;
                    }
                    else
                    {
                        Console.WriteLine($"Api Mes not reachable [{notAliveCounter}]..");
                        notAliveCounter++;
                    }
                }
                catch
                {
                    Console.WriteLine($"Api Mes not reachable [{notAliveCounter}]..");
                    notAliveCounter++;
                }

                if (notAliveCounter >= MAX_API_ATTEMPTS)
                {
                    Console.WriteLine($"Api Mes not reachable for {MAX_API_ATTEMPTS} attempts, terminating");
                    return false;
                }
            }
            return true;
        }

        private void KillProcessByPath(string processPath)
        {
   
            Process[] runningProcesses = Process.GetProcesses();
            foreach (Process process in runningProcesses.Where(p=>p.ProcessName == "ApiMes"))
            {
                if (process.MainModule != null &&
                    string.Compare(process.MainModule.FileName, processPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    process.Kill();
                }
            }
        }


        private bool StringContainsIgnoreCase(string str, string searchString)
        {
            return str.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private ProxyServerData GetProxyServerData(string proxyManagementAddress)
        {
            var client = new RestClient("http://"+ proxyManagementAddress);
            var request = new RestRequest("servers/apimes", DataFormat.Json);
            var response = client.Get(request);
            var responseObject = JsonConvert.DeserializeObject<JObject>(response.Content);

            if (responseObject != null)
            {

                var proxyData = new ProxyServerData();
                proxyData.ManagementAddress = proxyManagementAddress;
                proxyData.ListeningAddress = responseObject["bind"].Value<string>();
                proxyData.LinkedApiAddress = responseObject["discovery"]["static_list"].First().Value<string>().Split(" ")[0];

                return proxyData ;
            }
            else
            {
                return null;
            }
        }

        private bool SetProxyForwardingToAddress(string proxyManagement, string newAddress)
        {
            var client = new RestClient("http://" + proxyManagement);
            var request = new RestRequest("servers/apimes", DataFormat.Json);
            var response = client.Get(request);
            var responseObject = JsonConvert.DeserializeObject<JObject>(response.Content);

            var staticListOtherFields = responseObject["discovery"]["static_list"].First().Value<string>().Split(" ").Skip(1);
            var otherFieldsString = string.Join(' ', staticListOtherFields);
            responseObject["discovery"]["static_list"] = JArray.Parse($"[ '{newAddress} {otherFieldsString}' ]");

            if (response.StatusCode != System.Net.HttpStatusCode.OK || responseObject == null)
            {
                return false;
            }

            DateTime start = DateTime.Now;

            var deleteApiMesRequest = new RestRequest("servers/apimes", DataFormat.Json);
            var deleteResponse = client.Delete(deleteApiMesRequest);

            if (deleteResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }
                

            var postNewApiMesRequest = new RestRequest("servers/apimes", DataFormat.Json);
            postNewApiMesRequest.AddJsonBody(responseObject.ToString());
            var postNewApiMesResponse = client.Post(postNewApiMesRequest);

            if (postNewApiMesResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }

            Console.WriteLine("Total offline: " + (DateTime.Now - start).TotalMilliseconds);

            return true;
        }



    }
}
