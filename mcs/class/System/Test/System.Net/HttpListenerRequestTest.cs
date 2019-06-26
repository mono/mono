//
// HttpListenerRequestTest.cs - Unit tests for System.Net.HttpListenerRequest
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//	David Straw
//
// Copyright (C) 2007 Gert Driesen
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
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class HttpListenerRequestTest
	{
		[Test]
		[Category ("NotWorking")] // Bug #5742 
		public void HasEntityBody ()
		{
			HttpListenerContext ctx;
			HttpListenerRequest request;
			NetworkStream ns;
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener (
				"http://127.0.0.1:", out var port, "/HasEntityBody/");

			// POST with non-zero Content-Lenth
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "POST /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#A");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// POST with zero Content-Lenth
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "POST /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 0\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsFalse (request.HasEntityBody, "#B");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// POST with chunked encoding
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "POST /HasEntityBody HTTP/1.1\r\nHost: 127.0.0.1\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#C");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// GET with no Content-Length
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "GET /HasEntityBody HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsFalse (request.HasEntityBody, "#D");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// GET with non-zero Content-Length
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "GET /HasEntityBody HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#E");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// GET with zero Content-Length
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "GET /HasEntityBody HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 0\r\n\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsFalse (request.HasEntityBody, "#F");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// GET with chunked encoding
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "GET /HasEntityBody HTTP/1.1\r\nHost: 127.0.0.1\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#G");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// PUT with non-zero Content-Lenth
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "PUT /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#H");

			// PUT with zero Content-Lenth
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "PUT /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 0\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsFalse (request.HasEntityBody, "#I");

			// INVALID with non-zero Content-Lenth
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "INVALID /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#J");

			// INVALID with zero Content-Lenth
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "INVALID /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 0\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsFalse (request.HasEntityBody, "#K");

			// INVALID with chunked encoding
			ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "INVALID /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#L");

			listener.Close ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void HttpMethod ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener (
				"http://127.0.0.1:", out var port, "/HttpMethod/");
			NetworkStream ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "pOsT /HttpMethod/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			HttpListenerContext ctx = listener.GetContext ();
			HttpListenerRequest request = ctx.Request;
			Assert.AreEqual ("pOsT", request.HttpMethod);
			listener.Close ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
#if MONOTOUCH
		[Ignore ("Randomly produces ObjectDisposedExceptions, in particular on device. See bug #39780.")]
#endif
		public void HttpBasicAuthScheme ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://*:", out var port, "/authTest/", AuthenticationSchemes.Basic);
			//dummy-wait for context
			listener.BeginGetContext (null, listener);
			NetworkStream ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "GET /authTest/ HTTP/1.0\r\n\r\n");
			String response = HttpListener2Test.Receive (ns, 512);
			Assert.IsTrue (response.Contains ("WWW-Authenticate: Basic realm"), "#A");
			ns.Close ();
			listener.Close ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void HttpRequestUriIsNotDecoded ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener (
				"http://127.0.0.1:", out var port, "/RequestUriDecodeTest/");
			NetworkStream ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "GET /RequestUriDecodeTest/?a=b&c=d%26e HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n");
			HttpListenerContext ctx = listener.GetContext ();
			HttpListenerRequest request = ctx.Request;
			Assert.AreEqual ("/RequestUriDecodeTest/?a=b&c=d%26e", request.Url.PathAndQuery);
			listener.Close ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void HttpRequestIsLocal ()
		{
			var ips = new List<IPAddress> ();
			ips.Add (IPAddress.Loopback);
			foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces ()) {
				if (adapter.OperationalStatus != OperationalStatus.Up)
					continue;
				foreach (var ip in adapter.GetIPProperties ().UnicastAddresses) {
					ips.Add (ip.Address);
				}
			}

			foreach (var ip in ips) {
				if (ip.AddressFamily != AddressFamily.InterNetwork)
					continue;

				HttpListener listener = NetworkHelpers.CreateAndStartHttpListener (
					"http://" + ip + ":", out var port, "/HttpRequestIsLocal/");
				NetworkStream ns = HttpListener2Test.CreateNS (ip, port);
				HttpListener2Test.Send (ns, "GET /HttpRequestIsLocal/ HTTP/1.0\r\n\r\n");
				HttpListenerContext ctx = listener.GetContext ();
				HttpListenerRequest request = ctx.Request;
				Assert.AreEqual (true, request.IsLocal, "IP " + ip + " is not local");
				listener.Close ();
			}
		}

		[Test] // #29927
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void HttpRequestUriUnescape ()
		{
			var key = "Product/1";

			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://localhost:", out var port, "/", out var prefix);

			var expectedUrl = prefix + key + "/";
			var rawUrl = prefix + Uri.EscapeDataString (key) + "/";


			var contextTask = listener.GetContextAsync ();

			var request = (HttpWebRequest) WebRequest.Create (rawUrl);
			request.GetResponseAsync ();

			Assert.IsTrue (contextTask.Wait (1000));

			Assert.AreEqual (expectedUrl, contextTask.Result.Request.Url.AbsoluteUri);

			listener.Close ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void EmptyWrite ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://localhost:", out var port, "/", out var prefix);

			Task.Run (() => {
				var context = listener.GetContext ();

				var s = context.Response.OutputStream;
				s.Write (new byte[10], 0, 0);
				return;
			});

			var request = (HttpWebRequest)WebRequest.Create (prefix);
			var rsp = request.GetResponseAsync ();
			Assert.IsFalse (rsp.Wait (1000), "Don't send on empty write");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void HttpRequestIgnoreBadCookies ()
		{
			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener (
				"http://127.0.0.1:", out var port, "/HttpRequestIgnoreBadCookiesTest/");
			NetworkStream ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "GET /HttpRequestIgnoreBadCookiesTest/?a=b HTTP/1.1\r\nHost: 127.0.0.1\r\nCookie: ELOQUA=GUID=5ca2346347357f4-f877-4eff-96aa-70fe0b677650; ELQSTATUS=OK; WRUID=609099666.123259461695; CommunityServer-UserCookie2101=lv=Thu, 26 Jul 2012 15:25:11 GMT&mra=Mon, 01 Oct 2012 17:40:05 GMT; PHPSESSID=1234dg3opfjb4qafp0oo645; __utma=9761706.1153317537.1357240270.1357240270.1357317902.2; __utmb=9761706.6.10.1357317902; __utmc=9761706; __utmz=9761706.1357240270.1.1.utmcsr=test.testdomain.com|utmccn=(referral)|utmcmd=referral|utmcct=/test/1234\r\n\r\n");
			HttpListenerContext ctx = listener.GetContext ();
			HttpListenerRequest request = ctx.Request;
			Assert.AreEqual ("/HttpRequestIgnoreBadCookiesTest/?a=b", request.Url.PathAndQuery);
			listener.Close ();
		}
	}
}
