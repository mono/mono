//
// SocketCas.cs - CAS unit tests for System.Net.WebRequest class
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
using System.Text;
using System.Threading;

using MonoTests.System.Net.Sockets;

namespace MonoCasTests.System.Net.Sockets {

	[TestFixture]
	[Category ("CAS")]
	public class SocketCas {

		private const int timeout = 30000;

		static ManualResetEvent reset;
		private string message;
		static Socket socket;
		static EndPoint ep;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			reset = new ManualResetEvent (false);

			IPHostEntry host = Dns.Resolve ("www.example.com");
			IPAddress ip = host.AddressList[0];
			ep = new IPEndPoint (ip, 80);
			socket = new Socket (ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect (ep);
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

		private void AcceptCallback (IAsyncResult ar)
		{
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
		public void AsyncAccept ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 16279);
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Bind (ep);
			s.Listen (0);
			message = "AsyncAccept";
			reset.Reset ();
			IAsyncResult r = s.BeginAccept (new AsyncCallback (AcceptCallback), s);
			Assert.IsNotNull (r, "IAsyncResult");

			Socket c = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			c.Connect (ep);

			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		private void ConnectCallback (IAsyncResult ar)
		{
			Socket s = (Socket)ar.AsyncState;
			s.EndConnect (ar);
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
		public void AsyncConnect ()
		{
			message = "AsyncConnect";
			reset.Reset ();

			Socket s = new Socket (ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			IAsyncResult r = s.BeginConnect (ep, new AsyncCallback (ConnectCallback), s);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		private void ReceiveCallback (IAsyncResult ar)
		{
			Socket s = (Socket)ar.AsyncState;
			s.EndReceive (ar);
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
		public void AsyncReceive ()
		{
			message = "AsyncReceive";
			reset.Reset ();

			NetworkStream ns = new NetworkStream (socket, false);
			StreamWriter sw = new StreamWriter (ns);
			sw.Write ("GET / HTTP/1.0\n\n");
			sw.Flush ();

			IAsyncResult r = socket.BeginReceive (new byte[1024], 0, 1024, 
				SocketFlags.None, new AsyncCallback (ReceiveCallback), socket);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		private void ReceiveFromCallback (IAsyncResult ar)
		{
			Socket s = (Socket)ar.AsyncState;
			s.EndReceiveFrom (ar, ref ep);
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
		public void AsyncReceiveFrom ()
		{
			message = "AsyncReceiveFrom";
			reset.Reset ();

			NetworkStream ns = new NetworkStream (socket, false);
			StreamWriter sw = new StreamWriter (ns);
			sw.Write ("GET / HTTP/1.0\n\n");
			sw.Flush ();

			IAsyncResult r = socket.BeginReceiveFrom (new byte[1024], 0, 1024,
				SocketFlags.None, ref ep, new AsyncCallback (ReceiveFromCallback), socket);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		private void SendCallback (IAsyncResult ar)
		{
			Socket s = (Socket)ar.AsyncState;
			s.EndSend (ar);
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
		public void AsyncSend ()
		{
			message = "AsyncSend";
			reset.Reset ();

			byte[] get = Encoding.ASCII.GetBytes ("GET / HTTP/1.0\n\n");
			IAsyncResult r = socket.BeginSend (get, 0, get.Length, SocketFlags.None, 
				new AsyncCallback (SendCallback), socket);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		private void SendToCallback (IAsyncResult ar)
		{
			Socket s = (Socket)ar.AsyncState;
			s.EndSendTo (ar);
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
		public void AsyncSendTo ()
		{
			message = "AsyncSendTo";
			reset.Reset ();

			byte[] get = Encoding.ASCII.GetBytes ("GET / HTTP/1.0\n\n");
			IAsyncResult r = socket.BeginSendTo (get, 0, get.Length, SocketFlags.None, 
				ep, new AsyncCallback (SendToCallback), socket);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}
	}
}
