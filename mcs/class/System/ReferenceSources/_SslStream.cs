//
// Mono-specific additions to Microsoft's _SslStream.cs
//
#if MONO_FEATURE_NEW_TLS && SECURITY_DEP
namespace System.Net.Security
{
	using System.IO;
	using System.Threading;
	using System.Net.Sockets;

	partial class _SslStream
	{
		static readonly AsyncCallback _HandshakeWriteCallback = new AsyncCallback (HandshakeWriteCallback);
		static readonly HandshakeProtocolCallback _ResumeHandshakeWriteCallback = new HandshakeProtocolCallback (ResumeHandshakeWriteCallback);

		internal void BeginShutdown (LazyAsyncResult lazyResult)
		{
			HandshakeProtocolRequest asyncRequest = new HandshakeProtocolRequest (lazyResult);

			if (Interlocked.Exchange (ref _NestedWrite, 1) == 1)
				throw new NotSupportedException (SR.GetString (SR.net_io_invalidnestedcall, (asyncRequest != null ? "BeginShutdown" : "Shutdown"), "shutdown"));

			bool failed = false;
			try
			{
				ProtocolToken message = _SslState.CreateShutdownMessage ();
				asyncRequest.SetNextRequest (HandshakeProtocolState.Shutdown, message, _ResumeHandshakeWriteCallback);

				StartHandshakeWrite (asyncRequest);
			} catch (Exception e) {
				_SslState.FinishWrite ();
				failed = true;
				throw;
			} finally {
				if (failed)
					_NestedWrite = 0;
			}
		}

		internal void EndShutdown (LazyAsyncResult lazyResult)
		{
			if (Interlocked.Exchange (ref _NestedWrite, 0) == 0)
				throw new InvalidOperationException (SR.GetString (SR.net_io_invalidendcall, "EndShutdown"));

			// No "artificial" timeouts implemented so far, InnerStream controls timeout.
			lazyResult.InternalWaitForCompletion ();

			if (lazyResult.Result is Exception) {
				if (lazyResult.Result is IOException)
					throw (Exception)lazyResult.Result;
				throw new IOException (SR.GetString (SR.mono_net_io_shutdown), (Exception)lazyResult.Result);
			}
		}

		internal void BeginRenegotiate (LazyAsyncResult lazyResult)
		{
			HandshakeProtocolRequest asyncRequest = new HandshakeProtocolRequest (lazyResult);

			if (Interlocked.Exchange (ref _NestedWrite, 1) == 1)
				throw new NotSupportedException (SR.GetString (SR.net_io_invalidnestedcall, (asyncRequest != null ? "BeginRenegotiate" : "Renegotiate"), "renegotiate"));

			bool failed = false;
			try
			{
				if (_SslState.IsServer) {
					ProtocolToken message = _SslState.CreateHelloRequestMessage ();
					asyncRequest.SetNextRequest (HandshakeProtocolState.SendHelloRequest, message, _ResumeHandshakeWriteCallback);
				} else {
					asyncRequest.SetNextRequest (HandshakeProtocolState.ClientRenegotiation, null, _ResumeHandshakeWriteCallback);
				}

				StartHandshakeWrite (asyncRequest);
			} catch (Exception e) {
				_SslState.FinishWrite ();
				failed = true;
				throw;
			} finally {
				if (failed)
					_NestedWrite = 0;
			}
		}

		internal void EndRenegotiate (LazyAsyncResult lazyResult)
		{
			if (Interlocked.Exchange (ref _NestedWrite, 0) == 0)
				throw new InvalidOperationException (SR.GetString (SR.net_io_invalidendcall, "EndRenegotiate"));

			// No "artificial" timeouts implemented so far, InnerStream controls timeout.
			lazyResult.InternalWaitForCompletion();

			if (lazyResult.Result is Exception) {
				if (lazyResult.Result is IOException)
					throw (Exception)lazyResult.Result;
				throw new IOException (SR.GetString (SR.mono_net_io_renegotiate), (Exception)lazyResult.Result);
			}
		}

		void StartHandshakeWrite (HandshakeProtocolRequest asyncRequest)
		{
			byte[] buffer = null;
			if (asyncRequest.Message != null) {
				buffer = asyncRequest.Message.Payload;
				if (buffer.Length != asyncRequest.Message.Size) {
					buffer = new byte [asyncRequest.Message.Size];
					Buffer.BlockCopy (asyncRequest.Message.Payload, 0, buffer, 0, buffer.Length);
				}
			}

			switch (asyncRequest.State) {
			case HandshakeProtocolState.ClientRenegotiation:
			case HandshakeProtocolState.ServerRenegotiation:
				_SslState.StartReHandshake (asyncRequest);
				return;

			case HandshakeProtocolState.SendHelloRequest:
				if (_SslState.CheckEnqueueHandshakeWrite (buffer, asyncRequest)) {
					// operation is async and has been queued, return.
					return;
				}
				break;

			case HandshakeProtocolState.Shutdown:
				if (_SslState.CheckEnqueueWrite (asyncRequest)) {
					// operation is async and has been queued, return.
					return;
				}
				break;

			default:
				throw new InvalidOperationException ();
			}

			if (_SslState.LastPayload != null)
				throw new InvalidOperationException ();

			// prepare for the next request
			IAsyncResult ar = ((NetworkStream)_SslState.InnerStream).BeginWrite (buffer, 0, buffer.Length, _HandshakeWriteCallback, asyncRequest);
			if (!ar.CompletedSynchronously)
				return;

			HandshakeWriteCallback (asyncRequest, ar);
		}

		static void HandshakeWriteCallback (IAsyncResult transportResult)
		{
			if (transportResult.CompletedSynchronously)
				return;

			HandshakeProtocolRequest asyncRequest = (HandshakeProtocolRequest)transportResult.AsyncState;

			SslState sslState = (SslState)asyncRequest.AsyncObject;
			sslState.SecureStream.HandshakeWriteCallback (asyncRequest, transportResult);
		}

		void HandshakeWriteCallback (HandshakeProtocolRequest asyncRequest, IAsyncResult transportResult)
		{
			try {
				_SslState.InnerStream.EndWrite (transportResult);
			} catch (Exception e) {
				_SslState.FinishWrite ();
				if (!asyncRequest.IsUserCompleted) {
					asyncRequest.CompleteWithError (e);
					return;
				}
				throw;
			}

			if (asyncRequest.State == HandshakeProtocolState.SendHelloRequest) {
				asyncRequest.SetNextRequest (HandshakeProtocolState.ServerRenegotiation, null, _ResumeHandshakeWriteCallback);
				StartHandshakeWrite (asyncRequest);
				return;
			}

			try {
				_SslState.FinishWrite ();
				asyncRequest.CompleteUser ();
			} catch (Exception e) {
				if (!asyncRequest.IsUserCompleted) {
					asyncRequest.CompleteWithError (e);
					return;
				}
				throw;
			}
		}

		static void ResumeHandshakeWriteCallback (HandshakeProtocolRequest asyncRequest)
		{
			try {
				((_SslStream)asyncRequest.AsyncObject).StartHandshakeWrite (asyncRequest);
			} catch (Exception e) {
				if (asyncRequest.IsUserCompleted) {
					// This will throw on a worker thread.
					throw;
				}
				((_SslStream)asyncRequest.AsyncObject)._SslState.FinishWrite ();
				asyncRequest.CompleteWithError (e);
			}
		}

		delegate void HandshakeProtocolCallback (HandshakeProtocolRequest asyncRequest);

		enum HandshakeProtocolState {
			None,
			Shutdown,
			SendHelloRequest,
			ServerRenegotiation,
			ClientRenegotiation
		}

		class HandshakeProtocolRequest : AsyncProtocolRequest
		{
			public ProtocolToken Message;
			public HandshakeProtocolState State;

			public HandshakeProtocolRequest (LazyAsyncResult userAsyncResult)
				: base (userAsyncResult)
			{
				State = HandshakeProtocolState.None;
			}

			public void SetNextRequest (HandshakeProtocolState state, ProtocolToken message, HandshakeProtocolCallback callback)
			{
				State = state;
				Message = message;
				SetNextRequest (null, 0, 0, (r) => callback ((HandshakeProtocolRequest)r));
			}
		}
	}
}
#endif
