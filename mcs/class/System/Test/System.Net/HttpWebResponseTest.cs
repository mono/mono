//
// HttpWebResponseTest.cs - NUnit Test Cases for System.Net.HttpWebResponse
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (c) 2008 Gert Driesen
//

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class HttpWebResponseTest
	{
		[Test]
		public void CharacterSet_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					string charset = resp.CharacterSet;
					Assert.Fail ("#1:" + charset);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		[Test]
		public void Close_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();
				resp.Close ();
			}
		}

		[Test]
		public void ContentEncoding_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					string enc = resp.ContentEncoding;
					Assert.Fail ("#1:" + enc);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		[Test]
		public void ContentLength_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				Assert.AreEqual (9, resp.ContentLength);
			}
		}

		[Test]
		public void ContentType_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					string contentType = resp.ContentType;
					Assert.Fail ("#1:" + contentType);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		[Test]
		public void Cookies_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					CookieCollection cookies = resp.Cookies;
					Assert.Fail ("#A1:" + cookies);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#A5");
				}

				try {
					resp.Cookies = new CookieCollection ();
					Assert.Fail ("#B1");
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#B5");
				}
			}
		}

		[Test]
		public void GetResponseHeader_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					string server = resp.GetResponseHeader ("Server");
					Assert.Fail ("#1:" + server);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		[Test]
		public void GetResponseStream_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					Stream s = resp.GetResponseStream ();
					Assert.Fail ("#1:" + s);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		[Test]
		public void Headers_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

#if NET_2_0
				WebHeaderCollection headers = resp.Headers;
				Assert.AreEqual (6, headers.Count, "#1");
				Assert.AreEqual ("9", headers ["Content-Length"], "#2");
				Assert.AreEqual ("utf-8", headers ["Content-Encoding"], "#3");
				Assert.AreEqual ("text/xml; charset=UTF-8", headers ["Content-Type"], "#4");
				Assert.AreEqual ("Wed, 08 Jan 2003 23:11:55 GMT", headers ["Last-Modified"], "#5");
				Assert.AreEqual ("UserID=Miguel,StoreProfile=true", headers ["Set-Cookie"], "#6");
				Assert.AreEqual ("Mono/Test", headers ["Server"], "#7");
#else
				try {
					WebHeaderCollection headers = resp.Headers;
					Assert.Fail ("#1:" + headers);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
#endif
			}
		}

		[Test]
		public void LastModified_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					DateTime lastMod = resp.LastModified;
					Assert.Fail ("#1:" + lastMod);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		[Test]
		public void Method_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					string method = resp.Method;
					Assert.Fail ("#1:" + method);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		[Test]
		public void ProtocolVersion_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					Version protocolVersion = resp.ProtocolVersion;
					Assert.Fail ("#1:" + protocolVersion);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		[Test]
		public void ResponseUri_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					Uri respUri = resp.ResponseUri;
					Assert.Fail ("#1:" + respUri);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		[Test]
		public void Server_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					string server = resp.Server;
					Assert.Fail ("#1:" + server);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		[Test]
		public void StatusCode_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				Assert.AreEqual (HttpStatusCode.OK, resp.StatusCode);
			}
		}

		[Test]
		public void StatusDescription_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				try {
					string statusDesc = resp.StatusDescription;
					Assert.Fail ("#1:" + statusDesc);
				} catch (ObjectDisposedException ex) {
					Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual (typeof (HttpWebResponse).FullName, ex.ObjectName, "#5");
				}
			}
		}

		internal static byte [] FullResponseHandler (Socket socket)
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\r\n";
			sw.WriteLine ("HTTP/1.0 200 OK");
			sw.WriteLine ("Server: Mono/Test");
			sw.WriteLine ("Last-Modified: Wed, 08 Jan 2003 23:11:55 GMT");
			sw.WriteLine ("Content-Encoding: " + Encoding.UTF8.WebName);
			sw.WriteLine ("Content-Type: text/xml; charset=UTF-8");
			sw.WriteLine ("Content-Length: 9");
			sw.WriteLine ("Set-Cookie: UserID=Miguel");
			sw.WriteLine ("Set-Cookie: StoreProfile=true");
			sw.WriteLine ();
			sw.Write ("<dummy />");
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}
	}

	[TestFixture]
	public class HttpResponseStreamTest
	{
		[Test]
		public void BeginRead_Buffer_Null ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = null;
					try {
						try {
							rs.BeginRead (buffer, 0, 0, null, null);
							Assert.Fail ("#A1");
						} catch (ArgumentNullException ex) {
							Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
							Assert.IsNull (ex.InnerException, "#A3");
							Assert.IsNotNull (ex.Message, "#A4");
							Assert.AreEqual ("buffer", ex.ParamName, "#A5");
						}

						// read full response
						buffer = new byte [24];
						Assert.AreEqual (9, rs.Read (buffer, 0, buffer.Length));

						buffer = null;
						try {
							rs.BeginRead (buffer, 0, 0, null, null);
							Assert.Fail ("#B1");
						} catch (ArgumentNullException ex) {
							Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
							Assert.IsNull (ex.InnerException, "#B3");
							Assert.IsNotNull (ex.Message, "#B4");
							Assert.AreEqual ("buffer", ex.ParamName, "#B5");
						}
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void BeginWrite ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = new byte [5];
					try {
						rs.BeginWrite (buffer, 0, buffer.Length, null, null);
						Assert.Fail ("#1");
					} catch (NotSupportedException ex) {
						// The stream does not support writing
						Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void CanRead ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					try {
						Assert.IsTrue (rs.CanRead, "#1");
						rs.Close ();
						Assert.IsFalse (rs.CanRead, "#2");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void CanSeek ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					try {
						Assert.IsFalse (rs.CanSeek, "#1");
						rs.Close ();
						Assert.IsFalse (rs.CanSeek, "#2");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

#if NET_2_0
		[Test] // bug #324182
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void CanTimeout ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					try {
						Assert.IsTrue (rs.CanTimeout, "#1");
						rs.Close ();
						Assert.IsTrue (rs.CanTimeout, "#2");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}
#endif

		[Test]
		public void CanWrite ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					try {
						Assert.IsFalse (rs.CanWrite, "#1");
						rs.Close ();
						Assert.IsFalse (rs.CanWrite, "#2");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void Read ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = new byte [5];
					try {
						Assert.AreEqual (1, rs.Read (buffer, 4, 1), "#A1");
						Assert.AreEqual (new byte [] { 0x00, 0x00, 0x00, 0x00, 0x3c }, buffer, "#A2");
						Assert.AreEqual (2, rs.Read (buffer, 0, 2), "#B1");
						Assert.AreEqual (new byte [] { 0x64, 0x75, 0x00, 0x00, 0x3c }, buffer, "#B2");
						Assert.AreEqual (4, rs.Read (buffer, 1, 4), "#C1");
						Assert.AreEqual (new byte [] { 0x64, 0x6d, 0x6d, 0x79, 0x20 }, buffer, "#C2");
						Assert.AreEqual (2, rs.Read (buffer, 0, 3), "#D1");
						Assert.AreEqual (new byte [] { 0x2f, 0x3e, 0x6d, 0x79, 0x20 }, buffer, "#D2");
						Assert.AreEqual (0, rs.Read (buffer, 1, 3), "#E1");
						Assert.AreEqual (new byte [] { 0x2f, 0x3e, 0x6d, 0x79, 0x20 }, buffer, "#E2");
						Assert.AreEqual (0, rs.Read (buffer, buffer.Length, 0), "#G1");
						Assert.AreEqual (new byte [] { 0x2f, 0x3e, 0x6d, 0x79, 0x20 }, buffer, "#G2");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void Read_Buffer_Null ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = null;
					try {
						try {
							rs.Read (buffer, 0, 0);
							Assert.Fail ("#A1");
						} catch (ArgumentNullException ex) {
							Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
							Assert.IsNull (ex.InnerException, "#A3");
							Assert.IsNotNull (ex.Message, "#A4");
							Assert.AreEqual ("buffer", ex.ParamName, "#A5");
						}

						// read full response
						buffer = new byte [24];
						Assert.AreEqual (9, rs.Read (buffer, 0, buffer.Length));

						buffer = null;
						try {
							rs.Read (buffer, 0, 0);
							Assert.Fail ("#B1");
						} catch (ArgumentNullException ex) {
							Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
							Assert.IsNull (ex.InnerException, "#B3");
							Assert.IsNotNull (ex.Message, "#B4");
							Assert.AreEqual ("buffer", ex.ParamName, "#B5");
						}
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void Read_Count_Negative ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = new byte [5];
					try {
						try {
							rs.Read (buffer, 1, -1);
							Assert.Fail ("#A1");
						} catch (ArgumentOutOfRangeException ex) {
							// Specified argument was out of the range of valid values
							Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
							Assert.IsNull (ex.InnerException, "#A3");
							Assert.IsNotNull (ex.Message, "#A4");
							Assert.AreEqual ("size", ex.ParamName, "#A5");
						}

						// read full response
						buffer = new byte [24];
						Assert.AreEqual (9, rs.Read (buffer, 0, buffer.Length));

						try {
							rs.Read (buffer, 1, -1);
							Assert.Fail ("#B1");
						} catch (ArgumentOutOfRangeException ex) {
							// Specified argument was out of the range of valid values
							Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
							Assert.IsNull (ex.InnerException, "#B3");
							Assert.IsNotNull (ex.Message, "#B4");
							Assert.AreEqual ("size", ex.ParamName, "#B5");
						}
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void Read_Count_Overflow ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = new byte [5];
					try {
						try {
							rs.Read (buffer, buffer.Length - 2, 3);
							Assert.Fail ("#A1");
						} catch (ArgumentOutOfRangeException ex) {
							// Specified argument was out of the range of valid values
							Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
							Assert.IsNull (ex.InnerException, "#A3");
							Assert.IsNotNull (ex.Message, "#A4");
							Assert.AreEqual ("size", ex.ParamName, "#A5");
						}

						// read full response
						buffer = new byte [24];
						Assert.AreEqual (9, rs.Read (buffer, 0, buffer.Length));

						try {
							rs.Read (buffer, buffer.Length - 2, 3);
							Assert.Fail ("#B1");
						} catch (ArgumentOutOfRangeException ex) {
							// Specified argument was out of the range of valid values
							Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
							Assert.IsNull (ex.InnerException, "#B3");
							Assert.IsNotNull (ex.Message, "#B4");
							Assert.AreEqual ("size", ex.ParamName, "#B5");
						}
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void Read_Offset_Negative ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = new byte [5];
					try {
						try {
							rs.Read (buffer, -1, 0);
							Assert.Fail ("#A1");
						} catch (ArgumentOutOfRangeException ex) {
							// Specified argument was out of the range of valid values
							Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
							Assert.IsNull (ex.InnerException, "#A3");
							Assert.IsNotNull (ex.Message, "#A4");
							Assert.AreEqual ("offset", ex.ParamName, "#A5");
						}

						// read full response
						buffer = new byte [24];
						Assert.AreEqual (9, rs.Read (buffer, 0, buffer.Length));

						try {
							rs.Read (buffer, -1, 0);
							Assert.Fail ("#B1");
						} catch (ArgumentOutOfRangeException ex) {
							// Specified argument was out of the range of valid values
							Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
							Assert.IsNull (ex.InnerException, "#B3");
							Assert.IsNotNull (ex.Message, "#B4");
							Assert.AreEqual ("offset", ex.ParamName, "#B5");
						}
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void Read_Offset_Overflow ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = new byte [5];
					try {
						try {
							rs.Read (buffer, buffer.Length + 1, 0);
							Assert.Fail ("#A1");
						} catch (ArgumentOutOfRangeException ex) {
							// Specified argument was out of the range of valid values
							Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
							Assert.IsNull (ex.InnerException, "#A3");
							Assert.IsNotNull (ex.Message, "#A4");
							Assert.AreEqual ("offset", ex.ParamName, "#A5");
						}

						// read full response
						buffer = new byte [24];
						Assert.AreEqual (9, rs.Read (buffer, 0, buffer.Length));

						try {
							rs.Read (buffer, buffer.Length + 1, 0);
							Assert.Fail ("#B1");
						} catch (ArgumentOutOfRangeException ex) {
							// Specified argument was out of the range of valid values
							Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
							Assert.IsNull (ex.InnerException, "#B3");
							Assert.IsNotNull (ex.Message, "#B4");
							Assert.AreEqual ("offset", ex.ParamName, "#B5");
						}
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Read_Stream_Closed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req;
				
				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					rs.Close ();
					try {
						rs.Read (new byte [0], 0, 0);
						Assert.Fail ("#A1");
					} catch (WebException ex) {
						// The request was aborted: The connection was closed unexpectedly
						Assert.AreEqual (typeof (WebException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
						Assert.IsNull (ex.Response, "#A5");
						Assert.AreEqual (WebExceptionStatus.ConnectionClosed, ex.Status, "#A6");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = new byte [24];
					Assert.AreEqual (9, rs.Read (buffer, 0, buffer.Length));
					rs.Close ();
					try {
						rs.Read (new byte [0], 0, 0);
						Assert.Fail ("#B1");
					} catch (WebException ex) {
						// The request was aborted: The connection was closed unexpectedly
						Assert.AreEqual (typeof (WebException), ex.GetType (), "#B2");
						Assert.IsNull (ex.InnerException, "#B3");
						Assert.IsNotNull (ex.Message, "#B4");
						Assert.IsNull (ex.Response, "#B5");
						Assert.AreEqual (WebExceptionStatus.ConnectionClosed, ex.Status, "#B6");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

#if NET_2_0
		[Test]
		public void ReadTimeout ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					try {
						Assert.AreEqual (2000, rs.ReadTimeout, "#1");
						rs.Close ();
						Assert.AreEqual (2000, rs.ReadTimeout, "#2");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}
#endif

		[Test]
		public void Write ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = new byte [5];
					try {
						rs.Write (buffer, 0, buffer.Length);
						Assert.Fail ("#1");
					} catch (NotSupportedException ex) {
						// The stream does not support writing
						Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

#if NET_2_0
		[Test]
		public void WriteTimeout ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebResponseTest.FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					try {
						Assert.AreEqual (2000, rs.WriteTimeout, "#1");
						rs.Close ();
						Assert.AreEqual (2000, rs.WriteTimeout, "#2");
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}
#endif
	}
}
