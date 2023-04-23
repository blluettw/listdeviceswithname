using Microsoft.Win32;
using ListDevices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ListDevices
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static bool IsRunAsAdmin()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            if (!IsRunAsAdmin())
            {
                MessageBox.Show("Por favor execute o app como administrador!");
                Close();
            }
            string[] portNames = SerialPort.GetPortNames();

            this.RefreshDevices();
        }


      

        private string VID2 = string.Empty;

        private string PID2 = string.Empty;

      

        private static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> list = new List<USBDeviceInfo>();
            ManagementObjectCollection managementObjectCollection;
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PointingDevice"))
            {
                managementObjectCollection = managementObjectSearcher.Get();
            }
            foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
            {
                string text = (string)managementBaseObject.GetPropertyValue("Caption");
                list.Add(new USBDeviceInfo((string)managementBaseObject.GetPropertyValue("DeviceID"), (string)managementBaseObject.GetPropertyValue("DeviceID"), text, GetUSBDeviceManufacturer(text)));
            }
            managementObjectCollection.Dispose();
            return list;
        }

        private async void RefreshDevices()
        {
            string deviceName = "";
            List<USBDeviceInfo> usbdevices = GetUSBDevices();
            foreach (USBDeviceInfo usbdeviceInfo in usbdevices)
            {
                this.VID2 = usbdeviceInfo.DeviceID.Substring(8, 4);
                this.PID2 = usbdeviceInfo.PnpDeviceID.Substring(17, 4).Trim();

              
                if (usbdeviceInfo.DeviceID.Contains("HID\\VIRTUALDEVICE&10"))
                {
                    continue;
                }

       
                deviceName = await GetUSBDeviceNameAsync(this.VID2, this.PID2);
                int pidIndex = deviceName.IndexOf("(" + this.PID2 + ")");
                if (pidIndex != -1)
                {
                    deviceName = deviceName.Remove(pidIndex, 9);
                }

            
                if (deviceName.ToLower().Contains("virtual") || deviceName.ToLower().Contains("emulated"))
                {
                    continue;
                }


                this.comboBox1.Items.Add($"{deviceName.Trim()} - VID: {this.VID2} PID: {this.PID2}");
            }
        }


        private async Task<string> GetUSBDeviceNameAsync(string vid, string pid)
        {
            var httpClient = new HttpClient();
            try
            {
                string url = $"https://gist.githubusercontent.com/blluettw/ff8a63039459839915e098dc6a459119/raw/2e5943e642d3c337fb046a470fa483fda86a01b0/gistfile1.txt";
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                string[] lines = content.Split('\n');
                string deviceLine = null;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains($"{pid}"))
                    {
                        deviceLine = lines[i];
                        break;
                    }
                }
                if (deviceLine != null)
                {
                    int nameStartIndex = deviceLine.IndexOf('\t') + 1;
                    string deviceName = deviceLine.Substring(nameStartIndex).Substring(4);
                    return deviceName;
                }
                else
                {
                    return "UNKNOWN USB";
                }
            }
            catch (Exception ex)
            {
                return $"Erro ao buscar informações sobre o dispositivo USB: {ex.Message}";
            }
        }


        private static string GetUSBDeviceManufacturer(string pnpDeviceID)
        {
            string result = string.Empty;
            try
            {

                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\" + pnpDeviceID);
                string text = (string)registryKey.GetValue("Mfg");
                int num = text.IndexOf(";");
                bool flag = num > 0;
                if (flag)
                {
                    result = text.Substring(num + 1);
                }
                else
                {
                    result = text;
                }
            }
            catch
            {
                result = "<Manufacturer>";
            }
            return result;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
