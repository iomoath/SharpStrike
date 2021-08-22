using System;
using System.Management;
using System.Security;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using Models.CIM;

namespace ServiceLayer.CIM
{
    public class Connector
    {
        public CimSession ConnectedCimSession { get; set; }

        public ManagementScope ConnectedWmiSession { get; set; }

        public string SystemToConn { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }
        public string Domain { get; set; }


        public Connector()
        {

        }

        public Connector(bool cim, Planter planter)
        {
            if (cim)
            {
                ConnectedCimSession = DoCimConnection(planter);
            }
            else
            {
                ConnectedWmiSession = DoWmiConnection(planter);
            }
        }

        private CimSession DoCimConnection(Planter planter)
        {
            //Block for connecting to the remote system and returning a CimSession object
            SystemToConn = planter.System;
            Domain = planter.Domain;
            Username = planter.User;
            Password = planter.Password;
            var sessionOptions = new WSManSessionOptions();
            CimSession connectedCimSession;

            if (string.IsNullOrEmpty(SystemToConn)) 
                SystemToConn = "localhost";

            if (string.IsNullOrEmpty(Username))
                Username = Environment.UserName;


            switch (SystemToConn)
            {
                case "127.0.0.1":
                case "localhost":
                    Messenger.GoodMessage($"[+] Connecting to local CIM instance using {Username} ...");
                    break;
                default:
                    Messenger.GoodMessage($"[+] Connecting to remote CIM instance using {Username} ...");
                    break;
            }

            if (!string.IsNullOrEmpty(Password?.ToString()))
            {
                // create Credentials
                var credentials = new CimCredential(PasswordAuthenticationMechanism.Default,  Domain, Username, Password);
                sessionOptions.AddDestinationCredentials(credentials);
                sessionOptions.MaxEnvelopeSize = 256000; // Not sure how else to get around this
                connectedCimSession = CimSession.Create(SystemToConn, sessionOptions);

            }

            else
            {
                var options = new DComSessionOptions { Impersonation = ImpersonationType.Impersonate };
                connectedCimSession = CimSession.Create(SystemToConn, options);
            }

            // Test connection to make sure we're connected
            if (!connectedCimSession.TestConnection(out _, out _))
            {
                return null;
            }


            Messenger.GoodMessage("[+] Connected");
            return connectedCimSession;
        }

        private ManagementScope DoWmiConnection(Planter planter)
        {
            //Block for connecting to the remote system and returning a ManagementScope object
            SystemToConn = planter.System;
            Domain = planter.Domain;
            Username = planter.User;
            Password = planter.Password;


            ConnectionOptions options = new ConnectionOptions();

            if (string.IsNullOrEmpty(SystemToConn))
                SystemToConn = "localhost";

            if (string.IsNullOrEmpty(Username))
                Username = Environment.UserName;


            switch (SystemToConn)
            {
                case "127.0.0.1":
                case "localhost":
                    Messenger.GoodMessage($"[+] Connecting to local WMI instance using {Username} ...");
                    break;
                default:
                    Messenger.GoodMessage($"[+] Connecting to remote WMI instance using {Username} ...");
                    break;
            }

            if (!string.IsNullOrEmpty(Password?.ToString()))
            {
                options.Username = Username;
                options.SecurePassword = Password;
                options.Authority = "ntlmdomain:" + Domain;
                options.Impersonation = ImpersonationLevel.Impersonate;
                options.EnablePrivileges = true; // This may be ok for all or may not, need to verify

            }
            else
            {
                options.Impersonation = ImpersonationLevel.Impersonate;
                options.EnablePrivileges = true;
            }

            ManagementScope scope = new ManagementScope(@"\\" + SystemToConn + $@"\{planter.NameSpace}", options);
            //ManagementScope deviceguard = new ManagementScope(@"\\" + System + @"\root\Microsoft\Windows\DeviceGuard", options);
            // Need to create a second MS object since we use a separate namespace. 
            //! Need to find a more elegant solution to this!

            scope.Connect();
            //deviceguard.Connect();

            // We'll need this when we get the provider going so we can check for DG
            //ManagementScope deviceScope = scope.Clone();
            //deviceScope.Path = new ManagementPath(@"\\" + System + @"\root\Microsoft\Windows\DeviceGuard");
            //if (GetDeviceGuard.CheckDgWmi(scope, planter.System))
            //    Console.WriteLine("deviceguard enabled");


            Messenger.GoodMessage("[+] Connected");
            
            //return Tuple.Create(scope, deviceguard);
            return scope;
        }

    }
}
