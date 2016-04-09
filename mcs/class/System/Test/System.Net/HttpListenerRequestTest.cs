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
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

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
			var port = NetworkHelpers.FindFreePort ();
			HttpListener listener = HttpListener2Test.CreateAndStartListener (
				"http://127.0.0.1:" + port + "/HasEntityBody/");

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
		public void HttpMethod ()
		{
			var port = NetworkHelpers.FindFreePort ();
			HttpListener listener = HttpListener2Test.CreateAndStartListener (
				"http://127.0.0.1:" + port + "/HttpMethod/");
			NetworkStream ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "pOsT /HttpMethod/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			HttpListenerContext ctx = listener.GetContext ();
			HttpListenerRequest request = ctx.Request;
			Assert.AreEqual ("pOsT", request.HttpMethod);
			listener.Close ();
		}

		[Test]
		public void HttpBasicAuthScheme ()
		{
			var port = NetworkHelpers.FindFreePort ();			
			HttpListener listener = HttpListener2Test.CreateAndStartListener ("http://*:" + port + "/authTest/", AuthenticationSchemes.Basic);
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
		public void HttpRequestUriIsNotDecoded ()
		{
			var port = NetworkHelpers.FindFreePort ();
			HttpListener listener = HttpListener2Test.CreateAndStartListener (
				"http://127.0.0.1:" + port + "/RequestUriDecodeTest/");
			NetworkStream ns = HttpListener2Test.CreateNS (port);
			HttpListener2Test.Send (ns, "GET /RequestUriDecodeTest/?a=b&c=d%26e HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n");
			HttpListenerContext ctx = listener.GetContext ();
			HttpListenerRequest request = ctx.Request;
			Assert.AreEqual ("/RequestUriDecodeTest/?a=b&c=d%26e", request.Url.PathAndQuery);
			listener.Close ();
		}

		[Test]
		public void HttpRequestIsLocal ()
		{
			var ips = new List<IPAddress> (Dns.GetHostAddresses (Dns.GetHostName ()));
			ips.Add (IPAddress.Loopback);
			foreach (var ip in ips) {
				if (ip.AddressFamily != AddressFamily.InterNetwork)
					continue;

				HttpListener listener = HttpListener2Test.CreateAndStartListener (
					"http://" + ip + ":9000/HttpRequestIsLocal/");
				NetworkStream ns = HttpListener2Test.CreateNS (ip, 9000);
				HttpListener2Test.Send (ns, "GET /HttpRequestIsLocal/ HTTP/1.0\r\n\r\n");
				HttpListenerContext ctx = listener.GetContext ();
				HttpListenerRequest request = ctx.Request;
				Assert.AreEqual (true, request.IsLocal, "IP " + ip + " is not local");
				listener.Close ();
			}
		}

		[Test] // #29927
		public void HttpRequestUriUnescape ()
		{
			var prefix = "http://localhost:" + NetworkHelpers.FindFreePort () + "/";
			var key = "Product/1";

			var expectedUrl = prefix + key + "/";
			var rawUrl = prefix + Uri.EscapeDataString (key) + "/";

			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add (prefix);
			listener.Start ();

			var contextTask = listener.GetContextAsync ();

			var request = (HttpWebRequest) WebRequest.Create (rawUrl);
			request.GetResponseAsync ();

			if(!contextTask.Wait (1000))
				Assert.Fail ("Timeout");

			Assert.AreEqual (expectedUrl, contextTask.Result.Request.Url.AbsoluteUri);

			listener.Close ();
		}
	}
}
