//
// TcpClientCas.cs - CAS unit tests for System.Net.Sockets.TcpClient class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Security.Permissions;
using System.Threading;

using MonoTests.System.Net.Sockets;

namespace MonoCasTests.System.Net.Sockets {

	[TestFixture]
	[Category ("CAS")]
	public class TcpClientCas {

		private const int timeout = 30000;

		static ManualResetEvent reset;
		private string message;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			reset = new ManualResetEvent (false);
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

		// async tests (for stack propagation)
		private void ConnectCallback (IAsyncResult ar)
		{
			TcpClient c = (TcpClient)ar.AsyncState;
			c.EndConnect (ar);
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
		[Category ("InetAccess")]
		public void AsyncConnect_StringIntAsyncCallbackObject ()
		{
			TcpClient s = new TcpClient ();
			message = "AsyncConnect";
			reset.Reset ();
			IAsyncResult r = s.BeginConnect ("www.example.com", 80, new AsyncCallback (ConnectCallback), s);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		[Category ("InetAccess")]
		public void AsyncConnect_IPAddressIntAsyncCallbackObject ()
		{
			IPHostEntry host = Dns.Resolve ("www.example.com");
			TcpClient s = new TcpClient ();
			message = "AsyncConnect";
			reset.Reset ();
			IAsyncResult r = s.BeginConnect (host.AddressList[0], 80, new AsyncCallback (ConnectCallback), s);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		[Category ("InetAccess")]
		public void AsyncConnect_IPAddressArrayIntAsyncCallbackObject ()
		{
			IPHostEntry host = Dns.Resolve ("www.example.com");
			TcpClient s = new TcpClient ();
			message = "AsyncConnect";
			reset.Reset ();
			IAsyncResult r = s.BeginConnect (host.AddressList, 80, new AsyncCallback (ConnectCallback), s);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}
	}
}
