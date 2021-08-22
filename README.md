<div align="center">
	<h1>SharpStrike</h1>
	<br/>
</div>

SharpStrike is a post-exploitation tool written in C# that uses either CIM or WMI to query remote systems. It can use provided credentials or the current user's session.

Note: Some commands will use PowerShell in combination with WMI, denoted with ** in the `--show-commands` command.

## Introduction

SharpStrike is a C# rewrite and expansion on [@Matt_Grandy_](https://twitter.com/Matt_Grandy_)'s [CIMplant](https://github.com/FortyNorthSecurity/CIMplant) and [@christruncer](https://twitter.com/christruncer)'s [WMImplant](https://github.com/FortyNorthSecurity/WMImplant). 

SharpStrike allows you to gather data about a remote system, execute commands, exfil data, and more. The tool allows connections using Windows Management Instrumentation, [WMI](https://docs.microsoft.com/en-us/windows/win32/wmisdk/about-wmi), or Common Interface Model, [CIM](https://www.dmtf.org/standards/cim) ; well more accurately Windows Management Infrastructure, [MI](https://docs.microsoft.com/en-us/previous-versions/windows/desktop/wmi_v2/windows-management-infrastructure). CIMplant requires local administrator permissions on the target system.


## Setup:

It's probably easiest to use the built version under Releases, just note that it is compiled in Debug mode. If you want to build the solution yourself, follow the steps below.

1. Load SharpStrike.sln into Visual Studio
2. Go to Build at the top and then Build Solution if no modifications are wanted

The Build will produce two versions of SharpStrike: GUI (WinForms) & Console application. Each version implements the same features.


## Usage

```
Console Version:

SharpStrike.exe --help
SharpStrike.exe --show-commands
SharpStrike.exe --show-examples
SharpStrike.exe -c ls_domain_admins
SharpStrike.exe -c ls_domain_users_list
SharpStrike.exe -c cat -f "c:\users\user\desktop\file.txt" -s [remote IP address]
SharpStrike.exe -c cat -f "c:\users\user\desktop\file.txt" -s [remote IP address] -u [username] -d [domain] -p [password] -c 
SharpStrike.exe -c command_exec -e "quser" -s [remote IP address] -u [username] -d [domain] -p [password]

GUI version:

show-commands
show-examples
ls_domain_admins
ls_domain_users_list
cat -f "c:\users\user\desktop\file.txt" -s [remote IP address]
cat -f "c:\users\user\desktop\file.txt" -s [remote IP address] -u [username] -d [domain] -p [password]
command_exec -e "quser" [remote IP address] -u [username] -d [domain] -p [password]
```

## Functions

### File Operations:
    cat                          -  Reads the contents of a file
    copy                         -  Copies a file from one location to another
    download**                   -  Download a file from the targeted machine
    ls                           -  File/Directory listing of a specific directory
    search                       -  Search for a file on a user
    upload**                     -  Upload a file to the targeted machine

### Lateral Movement Facilitation
    command_exec**               -  Run a command line command and receive the output. Run with nops flag to disable PowerShell
    disable_wdigest              -  Sets the registry value for UseLogonCredential to zero
    enable_wdigest               -  Adds registry value UseLogonCredential
    disable_winrm**              -  Disables WinRM on the targeted system
    enable_winrm**               -  Enables WinRM on the targeted system
    reg_mod                      -  Modify the registry on the targeted machine
    reg_create                   -  Create the registry value on the targeted machine
    reg_delete                   -  Delete the registry on the targeted machine
    remote_posh**                -  Run a PowerShell script on a remote machine and receive the output
    sched_job                    -  Not implimented due to the Win32_ScheduledJobs accessing an outdated API
    service_mod                  -  Create, delete, or modify system services
    ls_domain_users***           - List domain users                                 
    ls_domain_users_list***      - List domain users sAMAccountName                  
    ls_domain_users_email***     - List domain users email address                   
    ls_domain_groups***          - List domain user groups                           
    ls_domain_admins***          - List domain admin users                           
    ls_user_groups***            - List domain user with their associated groups


#### Process Operations
    process_kill                 -  Kill a process via name or process id on the targeted machine
    process_start                -  Start a process on the targeted machine
    ps                           -  Process listing

### System Operations
    active_users                 -  List domain users with active processes on the targeted system
    basic_info                   -  Used to enumerate basic metadata about the targeted system
    drive_list                   -  List local and network drives
    ifconfig                     -  Receive IP info from NICs with active network connections
    installed_programs           -  Receive a list of the installed programs on the targeted machine
    logoff                       -  Log users off the targeted machine
    reboot (or restart)          -  Reboot the targeted machine
    power_off (or shutdown)      -  Power off the targeted machine
    vacant_system                -  Determine if a user is away from the system
    edr_query                    -  Query the local or remote system for EDR vendors

### Log Operations
    logon_events                 -  Identify users that have logged onto a system

    * All PowerShell can be disabled by using the --nops flag, although some commands will not execute (upload/download, enable/disable WinRM)
    ** Denotes PowerShell usage (either using a PowerShell Runspace or through Win32_Process::Create method)
    *** Denotes LDAP usage - "root\directory\ldap" namespace

### Some Example Usage Commands

Console version:
![SharpStrike-Console](Extras/SharpStrike-Usage.gif?raw=true)


GUI version:
![SharpStrike-GUI](Extras/SharpStrike-GUI.png?raw=true)



## Solution Architecture
SharpStrike is composed of three main projects
1. ServiceLayer -- Provides core functionality and consumed by the UI layer
2. Models -- Contains types, shared across all projects
3. User Interface -- GUI/Console

### ServiceLayer
1. Connector.cs
> This is where the initial CIM/WMI connections are made and passed to the rest of the application

2. ExecuteWMI.cs
> All function code for the WMI commands

3. ExecuteCIM.cs
> All function code for the CIM (MI) commands




### Read more
[CIMplant Part 1: Detection of a C# Implementation of WMImplant](https://fortynorthsecurity.com/blog/cimplant-part-1-detections/)
[WMImplant – A WMI Based Agentless Post-Exploitation RAT Developed in PowerShell](https://www.fireeye.com/blog/threat-research/2017/03/wmimplant_a_wmi_ba.html)
[https://c99.sh/sharpstrike-post-exploitation-tool-cim-wmi-inside/](https://c99.sh/sharpstrike-post-exploitation-tool-cim-wmi-inside/)