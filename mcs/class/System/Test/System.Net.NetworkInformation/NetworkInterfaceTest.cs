//
// NetworkInterfaceTest.cs - NUnit Test Cases for System.Net.NetworkInformation.NetworkInterface
//
// Authors:
//   Ben Woods (woodsb02@gmail.com)
//

using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace MonoTests.System.Net.NetworkInformation
{

	[TestFixture]
	public class NetworkInterfaceTest
	{
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void IsNetworkAvailable ()
		{
			Assert.IsTrue (NetworkInterface.GetIsNetworkAvailable ());
		}
	
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void LoopbackInterfaceIndex ()
		{
			Assert.IsTrue (NetworkInterface.LoopbackInterfaceIndex > 0);
		}
	
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AtLeastOneInterface ()
		{
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			Assert.IsTrue (adapters.Length > 0);
		}
	
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void FirstInterfaceId ()
		{
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			Assert.IsTrue (adapters[0].Id.Length > 0);
		}
	
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void FirstInterfaceName ()
		{
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			Assert.IsTrue (adapters[0].Name.Length > 0);
		}
	
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void FirstInterfaceType ()
		{
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			Assert.AreNotEqual (adapters[0].NetworkInterfaceType, NetworkInterfaceType.Unknown);
		}
	
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void FirstInterfaceOperationalStatus ()
		{
			var adapter = NetworkInterface.GetAllNetworkInterfaces ()[0];
			var status = adapter.OperationalStatus;
			// lo status is Unknown on Linux
			//Assert.AreNotEqual (adapter.OperationalStatus, OperationalStatus.Unknown);
		}
	
		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void FirstInterfaceSpeed ()
		{
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			Assert.IsTrue (adapters[0].Speed > 0);
		}

		[Test]
#if WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void IPv4Mask ()
		{
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			foreach (NetworkInterface adapter in adapters)
			{
				if (adapter.Supports (NetworkInterfaceComponent.IPv4))
				{
					IPInterfaceProperties adapterProperties = adapter.GetIPProperties ();
					foreach (UnicastIPAddressInformation uni in adapterProperties.UnicastAddresses)
					{
						if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							Assert.IsNotNull (uni.IPv4Mask);
						}
					}
				}
			}
		}
	}
}
