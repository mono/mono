//
// IPInterfacePropertiesTest.cs - NUnit Test Cases for System.Net.NetworkInformation.IPInterfaceProperties
//
// Authors:
//   Ben Woods (woodsb02@gmail.com)
//

using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace MonoTests.System.Net.NetworkInformation
{

	[TestFixture]
	public class IPInterfacePropertiesTest
	{
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AtLeastOneUnicastAddress ()
		{
			int numUnicastAddresses = 0;
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			foreach (NetworkInterface adapter in adapters)
			{
				IPInterfaceProperties adapterProperties = adapter.GetIPProperties ();
				UnicastIPAddressInformationCollection unicastAddresses = adapterProperties.UnicastAddresses;
				numUnicastAddresses += unicastAddresses.Count;
			}
			Assert.IsTrue (numUnicastAddresses > 0);
		}

		// Borrowed from IPInterfaceProperties.cs
		bool HasOnlyDefaultGateway (string iface)
		{
			int gwCount = 0;
			int defaultGwCount = 0;
#if MONODROID
			if (!File.Exists ("/proc/net/route"))
				return false;
			try {
				using (StreamReader reader = new StreamReader ("/proc/net/route")) {
					string line;
					reader.ReadLine (); // Ignore first line
					while ((line = reader.ReadLine ()) != null) {
						line = line.Trim ();
						if (line.Length == 0)
							continue;

						string [] parts = line.Split ('\t');
						if (parts.Length < 3)
							continue;
						string gw_address = parts [2].Trim ();
						byte [] ipbytes = new byte [4];
						if (gw_address.Length == 8 && iface.Equals (parts [0], StringComparison.OrdinalIgnoreCase)) {
							for (int i = 0; i < 4; i++) {
								if (!Byte.TryParse (gw_address.Substring (i * 2, 2), NumberStyles.HexNumber, null, out ipbytes [3 - i]))
									continue;
							}
							IPAddress ip = new IPAddress (ipbytes);
							if (ip.Equals (IPAddress.Any))
								defaultGwCount++;
							else
								gwCount++;
						}
					}
				}
			} catch {
			}
#endif
			return gwCount == 0 && defaultGwCount > 0;
		}
	
		[Test]
		[Category ("InetAccess")]
		public void AtLeastOneGatewayAddress ()
		{
			int numGatewayAddresses = 0;
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			
			// On Android (possibly on other systems too) it is possible that no gateway address is available and its lack is NOT an error
			// Here is a sample of /proc/net/route from Nexus 9 running Android 5.1.1 (IPInterfaceProperties parses that file on Linux)
			//
			//  Iface	Destination	Gateway 	Flags	RefCnt	Use	Metric	Mask		MTU	Window	IRTT
			//  wlan0	0001A8C0	00000000	0001	0	0	0	00FFFFFF	0	0	0
			//
			// Gateway is set to any address and it is explicitly ignored by the route information parser
			//
			// For comparison, here's route contents from an Android 4.4.4 device:
			//
			//  Iface	Destination	Gateway 	Flags	RefCnt	Use	Metric	Mask		MTU	Window	IRTT
			//  wlan0	00000000	0101A8C0	0003	0	0	0	00000000	0	0	0
			//  wlan0	00000000	0101A8C0	0003	0	0	203	00000000	0	0	0
			//  wlan0	0001A8C0	00000000	0001	0	0	0	00FFFFFF	0	0	0
			//  wlan0	0001A8C0	00000000	0001	0	0	0	00FFFFFF	0	0	0
			//  wlan0	0001A8C0	00000000	0001	0	0	203	00FFFFFF	0	0	0
			//  wlan0	0101A8C0	00000000	0005	0	0	0	FFFFFFFF	0	0	0
			//
			// Obviously, this test fails on the first device and succeeds on the second. For this reason the test is modified to succeed
			// in case of devices like the first one since it's not a real failure but a shortcoming of the .NET API
			//
			foreach (NetworkInterface adapter in adapters)
			{
				IPInterfaceProperties adapterProperties = adapter.GetIPProperties ();
				GatewayIPAddressInformationCollection gatewayAddresses = adapterProperties.GatewayAddresses;
				numGatewayAddresses += HasOnlyDefaultGateway (adapter.Name) ? 1 : gatewayAddresses.Count;
			}
			
			Assert.IsTrue (numGatewayAddresses > 0);
		}
	
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DnsEnabled ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				Assert.Ignore ("IsDnsEnabled is not necessarily enabled for all interfaces on windows.");

			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			foreach (NetworkInterface adapter in adapters)
			{
				IPInterfaceProperties adapterProperties = adapter.GetIPProperties ();
				Assert.IsTrue (adapterProperties.IsDnsEnabled);
			}
		}
	
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		// The code works as expected when part of a regular app. It fails when executed from within an NUnit test
		// Might be a problem with the test suite. To investigate.
		[Category("AndroidNotWorking")]
		public void AtLeastOneDnsAddress ()
		{
			int numDnsAddresses = 0;
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			foreach (NetworkInterface adapter in adapters)
			{
				IPInterfaceProperties adapterProperties = adapter.GetIPProperties ();
				IPAddressCollection dnsAddresses = adapterProperties.DnsAddresses;
				numDnsAddresses += dnsAddresses.Count;
			}
			Console.WriteLine ("numDnsAddresses == {0}", numDnsAddresses);
			// reading /etc/resolve.conf does not work on iOS devices (but works on simulator)
			// ref: https://bugzilla.xamarin.com/show_bug.cgi?id=27707
#if !MONOTOUCH
			Assert.IsTrue (numDnsAddresses > 0);
#endif
		}
	
	}
}
