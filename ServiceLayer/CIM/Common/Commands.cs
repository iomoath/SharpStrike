using System;
using System.Collections.Generic;

namespace ServiceLayer.CIM
{
    public class Commands
    {
        public static readonly string DefaultNameSpace = "root\\cimv2";

        public static readonly string[] CommandArray =
        {
            "cat",
            "copy",
            "download",
            "ls",
            "search",
            "upload",
            "command_exec",
            "process_kill",
            "process_start",
            "ps",
            "active_users",
            "basic_info",
            "drive_list",
            "ifconfig",
            "installed_programs",
            "logoff",
            "reboot",
            "restart",
            "power_off",
            "shutdown",
            "vacant_system",
            "logon_events",
            "command_exec",
            "disable_wdigest",
            "enable_wdigest",
            "disable_winrm",
            "enable_winrm",
            "reg_mod",
            "reg_create",
            "reg_delete",
            "remote_posh",
            "sched_job",
            "service_mod",
            "edr_query",

            "share_list",
            "ls_domain_users",
            "ls_domain_users_list",
            "ls_domain_users_email",
            "ls_domain_groups",
            "ls_computers",
            "ls_domain_admins"

        };

        public static readonly string[] Shutdown =
        {
            "logoff",
            "reboot",
            "restart",
            "power_off",
            "shutdown"
        };

        public static readonly string[] FileCommand =
        {
            "cat",
            "copy",
            "download",
            "ls",
            "search",
            "upload"
        };

        public static readonly string[] LateralMovement =
        {
            "command_exec",
            "disable_wdigest",
            "enable_wdigest",
            "disable_winrm",
            "enable_winrm",
            "remote_posh",
            "sched_job",
            "service_mod",

            "ls_domain_users",
            "ls_domain_users_list",
            "ls_domain_users_email",
            "ls_domain_groups",
            "ls_computers",
            "ls_user_groups"
        };

        public static readonly string[] RegistryModify =
        {
            "reg_mod",
            "reg_create",
            "reg_delete"
        };

        public static readonly string[] ServiceSubCommand =
        {
            "list",
            "start",
            "stop",
            "create",
            "delete"
        };

        public static readonly string[] ProcessCommand =
        {
            "ps",
            "process_kill",
            "process_start"
        };



        public static Dictionary<string, string> CommandNameSpace = new Dictionary<string, string>
        {
            {"ls_domain_users", "root\\directory\\ldap"},
            {"ls_domain_users_list", "root\\directory\\ldap"},
            {"ls_domain_users_email", "root\\directory\\ldap"},
            {"ls_domain_groups", "root\\directory\\ldap"},
            {"ls_computers", "root\\directory\\ldap"},
            {"ls_domain_admins", "root\\directory\\ldap"},
            {"ls_user_groups", "root\\directory\\ldap"},
        };

        public static string GetCommandNameSpace(string command)
        {
            if (string.IsNullOrEmpty(command))
                return DefaultNameSpace;


            if (CommandNameSpace.ContainsKey(command))
                return CommandNameSpace[command];

            return DefaultNameSpace;
        }

    }
}
