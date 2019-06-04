//
// NetworkStreamCas.cs -CAS unit tests for System.Net.Sockets.NetworkStream
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Text;

namespace MonoCasTests.System.Net.Sockets {

	[TestFixture]
	[Category ("CAS")]
	public class NetworkStreamCas {

		private const int timeout = 30000;
		private string message;

		static ManualResetEvent reset;
		static Socket socket;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			reset = new ManualResetEvent (false);

			IPHostEntry host = Dns.Resolve ("www.example.com");
			IPAddress ip = host.AddressList[0];
			socket = new Socket (ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect (new IPEndPoint (ip, 80));
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			reset.Close ();
			socket.Close ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// async tests (for stack propagation)

		private void ReadCallback (IAsyncResult ar)
		{
			NetworkStream s = (NetworkStream)ar.AsyncState;
			s.EndRead (ar);
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
		public void AsyncRead ()
		{
			message = "AsyncRead";
			reset.Reset ();

			NetworkStream ns = new NetworkStream (socket, false);
			StreamWriter sw = new StreamWriter (ns);
			sw.Write ("GET / HTTP/1.0\n\n");
			sw.Flush ();

			IAsyncResult r = ns.BeginRead (new byte [1024], 0, 1024, new AsyncCallback (ReadCallback), ns);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
			ns.Close ();
		}

		private void WriteCallback (IAsyncResult ar)
		{
			NetworkStream s = (NetworkStream)ar.AsyncState;
			s.EndWrite (ar);
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
		public void AsyncWrite ()
		{
			message = "AsyncWrite";
			reset.Reset ();

			NetworkStream ns = new NetworkStream (socket, false);
			byte[] get = Encoding.ASCII.GetBytes ("GET / HTTP/1.0\n\n");
			IAsyncResult r = ns.BeginWrite (get, 0, get.Length, new AsyncCallback (WriteCallback), ns);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
			ns.Close ();
		}
	}
}
