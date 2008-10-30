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
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void Close_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();
				resp.Close ();

				responder.Stop ();
			}
		}

		[Test]
		public void ContentEncoding_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void ContentLength_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				Assert.AreEqual (9, resp.ContentLength);

				responder.Stop ();
			}
		}

		[Test]
		public void ContentType_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void Cookies_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void GetResponseHeader_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void GetResponseStream_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void Headers_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void LastModified_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void Method_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void ProtocolVersion_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void ResponseUri_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void Server_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		[Test]
		public void StatusCode_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				Assert.AreEqual (HttpStatusCode.OK, resp.StatusCode);

				responder.Stop ();
			}
		}

		[Test]
		public void StatusDescription_Disposed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), new SocketRequestHandler (FullResponseHandler))) {
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

				responder.Stop ();
			}
		}

		static byte [] FullResponseHandler (Socket socket)
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
}
