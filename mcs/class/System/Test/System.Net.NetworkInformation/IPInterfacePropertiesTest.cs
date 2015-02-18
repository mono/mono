//
// IPInterfacePropertiesTest.cs - NUnit Test Cases for System.Net.NetworkInformation.IPInterfaceProperties
//
// Authors:
//   Ben Woods (woodsb02@gmail.com)
//

using NUnit.Framework;
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace MonoTests.System.Net.NetworkInformation
{

	[TestFixture]
	public class IPInterfacePropertiesTest
	{
		[Test]
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
	
		[Test]
		public void AtLeastOneGatewayAddress ()
		{
			int numGatewayAddresses = 0;
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			foreach (NetworkInterface adapter in adapters)
			{
				IPInterfaceProperties adapterProperties = adapter.GetIPProperties ();
				GatewayIPAddressInformationCollection gatewayAddresses = adapterProperties.GatewayAddresses;
				numGatewayAddresses += gatewayAddresses.Count;
			}
			Assert.IsTrue (numGatewayAddresses > 0);
		}
	
		[Test]
		public void DnsEnabled ()
		{
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			foreach (NetworkInterface adapter in adapters)
			{
				IPInterfaceProperties adapterProperties = adapter.GetIPProperties ();
				Assert.IsTrue (adapterProperties.IsDnsEnabled);
			}
		}
	
		[Test]
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
			Assert.IsTrue (numDnsAddresses > 0);
		}
	
	}
}
