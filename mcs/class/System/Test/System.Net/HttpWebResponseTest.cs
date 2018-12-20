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

using MonoTests.Helpers;

using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class HttpWebResponseTest
	{
		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CharacterSet_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Close_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ContentEncoding_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ContentLength_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ContentType_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Cookies_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void GetResponseHeader_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void GetResponseStream_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Headers_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
				((IDisposable) resp).Dispose ();

				WebHeaderCollection headers = resp.Headers;
				Assert.AreEqual (6, headers.Count, "#1");
				Assert.AreEqual ("9", headers ["Content-Length"], "#2");
				Assert.AreEqual ("identity", headers ["Content-Encoding"], "#3");
				Assert.AreEqual ("text/xml; charset=UTF-8", headers ["Content-Type"], "#4");
				Assert.AreEqual ("Wed, 08 Jan 2003 23:11:55 GMT", headers ["Last-Modified"], "#5");
				Assert.AreEqual ("UserID=Miguel,StoreProfile=true", headers ["Set-Cookie"], "#6");
				Assert.AreEqual ("Mono/Test", headers ["Server"], "#7");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void LastModified_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Method_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ProtocolVersion_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ResponseUri_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Server_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void StatusCode_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void StatusDescription_Disposed ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
			sw.WriteLine ("Content-Encoding: identity");
			sw.WriteLine ("Content-Type: text/xml; charset=UTF-8");
			sw.WriteLine ("Content-Length: 9");
			sw.WriteLine ("Set-Cookie: UserID=Miguel");
			sw.WriteLine ("Set-Cookie: StoreProfile=true");
			sw.WriteLine ();
			sw.Write ("<dummy />");
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		internal static byte [] GzipResponseHandler (Socket socket)
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\r\n";
			sw.WriteLine ("HTTP/1.0 200 OK");
			sw.WriteLine ("Server: Mono/Test");
			sw.WriteLine ("Content-Encoding: gzip");
			sw.WriteLine ("Content-Type: text/xml; charset=UTF-8");
			sw.WriteLine ();
			sw.Flush ();

			var gzipDummyXml = new byte[] {
				0x1f, 0x8b, 0x08, 0x08, 0xb6, 0xb1, 0xd3, 0x58, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x67, 0x7a,
				0x00, 0xb3, 0x49, 0x29, 0xcd, 0xcd, 0xad, 0x54, 0xd0, 0xb7, 0x03, 0x00, 0xed, 0x55, 0x32, 0xec,
				0x09, 0x00, 0x00, 0x00 };
			var header = Encoding.UTF8.GetBytes (sw.ToString ());
			
			var response = new byte[gzipDummyXml.Length + header.Length];
			header.CopyTo(response, 0);
			gzipDummyXml.CopyTo(response, header.Length);

			return response;
		}
	}

	[TestFixture]
	public class HttpResponseStreamTest
	{
		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void BeginRead_Buffer_Null ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void BeginWrite ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CanSeek ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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

		[Test] // bug #324182
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CanTimeout ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CanWrite ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Read ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Read_Buffer_Null ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Read_Count_Negative ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
							Assert.AreEqual ("count", ex.ParamName, "#A5");
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
							Assert.AreEqual ("count", ex.ParamName, "#B5");
						}
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Read_Count_Overflow ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
							Assert.AreEqual ("count", ex.ParamName, "#A5");
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
							Assert.AreEqual ("count", ex.ParamName, "#B5");
						}
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Read_Offset_Negative ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Read_Offset_Overflow ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ReadTimeout ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Write ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void WriteTimeout ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.FullResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
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


		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[Category ("MobileNotWorking")] // https://github.com/xamarin/xamarin-macios/issues/3827
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void AutomaticDecompression ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => HttpWebResponseTest.GzipResponseHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;
				req.AutomaticDecompression = DecompressionMethods.GZip;

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream rs = resp.GetResponseStream ();
					byte [] buffer = new byte [24];
					try {
						// read full response
						Assert.AreEqual (9, rs.Read (buffer, 0, buffer.Length));
						Assert.IsNull (resp.Headers[HttpRequestHeader.ContentEncoding]);
					} finally {
						rs.Close ();
						req.Abort ();
					}
				}
			}
		}
	}
}
