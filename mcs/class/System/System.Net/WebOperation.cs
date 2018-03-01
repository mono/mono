//
// WebOperation.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2017 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System.IO;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Diagnostics;

namespace System.Net
{
	class WebOperation
	{
		public HttpWebRequest Request {
			get;
		}

		public WebConnection Connection {
			get;
			private set;
		}

		public ServicePoint ServicePoint {
			get;
			private set;
		}

		public BufferOffsetSize WriteBuffer {
			get;
		}

		public bool IsNtlmChallenge {
			get;
		}

#if MONO_WEB_DEBUG
		static int nextID;
		internal readonly int ID = ++nextID;
#else
		internal readonly int ID;
#endif

		public WebOperation (HttpWebRequest request, BufferOffsetSize writeBuffer, bool isNtlmChallenge, CancellationToken cancellationToken)
		{
			Request = request;
			WriteBuffer = writeBuffer;
			IsNtlmChallenge = isNtlmChallenge;
			cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
			requestTask = new WebCompletionSource<WebRequestStream> ();
			requestWrittenTask = new WebCompletionSource<WebRequestStream> ();
			completeResponseReadTask = new WebCompletionSource<bool> ();
			responseTask = new WebCompletionSource<WebResponseStream> ();
			finishedTask = new WebCompletionSource<(bool, WebOperation)> (); 
		}

		CancellationTokenSource cts;
		WebCompletionSource<WebRequestStream> requestTask;
		WebCompletionSource<WebRequestStream> requestWrittenTask;
		WebCompletionSource<WebResponseStream> responseTask;
		WebCompletionSource<bool> completeResponseReadTask;
		WebCompletionSource<(bool, WebOperation)> finishedTask;
		WebRequestStream writeStream;
		WebResponseStream responseStream;
		ExceptionDispatchInfo disposedInfo;
		ExceptionDispatchInfo closedInfo;
		WebOperation priorityRequest;
		volatile bool finishedReading;
		int requestSent;

		public bool Aborted {
			get {
				if (disposedInfo != null || Request.Aborted)
					return true;
				if (cts != null && cts.IsCancellationRequested)
					return true;
				return false;
			}
		}

		public bool Closed {
			get {
				return Aborted || closedInfo != null;
			}
		}

		public void Abort ()
		{
			var (exception, disposed) = SetDisposed (ref disposedInfo);
			if (!disposed)
				return;
			cts?.Cancel ();
			SetCanceled ();
			Close ();
		}

		public void Close ()
		{
			var (exception, closed) = SetDisposed (ref closedInfo);
			if (!closed)
				return;

			var stream = Interlocked.Exchange (ref writeStream, null);
			if (stream != null) {
				try {
					stream.Close ();
				} catch { }
			}
		}

		void SetCanceled ()
		{
			requestTask.TrySetCanceled ();
			requestWrittenTask.TrySetCanceled ();
			responseTask.TrySetCanceled ();
			completeResponseReadTask.TrySetCanceled ();
		}

		void SetError (Exception error)
		{
			requestTask.TrySetException (error);
			requestWrittenTask.TrySetException (error);
			responseTask.TrySetException (error);
			completeResponseReadTask.TrySetException (error);
		}

		(ExceptionDispatchInfo, bool) SetDisposed (ref ExceptionDispatchInfo field)
		{
			var wexc = new WebException (SR.GetString (SR.net_webstatus_RequestCanceled), WebExceptionStatus.RequestCanceled);
			var exception = ExceptionDispatchInfo.Capture (wexc);
			var old = Interlocked.CompareExchange (ref field, exception, null);
			return (old ?? exception, old == null);
		}

		internal ExceptionDispatchInfo CheckDisposed (CancellationToken cancellationToken)
		{
			if (Aborted || cancellationToken.IsCancellationRequested)
				return CheckThrowDisposed (false, ref disposedInfo);
			return null;
		}

		internal void ThrowIfDisposed ()
		{
			ThrowIfDisposed (CancellationToken.None);
		}

		internal void ThrowIfDisposed (CancellationToken cancellationToken)
		{
			if (Aborted || cancellationToken.IsCancellationRequested)
				CheckThrowDisposed (true, ref disposedInfo);
		}

		internal void ThrowIfClosedOrDisposed ()
		{
			ThrowIfClosedOrDisposed (CancellationToken.None);
		}

		internal void ThrowIfClosedOrDisposed (CancellationToken cancellationToken)
		{
			if (Closed || cancellationToken.IsCancellationRequested)
				CheckThrowDisposed (true, ref closedInfo);
		}

		ExceptionDispatchInfo CheckThrowDisposed (bool throwIt, ref ExceptionDispatchInfo field)
		{
			var (exception, disposed) = SetDisposed (ref field);
			if (disposed)
				cts?.Cancel ();
			if (throwIt)
				exception.Throw ();
			return exception;
		}

		internal void RegisterRequest (ServicePoint servicePoint, WebConnection connection)
		{
			if (servicePoint == null)
				throw new ArgumentNullException (nameof (servicePoint));
			if (connection == null)
				throw new ArgumentNullException (nameof (connection));

			lock (this) {
				if (Interlocked.CompareExchange (ref requestSent, 1, 0) != 0)
					throw new InvalidOperationException ("Invalid nested call.");
				ServicePoint = servicePoint;
				Connection = connection;
			}

			cts.Token.Register (() => {
				Request.FinishedReading = true;
				SetDisposed (ref disposedInfo);
			});
		}

		public void SetPriorityRequest (WebOperation operation)
		{
			lock (this) {
				if (requestSent != 1 || ServicePoint == null || finishedReading)
					throw new InvalidOperationException ("Should never happen.");
				if (Interlocked.CompareExchange (ref priorityRequest, operation, null) != null)
					throw new InvalidOperationException ("Invalid nested request.");
			}
		}

		public async Task<Stream> GetRequestStream ()
		{
			return await requestTask.WaitForCompletion ().ConfigureAwait (false);
		}

		internal Task<WebRequestStream> GetRequestStreamInternal ()
		{
			return requestTask.WaitForCompletion ();
		}

		public Task WaitUntilRequestWritten ()
		{
			return requestWrittenTask.WaitForCompletion ();
		}

		public WebRequestStream WriteStream {
			get {
				ThrowIfDisposed ();
				return writeStream;
			}
		}

		public Task<WebResponseStream> GetResponseStream ()
		{
			return responseTask.WaitForCompletion ();
		}

		internal async Task<(bool, WebOperation)> WaitForCompletion (bool ignoreErrors)
		{
			try {
				return await finishedTask.WaitForCompletion ().ConfigureAwait (false);
			} catch {
				if (ignoreErrors)
					return (false, null);
				throw;
			}
		}

		internal async void Run ()
		{
			try {
				FinishReading ();

				ThrowIfClosedOrDisposed ();

				var requestStream = await Connection.InitConnection (this, cts.Token).ConfigureAwait (false);

				ThrowIfClosedOrDisposed ();

				writeStream = requestStream;

				await requestStream.Initialize (cts.Token).ConfigureAwait (false);

				ThrowIfClosedOrDisposed ();

				requestTask.TrySetCompleted (requestStream);

				var stream = new WebResponseStream (requestStream);
				responseStream = stream;

				await stream.InitReadAsync (cts.Token).ConfigureAwait (false);

				responseTask.TrySetCompleted (stream);
			} catch (OperationCanceledException) {
				Console.Error.WriteLine ($"WO SET CANCELED!");
				SetCanceled ();
			} catch (Exception e) {
				Console.Error.WriteLine ($"WO SET ERROR!");
				SetError (e);
			}
		}

		async void FinishReading ()
		{
			bool ok = false;
			Exception error = null;

			try {
				ok = await completeResponseReadTask.WaitForCompletion ().ConfigureAwait (false);
			} catch (Exception e) {
				error = e;
			}

			WebResponseStream stream;
			WebOperation next;

			lock (this) {
				finishedReading = true;
				stream = Interlocked.Exchange (ref responseStream, null);
				next = Interlocked.Exchange (ref priorityRequest, null);
				Request.FinishedReading = true;
			}

			if (error != null) {
				if (next != null)
					next.SetError (error);
				finishedTask.TrySetException (error);
				return;
			}

			WebConnection.Debug ($"WO FINISH READING: Cnc={Connection?.ID} Op={ID} ok={ok} error={error != null} stream={stream != null} next={next != null}");

			var keepAlive = !Aborted && ok && (stream?.KeepAlive ?? false);
			if (next != null && next.Aborted) {
				next = null;
				keepAlive = false;
			}

			finishedTask.TrySetCompleted ((keepAlive, next));

			WebConnection.Debug ($"WO FINISH READING DONE: Cnc={Connection.ID} Op={ID} - {keepAlive} next={next?.ID}");
		}

		internal void CompleteRequestWritten (WebRequestStream stream, Exception error = null)
		{
			WebConnection.Debug ($"WO COMPLETE REQUEST WRITTEN: Op={ID} {error != null}");

			if (error != null)
				SetError (error);
			else
				requestWrittenTask.TrySetCompleted (stream);
		}

		internal void CompleteResponseRead (bool ok, Exception error = null)
		{
			WebConnection.Debug ($"WO COMPLETE RESPONSE READ: Op={ID} {ok} {error?.GetType ()}");

			if (error != null)
				completeResponseReadTask.TrySetException (error);
			else
				completeResponseReadTask.TrySetCompleted (ok);
		}
	}
}
