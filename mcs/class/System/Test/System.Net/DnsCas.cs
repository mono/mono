//
// DnsCas.cs - CAS unit tests for System.Net.Dns class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

#if !MOBILE

using NUnit.Framework;

using System;
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace MonoCasTests.System.Net {

	[TestFixture]
	[Category ("CAS")]
	[Category ("NotWorking")] // compiler (CSC) issue (on Windows)
	public class DnsCas {

		private const string site = "www.example.com";
		private const int timeout = 30000;

		static ManualResetEvent reset;
		private string message;
		private string hostname;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			reset = new ManualResetEvent (false);
			hostname = Dns.GetHostName ();
			var ip = Dns.Resolve (site).AddressList[0];
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			reset.Close ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// test Demand by denying it's caller from the required permission

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_BeginGetHostName ()
		{
			Dns.BeginGetHostByName (null, null, null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Deny_EndGetHostByName ()
		{
			Dns.EndGetHostByName (null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_BeginResolve ()
		{
			Dns.BeginResolve (null, null, null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Deny_EndResolve ()
		{
			Dns.EndResolve (null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_GetHostByAddress_IPAddress ()
		{
			Dns.GetHostByAddress ((IPAddress)null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_GetHostByAddress_String ()
		{
			Dns.GetHostByAddress ((string)null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_GetHostByName ()
		{
			Dns.GetHostByName (site);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		// so it's not a declarative attribute on the method as the
		// null check is done before throwing the SecurityException
		public void Deny_GetHostByName_Null ()
		{
			Dns.GetHostByName (null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_GetHostByName_HostName ()
		{
			// ... so my first guess is that you can only query 
			// yourself without having unrestricted DnsPermission
			Assert.IsNotNull (Dns.GetHostByName (hostname));
			// but that's wrong :-(
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_GetHostName ()
		{
			Dns.GetHostName ();
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_Resolve ()
		{
			Dns.Resolve (null);
		}

		// TODO: New 2.0 methods aren't yet implemented in Mono
/*
		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_BeginGetHostAddresses ()
		{
			Dns.BeginGetHostAddresses (null, null, null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Deny_EndGetHostAddresses ()
		{
			Dns.EndGetHostAddresses (null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_BeginGetHostEntry_IPAddress ()
		{
			Dns.BeginGetHostEntry ((IPAddress)null, null, null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_BeginGetHostEntry_String ()
		{
			Dns.BeginGetHostEntry ((string)null, null, null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Deny_EndGetHostEntry ()
		{
			Dns.EndGetHostEntry (null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_GetHostEntry_IPAddress ()
		{
			Dns.GetHostEntry ((IPAddress)null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_GetHostEntry_String ()
		{
			Dns.GetHostEntry ((string)null);
		}

		[Test]
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_GetHostAddresses ()
		{
			Dns.GetHostAddresses (null);
		}
*/

		// ensure that only DnsPermission is required to call the methods

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_BeginGetHostName ()
		{
			Dns.BeginGetHostByName (null, null, null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_BeginResolve ()
		{
			Dns.BeginResolve (null, null, null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_GetHostByAddress_IPAddress ()
		{
			Dns.GetHostByAddress ((IPAddress)null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_GetHostByAddress_String ()
		{
			Dns.GetHostByAddress ((string)null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_GetHostByName ()
		{
			Dns.GetHostByName (null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PermitOnly_GetHostName ()
		{
			Assert.IsNotNull (Dns.GetHostName ());
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_Resolve ()
		{
			Dns.Resolve (null);
		}

		// TODO: New 2.0 methods aren't yet implemented in Mono
/*
		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_BeginGetHostAddresses ()
		{
			Dns.BeginGetHostAddresses (null, null, null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_EndGetHostAddresses ()
		{
			Dns.EndGetHostAddresses (null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_BeginGetHostEntry_IPAddress ()
		{
			Dns.BeginGetHostEntry ((IPAddress)null, null, null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_BeginGetHostEntry_String ()
		{
			Dns.BeginGetHostEntry ((string)null, null, null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_EndGetHostEntry ()
		{
			Dns.EndGetHostEntry (null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_GetHostEntry_IPAddress ()
		{
			Dns.GetHostEntry ((IPAddress)null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_GetHostEntry_String ()
		{
			Dns.GetHostEntry ((string)null);
		}

		[Test]
		[DnsPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermitOnly_GetHostAddresses ()
		{
			Dns.GetHostAddresses (null);
		}
*/

		// async tests (for stack propagation)

		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		private void GetHostByNameCallback (IAsyncResult ar)
		{
			Dns.EndGetHostByName (ar);
			try {
				// can we do something bad here ?
				Assert.IsNotNull (Environment.GetEnvironmentVariable ("USERNAME"));
				message = "Expected a SecurityException";
			}
			catch (SecurityException) {
				message = null;
				reset.Set ();
			}
			catch (Exception e)
			{
				message = e.ToString ();
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		public void AsyncGetHostByName ()
		{
			message = "AsyncGetHostByName";
			reset.Reset ();
			IAsyncResult r = Dns.BeginGetHostByName (site, new AsyncCallback (GetHostByNameCallback), null);
			Assert.IsNotNull (r, "IAsyncResult");
			// note for some reason r.AsyncWaitHandle.Wait won't work as expected
			// if (!r.AsyncWaitHandle.WaitOne (timeout, true))
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		private void ResolveCallback (IAsyncResult ar)
		{
			Dns.EndResolve (ar);
			try {
				// can we do something bad here ?
				Assert.IsNotNull (Environment.GetEnvironmentVariable ("USERNAME"));
				message = "Expected a SecurityException";
			}
			catch (SecurityException) {
				message = null;
				reset.Set ();
			}
			catch (Exception e) {
				message = e.ToString ();
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		public void AsyncResolve ()
		{
			message = "AsyncResolve";
			reset.Reset ();
			IAsyncResult r = Dns.BeginResolve (site, new AsyncCallback (ResolveCallback), null);
			Assert.IsNotNull (r, "IAsyncResult");
			// note for some reason r.AsyncWaitHandle.Wait won't work as expected
			// if (!r.AsyncWaitHandle.WaitOne (timeout, true))
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		// TODO: New 2.0 methods aren't yet implemented in Mono
/*
		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		private void GetHostEntryCallback (IAsyncResult ar)
		{
			Dns.EndGetHostEntry (ar);
			try {
				// can we do something bad here ?
				Assert.IsNotNull (Environment.GetEnvironmentVariable ("USERNAME"));
				message = "Expected a SecurityException";
			}
			catch (SecurityException) {
				message = null;
				reset.Set ();
			}
			catch (Exception e) {
				message = e.ToString ();
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		[Ignore ("fails when EndGetHostEntry is called")]
		public void AsyncGetHostEntry_IPAddress ()
		{
			message = "AsyncGetHostEntry_IPAddress";
			reset.Reset ();
			IAsyncResult r = Dns.BeginGetHostEntry (ip, new AsyncCallback (GetHostEntryCallback), null);
			Assert.IsNotNull (r, "IAsyncResult");
			// note for some reason r.AsyncWaitHandle.Wait won't work as expected
			// if (!r.AsyncWaitHandle.WaitOne (timeout, true))
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		public void AsyncGetHostEntry_String ()
		{
			message = "AsyncGetHostEntry_String";
			reset.Reset ();
			IAsyncResult r = Dns.BeginGetHostEntry (site, new AsyncCallback (GetHostEntryCallback), null);
			Assert.IsNotNull (r, "IAsyncResult");
			// note for some reason r.AsyncWaitHandle.Wait won't work as expected
			// if (!r.AsyncWaitHandle.WaitOne (timeout, true))
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		[DnsPermission (SecurityAction.Deny, Unrestricted = true)]
		private void GetHostAddressesCallback (IAsyncResult ar)
		{
			Dns.EndGetHostEntry (ar);
			try {
				// can we do something bad here ?
				Assert.IsNotNull (Environment.GetEnvironmentVariable ("USERNAME"));
				message = "Expected a SecurityException";
			}
			catch (SecurityException) {
				message = null;
				reset.Set ();
			}
			catch (Exception e) {
				message = e.ToString ();
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		public void AsyncGetHostAddresses ()
		{
			message = "AsyncGetHostAddresses";
			reset.Reset ();
			IAsyncResult r = Dns.BeginGetHostAddresses (site, new AsyncCallback (GetHostAddressesCallback), null);
			Assert.IsNotNull (r, "IAsyncResult");
			// note for some reason r.AsyncWaitHandle.Wait won't work as expected
			// if (!r.AsyncWaitHandle.WaitOne (timeout, true))
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}
*/
	}
}

#endif
