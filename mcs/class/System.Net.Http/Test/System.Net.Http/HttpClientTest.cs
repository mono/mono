//
// HttpClientTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Linq;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class HttpClientTest
	{
		class HttpMessageHandlerMock : HttpMessageHandler
		{
			public Func<HttpRequestMessage, HttpResponseMessage> OnSend;
			public Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> OnSendFull;
			public Func<HttpRequestMessage, Task<HttpResponseMessage>> OnSendAsync;

			public HttpMessageHandlerMock ()
			{
			}

			protected override HttpResponseMessage Send (HttpRequestMessage request, CancellationToken cancellationToken)
			{
				if (OnSend != null)
					return OnSend (request);

				if (OnSendFull != null)
				    return OnSendFull (request, cancellationToken);

				Assert.Fail ("Send");
				return null;
			}

			protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
			{
				if (OnSendAsync != null)
					return OnSendAsync (request);
				
				Assert.Fail ("SendAsync");
				return null;
			}
		}

		static readonly string TestHost = "localhost:810";
		static readonly string LocalServer = string.Format ("http://{0}/", TestHost);

		[Test]
		public void Ctor_Default ()
		{
			var client = new HttpClient ();
			Assert.IsNull (client.BaseAddress, "#1");
			Assert.IsNotNull (client.DefaultRequestHeaders, "#2");	// TODO: full check
			Assert.AreEqual (0x10000, client.MaxResponseContentBufferSize, "#3");
			Assert.AreEqual (TimeSpan.FromSeconds (100), client.Timeout, "#4");
		}

		[Test]
		public void CancelPendingRequests ()
		{
			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			var request = new HttpRequestMessage (HttpMethod.Get, "http://xamarin.com");
			var mre = new ManualResetEvent (false);

			mh.OnSendFull = (l, c) => {
				mre.Set ();
				Assert.IsTrue (c.WaitHandle.WaitOne (1000), "#20");
				Assert.IsTrue (c.IsCancellationRequested, "#21");
				mre.Set ();
				return new HttpResponseMessage ();
			};

			var t = Task.Factory.StartNew (() => {
				client.Send (request);
			});

			Assert.IsTrue (mre.WaitOne (500), "#1");
			mre.Reset ();
			client.CancelPendingRequests ();
			Assert.IsTrue (t.Wait (500), "#2");

			request = new HttpRequestMessage (HttpMethod.Get, "http://xamarin.com");
			mh.OnSendFull = (l, c) => {
				Assert.IsFalse (c.IsCancellationRequested, "#30");
				return new HttpResponseMessage ();
			};

			client.Send (request);
		}

		[Test]
		public void CancelPendingRequests_BeforeSend ()
		{
			var ct = new CancellationTokenSource ();
			ct.Cancel ();
			var rr = CancellationTokenSource.CreateLinkedTokenSource (new CancellationToken (), ct.Token);


			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			var request = new HttpRequestMessage (HttpMethod.Get, "http://xamarin.com");
			client.CancelPendingRequests ();

			mh.OnSendFull = (l, c) => {
				Assert.IsFalse (c.IsCancellationRequested, "#30");
				return new HttpResponseMessage ();
			};

			client.Send (request);

			request = new HttpRequestMessage (HttpMethod.Get, "http://xamarin.com");
			client.Send (request);
		}

		[Test]
		public void Properties ()
		{
			var client = new HttpClient ();
			client.BaseAddress = null;
			client.MaxResponseContentBufferSize = int.MaxValue;
			client.Timeout = Timeout.InfiniteTimeSpan;

			Assert.IsNull (client.BaseAddress, "#1");
			Assert.AreEqual (int.MaxValue, client.MaxResponseContentBufferSize, "#2");
			Assert.AreEqual (Timeout.InfiniteTimeSpan, client.Timeout, "#3");
		}

		[Test]
		public void Properties_Invalid ()
		{
			var client = new HttpClient ();
			try {
				client.MaxResponseContentBufferSize = 0;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				client.Timeout = TimeSpan.MinValue;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Send ()
		{
			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			client.BaseAddress = new Uri ("http://xamarin.com");
			var request = new HttpRequestMessage ();
			var response = new HttpResponseMessage ();

			mh.OnSend = l => {
				Assert.AreEqual (l, request, "#2");
				Assert.AreEqual (client.BaseAddress, l.RequestUri, "#2");
				return response;
			};

			Assert.AreEqual (response, client.Send (request), "#1");
		}

		[Test]
		public void Send_DefaultRequestHeaders ()
		{
			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			client.DefaultRequestHeaders.Referrer = new Uri ("http://google.com");

			var request = new HttpRequestMessage (HttpMethod.Get, "http://xamarin.com");
			var response = new HttpResponseMessage ();

			mh.OnSend = l => {
				Assert.AreEqual (client.DefaultRequestHeaders.Referrer, l.Headers.Referrer, "#2");
				Assert.IsNotNull (l.Headers.Referrer, "#3");
				return response;
			};

			Assert.AreEqual (response, client.Send (request), "#1");
		}

		[Test]
		public void Send_Complete_Default ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;

				Assert.IsNull (request.AcceptTypes, "#1");
				Assert.AreEqual (0, request.ContentLength64, "#2");
				Assert.IsNull (request.ContentType, "#3");
				Assert.AreEqual (0, request.Cookies.Count, "#4");
				Assert.IsFalse (request.HasEntityBody, "#5");
				Assert.AreEqual (2, request.Headers.Count, "#6");
				Assert.AreEqual ("Keep-Alive", request.Headers["Connection"], "#6a");
				Assert.AreEqual (TestHost, request.Headers["Host"], "#6b");
				Assert.AreEqual ("GET", request.HttpMethod, "#7");
				Assert.IsFalse (request.IsAuthenticated, "#8");
				Assert.IsTrue (request.IsLocal, "#9");
				Assert.IsFalse (request.IsSecureConnection, "#10");
				Assert.IsFalse (request.IsWebSocketRequest, "#11");
				Assert.IsTrue (request.KeepAlive, "#12");
				Assert.AreEqual (HttpVersion.Version11, request.ProtocolVersion, "#13");
				Assert.IsNull (request.ServiceName, "#14");
				Assert.IsNull (request.UrlReferrer, "#15");
				Assert.IsNull (request.UserAgent, "#16");
				Assert.IsNull (request.UserLanguages, "#17");
			});

			try {
				var client = new HttpClient ();
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);
				var response = client.Send (request, HttpCompletionOption.ResponseHeadersRead);

				Assert.AreEqual ("", response.Content.ReadAsString (), "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Complete_Version_1_0 ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;

				Assert.IsNull (request.AcceptTypes, "#1");
				Assert.AreEqual (0, request.ContentLength64, "#2");
				Assert.IsNull (request.ContentType, "#3");
				Assert.AreEqual (0, request.Cookies.Count, "#4");
				Assert.IsFalse (request.HasEntityBody, "#5");
				Assert.AreEqual (1, request.Headers.Count, "#6");
				Assert.AreEqual (TestHost, request.Headers["Host"], "#6a");
				Assert.AreEqual ("GET", request.HttpMethod, "#7");
				Assert.IsFalse (request.IsAuthenticated, "#8");
				Assert.IsTrue (request.IsLocal, "#9");
				Assert.IsFalse (request.IsSecureConnection, "#10");
				Assert.IsFalse (request.IsWebSocketRequest, "#11");
				Assert.IsFalse (request.KeepAlive, "#12");
				Assert.AreEqual (HttpVersion.Version10, request.ProtocolVersion, "#13");
				Assert.IsNull (request.ServiceName, "#14");
				Assert.IsNull (request.UrlReferrer, "#15");
				Assert.IsNull (request.UserAgent, "#16");
				Assert.IsNull (request.UserLanguages, "#17");
			});

			try {
				var client = new HttpClient ();
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);
				request.Version = HttpVersion.Version10;
				var response = client.Send (request, HttpCompletionOption.ResponseHeadersRead);

				Assert.AreEqual ("", response.Content.ReadAsString (), "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Complete_ClientHandlerSettings ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;

				Assert.IsNull (request.AcceptTypes, "#1");
				Assert.AreEqual (0, request.ContentLength64, "#2");
				Assert.IsNull (request.ContentType, "#3");
				Assert.AreEqual (1, request.Cookies.Count, "#4");
				Assert.AreEqual (new Cookie ("mycookie", "vv"), request.Cookies[0], "#4a");
				Assert.IsFalse (request.HasEntityBody, "#5");
				Assert.AreEqual (3, request.Headers.Count, "#6");
				Assert.AreEqual (TestHost, request.Headers["Host"], "#6a");
				Assert.AreEqual ("gzip", request.Headers["Accept-Encoding"], "#6b");
				Assert.AreEqual ("mycookie=vv", request.Headers["Cookie"], "#6c");
				Assert.AreEqual ("GET", request.HttpMethod, "#7");
				Assert.IsFalse (request.IsAuthenticated, "#8");
				Assert.IsTrue (request.IsLocal, "#9");
				Assert.IsFalse (request.IsSecureConnection, "#10");
				Assert.IsFalse (request.IsWebSocketRequest, "#11");
				Assert.IsFalse (request.KeepAlive, "#12");
				Assert.AreEqual (HttpVersion.Version10, request.ProtocolVersion, "#13");
				Assert.IsNull (request.ServiceName, "#14");
				Assert.IsNull (request.UrlReferrer, "#15");
				Assert.IsNull (request.UserAgent, "#16");
				Assert.IsNull (request.UserLanguages, "#17");
			});

			try {
				var chandler = new HttpClientHandler ();
				chandler.AllowAutoRedirect = true;
				chandler.AutomaticDecompression = DecompressionMethods.GZip;
				chandler.MaxAutomaticRedirections = 33;
				chandler.MaxRequestContentBufferSize = 5555;
				chandler.PreAuthenticate = true;
				chandler.CookieContainer.Add (new Uri (LocalServer), new Cookie ( "mycookie", "vv"));
				chandler.UseCookies = true;
				chandler.UseDefaultCredentials = true;
				chandler.Proxy = new WebProxy ("ee");
				chandler.UseProxy = true;

				var client = new HttpClient (chandler);
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);
				request.Version = HttpVersion.Version10;
				var response = client.Send (request, HttpCompletionOption.ResponseHeadersRead);

				Assert.AreEqual ("", response.Content.ReadAsString (), "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Complete_CustomHeaders ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;
				Assert.AreEqual ("vv", request.Headers["aa"], "#1");
				Assert.AreEqual ("bytes=3-20", request.Headers["Range"], "#2");
				Assert.AreEqual (4, request.Headers.Count, "#3");

				var response = l.Response;
				response.Headers.Add ("rsp", "rrr");
				response.Headers.Add ("upgrade", "vvvvaa");
				response.Headers.Add ("date", "aa");
				response.Headers.Add ("cache-control", "audio");

				response.StatusDescription = "test description";
				response.ProtocolVersion = HttpVersion.Version10;
				response.SendChunked = true;
				response.RedirectLocation = "w3.org";
			});

			try {
				var client = new HttpClient ();
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);
				request.Headers.AddWithoutValidation ("aa", "vv");
				request.Headers.Range = new RangeHeaderValue (3, 20);
				var response = client.Send (request, HttpCompletionOption.ResponseHeadersRead);

				Assert.AreEqual ("", response.Content.ReadAsString (), "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");

				IEnumerable<string> values;
				Assert.IsTrue (response.Headers.TryGetValues ("rsp", out values), "#102");
				Assert.AreEqual ("rrr", values.First (), "#102a");

				Assert.IsTrue (response.Headers.TryGetValues ("Transfer-Encoding", out values), "#103");
				Assert.AreEqual ("chunked", values.First (), "#103a");
				Assert.AreEqual (true, response.Headers.TransferEncodingChunked, "#103b");

				Assert.IsTrue (response.Headers.TryGetValues ("Date", out values), "#104");
				Assert.AreEqual (2, values.Count (), "#104b");
				Assert.IsNull (response.Headers.Date, "#104c");

				Assert.AreEqual (new ProductHeaderValue ("vvvvaa"), response.Headers.Upgrade.First (), "#105");

				Assert.AreEqual ("audio", response.Headers.CacheControl.Extensions.First ().Name, "#106");

				Assert.AreEqual ("w3.org", response.Headers.Location.OriginalString, "#107");

				Assert.AreEqual ("test description", response.ReasonPhrase, "#110");
				Assert.AreEqual (HttpVersion.Version11, response.Version, "#111");
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Complete_Error ()
		{
			var listener = CreateListener (l => {
				var response = l.Response;
				response.StatusCode = 500;
			});

			try {
				var client = new HttpClient ();
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);
				var response = client.Send (request, HttpCompletionOption.ResponseHeadersRead);

				Assert.AreEqual ("", response.Content.ReadAsString (), "#100");
				Assert.AreEqual (HttpStatusCode.InternalServerError, response.StatusCode, "#101");
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Timeout ()
		{
			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			client.Timeout = TimeSpan.FromMilliseconds (100);
			var request = new HttpRequestMessage (HttpMethod.Get, "http://xamarin.com");
			var response = new HttpResponseMessage ();

			mh.OnSendFull = (l, c) => {
				Assert.IsTrue (c.WaitHandle.WaitOne (500), "#2");
				return response;
			};

			Assert.AreEqual (response, client.Send (request), "#1");
		}

		[Test]
		public void Send_Invalid ()
		{
			var client = new HttpClient ();
			try {
				client.Send (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				var request = new HttpRequestMessage ();
				client.Send (request);
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void Send_InvalidHandler ()
		{
			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			client.BaseAddress = new Uri ("http://xamarin.com");
			var request = new HttpRequestMessage ();

			mh.OnSend = l => {
				Assert.AreEqual (l, request, "#1");
				return null;
			};

			try {
				client.Send (request);
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void Send_SameMessage ()
		{
			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			var request = new HttpRequestMessage (HttpMethod.Get, "http://xamarin.com");

			mh.OnSend = l => {
				return new HttpResponseMessage ();
			};

			client.Send (request);
			try {
				client.Send (request);
			} catch (InvalidOperationException) {
			}
		}

		static HttpListener CreateListener (Action<HttpListenerContext> contextAssert)
		{
			var l = new HttpListener ();
			l.Prefixes.Add ("http://+:810/");
			l.Start ();
			l.BeginGetContext (ar => {
				var ctx = l.EndGetContext (ar);

				try {
					if (contextAssert != null)
						contextAssert (ctx);
				} finally {
					ctx.Response.Close ();
				}
			}, null);

			return l;
		}
	}
}
