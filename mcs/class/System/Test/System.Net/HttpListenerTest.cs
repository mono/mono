//
// HttpListenerTest.cs
//	- Unit tests for System.Net.HttpListener
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
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using MonoTests.Helpers;

namespace MonoTests.System.Net {
	[TestFixture]
	public class HttpListenerTest {
		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DefaultProperties ()
		{
			HttpListener listener = new HttpListener ();
			Assert.AreEqual (AuthenticationSchemes.Anonymous, listener.AuthenticationSchemes, "#01");
			Assert.AreEqual (null, listener.AuthenticationSchemeSelectorDelegate, "#02");
			Assert.AreEqual (false, listener.IgnoreWriteExceptions, "#03");
			Assert.AreEqual (false, listener.IsListening, "#03");
			Assert.AreEqual (0, listener.Prefixes.Count, "#04");
			Assert.AreEqual (null, listener.Realm, "#05");
			Assert.AreEqual (false, listener.UnsafeConnectionNtlmAuthentication, "#06");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Start1 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Start ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Stop1 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Stop ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (InvalidOperationException))]
#endif
		public void GetContext1 ()
		{
			HttpListener listener = new HttpListener ();
			// "Please call Start () before calling this method"
			listener.GetContext ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (InvalidOperationException))]
#endif
		public void GetContext2 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Start ();
			// "Please call AddPrefix () before calling this method"
			listener.GetContext ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (InvalidOperationException))]
#endif
		public void BeginGetContext1 ()
		{
			HttpListener listener = new HttpListener ();
			// "Please call Start () before calling this method"
			listener.BeginGetContext (null, null);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void BeginGetContext2 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Start ();
			// One would expect this to fail as BeginGetContext1 does not fail and
			// calling EndGetContext will wait forever.
			// Lame. They should check that we have no prefixes.
			IAsyncResult ares = listener.BeginGetContext (null, null);
			Assert.IsFalse (ares.IsCompleted);
		}

		private bool CanOpenPort(int port)
		{
			try
			{
				using(Socket socket = new Socket (AddressFamily.InterNetwork,
					SocketType.Stream,
					ProtocolType.Tcp))
				{
					socket.Bind (new IPEndPoint (IPAddress.Loopback, port));
					socket.Listen(1);
				}
			}
			catch(Exception) {
				//Can be AccessDeniedException(ports 80/443 need root access) or
				//SocketException because other application is listening
				return false;
			}
			return true;
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DefaultHttpPort ()
		{
			if (!CanOpenPort (80))
				Assert.Ignore ("Can not open port 80 skipping test.");
			using(HttpListener listener = new HttpListener ())
			{
				listener.Prefixes.Add ("http://127.0.0.1/");
				listener.Start ();
				Assert.IsFalse (CanOpenPort (80), "HttpListener is not listening on port 80.");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DefaultHttpsPort ()
		{
			if (!CanOpenPort (443))
				Assert.Ignore ("Can not open port 443 skipping test.");
			using(HttpListener listener = new HttpListener ())
			{
				listener.Prefixes.Add ("https://127.0.0.1/");
				listener.Start ();
				Assert.IsFalse (CanOpenPort (443), "HttpListener is not listening on port 443.");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void TwoListeners_SameAddress ()
		{
			HttpListener listener1 = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var port, "/");
			HttpListener listener2 = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", port, "/hola/");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (HttpListenerException))]
#endif
		public void TwoListeners_SameURL ()
		{
			HttpListener listener1 = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var port, "/hola/");
			HttpListener listener2 = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", port, "/hola/");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (HttpListenerException))]
#endif
		public void MultipleSlashes ()
		{
			// this one throws on Start(), not when adding it.
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var port, "/hola////");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (HttpListenerException))]
#endif
		public void PercentSign ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var port, "/hola%3E/");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CloseBeforeStart ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CloseTwice ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var port, "/hola/");
			listener.Close ();
			listener.Close ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void StartStopStart ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var port, "/hola/");
			listener.Stop ();
			listener.Start ();
			listener.Close ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void StartStopDispose ()
		{
			using (HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var port, "/hola/")) {
				listener.Stop ();
			}
		}
		
		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AbortBeforeStart ()
		{
			HttpListener listener = new HttpListener ();
			listener.Abort ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AbortTwice ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://localhost:", out var port, "/hola/");
			listener.Abort ();
			listener.Abort ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void PropertiesWhenClosed1 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			Assert.AreEqual (AuthenticationSchemes.Anonymous, listener.AuthenticationSchemes, "#01");
			Assert.AreEqual (null, listener.AuthenticationSchemeSelectorDelegate, "#02");
			Assert.AreEqual (false, listener.IgnoreWriteExceptions, "#03");
			Assert.AreEqual (false, listener.IsListening, "#03");
			Assert.AreEqual (null, listener.Realm, "#05");
			Assert.AreEqual (false, listener.UnsafeConnectionNtlmAuthentication, "#06");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ObjectDisposedException))]
#endif
		public void PropertiesWhenClosed2 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			HttpListenerPrefixCollection p = listener.Prefixes;
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ObjectDisposedException))]
#endif
		public void PropertiesWhenClosedSet1 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			listener.AuthenticationSchemes = AuthenticationSchemes.None;
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ObjectDisposedException))]
#endif
		public void PropertiesWhenClosedSet2 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			listener.AuthenticationSchemeSelectorDelegate = null;
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ObjectDisposedException))]
#endif
		public void PropertiesWhenClosedSet3 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			listener.IgnoreWriteExceptions = true;
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ObjectDisposedException))]
#endif
		public void PropertiesWhenClosedSet4 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			listener.Realm = "hola";
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ObjectDisposedException))]
#endif
		public void PropertiesWhenClosedSet5 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			listener.UnsafeConnectionNtlmAuthentication = true;
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void PropertiesWhenClosed3 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			Assert.IsFalse (listener.IsListening);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CloseWhileBegin ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var _, "/closewhilebegin/");
			CallMe cm = new CallMe ();
			listener.BeginGetContext (cm.Callback, listener);
			listener.Close ();
			if (false == cm.Event.WaitOne (3000, false))
				Assert.Fail ("This should not time out.");
			Assert.IsNotNull (cm.Error);
			Assert.AreEqual (typeof (ObjectDisposedException), cm.Error.GetType (), "Exception type");
			cm.Dispose ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AbortWhileBegin ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var _, "/abortwhilebegin/");
			CallMe cm = new CallMe ();
			listener.BeginGetContext (cm.Callback, listener);
			listener.Abort ();
			if (false == cm.Event.WaitOne (3000, false))
				Assert.Fail ("This should not time out.");
			Assert.IsNotNull (cm.Error);
			Assert.AreEqual (typeof (ObjectDisposedException), cm.Error.GetType (), "Exception type");
			cm.Dispose ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (HttpListenerException))]
#endif
		public void CloseWhileGet ()
		{
			// "System.Net.HttpListener Exception : The I/O operation has been aborted
			// because of either a thread exit or an application request
			//   at System.Net.HttpListener.GetContext()
			//   at MonoTests.System.Net.HttpListenerTest.CloseWhileGet()

			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var _, "/closewhileget/");
			RunMe rm = new RunMe (1000, new ThreadStart (listener.Close), new object [0]);
			rm.Start ();
			HttpListenerContext ctx = listener.GetContext ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (HttpListenerException))]
#endif
		public void AbortWhileGet ()
		{
			// "System.Net.HttpListener Exception : The I/O operation has been aborted
			// because of either a thread exit or an application request
			//   at System.Net.HttpListener.GetContext()
			//   at MonoTests.System.Net.HttpListenerTest.CloseWhileGet()

			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://127.0.0.1:", out var _, "/abortwhileget/");
			RunMe rm = new RunMe (1000, new ThreadStart (listener.Abort), new object [0]);
			rm.Start ();
			HttpListenerContext ctx = listener.GetContext ();
		}

		class RunMe {
			Delegate d;
			int delay_ms;
			object [] args;
			public object Result;

			public RunMe (int delay_ms, Delegate d, object [] args)
			{
				this.delay_ms = delay_ms;
				this.d = d;
				this.args = args;
			}

			public void Start ()
			{
				Thread th = new Thread (new ThreadStart (Run));
				th.Start ();
			}

			void Run ()
			{
				Thread.Sleep (delay_ms);
				Result = d.DynamicInvoke (args);
			}
		}

		class CallMe {
			public ManualResetEvent Event = new ManualResetEvent (false);
			public bool Called;
			public HttpListenerContext Context;
			public Exception Error;

			public void Reset ()
			{
				Called = false;
				Context = null;
				Error = null;
				Event.Reset ();
			}

			public void Callback (IAsyncResult ares)
			{
				Called = true;
				if (ares == null) {
					Error = new ArgumentNullException ("ares");
					return;
				}
				
				try {
					HttpListener listener = (HttpListener) ares.AsyncState;
					Context = listener.EndGetContext (ares);
				} catch (Exception e) {
					Error = e;
				}
				Event.Set ();
			}

			public void Dispose ()
			{
				Event.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ConnectionReuse ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://localhost:", out var port, "/", out var uri);

			IPEndPoint expectedIpEndPoint = CreateListenerRequest (listener, uri);

			Assert.AreEqual (expectedIpEndPoint, CreateListenerRequest (listener, uri), "reuse1");
			Assert.AreEqual (expectedIpEndPoint, CreateListenerRequest (listener, uri), "reuse2");
		}

		public IPEndPoint CreateListenerRequest (HttpListener listener, string uri)
		{
			IPEndPoint ipEndPoint = null;
			var mre = new ManualResetEventSlim ();
			listener.BeginGetContext (result => {
				ipEndPoint = ListenerCallback (result);
				mre.Set ();
			}, listener);

			var request = (HttpWebRequest) WebRequest.Create (uri);
			request.Method = "POST";

			// We need to write something
			request.GetRequestStream ().Write (new byte [] {(byte)'a'}, 0, 1);
			request.GetRequestStream ().Dispose ();

			// Send request, socket is created or reused.
			var response = request.GetResponse ();

			// Close response so socket can be reused.
			response.Close ();

			mre.Wait ();

			return ipEndPoint;
		}

		public static IPEndPoint ListenerCallback (IAsyncResult result)
		{
			var listener = (HttpListener) result.AsyncState;
			var context = listener.EndGetContext (result);
			var clientEndPoint = context.Request.RemoteEndPoint;

			// Disposing InputStream should not avoid socket reuse
			context.Request.InputStream.Dispose ();

			// Close OutputStream to send response
			context.Response.OutputStream.Close ();

			return clientEndPoint;
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UserHeaderWithDoubleMultiValue ()
		{
			var l = NetworkHelpers.CreateAndStartHttpListener ("http://localhost:", out var port, "/", out var uri);

			l.BeginGetContext (ar => {
				var ctx = l.EndGetContext (ar);

				var response = ctx.Response;
				response.Headers.Add ("X-Custom-Header", "A");
				response.Headers.Add ("X-Custom-Header", "B");

				response.Close ();
			}, null);

			HttpWebRequest wr = HttpWebRequest.CreateHttp (uri);
			var resp = wr.GetResponse ();
			var vls = resp.Headers.GetValues ("X-Custom-Header");

			Assert.AreEqual (2, vls.Length);

			l.Close ();
		}
		
		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void HttpClientIsDisconnectedCheckForWriteException()
		{
			AutoResetEvent exceptionOccuredEvent = new AutoResetEvent (false);
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://localhost:", out var port, "/", out var uri,
				initializer: (v) => v.IgnoreWriteExceptions = false);
			listener.BeginGetContext (result =>
			{
				HttpListenerContext context = listener.EndGetContext (result);
				context.Response.SendChunked = true;
				context.Request.InputStream.Close ();
				
				var bytes = new byte [1024];
				using(Stream outputStream = context.Response.OutputStream) {
					try {
						while (true) 
							outputStream.Write (bytes, 0, bytes.Length);
					} catch {
						exceptionOccuredEvent.Set ();
					}
				}
			}, null);

			Task.Factory.StartNew (() =>
			{
				var webRequest = (HttpWebRequest)WebRequest.Create (uri);
				webRequest.Method = "POST";
				webRequest.KeepAlive = false;
				Stream requestStream = webRequest.GetRequestStream ();
				requestStream.WriteByte (1);
				requestStream.Close ();
				using (WebResponse response = webRequest.GetResponse ())
				using (Stream stream = response.GetResponseStream ()) {
					byte[] clientBytes = new byte [1024];
					Assert.IsNotNull (stream, "#01");
					stream.Read (clientBytes, 0, clientBytes.Length);
				}
			});

			Assert.IsTrue (exceptionOccuredEvent.WaitOne (15 * 1000), "#02");
		}
	}
}

