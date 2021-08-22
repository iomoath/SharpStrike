using System;
using System.Linq;
using System.Management;
using System.Management.Automation.Remoting;
using System.Reflection;
using System.Runtime.InteropServices;
using Models.CIM;

namespace ServiceLayer.CIM
{
    public class CommandHandler
    {
        #region Data Members

        private const string DefaultDebugFilePath = "%SystemRoot%\\MEMORY.DMP";

        private Planter _planter;

        private static Lazy<CommandHandler> _lazyInstance;

        public static CommandHandler Instance
        {
            get
            {
                if (_lazyInstance?.Value == null)
                    _lazyInstance = new Lazy<CommandHandler>(() => new CommandHandler());

                return _lazyInstance.Value;
            }
        }


        #endregion


        public void Handle(CommanderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));


            var commander = InitCommander(options);
            if (commander == null)
                return;

            _planter = InitPlanter(commander);
            if (_planter == null || (_planter.Connector?.ConnectedCimSession == null && _planter.Connector?.ConnectedWmiSession == null))
                return;

            // Init or reset debug path Win32_OSRecoveryConfiguration
            InitDebugPath();


            if (_planter.Commander.Command == null)
                return;



            // Block to check for the existence of Device Guard. If enabled, use PowerShell until I can find a better solution
            // If not enabled, install a WMI provider and use that method. This can be forced with the --provider flag
            // https://github.com/FortyNorthSecurity/CIMplant/blob/227c01f5e20bc53c6a0d5f75882b15235599dedc/CIMplant/Program.cs
            //bool credguard = false;
            //if (!options.Provider)
            //{
            //    try
            //    {
            //        credguard = options.Cim ? GetDeviceGuard.CheckDgCim(_planter.Connector.ConnectedCimSession) : GetDeviceGuard.CheckDgWmi(_planter.Connector.ConnectedWmiSession);
            //    }
            //    catch
            //    {
            //        Messenger.RedMessage("[-] Error when grabbing Device Guard info");
            //    }
            //}

            Messenger.GoodMessage("[+] Results from " + _planter.Commander.Command + ":\n");
            Execute();
        }

        private Commander InitCommander(CommanderOptions options)
        {
            Commander commander = null;

            if (options.Command != null && Commands.CommandArray.Any(options.Command.ToLower().Contains) || options.Reset)
            {
                try
                {
                    commander = new Commander(options);

                    if (commander.Method == null)
                        commander.Method = commander.Command;
                }
                catch (ArgumentNullException e)
                {
                    Messenger.RedMessage($"[-] Error: The parameter '{e.ParamName.ToLower()}' cannot be null (--{e.ParamName.ToLower()})");
                }
                catch (SubCommandException e)
                {

                    if (e.Message == "service_mod")
                    {
                        var m = "[-] Error: the sub-command for '{e.MessageData}' is incorrect. It must be list, create, start, or delete";
                        Messenger.RedMessage(m);
                    }
                }
                catch (ExtraCommandException)
                {
                    Messenger.RedMessage("[-] Error: Please only specify one command to execute using the -c or --command flag");
                }
                catch (ProcessCommandException)
                {
                    Messenger.RedMessage("[-] Error: Please specify a process or handle to kill (wildcards accepted for name, ex: --process note* or --process 5384)");
                }
                catch (Exception e)
                {
                    Messenger.RedMessage($"Exception {e.Message} Trace {e.StackTrace}");
                }
            }
            else
            {
                Messenger.RedMessage("\n[-] Incorrect command used. Try one of these:\n");
                Messages.GetCommands();
            }

            return commander;
        }

        private Planter InitPlanter(Commander commander)
        {
            var connector = new Connector();
            var planter =  new Planter(commander, connector);

            // Establish connection
            try
            {
                planter.Connector = new Connector(commander.Options.Cim, planter);

                // We can use && since one will always start as null
                if (planter.Connector.ConnectedWmiSession == null && planter.Connector.ConnectedCimSession == null)
                    planter = FallBack(planter);

                return planter;
            }
            catch (COMException e)
            {
                Messenger.RedMessage("\n[-] ERROR: Cannot connect to remote system, due to firewall or it being offline!");
                Messenger.RedMessage(e.Message);
                CloseConnection(planter);
                return FallBack(planter);
            }
            catch (UnauthorizedAccessException e)
            {
                Messenger.RedMessage("\n[-] ERROR: Access is denied, check the account you are using!");
                Messenger.RedMessage(e.Message);
                CloseConnection(planter);
            }
            catch (ManagementException e)
            {
                Messenger.RedMessage($"\n[-] ERROR: {e.Message}");
                Messenger.RedMessage(e.Message);
                CloseConnection(planter);

            }
            catch (Exception e)
            {
                Messenger.RedMessage("\n[-] ERROR: Something else went wrong, try a different protocol maybe");
                Messenger.RedMessage(e.Message);
                CloseConnection(planter);
            }

            return null;
        }

    
        private void CloseConnection(Planter p)
        {
            try
            {
                if (p?.Connector == null)
                    return;

                p.Connector.ConnectedCimSession?.Close();
                p.Connector.ConnectedWmiSession = null;
            }
            catch
            {
                //
            }
        }

        private void Execute()
        {
            ////////
            // Reflection Block
            ////////

            object result = null;

            // Block to set the specific Type for WMI/CIM command
            Type type = _planter.Commander.Options.Cim ? typeof(ExecuteCim) : typeof(ExecuteWmi);
            MethodInfo method = type.GetMethod(_planter.Commander.Method ?? _planter.Commander.Command);

            // Create an instance of the type
            object instance = Activator.CreateInstance(type);

            // Create parameter object
            object[] stringMethodParams = { _planter };

            try
            {
                if (!(method is null))
                    result = method.Invoke(instance, stringMethodParams);

                return;
            }

            catch (TargetInvocationException e)
            {
                if (e.InnerException != null && e.InnerException.Message == _planter.Commander.Service)
                {
                    Messenger.RedMessage($"[-] Error: The service name {_planter.Commander.Service} not valid, please ensure it's a valid service name (case sensitive)");
                }
                else if (e.InnerException != null && e.InnerException.Message == "System.Management.Automation.PropertyNotFoundException")
                {
                    Messenger.RedMessage("[-] Registry key does not exist or another issue occurred");
                }
            }
            catch (TimeoutException)
            {
                Messenger.Info(@"timeout hit");
            }
            catch (FormatException e)
            {
                Messenger.RedMessage("[-] The registry value for subkey " + _planter.Commander.RegSubKey + " is not in the correct format\n");
                Messenger.Info("Full error:\n" + e.Message);
            }
            catch (RektDebugFilePath)
            {
                // Good Sir or Madame reading this,
                // This only happens if something goes really, really wrong when using CIM.
                // We need to use WMI to reset the DebugFilePath if it's too large (above 512KB) unless we want to go through
                // A ton of mods to increase the maxEnvelopeSize within an administrative console
                // This should rarely happen but there's really no way around it
                Messenger.YellowMessage("[*] Something bad happened when resetting the DebugFilePath property. Using 'sudo'...");
               
                CloseConnection(_planter);


                if (_planter.Commander.Options.Fallback)
                {
                    if (_planter.Commander.Options.Cim)
                    {
                        _planter.Connector = new Connector(false, _planter);
                        _planter.Commander.Options.Cim = false;
                        ExecuteWmi.SetOsRecovery(_planter.Connector.ConnectedWmiSession, DefaultDebugFilePath);
                    }
                    else
                    {
                        _planter.Connector = new Connector(true, _planter);
                        _planter.Commander.Options.Cim = true;
                        ExecuteCim.SetOsRecovery(_planter.Connector.ConnectedCimSession, DefaultDebugFilePath);
                    }
                    Messenger.Info("\nDebugFilePath set back to the default Windows value\n");
                }

            }

            catch (PSRemotingTransportException)
            {
                // Pass, but we already caught above
            }

            catch (CaughtByAvException e)
            {
                Messenger.RedMessage("[-] Error: Issues with PowerShell script, it may have been flagged by AV");
                Messenger.Info($"Exception {e.Message} Trace {e.StackTrace}");
            }

            catch (Exception e)
            {
                Messenger.RedMessage($"Exception {e.Message} Trace {e.StackTrace}");
            }
            finally
            {
                CloseConnection(_planter);
            }

            if (result == null)
            {
                Messenger.RedMessage("\n[-] Issue running command after connecting. Try use other protocol CIM or WMI.");
                return;
            }


            Messenger.GoodMessage("\n\n[+] Successfully completed " + _planter.Commander.Options.Command + " command");
        }


        /// <summary>
        /// reset the DebugFilePath if needed
        /// </summary>
        private void InitDebugPath()
        {
            if (!_planter.Commander.Reset)
                return;


            if (!string.IsNullOrEmpty(_planter.Commander.Options.Command))
            {
                Messenger.Info(@"Please don't specify -r/--reset with -c/--command, the reset is redundant");
            }

            ResetDebugFilePathToDefault();
        }


        private void ResetDebugFilePathToDefault()
        {
            try
            {
                if (_planter.Commander.Options.Cim)
                    ExecuteCim.SetOsRecovery(_planter.Connector.ConnectedCimSession, DefaultDebugFilePath);
                else
                    ExecuteWmi.SetOsRecovery(_planter.Connector.ConnectedWmiSession, DefaultDebugFilePath);


                Messenger.Info("\nDebugFilePath set back to the default Windows value\n");
            }
            catch (RektDebugFilePath)
            {
                // Good Sir or Madame reading this,
                // This only happens if something goes really, really wrong when using CIM.
                // We need to use WMI to reset the DebugFilePath if it's too large (above 512KB) unless we want to go through
                // A ton of mods to increase the maxEnvelopeSize within an administrative console
                // This should rarely happen but there's really no way around it
                Messenger.YellowMessage("[*] Something bad happened when resetting the DebugFilePath property. Using 'sudo'...");
                CloseConnection(_planter);

                if (_planter.Commander.Options.Fallback)
                {
                    if (_planter.Commander.Options.Cim)
                    {
                        _planter.Connector = new Connector(false, _planter);
                        _planter.Commander.Options.Cim = false;
                        ExecuteWmi.SetOsRecovery(_planter.Connector.ConnectedWmiSession, DefaultDebugFilePath);
                    }
                    else
                    {
                        _planter.Connector = new Connector(true, _planter);
                        _planter.Commander.Options.Cim = true;
                        ExecuteCim.SetOsRecovery(_planter.Connector.ConnectedCimSession, DefaultDebugFilePath);
                    }

                    Messenger.Info("\nDebugFilePath set back to the default Windows value\n");
                }
            }
            catch (Exception e)
            {
                Messenger.RedMessage("[-] Issue resetting DebugFilePath\n\n");
                //Messenger.Info($"Exception {e.Message} Trace {e.StackTrace}");
                Messenger.RedMessage(e.Message);

                if (_planter.Commander.Options.Fallback)
                {
                    if (_planter.Commander.Options.Cim)
                    {
                        _planter.Connector = new Connector(false, _planter);
                        _planter.Commander.Options.Cim = false;
                        ExecuteWmi.SetOsRecovery(_planter.Connector.ConnectedWmiSession, DefaultDebugFilePath);
                    }
                    else
                    {
                        _planter.Connector = new Connector(true, _planter);
                        _planter.Commander.Options.Cim = true;
                        ExecuteCim.SetOsRecovery(_planter.Connector.ConnectedCimSession, DefaultDebugFilePath);
                    }

                    Messenger.Info("\nDebugFilePath set back to the default Windows value\n");
                }
            }
        }


        private Planter FallBack(Planter planter)
        {
            // We can use && since one will always start as null
            if (planter.Connector.ConnectedCimSession == null && planter.Connector.ConnectedWmiSession == null)
            {
                if (planter.Commander.Options.Fallback)
                {

                    //planter.Commander.Options.Wmi = !planter.Commander.Options.Wmi;
                    Messenger.RedMessage("[-] Issue with using the selected protocol, falling back to the other");

                    if (planter.Commander.Options.Cim)
                    {
                        planter.Connector = new Connector(false, planter);
                        planter.Commander.Options.Cim = false;
                    }
                    else
                    {
                        planter.Connector = new Connector(true, planter);
                        planter.Commander.Options.Cim = true;
                    }

                    if (planter.Connector.ConnectedCimSession == null && planter.Connector.ConnectedWmiSession == null)
                    {
                        Messenger.RedMessage("[-] ERROR: Unable to connect using either CIM or WMI.");
                    }

                    return planter;
                }

                Messenger.RedMessage("[-] Issue with using the selected protocol.");
                CloseConnection(planter);
                return planter;
            }
            return planter;
        }


    }
}

