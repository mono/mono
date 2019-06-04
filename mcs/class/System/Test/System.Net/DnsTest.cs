// DnsTest.cs - NUnit Test Cases for the System.Net.Dns class
//
// Authors: 
//   Mads Pultz (mpultz@diku.dk)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2001 Mads Pultz
// (C) 2003 Martin Willemoes Hansen
// 

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
	[Category ("InetAccess")]
	public class DnsTest
	{
		private String site1Name = "google-public-dns-a.google.com",
			site1Dot = "8.8.8.8",
			site2Name = "google-public-dns-b.google.com",
			site2Dot = "8.8.4.4",
			noneExistingSite = "unlikely.xamarin.com";
		private uint site1IP = 134744072, site2IP = 134743044; // Big-Endian

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AsyncGetHostByName ()
		{
			IAsyncResult async = Dns.BeginGetHostByName (site1Name, null, null);
			IPHostEntry entry = Dns.EndGetHostByName (async);
			SubTestValidIPHostEntry (entry);
			Assert.AreEqual ("google-public-dns-a.google.com", entry.HostName, "#1");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AsyncGetHostByNameCallback ()
		{
			var evt = new ManualResetEvent (false);
			Exception ex = null;
			Dns.BeginGetHostByName (site1Name, new AsyncCallback ((IAsyncResult ar) =>
			{
				try {
					IPHostEntry h;
					h = Dns.EndGetHostByName (ar);
					SubTestValidIPHostEntry (h);
				} catch (Exception e) {
					ex = e;
				} finally {
					evt.Set ();
				}
			}), null);

			Assert.IsTrue (evt.WaitOne (TimeSpan.FromSeconds (60)), "Wait");
			Assert.IsNull (ex, "Exception");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AsyncResolve ()
		{
			IAsyncResult async = Dns.BeginResolve (site1Dot, null, null);
			IPHostEntry entry = Dns.EndResolve (async);
			SubTestValidIPHostEntry (entry);
			CheckIPv4Address (entry, IPAddress.Parse (site1Dot));
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AsyncResolveCallback ()
		{
			var evt = new ManualResetEvent (false);
			Exception ex = null;
			Dns.BeginResolve (site1Name, new AsyncCallback ((IAsyncResult ar) =>
			{
				try {
					IPHostEntry h = Dns.EndResolve (ar);
					SubTestValidIPHostEntry (h);
				} catch (Exception e) {
					ex = e;
				} finally {
					evt.Set ();
				}
			}), null);

			Assert.IsTrue (evt.WaitOne (TimeSpan.FromSeconds (60)), "Wait");
			Assert.IsNull (ex, "Exception");
		}

		[Test]
		public void BeginGetHostAddresses_HostNameOrAddress_Null ()
		{
			try {
				Dns.BeginGetHostAddresses (
					(string) null,
					new AsyncCallback (ShouldntBeCalled),
					null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("hostName", ex.ParamName, "#5");
			}
		}

		[Test]
		public void BeginGetHostAddresses_HostNameOrAddress_UnspecifiedAddress ()
		{
			// IPv4
			try {
				Dns.BeginGetHostAddresses (
					"0.0.0.0",
					new AsyncCallback (ShouldntBeCalled),
					null);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// IPv4 address 0.0.0.0 and IPv6 address ::0 are
				// unspecified addresses that cannot be used as
				// a target address
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("hostNameOrAddress", ex.ParamName, "#A5");
			}

			// IPv6
			try {
				Dns.BeginGetHostAddresses (
					"::0",
					new AsyncCallback (ShouldntBeCalled),
					null);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// IPv4 address 0.0.0.0 and IPv6 address ::0 are
				// unspecified addresses that cannot be used as
				// a target address
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("hostNameOrAddress", ex.ParamName, "#B5");
			}
		}

		void ShouldntBeCalled (IAsyncResult ar)
		{
			Assert.Fail ("Should not be called");
		}

		[Test]
		public void GetHostAddresses_HostNameOrAddress_Null ()
		{
			try {
				Dns.GetHostAddresses ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("hostNameOrAddress", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetHostAddresses_HostNameOrAddress_UnspecifiedAddress ()
		{
			// IPv4
			try {
				Dns.GetHostAddresses ("0.0.0.0");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// IPv4 address 0.0.0.0 and IPv6 address ::0 are
				// unspecified addresses that cannot be used as
				// a target address
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("hostNameOrAddress", ex.ParamName, "#A5");
			}

			// IPv6
			try {
				Dns.GetHostAddresses ("::0");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// IPv4 address 0.0.0.0 and IPv6 address ::0 are
				// unspecified addresses that cannot be used as
				// a target address
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("hostNameOrAddress", ex.ParamName, "#B5");
			}
		}

		[Test]
		[Category("NotOnWindows")]
		public void GetHostAddresses_IPv6 ()
		{
			var address = Dns.GetHostAddresses("ipv6.google.com");
		}

		[Test]
		public void GetHostName ()
		{
			string hostName = Dns.GetHostName ();
			Assert.IsNotNull (hostName);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		[NUnit.Framework.Category ("AndroidNotWorking")] //Some Android devices like to return catch-all IPs for invalid host names.
		public void GetHostByName ()
		{
			SubTestGetHostByName (site1Name, site1Dot);
			SubTestGetHostByName (site2Name, site2Dot);
			try {
				var entry = Dns.GetHostByName (noneExistingSite);
				Assert.Fail ("Should raise a SocketException (assuming that '" + noneExistingSite + "' does not exist)");
			} catch (SocketException) {
			}
		}

		static void CheckIPv4Address (IPHostEntry h, IPAddress a)
		{
			var al = h.AddressList.Contains (a);
			Assert.True (al, "Failed to resolve host name " + h.HostName + " to expected IP address " + a.ToString());
		}

		void SubTestGetHostByName (string siteName, string siteDot)
		{
			IPHostEntry h = Dns.GetHostByName (siteName);
			SubTestValidIPHostEntry (h);
			Assert.AreEqual (siteName, h.HostName, "siteName");
			CheckIPv4Address (h, IPAddress.Parse (siteDot));
		}

		[Test]
		public void GetHostByName_HostName_Null ()
		{
			try {
				Dns.GetHostByName ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("hostName", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetHostByAddressString_Address_Null ()
		{
			try {
				Dns.GetHostByAddress ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("address", ex.ParamName, "#5");
			}
		}

		[Test]
		[ExpectedException (typeof (SocketException))]
		public void GetHostByAddressString2 ()
		{
			Dns.GetHostByAddress ("123.255.23");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void GetHostByAddressString3 ()
		{
			Dns.GetHostByAddress ("123.256.34.10");
		}

		[Test, ExpectedException (typeof (FormatException))]
		public void GetHostByAddressString4 ()
		{
			Dns.GetHostByAddress ("not an IP address");
		}

		[Test]
		public void GetHostByAddressString5 ()
		{
			Dns.GetHostByAddress (site1Dot);
		}

		[Test]
		public void GetHostByAddressIPAddress_Address_Null ()
		{
			try {
				Dns.GetHostByAddress ((IPAddress) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("address", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetHostByAddressIPAddress2 ()
		{
			IPAddress addr = new IPAddress (IPAddress.NetworkToHostOrder ((int) site1IP));
			IPHostEntry h = Dns.GetHostByAddress (addr);
			SubTestValidIPHostEntry (h);
			CheckIPv4Address (h, addr);
		}

		[Test]
		public void GetHostByAddressIPAddress3 ()
		{
			IPAddress addr = new IPAddress (IPAddress.NetworkToHostOrder ((int) site2IP));
			IPHostEntry h = Dns.GetHostByAddress (addr);
			SubTestValidIPHostEntry (h);
			CheckIPv4Address (h, addr);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void BeginResolve_HostName_Null ()
		{
			try {
				Dns.BeginResolve ((string) null,
					new AsyncCallback (ShouldntBeCalled),
					null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("hostName", ex.ParamName, "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Resolve ()
		{
			SubTestResolve (site1Name);
			SubTestResolve (site2Name);
			SubTestResolve (site1Dot);
			SubTestResolve (site2Dot);
		}

		void SubTestResolve (string addr)
		{
			IPHostEntry h = Dns.Resolve (addr);
			SubTestValidIPHostEntry (h);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Resolve_HostName_Null ()
		{
			try {
				Dns.Resolve ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("hostName", ex.ParamName, "#5");
			}
		}

		[Test] // BeginGetHostEntry (IPAddress, AsyncCallback, Object)
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void BeginGetHostEntry1_Address_Null ()
		{
			try {
				Dns.BeginGetHostEntry (
					(IPAddress) null,
					new AsyncCallback (ShouldntBeCalled),
					null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("address", ex.ParamName, "#5");
			}
		}

		[Test] // BeginGetHostEntry (String, AsyncCallback, Object)
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void BeginGetHostEntry2_HostNameOrAddress_Null ()
		{
			try {
				Dns.BeginGetHostEntry (
					(string) null,
					new AsyncCallback (ShouldntBeCalled),
					null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("hostName", ex.ParamName, "#5");
			}
		}

		[Test] // BeginGetHostEntry (String, AsyncCallback, Object)
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void BeginGetHostEntry2_HostNameOrAddress_UnspecifiedAddress ()
		{
			// IPv4
			try {
				Dns.BeginGetHostEntry (
					"0.0.0.0",
					new AsyncCallback (GetHostEntryCallback),
					null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// IPv4 address 0.0.0.0 and IPv6 address ::0 are
				// unspecified addresses that cannot be used as
				// a target address
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("hostNameOrAddress", ex.ParamName, "#5");
			}

			// IPv6
			try {
				Dns.BeginGetHostEntry (
					"::0",
					new AsyncCallback (GetHostEntryCallback),
					null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// IPv4 address 0.0.0.0 and IPv6 address ::0 are
				// unspecified addresses that cannot be used as
				// a target address
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("hostNameOrAddress", ex.ParamName, "#5");
			}
		}

		void GetHostEntryCallback (IAsyncResult ar)
		{
			IPHostEntry hostEntry = Dns.EndGetHostEntry (ar);
			Assert.IsNotNull (hostEntry);
		}

		[Test] // GetHostEntry (IPAddress)
		public void GetHostEntry1_Address_Null ()
		{
			try {
				Dns.GetHostEntry ((IPAddress) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("address", ex.ParamName, "#5");
			}
		}

		[Test] // GetHostEntry (String)
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void GetHostEntry2 ()
		{
			Dns.GetHostEntry (site1Name); // hostname
			Dns.GetHostEntry (site1Dot); // IP address
		}

		[Test] // GetHostEntry (String)
		public void GetHostEntry2_HostNameOrAddress_Null ()
		{
			try {
				Dns.GetHostEntry ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("hostNameOrAddress", ex.ParamName, "#5");
			}
		}

		[Test] // GetHostEntry (String)
		public void GetHostEntry2_HostNameOrAddress_UnspecifiedAddress ()
		{
			// IPv4
			try {
				Dns.GetHostEntry ("0.0.0.0");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// IPv4 address 0.0.0.0 and IPv6 address ::0 are
				// unspecified addresses that cannot be used as
				// a target address
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("hostNameOrAddress", ex.ParamName, "#A5");
			}

			// IPv6
			try {
				Dns.GetHostEntry ("::0");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// IPv4 address 0.0.0.0 and IPv6 address ::0 are
				// unspecified addresses that cannot be used as
				// a target address
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("hostNameOrAddress", ex.ParamName, "#B5");
			}
		}

		void SubTestValidIPHostEntry (IPHostEntry h)
		{
			Assert.IsNotNull (h.HostName, "HostName not null");
			Assert.IsNotNull (h.AddressList, "AddressList not null");
			Assert.IsTrue (h.AddressList.Length > 0, "AddressList.Length");
		}

		[Test]
		public void GetHostEntry_StringEmpty ()
		{
			Dns.GetHostEntry ("localhost");
		}

		/* This isn't used anymore, but could be useful for debugging
		static void printIPHostEntry(IPHostEntry h)
		{
			Console.WriteLine("----------------------------------------------------");
			Console.WriteLine("Host name:");
			Console.WriteLine(h.HostName);
			Console.WriteLine("IP addresses:");
			IPAddress[] list = h.AddressList;
			for(int i = 0; i < list.Length; ++i)
				Console.WriteLine(list[i]);
			Console.WriteLine("Aliases:");
			string[] aliases = h.Aliases;
			for(int i = 0; i < aliases.Length; ++i)
				Console.WriteLine(aliases[i]);
			Console.WriteLine("----------------------------------------------------");
		}
		*/
	}
}
