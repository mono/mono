

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebAssembly.Core;
using WebAssembly.Host;

namespace WebAssembly.Net.WebSockets {

	/// <summary>
	/// Provides a client for connecting to WebSocket services.
	/// </summary>
	public sealed class ClientWebSocket : WebSocket {

		private ActionQueue<ReceivePayload> receiveMessageQueue = new ActionQueue<ReceivePayload> ();

		private TaskCompletionSource<bool> tcsClose;
		private WebSocketCloseStatus? innerWebSocketCloseStatus;
		private string innerWebSocketCloseStatusDescription;

		private JSObject innerWebSocket;

		private Action<JSObject> onOpen;
		private Action<JSObject> onError;
		private Action<JSObject> onClose;
		private Action<JSObject> onMessage;


		private readonly ClientWebSocketOptions options;
		private readonly CancellationTokenSource cts;

		// Stages of this class. 
		private int state;
		private const int created = 0;
		private const int connecting = 1;
		private const int connected = 2;
		private const int disposed = 3;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WebAssembly.Net.WebSockets.ClientWebSocket"/> class.
		/// </summary>
		public ClientWebSocket ()
		{
			state = created;
			options = new ClientWebSocketOptions ();
			cts = new CancellationTokenSource ();


		}

		#region Properties

		public ClientWebSocketOptions Options => options;

		/// <summary>
		/// Gets the WebSocket state of the <see cref="T:WebAssembly.Net.WebSockets.ClientWebSocket"/> instance.
		/// </summary>
		/// <value>The state.</value>
		public override WebSocketState State {
			get {

				if (innerWebSocket != null && !innerWebSocket.IsDisposed) {
					return ReadyStateToDotNetState ((int)innerWebSocket.GetObjectProperty ("readyState"));
				}
				switch (state) {
				case created:
					return WebSocketState.None;
				case connecting:
					return WebSocketState.Connecting;
				case disposed: // We only get here if disposed before connecting
					return WebSocketState.Closed;
				default:
					return WebSocketState.Closed;
				}
			}
		}


		private WebSocketState ReadyStateToDotNetState (int readyState)
		{
			// https://developer.mozilla.org/en-US/docs/Web/API/WebSocket/readyState
			switch (readyState) {
			case 0: // 0 (CONNECTING)
				return WebSocketState.Connecting;
			case 1: // 1 (OPEN)
				return WebSocketState.Open;
			case 2: // 2 (CLOSING)
				return WebSocketState.CloseSent;
			case 3: // 3 (CLOSED)
				return WebSocketState.Closed;
			default:
				return WebSocketState.None;


			}
		}

		/// <summary>
		/// Gets the reason why the close handshake was initiated on <see cref="T:WebAssembly.Net.WebSockets.ClientWebSocket"/> instance.
		/// </summary>
		/// <value>The close status.</value>
		public override WebSocketCloseStatus? CloseStatus {
			get {
				if (innerWebSocket != null) {
					return innerWebSocketCloseStatus;
				}
				return null;
			}
		}

		/// <summary>
		/// Gets a description of the reason why the <see cref="T:WebAssembly.Net.WebSockets.ClientWebSocket"/> instance was closed.
		/// </summary>
		/// <value>The close status description.</value>
		public override string CloseStatusDescription {
			get {
				if (innerWebSocket != null) {
					return innerWebSocketCloseStatusDescription;
				}
				return null;
			}
		}

		/// <summary>
		/// Gets the supported WebSocket sub-protocol for the <see cref="T:WebAssembly.Net.WebSockets.ClientWebSocket"/>s instance.
		/// </summary>
		/// <value>The sub protocol.</value>
		public override string SubProtocol {
			get {
				if (innerWebSocket != null && !innerWebSocket.IsDisposed) {
					return innerWebSocket.GetObjectProperty ("protocol")?.ToString ();
				}
				return null;
			}
		}


		#endregion Properties

		/// <summary>
		/// Connect to a WebSocket server as an asynchronous operation.
		/// </summary>
		/// <returns>The async.</returns>
		/// <param name="uri">URI.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public Task ConnectAsync (Uri uri, CancellationToken cancellationToken)
		{
			if (uri == null) {
				throw new ArgumentNullException (nameof (uri));
			}
			if (!uri.IsAbsoluteUri) {
				throw new ArgumentException ("Uri is not absolute", nameof (uri));
			}

			// Check that we have not started already
			int priorState = Interlocked.CompareExchange (ref state, connecting, created);
			if (priorState == disposed) {
				throw new ObjectDisposedException (GetType ().FullName);
			} else if (priorState != created) {
				throw new InvalidOperationException ("WebSocket already started");
			}

			options.SetToReadOnly ();

			return ConnectAsyncJavaScript (uri, cancellationToken);
		}

		private async Task ConnectAsyncJavaScript (Uri uri, CancellationToken cancellationToken)
		{
			var tcsConnect = new TaskCompletionSource<bool> ();

			// For Abort/Dispose.  Calling Abort on the request at any point will close the connection.
			cts.Token.Register (AbortRequest);

			// Wrap the cancellationToken in a using so that it can be disposed of whether
			// we successfully connected or failed trying.
			// Otherwise any timeout/cancellation would apply to the full session.
			// In the failure case we need to release the references and dispose of the objects.
			using (cancellationToken.Register (() => tcsConnect.TrySetCanceled ())) {
				try {
					Core.Array subProtocols = null;
					if (Options.RequestedSubProtocols.Count > 0) {
						subProtocols = new Core.Array();
						foreach (var item in Options.RequestedSubProtocols) {
							subProtocols.Push(item);
						}
					}
					innerWebSocket = new HostObject ("WebSocket", uri.ToString (), subProtocols);

					subProtocols?.Dispose ();

					// Setup the onError callback
					onError = new Action<JSObject> ((errorEvt) => {

						errorEvt.Dispose ();
					});

					// Attach the onError callback
					innerWebSocket.SetObjectProperty ("onerror", onError);

					// Setup the onClose callback
					onClose = new Action<JSObject> ((closeEvt) => {
						innerWebSocketCloseStatus = (WebSocketCloseStatus)closeEvt.GetObjectProperty ("code");
						innerWebSocketCloseStatusDescription = closeEvt.GetObjectProperty ("reason")?.ToString ();
						var mess = new ReceivePayload (WebSocketHelpers.EmptyPayload, WebSocketMessageType.Close);
						receiveMessageQueue.BufferPayload (mess);

						if (!tcsConnect.Task.IsCanceled && !tcsConnect.Task.IsCompleted && !tcsConnect.Task.IsFaulted) {
							tcsConnect.SetException (new WebSocketException (WebSocketError.NativeError));
						} else {
							tcsClose?.SetResult (true);
						}

						closeEvt.Dispose ();
					});

					// Attach the onClose callback
					innerWebSocket.SetObjectProperty ("onclose", onClose);

					// Setup the onOpen callback
					onOpen = new Action<JSObject> ((evt) => {
						if (!cancellationToken.IsCancellationRequested) {
							// Change internal state to 'connected' to enable the other methods
							if (Interlocked.CompareExchange (ref state, connected, connecting) != connecting) {
								// Aborted/Disposed during connect.
								throw new ObjectDisposedException (GetType ().FullName);
							}

							tcsConnect.SetResult (true);
						}

						evt.Dispose ();
					});

					// Attach the onOpen callback
					innerWebSocket.SetObjectProperty ("onopen", onOpen);

					// Setup the onMessage callback
					onMessage = new Action<JSObject> ((messageEvent) => {
						ThrowIfNotConnected ();

						// get the events "data"
						var eventData = messageEvent.GetObjectProperty ("data");

						// If the messageEvent's data property is marshalled as a JSObject then we are dealing with 
						// binary data
						if (eventData is JSObject) {
							// TODO: Handle ArrayBuffer binary type but have only seen 'blob' so far without
							// changing the default websocket binary type manually.
							if (innerWebSocket.GetObjectProperty ("binaryType").ToString () == "blob") {

								Action<JSObject> loadend = null;
								// Create a new "FileReader" object
								using (var reader = new HostObject("FileReader")) {
									loadend = new Action<JSObject> ((loadEvent) => {
										using (var target = (JSObject)loadEvent.GetObjectProperty ("target")) {
											if ((int)target.GetObjectProperty ("readyState") == 2) {
												using (var binResult = (ArrayBuffer)target.GetObjectProperty ("result")) {
													var mess = new ReceivePayload (binResult, WebSocketMessageType.Binary);
													receiveMessageQueue.BufferPayload (mess);
													Runtime.FreeObject (loadend);
												}
											}
										}
										loadEvent.Dispose ();

									});

									reader.Invoke ("addEventListener", "loadend", loadend);

									using (var blobData = (JSObject)messageEvent.GetObjectProperty ("data"))
										reader.Invoke ("readAsArrayBuffer", blobData);
								}
							} else
								throw new NotImplementedException ($"WebSocket bynary type '{innerWebSocket.GetObjectProperty ("binaryType").ToString ()}' not supported.");
						} else if (eventData is string) {

							var mess = new ReceivePayload (Encoding.UTF8.GetBytes (((string)eventData).ToString ()), WebSocketMessageType.Text);
							receiveMessageQueue.BufferPayload (mess);
						}
						messageEvent.Dispose ();

					});

					// Attach the onMessage callaback
					innerWebSocket.SetObjectProperty ("onmessage", onMessage);

					await tcsConnect.Task;

				} catch (Exception wse) {
					ConnectExceptionCleanup ();
					WebSocketException wex = new WebSocketException ("WebSocket connection failure.", wse);
					throw wex;
				}

			}

		}

		private void ConnectExceptionCleanup ()
		{
			Dispose ();

		}


		public override void Dispose ()
		{
			int priorState = Interlocked.Exchange (ref state, disposed);
			if (priorState == disposed) {
				// No cleanup required.
				return;
			}

			// registered by the CancellationTokenSource cts in the connect method
			cts.Cancel (false);
			cts.Dispose ();


			// We need to clear the events on websocket as well or stray events
			// are possible leading to crashes.
			if (onClose != null) {
				innerWebSocket.SetObjectProperty ("onclose", "");
				Runtime.FreeObject (onClose);
			}
			if (onError != null) {
				innerWebSocket.SetObjectProperty ("onerror", "");
				Runtime.FreeObject (onError);
			}
			if (onOpen != null) {
				innerWebSocket.SetObjectProperty ("onopen", "");
				Runtime.FreeObject (onOpen);
			}
			if (onMessage != null) {
				innerWebSocket.SetObjectProperty ("onmessage", "");
				Runtime.FreeObject (onMessage);
			}
			innerWebSocket?.Dispose ();
		}

		// This method is registered by the CancellationTokenSource cts in the connect method
		// and called by Dispose or Abort so that any open websocket connection can be closed.
		private void AbortRequest ()
		{
			if (State == WebSocketState.Open) {
				var closeResult = CloseAsync (WebSocketCloseStatus.NormalClosure, "Connection was aborted", CancellationToken.None);
			}

		}

		/// <summary>
		/// Send data on <see cref="T:WebAssembly.Net.WebSockets.ClientWebSocket"/> as an asynchronous operation.
		/// </summary>
		/// <returns>The async.</returns>
		/// <param name="buffer">Buffer.</param>
		/// <param name="messageType">Message type.</param>
		/// <param name="endOfMessage">If set to <c>true</c> end of message.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public override async Task SendAsync (ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
		{
			// TODO: Support send async buffering.

			ThrowIfNotConnected ();

			if (messageType != WebSocketMessageType.Binary &&
				messageType != WebSocketMessageType.Text) {
				throw new ArgumentException ($"Invalid message type: '{messageType}' specified in method 'SendAsync'.  Valid types are 'Binary' and 'Text'",
				    nameof (messageType));
			}

			var tcsSend = new TaskCompletionSource<bool> ();
			// Wrap the cancellationToken in a using so that it can be disposed of whether
			// we successfully send or not.
			// Otherwise any timeout/cancellation would apply to the full session.
			using (cancellationToken.Register (() => tcsSend.TrySetCanceled ())) {

				if (messageType == WebSocketMessageType.Binary) {

					try {
						using (var uint8Buffer = Uint8Array.From(buffer))
						{
							innerWebSocket.Invoke ("send", uint8Buffer);
							tcsSend.SetResult (true);
						}
					} catch (Exception excb) {
						throw new WebSocketException (WebSocketError.NativeError, excb);
					}
					await tcsSend.Task;

				}
				if (messageType == WebSocketMessageType.Text) {

					try {
						var bytesToSend = new byte [buffer.Count];
						Buffer.BlockCopy (buffer.Array, buffer.Offset, bytesToSend, 0, buffer.Count);

						var strBuffer = Encoding.UTF8.GetString (bytesToSend, 0, bytesToSend.Length);
						innerWebSocket.Invoke ("send", strBuffer);
						tcsSend.SetResult (true);
					} catch (Exception exct) {
						throw new WebSocketException (WebSocketError.NativeError, exct);
					}

					await tcsSend.Task;

				}

			}
		}

		private ReceivePayload bufferedPayload;

		/// <summary>
		/// Receives data on <see cref="T:WebAssembly.Net.WebSockets.ClientWebSocket"/> as an asynchronous operation.
		/// </summary>
		/// <returns>The async.</returns>
		/// <param name="buffer">Buffer.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public override async Task<WebSocketReceiveResult> ReceiveAsync (ArraySegment<byte> buffer, CancellationToken cancellationToken)
		{
			ThrowIfDisposed ();
			ThrowOnInvalidState (State, WebSocketState.Open, WebSocketState.CloseSent);

			var tcsReceive = new TaskCompletionSource<WebSocketReceiveResult> ();

			// Wrap the cancellationToken in a using so that it can be disposed of whether
			// we successfully receive or not.
			// Otherwise any timeout/cancellation would apply to the full session.
			using (cancellationToken.Register (() => tcsReceive.TrySetCanceled ())) {

				if (bufferedPayload == null)
					bufferedPayload = await receiveMessageQueue.DequeuePayloadAsync (cancellationToken);

				try {
					var endOfMessage = bufferedPayload.BufferPayload (buffer, out WebSocketReceiveResult receiveResult);

					tcsReceive.SetResult (receiveResult);

					if (endOfMessage)
						bufferedPayload = null;
				} catch (Exception exc) {
					throw new WebSocketException (WebSocketError.NativeError, exc);
				}

				return await tcsReceive.Task;
			}
		}

		/// <summary>
		/// Aborts the connection and cancels any pending IO operations.
		/// </summary>
		public override void Abort ()
		{
			if (state == disposed) {
				return;
			}
			state = (int)WebSocketState.Aborted;
			Dispose ();
		}

		public override async Task CloseAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			ThrowIfNotConnected ();
			ThrowOnInvalidState (State,
			    WebSocketState.Open, WebSocketState.CloseReceived, WebSocketState.CloseSent);

			WebSocketHelpers.ValidateCloseStatus (closeStatus, statusDescription);

			tcsClose = new TaskCompletionSource<bool> ();
			// Wrap the cancellationToken in a using so that it can be disposed of whether
			// we successfully connected or failed trying.
			// Otherwise any timeout/cancellation would apply to the full session.
			// In the failure case we need to release the references and dispose of the objects.
			using (cancellationToken.Register (() => tcsClose.TrySetCanceled ())) {

				innerWebSocketCloseStatus = closeStatus;
				innerWebSocketCloseStatusDescription = statusDescription;

				try {
					innerWebSocket.Invoke ("close", (int)closeStatus, statusDescription);
				} catch (Exception exc) {
					throw exc;
				}

				await tcsClose.Task;
			}
		}

		public override Task CloseOutputAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) => throw new NotImplementedException ();

		private void ThrowIfNotConnected ()
		{
			if (state == disposed) {
				throw new ObjectDisposedException (GetType ().FullName);
			} else if (State != WebSocketState.Open) {
				throw new InvalidOperationException ("WebSocket is not connected");
			}
		}

		private void ThrowIfDisposed ()
		{
			if (state == disposed) {
				throw new ObjectDisposedException (GetType ().FullName);
			}
		}

		private class ActionQueue<T> {

			private readonly SemaphoreSlim actionSem;
			private readonly ConcurrentQueue<T> actionQueue;

			public ActionQueue ()
			{
				actionSem = new SemaphoreSlim (0);
				actionQueue = new ConcurrentQueue<T> ();
			}

			public void BufferPayload (T item)
			{
				actionQueue.Enqueue (item);
				actionSem.Release ();
			}

			public async Task<T> DequeuePayloadAsync (CancellationToken cancellationToken = default (CancellationToken))
			{
				while (true) {
					await actionSem.WaitAsync (cancellationToken);

					T item;
					if (actionQueue.TryDequeue (out item)) {
						return item;
					}
				}
			}
		}

		public sealed class ClientWebSocketOptions {
			private bool isReadOnly; // After ConnectAsync is called the options cannot be modified.
			private readonly IList<string> requestedSubProtocols;

			internal ClientWebSocketOptions ()
			{
				requestedSubProtocols = new List<string> ();
			}

			#region HTTP Settings

			// Note that some headers are restricted like Host.
			public void SetRequestHeader (string headerName, string headerValue)
			{
				throw new PlatformNotSupportedException ();
			}

			public bool UseDefaultCredentials {
				get => throw new PlatformNotSupportedException ();
				set => throw new PlatformNotSupportedException ();
			}

			public System.Net.ICredentials Credentials {
				get => throw new PlatformNotSupportedException ();
				set => throw new PlatformNotSupportedException ();
			}

			public System.Net.IWebProxy Proxy {
				get => throw new PlatformNotSupportedException ();
				set => throw new PlatformNotSupportedException ();
			}

			public X509CertificateCollection ClientCertificates {
				get => throw new PlatformNotSupportedException ();
				set => throw new PlatformNotSupportedException ();
			}

			public System.Net.Security.RemoteCertificateValidationCallback RemoteCertificateValidationCallback {
				get => throw new PlatformNotSupportedException ();
				set => throw new PlatformNotSupportedException ();
			}

			public System.Net.CookieContainer Cookies {
				get => throw new PlatformNotSupportedException ();
				set => throw new PlatformNotSupportedException ();
			}

			#endregion HTTP Settings

			#region WebSocket Settings

			public void AddSubProtocol (string subProtocol)
			{
				ThrowIfReadOnly ();

				// Duplicates not allowed.
				foreach (string item in requestedSubProtocols) {
					if (string.Equals (item, subProtocol, StringComparison.OrdinalIgnoreCase)) {
						throw new ArgumentException ($"Duplicate protocal '{subProtocol}' not allowed", nameof (subProtocol));
					}
				}
				requestedSubProtocols.Add (subProtocol);
			}

			internal IList<string> RequestedSubProtocols { get { return requestedSubProtocols; } }

			public TimeSpan KeepAliveInterval {
				get => throw new PlatformNotSupportedException ();
				set => throw new PlatformNotSupportedException ();
			}

			public void SetBuffer (int receiveBufferSize, int sendBufferSize)
			{
				throw new NotImplementedException ();
			}

			public void SetBuffer (int receiveBufferSize, int sendBufferSize, ArraySegment<byte> buffer)
			{
				throw new PlatformNotSupportedException ();
			}

			#endregion WebSocket settings

			#region Helpers

			internal void SetToReadOnly ()
			{
				isReadOnly = true;
			}

			private void ThrowIfReadOnly ()
			{
				if (isReadOnly) {
					throw new InvalidOperationException ("WebSocket has already been started.");
				}
			}

			#endregion Helpers
		}

	}

}
