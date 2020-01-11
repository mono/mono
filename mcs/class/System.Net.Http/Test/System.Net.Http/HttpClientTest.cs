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
using System.IO;

using MonoTests.Helpers;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class HttpClientTest
	{
		class HttpMessageHandlerMock : HttpMessageHandler
		{
			public Func<HttpRequestMessage, Task<HttpResponseMessage>> OnSend;
			public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> OnSendFull;

			public HttpMessageHandlerMock ()
			{
			}

			protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
			{
				if (OnSend != null)
					return OnSend (request);

				if (OnSendFull != null)
					return OnSendFull (request, cancellationToken);

				Assert.Fail ("Send");
				return null;
			}
		}

		class HttpClientHandlerMock : HttpClientHandler
		{
			public Func<HttpRequestMessage, Task<HttpResponseMessage>> OnSend;
			public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> OnSendFull;

			protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
			{
				if (OnSend != null)
					return OnSend (request);

				if (OnSendFull != null)
					return OnSendFull (request, cancellationToken);

				Assert.Fail ("Send");
				return null;
			}
		}

		class CustomStream : Stream
		{
			public override void Flush ()
			{
				throw new NotImplementedException ();
			}

			int pos;

			public override int Read (byte[] buffer, int offset, int count)
			{
				++pos;
				if (pos > 4)
					return 0;

				return 11;
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				throw new NotImplementedException ();
			}

			public override void SetLength (long value)
			{
				throw new NotImplementedException ();
			}

			public override void Write (byte[] buffer, int offset, int count)
			{
				throw new NotImplementedException ();
			}

			public override bool CanRead {
				get {
					return true;
				}
			}

			public override bool CanSeek {
				get {
					return false;
				}
			}

			public override bool CanWrite {
				get {
					throw new NotImplementedException ();
				}
			}

			public override long Length {
				get {
					throw new NotImplementedException ();
				}
			}

			public override long Position {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}
		}

		class ThrowOnlyProxy : IWebProxy
		{
			public ICredentials Credentials {
				get {
					throw new NotImplementedException ();
				}

				set {
					throw new NotImplementedException ();
				}
			}

			public Uri GetProxy (Uri destination)
			{
				throw new NotImplementedException ();
			}

			public bool IsBypassed (Uri host)
			{
				throw new NotImplementedException ();
			}
		}

		const int WaitTimeout = 5000;

		[Test]
		public void Ctor ()
		{
			var client = new HttpClient ();
			Assert.IsNull (client.BaseAddress, "#1");
			Assert.IsNotNull (client.DefaultRequestHeaders, "#2");	// TODO: full check
			Assert.AreEqual (int.MaxValue, client.MaxResponseContentBufferSize, "#3");
			Assert.AreEqual (TimeSpan.FromSeconds (100), client.Timeout, "#4");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
#if !MONOTOUCH_WATCH			
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif		
#endif
		public void Ctor_HttpClientHandler ()
		{
			var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
			Assert.IsNull (client.BaseAddress, "#1");
			Assert.IsNotNull (client.DefaultRequestHeaders, "#2");	// TODO: full check
			Assert.AreEqual (int.MaxValue, client.MaxResponseContentBufferSize, "#3");
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
				return Task.FromResult (new HttpResponseMessage ());
			};

			var t = Task.Factory.StartNew (() => {
				client.SendAsync (request).Wait (WaitTimeout);
			});

			Assert.IsTrue (mre.WaitOne (500), "#1");
			mre.Reset ();
			client.CancelPendingRequests ();
			Assert.IsTrue (t.Wait (500), "#2");

			request = new HttpRequestMessage (HttpMethod.Get, "http://xamarin.com");
			mh.OnSendFull = (l, c) => {
				Assert.IsFalse (c.IsCancellationRequested, "#30");
				return Task.FromResult (new HttpResponseMessage ());
			};

			client.SendAsync (request).Wait (WaitTimeout);
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
				return Task.FromResult (new HttpResponseMessage ());
			};

			client.SendAsync (request).Wait (WaitTimeout);

			request = new HttpRequestMessage (HttpMethod.Get, "http://xamarin.com");
			client.SendAsync (request).Wait (WaitTimeout);
		}


		[Test]
		[Ignore]
#if FEATURE_NO_BSD_SOCKETS
		// Using HttpClientHandler, which indirectly requires BSD sockets.
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CancelRequestViaProxy ()
		{
			var handler = HttpClientTestHelpers.CreateHttpClientHandler ();
			handler.Proxy = new WebProxy ("192.168.10.25:8888/"); // proxy that doesn't exist
			handler.UseProxy = true;
			handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			var httpClient = new HttpClient (handler) {
				BaseAddress = new Uri ("https://www.example.com"),
				Timeout = TimeSpan.FromMilliseconds (1)
			};

			try {
				var restRequest = new HttpRequestMessage {
					Method = HttpMethod.Post,
					RequestUri = new Uri("foo", UriKind.Relative),
					Content = new StringContent("", null, "application/json")
				};

				httpClient.PostAsync (restRequest.RequestUri, restRequest.Content).Wait (WaitTimeout);
				Assert.Fail ("#1");
			} catch (AggregateException e) {
				Assert.That (e.InnerException, Is.InstanceOf<TaskCanceledException> (), $"#2: {e}");
			}
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

			try {
				client.Timeout = TimeSpan.Zero;
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				client.Timeout = TimeSpan.FromMilliseconds (int.MaxValue + 1L);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Proxy_Disabled ()
		{
			var pp = WebRequest.DefaultWebProxy;

			try {
				WebRequest.DefaultWebProxy = new ThrowOnlyProxy ();

				var request = HttpClientTestHelpers.CreateHttpClientHandler ();
				request.UseProxy = false;

				var client = new HttpClient (request);
				Assert.IsTrue (client.GetAsync ("http://www.example.com").Wait (5000), "needs internet access");
			} finally {
				WebRequest.DefaultWebProxy = pp;
			}
		}

		[Test]
		public void Send ()
		{
			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			client.BaseAddress = new Uri ("http://www.example.com");
			var request = new HttpRequestMessage ();
			var response = new HttpResponseMessage ();

			mh.OnSend = l => {
				Assert.AreEqual (l, request, "#2");
				Assert.AreEqual (client.BaseAddress, l.RequestUri, "#2");
				return Task.FromResult (response);
			};

			Assert.AreEqual (response, client.SendAsync (request).Result, "#1");
		}

		[Test]		
		public void Send_BaseAddress ()
		{
			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			client.BaseAddress = new Uri ("http://localhost/");
			var response = new HttpResponseMessage ();

			mh.OnSend = l => {
				Assert.AreEqual ("http://localhost/relative", l.RequestUri.ToString (), "#2");
				return Task.FromResult (response);
			};

			Assert.AreEqual (response, client.GetAsync ("relative").Result, "#1");
			Assert.AreEqual (response, client.GetAsync ("/relative").Result, "#2");
		}

		[Test]
		public void Send_DefaultRequestHeaders ()
		{
			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			client.DefaultRequestHeaders.Referrer = new Uri ("http://www.example.com");

			var request = new HttpRequestMessage (HttpMethod.Get, "http://www.example.org");
			var response = new HttpResponseMessage ();

			mh.OnSend = l => {
				Assert.AreEqual (client.DefaultRequestHeaders.Referrer, l.Headers.Referrer, "#2");
				Assert.IsNotNull (l.Headers.Referrer, "#3");
				return Task.FromResult (response);
			};

			Assert.AreEqual (response, client.SendAsync (request).Result, "#1");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_Default ()
		{
			bool? failed = null;
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_Default/");
			AddListenerContext (listener, l => {
				try {
					var request = l.Request;
	
					Assert.IsNull (request.AcceptTypes, "#1");
					Assert.AreEqual (0, request.ContentLength64, "#2");
					Assert.IsNull (request.ContentType, "#3");
					Assert.AreEqual (0, request.Cookies.Count, "#4");
					Assert.IsFalse (request.HasEntityBody, "#5");
					Assert.AreEqual ($"localhost:{port}", request.Headers["Host"], "#6b");
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
					failed = false;
				} catch {
					failed = true;
				}
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				var request = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Complete_Default/");
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
				Assert.AreEqual (false, failed, "#102");
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_Version_1_0 ()
		{
			bool? failed = null;
			
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_Version_1_0/");
			AddListenerContext (listener, l => {
				try {
					var request = l.Request;
	
					Assert.IsNull (request.AcceptTypes, "#1");
					Assert.AreEqual (0, request.ContentLength64, "#2");
					Assert.IsNull (request.ContentType, "#3");
					Assert.AreEqual (0, request.Cookies.Count, "#4");
					Assert.IsFalse (request.HasEntityBody, "#5");
					Assert.AreEqual (1, request.Headers.Count, "#6");
					Assert.AreEqual ($"localhost:{port}", request.Headers["Host"], "#6a");
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
					failed = false;
				} catch {
					failed = true;
				}
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				var request = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Complete_Version_1_0/");
				request.Version = HttpVersion.Version10;
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
				Assert.AreEqual (false, failed, "#102");
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_ClientHandlerSettings ()
		{
			bool? failed = null;
			
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_ClientHandlerSettings/");
			AddListenerContext (listener, l => {
				var request = l.Request;
				
				try {
					Assert.IsNull (request.AcceptTypes, "#1");
					Assert.AreEqual (0, request.ContentLength64, "#2");
					Assert.IsNull (request.ContentType, "#3");
					Assert.AreEqual (1, request.Cookies.Count, "#4");
					Assert.AreEqual (new Cookie ("mycookie", "vv"), request.Cookies[0], "#4a");
					Assert.IsFalse (request.HasEntityBody, "#5");
					Assert.AreEqual (4, request.Headers.Count, "#6");
					Assert.AreEqual ($"localhost:{port}", request.Headers["Host"], "#6a");
					Assert.AreEqual ("gzip", request.Headers["Accept-Encoding"], "#6b");
					Assert.AreEqual ("mycookie=vv", request.Headers["Cookie"], "#6c");
					Assert.AreEqual ("GET", request.HttpMethod, "#7");
					Assert.IsFalse (request.IsAuthenticated, "#8");
					Assert.IsTrue (request.IsLocal, "#9");
					Assert.IsFalse (request.IsSecureConnection, "#10");
					Assert.IsFalse (request.IsWebSocketRequest, "#11");
					Assert.IsTrue (request.KeepAlive, "#12");
					Assert.AreEqual (HttpVersion.Version10, request.ProtocolVersion, "#13");
					Assert.IsNull (request.ServiceName, "#14");
					Assert.IsNull (request.UrlReferrer, "#15");
					Assert.IsNull (request.UserAgent, "#16");
					Assert.IsNull (request.UserLanguages, "#17");
					failed = false;
				} catch {
					failed = true;
				}
			});

			try {
				var chandler = HttpClientTestHelpers.CreateHttpClientHandler ();
				chandler.AllowAutoRedirect = true;
				chandler.AutomaticDecompression = DecompressionMethods.GZip;
				chandler.MaxAutomaticRedirections = 33;
				chandler.MaxRequestContentBufferSize = 5555;
				chandler.PreAuthenticate = true;
				chandler.CookieContainer.Add (new Uri ($"http://localhost:{port}/Send_Complete_ClientHandlerSettings/"), new Cookie ( "mycookie", "vv"));
				chandler.UseCookies = true;
				chandler.UseDefaultCredentials = true;
				chandler.Proxy = new WebProxy ("ee");
				chandler.UseProxy = true;

				var client = new HttpClient (chandler);
				var request = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Complete_ClientHandlerSettings/");
				request.Version = HttpVersion.Version10;
				request.Headers.Add ("Keep-Alive", "false");
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
				Assert.AreEqual (false, failed, "#102");
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_CustomHeaders ()
		{
			bool? failed = null;
			
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_CustomHeaders/");
			AddListenerContext (listener, l => {
				var request = l.Request;
				try {
					Assert.AreEqual ("vv", request.Headers["aa"], "#1");
	
					var response = l.Response;
					response.Headers.Add ("rsp", "rrr");
					response.Headers.Add ("upgrade", "vvvvaa");
					response.Headers.Add ("Date", "aa");
					response.Headers.Add ("cache-control", "audio");
	
					response.StatusDescription = "test description";
					response.ProtocolVersion = HttpVersion.Version10;
					response.SendChunked = true;
					response.RedirectLocation = "w3.org";
					
					failed = false;
				} catch {
					failed = true;
				}
			});

			try {
				var handler = HttpClientTestHelpers.CreateHttpClientHandler ();
				var client = new HttpClient (handler);
				var request = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Complete_CustomHeaders/");
				Assert.IsTrue (request.Headers.TryAddWithoutValidation ("aa", "vv"), "#0");
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
				
				IEnumerable<string> values;
				Assert.IsTrue (response.Headers.TryGetValues ("rsp", out values), "#102");
				Assert.AreEqual ("rrr", values.First (), "#102a");

				Assert.IsTrue (response.Headers.TryGetValues ("Transfer-Encoding", out values), "#103");
				Assert.AreEqual ("chunked", values.First (), "#103a");
				Assert.AreEqual (true, response.Headers.TransferEncodingChunked, "#103b");

				Assert.IsTrue (response.Headers.TryGetValues ("Date", out values), "#104");
				Assert.AreEqual (1, values.Count (), "#104b");
				// .NET overwrites Date, Mono does not
				// Assert.IsNotNull (response.Headers.Date, "#104c");

				Assert.AreEqual (new ProductHeaderValue ("vvvvaa"), response.Headers.Upgrade.First (), "#105");

				Assert.AreEqual ("audio", response.Headers.CacheControl.Extensions.First ().Name, "#106");

				Assert.AreEqual ("w3.org", response.Headers.Location.OriginalString, "#107");

				Assert.AreEqual ("test description", response.ReasonPhrase, "#110");
				if (HttpClientTestHelpers.IsSocketsHandler (handler))
					Assert.AreEqual (HttpVersion.Version10, response.Version, "#111");
				else
					Assert.AreEqual (HttpVersion.Version11, response.Version, "#111");
				
				Assert.AreEqual (false, failed, "#112");
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_CustomHeaders_SpecialSeparators ()
		{
			bool? failed = null;

			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_CustomHeaders_SpecialSeparators/");
			AddListenerContext (listener, l => {
				var request = l.Request;

				try {
					Assert.AreEqual ("MLK Android Phone 1.1.9", request.UserAgent, "#1");
					failed = false;
				} catch {
					failed = true;
				}
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();

				client.DefaultRequestHeaders.Add("User-Agent", "MLK Android Phone 1.1.9");

				var request = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Complete_CustomHeaders_SpecialSeparators/");

				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
				Assert.AreEqual (false, failed, "#102");
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_CustomHeaders_Host ()
		{
			Exception error = null;
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_CustomHeaders_Host/");
			AddListenerContext (listener, l => {
				var request = l.Request;

				try {
					Assert.AreEqual ("customhost", request.Headers["Host"], "#1");
				} catch (Exception ex) {
					error = ex;
				}
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();

				client.DefaultRequestHeaders.Add("Host", "customhost");

				var request = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Complete_CustomHeaders_Host/");

				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
				Assert.IsNull (error, "#102");
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Transfer_Encoding_Chunked_Needs_Content ()
		{
			if (!HttpClientTestHelpers.UsingSocketsHandler)
				Assert.Ignore ("Requires SocketsHttpHandler");

			bool? failed = null;

			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Transfer_Encoding_Chunked_Needs_Content/");
			AddListenerContext (listener, l => {
				failed = true;
			});

			try {
				try {
					var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
					client.DefaultRequestHeaders.TransferEncodingChunked = true;
					client.GetAsync ($"http://localhost:{port}/Send_Transfer_Encoding_Chunked_Needs_Content/").Wait ();
					// fails with
					// 'Transfer-Encoding: chunked' header can not be used when content object is not specified.
				} catch (AggregateException e) {
					Assert.AreEqual (typeof (HttpRequestException), e.InnerException.GetType (), "#2");
				}
				Assert.IsNull (failed, "#102");
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Transfer_Encoding_Chunked ()
		{
			if (HttpClientTestHelpers.UsingSocketsHandler)
				Assert.Ignore ("Requires LegacyHttpClient");

			bool? failed = null;

			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Transfer_Encoding_Chunked/");
			AddListenerContext (listener, l => {
				var request = l.Request;

				try {
					Assert.AreEqual (2, request.Headers.Count, "#1");
					Assert.AreEqual ("keep-alive", request.Headers ["Connection"], "#2");
					failed = false;
				} catch (Exception ex){
					Console.WriteLine (ex);
					Console.WriteLine (String.Join ("#", l.Request.Headers.AllKeys));
					failed = true;
				}
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				client.DefaultRequestHeaders.TransferEncodingChunked = true;

				client.GetAsync ($"http://localhost:{port}/Send_Transfer_Encoding_Chunked/").Wait ();

				Assert.AreEqual (false, failed, "#102");
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_Content ()
		{
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_Content/");
			AddListenerContext (listener, l => {
				var request = l.Request;
				l.Response.OutputStream.WriteByte (55);
				l.Response.OutputStream.WriteByte (75);
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				var request = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Complete_Content/");
				Assert.IsTrue (request.Headers.TryAddWithoutValidation ("aa", "vv"), "#0");
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("7K", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");

				IEnumerable<string> values;
				Assert.IsTrue (response.Headers.TryGetValues ("Transfer-Encoding", out values), "#102");
				Assert.AreEqual ("chunked", values.First (), "#102a");
				Assert.AreEqual (true, response.Headers.TransferEncodingChunked, "#102b");
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_Content_MaxResponseContentBufferSize ()
		{
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_Content_MaxResponseContentBufferSize/");
			AddListenerContext (listener, l => {
				var request = l.Request;
				var b = new byte[4000];
				l.Response.OutputStream.Write (b, 0, b.Length);
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				client.MaxResponseContentBufferSize = 1000;
				var request = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Complete_Content_MaxResponseContentBufferSize/");
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual (4000, response.Content.ReadAsStringAsync ().Result.Length, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_Content_MaxResponseContentBufferSize_Error ()
		{
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_Content_MaxResponseContentBufferSize_Error/");
			AddListenerContext (listener, l => {
				var request = l.Request;
				var b = new byte[4000];
				l.Response.OutputStream.Write (b, 0, b.Length);
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				client.MaxResponseContentBufferSize = 1000;
				var request = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Complete_Content_MaxResponseContentBufferSize_Error/");

				try {
					client.SendAsync (request, HttpCompletionOption.ResponseContentRead).Wait (WaitTimeout);
					Assert.Fail ("#2");
				} catch (AggregateException e) {
					Assert.That (e.InnerException, Is.InstanceOf<HttpRequestException> (), $"#3: {e}");
				}

			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_NoContent_Post ()
		{
			Send_Complete_NoContent (HttpMethod.Post);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_NoContent_Put ()
		{
			Send_Complete_NoContent (HttpMethod.Put);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_NoContent_Delete ()
		{
			Send_Complete_NoContent (HttpMethod.Delete);
		}

		void Send_Complete_NoContent (HttpMethod method)
		{
			bool? failed = null;
			var handler = HttpClientTestHelpers.CreateHttpClientHandler ();
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_NoContent/");
			AddListenerContext (listener, l => {
				try {
					var request = l.Request;

					if (HttpClientTestHelpers.IsSocketsHandler (handler)) {
						Assert.AreEqual (2, request.Headers.Count, "#1");
						Assert.IsNull (request.Headers["Connection"], "#1c");
					} else {
						Assert.AreEqual (3, request.Headers.Count, "#1");
						Assert.AreEqual ("keep-alive", request.Headers["Connection"], "#1c");
					}
					Assert.AreEqual ("0", request.Headers ["Content-Length"], "#1b");
					Assert.AreEqual (method.Method, request.HttpMethod, "#2");
					failed = false;
				} catch (Exception ex){
					Console.WriteLine (ex);
					Console.WriteLine (String.Join ("#", l.Request.Headers.AllKeys));
					
					failed = true;
				}
			});

			try {
				var client = new HttpClient (handler);
				var request = new HttpRequestMessage (method, $"http://localhost:{port}/Send_Complete_NoContent/");
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
				Assert.AreEqual (false, failed, "#102");
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Complete_Error ()
		{
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Complete_Error/");
			AddListenerContext (listener, l => {
				var response = l.Response;
				response.StatusCode = 500;
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				var request = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Complete_Error/");
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.InternalServerError, response.StatusCode, "#101");
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Content_Get ()
		{
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Content_Get/");
			AddListenerContext (listener, l => {
				var request = l.Request;
				l.Response.OutputStream.WriteByte (72);
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				var r = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Content_Get/");
				var response = client.SendAsync (r).Result;

				Assert.AreEqual ("H", response.Content.ReadAsStringAsync ().Result);
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Content_BomEncoding ()
		{
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Content_BomEncoding/");
			AddListenerContext (listener, l => {
				var request = l.Request;

				var str = l.Response.OutputStream;
				str.WriteByte (0xEF);
				str.WriteByte (0xBB);
				str.WriteByte (0xBF);
				str.WriteByte (71);
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				var r = new HttpRequestMessage (HttpMethod.Get, $"http://localhost:{port}/Send_Content_BomEncoding/");
				var response = client.SendAsync (r).Result;

				Assert.AreEqual ("G", response.Content.ReadAsStringAsync ().Result);
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Content_Put ()
		{
			bool passed = false;
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Content_Put/");
			AddListenerContext (listener, l => {
				var request = l.Request;
				passed = 7 == request.ContentLength64;
				passed &= request.ContentType == "text/plain; charset=utf-8";
				passed &= request.InputStream.ReadByte () == 'm';
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				var r = new HttpRequestMessage (HttpMethod.Put, $"http://localhost:{port}/Send_Content_Put/");
				r.Content = new StringContent ("my text");
				var response = client.SendAsync (r).Result;

				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#1");
				Assert.IsTrue (passed, "#2");
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Content_Put_CustomStream ()
		{
			bool passed = false;
			var handler = HttpClientTestHelpers.CreateHttpClientHandler ();
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Send_Content_Put_CustomStream/");
			AddListenerContext (listener, l => {
				var request = l.Request;
				if (HttpClientTestHelpers.IsSocketsHandler (handler))
					passed = -1 == request.ContentLength64;
				else
					passed = 44 == request.ContentLength64;
				passed &= request.ContentType == null;
			});

			try {
				var client = new HttpClient (handler);
				var r = new HttpRequestMessage (HttpMethod.Put, $"http://localhost:{port}/Send_Content_Put_CustomStream/");
				r.Content = new StreamContent (new CustomStream ());
				var response = client.SendAsync (r).Result;

				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#1");
				Assert.IsTrue (passed, "#2");
			} finally {
				listener.Abort ();

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
				return Task.FromResult (response);
			};

			Assert.AreEqual (response, client.SendAsync (request).Result, "#1");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
#if !MONOTOUCH_WATCH			
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif		
#endif
		public void Send_Invalid ()
		{
			var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
			try {
				client.SendAsync (null).Wait (WaitTimeout);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				var request = new HttpRequestMessage ();
				client.SendAsync (request).Wait (WaitTimeout);
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
				// Broken by design
				client.SendAsync (request).Wait (WaitTimeout);
				Assert.Fail ("#2");
			} catch (Exception) {
			}
		}

		[Test]
		public void Send_SameMessage ()
		{
			var mh = new HttpMessageHandlerMock ();

			var client = new HttpClient (mh);
			var request = new HttpRequestMessage (HttpMethod.Get, "http://xamarin.com");

			mh.OnSend = l => Task.FromResult (new HttpResponseMessage ());

			client.SendAsync (request).Wait (WaitTimeout);
			try {
				client.SendAsync (request).Wait (WaitTimeout);
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Post_TransferEncodingChunked ()
		{
			bool? failed = null;
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Post_TransferEncodingChunked/");
			AddListenerContext (listener, l => {
				try {
					var request = l.Request;

					Assert.IsNull (request.AcceptTypes, "#1");
					Assert.AreEqual (-1, request.ContentLength64, "#2");
					Assert.IsNull (request.ContentType, "#3");
					Assert.AreEqual (0, request.Cookies.Count, "#4");
					Assert.IsTrue (request.HasEntityBody, "#5");
					Assert.AreEqual ($"localhost:{port}", request.Headers ["Host"], "#6b");
					Assert.AreEqual ("POST", request.HttpMethod, "#7");
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
					Assert.AreEqual ("chunked", request.Headers ["Transfer-Encoding"], "#18");
					Assert.IsNull (request.Headers ["Content-Length"], "#19");
					failed = false;
				} catch (Exception e) {
					failed = true;
					Console.WriteLine (e);
				}
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();

				client.DefaultRequestHeaders.TransferEncodingChunked = true;

				var imageContent = new StreamContent (new MemoryStream ());

				var response = client.PostAsync ($"http://localhost:{port}/Post_TransferEncodingChunked/", imageContent).Result;

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "#101");
				Assert.AreEqual(false, failed, "#102");
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Post_StreamCaching ()
		{
			bool? failed = null;
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/Post_StreamCaching/");
			AddListenerContext (listener, l => {
				try {
					var request = l.Request;

					Assert.IsNull (request.AcceptTypes, "#1");
					Assert.AreEqual (0, request.ContentLength64, "#2");
					Assert.IsNull (request.ContentType, "#3");
					Assert.AreEqual (0, request.Cookies.Count, "#4");
					Assert.IsFalse (request.HasEntityBody, "#5");
					Assert.AreEqual ($"localhost:{port}", request.Headers ["Host"], "#6b");
					Assert.AreEqual ("POST", request.HttpMethod, "#7");
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
					Assert.IsNull (request.Headers ["Transfer-Encoding"], "#18");
					Assert.AreEqual ("0", request.Headers ["Content-Length"], "#19");
					failed = false;
				} catch (Exception e) {
					failed = true;
					Console.WriteLine (e);
				}
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();

				var imageContent = new StreamContent (new MemoryStream ());

				var response = client.PostAsync ($"http://localhost:{port}/Post_StreamCaching/", imageContent).Result;

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "#101");
				Assert.AreEqual(false, failed, "#102");
			} finally {
				listener.Close ();
			}
		}

		[Test]
		[Category ("MobileNotWorking")] // Missing encoding
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void GetString_Many ()
		{
			Action<HttpListenerContext> context = (HttpListenerContext l) => {
				var response = l.Response;
				response.StatusCode = 200;
				response.OutputStream.WriteByte (0x68);
				response.OutputStream.WriteByte (0x65);
				response.OutputStream.WriteByte (0x6c);
				response.OutputStream.WriteByte (0x6c);
				response.OutputStream.WriteByte (0x6f);
			};

			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/GetString_Many/");
			AddListenerContext (listener, context);  // creates a default request handler
			AddListenerContext (listener, context);  // add another request handler for the second request

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				var t1 = client.GetStringAsync ($"http://localhost:{port}/GetString_Many/");
				var t2 = client.GetStringAsync ($"http://localhost:{port}/GetString_Many/");
				Assert.IsTrue (Task.WaitAll (new [] { t1, t2 }, WaitTimeout));
				Assert.AreEqual ("hello", t1.Result, "#1");
				Assert.AreEqual ("hello", t2.Result, "#2");
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void GetByteArray_ServerError ()
		{
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/GetByteArray_ServerError/");
			AddListenerContext (listener, l => {
				var response = l.Response;
				response.StatusCode = 500;
				l.Response.OutputStream.WriteByte (72);
			});

			try {
				var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ();
				try {
					client.GetByteArrayAsync ($"http://localhost:{port}/GetByteArray_ServerError/").Wait (WaitTimeout);
					Assert.Fail ("#1");
				} catch (AggregateException e) {
					Assert.That (e.InnerException, Is.InstanceOf<HttpRequestException> (), $"#2: {e}");
				}
			} finally {
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DisallowAutoRedirect ()
		{
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/DisallowAutoRedirect/");
			AddListenerContext (listener, l => {
				var request = l.Request;
				var response = l.Response;
				
				response.StatusCode = (int)HttpStatusCode.Moved;
				response.RedirectLocation = "http://xamarin.com/";
			});

			try {
				var chandler = HttpClientTestHelpers.CreateHttpClientHandler ();
				chandler.AllowAutoRedirect = false;
				var client = new HttpClient (chandler);

				try {
					client.GetStringAsync ($"http://localhost:{port}/DisallowAutoRedirect/").Wait (WaitTimeout);
					Assert.Fail ("#1");
				} catch (AggregateException e) {
					Assert.That (e.InnerException, Is.InstanceOf<HttpRequestException> (), $"#2: {e}");
				}
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void RequestUriAfterRedirect ()
		{
			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/RequestUriAfterRedirect/");
			var listener2 = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int redirectPort, "/RequestUriAfterRedirect/");

			AddListenerContext (listener, l => {
				var request = l.Request;
				var response = l.Response;

				response.StatusCode = (int)HttpStatusCode.Moved;
				response.RedirectLocation = $"http://localhost:{redirectPort}/RequestUriAfterRedirect/";
			});

			AddListenerContext (listener2, l => {
				var response = l.Response;

				response.StatusCode = (int)HttpStatusCode.OK;
				response.OutputStream.WriteByte (0x68);
				response.OutputStream.WriteByte (0x65);
				response.OutputStream.WriteByte (0x6c);
				response.OutputStream.WriteByte (0x6c);
				response.OutputStream.WriteByte (0x6f);
			});

			try {
				var chandler = HttpClientTestHelpers.CreateHttpClientHandler ();
				chandler.AllowAutoRedirect = true;
				var client = new HttpClient (chandler);

				var r = client.GetAsync ($"http://localhost:{port}/RequestUriAfterRedirect/");
				Assert.IsTrue (r.Wait (WaitTimeout), "#1");
				var resp = r.Result;
				Assert.AreEqual ($"http://localhost:{redirectPort}/RequestUriAfterRedirect/", resp.RequestMessage.RequestUri.AbsoluteUri, "#2");
				Assert.AreEqual ("hello", resp.Content.ReadAsStringAsync ().Result, "#3");
			} finally {
				listener.Abort ();
				listener.Close ();
				listener2.Abort ();
				listener2.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		/*
		 * Properties may only be modified before sending the first request.
		 */
		public void ModifyHandlerAfterFirstRequest ()
		{
			var chandler = HttpClientTestHelpers.CreateHttpClientHandler ();
			chandler.AllowAutoRedirect = true;
			var client = new HttpClient (chandler, true);

			var listener = NetworkHelpers.CreateAndStartHttpListener("http://*:", out int port, "/ModifyHandlerAfterFirstRequest/");
			AddListenerContext (listener, l => {
				var response = l.Response;
				response.StatusCode = 200;
				response.OutputStream.WriteByte (55);
			});

			try {
				client.GetStringAsync ($"http://localhost:{port}/ModifyHandlerAfterFirstRequest/").Wait (WaitTimeout);
				try {
					chandler.AllowAutoRedirect = false;
					Assert.Fail ("#1");
				} catch (InvalidOperationException) {
					;
				}
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		// Using HttpClientHandler, which indirectly requires BSD sockets.
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		/*
		 * However, this policy is not enforced for custom handlers and there
		 * is also no way a derived class could tell its HttpClientHandler parent
		 * that it just sent a request.
		 * 
		 */
		public void ModifyHandlerAfterFirstRequest_Mock ()
		{
			var ch = new HttpClientHandlerMock ();
			ch.AllowAutoRedirect = true;

			var client = new HttpClient (ch);

			ch.OnSend = (l) => {
				return Task.FromResult (new HttpResponseMessage ());
			};

			client.GetAsync ("http://xamarin.com").Wait (WaitTimeout);
			ch.AllowAutoRedirect = false;
		}

#if !FEATURE_NO_BSD_SOCKETS
		[Test]
		// https://github.com/mono/mono/issues/7355
		public void WildcardConnect ()
		{
			if (HttpClientTestHelpers.UsingSocketsHandler)
				Assert.Ignore ("Throws System.NullReferenceException");

			try {
				using (var client = HttpClientTestHelpers.CreateHttpClientWithHttpClientHandler ()) {
					client.GetAsync ("http://255.255.255.255").Wait (WaitTimeout);
				}
			} catch (AggregateException e) {
				Assert.That (e.InnerException, Is.InstanceOf<HttpRequestException> (), "#1");
				var rex = (HttpRequestException)e.InnerException;
				Assert.That (rex.InnerException, Is.InstanceOf<WebException> (), "#2");
				var wex = (WebException)rex.InnerException;
				Assert.That (wex.Status, Is.EqualTo (WebExceptionStatus.ConnectFailure), "#3");
			}
		}
#endif

		void AddListenerContext (HttpListener l, Action<HttpListenerContext> contextAssert)
		{
			l.BeginGetContext (ar => {
				var ctx = l.EndGetContext (ar);

				try {
					if (contextAssert != null)
						contextAssert (ctx);
				} finally {
					ctx.Response.Close ();
				}
			}, null);
		}
	}
}
