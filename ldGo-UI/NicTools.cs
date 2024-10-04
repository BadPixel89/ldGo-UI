using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ldGoUI
{
    struct SwitchData
    {
        public string Protocol;
        public string SwitchName;
        public string Interface;
        public string VLAN;
        public string SwitchModel;
        public string VTPDomain;
    }
    internal class NicTools
    {
        public NetworkInterface[] Nics { get; set; }
        public string[] GetNicNames()
        {
            LoadNics();
            List<string> names = new List<string>();

            foreach (NetworkInterface nic in Nics)
            {
                string textBoxNicState = "";
                if (nic != null && !nic.Name.ToLower().Contains("pseudo"))
                {
                    foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork && nic.OperationalStatus == OperationalStatus.Up)
                        {
                            textBoxNicState = "[✓] ";
                            break;
                        }
                        else
                        {
                            textBoxNicState = "[⨯] ";
                        }
                    }
                    names.Add(textBoxNicState + nic.Name);
                }
            }
            return names.ToArray();
        }
        public void LoadNics()
        {
            Nics = NetworkInterface.GetAllNetworkInterfaces();
        }
        public string ConvertMac(string mac)
        {
            string convertedMac = "";

            int addColon = -1;
            for (int i = 0; i < mac.Length; i++)
            {
                addColon++;
                if (addColon == 2)
                {
                    convertedMac += ":";
                    addColon = 0;
                }
                convertedMac += mac[i];
            }
            return convertedMac;
        }
        public SwitchData GetSwitchDataFromFile(string filename)
        {
            SwitchData d = new SwitchData();

            using (StreamReader sr = new StreamReader(filename))
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                d = (SwitchData)serializer.Deserialize(sr, typeof(SwitchData));
            }

            return d;
        }
        public string GetStringOfStructValues(SwitchData switchData)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(switchData.Protocol + "\n");
            sb.Append(switchData.SwitchName + "\n");
            sb.Append(switchData.Interface + "\n");
            sb.Append(switchData.VLAN + "\n");
            sb.Append(switchData.SwitchModel + "\n");
            sb.Append(switchData.VTPDomain);
            return sb.ToString();
        }
        public string GetStringOfStructNamesValues(SwitchData switchData)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Protocol: "     + switchData.Protocol + "\n");
            sb.Append("Switch Name: "  + switchData.SwitchName + "\n");
            sb.Append("Interface: "    + switchData.Interface + "\n");
            sb.Append("VLAN: "         + switchData.VLAN + "\n");
            sb.Append("Switch Model: " + switchData.SwitchModel + "\n");
            sb.Append("VTP Domain: "   + switchData.VTPDomain);
            return sb.ToString();
        }
    }
}
