using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Principal;
namespace ApiDiagnostics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine("This application must be executed with administrator rights");
                Console.WriteLine("-------------------------------------------------------------------");
                Environment.Exit(-1);
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
