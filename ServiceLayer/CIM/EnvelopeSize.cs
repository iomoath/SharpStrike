using System;
using Microsoft.Management.Infrastructure;
using Microsoft.Win32;

namespace ServiceLayer.CIM
{

    public class EnvelopeSize
    {
        // Let's get the maxEnvelopeSize if it's set something other than default
        public static string GetLocalMaxEnvelopeSize()
        {
            //Messenger.YellowMessage("[*] Getting the MaxEnvelopeSizeKB on the local system to reset later");
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WSMAN\Client"))
                {
                    return key?.GetValue("maxEnvelopeSize")?.ToString();
                }
            }
            catch (Exception e)
            {
                Messenger.RedMessage("[-] Error: Unable to create local runspace to change maxEnvelopeSizeKB.\n");
                Messenger.Info(e.Message);
                return "0";
            }
        }

        public static string GetMaxEnvelopeSize(CimSession cimSession)
        {
            CimMethodResult result = RegistryMod.CheckRegistryCim("GetDWORDValue", 0x80000002, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WSMAN\Client", "maxEnvelopeSize", cimSession);
            if (Convert.ToUInt32(result.ReturnValue.Value.ToString()) == 0)
            {
                return result.OutParameters["uValue"].Value.ToString();
            }

            Messenger.RedMessage("Issues getting maxEnvelopeSize");
            return "0";
        }

        public static void SetLocalMaxEnvelopeSize(int envelopeSize)
        {
            Messenger.YellowMessage("[*] Setting the MaxEnvelopeSizeKB on the local system to " + envelopeSize);

            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WSMAN\Client"))
                {
                    key?.SetValue("maxEnvelopeSize", Convert.ToUInt32(envelopeSize), RegistryValueKind.DWord);
                }
            }
            catch (Exception e)
            {
                Messenger.RedMessage("[-] Error: Unable to create local runspace to change maxEnvelopeSizeKB.\n");
                Messenger.Info(e.Message);
            }
        }

        public static void SetMaxEnvelopeSize(string envelopeSize, CimSession cimSession)
        {
            Messenger.YellowMessage("[*] Setting the MaxEnvelopeSizeKB on the remote system to " + envelopeSize);

            CimMethodResult result = RegistryMod.SetRegistryCim("SetDWORDValue",  0x80000002,
                 @"SOFTWARE\Microsoft\Windows\CurrentVersion\WSMAN\Client",
                "maxEnvelopeSize", envelopeSize, cimSession);
            if (Convert.ToUInt32(result.ReturnValue.Value.ToString()) == 0)
            {
            }
            else
            {
                Messenger.Info("Issues setting maxEnvelopeSize");
            }
        }
    }

}
