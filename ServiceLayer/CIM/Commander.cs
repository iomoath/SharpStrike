using System;
using System.Linq;
using Models.CIM;

namespace ServiceLayer.CIM
{
    public class Commander
    {
        #region Members

        public string Command { get; set; }

        public string Execute { get; set; }

        public string File { get; set; }

        public string Cmdlet { get; set; }

        public string FileTo { get; set; }

        public string Directory { get; set; }


        public string RegKey { get; set; }

        public string RegSubKey { get; set; }

        public string RegVal { get; set; }

        public string RegValType { get; set; }

        public string Service { get; set; }

        public string ServiceBin { get; set; }

        public string Method { get; set; }

        public string Process { get; set; }

        public bool Reset { get; set; }

        public bool NoPS { get; set; }

        public CommanderOptions Options { get; }

        #endregion


        public Commander(CommanderOptions options)
        {
            Options = options ??
                      throw new ArgumentNullException(nameof(options));
            
            RegKey = options.RegistryKey;
            RegSubKey = options.RegistrySubkey;
            RegVal = options.RegistryValue;
            Execute = options.Execute;
            RegValType = options.RegistryValueType;
            Service = options.Service;
            ServiceBin = options.ServiceBin;
            Cmdlet = options.Cmdlet;
            File = options.File?.Trim();
            FileTo = options.FileTo?.Trim();
            Directory = options.Directory?.Trim();
            Reset = options.Reset;
            Process = options.Process;
            Method = null;
            NoPS = options.NoPS;
            Command = options.Command;

            if (!string.IsNullOrEmpty(Directory) && !string.IsNullOrWhiteSpace(Directory))
            {
                if (Directory.EndsWith("\""))
                    Directory = Directory.Remove(Directory.Length - 1, 1);
            }




            ParseCommands();
        }


        private void ParseCommands()
        {
            if (string.IsNullOrEmpty(Command) || string.IsNullOrWhiteSpace(Command))
                return;

            // Commands that do not require arguments parsing
            // Nothing require, avoid other checks.
            if (!RequireParsing(Command))
                return;
            

            if (Command == "cat")
                _ = File ?? throw new ArgumentNullException(nameof(File));

            if (Commands.Shutdown.Any(Command.Contains))
            {
                //This method is the same for all 3 types of commands in the shutdown array, so use this one and pass it the command
                Method = "Win32Shutdown";
            }

            if (Commands.RegistryModify.Any(Command.Contains))
            {
                Method = "registry_mod";
                switch (Command)
                {
                    case "reg_mod" when RegKey != null &&
                                        RegSubKey != null &&
                                        RegVal != null:
                        Method = "registry_mod";
                        break;
                    case "reg_delete" when RegKey != null &&
                                           RegSubKey != null:
                        Method = "registry_mod";
                        break;
                    case "reg_create" when RegKey != null &&
                                           RegSubKey != null &&
                                           RegVal != null &&
                                           RegValType != null:
                        Method = "registry_mod";
                        break;
                    default:
                        {
                            _ = RegKey ?? throw new ArgumentNullException(nameof(RegKey));
                            _ = RegSubKey ?? throw new ArgumentNullException(nameof(RegSubKey));
                            _ = RegVal ?? throw new ArgumentNullException(nameof(RegVal));
                            _ = RegValType ?? throw new ArgumentNullException(nameof(RegValType));
                        }
                        break;
                }
            }

            if (Command == "service_mod")
            {
                // All commands need this val so check first
                _ = Execute ?? throw new ArgumentNullException(nameof(Execute));

                if (Commands.ServiceSubCommand.Any(Execute.Contains))
                {
                    switch (Execute)
                    {
                        case "list":
                            break;
                        case "create" when Service != null &&
                                           ServiceBin != null:
                            break;
                        case "start" when Service != null:
                            break;
                        case "delete" when Service != null:
                            break;
                        default:
                            {
                                _ = Service ?? throw new ArgumentNullException(nameof(Service));
                                _ = ServiceBin ?? throw new ArgumentNullException(nameof(ServiceBin));
                            }
                            break;
                    }
                }
                else
                {
                    throw new SubCommandException("service_mod");
                }
            }

            if (Commands.ProcessCommand.Any(Command.Contains))
            {
                switch (Command)
                {
                    case "ps":
                        break;
                    case "process_kill" when Process != null:
                        break;
                    case "process_start" when Process != null:
                        break;
                    default:
                        {
                            _ = Process ?? throw new ArgumentNullException(nameof(Process));
                        }
                        break;
                }
            }

            if (Commands.FileCommand.Any(Command.Contains))
            {
                switch (Command)
                {
                    case "ls":
                        _ = Directory ?? throw new ArgumentNullException(nameof(Directory));
                        break;
                    case "cat" when File != null:
                        break;
                    case "copy" when File != null &&
                                     FileTo != null:
                        break;
                    case "download" when File != null:
                        break;
                    case "search" when File != null:
                        _ = Directory ?? throw new ArgumentNullException(nameof(Directory));
                        break;
                    case "upload" when File != null &&
                                     FileTo != null:
                        break;
                    default:
                        {
                            _ = File ?? throw new ArgumentNullException(nameof(File));
                            _ = FileTo ?? throw new ArgumentNullException(nameof(FileTo));
                            _ = Directory ?? throw new ArgumentNullException(nameof(Directory));
                        }
                        break;
                }
            }

            if (Commands.LateralMovement.Any(Command.Contains))
            {
                switch (Command)
                {
                    case "command_exec" when Execute != null:
                        break;

                    case "remote_posh":
                        _ = File ?? throw new ArgumentNullException(nameof(File));
                        break;
                    case "disable_wdigest":
                    case "enable_wdigest":
                    case "disable_winrm":
                    case "enable_winrm":
                    case "ls_domain_users":
                        break;
                    default:
                        _ = Execute ?? throw new ArgumentNullException(nameof(Execute));
                        break;
                }
            }
        }

        private bool RequireParsing(string command)
        {
            if (command == "ls_domain_users")
                return false;
            if (command == "ls_domain_users_list")
                return false;
            if (Command == "ls_domain_users_email")
                return false;
            if (Command == "ls_domain_groups")
                return false;
            if (Command == "ls_user_groups")
                return false;
            if (Command == "domain_lockout_policy")
                return false;
            if (Command == "ls_computers")
                return false;
            if (Command == "ls_domain_admins")
                return false;
            return true;
        }

    }
}
