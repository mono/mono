//
// Mono-specific additions to Microsoft's _SslState.cs
//
#if MONO_FEATURE_NEW_TLS && SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif
namespace System.Net.Security
{
	using System.IO;
	using System.Threading;
	using System.Net.Sockets;

	partial class SslState
	{
		int _SentShutdown;

		internal MonoTlsConnectionInfo GetMonoConnectionInfo ()
		{
			return Context.GetMonoConnectionInfo ();
		}

		internal bool IsClosed {
			get { return Context.IsClosed; }
		}

		internal ProtocolToken CreateShutdownMessage ()
		{
			return Context.CreateShutdownMessage ();
		}

		internal ProtocolToken CreateHelloRequestMessage ()
		{
			return Context.CreateHelloRequestMessage ();
		}

		internal IAsyncResult BeginShutdown (AsyncCallback asyncCallback, object asyncState)
		{
			var shutdownResult = new ShutdownAsyncResult (this, asyncState, asyncCallback);

			if (Interlocked.CompareExchange (ref _SentShutdown, 1, 0) == 1) {
				shutdownResult.InvokeCallback ();
				return shutdownResult;
			}

			try
			{
				CheckThrow (false);
				shutdownResult.SentShutdown = true;
				SecureStream.BeginShutdown (shutdownResult);
				return shutdownResult;
			} catch (Exception e) {
				if (e is IOException)
					throw;
				throw new IOException (SR.GetString (SR.mono_net_io_shutdown), e);
			}
		}

		internal void EndShutdown (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			var shutdownResult = asyncResult as ShutdownAsyncResult;
			if (shutdownResult == null)
				throw new ArgumentException (SR.GetString (SR.net_io_async_result, asyncResult.GetType ().FullName), "asyncResult");

			if (shutdownResult.SentShutdown)
				SecureStream.EndShutdown (shutdownResult);
		}

		internal IAsyncResult BeginRenegotiate (AsyncCallback asyncCallback, object asyncState)
		{
			var lazyResult = new LazyAsyncResult (this, asyncState, asyncCallback);

			if (Interlocked.Exchange (ref _NestedAuth, 1) == 1)
				throw new InvalidOperationException (SR.GetString (SR.net_io_invalidnestedcall, "BeginRenegotiate", "renegotiate"));
			if (Interlocked.CompareExchange (ref _PendingReHandshake, 1, 0) == 1)
				throw new InvalidOperationException (SR.GetString (SR.net_io_invalidnestedcall, "BeginRenegotiate", "renegotiate"));

			try {
				CheckThrow (false);
				SecureStream.BeginRenegotiate (lazyResult);
				return lazyResult;
			} catch (Exception e) {
				_NestedAuth = 0;
				if (e is IOException)
					throw;
				throw new IOException (SR.GetString (SR.mono_net_io_renegotiate), e);
			}
		}

		internal void EndRenegotiate (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("asyncResult");

			LazyAsyncResult lazyResult = result as LazyAsyncResult;
			if (lazyResult == null)
				throw new ArgumentException (SR.GetString (SR.net_io_async_result, result.GetType ().FullName), "asyncResult");

			if (Interlocked.Exchange (ref _NestedAuth, 0) == 0)
				throw new InvalidOperationException (SR.GetString (SR.net_io_invalidendcall, "EndRenegotiate"));

			SecureStream.EndRenegotiate (lazyResult);
		}

		internal bool CheckEnqueueHandshakeWrite (byte[] buffer, AsyncProtocolRequest asyncRequest)
		{
			return CheckEnqueueHandshake (buffer, asyncRequest);
		}

		internal void StartReHandshake (AsyncProtocolRequest asyncRequest)
		{
			if (IsServer) {
				byte[] buffer = null;
				if (CheckEnqueueHandshakeRead (ref buffer, asyncRequest))
					return;

				StartReceiveBlob (buffer, asyncRequest);
				return;
			}

			ForceAuthentication (false, null, asyncRequest);
		}

		class ShutdownAsyncResult : LazyAsyncResult
		{
			public bool SentShutdown;

			internal ShutdownAsyncResult (SslState instance, object userState, AsyncCallback callback)
				: base (instance, userState, callback)
			{
			}
		}
	}
}
#endif
