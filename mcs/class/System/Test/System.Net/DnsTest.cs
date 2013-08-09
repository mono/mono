// DnsTest.cs - NUnit Test Cases for the System.Net.Dns class
//
// Authors: 
//   Mads Pultz (mpultz@diku.dk)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2001 Mads Pultz
// (C) 2003 Martin Willemoes Hansen
// 
// This test assumes the following:
// 1) The following Internet sites exist:
//        www.go-mono.com with IP address 64.14.94.188
//        info.diku.dk with IP address 130.225.96.4
// 2) The following DNS name does not exist:
//        www.hopefullydoesnotexist.dk
//

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class DnsTest
	{
		private String site1Name = "mono-project.com",
			site1Dot = "96.126.105.110",
			site2Name = "info.diku.dk",
			site2Dot = "130.225.96.4",
			noneExistingSite = "unlikely.xamarin.com";
		private uint site1IP = 1852407392, site2IP = 2195808260; // Big-Endian

		[Test]
		public void AsyncGetHostByName ()
		{
			IAsyncResult r;
			r = Dns.BeginGetHostByName (site1Name, new AsyncCallback (GetHostByNameCallback), null);

			IAsyncResult async = Dns.BeginGetHostByName (site1Name, null, null);
			IPHostEntry entry = Dns.EndGetHostByName (async);
			SubTestValidIPHostEntry (entry);
			Assert.IsTrue (entry.HostName == "www.mono-project.com" || entry.HostName == "mono-project.com");
		}

		void GetHostByNameCallback (IAsyncResult ar)
		{
			IPHostEntry h;
			h = Dns.EndGetHostByName (ar);
			SubTestValidIPHostEntry (h);
		}

		[Test]
		public void AsyncResolve ()
		{
			IAsyncResult r;
			r = Dns.BeginResolve (site1Name, new AsyncCallback (ResolveCallback), null);

			IAsyncResult async = Dns.BeginResolve (site1Dot, null, null);
			IPHostEntry entry = Dns.EndResolve (async);
			SubTestValidIPHostEntry (entry);
			Assert.AreEqual (site1Dot, entry.AddressList [0].ToString ());
		}

		void ResolveCallback (IAsyncResult ar)
		{
			IPHostEntry h = Dns.EndResolve (ar);
			SubTestValidIPHostEntry (h);
		}

#if NET_2_0
		[Test]
		public void BeginGetHostAddresses_HostNameOrAddress_Null ()
		{
			try {
				Dns.BeginGetHostAddresses (
					(string) null,
					new AsyncCallback (GetHostAddressesCallback),
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
					new AsyncCallback (GetHostAddressesCallback),
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
					new AsyncCallback (GetHostAddressesCallback),
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

		void GetHostAddressesCallback (IAsyncResult ar)
		{
			IPAddress [] addresses = Dns.EndGetHostAddresses (ar);
			Assert.IsNotNull (addresses);
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
#endif

		[Test]
		public void GetHostName ()
		{
			string hostName = Dns.GetHostName ();
			Assert.IsNotNull (hostName);
		}

		[Test]
		public void GetHostByName ()
		{
			SubTestGetHostByName ("www.mono-project.com", site1Dot);
			SubTestGetHostByName (site2Name, site2Dot);
			try {
				var entry = Dns.GetHostByName (noneExistingSite);
				/*
				 * Work around broken t-online.de DNS Server.
				 * 
				 * T-Online's DNS Server for DSL Customers resolves
				 * non-exisitng domain names to
				 * http://navigationshilfe1.t-online.de/dnserror?url=....
				 * instead of reporting an error.
				 */
				var navigationshilfe1 = IPAddress.Parse ("80.156.86.78");
				var navigationshilfe2 = IPAddress.Parse ("62.157.140.133");
				foreach (var addr in entry.AddressList) {
					if (addr.Equals (navigationshilfe1) || addr.Equals (navigationshilfe2))
						return;
				}
				Assert.Fail ("Should raise a SocketException (assuming that '" + noneExistingSite + "' does not exist)");
			} catch (SocketException) {
			}
		}

		void SubTestGetHostByName (string siteName, string siteDot)
		{
			IPHostEntry h = Dns.GetHostByName (siteName);
			SubTestValidIPHostEntry (h);
			Assert.AreEqual (siteName, h.HostName, "siteName");
			Assert.AreEqual (siteDot, h.AddressList [0].ToString (), "siteDot");
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
#if TARGET_JVM
		[Ignore ("Ignore failures in Sys.Net")]
#endif
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
			Assert.AreEqual (addr.ToString (), h.AddressList [0].ToString ());
		}

		[Test]
		public void GetHostByAddressIPAddress3 ()
		{
			IPAddress addr = new IPAddress (IPAddress.NetworkToHostOrder ((int) site2IP));
			IPHostEntry h = Dns.GetHostByAddress (addr);
			SubTestValidIPHostEntry (h);
			Assert.AreEqual (addr.ToString (), h.AddressList [0].ToString ());
		}

		[Test]
		public void BeginResolve_HostName_Null ()
		{
			try {
				Dns.BeginResolve ((string) null,
					new AsyncCallback (ResolveCallback),
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
		public void BeginGetHostEntry1_Address_Null ()
		{
			try {
				Dns.BeginGetHostEntry (
					(IPAddress) null,
					new AsyncCallback (GetHostAddressesCallback),
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
		public void BeginGetHostEntry2_HostNameOrAddress_Null ()
		{
			try {
				Dns.BeginGetHostEntry (
					(string) null,
					new AsyncCallback (GetHostAddressesCallback),
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
			Dns.GetHostEntry (string.Empty);
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
