//
// HttpListener2Test.cs
//	- Unit tests for System.Net.HttpListener - connection testing
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
#if NET_2_0
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;

// ***************************************************************************************
// NOTE: when adding prefixes, make then unique per test, as MS might take 'some time' to
// unregister it even after explicitly closing the listener.
// ***************************************************************************************
namespace MonoTests.System.Net {
	[TestFixture]
	public class HttpListener2Test {
		class MyNetworkStream : NetworkStream {
			public MyNetworkStream (Socket sock) : base (sock, true)
			{
			}

			public Socket GetSocket ()
			{
				return Socket;
			}
		}

		HttpListener CreateAndStartListener (string prefix)
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add (prefix);
			listener.Start ();
			return listener;
		}

		MyNetworkStream CreateNS (int port)
		{
			Socket sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			sock.Connect (new IPEndPoint (IPAddress.Loopback, port));
			return new MyNetworkStream (sock);
		}

		void Send (Stream stream, string str)
		{
			byte [] bytes = Encoding.ASCII.GetBytes (str);
			stream.Write (bytes, 0, bytes.Length);
		}

		string Receive (Stream stream, int size)
		{
			byte [] bytes = new byte [size];
			int nread = stream.Read (bytes, 0, size);
			return Encoding.ASCII.GetString (bytes, 0, nread);
		}

		string ReceiveWithTimeout (Stream stream, int size, int timeout, out bool timed_out)
		{
			byte [] bytes = new byte [size];
			IAsyncResult ares = stream.BeginRead (bytes, 0, size, null, null);
			timed_out = !ares.AsyncWaitHandle.WaitOne (timeout, false);
			if (timed_out)
				return null;
			int nread = stream.EndRead (ares);
			return Encoding.ASCII.GetString (bytes, 0, nread);
		}

		HttpListenerContext GetContextWithTimeout (HttpListener listener, int timeout, out bool timed_out)
		{
			IAsyncResult ares = listener.BeginGetContext (null, null);
			timed_out = !ares.AsyncWaitHandle.WaitOne (timeout, false);
			if (timed_out)
				return null;
			return listener.EndGetContext (ares);
		}

		[Test]
		public void Test1 ()
		{
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test1/");
			NetworkStream ns = CreateNS (9000);
			Send (ns, "GET / HTTP/1.1\r\n\r\n"); // No host
			string response = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (response.StartsWith ("HTTP/1.1 400"));
		}

		[Test]
		public void Test2 ()
		{
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test2/");
			NetworkStream ns = CreateNS (9000);
			Send (ns, "GET / HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"); // no prefix
			string response = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (response.StartsWith ("HTTP/1.1 400"));
		}

		[Test]
		public void Test3 ()
		{
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test3/");
			NetworkStream ns = CreateNS (9000);
			Send (ns, "MET / HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"); // bad method
			string response = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (response.StartsWith ("HTTP/1.1 400"));
		}

		[Test]
		public void Test4 ()
		{
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test4/");
			NetworkStream ns = CreateNS (9000);
			Send (ns, "POST /test4/ HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"); // length required
			string response = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (response.StartsWith ("HTTP/1.1 411"));
		}

		[Test]
		public void Test5 ()
		{
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test5/");
			NetworkStream ns = CreateNS (9000);
			Send (ns, "POST / HTTP/1.1\r\nHost: 127.0.0.1\r\nTransfer-Encoding: pepe\r\n\r\n"); // not implemented
			string response = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (response.StartsWith ("HTTP/1.1 501"));
		}

		[Test]
		public void Test6 ()
		{
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test6/");
			NetworkStream ns = CreateNS (9000);
			 // not implemented! This is against the RFC. Should be a bad request/length required
			Send (ns, "POST /test6/ HTTP/1.1\r\nHost: 127.0.0.1\r\nTransfer-Encoding: identity\r\n\r\n");
			string response = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (response.StartsWith ("HTTP/1.1 501"));
		}

		[Test]
		public void Test7 ()
		{
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test7/");
			NetworkStream ns = CreateNS (9000);
			Send (ns, "POST /test7/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			HttpListenerContext ctx = listener.GetContext ();
			Send (ctx.Response.OutputStream, "%%%OK%%%");
			string response = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (response.StartsWith ("HTTP/1.1 200"));
			Assert.IsTrue (-1 != response.IndexOf ("Transfer-Encoding: chunked"));
		}

		[Test]
		public void Test8 ()
		{
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test8/");
			NetworkStream ns = CreateNS (9000);
			// Just like Test7, but 1.0
			Send (ns, "POST /test8/ HTTP/1.0\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			HttpListenerContext ctx = listener.GetContext ();
			Send (ctx.Response.OutputStream, "%%%OK%%%");
			string response = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (response.StartsWith ("HTTP/1.1 200"));
			Assert.IsTrue (-1 == response.IndexOf ("Transfer-Encoding: chunked"));
		}

		[Test]
		public void Test9 ()
		{
			// 1.0 + "Transfer-Encoding: chunked"
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test9/");
			NetworkStream ns = CreateNS (9000);
			Send (ns, "POST /test9/ HTTP/1.0\r\nHost: 127.0.0.1\r\nTransfer-Encoding: chunked\r\n\r\n3\r\n123\r\n0\r\n\r\n");
			bool timeout;
			string response = ReceiveWithTimeout (ns, 512, 1000, out timeout);
			Assert.IsFalse (timeout);
			listener.Close ();
			ns.Close ();
			Assert.IsTrue (response.StartsWith ("HTTP/1.1 411"));
		}

		[Test]
		public void Test10 ()
		{
			// Same as Test9, but now we shutdown the socket for sending.
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test10/");
			MyNetworkStream ns = CreateNS (9000);
			Send (ns, "POST /test10/ HTTP/1.0\r\nHost: 127.0.0.1\r\nTransfer-Encoding: chunked\r\n\r\n3\r\n123\r\n0\r\n\r\n");
			ns.GetSocket ().Shutdown (SocketShutdown.Send);
			bool timeout;
			string response = ReceiveWithTimeout (ns, 512, 1000, out timeout);
			Assert.IsFalse (timeout);
			listener.Close ();
			ns.Close ();
			Assert.IsTrue (response.StartsWith ("HTTP/1.1 411"));
		}

		[Test]
		public void Test11 ()
		{
			// 0.9
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test11/");
			MyNetworkStream ns = CreateNS (9000);
			Send (ns, "POST /test11/ HTTP/0.9\r\nHost: 127.0.0.1\r\n\r\n123");
			ns.GetSocket ().Shutdown (SocketShutdown.Send);
			string input = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (input.StartsWith ("HTTP/1.1 400"));
		}

		[Test]
		public void Test12 ()
		{
			// 0.9
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test12/");
			MyNetworkStream ns = CreateNS (9000);
			Send (ns, "POST /test12/ HTTP/0.9\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			ns.GetSocket ().Shutdown (SocketShutdown.Send);
			string input = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (input.StartsWith ("HTTP/1.1 400"));
		}

		[Test]
		public void Test13 ()
		{
			// 0.9
			HttpListener listener = CreateAndStartListener ("http://127.0.0.1:9000/test13/");
			MyNetworkStream ns = CreateNS (9000);
			Send (ns, "GEt /test13/ HTTP/0.9\r\nHost: 127.0.0.1\r\n\r\n");
			ns.GetSocket ().Shutdown (SocketShutdown.Send);
			string input = Receive (ns, 512);
			ns.Close ();
			listener.Close ();
			Assert.IsTrue (input.StartsWith ("HTTP/1.1 400"));
		}
	}
}
#endif

