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

		const int WaitTimeout = 5000;

		string port, TestHost, LocalServer;

		[SetUp]
		public void SetupFixture ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				port = "810";
			} else {
				port = "8810";
			}

			TestHost = "localhost:" + port;
			LocalServer = string.Format ("http://{0}/", TestHost);
		}

		[Test]
		public void Ctor_Default ()
		{
			var client = new HttpClient ();
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
		public void CancelRequestViaProxy ()
		{
			var handler = new HttpClientHandler {
				Proxy = new WebProxy ("192.168.10.25:8888/"), // proxy that doesn't exist
				UseProxy = true,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			};

			var httpClient = new HttpClient (handler) {
				BaseAddress = new Uri ("https://google.com"),
				Timeout = TimeSpan.FromSeconds (1)
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
				Assert.IsTrue (e.InnerException is TaskCanceledException, "#2");
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
				return Task.FromResult (response);
			};

			Assert.AreEqual (response, client.SendAsync (request).Result, "#1");
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
				return Task.FromResult (response);
			};

			Assert.AreEqual (response, client.SendAsync (request).Result, "#1");
		}

		[Test]
		public void Send_Complete_Default ()
		{
			bool? failed = null;
			var listener = CreateListener (l => {
				try {
					var request = l.Request;
	
					Assert.IsNull (request.AcceptTypes, "#1");
					Assert.AreEqual (0, request.ContentLength64, "#2");
					Assert.IsNull (request.ContentType, "#3");
					Assert.AreEqual (0, request.Cookies.Count, "#4");
					Assert.IsFalse (request.HasEntityBody, "#5");
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
					failed = false;
				} catch {
					failed = true;
				}
			});

			try {
				var client = new HttpClient ();
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
				Assert.AreEqual (false, failed, "#102");
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Complete_Version_1_0 ()
		{
			bool? failed = null;
			
			var listener = CreateListener (l => {
				try {
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
					failed = false;
				} catch {
					failed = true;
				}
			});

			try {
				var client = new HttpClient ();
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);
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
		public void Send_Complete_ClientHandlerSettings ()
		{
			bool? failed = null;
			
			var listener = CreateListener (l => {
				var request = l.Request;
				
				try {
					Assert.IsNull (request.AcceptTypes, "#1");
					Assert.AreEqual (0, request.ContentLength64, "#2");
					Assert.IsNull (request.ContentType, "#3");
					Assert.AreEqual (1, request.Cookies.Count, "#4");
					Assert.AreEqual (new Cookie ("mycookie", "vv"), request.Cookies[0], "#4a");
					Assert.IsFalse (request.HasEntityBody, "#5");
					Assert.AreEqual (4, request.Headers.Count, "#6");
					Assert.AreEqual (TestHost, request.Headers["Host"], "#6a");
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
		public void Send_Complete_CustomHeaders ()
		{
			bool? failed = null;
			
			var listener = CreateListener (l => {
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
				var client = new HttpClient ();
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);
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
				Assert.AreEqual (HttpVersion.Version11, response.Version, "#111");
				
				Assert.AreEqual (false, failed, "#112");
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Complete_Content ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;
				l.Response.OutputStream.WriteByte (55);
				l.Response.OutputStream.WriteByte (75);
			});

			try {
				var client = new HttpClient ();
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);
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
		public void Send_Complete_Content_MaxResponseContentBufferSize ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;
				var b = new byte[4000];
				l.Response.OutputStream.Write (b, 0, b.Length);
			});

			try {
				var client = new HttpClient ();
				client.MaxResponseContentBufferSize = 1000;
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual (4000, response.Content.ReadAsStringAsync ().Result.Length, "#100");
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Complete_Content_MaxResponseContentBufferSize_Error ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;
				var b = new byte[4000];
				l.Response.OutputStream.Write (b, 0, b.Length);
			});

			try {
				var client = new HttpClient ();
				client.MaxResponseContentBufferSize = 1000;
				var request = new HttpRequestMessage (HttpMethod.Get, LocalServer);

				try {
					client.SendAsync (request, HttpCompletionOption.ResponseContentRead).Wait (WaitTimeout);
					Assert.Fail ("#2");
				} catch (AggregateException e) {
					Assert.IsTrue (e.InnerException is HttpRequestException, "#3");
				}

			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Complete_NoContent ()
		{
			foreach (var method in new HttpMethod[] { HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete }) {
				bool? failed = null;
				var listener = CreateListener (l => {
					try {
						var request = l.Request;

						Assert.AreEqual (2, request.Headers.Count, "#1");
						Assert.AreEqual ("0", request.Headers ["Content-Length"], "#1b");
						Assert.AreEqual (method.Method, request.HttpMethod, "#2");
						failed = false;
					} catch {
						failed = true;
					}
				});

				try {
					var client = new HttpClient ();
					var request = new HttpRequestMessage (method, LocalServer);
					var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

					Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
					Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#101");
					Assert.AreEqual (false, failed, "#102");
				} finally {
					listener.Close ();
				}
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
				var response = client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead).Result;

				Assert.AreEqual ("", response.Content.ReadAsStringAsync ().Result, "#100");
				Assert.AreEqual (HttpStatusCode.InternalServerError, response.StatusCode, "#101");
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Content_Get ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;
				l.Response.OutputStream.WriteByte (72);
			});

			try {
				var client = new HttpClient ();
				var r = new HttpRequestMessage (HttpMethod.Get, LocalServer);
				var response = client.SendAsync (r).Result;

				Assert.AreEqual ("H", response.Content.ReadAsStringAsync ().Result);
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Content_BomEncoding ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;

				var str = l.Response.OutputStream;
				str.WriteByte (0xEF);
				str.WriteByte (0xBB);
				str.WriteByte (0xBF);
				str.WriteByte (71);
			});

			try {
				var client = new HttpClient ();
				var r = new HttpRequestMessage (HttpMethod.Get, LocalServer);
				var response = client.SendAsync (r).Result;

				Assert.AreEqual ("G", response.Content.ReadAsStringAsync ().Result);
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void Send_Content_Put ()
		{
			bool passed = false;
			var listener = CreateListener (l => {
				var request = l.Request;
				passed = 7 == request.ContentLength64;
				passed &= request.ContentType == "text/plain; charset=utf-8";
				passed &= request.InputStream.ReadByte () == 'm';
			});

			try {
				var client = new HttpClient ();
				var r = new HttpRequestMessage (HttpMethod.Put, LocalServer);
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
		public void Send_Invalid ()
		{
			var client = new HttpClient ();
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
		public void GetString_RelativeUri ()
		{
			var client = new HttpClient ();
			client.BaseAddress = new Uri ("http://en.wikipedia.org/wiki/");
			var uri = new Uri ("Computer", UriKind.Relative);

			Assert.That (client.GetStringAsync (uri).Result != null);
			Assert.That (client.GetStringAsync ("Computer").Result != null);
		}

		[Test]
		[Category ("MobileNotWorking")] // Missing encoding
		public void GetString_Many ()
		{
			var client = new HttpClient ();
			var t1 = client.GetStringAsync ("http://www.google.com");
			var t2 = client.GetStringAsync ("http://www.google.com");
			Assert.IsTrue (Task.WaitAll (new [] { t1, t2 }, WaitTimeout));		
		}

		[Test]
		public void GetByteArray_ServerError ()
		{
			var listener = CreateListener (l => {
				var response = l.Response;
				response.StatusCode = 500;
				l.Response.OutputStream.WriteByte (72);
			});

			try {
				var client = new HttpClient ();
				try {
					client.GetByteArrayAsync (LocalServer).Wait (WaitTimeout);
					Assert.Fail ("#1");
				} catch (AggregateException e) {
					Assert.IsTrue (e.InnerException is HttpRequestException , "#2");
				}
			} finally {
				listener.Close ();
			}
		}

		[Test]
		public void DisallowAutoRedirect ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;
				var response = l.Response;
				
				response.StatusCode = (int)HttpStatusCode.Moved;
				response.RedirectLocation = "http://xamarin.com/";
			});

			try {
				var chandler = new HttpClientHandler ();
				chandler.AllowAutoRedirect = false;
				var client = new HttpClient (chandler);

				try {
					client.GetStringAsync (LocalServer).Wait (WaitTimeout);
					Assert.Fail ("#1");
				} catch (AggregateException e) {
					Assert.IsTrue (e.InnerException is HttpRequestException, "#2");
				}
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
		public void RequestUriAfterRedirect ()
		{
			var listener = CreateListener (l => {
				var request = l.Request;
				var response = l.Response;

				response.StatusCode = (int)HttpStatusCode.Moved;
				response.RedirectLocation = "http://xamarin.com/";
			});

			try {
				var chandler = new HttpClientHandler ();
				chandler.AllowAutoRedirect = true;
				var client = new HttpClient (chandler);

				var resp = client.GetAsync (LocalServer).Result;
				Assert.AreEqual ("http://xamarin.com/", resp.RequestMessage.RequestUri.AbsoluteUri, "#1");
			} finally {
				listener.Abort ();
				listener.Close ();
			}
		}

		[Test]
		/*
		 * Properties may only be modified before sending the first request.
		 */
		public void ModifyHandlerAfterFirstRequest ()
		{
			var chandler = new HttpClientHandler ();
			chandler.AllowAutoRedirect = true;
			var client = new HttpClient (chandler, true);

			var listener = CreateListener (l => {
				var response = l.Response;
				response.StatusCode = 200;
				response.OutputStream.WriteByte (55);
			});

			try {
				client.GetStringAsync (LocalServer).Wait (WaitTimeout);
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

		HttpListener CreateListener (Action<HttpListenerContext> contextAssert)
		{
			var l = new HttpListener ();
			l.Prefixes.Add (string.Format ("http://+:{0}/", port));
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
