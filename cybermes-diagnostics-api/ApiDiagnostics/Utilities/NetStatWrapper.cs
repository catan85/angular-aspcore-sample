using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApiDiagnostics.Utilities
{
    public class NetStatWrapper
    {

        public static List<ProcessPort> GetNetStatPorts()
        {
            var Ports = new List<ProcessPort>();

            try
            {
                using (Process p = new Process())
                {

                    ProcessStartInfo ps = new ProcessStartInfo();
                    ps.Arguments = "-a -n -o";
                    ps.FileName = "netstat.exe";
                    ps.UseShellExecute = false;
                    ps.WindowStyle = ProcessWindowStyle.Hidden;
                    ps.RedirectStandardInput = true;
                    ps.RedirectStandardOutput = true;
                    ps.RedirectStandardError = true;

                    p.StartInfo = ps;
                    p.Start();

                    StreamReader stdOutput = p.StandardOutput;
                    StreamReader stdError = p.StandardError;

                    string content = stdOutput.ReadToEnd() + stdError.ReadToEnd();
                    string exitStatus = p.ExitCode.ToString();

                    if (exitStatus != "0")
                    {
                        // Command Errored. Handle Here If Need Be
                    }

                    //Get The Rows
                    string[] rows = Regex.Split(content, "\r\n");
                    foreach (string row in rows)
                    {
                        //Split it baby
                        string[] tokens = Regex.Split(row, "\\s+");
                        if (tokens.Length > 4 && (tokens[1].Equals("UDP") || tokens[1].Equals("TCP")))
                        {
                            string localAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1");

                            string protocol = tokens[1];
                            string pid = protocol == "UDP" ? tokens[4] : tokens[5];


                            Ports.Add(new ProcessPort
                            {
                                Protocol = localAddress.Contains("1.1.1.1") ? String.Format("{0}v6", tokens[1]) : String.Format("{0}v4", tokens[1]),
                                PortNumber = localAddress.Split(':')[1],
                                ProcessName = LookupProcess(Convert.ToInt16(pid)),
                                ProcessPid = pid
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Ports;
        }

        public static string LookupProcess(int pid)
        {
            string procName;
            try { procName = Process.GetProcessById(pid).MainModule.FileName; }
            catch (Exception ex) 
            { 
                procName = "EXCEPTION";
                Console.WriteLine(ex.Message);
            }
            return procName;
        }


        public class ProcessPort
        {
            public string name
            {
                get
                {
                    return string.Format($"PID[{ProcessPid}]\t({Protocol}\tport:{PortNumber}\tname:{ProcessName})");
                }
                set { }
            }
            public string ProcessPid { get; set; }
            public string PortNumber { get; set; }
            public string ProcessName { get; set; }
            public string Protocol { get; set; }
        }
    }
}
