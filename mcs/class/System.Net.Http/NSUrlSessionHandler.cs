//
// NSUrlSessionHandler.cs:
//
// Authors:
//     Paul Betts <paul@paulbetts.org>
//     Nick Berardi <nick@nickberardi.com>
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

#if UNIFIED
using CoreFoundation;
using Foundation;
using Security;
#else
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.Security;
using System.Globalization;
using nint = System.Int32;
using nuint = System.UInt32;
#endif

#if !MONOMAC
using UIKit;
#endif

#if SYSTEM_NET_HTTP
namespace System.Net.Http {
#else
namespace Foundation {
#endif

	// useful extensions for the class in order to set it in a header
	static class NSHttpCookieExtensions
	{
		static void AppendSegment(StringBuilder builder, string name, string value)
		{
			if (builder.Length > 0)
				builder.Append ("; ");

			builder.Append (name);
			if (value != null)
				builder.Append ("=").Append (value);
		}

		// returns the header for a cookie
		public static string GetHeaderValue (this NSHttpCookie cookie)
		{
			var header = new StringBuilder();
			AppendSegment (header, cookie.Name, cookie.Value);
			AppendSegment (header, NSHttpCookie.KeyPath.ToString (), cookie.Path.ToString ());
			AppendSegment (header, NSHttpCookie.KeyDomain.ToString (), cookie.Domain.ToString ());
			AppendSegment (header, NSHttpCookie.KeyVersion.ToString (), cookie.Version.ToString ());

			if (cookie.Comment != null)
				AppendSegment (header, NSHttpCookie.KeyComment.ToString (), cookie.Comment.ToString());

			if (cookie.CommentUrl != null)
				AppendSegment (header, NSHttpCookie.KeyCommentUrl.ToString (), cookie.CommentUrl.ToString());

			if (cookie.Properties.ContainsKey (NSHttpCookie.KeyDiscard))
				AppendSegment (header, NSHttpCookie.KeyDiscard.ToString (), null);

			if (cookie.ExpiresDate != null) {
				// Format according to RFC1123; 'r' uses invariant info (DateTimeFormatInfo.InvariantInfo)
				var dateStr = ((DateTime) cookie.ExpiresDate).ToUniversalTime ().ToString("r", CultureInfo.InvariantCulture);
				AppendSegment (header, NSHttpCookie.KeyExpires.ToString (), dateStr);
			}

			if (cookie.Properties.ContainsKey (NSHttpCookie.KeyMaximumAge)) {
				var timeStampString = (NSString) cookie.Properties[NSHttpCookie.KeyMaximumAge];
				AppendSegment (header, NSHttpCookie.KeyMaximumAge.ToString (), timeStampString);
			}

			if (cookie.IsSecure)
				AppendSegment (header, NSHttpCookie.KeySecure.ToString(), null);

			if (cookie.IsHttpOnly)
				AppendSegment (header, "httponly", null); // Apple does not show the key for the httponly

			return header.ToString ();
		}
	}

	public partial class NSUrlSessionHandler : HttpMessageHandler
	{
		private const string SetCookie = "Set-Cookie";
		readonly Dictionary<string, string> headerSeparators = new Dictionary<string, string> {
			["User-Agent"] = " ",
			["Server"] = " "
		};

		readonly NSUrlSession session;
		readonly Dictionary<NSUrlSessionTask, InflightData> inflightRequests;
		readonly object inflightRequestsLock = new object ();
#if !MONOMAC && !MONOTOUCH_WATCH
		readonly bool isBackgroundSession = false;
		NSObject notificationToken;  // needed to make sure we do not hang if not using a background session
#endif

		static NSUrlSessionConfiguration CreateConfig ()
		{
			// modifying the configuration does not affect future calls
			var config = NSUrlSessionConfiguration.DefaultSessionConfiguration;
			// but we want, by default, the timeout from HttpClient to have precedence over the one from NSUrlSession
			// Double.MaxValue does not work, so default to 24 hours
			config.TimeoutIntervalForRequest = 24 * 60 * 60;
			config.TimeoutIntervalForResource = 24 * 60 * 60;
			return config;
		}

		public NSUrlSessionHandler () : this (CreateConfig ())
		{
		}

		[CLSCompliant (false)]
		public NSUrlSessionHandler (NSUrlSessionConfiguration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException (nameof (configuration));

#if !MONOMAC  && !MONOTOUCH_WATCH 
			// if the configuration has an identifier, we are dealing with a background session, 
			// therefore, we do not have to listen to the notifications.
			isBackgroundSession = !string.IsNullOrEmpty (configuration.Identifier);
#endif

			AllowAutoRedirect = true;

			// we cannot do a bitmask but we can set the minimum based on ServicePointManager.SecurityProtocol minimum
			var sp = ServicePointManager.SecurityProtocol;
			if ((sp & SecurityProtocolType.Ssl3) != 0)
				configuration.TLSMinimumSupportedProtocol = SslProtocol.Ssl_3_0;
			else if ((sp & SecurityProtocolType.Tls) != 0)
				configuration.TLSMinimumSupportedProtocol = SslProtocol.Tls_1_0;
			else if ((sp & SecurityProtocolType.Tls11) != 0)
				configuration.TLSMinimumSupportedProtocol = SslProtocol.Tls_1_1;
			else if ((sp & SecurityProtocolType.Tls12) != 0)
				configuration.TLSMinimumSupportedProtocol = SslProtocol.Tls_1_2;

			session = NSUrlSession.FromConfiguration (configuration, (INSUrlSessionDelegate) new NSUrlSessionHandlerDelegate (this), null);
			inflightRequests = new Dictionary<NSUrlSessionTask, InflightData> ();
		}

#if !MONOMAC  && !MONOTOUCH_WATCH

		void AddNotification ()
		{
			if (!isBackgroundSession && notificationToken == null)
				notificationToken = NSNotificationCenter.DefaultCenter.AddObserver (UIApplication.WillResignActiveNotification, BackgroundNotificationCb);
		}

		void RemoveNotification ()
		{
			if (notificationToken != null) {
				NSNotificationCenter.DefaultCenter.RemoveObserver (notificationToken);
				notificationToken = null;
			}
		}

		void BackgroundNotificationCb (NSNotification obj)
		{
			// we do not need to call the lock, we call cancel on the source, that will trigger all the needed code to 
			// clean the resources and such
			foreach (var r in inflightRequests.Values) {
				r.CompletionSource.TrySetCanceled ();
			}
		}
#endif

		public long MaxInputInMemory { get; set; } = long.MaxValue;

		void RemoveInflightData (NSUrlSessionTask task, bool cancel = true)
		{
			lock (inflightRequestsLock) {
				if (inflightRequests.TryGetValue (task, out var data)) {
					if (cancel)
						data.CancellationTokenSource.Cancel ();
					data.Dispose ();
					inflightRequests.Remove (task);
				}
#if !MONOMAC  && !MONOTOUCH_WATCH
				// do we need to be notified? If we have not inflightData, we do not
				if (inflightRequests.Count == 0)
					RemoveNotification ();
#endif
			}

			if (cancel)
				task?.Cancel ();

			task?.Dispose ();
		}

		protected override void Dispose (bool disposing)
		{
#if !MONOMAC  && !MONOTOUCH_WATCH
			// remove the notification if present, method checks against null
			RemoveNotification ();
#endif
			lock (inflightRequestsLock) {
				foreach (var pair in inflightRequests) {
					pair.Key?.Cancel ();
					pair.Key?.Dispose ();
					pair.Value?.Dispose ();
				}

				inflightRequests.Clear ();
			}
			base.Dispose (disposing);
		}

		bool disableCaching;

		public bool DisableCaching {
			get {
				return disableCaching;
			}
			set {
				EnsureModifiability ();
				disableCaching = value;
			}
		}

		string GetHeaderSeparator (string name)
		{
			string value;
			if (!headerSeparators.TryGetValue (name, out value))
				value = ",";
			return value;
		}

		async Task<NSUrlRequest> CreateRequest (HttpRequestMessage request)
		{
			var stream = Stream.Null;
			var headers = request.Headers as IEnumerable<KeyValuePair<string, IEnumerable<string>>>;

			if (request.Content != null) {
				stream = await request.Content.ReadAsStreamAsync ().ConfigureAwait (false);
				headers = System.Linq.Enumerable.ToArray(headers.Union (request.Content.Headers));
			}

			var nsrequest = new NSMutableUrlRequest {
				AllowsCellularAccess = true,
				CachePolicy = DisableCaching ? NSUrlRequestCachePolicy.ReloadIgnoringCacheData : NSUrlRequestCachePolicy.UseProtocolCachePolicy,
				HttpMethod = request.Method.ToString ().ToUpperInvariant (),
				Url = NSUrl.FromString (request.RequestUri.AbsoluteUri),
				Headers = headers.Aggregate (new NSMutableDictionary (), (acc, x) => {
					acc.Add (new NSString (x.Key), new NSString (string.Join (GetHeaderSeparator (x.Key), x.Value)));
					return acc;
				})
			};
			if (stream != Stream.Null) {
				// HttpContent.TryComputeLength is `protected internal` :-( but it's indirectly called by headers
				var length = request.Content.Headers.ContentLength;
				if (length.HasValue && (length <= MaxInputInMemory))
					nsrequest.Body = NSData.FromStream (stream);
				else
					nsrequest.BodyStream = new WrappedNSInputStream (stream);
			}
			return nsrequest;
		}

#if SYSTEM_NET_HTTP || MONOMAC
		internal
#endif
		protected override async Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Volatile.Write (ref sentRequest, true);

			var nsrequest = await CreateRequest (request).ConfigureAwait(false);
			var dataTask = session.CreateDataTask (nsrequest);

			var tcs = new TaskCompletionSource<HttpResponseMessage> ();

			lock (inflightRequestsLock) {
#if !MONOMAC  && !MONOTOUCH_WATCH
				// Add the notification whenever needed
				AddNotification ();
#endif
				inflightRequests.Add (dataTask, new InflightData {
					RequestUrl = request.RequestUri.AbsoluteUri,
					CompletionSource = tcs,
					CancellationToken = cancellationToken,
					CancellationTokenSource = new CancellationTokenSource (),
					Stream = new NSUrlSessionDataTaskStream (),
					Request = request
				});
			}

			if (dataTask.State == NSUrlSessionTaskState.Suspended)
				dataTask.Resume ();

			// as per documentation: 
			// If this token is already in the canceled state, the 
			// delegate will be run immediately and synchronously.
			// Any exception the delegate generates will be 
			// propagated out of this method call.
			//
			// The execution of the register ensures that if we 
			// receive a already cancelled token or it is cancelled
			// just before this call, we will cancel the task. 
			// Other approaches are harder, since querying the state
			// of the token does not guarantee that in the next
			// execution a threads cancels it.
			cancellationToken.Register (() => {
				RemoveInflightData (dataTask);
				tcs.TrySetCanceled ();
			});

			return await tcs.Task.ConfigureAwait (false);
		}

#if MONOMAC
		// Needed since we strip during linking since we're inside a product assembly.
		[Preserve (AllMembers = true)]
#endif
		partial class NSUrlSessionHandlerDelegate : NSUrlSessionDataDelegate
		{
			readonly NSUrlSessionHandler sessionHandler;

			public NSUrlSessionHandlerDelegate (NSUrlSessionHandler handler)
			{
				sessionHandler = handler;
			}

			InflightData GetInflightData (NSUrlSessionTask task)
			{
				var inflight = default (InflightData);

				lock (sessionHandler.inflightRequestsLock)
					if (sessionHandler.inflightRequests.TryGetValue (task, out inflight)) {
						// ensure that we did not cancel the request, if we did, do cancel the task
						if (inflight.CancellationToken.IsCancellationRequested)
							task?.Cancel ();
						return inflight;
					}

				// if we did not manage to get the inflight data, we either got an error or have been canceled, lets cancel the task, that will execute DidCompleteWithError
				task?.Cancel ();
				return null;
			}

			public override void DidReceiveResponse (NSUrlSession session, NSUrlSessionDataTask dataTask, NSUrlResponse response, Action<NSUrlSessionResponseDisposition> completionHandler)
			{
				var inflight = GetInflightData (dataTask);

				if (inflight == null)
					return;

				try {
					var urlResponse = (NSHttpUrlResponse)response;
					var status = (int)urlResponse.StatusCode;

					var content = new NSUrlSessionDataTaskStreamContent (inflight.Stream, () => {
						if (!inflight.Completed) {
							dataTask.Cancel ();
						}

						inflight.Disposed = true;
						inflight.Stream.TrySetException (new ObjectDisposedException ("The content stream was disposed."));

						sessionHandler.RemoveInflightData (dataTask);
					}, inflight.CancellationTokenSource.Token);

					// NB: The double cast is because of a Xamarin compiler bug
					var httpResponse = new HttpResponseMessage ((HttpStatusCode)status) {
						Content = content,
						RequestMessage = inflight.Request
					};
					httpResponse.RequestMessage.RequestUri = new Uri (urlResponse.Url.AbsoluteString);

					foreach (var v in urlResponse.AllHeaderFields) {
						// NB: Cocoa trolling us so hard by giving us back dummy dictionary entries
						if (v.Key == null || v.Value == null) continue;
						// NSUrlSession tries to be smart with cookies, we will not use the raw value but the ones provided by the cookie storage
						if (v.Key.ToString () == SetCookie) continue;

						httpResponse.Headers.TryAddWithoutValidation (v.Key.ToString (), v.Value.ToString ());
						httpResponse.Content.Headers.TryAddWithoutValidation (v.Key.ToString (), v.Value.ToString ());
					}

					var cookies = session.Configuration.HttpCookieStorage.CookiesForUrl (response.Url);
					for (var index = 0; index < cookies.Length; index++) {
						httpResponse.Headers.TryAddWithoutValidation (SetCookie, cookies [index].GetHeaderValue ());
					}

					inflight.Response = httpResponse;

					// We don't want to send the response back to the task just yet.  Because we want to mimic .NET behavior
					// as much as possible.  When the response is sent back in .NET, the content stream is ready to read or the
					// request has completed, because of this we want to send back the response in DidReceiveData or DidCompleteWithError
					if (dataTask.State == NSUrlSessionTaskState.Suspended)
						dataTask.Resume ();

				} catch (Exception ex) {
					inflight.CompletionSource.TrySetException (ex);
					inflight.Stream.TrySetException (ex);

					sessionHandler.RemoveInflightData (dataTask);
				}

				completionHandler (NSUrlSessionResponseDisposition.Allow);
			}

			public override void DidReceiveData (NSUrlSession session, NSUrlSessionDataTask dataTask, NSData data)
			{
				var inflight = GetInflightData (dataTask);

				if (inflight == null)
					return;

				inflight.Stream.Add (data);
				SetResponse (inflight);
			}

			public override void DidCompleteWithError (NSUrlSession session, NSUrlSessionTask task, NSError error)
			{
				var inflight = GetInflightData (task);

				// this can happen if the HTTP request times out and it is removed as part of the cancellation process
				if (inflight != null) {
					// set the stream as finished
					inflight.Stream.TrySetReceivedAllData ();

					// send the error or send the response back
					if (error != null) {
						// got an error, cancel the stream operatios before we do anything
						inflight.CancellationTokenSource.Cancel (); 
						inflight.Errored = true;

						var exc = createExceptionForNSError (error);
						inflight.CompletionSource.TrySetException (exc);
						inflight.Stream.TrySetException (exc);
					} else {
						inflight.Completed = true;
						SetResponse (inflight);
					}

					sessionHandler.RemoveInflightData (task, cancel: false);
				}
			}

			void SetResponse (InflightData inflight)
			{
				lock (inflight.Lock) {
					if (inflight.ResponseSent)
						return;

					if (inflight.CancellationTokenSource.Token.IsCancellationRequested)
						return;

					if (inflight.CompletionSource.Task.IsCompleted)
						return;

					var httpResponse = inflight.Response;

					inflight.ResponseSent = true;

					// EVIL HACK: having TrySetResult inline was blocking the request from completing
					Task.Run (() => inflight.CompletionSource.TrySetResult (httpResponse));
				}
			}

			public override void WillCacheResponse (NSUrlSession session, NSUrlSessionDataTask dataTask, NSCachedUrlResponse proposedResponse, Action<NSCachedUrlResponse> completionHandler)
			{
				completionHandler (sessionHandler.DisableCaching ? null : proposedResponse);
			}

			public override void WillPerformHttpRedirection (NSUrlSession session, NSUrlSessionTask task, NSHttpUrlResponse response, NSUrlRequest newRequest, Action<NSUrlRequest> completionHandler)
			{
				completionHandler (sessionHandler.AllowAutoRedirect ? newRequest : null);
			}

			public override void DidReceiveChallenge (NSUrlSession session, NSUrlSessionTask task, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
			{
				var inflight = GetInflightData (task);

				if (inflight == null)
					return;

				// case for the basic auth failing up front. As per apple documentation:
				// The URL Loading System is designed to handle various aspects of the HTTP protocol for you. As a result, you should not modify the following headers using
				// the addValue(_:forHTTPHeaderField:) or setValue(_:forHTTPHeaderField:) methods:
				// 	Authorization
				// 	Connection
				// 	Host
				// 	Proxy-Authenticate
				// 	Proxy-Authorization
				// 	WWW-Authenticate
				// but we are hiding such a situation from our users, we can nevertheless know if the header was added and deal with it. The idea is as follows,
				// check if we are in the first attempt, if we are (PreviousFailureCount == 0), we check the headers of the request and if we do have the Auth 
				// header, it means that we do not have the correct credentials, in any other case just do what it is expected.
				
				if (challenge.PreviousFailureCount == 0) {
					var authHeader = inflight.Request?.Headers?.Authorization;
					if (!(string.IsNullOrEmpty (authHeader?.Scheme) && string.IsNullOrEmpty (authHeader?.Parameter))) {
						completionHandler (NSUrlSessionAuthChallengeDisposition.RejectProtectionSpace, null);
						return;
					}
				}

				if (sessionHandler.Credentials != null && TryGetAuthenticationType (challenge.ProtectionSpace, out string authType)) {
					NetworkCredential credentialsToUse = null;
					if (authType != RejectProtectionSpaceAuthType) {
						var uri = inflight.Request.RequestUri;
						credentialsToUse = sessionHandler.Credentials.GetCredential (uri, authType);
					}

					if (credentialsToUse != null) {
						var credential = new NSUrlCredential (credentialsToUse.UserName, credentialsToUse.Password, NSUrlCredentialPersistence.ForSession);
						completionHandler (NSUrlSessionAuthChallengeDisposition.UseCredential, credential);
					} else {
						// Rejecting the challenge allows the next authentication method in the request to be delivered to
						// the DidReceiveChallenge method. Another authentication method may have credentials available.
						completionHandler (NSUrlSessionAuthChallengeDisposition.RejectProtectionSpace, null);
					}
				} else {
					completionHandler (NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, challenge.ProposedCredential);
				}
			}

			static readonly string RejectProtectionSpaceAuthType = "reject";

			static bool TryGetAuthenticationType (NSUrlProtectionSpace protectionSpace, out string authenticationType)
			{
				if (protectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodNTLM) {
					authenticationType = "NTLM";
				} else if (protectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodHTTPBasic) {
					authenticationType = "basic";
				} else if (protectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodNegotiate ||
					protectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodHTMLForm ||
					protectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodHTTPDigest) {
					// Want to reject this authentication type to allow the next authentication method in the request to
					// be used.
					authenticationType = RejectProtectionSpaceAuthType;
				} else {
					// ServerTrust, ClientCertificate or Default.
					authenticationType = null;
					return false;
				}
				return true;
			}
		}

#if MONOMAC
		// Needed since we strip during linking since we're inside a product assembly.
		[Preserve (AllMembers = true)]
#endif
		class InflightData : IDisposable
		{
			public readonly object Lock = new object ();
			public string RequestUrl { get; set; }

			public TaskCompletionSource<HttpResponseMessage> CompletionSource { get; set; }
			public CancellationToken CancellationToken { get; set; }
			public CancellationTokenSource CancellationTokenSource { get; set; }
			public NSUrlSessionDataTaskStream Stream { get; set; }
			public HttpRequestMessage Request { get; set; }
			public HttpResponseMessage Response { get; set; }

			public bool ResponseSent { get; set; }
			public bool Errored { get; set; }
			public bool Disposed { get; set; }
			public bool Completed { get; set; }
			public bool Done { get { return Errored || Disposed || Completed || CancellationToken.IsCancellationRequested; } }

			public void Dispose()
			{
				Dispose (true);
				GC.SuppressFinalize(this);
			}

			// The bulk of the clean-up code is implemented in Dispose(bool)
			protected virtual void Dispose (bool disposing)
			{
				if (disposing) {
					if (CancellationTokenSource != null) {
						CancellationTokenSource.Dispose ();
						CancellationTokenSource = null;
					}
				}
			}

		}

#if MONOMAC
		// Needed since we strip during linking since we're inside a product assembly.
		[Preserve (AllMembers = true)]
#endif
		class NSUrlSessionDataTaskStreamContent : StreamContent
		{
			Action disposed;

			public NSUrlSessionDataTaskStreamContent (NSUrlSessionDataTaskStream source, Action onDisposed, CancellationToken token) : base (source, token)
			{
				disposed = onDisposed;
			}

			protected override void Dispose (bool disposing)
			{
				var action = Interlocked.Exchange (ref disposed, null);
				action?.Invoke ();

				base.Dispose (disposing);
			}
		}

#if MONOMAC
		// Needed since we strip during linking since we're inside a product assembly.
		[Preserve (AllMembers = true)]
#endif
		class NSUrlSessionDataTaskStream : Stream
		{
			readonly Queue<NSData> data;
			readonly object dataLock = new object ();

			long position;
			long length;

			bool receivedAllData;
			Exception exc;

			NSData current;
			Stream currentStream;

			public NSUrlSessionDataTaskStream ()
			{
				data = new Queue<NSData> ();
			}

			public void Add (NSData d)
			{
				lock (dataLock) {
					data.Enqueue (d);
					length += (int)d.Length;
				}
			}

			public void TrySetReceivedAllData ()
			{
				receivedAllData = true;
			}

			public void TrySetException (Exception e)
			{
				exc = e;
				TrySetReceivedAllData ();
			}

			void ThrowIfNeeded (CancellationToken cancellationToken)
			{
				if (exc != null)
					throw exc;

				cancellationToken.ThrowIfCancellationRequested ();
			}

			public override int Read (byte [] buffer, int offset, int count)
			{
				return ReadAsync (buffer, offset, count).Result;
			}

			public override async Task<int> ReadAsync (byte [] buffer, int offset, int count, CancellationToken cancellationToken)
			{
				// try to throw on enter
				ThrowIfNeeded (cancellationToken);

				while (current == null) {
					lock (dataLock) {
						if (data.Count == 0 && receivedAllData && position == length)
							return 0;

						if (data.Count > 0 && current == null) {
							current = data.Peek ();
							currentStream = current.AsStream ();
							break;
						}
					}

					try {
						await Task.Delay (50, cancellationToken).ConfigureAwait (false);
					} catch (TaskCanceledException ex) {
						// add a nicer exception for the user to catch, add the cancelation exception
						// to have a decent stack
						throw new TimeoutException ("The request timed out.", ex);
					}
				}

				// try to throw again before read
				ThrowIfNeeded (cancellationToken);

				var d = currentStream;
				var bufferCount = Math.Min (count, (int)(d.Length - d.Position));
				var bytesRead = await d.ReadAsync (buffer, offset, bufferCount, cancellationToken).ConfigureAwait (false);

				// add the bytes read from the pointer to the position
				position += bytesRead;

				// remove the current primary reference if the current position has reached the end of the bytes
				if (d.Position == d.Length) {
					lock (dataLock) {
						// this is the same object, it was done to make the cleanup
						data.Dequeue ();
						currentStream?.Dispose ();
						// We cannot use current?.Dispose. The reason is the following one:
						// In the DidReceiveResponse, if iOS realizes that a buffer can be reused,
						// because the data is the same, it will do so. Such a situation does happen
						// between requests, that is, request A and request B will get the same NSData
						// (buffer) in the delegate. In this case, we cannot dispose the NSData because
						// it might be that a different request received it and it is present in
						// its NSUrlSessionDataTaskStream stream. We can only trust the gc to do the job
						// which is better than copying the data over. 
						current = null;
						currentStream = null;
					}
				}

				return bytesRead;
			}

			public override bool CanRead => true;

			public override bool CanSeek => false;

			public override bool CanWrite => false;

			public override bool CanTimeout => false;

			public override long Length => length;

			public override void SetLength (long value)
			{
				throw new InvalidOperationException ();
			}

			public override long Position {
				get { return position; }
				set { throw new InvalidOperationException (); }
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				throw new InvalidOperationException ();
			}

			public override void Flush ()
			{
				throw new InvalidOperationException ();
			}

			public override void Write (byte [] buffer, int offset, int count)
			{
				throw new InvalidOperationException ();
			}
		}

#if MONOMAC
		// Needed since we strip during linking since we're inside a product assembly.
		[Preserve (AllMembers = true)]
#endif
		class WrappedNSInputStream : NSInputStream
		{
			NSStreamStatus status;
			CFRunLoopSource source;
			readonly Stream stream;
			bool notifying;

			public WrappedNSInputStream (Stream inputStream)
			{
				status = NSStreamStatus.NotOpen;
				stream = inputStream;
				source = new CFRunLoopSource (Handle);
			}

			public override NSStreamStatus Status => status;

			public override void Open ()
			{
				status = NSStreamStatus.Open;
				Notify (CFStreamEventType.OpenCompleted);
			}

			public override void Close ()
			{
				status = NSStreamStatus.Closed;
			}

			public override nint Read (IntPtr buffer, nuint len)
			{
				var sourceBytes = new byte [len];
				var read = stream.Read (sourceBytes, 0, (int)len);
				Marshal.Copy (sourceBytes, 0, buffer, (int)len);

				if (notifying)
					return read;

				notifying = true;
				if (stream.CanSeek && stream.Position == stream.Length) {
					Notify (CFStreamEventType.EndEncountered);
					status = NSStreamStatus.AtEnd;
				}
				notifying = false;

				return read;
			}

			public override bool HasBytesAvailable ()
			{
				return true;
			}

			protected override bool GetBuffer (out IntPtr buffer, out nuint len)
			{
				// Just call the base implemention (which will return false)
				return base.GetBuffer (out buffer, out len);
			}

			// NSInvalidArgumentException Reason: *** -propertyForKey: only defined for abstract class.  Define -[System_Net_Http_NSUrlSessionHandler_WrappedNSInputStream propertyForKey:]!
			protected override NSObject GetProperty (NSString key)
			{
				return null;
			}

			protected override bool SetProperty (NSObject property, NSString key)
			{
				return false;
			}

			protected override bool SetCFClientFlags (CFStreamEventType inFlags, IntPtr inCallback, IntPtr inContextPtr)
			{
				// Just call the base implementation, which knows how to handle everything.
				return base.SetCFClientFlags (inFlags, inCallback, inContextPtr);
			}

			public override void Schedule (NSRunLoop aRunLoop, string mode)
			{
				var cfRunLoop = aRunLoop.GetCFRunLoop ();
				var nsMode = new NSString (mode);

				cfRunLoop.AddSource (source, nsMode);

				if (notifying)
					return;

				notifying = true;
				Notify (CFStreamEventType.HasBytesAvailable);
				notifying = false;
			}

			public override void Unschedule (NSRunLoop aRunLoop, string mode)
			{
				var cfRunLoop = aRunLoop.GetCFRunLoop ();
				var nsMode = new NSString (mode);

				cfRunLoop.RemoveSource (source, nsMode);
			}

			protected override void Dispose (bool disposing)
			{
				stream?.Dispose ();
			}
		}
	}
}
