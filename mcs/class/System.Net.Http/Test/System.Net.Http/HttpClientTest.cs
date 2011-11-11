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
			client.DefaultRequestHeaders.Add ("Referer", "xxxx");

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
	}
}
