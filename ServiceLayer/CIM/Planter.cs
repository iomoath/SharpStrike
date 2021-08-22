using System.Security;

namespace ServiceLayer.CIM
{
    public class Planter
    {
        public string System, NameSpace, Domain, User;
        public SecureString Password;
        public Commander Commander;
        public Connector Connector;


        public Planter(Commander commander, Connector connector)
        {
            Commander = commander;
            Connector = connector;

            System = Commander.Options.System;
            Domain = Commander.Options.Domain;
            User = Commander.Options.Username;
            Password = CreateSecuredString(Commander.Options.Password);
            NameSpace = Commander.Options.NameSpace;
        }

        public static SecureString CreateSecuredString(string pw)
        {
            var secureString = new SecureString();

            if (string.IsNullOrEmpty(pw))
                return null;

            foreach (char c in pw)
                secureString.AppendChar(c);
            return secureString;
        }
    }
}
