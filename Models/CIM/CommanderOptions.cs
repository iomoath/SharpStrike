using CommandLine;

namespace Models.CIM
{
    public class CommanderOptions
    {
        // Command line options
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose")]
        public bool Verbose { get; set; }


        [Option('u', "username", Required = false, HelpText = "Specify a username to use")]
        public string Username { get; set; }


        [Option('p', "password", Required = false, HelpText = "Specify a password to use", Default = null)]
        public string Password { get; set; }


        [Option('d', "domain", Required = false, HelpText = "Specify a domain", Default = "WORKGROUP")]
        public string Domain { get; set; }


        [Option('s', "system", Group = "Required", Required = true, HelpText = "Specify a system to target", Default = "localhost")]
        public string System { get; set; }


        [Option('n', "namespace", Required = false, HelpText = "Specify a namespace to use", Default = "root\\cimv2")]
        public string NameSpace { get; set; }


        [Option('c', "command", Group = "Command", Required = true, HelpText = "Specify a command to run, run program with just '--show-commands' for a list of commands")]
        public string Command { get; set; }


        [Option('r', "reset", Group = "Command", Required = true, HelpText = "Reset the DebugFilePath property back to the Windows default in the event of any execution errors")]
        public bool Reset { get; set; }


        [Option('e', "execute", Required = false, HelpText = "Specify a command-line command to execute and receive the output for (use double quotes \"command\" for complex commands)")]
        public string Execute { get; set; }


        [Option('f', "file", Group = "Required", Required = true, HelpText = "Specify a remote or local file to cat/download/copy/search for/execute ps1/etc.", Default = null)]
        public string File { get; set; }


        [Option("cmdlet", Group = "Required", Required = true, HelpText = "Specify a cmdlet to run and obtain the results for", Default = null)]
        public string Cmdlet { get; set; }


        [Option("fileto", Group = "Required", Required = true, HelpText = "Specify a name to copy the file to", Default = null)]
        public string FileTo { get; set; }


        [Option('l', "directory", Group = "Required", Required = true, HelpText = "Specify a directory to list/search", Default = null)]
        public string Directory { get; set; }

        
        [Option("regkey", Group = "Required", Required = true, HelpText =  "Specify a registry key to create/delete/modify (ex: HKLM\\SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\WDigest)", Default = null)]
        public string RegistryKey { get; set; }


        [Option("regsubkey", Group = "Required", Required = true, HelpText = "Specify a registry subkey to create/delete/modify (ex: UseLogonCredential)", Default = null)]
        public string RegistrySubkey { get; set; }


        [Option("regval", Group = "Required", Required = true, HelpText = "Specify a registry data value to create/delete/modify (ex: \"1\" for REG_DWORD)", Default = null)]
        public string RegistryValue { get; set; }


        [Option("regvaltype", Group = "Required", Required = true, HelpText = "Specify a registry data type to create/delete/modify (case insensitive, ex: REG_DWORD, reg_binary, or reg_sz)", Default = null)]
        public string RegistryValueType { get; set; }


        [Option("service", Group = "Required", Required = true, HelpText = "Specify a service name to create/delete/start/stop", Default = null)]
        public string Service { get; set; }


        [Option("servicebin", Group = "Required", Required = true, HelpText = "Specify a service binary while creating a new service", Default = null)]
        public string ServiceBin { get; set; }


        [Option("process", Group = "Required", Required = true, HelpText = "Specify a process name or handle to kill or start (wildcards accepted for name)", Default = null)]
        public string Process { get; set; }


        [Option("cim", Required = false, HelpText = "Use CIM/MI (WSMan) to connect to the remote system instead of WMI (DCOM)", Default = false)]
        public bool Cim { get; set; }


        [Option("enable-fallback", Required = false, HelpText = "Fallback to WMI (DCOM) or CIM/MI (WSMan) when connection fails.", Default = true)]
        public bool Fallback { get; set; }


        [Option("provider", Required = false, HelpText = "Use InstallUtil to register a WMI provider (Not Currently Working)", Default = false)]
        public bool Provider { get; set; }


        [Option("nops", Required = false, HelpText = "Do not allow any PowerShell execution (will die before)", Default = false)]
        public bool NoPS { get; set; }


        [Option("show-commands", Group = "Command", Required = true, HelpText = "Displays a list of available commands")]
        public bool ShowCommands { get; set; }


        [Option("show-examples", Group = "Command", Required = true, HelpText = "Displays examples for all available commands")]
        public bool ShowExamples { get; set; }


        [Option("test", Group = "Command", Required = false, HelpText = "Tests all commands with a specified username/password/system (or against the localhost)")]
        public bool Test { get; set; }
    }
}
