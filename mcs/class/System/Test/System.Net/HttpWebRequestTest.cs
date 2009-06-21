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
#if !TARGET_JVM
using Mono.Security.Authenticode;
using Mono.Security.Protocol.Tls;
#endif

namespace MonoTests.System.Net
{
	[TestFixture]
	public class HttpWebRequestTest
	{
		[Test]
#if TARGET_JVM
		[Ignore ("Ignore failures in Sys.Net")]
#endif
		public void Proxy_Null ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://www.google.com");
			Assert.IsNotNull (req.Proxy, "#1");
#if NET_2_0
			req.Proxy = null;
			Assert.IsNull (req.Proxy, "#2");
#else
			try {
				req.Proxy = null;
				Assert.Fail ("#2");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
			}
#endif
		}

		[Test]
		[Category("InetAccess")]
#if TARGET_JVM
		[Ignore ("NMA - wrong cookies number returned")]
#endif
		public void Sync ()
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://www.google.com");
			Assertion.AssertNotNull ("req:If Modified Since: ", req.IfModifiedSince);

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
			Assertion.AssertEquals ("#1", "bytes=10-,50-90,100-,100-120", req.Headers ["Range"]);
			try {
				req.AddRange ("bits", 2000);
				Assertion.Fail ("#2");
			} catch (InvalidOperationException) {}
		}

		[Test] // bug #471782
		public void CloseRequestStreamAfterReadingResponse ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

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
			Assertion.AssertEquals ("#01", 0, cookies.Count);
			using (HttpWebResponse res = (HttpWebResponse) req.GetResponse()) {
				CookieCollection coll = req.CookieContainer.GetCookies (new Uri (url));
				Assertion.AssertEquals ("#02", 1, coll.Count);
				Assertion.AssertEquals ("#03", 1, res.Cookies.Count);
				Cookie one = coll [0];
				Cookie two = res.Cookies [0];
				Assertion.AssertEquals ("#04", true, object.ReferenceEquals (one, two));
			}
		}

#if !TARGET_JVM //NotWorking
		[Test]
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
				Assertion.AssertEquals ("StatusCode", 200, (int) resp.StatusCode);
				StreamReader sr = new StreamReader (resp.GetResponseStream (), Encoding.UTF8);
				string x = sr.ReadToEnd ();
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
				Assertion.AssertEquals ("#01", 16, nread);
				x = Encoding.ASCII.GetString (bytes, 0, 16);
			} finally {
				resp.Close ();
				server.Stop ();
			}

			if (server.Error != null)
				throw server.Error;

			Assertion.AssertEquals ("1234567890123456", x);
		}

		[Test]
		[Ignore ("This test asserts that our code violates RFC 2616")]
		public void MethodCase ()
		{
			ListDictionary methods = new ListDictionary ();
#if NET_2_0
			methods.Add ("post", "POST");
			methods.Add ("puT", "PUT");
#else
			methods.Add ("post", "post");
			methods.Add ("puT", "puT");
#endif
			methods.Add ("POST", "POST");
			methods.Add ("whatever", "whatever");
			methods.Add ("PUT", "PUT");

			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			foreach (DictionaryEntry de in methods) {
				SocketResponder responder = new SocketResponder (new IPEndPoint (IPAddress.Loopback, 8000), 
					new SocketRequestHandler (EchoRequestHandler));
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
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

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
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8002);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8002/test/";

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
		public void BeginGetResponse ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8003);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8003/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req;

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = false;
				req.AllowWriteStreamBuffering = false;
				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.SendChunked = true;
				req.KeepAlive = false;
				req.AllowWriteStreamBuffering = false;
				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.ContentLength = 5;
				req.SendChunked = false;
				req.KeepAlive = false;
				req.AllowWriteStreamBuffering = false;
				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = true;
				req.AllowWriteStreamBuffering = false;
#if NET_2_0
				req.BeginGetResponse (null, null);
				req.Abort ();
#else
				try {
					req.BeginGetResponse (null, null);
				} catch (ProtocolViolationException ex) {
					// Either ContentLength must be set to a non-negative
					// number, or SendChunked set to true in order to perform
					// the write operation when AllowWriteStreamBuffering
					// is disabled
					Assert.IsNull (ex.InnerException, "#A2");
					Assert.IsNotNull (ex.Message, "#A3");
				} finally {
					req.Abort ();
				}
#endif

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = false;
				req.AllowWriteStreamBuffering = false;
				req.ContentLength = 5;
				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "POST";
				req.SendChunked = false;
				req.KeepAlive = true;
				req.AllowWriteStreamBuffering = false;
				req.ContentLength = 5;
				req.BeginGetResponse (null, null);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.SendChunked = true;
#if NET_2_0
				req.BeginGetResponse (null, null);
				req.Abort ();
#else
				try {
					req.BeginGetResponse (null, null);
					Assert.Fail ("#B1");
				} catch (ProtocolViolationException ex) {
					// Content-Length cannot be set for a
					// non-write operation
					Assert.IsNull (ex.InnerException, "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
				} finally {
					req.Abort ();
				}
#endif

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.ContentLength = 5;
#if NET_2_0
				req.BeginGetResponse (null, null);
				req.Abort ();
#else
				try {
					req.BeginGetResponse (null, null);
					Assert.Fail ("#C1");
				} catch (ProtocolViolationException ex) {
					// Content-Length cannot be set for a
					// non-write operation
					Assert.IsNull (ex.InnerException, "#C2");
					Assert.IsNotNull (ex.Message, "#C3");
				} finally {
					req.Abort ();
				}
#endif

				req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.ContentLength = 0;
#if NET_2_0
				req.BeginGetResponse (null, null);
				req.Abort ();
#else
				try {
					req.BeginGetResponse (null, null);
					Assert.Fail ("#D1");
				} catch (ProtocolViolationException ex) {
					// Content-Length cannot be set for a
					// non-write operation
					Assert.IsNull (ex.InnerException, "#D2");
					Assert.IsNotNull (ex.Message, "#D3");
				} finally {
					req.Abort ();
				}
#endif
			}
		}

		[Test] // bug #429200
		public void GetRequestStream ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

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

		[Test] // bug #510661 and #514996
		[Category ("NotWorking")]
		public void GetRequestStream_Close_NotAllBytesWritten ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

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
		public void GetRequestStream_Write_Overflow ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8001);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8001/test/";

			// buffered, non-chunked
			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (EchoRequestHandler))) {
				responder.Start ();

				HttpWebRequest req;
				Stream rs;
				byte [] buffer;

				req = (HttpWebRequest) WebRequest.Create (url);
				req.ProtocolVersion = HttpVersion.Version11;
				req.Method = "POST";
				req.Timeout = 1000;
				req.ReadWriteTimeout = 100;
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
				req.ProtocolVersion = HttpVersion.Version11;
				req.Method = "POST";
				req.Timeout = 1000;
				req.ReadWriteTimeout = 100;
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
				req.ProtocolVersion = HttpVersion.Version11;
				req.Method = "POST";
				req.SendChunked = true;
				req.Timeout = 1000;
				req.ReadWriteTimeout = 100;
				req.ContentLength = 2;

				rs = req.GetRequestStream ();
				rs.WriteByte (0x2c);

				buffer = new byte [] { 0x2a, 0x1d };
				rs.Write (buffer, 0, buffer.Length);
				req.Abort ();
				*/

				req = (HttpWebRequest) WebRequest.Create (url);
				req.ProtocolVersion = HttpVersion.Version11;
				req.Method = "POST";
				req.SendChunked = true;
				req.Timeout = 1000;
				req.ReadWriteTimeout = 100;
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
				req.ProtocolVersion = HttpVersion.Version11;
				req.Method = "POST";
				req.Timeout = 1000;
				req.ReadWriteTimeout = 100;
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
				req.ProtocolVersion = HttpVersion.Version11;
				req.Method = "POST";
				req.Timeout = 1000;
				req.ReadWriteTimeout = 100;
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
				req.ProtocolVersion = HttpVersion.Version11;
				req.Method = "POST";
				req.SendChunked = true;
				req.Timeout = 1000;
				req.ReadWriteTimeout = 100;
				req.ContentLength = 2;

				rs = req.GetRequestStream ();
				rs.WriteByte (0x2c);

				buffer = new byte [] { 0x2a, 0x1d };
				rs.Write (buffer, 0, buffer.Length);
				req.Abort ();

				req = (HttpWebRequest) WebRequest.Create (url);
				req.AllowWriteStreamBuffering = false;
				req.ProtocolVersion = HttpVersion.Version11;
				req.Method = "POST";
				req.SendChunked = true;
				req.Timeout = 1000;
				req.ReadWriteTimeout = 100;
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
				req.ReadWriteTimeout = 100;
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
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8764);
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

		[Test] // bug #324347
		[Category ("NotWorking")]
		public void InternalServerError ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8764);
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
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8764);
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
#if NET_2_0
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
#else
					// The remote server returned an error:
					// (500) Internal Server Error
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.AreEqual (WebExceptionStatus.ProtocolError, ex.Status, "#A4");

					HttpWebResponse webResponse = ex.Response as HttpWebResponse;
					Assert.IsNotNull (webResponse, "#A5");
					Assert.AreEqual ("POST", webResponse.Method, "#A6");
					webResponse.Close ();
#endif
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
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/moved/";

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
		public void NotModiedSince ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 8000);
			string url = "http://" + IPAddress.Loopback.ToString () + ":8000/test/";

			using (SocketResponder responder = new SocketResponder (ep, new SocketRequestHandler (NotModifiedSinceHandler))) {
				responder.Start ();

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				req.Method = "GET";
				req.KeepAlive = false;
				req.Timeout = 20000;
				req.ReadWriteTimeout = 20000;
#if NET_2_0
				req.Headers.Add (HttpRequestHeader.IfNoneMatch, "898bbr2347056cc2e096afc66e104653");
#else
				req.Headers.Add ("If-None-Match", "898bbr2347056cc2e096afc66e104653");
#endif
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

#if NET_2_0
		[Test] // bug #324182
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Stream_CanTimeout ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8764);
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
				Assert.IsTrue (rs.CanTimeout, "#1");
				rs.Close ();
				using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
					Stream os = resp.GetResponseStream ();
					Assert.IsTrue (os.CanTimeout, "#2");
					os.Close ();
				}
				responder.Stop ();
			}
		}
#endif

		[Test] // bug #353495
		[Category ("NotWorking")]
		public void LastModifiedKind ()
		{
			const string reqURL = "http://coffeefaq.com/site/node/25";
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create (reqURL);
			HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
			DateTime lastMod = resp.LastModified;
			string rawLastMod = resp.Headers ["Last-Modified"];
			resp.Close ();
			//Assert.AreEqual ("Tue, 15 Jan 2008 08:59:59 GMT", rawLastMod, "#1");
#if NET_2_0
			Assert.AreEqual (DateTimeKind.Local, lastMod.Kind, "#2");
#endif
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

		static byte [] EchoRequestHandler (Socket socket)
		{
			MemoryStream ms = new MemoryStream ();
			byte [] buffer = new byte [4096];
			int bytesReceived = socket.Receive (buffer);
			while (bytesReceived > 0) {
				ms.Write (buffer, 0, bytesReceived);
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
				sw.WriteLine ("Location: " + "http://" + IPAddress.Loopback.ToString () + ":8764/moved/");
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

#if !TARGET_JVM
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
					evt.WaitOne (50000, false);
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
	}
}
