using System;

namespace ListDevices
{
	// Token: 0x02000003 RID: 3
	internal class USBDeviceInfo
	{
		// Token: 0x0600000F RID: 15 RVA: 0x00003EA0 File Offset: 0x000020A0
		public USBDeviceInfo(string deviceID, string pnpDeviceID, string description, string manufacturer)
		{
			this.Manufacturer = manufacturer;
			this.DeviceID = deviceID;
			this.PnpDeviceID = pnpDeviceID;
			this.Description = description;
		}
		public string DeviceID { get; private set; }

		public string PnpDeviceID { get; private set; }
		public string Description { get; private set; }

		public string Manufacturer { get; private set; }
	}
}
