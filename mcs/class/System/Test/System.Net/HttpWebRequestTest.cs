//
// HttpWebRequestTest.cs - NUnit Test Cases for System.Net.HttpWebRequest
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Mono.Security.Authenticode;
#if !MOBILE
using Mono.Security.Protocol.Tls;
#endif

namespace MonoTests.System.Net
{
	[TestFixture]
	public class HttpWebRequestTest
	{
		private Random rand = new Random ();
		private byte [] data64KB = new byte [64 * 1024];

		[TestFixtureSetUp]
		public void Setup ()
		{
				ServicePointManager.Expect100Continue = false;
				rand.NextBytes (data64KB);
		}

		[Test]
#if TARGET_JVM
		[Ignore ("Ignore failures in Sys.Net")]
#endif
		public void Proxy_Null ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://www.google.com");
			Assert.IsNotNull (req.Proxy, "#1");
			req.Proxy = null;
			Assert.IsNull (req.Proxy, "#2");
		}

		[Test]
		[Category("InetAccess")]
#if TARGET_JVM
		[Ignore ("NMA - wrong cookies number returned")]
#endif
		public void Sync ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://www.google.com");
			Assert.IsNotNull (req.IfModifiedSince, "req:If Modified Since: ");

			req.UserAgent = "MonoClient v1.0";
			Assert.AreEqual ("User-Agent", req.Headers.GetKey (0), "#A1");
			Assert.AreEqual ("MonoClient v1.0", req.Headers.Get (0), "#A2");

			HttpWebResponse res = (HttpWebResponse) req.GetResponse ();
			Assert.AreEqual ("OK", res.StatusCode.ToString (), "#B1");
			Assert.AreEqual ("OK", res.StatusDescription, "#B2");

			Assert.AreEqual ("text/html; charset=ISO-8859-1", res.Headers.Get ("Content-Type"), "#C1");
			Assert.IsNotNull (res.LastModified, "#C2");
			Assert.AreEqual (0, res.Cookies.Count, "#C3");

			res.Close ();
		}

		[Test]
		public void AddRange ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://www.google.com");
			req.AddRange (10);
			req.AddRange (50, 90);
			req.AddRange ("bytes", 100); 
			req.AddRange ("bytes", 100, 120);
			Assert.AreEqual ("bytes=10-,50-90,100-,100-120", req.Headers ["Range"], "#1");
			try {
				req.AddRange ("bits", 2000);
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {}
		}

		[Test] // bug #471782
		public void CloseRequestStreamAfterReadingResponse ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9152);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;

				byte [] data = new byte [128];
				req.ContentLength = data.Length;

				Stream rs = req.GetRequestStream ();
				rs.Write (data, 0, data.Length);
				rs.Flush ();

				HttpWebResponse response = (HttpWebResponse) req.GetResponse ();
				response.Close ();

				rs.Close ();

				responder.Stop ();
			}
		}

		[Test]
		[Category("InetAccess")]
		public void Cookies1 ()
		{
			// The purpose of this test is to ensure that the cookies we get from a request
			// are stored in both, the CookieCollection in HttpWebResponse and the CookieContainer
			// in HttpWebRequest.
			// If this URL stops sending *one* and only one cookie, replace it.
			string url = "http://www.elmundo.es";
			CookieContainer cookies = new CookieContainer ();
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
			req.KeepAlive = false;
			req.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv; 1.7.6) Gecko/20050317 Firefox/1.0.2";
			req.CookieContainer = cookies;
			Assert.AreEqual (0, cookies.Count, "#01");
			using (HttpWebResponse res = (HttpWebResponse) req.GetResponse()) {
				CookieCollection coll = req.CookieContainer.GetCookies (new Uri (url));
				Assert.AreEqual (1, coll.Count, "#02");
				Assert.AreEqual (1, res.Cookies.Count, "#03");
				Cookie one = coll [0];
				Cookie two = res.Cookies [0];
				Assert.AreEqual (true, object.ReferenceEquals (one, two), "#04");
			}
		}

#if !TARGET_JVM && !MOBILE
		[Test]
		[Ignore ("Fails on MS.NET")]
		public void SslClientBlock ()
		{
			// This tests that the write request/initread/write body sequence does not hang
			// when using SSL.
			// If there's a regression for this, the test will hang.
			ServicePointManager.CertificatePolicy = new AcceptAllPolicy ();
			try {
				SslHttpServer server = new SslHttpServer ();
				server.Start ();

				string url = String.Format ("https://{0}:{1}/nothing.html", server.IPAddress, server.Port);
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
				request.Method = "POST";
				Stream stream = request.GetRequestStream ();
				byte [] bytes = new byte [100];
				stream.Write (bytes, 0, bytes.Length);
				stream.Close ();
				HttpWebResponse resp = (HttpWebResponse) request.GetResponse ();
				Assert.AreEqual (200, (int) resp.StatusCode, "StatusCode");
				StreamReader sr = new StreamReader (resp.GetResponseStream (), Encoding.UTF8);
				sr.ReadToEnd ();
				sr.Close ();
				resp.Close ();
				server.Stop ();
				if (server.Error != null)
					throw server.Error;
			} finally {
				ServicePointManager.CertificatePolicy = null;
			}
		}
#endif
		[Test]
#if TARGET_JVM
		[Category("NotWorking")]
#endif
		public void Missing_ContentEncoding ()
		{
			ServicePointManager.CertificatePolicy = new AcceptAllPolicy ();
			try {
				BadChunkedServer server = new BadChunkedServer ();
				server.Start ();

				string url = String.Format ("http://{0}:{1}/nothing.html", server.IPAddress, server.Port);
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
				request.Method = "GET";
				HttpWebResponse resp = (HttpWebResponse) request.GetResponse ();
				Assert.AreEqual ("", resp.ContentEncoding);
				resp.Close ();
				server.Stop ();
				if (server.Error != null)
					throw server.Error;
			} finally {
				ServicePointManager.CertificatePolicy = null;
			}
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void BadServer_ChunkedClose ()
		{
			// The server will send a chunked response without a 'last-chunked' mark
			// and then shutdown the socket for sending.
			BadChunkedServer server = new BadChunkedServer ();
			server.Start ();
			string url = String.Format ("http://{0}:{1}/nothing.html", server.IPAddress, server.Port);
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
			HttpWebResponse resp = (HttpWebResponse) request.GetResponse ();
			string x = null;
			try {
				byte [] bytes = new byte [32];
				// Using StreamReader+UTF8Encoding here fails on MS runtime
				Stream stream = resp.GetResponseStream ();
				int nread = stream.Read (bytes, 0, 32);
				Assert.AreEqual (16, nread, "#01");
				x = Encoding.ASCII.GetString (bytes, 0, 16);
			} finally {
				resp.Close ();
				server.Stop ();
			}

			if (server.Error != null)
				throw server.Error;

			Assert.AreEqual ("1234567890123456", x);
		}

		[Test]
		[Ignore ("This test asserts that our code violates RFC 2616")]
		public void MethodCase ()
		{
			ListDictionary methods = new ListDictionary ();
			methods.Add ("post", "POST");
			methods.Add ("puT", "PUT");
			methods.Add ("POST", "POST");
			methods.Add ("whatever", "whatever");
			methods.Add ("PUT", "PUT");

			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9153);
			string url = "http://" + ep.ToString () + "/test/";

			foreach (DictionaryEntry de in methods) {
				SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler));
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = (string) de.Key;
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;
				Stream rs = req.GetRequestStream ();
				rs.Close ();
				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					StreamReader sr = new StreamReader (resp.GetResponseStream (),
						Encoding.UTF8);
					string line = sr.ReadLine ();
					sr.Close ();
					Assert.AreEqual (((string) de.Value) + " /test/ HTTP/1.1",
						line, req.Method);
					resp.Close ();
				}
				responder.Stop ();
			}
		}

		[Test]
		public void BeginGetRequestStream_Body_NotAllowed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9154);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest request;

				request = (HttpWebRequest) WebRequest.Create (url);
				request.Method = "GET";

				try {
					request.BeginGetRequestStream (null, null);
					Assert.Fail ("#A1");
				} catch (ProtocolViolationException ex) {
					// Cannot send a content-body with this
					// verb-type
					Assert.IsNull (ex.InnerException, "#A2");
					Assert.IsNotNull (ex.Message, "#A3");
				}

				request = (HttpWebRequest) WebRequest.Create (url);
				request.Method = "HEAD";

				try {
					request.BeginGetRequestStream (null, null);
					Assert.Fail ("#B1");
				} catch (ProtocolViolationException ex) {
					// Cannot send a content-body with this
					// verb-type
					Assert.IsNull (ex.InnerException, "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
				}
			}
		}

		[Test] // bug #465613
		public void BeginGetRequestStream_NoBuffering ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 11001);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req;
				Stream rs;
				IAsyncResult ar;

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = false;
				req.AllowWriteStreamBuffering = false;

				ar = req.BeginGetRequestStream (null, null);
				rs = req.EndGetRequestStream (ar);
				rs.Close ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = true;
				req.AllowWriteStreamBuffering = false;

				try {
					req.BeginGetRequestStream (null, null);
					Assert.Fail ("#A1");
				} catch (ProtocolViolationException ex) {
					// When performing a write operation with
					// AllowWriteStreamBuffering set to false,
					// you must either set ContentLength to a
					// non-negative number or set SendChunked
					// to true
					Assert.IsNull (ex.InnerException, "#A2");
					Assert.IsNotNull (ex.Message, "#A3");
				}

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = true;
				req.AllowWriteStreamBuffering = false;
				req.ContentLength = 0;

				ar = req.BeginGetRequestStream (null, null);
				rs = req.EndGetRequestStream (ar);
				rs.Close ();
			}
		}

		[Test] // bug #508027
		[Category ("NotWorking")] // #5842
		public void BeginGetResponse ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8001);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req;

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Timeout = 5000;
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = false;
				req.AllowWriteStreamBuffering = false;
				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Timeout = 5000;
				req.Method = "POST";
				req.SendChunked = true;
				req.KeepAlive = false;
				req.AllowWriteStreamBuffering = false;
				req.GetRequestStream ().WriteByte (1);
				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Timeout = 5000;
				req.Method = "POST";
				req.ContentLength = 5;
				req.SendChunked = false;
				req.KeepAlive = false;
				req.AllowWriteStreamBuffering = false;
				req.GetRequestStream ().WriteByte (5);
				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Timeout = 5000;
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = true;
				req.AllowWriteStreamBuffering = false;

				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Timeout = 5000;
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = false;
				req.AllowWriteStreamBuffering = false;
				req.ContentLength = 5;
				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Timeout = 5000;
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = true;
				req.AllowWriteStreamBuffering = false;
				req.ContentLength = 5;
				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Timeout = 5000;
				req.Method = "GET";
				req.SendChunked = true;

				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Timeout = 5000;
				req.Method = "GET";
				req.ContentLength = 5;

				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Timeout = 5000;
				req.Method = "GET";
				req.ContentLength = 0;

				req.BeginGetResponse (null, null);
				req.Abort ();
			}
		}

		[Test] // bug #511851
		public void BeginGetRequestStream_Request_Aborted ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8002);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Abort ();

				try {
					req.BeginGetRequestStream (null, null);
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// The request was aborted: The request was canceled
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.Response, "#5");
					Assert.AreEqual (WebExceptionStatus.RequestCanceled, ex.Status, "#6");
				}
			}
		}

		[Test] // bug #511851
		public void BeginGetResponse_Request_Aborted ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9155);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Abort ();

				try {
					req.BeginGetResponse (null, null);
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// The request was aborted: The request was canceled
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.Response, "#5");
					Assert.AreEqual (WebExceptionStatus.RequestCanceled, ex.Status, "#6");
				}
			}
		}

		[Test]
		public void EndGetRequestStream_AsyncResult_Null ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9156);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.BeginGetRequestStream (null, null);

				try {
					req.EndGetRequestStream (null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("asyncResult", ex.ParamName, "#5");
				} finally {
					req.Abort ();
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // do not get consistent result on MS
		public void EndGetRequestStream_Request_Aborted ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8003);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				IAsyncResult ar = req.BeginGetRequestStream (null, null);
				req.Abort ();
				Thread.Sleep (500);

				try {
					req.EndGetRequestStream (ar);
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// The request was aborted: The request was canceled
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.Response, "#5");
					Assert.AreEqual (WebExceptionStatus.RequestCanceled, ex.Status, "#6");
				}
			}
		}

		[Test] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=471522
		[Category ("NotWorking")]
		public void EndGetResponse_AsyncResult_Invalid ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9157);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				IAsyncResult ar = req.BeginGetRequestStream (null, null);

				// AsyncResult was not returned from call to BeginGetResponse
				try {
					req.EndGetResponse (ar);
					Assert.Fail ();
				} catch (InvalidCastException) {
				} finally {
					req.Abort ();
				}
			}
		}

		[Test]
		public void EndGetResponse_AsyncResult_Null ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9158);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.Method = "POST";
				IAsyncResult ar = req.BeginGetResponse (null, null);

				try {
					req.EndGetResponse (null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("asyncResult", ex.ParamName, "#5");
				} finally {
					req.Abort ();
					/*
					using (HttpWebResponse resp = (HttpWebResponse) req.EndGetResponse (ar)) {
						resp.Close ();
					}*/
				}
			}
		}

		[Test] // bug #429200
		public void GetRequestStream ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 10000);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;

				Stream rs1 = req.GetRequestStream ();
				Stream rs2 = req.GetRequestStream ();

				Assert.IsNotNull (rs1, "#1");
				Assert.AreSame (rs1, rs2, "#2");

				rs1.Close ();
			}
		}

		[Test] // bug #511851
		public void GetRequestStream_Request_Aborted ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 10001);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Abort ();

				try {
					req.GetRequestStream ();
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// The request was aborted: The request was canceled
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.Response, "#5");
					Assert.AreEqual (WebExceptionStatus.RequestCanceled, ex.Status, "#6");
				}
			}
		}

		[Test] // bug #510661
		[Category ("NotWorking")] // #5842
		public void GetRequestStream_Close_NotAllBytesWritten ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 10002);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req;
				Stream rs;

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.ContentLength = 2;
				rs = req.GetRequestStream ();
				try {
					rs.Close ();
					Assert.Fail ("#A1");
				} catch (WebException ex) {
					// The request was aborted: The request was canceled
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#A2");
					Assert.IsNotNull (ex.Message, "#A3");
					Assert.IsNull (ex.Response, "#A4");
					Assert.AreEqual (WebExceptionStatus.RequestCanceled, ex.Status, "#A5");

					// Cannot close stream until all bytes are written
					Exception inner = ex.InnerException;
					Assert.IsNotNull (inner, "#A6");
					Assert.AreEqual (typeof (IOException), inner.GetType (), "#A7");
					Assert.IsNull (inner.InnerException, "#A8");
					Assert.IsNotNull (inner.Message, "#A9");
				}

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.ContentLength = 2;
				rs = req.GetRequestStream ();
				rs.WriteByte (0x0d);
				try {
					rs.Close ();
					Assert.Fail ("#B1");
				} catch (WebException ex) {
					// The request was aborted: The request was canceled
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
					Assert.IsNull (ex.Response, "#B4");
					Assert.AreEqual (WebExceptionStatus.RequestCanceled, ex.Status, "#B5");

					// Cannot close stream until all bytes are written
					Exception inner = ex.InnerException;
					Assert.IsNotNull (inner, "#B6");
					Assert.AreEqual (typeof (IOException), inner.GetType (), "#B7");
					Assert.IsNull (inner.InnerException, "#B8");
					Assert.IsNotNull (inner.Message, "#B9");
				}

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.ContentLength = 2;
				rs = req.GetRequestStream ();
				rs.WriteByte (0x0d);
				rs.WriteByte (0x0d);
				rs.Close ();
			}
		}

		[Test] // bug #510642
		[Category ("NotWorking")] // #5842
		public void GetRequestStream_Write_Overflow ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8010);
			string url = "http://" + ep.ToString () + "/test/";

			// buffered, non-chunked
			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req;
				Stream rs;
				byte [] buffer;

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 1000;
				req.ReadWriteTimeout = 2000;
				req.ContentLength = 2;

				rs = req.GetRequestStream ();
				rs.WriteByte (0x2c);

				buffer = new byte [] { 0x2a, 0x1d };
				try {
					rs.Write (buffer, 0, buffer.Length);
					Assert.Fail ("#A1");
				} catch (ProtocolViolationException ex) {
					// Bytes to be written to the stream exceed
					// Content-Length bytes size specified
					Assert.IsNull (ex.InnerException, "#A2");
					Assert.IsNotNull (ex.Message, "#A3");
				} finally {
					req.Abort ();
				}

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 1000;
				req.ReadWriteTimeout = 2000;
				req.ContentLength = 2;

				rs = req.GetRequestStream ();

				buffer = new byte [] { 0x2a, 0x2c, 0x1d };
				try {
					rs.Write (buffer, 0, buffer.Length);
					Assert.Fail ("#B1");
				} catch (ProtocolViolationException ex) {
					// Bytes to be written to the stream exceed
					// Content-Length bytes size specified
					Assert.IsNull (ex.InnerException, "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
				} finally {
					req.Abort ();
				}
			}

			// buffered, chunked
			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req;
				Stream rs;
				byte [] buffer;

				/*
				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.SendChunked = true;
				req.Timeout = 1000;
				req.ReadWriteTimeout = 2000;
				req.ContentLength = 2;

				rs = req.GetRequestStream ();
				rs.WriteByte (0x2c);

				buffer = new byte [] { 0x2a, 0x1d };
				rs.Write (buffer, 0, buffer.Length);
				req.Abort ();
				*/

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.SendChunked = true;
				req.Timeout = 1000;
				req.ReadWriteTimeout = 2000;
				req.ContentLength = 2;

				rs = req.GetRequestStream ();

				buffer = new byte [] { 0x2a, 0x2c, 0x1d };
				rs.Write (buffer, 0, buffer.Length);
				req.Abort ();
			}

			// non-buffered, non-chunked
			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req;
				Stream rs;
				byte [] buffer;

				req = (HttpWebRequest) WebRequest.Create (url);
				req.AllowWriteStreamBuffering = false;
				req.Method = "POST";
				req.Timeout = 1000;
				req.ReadWriteTimeout = 2000;
				req.ContentLength = 2;

				rs = req.GetRequestStream ();
				rs.WriteByte (0x2c);

				buffer = new byte [] { 0x2a, 0x1d };
				try {
					rs.Write (buffer, 0, buffer.Length);
					Assert.Fail ("#C1");
				} catch (ProtocolViolationException ex) {
					// Bytes to be written to the stream exceed
					// Content-Length bytes size specified
					Assert.IsNull (ex.InnerException, "#C2");
					Assert.IsNotNull (ex.Message, "#3");
				} finally {
					req.Abort ();
				}

				req = (HttpWebRequest) WebRequest.Create (url);
				req.AllowWriteStreamBuffering = false;
				req.Method = "POST";
				req.Timeout = 1000;
				req.ReadWriteTimeout = 2000;
				req.ContentLength = 2;

				rs = req.GetRequestStream ();

				buffer = new byte [] { 0x2a, 0x2c, 0x1d };
				try {
					rs.Write (buffer, 0, buffer.Length);
					Assert.Fail ("#D1");
				} catch (ProtocolViolationException ex) {
					// Bytes to be written to the stream exceed
					// Content-Length bytes size specified
					Assert.IsNull (ex.InnerException, "#D2");
					Assert.IsNotNull (ex.Message, "#D3");
				} finally {
					req.Abort ();
				}
			}

			// non-buffered, chunked
			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req;
				Stream rs;
				byte [] buffer;

				req = (HttpWebRequest) WebRequest.Create (url);
				req.AllowWriteStreamBuffering = false;
				req.Method = "POST";
				req.SendChunked = true;
				req.Timeout = 1000;
				req.ReadWriteTimeout = 2000;
				req.ContentLength = 2;

				rs = req.GetRequestStream ();
				rs.WriteByte (0x2c);

				buffer = new byte [] { 0x2a, 0x1d };
				rs.Write (buffer, 0, buffer.Length);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.AllowWriteStreamBuffering = false;
				req.Method = "POST";
				req.SendChunked = true;
				req.Timeout = 1000;
				req.ReadWriteTimeout = 2000;
				req.ContentLength = 2;

				rs = req.GetRequestStream ();

				buffer = new byte [] { 0x2a, 0x2c, 0x1d };
				rs.Write (buffer, 0, buffer.Length);
				req.Abort ();
			}
		}

		[Test]
		[Ignore ("This test asserts that our code violates RFC 2616")]
		public void GetRequestStream_Body_NotAllowed ()
		{
			string [] methods = new string [] { "GET", "HEAD", "CONNECT",
				"get", "HeAd", "ConNect" };

			foreach (string method in methods) {
				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (
					"http://localhost:8000");
				req.Method = method;
				try {
					req.GetRequestStream ();
					Assert.Fail ("#1:" + method);
				} catch (ProtocolViolationException ex) {
					Assert.AreEqual (typeof (ProtocolViolationException), ex.GetType (), "#2:" + method);
					Assert.IsNull (ex.InnerException, "#3:" + method);
					Assert.IsNotNull (ex.Message, "#4:" + method);
				}
			}
		}

		[Test] // bug #511851
		public void GetResponse_Request_Aborted ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 10100);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Abort ();

				try {
					req.GetResponse ();
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// The request was aborted: The request was canceled
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.Response, "#5");
					Assert.AreEqual (WebExceptionStatus.RequestCanceled, ex.Status, "#6");
				}
			}
		}

		[Test]
#if TARGET_JVM
		[Category("NotWorking")]
#endif
		[Ignore ("This does not timeout any more. That's how MS works when reading small responses")]
		public void ReadTimeout ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8764);
			string url = "http://" + localEP.ToString () + "/original/";

			using (SocketResponder responder = new SocketResponder (localEP, new SocketRequestHandler (RedirectRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.AllowAutoRedirect = false;
				req.Timeout = 200;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;
				Stream rs = req.GetRequestStream ();
				rs.Close ();
				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					try {
						Stream s = resp.GetResponseStream ();
						s.ReadByte ();
						Assert.Fail ("#1");
					} catch (WebException ex) {
						Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNull (ex.Response, "#4");
						Assert.AreEqual (WebExceptionStatus.Timeout, ex.Status, "#5");
					}
				}
				responder.Stop ();
			}
		}

		[Test] // bug #324300
#if TARGET_JVM
		[Category("NotWorking")]
#endif
		public void AllowAutoRedirect ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8765);
			string url = "http://" + localEP.ToString () + "/original/";

			// allow autoredirect
			using (SocketResponder responder = new SocketResponder (localEP, new SocketRequestHandler (RedirectRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;
				Stream rs = req.GetRequestStream ();
				rs.Close ();
				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					StreamReader sr = new StreamReader (resp.GetResponseStream (),
						Encoding.UTF8);
					string body = sr.ReadToEnd ();

					Assert.AreEqual (resp.StatusCode, HttpStatusCode.OK, "#A1");
					Assert.AreEqual (resp.ResponseUri.ToString (), "http://" +
						localEP.ToString () + "/moved/", "#A2");
					Assert.AreEqual ("GET", resp.Method, "#A3");
					Assert.AreEqual ("LOOKS OK", body, "#A4");
				}
				responder.Stop ();
			}

			// do not allow autoredirect
			using (SocketResponder responder = new SocketResponder (localEP, new SocketRequestHandler (RedirectRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.AllowAutoRedirect = false;
				req.Timeout = 1000;
				req.ReadWriteTimeout = 1000;
				req.KeepAlive = false;
				Stream rs = req.GetRequestStream ();
				rs.Close ();
				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Assert.AreEqual (resp.StatusCode, HttpStatusCode.Found, "#B1");
					Assert.AreEqual (url, resp.ResponseUri.ToString (), "#B2");
					Assert.AreEqual ("POST", resp.Method, "#B3");
				}
				responder.Stop ();
			}
		}

		[Test]
		public void PostAndRedirect_NoCL ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8769);
			string url = "http://" + localEP.ToString () + "/original/";

			using (SocketResponder responder = new SocketResponder (localEP, new SocketRequestHandler (RedirectRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				Stream rs = req.GetRequestStream ();
				rs.WriteByte (10);
				rs.Close ();
				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					StreamReader sr = new StreamReader (resp.GetResponseStream (),
						Encoding.UTF8);
					string body = sr.ReadToEnd ();

					Assert.AreEqual (resp.StatusCode, HttpStatusCode.OK, "#A1");
					Assert.AreEqual (resp.ResponseUri.ToString (), "http://" +
						localEP.ToString () + "/moved/", "#A2");
					Assert.AreEqual ("GET", resp.Method, "#A3");
					Assert.AreEqual ("LOOKS OK", body, "#A4");
				}
				responder.Stop ();
			}
		}

		[Test]
		public void PostAndRedirect_CL ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8770);
			string url = "http://" + localEP.ToString () + "/original/";

			using (SocketResponder responder = new SocketResponder (localEP, new SocketRequestHandler (RedirectRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.ContentLength  = 1;
				Stream rs = req.GetRequestStream ();
				rs.WriteByte (10);
				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					StreamReader sr = new StreamReader (resp.GetResponseStream (),
						Encoding.UTF8);
					string body = sr.ReadToEnd ();

					Assert.AreEqual (resp.StatusCode, HttpStatusCode.OK, "#A1");
					Assert.AreEqual (resp.ResponseUri.ToString (), "http://" +
						localEP.ToString () + "/moved/", "#A2");
					Assert.AreEqual ("GET", resp.Method, "#A3");
					Assert.AreEqual ("LOOKS OK", body, "#A4");
				}
				responder.Stop ();
			}
		}

		[Test]
		public void PostAnd401 ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8771);
			string url = "http://" + localEP.ToString () + "/original/";

			using (SocketResponder responder = new SocketResponder (localEP, new SocketRequestHandler (RedirectRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.ContentLength  = 1;
				Stream rs = req.GetRequestStream ();
				rs.WriteByte (10);
				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					StreamReader sr = new StreamReader (resp.GetResponseStream (),
						Encoding.UTF8);
					string body = sr.ReadToEnd ();

					Assert.AreEqual (resp.StatusCode, HttpStatusCode.OK, "#A1");
					Assert.AreEqual (resp.ResponseUri.ToString (), "http://" +
						localEP.ToString () + "/moved/", "#A2");
					Assert.AreEqual ("GET", resp.Method, "#A3");
					Assert.AreEqual ("LOOKS OK", body, "#A4");
				}
				responder.Stop ();
			}
		}

		[Test] // bug #324347
		[Category ("NotWorking")]
		public void InternalServerError ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8766);
			string url = "http://" + localEP.ToString () + "/original/";

			// POST
			using (SocketResponder responder = new SocketResponder (localEP, new SocketRequestHandler (InternalErrorHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;
				Stream rs = req.GetRequestStream ();
				rs.Close ();

				try {
					req.GetResponse ();
					Assert.Fail ("#A1");
				} catch (WebException ex) {
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.AreEqual (WebExceptionStatus.ProtocolError, ex.Status, "#A5");

					HttpWebResponse webResponse = ex.Response as HttpWebResponse;
					Assert.IsNotNull (webResponse, "#A6");
					Assert.AreEqual ("POST", webResponse.Method, "#A7");
					webResponse.Close ();
				}

				responder.Stop ();
			}

			// GET
			using (SocketResponder responder = new SocketResponder (localEP, new SocketRequestHandler (InternalErrorHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				try {
					req.GetResponse ();
					Assert.Fail ("#B1");
				} catch (WebException ex) {
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.AreEqual (WebExceptionStatus.ProtocolError, ex.Status, "#B4");

					HttpWebResponse webResponse = ex.Response as HttpWebResponse;
					Assert.IsNotNull (webResponse, "#B5");
					Assert.AreEqual ("GET", webResponse.Method, "#B6");
					webResponse.Close ();
				}

				responder.Stop ();
			}
		}

		[Test]
		[Category ("NotWorking")] // #B3 fails; we get a SocketException: An existing connection was forcibly closed by the remote host
		public void NoContentLength ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8767);
			string url = "http://" + localEP.ToString () + "/original/";

			// POST
			using (SocketResponder responder = new SocketResponder (localEP, new SocketRequestHandler (NoContentLengthHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;
				Stream rs = req.GetRequestStream ();
				rs.Close ();

				try {
					req.GetResponse ();
					Assert.Fail ("#A1");
				} catch (WebException ex) {
					// The underlying connection was closed:
					// An unexpected error occurred on a
					// receive
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#A2");
					Assert.IsNotNull (ex.InnerException, "#A3");
					Assert.AreEqual (WebExceptionStatus.ReceiveFailure, ex.Status, "#A4");
					Assert.AreEqual (typeof (IOException), ex.InnerException.GetType (), "#A5");
					
					// Unable to read data from the transport connection:
					// A connection attempt failed because the connected party
					// did not properly respond after a period of time, or
					// established connection failed because connected host has
					// failed to respond
					IOException ioe = (IOException) ex.InnerException;
					Assert.IsNotNull (ioe.InnerException, "#A6");
					Assert.IsNotNull (ioe.Message, "#A7");
					Assert.AreEqual (typeof (SocketException), ioe.InnerException.GetType (), "#A8");

					// An existing connection was forcibly
					// closed by the remote host
					SocketException soe = (SocketException) ioe.InnerException;
					Assert.IsNull (soe.InnerException, "#A9");
					Assert.IsNotNull (soe.Message, "#A10");

					HttpWebResponse webResponse = ex.Response as HttpWebResponse;
					Assert.IsNull (webResponse, "#A11");
				}

				responder.Stop ();
			}

			// GET
			using (SocketResponder responder = new SocketResponder (localEP, new SocketRequestHandler (NoContentLengthHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.Timeout = 2000;
				req.ReadWriteTimeout = 2000;
				req.KeepAlive = false;

				try {
					req.GetResponse ();
					Assert.Fail ("#B1");
				} catch (WebException ex) {
					// The remote server returned an error:
					// (500) Internal Server Error
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.AreEqual (WebExceptionStatus.ProtocolError, ex.Status, "#B4");

					HttpWebResponse webResponse = ex.Response as HttpWebResponse;
					Assert.IsNotNull (webResponse, "#B5");
					Assert.AreEqual ("GET", webResponse.Method, "#B6");
					webResponse.Close ();
				}

				responder.Stop ();
			}
		}

		[Test] // bug #513087
		public void NonStandardVerb ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8123);
			string url = "http://" + ep.ToString () + "/moved/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (VerbEchoHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "WhatEver";
				req.KeepAlive = false;
				req.Timeout = 20000;
				req.ReadWriteTimeout = 20000;

				Stream rs = req.GetRequestStream ();
				rs.Close ();

				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					StreamReader sr = new StreamReader (resp.GetResponseStream (),
						Encoding.UTF8);
					string body = sr.ReadToEnd ();

					Assert.AreEqual (resp.StatusCode, HttpStatusCode.OK, "#1");
					Assert.AreEqual (resp.ResponseUri.ToString (), "http://" +
						ep.ToString () + "/moved/", "#2");
					Assert.AreEqual ("WhatEver", resp.Method, "#3");
					Assert.AreEqual ("WhatEver", body, "#4");
				}

				responder.Stop ();
			}
		}

		[Test]
		[Category ("NotWorking")] // Assert #2 fails
		public void NotModifiedSince ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9123);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (NotModifiedSinceHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.KeepAlive = false;
				req.Timeout = 20000;
				req.ReadWriteTimeout = 20000;
				req.Headers.Add (HttpRequestHeader.IfNoneMatch, "898bbr2347056cc2e096afc66e104653");
				req.IfModifiedSince = new DateTime (2010, 01, 04);

				DateTime start = DateTime.Now;
				HttpWebResponse response = null;

				try {
					req.GetResponse ();
					Assert.Fail ("#1");
				} catch (WebException e) {
					response = (HttpWebResponse) e.Response;
				}

				Assert.IsNotNull (response, "#2");
				using (Stream stream = response.GetResponseStream ()) {
					byte [] buffer = new byte [4096];
					int bytesRead = stream.Read (buffer, 0, buffer.Length);
					Assert.AreEqual (0, bytesRead, "#3");
				}

				TimeSpan elapsed = DateTime.Now - start;
				Assert.IsTrue (elapsed.TotalMilliseconds < 2000, "#4");

				responder.Stop ();
			}
		}

		[Test] // bug #353495
		[Category ("NotWorking")]
		public void LastModifiedKind ()
		{
			const string reqURL = "http://coffeefaq.com/site/node/25";
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create (reqURL);
			HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
			DateTime lastMod = resp.LastModified;
			//string rawLastMod = resp.Headers ["Last-Modified"];
			resp.Close ();
			//Assert.AreEqual ("Tue, 15 Jan 2008 08:59:59 GMT", rawLastMod, "#1");
			Assert.AreEqual (DateTimeKind.Local, lastMod.Kind, "#2");
			req = (HttpWebRequest) WebRequest.Create (reqURL);
			req.IfModifiedSince = lastMod;
			try {
				resp = (HttpWebResponse) req.GetResponse ();
				resp.Close ();
				Assert.Fail ("Should result in 304");
			} catch (WebException ex) {
				Assert.AreEqual (WebExceptionStatus.ProtocolError, ex.Status, "#3");
				Assert.AreEqual (((HttpWebResponse) ex.Response).StatusCode, HttpStatusCode.NotModified, "#4");
			}
		}

		internal static byte [] EchoRequestHandler (Socket socket)
		{
			MemoryStream ms = new MemoryStream ();
			byte [] buffer = new byte [4096];
			int bytesReceived = socket.Receive (buffer);
			while (bytesReceived > 0) {
				ms.Write (buffer, 0, bytesReceived);
				 // We don't check for Content-Length or anything else here, so we give the client a little time to write
				 // after sending the headers
				Thread.Sleep (200);
				if (socket.Available > 0) {
					bytesReceived = socket.Receive (buffer);
				} else {
					bytesReceived = 0;
				}
			}
			ms.Flush ();
			ms.Position = 0;
			StreamReader sr = new StreamReader (ms, Encoding.UTF8);
			string request = sr.ReadToEnd ();

			StringWriter sw = new StringWriter ();
			sw.WriteLine ("HTTP/1.1 200 OK");
			sw.WriteLine ("Content-Type: text/xml");
			sw.WriteLine ("Content-Length: " + request.Length.ToString (CultureInfo.InvariantCulture));
			sw.WriteLine ();
			sw.Write (request);
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		static byte [] RedirectRequestHandler (Socket socket)
		{
			MemoryStream ms = new MemoryStream ();
			byte [] buffer = new byte [4096];
			int bytesReceived = socket.Receive (buffer);
			while (bytesReceived > 0) {
				ms.Write (buffer, 0, bytesReceived);
				 // We don't check for Content-Length or anything else here, so we give the client a little time to write
				 // after sending the headers
				Thread.Sleep (200);
				if (socket.Available > 0) {
					bytesReceived = socket.Receive (buffer);
				} else {
					bytesReceived = 0;
				}
			}
			ms.Flush ();
			ms.Position = 0;
			string statusLine = null;
			using (StreamReader sr = new StreamReader (ms, Encoding.UTF8)) {
				statusLine = sr.ReadLine ();
			}

			StringWriter sw = new StringWriter ();
			if (statusLine.StartsWith ("POST /original/")) {
				sw.WriteLine ("HTTP/1.0 302 Found");
				EndPoint ep = socket.LocalEndPoint;
				sw.WriteLine ("Location: " + "http://" + ep.ToString () + "/moved/");
				sw.WriteLine ();
				sw.Flush ();
			} else if (statusLine.StartsWith ("GET /moved/")) {
				sw.WriteLine ("HTTP/1.0 200 OK");
				sw.WriteLine ("Content-Type: text/plain");
				sw.WriteLine ("Content-Length: 8");
				sw.WriteLine ();
				sw.Write ("LOOKS OK");
				sw.Flush ();
			} else {
				sw.WriteLine ("HTTP/1.0 500 Too Lazy");
				sw.WriteLine ();
				sw.Flush ();
			}

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		static byte [] InternalErrorHandler (Socket socket)
		{
			byte [] buffer = new byte [4096];
			int bytesReceived = socket.Receive (buffer);
			while (bytesReceived > 0) {
				 // We don't check for Content-Length or anything else here, so we give the client a little time to write
				 // after sending the headers
				Thread.Sleep (200);
				if (socket.Available > 0) {
					bytesReceived = socket.Receive (buffer);
				} else {
					bytesReceived = 0;
				}
			}
			StringWriter sw = new StringWriter ();
			sw.WriteLine ("HTTP/1.1 500 Too Lazy");
			sw.WriteLine ("Content-Length: 0");
			sw.WriteLine ();
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		static byte [] NoContentLengthHandler (Socket socket)
		{
			StringWriter sw = new StringWriter ();
			sw.WriteLine ("HTTP/1.1 500 Too Lazy");
			sw.WriteLine ();
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		static byte [] NotModifiedSinceHandler (Socket socket)
		{
			StringWriter sw = new StringWriter ();
			sw.WriteLine ("HTTP/1.1 304 Not Modified");
			sw.WriteLine ("Date: Fri, 06 Feb 2009 12:50:26 GMT");
			sw.WriteLine ("Server: Apache/2.2.6 (Debian) PHP/5.2.6-2+b1 with Suhosin-Patch mod_ssl/2.2.6 OpenSSL/0.9.8g");
			sw.WriteLine ("Not-Modified-Since: Sun, 08 Feb 2009 08:49:26 GMT");
			sw.WriteLine ("ETag: 898bbr2347056cc2e096afc66e104653");
			sw.WriteLine ("Connection: close");
			sw.WriteLine ();
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		static byte [] VerbEchoHandler (Socket socket)
		{
			MemoryStream ms = new MemoryStream ();
			byte [] buffer = new byte [4096];
			int bytesReceived = socket.Receive (buffer);
			while (bytesReceived > 0) {
				ms.Write (buffer, 0, bytesReceived);
				 // We don't check for Content-Length or anything else here, so we give the client a little time to write
				 // after sending the headers
				Thread.Sleep (200);
				if (socket.Available > 0) {
					bytesReceived = socket.Receive (buffer);
				} else {
					bytesReceived = 0;
				}
			}
			ms.Flush ();
			ms.Position = 0;
			string statusLine = null;
			using (StreamReader sr = new StreamReader (ms, Encoding.UTF8)) {
				statusLine = sr.ReadLine ();
			}

			string verb = "DEFAULT";
			if (statusLine != null) {
				string [] parts = statusLine.Split (' ');
				if (parts.Length > 0)
					verb = parts [0];
			}

			StringWriter sw = new StringWriter ();
			sw.WriteLine ("HTTP/1.1 200 OK");
			sw.WriteLine ("Content-Type: text/plain");
			sw.WriteLine ("Content-Length: " + verb.Length);
			sw.WriteLine ();
			sw.Write (verb);
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		static byte [] PostAnd401Handler (Socket socket)
		{
			MemoryStream ms = new MemoryStream ();
			byte [] buffer = new byte [4096];
			int bytesReceived = socket.Receive (buffer);
			while (bytesReceived > 0) {
				ms.Write (buffer, 0, bytesReceived);
				 // We don't check for Content-Length or anything else here, so we give the client a little time to write
				 // after sending the headers
				Thread.Sleep (200);
				if (socket.Available > 0) {
					bytesReceived = socket.Receive (buffer);
				} else {
					bytesReceived = 0;
				}
			}
			ms.Flush ();
			ms.Position = 0;
			string statusLine = null;
			bool have_auth = false;
			int cl = -1;
			using (StreamReader sr = new StreamReader (ms, Encoding.UTF8)) {
				string l;
				while ((l = sr.ReadLine ()) != null) {
					if (statusLine == null) {
						statusLine = l;
					} else if (l.StartsWith ("Authorization:")) {
						have_auth = true;
					} else if (l.StartsWith ("Content-Length:")) {
						cl = Int32.Parse (l.Substring ("content-length: ".Length));
					}
				}
			}

			StringWriter sw = new StringWriter ();
			if (!have_auth) {
				sw.WriteLine ("HTTP/1.0 401 Invalid Credentials");
				sw.WriteLine ("WWW-Authenticate: basic Yeah");
				sw.WriteLine ();
				sw.Flush ();
			} else if (cl > 0 && statusLine.StartsWith ("POST ")) {
				sw.WriteLine ("HTTP/1.0 200 OK");
				sw.WriteLine ("Content-Type: text/plain");
				sw.WriteLine ("Content-Length: 8");
				sw.WriteLine ();
				sw.Write ("LOOKS OK");
				sw.Flush ();
			} else {
				sw.WriteLine ("HTTP/1.0 500 test failed");
				sw.WriteLine ("Content-Length: 0");
				sw.WriteLine ();
				sw.Flush ();
			}

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}
		[Test]
		public void NtlmAuthentication ()
		{
			NtlmServer server = new NtlmServer ();
			server.Start ();

			string url = String.Format ("http://{0}:{1}/nothing.html", server.IPAddress, server.Port);
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
			request.Timeout = 5000;
			request.Credentials = new NetworkCredential ("user", "password", "domain");
			HttpWebResponse resp = (HttpWebResponse) request.GetResponse ();
			string res = null;
			using (StreamReader reader = new StreamReader (resp.GetResponseStream ())) {
				res = reader.ReadToEnd ();
			}
			resp.Close ();
			server.Stop ();
			Assert.AreEqual ("OK", res);
		}

		class NtlmServer : HttpServer {
			public string Where = "";
			protected override void Run ()
			{
				Where = "before accept";
				Socket client = sock.Accept ();
				NetworkStream ns = new NetworkStream (client, false);
				StreamReader reader = new StreamReader (ns, Encoding.ASCII);
				string line;
				Where = "first read";
				while ((line = reader.ReadLine ()) != null) {
					if (line.Trim () == String.Empty) {
						break;
					}
				}
				Where = "first write";
				StreamWriter writer = new StreamWriter (ns, Encoding.ASCII);
				writer.Write (  "HTTP/1.1 401 Unauthorized\r\n" +
						"WWW-Authenticate: NTLM\r\n" +
						"Content-Length: 5\r\n\r\nWRONG");

				writer.Flush ();
				Where = "second read";
				while ((line = reader.ReadLine ()) != null) {
					if (line.Trim () == String.Empty) {
						break;
					}
				}
				Where = "second write";
				writer.Write (  "HTTP/1.1 401 Unauthorized\r\n" +
						"WWW-Authenticate: NTLM TlRMTVNTUAACAAAAAAAAADgAAAABggAC8GDhqIONH3sAAAAAAAAAAAAAAAA4AAAABQLODgAAAA8=\r\n" +
						"Content-Length: 5\r\n\r\nWRONG");
				writer.Flush ();

				Where = "third read";
				while ((line = reader.ReadLine ()) != null) {
					if (line.Trim () == String.Empty) {
						break;
					}
				}
				Where = "third write";
				writer.Write (  "HTTP/1.1 200 OK\r\n" +
						"Keep-Alive: true\r\n" +
						"Content-Length: 2\r\n\r\nOK");
				writer.Flush ();
				Thread.Sleep (1000);
				writer.Close ();
				reader.Close ();
				client.Close ();
			}
		}

		class BadChunkedServer : HttpServer {
			protected override void Run ()
			{
				Socket client = sock.Accept ();
				NetworkStream ns = new NetworkStream (client, true);
				StreamWriter writer = new StreamWriter (ns, Encoding.ASCII);
				writer.Write (  "HTTP/1.1 200 OK\r\n" +
						"Transfer-Encoding: chunked\r\n" +
						"Connection: close\r\n" +
						"Content-Type: text/plain; charset=UTF-8\r\n\r\n");

				// This body lacks a 'last-chunk' (see RFC 2616)
				writer.Write ("10\r\n1234567890123456\r\n");
				writer.Flush ();
				client.Shutdown (SocketShutdown.Send);
				Thread.Sleep (1000);
				writer.Close ();
			}
		}

		class AcceptAllPolicy : ICertificatePolicy {
			public bool CheckValidationResult (ServicePoint sp, X509Certificate certificate, WebRequest request, int error)
			{
				return true;
			}
		}

		abstract class HttpServer
		{
			protected Socket sock;
			protected Exception error;
			protected ManualResetEvent evt;

			public HttpServer ()
			{
				sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				sock.Bind (new IPEndPoint (IPAddress.Loopback, 0));
				sock.Listen (1);
			}

			public void Start ()
			{
				evt = new ManualResetEvent (false);
				Thread th = new Thread (new ThreadStart (Run));
				th.Start ();
			}

			public void Stop ()
			{
				evt.Set ();
				sock.Close ();
			}
			
			public IPAddress IPAddress {
				get { return ((IPEndPoint) sock.LocalEndPoint).Address; }
			}
			
			public int Port {
				get { return ((IPEndPoint) sock.LocalEndPoint).Port; }
			}

			public Exception Error { 
				get { return error; }
			}

			protected abstract void Run ();
		}

		[Test]
		public void BeginGetRequestStream ()
		{
			this.DoRequest (
			(r, c) =>
			{
				r.Method = "POST";
				r.ContentLength = 0;
				r.BeginGetRequestStream ((a) =>
				{
				using (Stream s = r.EndGetRequestStream (a)) { };
				c.Set();
				},
				null);
			},
			(c) => { });
		}

		[Test]
		public void BeginGetRequestStreamNoClose ()
		{
			this.DoRequest (
			(r, c) => {
				r.Method = "POST";
				r.ContentLength = 1;
				r.BeginGetRequestStream ((a) =>
				{
					r.EndGetRequestStream (a);
					c.Set ();
				},
				null);
			},
			(c) => {});
		}

		[Test]
		public void BeginGetRequestStreamCancelIfNotAllBytesWritten ()
		{
			this.DoRequest (
			(r, c) =>
			{
				r.Method = "POST";
				r.ContentLength = 10;
				r.BeginGetRequestStream ((a) =>
				{
					WebException ex = ExceptionAssert.Throws<WebException> (() =>
					{
						using (Stream s = r.EndGetRequestStream (a)) {
						}
					}
				);
				Assert.AreEqual (ex.Status, WebExceptionStatus.RequestCanceled);
				c.Set();
				},
				null);
			},
			(c) => { });
		}

		[Test]
		public void GetRequestStream2 ()
		{
			this.DoRequest (
			(r, c) =>
			{
				r.Method = "POST";
				r.ContentLength = data64KB.Length;
				using (Stream s = r.GetRequestStream ()) {
					s.Write (data64KB, 0, data64KB.Length);
				}
				c.Set ();
			},
			(c) => { });
		}

		[Test]
		public void GetRequestStreamNotAllBytesWritten ()
		{
			this.DoRequest (
			(r, c) =>
			{
				r.Method = "POST";
				r.ContentLength = data64KB.Length;
				WebException ex = ExceptionAssert.Throws<WebException> (() => r.GetRequestStream ().Close ());
				Assert.AreEqual (ex.Status, WebExceptionStatus.RequestCanceled);
				c.Set ();
			},
			(c) => {});
		}

		[Test]
		public void GetRequestStreamTimeout ()
		{
			this.DoRequest (
			(r, c) =>
			{
				r.Method = "POST";
				r.ContentLength = data64KB.Length;
				r.Timeout = 100;
				WebException ex = ExceptionAssert.Throws<WebException> (() => r.GetRequestStream ());
				Assert.IsTrue (ex.Status == WebExceptionStatus.Timeout || ex.Status == WebExceptionStatus.ConnectFailure);
				c.Set();
			});
		}

		[Test]
		public void BeginWrite ()
		{
			byte[] received = new byte[data64KB.Length];

			this.DoRequest (
			(r, c) =>
			{
				r.Method = "POST";
				r.ContentLength = data64KB.Length;

				Stream s = r.GetRequestStream ();
				s.BeginWrite (data64KB, 0, data64KB.Length,
				(a) =>
				{
					s.EndWrite (a);
					s.Close ();
					r.GetResponse ().Close ();
					c.Set();
				},
				null);
			},
			(c) =>
			{
				c.Request.InputStream.ReadAll (received, 0, received.Length);
				c.Response.StatusCode = 204;
				c.Response.Close ();
			});

			Assert.AreEqual (data64KB, received);
		}

		[Test]
		public void BeginWriteAfterAbort ()
		{
			byte [] received = new byte [data64KB.Length];

			this.DoRequest (
			(r, c) =>
			{
				r.Method = "POST";
				r.ContentLength = data64KB.Length;

				Stream s = r.GetRequestStream ();
				r.Abort();

				WebException ex = ExceptionAssert.Throws<WebException> (() => s.BeginWrite (data64KB, 0, data64KB.Length, null, null));
				Assert.AreEqual (ex.Status, WebExceptionStatus.RequestCanceled);

				c.Set();
			},
			(c) =>
			{
				//c.Request.InputStream.ReadAll (received, 0, received.Length);
				//c.Response.StatusCode = 204;
				//c.Response.Close();
			});
		}

		[Test]
		public void PrematureStreamCloseAborts ()
		{
			byte [] received = new byte [data64KB.Length];

			this.DoRequest (
			(r, c) =>
			{
				r.Method = "POST";
				r.ContentLength = data64KB.Length * 2;

				Stream s = r.GetRequestStream ();
				s.Write (data64KB, 0, data64KB.Length);

				WebException ex = ExceptionAssert.Throws<WebException>(() => s.Close());
				Assert.AreEqual(ex.Status, WebExceptionStatus.RequestCanceled);

				c.Set();
			},
			(c) =>
			{
				c.Request.InputStream.ReadAll (received, 0, received.Length);
//				c.Response.StatusCode = 204;
//				c.Response.Close ();
			});
		}

		[Test]
		public void Write ()
		{
			byte [] received = new byte [data64KB.Length];

			this.DoRequest (
			(r, c) =>
			{
				r.Method = "POST";
				r.ContentLength = data64KB.Length;

				using (Stream s = r.GetRequestStream ()) {
					s.Write (data64KB, 0, data64KB.Length);
				}

				r.GetResponse ().Close ();
				c.Set ();
			},
			(c) =>
			{
				c.Request.InputStream.ReadAll (received, 0, received.Length);
				c.Response.StatusCode = 204;
				c.Response.Close ();
			});

			Assert.AreEqual(data64KB, received);
		}

		/*
		Invalid test: it does not work on linux.
		[pid 30973] send(9, "POST / HTTP/1.1\r\nContent-Length:"..., 89, 0) = 89
		Abort set
		[pid 30970] send(16, "HTTP/1.1 200 OK\r\nServer: Mono-HT"..., 133, 0) = 133
		Calling abort
		[pid 30970] close(16)                   = 0
		Closing!!!
		[pid 30980] send(9, "\213t\326\350\312u\36n\234\351\225L\r\243a\200\226\371\350F\271~oZ\32\270\24\226z4\211\345"..., 65536, 0) = 65536
		Writing...
		[pid 30966] close(4)                    = 0
		OK
		 *
		 The server sideis closed (FD 16) and the send on the client side (FD 9) succeeds.
		[Test]
		[Category("NotWorking")]
		public void WriteServerAborts ()
		{
			ManualResetEvent abort = new ManualResetEvent (false);
			byte [] received = new byte [data64KB.Length];

			this.DoRequest (
			(r, c) =>
			{
				r.Method = "POST";
				r.ContentLength = data64KB.Length;

				using (Stream s = r.GetRequestStream()) {
					abort.Set();
					Thread.Sleep(100);
					IOException ex = ExceptionAssert.Throws<IOException> (() => s.Write(data64KB, 0, data64KB.Length));
				}

				c.Set();
			},
			(c) =>
			{
				abort.WaitOne();
				c.Response.Abort();
			});
		}
		**/

		[Test]
		public void Read ()
		{
			byte [] received = new byte [data64KB.Length];

			this.DoRequest (
			(r, c) =>
			{
				using (HttpWebResponse x = (HttpWebResponse) r.GetResponse ())
				using (Stream s = x.GetResponseStream()) {
					s.ReadAll (received, 0, received.Length);
				}

				c.Set ();
			},
			(c) =>
			{
				c.Response.StatusCode = 200;
				c.Response.ContentLength64 = data64KB.Length;
				c.Response.OutputStream.Write (data64KB, 0, data64KB.Length);
				c.Response.OutputStream.Close ();
				c.Response.Close ();
			});

			Assert.AreEqual (data64KB, received);
		}

		[Test]
		public void ReadTimeout2 ()
		{
			byte [] received = new byte [data64KB.Length];

			this.DoRequest (
			(r, c) =>
			{
				r.ReadWriteTimeout = 10;
				using (HttpWebResponse x = (HttpWebResponse) r.GetResponse ())
				using (Stream s = x.GetResponseStream ()) {
					WebException ex = ExceptionAssert.Throws<WebException> (() => s.ReadAll (received, 0, received.Length));
					Assert.AreEqual (ex.Status, WebExceptionStatus.Timeout);
				}

				c.Set();
			},
			(c) =>
			{
				c.Response.StatusCode = 200;
				c.Response.ContentLength64 = data64KB.Length;
				c.Response.OutputStream.Write (data64KB, 0, data64KB.Length / 2);
				Thread.Sleep (1000);
//				c.Response.OutputStream.Write (data64KB, data64KB.Length / 2, data64KB.Length / 2);
				c.Response.OutputStream.Close ();
				c.Response.Close ();
			});
		}

		[Test]
		public void ReadServerAborted ()
		{
			byte [] received = new byte [data64KB.Length];

			this.DoRequest (
			(r, c) =>
			{
				using (HttpWebResponse x = (HttpWebResponse) r.GetResponse ())
				using (Stream s = x.GetResponseStream ()) {
					Assert.AreEqual (1, s.ReadAll (received, 0, received.Length));
				}

				c.Set();
			},
			(c) =>
			{
				c.Response.StatusCode = 200;
				c.Response.ContentLength64 = data64KB.Length;
				c.Response.OutputStream.Write (data64KB, 0, 1);
				c.Response.Abort ();
			});
		}

		[Test]
		public void BeginGetResponse2 ()
		{
			byte [] received = new byte [data64KB.Length];

			this.DoRequest (
			(r, c) =>
			{
				r.BeginGetResponse ((a) =>
				{
					using (HttpWebResponse x = (HttpWebResponse) r.EndGetResponse (a))
					using (Stream s = x.GetResponseStream ()) {
						s.ReadAll (received, 0, received.Length);
					}

					c.Set();
				}, null);
			},
			(c) =>
			{
				c.Response.StatusCode = 200;
				c.Response.ContentLength64 = data64KB.Length;
				c.Response.OutputStream.Write (data64KB, 0, data64KB.Length);
				c.Response.OutputStream.Close ();
				c.Response.Close ();
			});

			Assert.AreEqual (data64KB, received);
		}

		[Test]
		public void BeginGetResponseAborts ()
		{
			ManualResetEvent aborted = new ManualResetEvent(false);

			this.DoRequest (
			(r, c) =>
			{
				r.BeginGetResponse((a) =>
				{
					WebException ex = ExceptionAssert.Throws<WebException> (() => r.EndGetResponse (a));
					Assert.AreEqual (ex.Status, WebExceptionStatus.RequestCanceled);
					c.Set ();
				}, null);

				aborted.WaitOne ();
				r.Abort ();
			},
			(c) =>
			{
				aborted.Set ();
//				Thread.Sleep (100);
//				c.Response.StatusCode = 200;
//				c.Response.ContentLength64 = 0;
//				c.Response.Close ();
			});

			return;
		}

		void DoRequest (Action<HttpWebRequest, EventWaitHandle> request)
		{
			int port = rand.Next (20000, 65535);

			ManualResetEvent completed = new ManualResetEvent (false);
			Uri address = new Uri (string.Format ("http://localhost:{0}", port));
			HttpWebRequest client = (HttpWebRequest) WebRequest.Create (address);

			request (client, completed);

			if (!completed.WaitOne (10000))
				Assert.Fail ("Test hung");
		}

		void DoRequest (Action<HttpWebRequest, EventWaitHandle> request, Action<HttpListenerContext> processor)
		{
			int port = rand.Next (20000, 65535);

			ManualResetEvent [] completed = new ManualResetEvent [2];
			completed [0] = new ManualResetEvent (false);
			completed [1] = new ManualResetEvent (false);

			using (ListenerScope scope = new ListenerScope (processor, port, completed [0])) {
				ManualResetEvent clientCompleted = new ManualResetEvent (false);
				Uri address = new Uri (string.Format ("http://localhost:{0}", port));
				HttpWebRequest client = (HttpWebRequest) WebRequest.Create (address);

				ThreadPool.QueueUserWorkItem ((o) => request (client, completed [1]));

				if (!WaitHandle.WaitAll (completed, 10000))
					Assert.Fail ("Test hung.");
			}
		}

#if NET_4_0
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullHost ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com");
			req.Host = null;
		}

		[Test]
		public void NoHost ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com");
			Assert.AreEqual (req.Host, "go-mono.com");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void EmptyHost ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com");
			req.Host = "";
		}

		[Test]
		public void HostAndPort ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com:80");
			Assert.AreEqual ("go-mono.com", req.Host, "#01");
			req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com:9000");
			Assert.AreEqual ("go-mono.com:9000", req.Host, "#02");
		}

		[Test]
		public void PortRange ()
		{
			for (int i = 0; i < 65536; i++) {
				if (i == 80)
					continue;
				string s = i.ToString ();
				HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com:" + s);
				Assert.AreEqual ("go-mono.com:" + s, req.Host, "#" + s);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PortBelow ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com");
			req.Host = "go-mono.com:-1";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PortAbove ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com");
			req.Host = "go-mono.com:65536";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HostTooLong ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com");
			string s = new string ('a', 100);
			req.Host = s + "." + s + "." + s + "." + s + "." + s + "." + s; // Over 255 bytes
		}

		[Test]
		[Category ("NotWorking")] // #5490
		public void InvalidNamesThatWork ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com");
			req.Host = "-";
			req.Host = "-.-";
			req.Host = "á";
			req.Host = new string ('a', 64); // Should fail. Max. is 63.
		}

		[Test]
		public void NoDate ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com");
			Assert.AreEqual (DateTime.MinValue, req.Date);
		}

		[Test]
		public void UtcDate ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://go-mono.com");
			req.Date = DateTime.UtcNow;
			DateTime date = req.Date;
			Assert.AreEqual (DateTimeKind.Local, date.Kind);
		}

		[Test]
		public void AddAndRemoveDate ()
		{
			// Neil Armstrong set his foot on Moon
			var landing = new DateTime (1969, 7, 21, 2, 56, 0, DateTimeKind.Utc);
			Assert.AreEqual (621214377600000000, landing.Ticks);
			var unspecified = new DateTime (1969, 7, 21, 2, 56, 0);
			var local = landing.ToLocalTime ();

			var req = (HttpWebRequest)WebRequest.Create ("http://www.mono-project.com/");
			req.Date = landing;
			Assert.AreEqual (DateTimeKind.Local, req.Date.Kind);
			Assert.AreEqual (local.Ticks, req.Date.Ticks);
			Assert.AreEqual (local, req.Date);

			req.Date = unspecified;
			Assert.AreEqual (DateTimeKind.Local, req.Date.Kind);
			Assert.AreEqual (unspecified.Ticks, req.Date.Ticks);
			Assert.AreEqual (unspecified, req.Date);

			req.Date = local;
			Assert.AreEqual (DateTimeKind.Local, req.Date.Kind);
			Assert.AreEqual (local.Ticks, req.Date.Ticks);
			Assert.AreEqual (local, req.Date);

			req.Date = DateTime.MinValue;
			Assert.AreEqual (DateTimeKind.Unspecified, DateTime.MinValue.Kind);
			Assert.AreEqual (DateTimeKind.Unspecified, req.Date.Kind);
			Assert.AreEqual (0, req.Date.Ticks);

			Assert.AreEqual (null, req.Headers.Get ("Date"));
		}
#endif
		class ListenerScope : IDisposable {
			EventWaitHandle completed;
			public HttpListener listener;
			Action<HttpListenerContext> processor;

			public ListenerScope (Action<HttpListenerContext> processor, int port, EventWaitHandle completed)
			{
				this.processor = processor;
				this.completed = completed;

				this.listener = new HttpListener ();
				this.listener.Prefixes.Add (string.Format ("http://localhost:{0}/", port));
				this.listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
				this.listener.Start ();

				this.listener.BeginGetContext (this.RequestHandler, null);
			}

			void RequestHandler (IAsyncResult result)
			{
				HttpListenerContext context = null;

				try {
					context = this.listener.EndGetContext (result);
				} catch (HttpListenerException ex) {
					// check if the thread has been aborted as in the case when we are shutting down.
					if (ex.ErrorCode == 995)
						return;
				} catch (ObjectDisposedException) {
					return;
				}

				ThreadPool.QueueUserWorkItem ((o) =>
				{
					try {
						this.processor (context);
					} catch (HttpListenerException) {
					}
				});

				this.completed.Set ();
			}

			public void Dispose ()
			{
				this.listener.Stop ();
			}
		}

#if !TARGET_JVM && !MOBILE
		class SslHttpServer : HttpServer {
			X509Certificate _certificate;

			protected override void Run ()
			{
				try {
					Socket client = sock.Accept ();
					NetworkStream ns = new NetworkStream (client, true);
					SslServerStream s = new SslServerStream (ns, Certificate, false, false);
					s.PrivateKeyCertSelectionDelegate += new PrivateKeySelectionCallback (GetPrivateKey);

					StreamReader reader = new StreamReader (s);
					StreamWriter writer = new StreamWriter (s, Encoding.ASCII);

					string line;
					string hello = "<html><body><h1>Hello World!</h1></body></html>";
					string answer = "HTTP/1.0 200\r\n" +
							"Connection: close\r\n" +
							"Content-Type: text/html\r\n" +
							"Content-Encoding: " + Encoding.ASCII.WebName + "\r\n" +
							"Content-Length: " + hello.Length + "\r\n" +
							"\r\n" + hello;

					// Read the headers
					do {
						line = reader.ReadLine ();
					} while (line != "" && line != null && line.Length > 0);

					// Now the content. We know it's 100 bytes.
					// This makes BeginRead in sslclientstream block.
					char [] cs = new char [100];
					reader.Read (cs, 0, 100);

					writer.Write (answer);
					writer.Flush ();
					if (evt.WaitOne (5000, false))
						error = new Exception ("Timeout when stopping the server");
				} catch (Exception e) {
					error = e;
				}
			}

			X509Certificate Certificate {
				get {
					if (_certificate == null)
						_certificate = new X509Certificate (CertData.Certificate);

					return _certificate;
				}
			}

			AsymmetricAlgorithm GetPrivateKey (X509Certificate certificate, string targetHost)
			{
				PrivateKey key = new PrivateKey (CertData.PrivateKey, null);
				return key.RSA;
			}
		}

		class CertData {
			public readonly static byte [] Certificate = {
				48, 130, 1, 191, 48, 130, 1, 40, 160, 3, 2, 1, 2, 2, 16, 36, 
				14, 97, 190, 146, 132, 208, 71, 175, 6, 87, 168, 185, 175, 55, 43, 48, 
				13, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 4, 5, 0, 48, 18, 
				49, 16, 48, 14, 6, 3, 85, 4, 3, 19, 7, 103, 111, 110, 122, 97, 
				108, 111, 48, 30, 23, 13, 48, 53, 48, 54, 50, 50, 49, 57, 51, 48, 
				52, 54, 90, 23, 13, 51, 57, 49, 50, 51, 49, 50, 51, 53, 57, 53, 
				57, 90, 48, 18, 49, 16, 48, 14, 6, 3, 85, 4, 3, 19, 7, 103, 
				111, 110, 122, 97, 108, 111, 48, 129, 158, 48, 13, 6, 9, 42, 134, 72, 
				134, 247, 13, 1, 1, 1, 5, 0, 3, 129, 140, 0, 48, 129, 136, 2, 
				129, 129, 0, 138, 9, 38, 25, 166, 252, 59, 26, 39, 184, 128, 216, 38, 
				73, 41, 86, 30, 228, 160, 205, 41, 135, 115, 223, 44, 62, 42, 198, 178, 
				190, 81, 11, 25, 21, 216, 49, 179, 130, 246, 52, 97, 175, 212, 94, 157, 
				231, 162, 66, 161, 103, 63, 204, 83, 141, 172, 119, 97, 225, 206, 98, 101, 
				210, 106, 2, 206, 81, 90, 173, 47, 41, 199, 209, 241, 177, 177, 96, 207, 
				254, 220, 190, 66, 180, 153, 0, 209, 14, 178, 69, 194, 3, 37, 116, 239, 
				49, 23, 185, 245, 255, 126, 35, 85, 246, 56, 244, 107, 117, 24, 14, 57, 
				9, 111, 147, 189, 220, 142, 57, 104, 153, 193, 205, 19, 14, 22, 157, 16, 
				24, 80, 201, 2, 2, 0, 17, 163, 23, 48, 21, 48, 19, 6, 3, 85, 
				29, 37, 4, 12, 48, 10, 6, 8, 43, 6, 1, 5, 5, 7, 3, 1, 
				48, 13, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 4, 5, 0, 3, 
				129, 129, 0, 64, 49, 57, 253, 218, 198, 229, 51, 189, 12, 154, 225, 183, 
				160, 147, 90, 113, 172, 69, 122, 28, 77, 97, 215, 231, 194, 150, 29, 196, 
				65, 95, 218, 99, 142, 111, 79, 205, 109, 76, 32, 92, 220, 76, 88, 53, 
				237, 80, 11, 85, 44, 91, 21, 210, 12, 34, 223, 234, 18, 187, 136, 62, 
				26, 240, 103, 180, 12, 226, 221, 250, 247, 129, 51, 23, 129, 165, 56, 67, 
				43, 83, 244, 110, 207, 24, 253, 195, 16, 46, 80, 113, 80, 18, 2, 254, 
				120, 147, 151, 164, 23, 210, 230, 100, 19, 197, 179, 28, 194, 48, 106, 159, 
				155, 144, 37, 82, 44, 160, 40, 52, 146, 174, 77, 188, 160, 230, 75, 172, 
				123, 3, 254, 
			};

			public readonly static byte [] PrivateKey = {
				30, 241, 181, 176, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 
				0, 0, 0, 0, 84, 2, 0, 0, 7, 2, 0, 0, 0, 36, 0, 0, 
				82, 83, 65, 50, 0, 4, 0, 0, 17, 0, 0, 0, 201, 80, 24, 16, 
				157, 22, 14, 19, 205, 193, 153, 104, 57, 142, 220, 189, 147, 111, 9, 57, 
				14, 24, 117, 107, 244, 56, 246, 85, 35, 126, 255, 245, 185, 23, 49, 239, 
				116, 37, 3, 194, 69, 178, 14, 209, 0, 153, 180, 66, 190, 220, 254, 207, 
				96, 177, 177, 241, 209, 199, 41, 47, 173, 90, 81, 206, 2, 106, 210, 101, 
				98, 206, 225, 97, 119, 172, 141, 83, 204, 63, 103, 161, 66, 162, 231, 157, 
				94, 212, 175, 97, 52, 246, 130, 179, 49, 216, 21, 25, 11, 81, 190, 178, 
				198, 42, 62, 44, 223, 115, 135, 41, 205, 160, 228, 30, 86, 41, 73, 38, 
				216, 128, 184, 39, 26, 59, 252, 166, 25, 38, 9, 138, 175, 88, 190, 223, 
				27, 24, 224, 123, 190, 69, 164, 234, 129, 59, 108, 229, 248, 62, 187, 15, 
				235, 147, 162, 83, 47, 123, 170, 190, 224, 31, 215, 110, 143, 31, 227, 216, 
				85, 88, 154, 83, 207, 229, 41, 28, 237, 116, 181, 17, 37, 141, 224, 185, 
				164, 144, 141, 233, 164, 138, 177, 241, 115, 181, 230, 150, 7, 92, 139, 141, 
				113, 95, 57, 191, 211, 165, 217, 250, 197, 68, 164, 184, 168, 43, 48, 65, 
				177, 237, 173, 144, 148, 221, 62, 189, 147, 63, 216, 188, 206, 103, 226, 171, 
				32, 20, 230, 116, 144, 192, 1, 39, 202, 87, 74, 250, 6, 142, 188, 23, 
				45, 4, 112, 191, 253, 67, 69, 70, 128, 143, 44, 234, 41, 96, 195, 82, 
				202, 35, 158, 149, 240, 151, 23, 25, 166, 179, 85, 144, 58, 120, 149, 229, 
				205, 34, 8, 110, 86, 119, 130, 210, 37, 173, 65, 71, 169, 67, 8, 51, 
				20, 96, 51, 155, 3, 39, 85, 187, 40, 193, 57, 19, 99, 78, 173, 28, 
				129, 154, 108, 175, 8, 138, 237, 71, 27, 148, 129, 35, 47, 57, 101, 237, 
				168, 178, 227, 221, 212, 63, 124, 254, 253, 215, 183, 159, 49, 103, 74, 49, 
				67, 160, 171, 72, 194, 215, 108, 251, 178, 18, 184, 100, 211, 105, 21, 186, 
				39, 66, 218, 154, 72, 222, 90, 237, 179, 251, 51, 224, 212, 56, 251, 6, 
				209, 151, 198, 176, 89, 110, 35, 141, 248, 237, 223, 68, 135, 206, 207, 169, 
				254, 219, 243, 130, 71, 11, 94, 113, 233, 92, 63, 156, 169, 72, 215, 110, 
				95, 94, 191, 50, 59, 89, 187, 59, 183, 99, 161, 146, 233, 245, 219, 80, 
				87, 113, 251, 50, 144, 195, 158, 46, 189, 232, 119, 91, 75, 22, 6, 176, 
				39, 206, 25, 196, 213, 195, 219, 24, 28, 103, 104, 36, 137, 128, 4, 119, 
				163, 40, 126, 87, 18, 86, 128, 243, 213, 101, 2, 237, 78, 64, 160, 55, 
				199, 93, 90, 126, 175, 199, 55, 89, 234, 190, 5, 16, 196, 88, 28, 208, 
				28, 92, 32, 115, 204, 9, 202, 101, 15, 123, 43, 75, 90, 144, 95, 179, 
				102, 249, 57, 150, 204, 99, 147, 203, 16, 63, 81, 244, 226, 237, 82, 204, 
				20, 200, 140, 65, 83, 217, 161, 23, 123, 37, 115, 12, 100, 73, 70, 190, 
				32, 235, 174, 140, 148, 157, 47, 238, 40, 208, 228, 80, 54, 187, 156, 252, 
				253, 230, 231, 156, 138, 125, 96, 79, 3, 27, 143, 55, 146, 169, 165, 61, 
				238, 60, 227, 77, 217, 93, 117, 122, 111, 46, 173, 113, 
			};
		}
#endif

		[Test]
		public void CookieContainerTest ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 11002);
			string url = "http://" + ep.ToString ();

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (CookieRequestHandler))) {
				responder.Start ();

				CookieContainer container = new CookieContainer ();
				container.Add(new Uri (url), new Cookie ("foo", "bar"));
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
				request.CookieContainer = container;
				WebHeaderCollection headers = request.Headers;
				headers.Add("Cookie", "foo=baz");
				HttpWebResponse response = (HttpWebResponse) request.GetResponse ();
				string responseString = null;
				using (StreamReader reader = new StreamReader (response.GetResponseStream ())) {
					responseString = reader.ReadToEnd ();
				}
				response.Close ();
				Assert.AreEqual (1, response.Cookies.Count, "#01");
				Assert.AreEqual ("foo=bar", response.Headers.Get("Set-Cookie"), "#02");
			}

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (CookieRequestHandler))) {
				responder.Start ();

				CookieContainer container = new CookieContainer ();
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
				request.CookieContainer = container;
				WebHeaderCollection headers = request.Headers;
				headers.Add("Cookie", "foo=baz");
				HttpWebResponse response = (HttpWebResponse) request.GetResponse ();
				string responseString = null;
				using (StreamReader reader = new StreamReader (response.GetResponseStream ())) {
					responseString = reader.ReadToEnd ();
				}
				response.Close ();
				Assert.AreEqual (0, response.Cookies.Count, "#03");
				Assert.AreEqual ("", response.Headers.Get("Set-Cookie"), "#04");
			}
		}

		internal static byte[] CookieRequestHandler (Socket socket)
		{
			MemoryStream ms = new MemoryStream ();
			byte[] buffer = new byte[4096];
			int bytesReceived = socket.Receive (buffer);
			while (bytesReceived > 0) {
				ms.Write(buffer, 0, bytesReceived);
				// We don't check for Content-Length or anything else here, so we give the client a little time to write
				// after sending the headers
				Thread.Sleep(200);
				if (socket.Available > 0) {
					bytesReceived = socket.Receive (buffer);
				} else {
					bytesReceived = 0;
				}
			}
			ms.Flush();
			ms.Position = 0;
			string cookies = string.Empty;
			using (StreamReader sr = new StreamReader (ms, Encoding.UTF8)) {
				string line;
				while ((line = sr.ReadLine ()) != null) {
					if (line.StartsWith ("Cookie:")) {
						cookies = line.Substring ("cookie: ".Length);
					}
				}
			}

			StringWriter sw = new StringWriter ();
			sw.WriteLine ("HTTP/1.1 200 OK");
			sw.WriteLine ("Content-Type: text/xml");
			sw.WriteLine ("Set-Cookie: " + cookies);
			sw.WriteLine ("Content-Length: " + cookies.Length.ToString (CultureInfo.InvariantCulture));
			sw.WriteLine ();
			sw.Write (cookies);
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}
	}

	[TestFixture]
	public class HttpRequestStreamTest
	{
		[Test]
		public void BeginRead ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9124);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					byte [] buffer = new byte [10];
					try {
						rs.BeginRead (buffer, 0, buffer.Length, null, null);
						Assert.Fail ("#1");
					} catch (NotSupportedException ex) {
						// The stream does not support reading
						Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
					} finally {
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void BeginWrite_Request_Aborted ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9125);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					req.Abort ();
					try {
						rs.BeginWrite (new byte [] { 0x2a, 0x2f }, 0, 2, null, null);
						Assert.Fail ("#1");
					} catch (WebException ex) {
						// The request was aborted: The request was canceled
						Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
						Assert.IsNull (ex.Response, "#5");
						Assert.AreEqual (WebExceptionStatus.RequestCanceled, ex.Status, "#6");
					}
				}
			}
		}

		[Test]
		public void CanRead ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9126);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				Stream rs = req.GetRequestStream ();
				try {
					Assert.IsFalse (rs.CanRead, "#1");
					rs.Close ();
					Assert.IsFalse (rs.CanRead, "#2");
				} finally {
					rs.Close ();
					req.Abort ();
				}
			}
		}

		[Test]
		public void CanSeek ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9127);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				Stream rs = req.GetRequestStream ();
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

		[Test] // bug #324182
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void CanTimeout ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9128);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				Stream rs = req.GetRequestStream ();
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

		[Test]
		public void CanWrite ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9129);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				Stream rs = req.GetRequestStream ();
				try {
					Assert.IsTrue (rs.CanWrite, "#1");
					rs.Close ();
					Assert.IsFalse (rs.CanWrite, "#2");
				} finally {
					rs.Close ();
					req.Abort ();
				}
			}
		}

		[Test]
		public void Read ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9130);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					byte [] buffer = new byte [10];
					try {
						rs.Read (buffer, 0, buffer.Length);
						Assert.Fail ("#1");
					} catch (NotSupportedException ex) {
						// The stream does not support reading
						Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
					} finally {
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void ReadByte ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9140);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					try {
						rs.ReadByte ();
						Assert.Fail ("#1");
					} catch (NotSupportedException ex) {
						// The stream does not support reading
						Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
					} finally {
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void ReadTimeout ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9141);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				Stream rs = req.GetRequestStream ();
				try {
					Assert.AreEqual (300000, rs.ReadTimeout, "#1");
					rs.Close ();
					Assert.AreEqual (300000, rs.ReadTimeout, "#2");
				} finally {
					rs.Close ();
					req.Abort ();
				}
			}
		}

		[Test]
		public void Seek ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9142);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					try {
						rs.Seek (0, SeekOrigin.Current);
						Assert.Fail ("#1");
					} catch (NotSupportedException ex) {
						// This stream does not support seek operations
						Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
					} finally {
						req.Abort ();
					}
				}
			}
		}

		[Test]
		public void Write_Buffer_Null ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9143);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					try {
						rs.Write ((byte []) null, -1, -1);
						Assert.Fail ("#1");
					} catch (ArgumentNullException ex) {
						Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
						Assert.AreEqual ("buffer", ex.ParamName, "#5");
					}
				}

				req.Abort ();
			}
		}

		[Test]
		public void Write_Count_Negative ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9144);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					byte [] buffer = new byte [] { 0x2a, 0x2c, 0x1d, 0x00, 0x0f };
					try {
						rs.Write (buffer, 1, -1);
						Assert.Fail ("#1");
					} catch (ArgumentOutOfRangeException ex) {
						// Specified argument was out of the range of valid values
						Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
						Assert.AreEqual ("size", ex.ParamName, "#A5");
					}
				}

				req.Abort ();
			}
		}

		[Test]
		public void Write_Count_Overflow ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9145);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					byte [] buffer = new byte [] { 0x2a, 0x2c, 0x1d, 0x00, 0x0f };
					try {
						rs.Write (buffer, buffer.Length - 2, 3);
						Assert.Fail ("#1");
					} catch (ArgumentOutOfRangeException ex) {
						// Specified argument was out of the range of valid values
						Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
						Assert.AreEqual ("size", ex.ParamName, "#5");
					}
				}

				req.Abort ();
			}
		}

		[Test]
		public void Write_Offset_Negative ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9146);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					byte [] buffer = new byte [] { 0x2a, 0x2c, 0x1d, 0x00, 0x0f };
					try {
						rs.Write (buffer, -1, 0);
						Assert.Fail ("#1");
					} catch (ArgumentOutOfRangeException ex) {
						// Specified argument was out of the range of valid values
						Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
						Assert.AreEqual ("offset", ex.ParamName, "#5");
					}
				}

				req.Abort ();
			}
		}

		[Test]
		public void Write_Offset_Overflow ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9147);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					byte [] buffer = new byte [] { 0x2a, 0x2c, 0x1d, 0x00, 0x0f };
					try {
						rs.Write (buffer, buffer.Length + 1, 0);
						Assert.Fail ("#1");
					} catch (ArgumentOutOfRangeException ex) {
						// Specified argument was out of the range of valid values
						Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
						Assert.AreEqual ("offset", ex.ParamName, "#5");
					}
				}

				req.Abort ();
			}
		}

		[Test]
		public void Write_Request_Aborted ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9148);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					req.Abort ();
					try {
						rs.Write (new byte [0], 0, 0);
						Assert.Fail ("#1");
					} catch (WebException ex) {
						// The request was aborted: The request was canceled
						Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
						Assert.IsNull (ex.Response, "#5");
						Assert.AreEqual (WebExceptionStatus.RequestCanceled, ex.Status, "#6");
					}
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_Stream_Closed ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9149);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					rs.Close ();
					try {
						rs.Write (new byte [0], 0, 0);
						Assert.Fail ("#1");
					} catch (WebException ex) {
						// The request was aborted: The connection was closed unexpectedly
						Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
						Assert.IsNull (ex.Response, "#5");
						Assert.AreEqual (WebExceptionStatus.ConnectionClosed, ex.Status, "#6");
					}
				}
			}
		}

		[Test]
		public void WriteByte_Request_Aborted ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9150);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				using (Stream rs = req.GetRequestStream ()) {
					req.Abort ();
					try {
						rs.WriteByte (0x2a);
						Assert.Fail ("#1");
					} catch (WebException ex) {
						// The request was aborted: The request was canceled
						Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
						Assert.IsNull (ex.Response, "#5");
						Assert.AreEqual (WebExceptionStatus.RequestCanceled, ex.Status, "#6");
					}
				}
			}
		}

		[Test]
		public void WriteTimeout ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 9151);
			string url = "http://" + ep.ToString () + "/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (HttpWebRequestTest.EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";

				Stream rs = req.GetRequestStream ();
				try {
					Assert.AreEqual (300000, rs.WriteTimeout, "#1");
					rs.Close ();
					Assert.AreEqual (300000, rs.WriteTimeout, "#2");
				} finally {
					rs.Close ();
					req.Abort ();
				}
			}
		}

#if NET_4_0
		[Test]
		// Bug6737
		// This test is supposed to fail prior to .NET 4.0
		public void Post_EmptyRequestStream ()
		{
			var wr = HttpWebRequest.Create ("http://google.com");
			wr.Method = "POST";
			wr.GetRequestStream ();
			
			var gr = wr.BeginGetResponse (delegate { }, null);
			Assert.AreEqual (true, gr.AsyncWaitHandle.WaitOne (5000), "#1");
		}
#endif
	}

	static class StreamExtensions {
		public static int ReadAll(this Stream stream, byte[] buffer, int offset, int count)
		{
			int totalRead = 0;

			while (totalRead < count) {
				int bytesRead = stream.Read (buffer, offset + totalRead, count - totalRead);
				if (bytesRead == 0)
					break;

				totalRead += bytesRead;
			}

			return totalRead;
		}
	}

	static class ExceptionAssert {
		/// <summary>
		/// Asserts that the function throws an exception.
		/// </summary>
		/// <param name="f">A function execute that is expected to raise an exception.</param>
		/// <typeparam name="T">The type of exception that is expected.</typeparam>
		/// <returns>The exception thrown.</returns>
		/// <exception cref="AssertFailedException">If the function does not throw an exception 
		/// or throws a different exception.</exception>
		/// <example><![CDATA[
		///     ExceptionAssert.Throws(typeof(ArgumentNullException), delegate {
		///         myObject.myFunction(null); });
		/// ]]></example>
		public static T Throws<T> (Action f) where T : Exception {
			Exception actualException = null;

			try {
				f ();
			} catch (Exception ex) {
				actualException = ex;
			}

			if (actualException == null)
				throw new AssertionException (string.Format (
					"No exception thrown. Expected '{0}'",
					typeof (T).FullName));
			else if (typeof(T) != actualException.GetType())
				throw new AssertionException (string.Format (
					"Caught exception of type '{0}'. Expected '{1}':{2}",
					actualException.GetType().FullName,
					typeof (T).FullName,
					Environment.NewLine + actualException));

			return (T) actualException;
		}
	}
}
