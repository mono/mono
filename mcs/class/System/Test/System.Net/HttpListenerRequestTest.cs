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

using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
#if TARGET_JVM
	[Ignore ("The class HttpListener is not supported")]
#endif
	public class HttpListenerRequestTest
	{
		[Test]
		[Category ("NotWorking")] // Bug #5742 
		public void HasEntityBody ()
		{
			HttpListenerContext ctx;
			HttpListenerRequest request;
			NetworkStream ns;

			HttpListener listener = HttpListener2Test.CreateAndStartListener (
				"http://127.0.0.1:9000/HasEntityBody/");

			// POST with non-zero Content-Lenth
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "POST /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#A");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// POST with zero Content-Lenth
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "POST /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 0\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsFalse (request.HasEntityBody, "#B");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// POST with chunked encoding
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "POST /HasEntityBody HTTP/1.1\r\nHost: 127.0.0.1\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#C");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// GET with no Content-Length
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "GET /HasEntityBody HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsFalse (request.HasEntityBody, "#D");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// GET with non-zero Content-Length
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "GET /HasEntityBody HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#E");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// GET with zero Content-Length
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "GET /HasEntityBody HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 0\r\n\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsFalse (request.HasEntityBody, "#F");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// GET with chunked encoding
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "GET /HasEntityBody HTTP/1.1\r\nHost: 127.0.0.1\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#G");
			HttpListener2Test.Send (ctx.Response.OutputStream, "%%%OK%%%");

			// PUT with non-zero Content-Lenth
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "PUT /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#H");

			// PUT with zero Content-Lenth
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "PUT /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 0\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsFalse (request.HasEntityBody, "#I");

			// INVALID with non-zero Content-Lenth
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "INVALID /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#J");

			// INVALID with zero Content-Lenth
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "INVALID /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 0\r\n\r\n123");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsFalse (request.HasEntityBody, "#K");

			// INVALID with chunked encoding
			ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "INVALID /HasEntityBody/ HTTP/1.1\r\nHost: 127.0.0.1\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n");
			ctx = listener.GetContext ();
			request = ctx.Request;
			Assert.IsTrue (request.HasEntityBody, "#L");

			listener.Close ();
		}

		[Test]
		public void HttpMethod ()
		{
			HttpListener listener = HttpListener2Test.CreateAndStartListener (
				"http://127.0.0.1:9000/HttpMethod/");
			NetworkStream ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "pOsT /HttpMethod/ HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: 3\r\n\r\n123");
			HttpListenerContext ctx = listener.GetContext ();
			HttpListenerRequest request = ctx.Request;
			Assert.AreEqual ("pOsT", request.HttpMethod);
			listener.Close ();
		}

		[Test]
		public void HttpBasicAuthScheme ()
		{
			HttpListener listener = HttpListener2Test.CreateAndStartListener ("http://*:9000/authTest/", AuthenticationSchemes.Basic);
			//dummy-wait for context
			listener.BeginGetContext (null, listener);
			NetworkStream ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "GET /authTest/ HTTP/1.0\r\n\r\n");
			String response = HttpListener2Test.Receive (ns, 512);
			Assert.IsTrue (response.Contains ("WWW-Authenticate: Basic realm"), "#A");
			ns.Close ();
			listener.Close ();
		}

		[Test]
		public void HttpRequestUriIsNotDecoded ()
		{
			HttpListener listener = HttpListener2Test.CreateAndStartListener (
				"http://127.0.0.1:9000/RequestUriDecodeTest/");
			NetworkStream ns = HttpListener2Test.CreateNS (9000);
			HttpListener2Test.Send (ns, "GET /RequestUriDecodeTest/?a=b&c=d%26e HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n");
			HttpListenerContext ctx = listener.GetContext ();
			HttpListenerRequest request = ctx.Request;
			Assert.AreEqual ("/RequestUriDecodeTest/?a=b&c=d%26e", request.Url.PathAndQuery);
			listener.Close ();
		}
	}
}
