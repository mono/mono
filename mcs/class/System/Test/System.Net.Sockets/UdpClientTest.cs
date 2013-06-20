// System.Net.Sockets.UdpClientTest.cs
//
// Authors:
//	Chris Bacon <chris.bacon@docobo.co.uk>
//	Gert Driesen <drieseng@users.sourceforge.net>
//

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Net.Sockets {
#if TARGET_JVM
	[Ignore("UdpClient is not supported - since UDP sockets are not supported")]
#endif
	[TestFixture]
	public class UdpClientTest {
		[Test] // .ctor ()
		public void Constructor1 ()
		{
			MyUdpClient client;
			Socket s;

			client = new MyUdpClient ();
			s = client.Client;
			Assert.IsNotNull (s, "Client");
			Assert.AreEqual (AddressFamily.InterNetwork, s.AddressFamily, "Client:AddressFamily");
			Assert.IsFalse (s.Connected, "Client:Connected");
#if NET_2_0
			Assert.IsFalse (s.IsBound, "#A:Client:IsBound");
#endif
			Assert.IsNull (s.LocalEndPoint, "Client:LocalEndPoint");
			Assert.AreEqual (ProtocolType.Udp, s.ProtocolType, "Client:ProtocolType");
			Assert.IsNull (s.RemoteEndPoint, "Client:RemoteEndPoint");
			Assert.AreEqual (SocketType.Dgram, s.SocketType, "Client:SocketType");
			Assert.IsFalse (client.Active, "Active");
#if NET_2_0
			Assert.IsFalse (client.DontFragment, "DontFragment");
			Assert.IsFalse (client.EnableBroadcast, "EnableBroadcast");
			//Assert.IsFalse (client.ExclusiveAddressUse, "ExclusiveAddressUse");
			Assert.IsTrue (client.MulticastLoopback, "MulticastLoopback");
			//Assert.AreEqual (32, client.Ttl, "Ttl");
#endif
		}

		[Test] // .ctor (AddressFamily)
		public void Constructor2 ()
		{
			MyUdpClient client;
			Socket s;

			client = new MyUdpClient (AddressFamily.InterNetwork);
			s = client.Client;
			Assert.IsNotNull (s, "#A:Client");
			Assert.AreEqual (AddressFamily.InterNetwork, s.AddressFamily, "#A:Client:AddressFamily");
			Assert.IsFalse (s.Connected, "#A:Client:Connected");
#if NET_2_0
			Assert.IsFalse (s.IsBound, "#A:Client:IsBound");
#endif
			Assert.IsNull (s.LocalEndPoint, "#A:Client:LocalEndPoint");
			Assert.AreEqual (ProtocolType.Udp, s.ProtocolType, "#A:Client:ProtocolType");
			Assert.IsNull (s.RemoteEndPoint, "#A:Client:RemoteEndPoint");
			Assert.AreEqual (SocketType.Dgram, s.SocketType, "#A:Client:SocketType");
			Assert.IsFalse (client.Active, "#A:Active");
#if NET_2_0
			//Assert.IsFalse (client.DontFragment, "#A:DontFragment");
			Assert.IsFalse (client.EnableBroadcast, "#A:EnableBroadcast");
			//Assert.IsFalse (client.ExclusiveAddressUse, "#A:ExclusiveAddressUse");
			Assert.IsTrue (client.MulticastLoopback, "#A:MulticastLoopback");
			//Assert.AreEqual (32, client.Ttl, "#A:Ttl");
#endif

			client = new MyUdpClient (AddressFamily.InterNetworkV6);
			s = client.Client;
			Assert.IsNotNull (s, "#B:Client");
			Assert.AreEqual (AddressFamily.InterNetworkV6, s.AddressFamily, "#B:Client:AddressFamily");
			Assert.IsFalse (s.Connected, "#B:Client:Connected");
#if NET_2_0
			Assert.IsFalse (s.IsBound, "#A:Client:IsBound");
#endif
			Assert.IsNull (s.LocalEndPoint, "#B:Client:LocalEndPoint");
			Assert.AreEqual (ProtocolType.Udp, s.ProtocolType, "#B:Client:ProtocolType");
			Assert.IsNull (s.RemoteEndPoint, "#B:Client:RemoteEndPoint");
			Assert.AreEqual (SocketType.Dgram, s.SocketType, "#B:Client:SocketType");
			Assert.IsFalse (client.Active, "#B:Active");
#if NET_2_0
			//Assert.IsFalse (client.DontFragment, "#B:DontFragment");
			Assert.IsFalse (client.EnableBroadcast, "#B:EnableBroadcast");
			//Assert.IsFalse (client.ExclusiveAddressUse, "#B:ExclusiveAddressUse");
			Assert.IsTrue (client.MulticastLoopback, "#B:MulticastLoopback");
			//Assert.AreEqual (32, client.Ttl, "#B:Ttl");
#endif
		}

		[Test] // .ctor (AddressFamily)
		public void Constructor2_Family_Invalid ()
		{
			try {
				new UdpClient (AddressFamily.NetBios);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
#if NET_2_0
				// 'UDP' Client can only accept InterNetwork or InterNetworkV6
				// addresses
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("family", ex.ParamName, "#A5");
#else
				Assert.AreEqual ("family", ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
#endif
			}

			try {
				new UdpClient ((AddressFamily) 666);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
#if NET_2_0
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("family", ex.ParamName, "#B5");
#else
				Assert.AreEqual ("family", ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
#endif
			}
		}

		[Test] // .ctor (Int32)
		public void Constructor3 ()
		{
			MyUdpClient client;
			Socket s;
			IPEndPoint localEP;

			client = new MyUdpClient (IPEndPoint.MinPort);
			s = client.Client;
			Assert.IsNotNull (s, "#A:Client");
			Assert.AreEqual (AddressFamily.InterNetwork, s.AddressFamily, "#A:Client:AddressFamily");
			Assert.IsFalse (s.Connected, "#A:Client:Connected");
#if NET_2_0
			Assert.IsTrue (s.IsBound, "#A:Client:IsBound");
#endif
			Assert.AreEqual (ProtocolType.Udp, s.ProtocolType, "#A:Client:ProtocolType");
			Assert.AreEqual (SocketType.Dgram, s.SocketType, "#A:Client:SocketType");
			Assert.IsFalse (client.Active, "#A:Active");
#if NET_2_0
			Assert.IsFalse (client.DontFragment, "#A:DontFragment");
			Assert.IsFalse (client.EnableBroadcast, "#A:EnableBroadcast");
			//Assert.IsFalse (client.ExclusiveAddressUse, "#A:ExclusiveAddressUse");
			Assert.IsTrue (client.MulticastLoopback, "#A:MulticastLoopback");
			//Assert.AreEqual (32, client.Ttl, "#A:Ttl");
#endif
			localEP = s.LocalEndPoint as IPEndPoint;
			Assert.IsNotNull (localEP, "#A:Client:LocalEndpoint");
			Assert.AreEqual (IPAddress.Any, localEP.Address, "#A:Client:LocalEndPoint/Address");
			Assert.AreEqual (AddressFamily.InterNetwork, localEP.AddressFamily, "#A:Client:LocalEndPoint/AddressFamily");

			client = new MyUdpClient (IPEndPoint.MaxPort);
			s = client.Client;
			Assert.IsNotNull (s, "#B:Client");
			Assert.AreEqual (AddressFamily.InterNetwork, s.AddressFamily, "#B:Client:AddressFamily");
			Assert.IsFalse (s.Connected, "#B:Client:Connected");
#if NET_2_0
			Assert.IsTrue (s.IsBound, "#B:Client:IsBound");
#endif
			Assert.AreEqual (ProtocolType.Udp, s.ProtocolType, "#B:Client:ProtocolType");
			Assert.AreEqual (SocketType.Dgram, s.SocketType, "#B:Client:SocketType");
			Assert.IsFalse (client.Active, "#B:Active");
#if NET_2_0
			Assert.IsFalse (client.DontFragment, "#B:DontFragment");
			Assert.IsFalse (client.EnableBroadcast, "#B:EnableBroadcast");
			//Assert.IsFalse (client.ExclusiveAddressUse, "#B:ExclusiveAddressUse");
			Assert.IsTrue (client.MulticastLoopback, "#B:MulticastLoopback");
			//Assert.AreEqual (32, client.Ttl, "#B:Ttl");
#endif
			localEP = s.LocalEndPoint as IPEndPoint;
			Assert.IsNotNull (localEP, "#B:Client:LocalEndpoint");
			Assert.AreEqual (IPAddress.Any, localEP.Address, "#B:Client:LocalEndPoint/Address");
			Assert.AreEqual (AddressFamily.InterNetwork, localEP.AddressFamily, "#B:Client:LocalEndPoint/AddressFamily");
			Assert.AreEqual (IPEndPoint.MaxPort, localEP.Port, "#B:Client:LocalEndPoint/Port");
		}

		[Test] // .ctor (Int32)
		public void Constructor3_Port_OutOfRange ()
		{
			try {
				new UdpClient (IPEndPoint.MaxPort + 1);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("port", ex.ParamName, "#A5");
			}

			try {
				new UdpClient (IPEndPoint.MinPort - 1);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("port", ex.ParamName, "#A5");
			}
		}

		[Test] // .ctor (IPEndPoint)
		public void Constructor4 ()
		{
			MyUdpClient client;
			Socket s;
			IPEndPoint localEP;
			IPEndPoint clientEP;

			clientEP = new IPEndPoint (IPAddress.Loopback, 8001);
			client = new MyUdpClient (clientEP);
			s = client.Client;
			Assert.IsNotNull (s, "#A:Client");
			Assert.AreEqual (AddressFamily.InterNetwork, s.AddressFamily, "#A:Client:AddressFamily");
			Assert.IsFalse (s.Connected, "#A:Client:Connected");
#if NET_2_0
			Assert.IsTrue (s.IsBound, "#A:Client:IsBound");
#endif
			Assert.AreEqual (ProtocolType.Udp, s.ProtocolType, "#A:Client:ProtocolType");
			Assert.AreEqual (SocketType.Dgram, s.SocketType, "#A:Client:SocketType");
			Assert.IsFalse (client.Active, "#A:Active");
#if NET_2_0
			Assert.IsFalse (client.DontFragment, "#A:DontFragment");
			Assert.IsFalse (client.EnableBroadcast, "#A:EnableBroadcast");
			//Assert.IsFalse (client.ExclusiveAddressUse, "#A:ExclusiveAddressUse");
			Assert.IsTrue (client.MulticastLoopback, "#A:MulticastLoopback");
			//Assert.AreEqual (32, client.Ttl, "#A:Ttl");
#endif
			localEP = s.LocalEndPoint as IPEndPoint;
			Assert.IsNotNull (localEP, "#A:Client:LocalEndpoint");
			Assert.IsFalse (object.ReferenceEquals (clientEP, localEP), "#A:Client:LocalEndPoint/ReferenceEquality");
			Assert.AreEqual (clientEP.Address, localEP.Address, "#A:Client:LocalEndPoint/Address");
			Assert.AreEqual (clientEP.AddressFamily, localEP.AddressFamily, "#A:Client:LocalEndPoint/AddressFamily");
			Assert.AreEqual (clientEP.Port, localEP.Port, "#A:Client:LocalEndPoint/Port");
		}

		[Test] // .ctor (IPEndPoint)
		public void Constructor4_LocalEP_Null ()
		{
			try {
				new UdpClient ((IPEndPoint) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("localEP", ex.ParamName, "#5");
			}
		}

		[Test] // .ctor (Int32, AddressFamily)
		public void Constructor5 ()
		{
			MyUdpClient client;
			Socket s;
			IPEndPoint localEP;

			client = new MyUdpClient (IPEndPoint.MinPort, AddressFamily.InterNetwork);
			s = client.Client;
			Assert.IsNotNull (s, "#A:Client");
			Assert.AreEqual (AddressFamily.InterNetwork, s.AddressFamily, "#A:Client:AddressFamily");
			Assert.IsFalse (s.Connected, "#A:Client:Connected");
#if NET_2_0
			Assert.IsTrue (s.IsBound, "#A:Client:IsBound");
#endif
			Assert.AreEqual (ProtocolType.Udp, s.ProtocolType, "#A:Client:ProtocolType");
			Assert.AreEqual (SocketType.Dgram, s.SocketType, "#A:Client:SocketType");
			Assert.IsFalse (client.Active, "#A:Active");
#if NET_2_0
			//Assert.IsFalse (client.DontFragment, "#A:DontFragment");
			Assert.IsFalse (client.EnableBroadcast, "#A:EnableBroadcast");
			//Assert.IsFalse (client.ExclusiveAddressUse, "#A:ExclusiveAddressUse");
			Assert.IsTrue (client.MulticastLoopback, "#A:MulticastLoopback");
			//Assert.AreEqual (32, client.Ttl, "#A:Ttl");
#endif
			localEP = s.LocalEndPoint as IPEndPoint;
			Assert.IsNotNull (localEP, "#A:Client:LocalEndpoint");
			Assert.AreEqual (IPAddress.Any, localEP.Address, "#A:Client:LocalEndPoint/Address");
			Assert.AreEqual (AddressFamily.InterNetwork, localEP.AddressFamily, "#A:Client:LocalEndPoint/AddressFamily");

			client = new MyUdpClient (IPEndPoint.MaxPort, AddressFamily.InterNetworkV6);
			s = client.Client;
			Assert.IsNotNull (s, "#B:Client");
			Assert.AreEqual (AddressFamily.InterNetworkV6, s.AddressFamily, "#B:Client:AddressFamily");
			Assert.IsFalse (s.Connected, "#B:Client:Connected");
#if NET_2_0
			Assert.IsTrue (s.IsBound, "#B:Client:IsBound");
#endif
			Assert.AreEqual (ProtocolType.Udp, s.ProtocolType, "#B:Client:ProtocolType");
			Assert.AreEqual (SocketType.Dgram, s.SocketType, "#B:Client:SocketType");
			Assert.IsFalse (client.Active, "#B:Active");
#if NET_2_0
			//Assert.IsFalse (client.DontFragment, "#B:DontFragment");
			Assert.IsFalse (client.EnableBroadcast, "#B:EnableBroadcast");
			//Assert.IsFalse (client.ExclusiveAddressUse, "#B:ExclusiveAddressUse");
			Assert.IsTrue (client.MulticastLoopback, "#B:MulticastLoopback");
			//Assert.AreEqual (32, client.Ttl, "#B:Ttl");
#endif
			localEP = s.LocalEndPoint as IPEndPoint;
			Assert.IsNotNull (localEP, "#B:Client:LocalEndpoint");
			Assert.AreEqual (IPAddress.IPv6Any, localEP.Address, "#B:Client:LocalEndPoint/Address");
			Assert.AreEqual (AddressFamily.InterNetworkV6, localEP.AddressFamily, "#B:Client:LocalEndPoint/AddressFamily");
			Assert.AreEqual (IPEndPoint.MaxPort, localEP.Port, "#B:Client:LocalEndPoint/Port");
		}

		[Test] // .ctor (Int32, AddressFamily)
		public void Constructor5_Family_Invalid ()
		{
			try {
				new UdpClient (80, AddressFamily.NetBios);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// family
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
#if NET_2_0
				// 'UDP' Client can only accept InterNetwork or InterNetworkV6
				// addresses
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("family", ex.ParamName, "#A5");
#else
				Assert.AreEqual ("family", ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
#endif
			}

			try {
				new UdpClient (80, (AddressFamily) 666);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// family
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
#if NET_2_0
				// 'UDP' Client can only accept InterNetwork or InterNetworkV6
				// addresses
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("family", ex.ParamName, "#B5");
#else
				Assert.AreEqual ("family", ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
#endif
			}
		}

		[Test] // .ctor (Int32, AddressFamily)
		public void Constructor5_Port_OutOfRange ()
		{
			try {
				new UdpClient (IPEndPoint.MaxPort + 1, AddressFamily.InterNetwork);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("port", ex.ParamName, "#A5");
			}

			try {
				new UdpClient (IPEndPoint.MinPort - 1, AddressFamily.InterNetwork);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("port", ex.ParamName, "#A5");
			}
		}

		[Test] // .ctor (String, Int32)
		public void Constructor6 ()
		{
			MyUdpClient client;
			Socket s;
			IPEndPoint localEP;

			// Bug #5503
			// UDP port 0 doesn't seem to be valid.
			client = new MyUdpClient ("127.0.0.1", 53);
			s = client.Client;
			Assert.IsNotNull (s, "#A:Client");
			Assert.AreEqual (AddressFamily.InterNetwork, s.AddressFamily, "#A:Client:AddressFamily");
			Assert.IsTrue (s.Connected, "#A:Client:Connected");
#if NET_2_0
			Assert.IsTrue (s.IsBound, "#A:Client:IsBound");
#endif
			Assert.AreEqual (ProtocolType.Udp, s.ProtocolType, "#A:Client:ProtocolType");
			Assert.AreEqual (SocketType.Dgram, s.SocketType, "#A:Client:SocketType");
			Assert.IsTrue (client.Active, "#A:Active");
#if NET_2_0
			Assert.IsFalse (client.DontFragment, "#A:DontFragment");
			Assert.IsFalse (client.EnableBroadcast, "#A:EnableBroadcast");
			//Assert.IsFalse (client.ExclusiveAddressUse, "#A:ExclusiveAddressUse");
			//Assert.IsFalse (client.MulticastLoopback, "#A:MulticastLoopback");
			//Assert.AreEqual (32, client.Ttl, "#A:Ttl");
#endif
			localEP = s.LocalEndPoint as IPEndPoint;
			Assert.IsNotNull (localEP, "#A:Client:LocalEndpoint");
			Assert.AreEqual (IPAddress.Loopback, localEP.Address, "#A:Client:LocalEndPoint/Address");
			Assert.AreEqual (AddressFamily.InterNetwork, localEP.AddressFamily, "#A:Client:LocalEndPoint/AddressFamily");

			client = new MyUdpClient ("127.0.0.1", IPEndPoint.MaxPort);
			s = client.Client;
			Assert.IsNotNull (s, "#B:Client");
			Assert.AreEqual (AddressFamily.InterNetwork, s.AddressFamily, "#B:Client:AddressFamily");
			Assert.IsTrue (s.Connected, "#B:Client:Connected");
#if NET_2_0
			Assert.IsTrue (s.IsBound, "#B:Client:IsBound");
#endif
			Assert.AreEqual (ProtocolType.Udp, s.ProtocolType, "#B:Client:ProtocolType");
			Assert.AreEqual (SocketType.Dgram, s.SocketType, "#B:Client:SocketType");
			Assert.IsTrue (client.Active, "#B:Active");
#if NET_2_0
			Assert.IsFalse (client.DontFragment, "#B:DontFragment");
			Assert.IsFalse (client.EnableBroadcast, "#B:EnableBroadcast");
			//Assert.IsFalse (client.ExclusiveAddressUse, "#B:ExclusiveAddressUse");
			//Assert.IsFalse (client.MulticastLoopback, "#B:MulticastLoopback");
			//Assert.AreEqual (32, client.Ttl, "#B:Ttl");
#endif
			localEP = s.LocalEndPoint as IPEndPoint;
			Assert.IsNotNull (localEP, "#B:Client:LocalEndpoint");
			Assert.AreEqual (IPAddress.Loopback, localEP.Address, "#B:Client:LocalEndPoint/Address");
			Assert.AreEqual (AddressFamily.InterNetwork, localEP.AddressFamily, "#B:Client:LocalEndPoint/AddressFamily");
		}

		[Test] // .ctor (String, Int32)
		public void Constructor6_HostName_Null ()
		{
			try {
				new UdpClient ((string) null, int.MaxValue);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("hostname", ex.ParamName, "#5");
			}
		}

		[Test] // .ctor (String, Int32)
		public void Constructor6_Port_OutOfRange ()
		{
			try {
				new UdpClient ("local", IPEndPoint.MaxPort + 1);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("port", ex.ParamName, "#A5");
			}

			try {
				new UdpClient ("local", IPEndPoint.MinPort - 1);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("port", ex.ParamName, "#A5");
			}
		}

		[Test]
		public void UdpClientBroadcastTest () 
		{
			UdpClient client = new UdpClient ();
			byte[] bytes = new byte[] {10, 11, 12, 13};

			try {
				client.Send (bytes, bytes.Length, new IPEndPoint (IPAddress.Broadcast, 1235));
			} finally {
				client.Close ();
			}
		}

		[Test] // JoinMulticastGroup (IPAddress)
		public void JoinMulticastGroup1_IPv4 ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.23");

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Any, 1234))) {
				client.JoinMulticastGroup (mcast_addr);
			}
		}

		[Test] // JoinMulticastGroup (IPAddress)
		public void JoinMulticastGroup1_IPv6 ()
		{
#if NET_2_0
			if (!Socket.OSSupportsIPv6)
#else
			if (!Socket.SupportsIPv6)
#endif
				Assert.Ignore ("IPv6 not enabled.");

			IPAddress mcast_addr = IPAddress.Parse ("ff02::1");

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.IPv6Any, 1234))) {
				client.JoinMulticastGroup (mcast_addr);
			}
		}

		[Test] // JoinMulticastGroup (IPAddress)
		public void JoinMulticastGroup1_MulticastAddr_Null ()
		{
			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Loopback, 1234))) {
				try {
					client.JoinMulticastGroup ((IPAddress) null);
					Assert.Fail ("#1");
#if NET_2_0
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("multicastAddr", ex.ParamName, "#5");
				}
#else
				} catch (NullReferenceException ex) {
					Assert.AreEqual (typeof (NullReferenceException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
#endif
			}
		}

		[Test] // JoinMulticastGroup (IPAddress)
		public void JoinMulticastGroup1_Socket_Closed ()
		{
			IPAddress mcast_addr = null;

			UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Loopback, 1234));
			client.Close ();
			try {
				client.JoinMulticastGroup (mcast_addr);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (UdpClient).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // JoinMulticastGroup (IPAddress)
		[Category ("NotWorking")]
		public void JoinMulticastGroup1_Socket_NotBound ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.23");

			UdpClient client = new UdpClient (AddressFamily.InterNetwork);
			try {
				client.JoinMulticastGroup (mcast_addr);
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				// An invalid argument was supplied
				Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
				Assert.AreEqual (10022, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (10022, ex.NativeErrorCode, "#6");
#if NET_2_0
				Assert.AreEqual (SocketError.InvalidArgument, ex.SocketErrorCode, "#7");
#endif
			} finally {
				client.Close ();
			}
		}

		[Test] // JoinMulticastGroup (In32, IPAddress)
		public void JoinMulticastGroup2_IPv4 ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.23");

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Any, 1234))) {
				try {
					client.JoinMulticastGroup (0, mcast_addr);
					Assert.Fail ("#1");
				} catch (SocketException ex) {
					// The attempted operation is not supported for the type of
					// object referenced
					Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
					Assert.AreEqual (10045, ex.ErrorCode, "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
					Assert.AreEqual (10045, ex.NativeErrorCode, "#6");
#if NET_2_0
					Assert.AreEqual (SocketError.OperationNotSupported, ex.SocketErrorCode, "#7");
#endif
				}
			}
		}

		[Test] // JoinMulticastGroup (In32, IPAddress)
		public void JoinMulticastGroup2_IPv6 ()
		{
#if NET_2_0
			if (!Socket.OSSupportsIPv6)
#else
			if (!Socket.SupportsIPv6)
#endif
				Assert.Ignore ("IPv6 not enabled.");

			IPAddress mcast_addr = IPAddress.Parse ("ff02::1");

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.IPv6Any, 1234))) {
				client.JoinMulticastGroup (0, mcast_addr);
			}
		}

		[Test] // JoinMulticastGroup (Int32, IPAddress)
		public void JoinMulticastGroup2_MulticastAddr_Null ()
		{
			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Loopback, 1234))) {
				try {
					client.JoinMulticastGroup (0, (IPAddress) null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("multicastAddr", ex.ParamName, "#5");
				}
			}
		}

		[Test] // JoinMulticastGroup (Int32, IPAddress)
		public void JoinMulticastGroup2_Socket_Closed ()
		{
			IPAddress mcast_addr = null;

			UdpClient client = new UdpClient (new IPEndPoint (IPAddress.IPv6Any, 1234));
			client.Close ();
			try {
				client.JoinMulticastGroup (0, mcast_addr);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (UdpClient).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // JoinMulticastGroup (Int32, IPAddress)
		[Category ("NotWorking")]
		public void JoinMulticastGroup2_Socket_NotBound ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("ff02::1");

			UdpClient client = new UdpClient (AddressFamily.InterNetworkV6);
			try {
				client.JoinMulticastGroup (0, mcast_addr);
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				// An invalid argument was supplied
				Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
				Assert.AreEqual (10022, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (10022, ex.NativeErrorCode, "#6");
#if NET_2_0
				Assert.AreEqual (SocketError.InvalidArgument, ex.SocketErrorCode, "#7");
#endif
			} finally {
				client.Close ();
			}
		}

		[Test] // JoinMulticastGroup (IPAddress, Int32)
		public void JoinMulticastGroup3_IPv4 ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.23");

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Any, 1234))) {
				client.JoinMulticastGroup (mcast_addr, 0);
			}

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Any, 1234))) {
				client.JoinMulticastGroup (mcast_addr, 255);
			}
		}

		[Test] // JoinMulticastGroup (IPAddress, Int32)
		public void JoinMulticastGroup3_IPv6 ()
		{
#if NET_2_0
			if (!Socket.OSSupportsIPv6)
#else
			if (!Socket.SupportsIPv6)
#endif
				Assert.Ignore ("IPv6 not enabled.");

			IPAddress mcast_addr = IPAddress.Parse ("ff02::1");

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.IPv6Any, 1234))) {
				client.JoinMulticastGroup (mcast_addr, 0);
			}

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.IPv6Any, 1234))) {
				client.JoinMulticastGroup (mcast_addr, 255);
			}
		}

		[Test] // JoinMulticastGroup (IPAddress, Int32)
		public void JoinMulticastGroup3_MulticastAddr_Null ()
		{
			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Loopback, 1234))) {
				try {
					client.JoinMulticastGroup ((IPAddress) null, int.MaxValue);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("multicastAddr", ex.ParamName, "#5");
				}
			}
		}

		[Test] // JoinMulticastGroup (IPAddress, Int32)
		public void JoinMulticastGroup3_Socket_Closed ()
		{
			IPAddress mcast_addr = null;

			UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Any, 1234));
			client.Close ();
			try {
				client.JoinMulticastGroup (mcast_addr, 0);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (UdpClient).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // JoinMulticastGroup (IPAddress, Int32)
		[Category ("NotWorking")]
		public void JoinMulticastGroup3_Socket_NotBound ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.23");

			UdpClient client = new UdpClient (AddressFamily.InterNetwork);
			try {
				client.JoinMulticastGroup (mcast_addr, 5);
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				// An invalid argument was supplied
				Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
				Assert.AreEqual (10022, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (10022, ex.NativeErrorCode, "#6");
#if NET_2_0
				Assert.AreEqual (SocketError.InvalidArgument, ex.SocketErrorCode, "#7");
#endif
			} finally {
				client.Close ();
			}
		}

#if NET_2_0
		[Test] // JoinMulticastGroup (IPAddress, IPAddress)
		public void JoinMulticastGroup4_IPv4 ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.23");
			IPAddress local_addr = IPAddress.Any;

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Any, 1234))) {
				client.JoinMulticastGroup (mcast_addr, local_addr);
			}
		}

		[Test] // JoinMulticastGroup (IPAddress, IPAddress)
		public void JoinMulticastGroup4_IPv6 ()
		{
			if (!Socket.OSSupportsIPv6)
				Assert.Ignore ("IPv6 not enabled.");

			IPAddress mcast_addr = IPAddress.Parse ("ff02::1");
			IPAddress local_addr = IPAddress.IPv6Any;

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.IPv6Any, 1234))) {
				try {
					client.JoinMulticastGroup (mcast_addr, local_addr);
					Assert.Fail ("#1");
				} catch (SocketException ex) {
					// The attempted operation is not supported for the type of
					// object referenced
					Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
					Assert.AreEqual (10045, ex.ErrorCode, "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
					Assert.AreEqual (10045, ex.NativeErrorCode, "#6");
					Assert.AreEqual (SocketError.OperationNotSupported, ex.SocketErrorCode, "#7");
				}
			}
		}

		[Test] // JoinMulticastGroup (IPAddress, IPAddress)
		public void JoinMulticastGroup4_LocalAddress_Null ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.23");

			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Loopback, 1234))) {
				try {
					client.JoinMulticastGroup (mcast_addr, (IPAddress) null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("mcint", ex.ParamName, "#5");
				}
			}
		}

		[Test] // JoinMulticastGroup (IPAddress, IPAddress)
		public void JoinMulticastGroup4_MulticastAddr_Null ()
		{
			using (UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Loopback, 1234))) {
				try {
					client.JoinMulticastGroup ((IPAddress) null, IPAddress.Loopback);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("group", ex.ParamName, "#5");
				}
			}
		}

		[Test] // JoinMulticastGroup (IPAddress, IPAddress)
		public void JoinMulticastGroup4_Socket_Closed ()
		{
			IPAddress mcast_addr = null;
			IPAddress local_addr = null;

			UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Any, 1234));
			client.Close ();
			try {
				client.JoinMulticastGroup (mcast_addr, local_addr);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (UdpClient).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // JoinMulticastGroup (IPAddress, IPAddress)
		[Category ("NotWorking")]
		public void JoinMulticastGroup4_Socket_NotBound ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.23");
			IPAddress local_addr = Dns.GetHostEntry (string.Empty).AddressList [0];

			UdpClient client = new UdpClient (AddressFamily.InterNetwork);
			try {
				client.JoinMulticastGroup (mcast_addr, local_addr);
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				// An invalid argument was supplied
				Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
				Assert.AreEqual (10022, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (10022, ex.NativeErrorCode, "#6");
				Assert.AreEqual (SocketError.InvalidArgument, ex.SocketErrorCode, "#7");
			} finally {
				client.Close ();
			}
		}

		[Test]
		public void CloseInReceive ()
		{
			UdpClient client = null;
			var rnd = new Random ();
			for (int i = 0; i < 5; i++) {
				int port = rnd.Next (1025, 65534);
				try {
					client = new UdpClient (port);
					break;
				} catch (Exception ex) {
					if (i == 5)
						throw;
				}
			}

			new Thread(delegate() {
				Thread.Sleep(2000);
				client.Close();
				}).Start();

			bool got_exc = false;
			IPEndPoint ep = new IPEndPoint (IPAddress.Any, 0);
			try {
				client.Receive(ref ep);
			} catch (SocketException) {
				got_exc = true;
			} finally {
				client.Close ();
			}
			Assert.IsTrue (got_exc);
		}

		// Test for bug 324033
		[Test]
		public void JoinMulticastGroupWithLocal ()
		{
			UdpClient client = new UdpClient (9001);
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.24");
			IPAddress local_addr = IPAddress.Any;

			try {
				client.JoinMulticastGroup (mcast_addr, local_addr);
			} finally {
				client.Close ();
			}
		}
		
		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void BeginSendNull ()
		{
			UdpClient client = new UdpClient ();
			client.BeginSend (null, 0, null, null);
			client.Close ();
		}
		
		static bool BSSent = false;
		static int BSBytes;
		static ManualResetEvent BSCalledBack = new ManualResetEvent (false);
		
		private static void BSCallback (IAsyncResult asyncResult)
		{
			UdpClient client = (UdpClient)asyncResult.AsyncState;
			
			BSBytes = client.EndSend (asyncResult);
			
			BSSent = true;
			BSCalledBack.Set ();
		}
		
		[Test]
		public void BeginSend ()
		{
			UdpClient client = new UdpClient ();
			byte[] bytes = new byte[] {10, 11, 12, 13};

			try {
				client.BeginSend (bytes, bytes.Length, new AsyncCallback (BSCallback), client);
				Assert.Fail ("BeginSend #1");
			} catch (SocketException ex) {
				Assert.AreEqual (10057, ex.ErrorCode,
						 "BeginSend #2");
			}
			
			try {
				client.BeginSend (bytes, bytes.Length, null, new AsyncCallback (BSCallback), client);
				Assert.Fail ("BeginSend #3");
			} catch (SocketException ex) {
				Assert.AreEqual (10057, ex.ErrorCode,
						 "BeginSend #4");
			}

			IPEndPoint ep = new IPEndPoint (Dns.GetHostEntry (string.Empty).AddressList[0], 1236);
			
			BSCalledBack.Reset ();
			
			client.BeginSend (bytes, bytes.Length, ep,
					  new AsyncCallback (BSCallback),
					  client);
			if (BSCalledBack.WaitOne (2000, false) == false) {
				Assert.Fail ("BeginSend wait timed out");
			}
			
			Assert.AreEqual (true, BSSent, "BeginSend #5");
			Assert.AreEqual (4, BSBytes, "BeginSend #6");

			client.Close ();
		}
		
		static bool BRReceived = false;
		static byte[] BRBytes;
		static IPEndPoint BRFrom;
		static ManualResetEvent BRCalledBack = new ManualResetEvent (false);
		
		private static void BRCallback (IAsyncResult asyncResult)
		{
			UdpClient client = (UdpClient)asyncResult.AsyncState;
			
			BRBytes = client.EndReceive (asyncResult, ref BRFrom);
			
			BRReceived = true;
			BRCalledBack.Set ();
		}
		
		[Test]
		public void BeginReceive ()
		{
			UdpClient client = new UdpClient (1237);
			
			BRCalledBack.Reset ();
			
			client.BeginReceive (BRCallback, client);

			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 1237);
			byte[] send_bytes = new byte[] {10, 11, 12, 13};
			client.Send (send_bytes, send_bytes.Length, ep);

			if (BRCalledBack.WaitOne (2000, false) == false) {
				Assert.Fail ("BeginReceive wait timed out");
			}
			
			Assert.AreEqual (true, BRReceived, "BeginReceive #1");
			Assert.AreEqual (4, BRBytes.Length, "BeginReceive #2");
			Assert.AreEqual (ep. Port, BRFrom.Port, "BeginReceive #3");
			Assert.AreEqual (ep.Address, BRFrom.Address, "BeginReceive #4");

			client.Close ();
		}
		
		[Test]
		public void Available ()
		{
			UdpClient client = new UdpClient (1238);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 1238);
			byte[] bytes = new byte[] {10, 11, 12, 13};
			
			client.Send (bytes, bytes.Length, ep);
			int avail = client.Available;
			
			Assert.AreEqual (bytes.Length, avail, "Available #1");

			client.Close ();
		}
		
		[Test]
		[Category ("NotWorking")]  // Using PMTU settings workaround on Linux, default true
		public void DontFragmentDefault ()
		{
			UdpClient client = new UdpClient ();
			
			/* Ignore the docs, testing shows the default
			 * here is in fact false
			 */
			Assert.AreEqual (false, client.DontFragment, "DontFragmentDefault");

			client.Close ();
		}
		
		[Test]
		public void EnableBroadcastDefault ()
		{
			UdpClient client = new UdpClient ();
			
			Assert.AreEqual (false, client.EnableBroadcast, "EnableBroadcastDefault");

			client.Close ();
		}
		
		/* Can't test the default for ExclusiveAddressUse as
		 * it's different on different versions and service
		 * packs of windows
		 */
		[Test]
		[Category ("NotWorking")] // Not supported on Linux
		public void ExclusiveAddressUseUnbound ()
		{
			UdpClient client = new UdpClient ();

			client.ExclusiveAddressUse = true;

			Assert.AreEqual (true, client.ExclusiveAddressUse, "ExclusiveAddressUseUnbound");

			client.Close ();
		}
		
		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		[Category ("NotWorking")] // Not supported on Linux
		public void ExclusiveAddressUseBound ()
		{
			UdpClient client = new UdpClient (1239);

			client.ExclusiveAddressUse = true;

			client.Close ();
		}
		
		[Test]
		public void MulticastLoopbackDefault ()
		{
			UdpClient client = new UdpClient ();
			
			Assert.AreEqual (true, client.MulticastLoopback, "MulticastLoopbackDefault");

			client.Close ();
		}
		
		/* No test for Ttl default as it is platform dependent */
#endif

		class MyUdpClient : UdpClient
		{
			public MyUdpClient ()
			{
			}

			public MyUdpClient (AddressFamily family)
				: base (family)
			{
			}

			public MyUdpClient (Int32 port)
				: base (port)
			{
			}


			public MyUdpClient (IPEndPoint localEP)
				: base (localEP)
			{
			}

			public MyUdpClient (int port, AddressFamily family)
				: base (port, family)
			{
			}

			public MyUdpClient (string hostname, int port)
				: base (hostname, port)
			{
			}

			public new bool Active {
				get { return base.Active; }
				set { base.Active = value; }
			}

#if ONLY_1_1
			public new Socket Client {
				get { return base.Client; }
				set { base.Client = value; }
			}
#endif
		}
	}
}
