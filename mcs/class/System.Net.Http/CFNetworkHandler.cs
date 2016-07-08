//
// CFNetworkHandler.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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

using System.Threading;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Net;

#if XAMCORE_4_0
using CFNetwork;
using CoreFoundation;
using CF=CoreFoundation;
#elif XAMCORE_2_0
using CoreServices;
using CoreFoundation;
using CF=CoreFoundation;
#else
using MonoTouch.CoreServices;
using MonoTouch.CoreFoundation;
using CF=MonoTouch.CoreFoundation;
#endif

namespace System.Net.Http
{
	public class CFNetworkHandler : HttpMessageHandler
	{
		class StreamBucket
		{
			public TaskCompletionSource<HttpResponseMessage> Response;
			public HttpRequestMessage Request;
			public CancellationTokenRegistration CancellationTokenRegistration;
			public CFContentStream ContentStream;

			public void Close ()
			{
				CancellationTokenRegistration.Dispose ();
                                if (ContentStream != null) {
                                    ContentStream.Close ();
                                }
			}
		}

		bool allowAutoRedirect;
		bool sentRequest;
		bool useSystemProxy;
		CookieContainer cookies;

		Dictionary<IntPtr, StreamBucket> streamBuckets;

		public CFNetworkHandler ()
		{
			allowAutoRedirect = true;
			streamBuckets = new Dictionary<IntPtr, StreamBucket> ();
		}

		void EnsureModifiability ()
		{
			if (sentRequest)
				throw new InvalidOperationException (
					"This instance has already started one or more requests. " +
					"Properties can only be modified before sending the first request.");
		}

		public bool AllowAutoRedirect {
			get {
				return allowAutoRedirect;
			}
			set {
				EnsureModifiability ();
				allowAutoRedirect = value;
			}
		}

		public CookieContainer CookieContainer {
			get {
				return cookies ?? (cookies = new CookieContainer ());
			}
			set {
				EnsureModifiability ();
				cookies = value;
			}
		}

		public bool UseSystemProxy {
			get {
				return useSystemProxy;
			}
			set {
				EnsureModifiability ();
				useSystemProxy = value;
			}
		}

		// TODO: Add more properties

		protected override void Dispose (bool disposing)
		{
			// TODO: CloseStream remaining stream buckets if there are any

			base.Dispose (disposing);
		}

		CFHTTPMessage CreateWebRequestAsync (HttpRequestMessage request)
		{
			var req = CFHTTPMessage.CreateRequest (request.RequestUri, request.Method.Method, request.Version);

			// TODO:
/*
			if (wr.ProtocolVersion == HttpVersion.Version10) {
				wr.KeepAlive = request.Headers.ConnectionKeepAlive;
			} else {
				wr.KeepAlive = request.Headers.ConnectionClose != true;
			}

			if (useDefaultCredentials) {
				wr.UseDefaultCredentials = true;
			} else {
				wr.Credentials = credentials;
			}

			if (useProxy) {
				wr.Proxy = proxy;
			}
*/
			if (cookies != null) {
				string cookieHeader = cookies.GetCookieHeader (request.RequestUri);
				if (cookieHeader != "")
					req.SetHeaderFieldValue ("Cookie", cookieHeader);
			}

			foreach (var header in request.Headers) {
				foreach (var value in header.Value) {
					req.SetHeaderFieldValue (header.Key, value);
				}
			}

			if (request.Content != null) {
				foreach (var header in request.Content.Headers) {
					foreach (var value in header.Value) {
						req.SetHeaderFieldValue (header.Key, value);
					}
				}
			}

			return req;
		}

		protected internal override async Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			sentRequest = true;

			CFHTTPStream stream;
			using (var message = CreateWebRequestAsync (request))
			{
				if (request.Content != null) {
					var data = await request.Content.ReadAsByteArrayAsync ().ConfigureAwait (false);
					message.SetBody (data);
				}

				stream = CFHTTPStream.CreateForHTTPRequest (message);
			}

			if (useSystemProxy) {
				var proxies = CF.CFNetwork.GetSystemProxySettings ();
				if (proxies.HTTPEnable) {
					stream.SetProxy (proxies);
				}
			}

			stream.ShouldAutoredirect = allowAutoRedirect;
			stream.HasBytesAvailableEvent += HandleHasBytesAvailableEvent;
			stream.ErrorEvent += HandleErrorEvent;
			stream.ClosedEvent += HandleClosedEvent;

			var response = new TaskCompletionSource<HttpResponseMessage> ();

			if (cancellationToken.IsCancellationRequested) {
				response.SetCanceled ();
				return await response.Task;
			}

			var bucket = new StreamBucket () {
				Request = request,
				Response = response,
			};

			streamBuckets.Add (stream.Handle, bucket);

			//
			// Always schedule stream events handling on main-loop. Due to ConfigureAwait (false) we may end up
			// on any thread-pool thread which may not have run-loop running
			//
#if XAMCORE_2_0
			stream.EnableEvents (CF.CFRunLoop.Main, CF.CFRunLoop.ModeCommon);
#else
			stream.EnableEvents (CF.CFRunLoop.Main, CF.CFRunLoop.CFRunLoopCommonModes);
#endif

			stream.Open ();

			bucket.CancellationTokenRegistration = cancellationToken.Register (() => {
				StreamBucket bucket2;
				if (!streamBuckets.TryGetValue (stream.Handle, out bucket2))
					return;

				bucket2.Response.TrySetCanceled ();
				CloseStream (stream);
			});

			return await response.Task;
		}

		void HandleErrorEvent (object sender, CFStream.StreamEventArgs e)
		{
			var stream = (CFHTTPStream)sender;

			StreamBucket bucket;
			if (!streamBuckets.TryGetValue (stream.Handle, out bucket))
				return;

			bucket.Response.TrySetException (stream.GetError ());
			CloseStream (stream);
		}

		void HandleClosedEvent (object sender, CFStream.StreamEventArgs e)
		{
			var stream = (CFHTTPStream)sender;
			CloseStream (stream);
		}

		void CloseStream (CFHTTPStream stream)
		{
			StreamBucket bucket;
			if (streamBuckets.TryGetValue (stream.Handle, out bucket)) {
				bucket.Close ();
				streamBuckets.Remove (stream.Handle);
			}

			stream.Close ();
		}

		void HandleHasBytesAvailableEvent (object sender, CFStream.StreamEventArgs e)
		{
			var stream = (CFHTTPStream) sender;

			StreamBucket bucket;
			if (!streamBuckets.TryGetValue (stream.Handle, out bucket))
				return;

			if (bucket.Response.Task.IsCompleted) {
				bucket.ContentStream.ReadStreamData ();
				return;
			}

			var header = stream.GetResponseHeader ();

			// Is this possible?
			if (!header.IsHeaderComplete)
				throw new NotImplementedException ();

			bucket.ContentStream = new CFContentStream (stream);
				
			var response_msg = new HttpResponseMessage (header.ResponseStatusCode);
			response_msg.RequestMessage = bucket.Request;
			response_msg.ReasonPhrase = header.ResponseStatusLine;
			response_msg.Content = bucket.ContentStream;

			var fields = header.GetAllHeaderFields ();
			if (fields != null) {
				foreach (var entry in fields) {
					if (entry.Key == null)
						continue;

					var key = entry.Key.ToString ();
					var value = entry.Value == null ? string.Empty : entry.Value.ToString ();
					HttpHeaders item_headers;
					if (HttpHeaders.GetKnownHeaderKind (key) == Headers.HttpHeaderKind.Content) {
						item_headers = response_msg.Content.Headers;
					} else {
						item_headers = response_msg.Headers;

						if (cookies != null && (key == "Set-Cookie" || key == "Set-Cookie2"))
							AddCookie (value, bucket.Request.RequestUri, key);
					}

					item_headers.TryAddWithoutValidation (key, value);
				}
			}

			bucket.Response.TrySetResult (response_msg);

			bucket.ContentStream.ReadStreamData ();
		}

		void AddCookie (string value, Uri uri, string header)
		{
			CookieCollection cookies1 = null;
			try {
				cookies1 = cookies.CookieCutter (uri, header, value, false);
			} catch {
			}

			if (cookies1 != null && cookies1.Count != 0) 
				cookies.Add (cookies1);
		}
	}
}
