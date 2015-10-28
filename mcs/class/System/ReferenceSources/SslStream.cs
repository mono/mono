//
// Mono-specific additions to Microsoft's SslStream.cs
//
#if MONO_FEATURE_NEW_TLS && SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using Mono.Net.Security;

namespace System.Net.Security
{
	using System.Net.Sockets;
	using System.IO;

	partial class SslStream : IMonoTlsEventSink
	{
		#if SECURITY_DEP
		SSPIConfiguration _Configuration;

		internal SslStream (Stream innerStream, bool leaveInnerStreamOpen, EncryptionPolicy encryptionPolicy, MonoTlsSettings settings)
			: base (innerStream, leaveInnerStreamOpen)
		{
#if MARTIN_FIXME
			if (encryptionPolicy != EncryptionPolicy.RequireEncryption && encryptionPolicy != EncryptionPolicy.AllowNoEncryption && encryptionPolicy != EncryptionPolicy.NoEncryption)
				throw new ArgumentException (SR.GetString (SR.net_invalid_enum, "EncryptionPolicy"), "encryptionPolicy");

			var validationHelper = ChainValidationHelper.CloneWithCallbackWrapper (ref settings, myUserCertValidationCallbackWrapper);

			LocalCertSelectionCallback selectionCallback = null;
			if (validationHelper.HasCertificateSelectionCallback)
				selectionCallback = validationHelper.SelectClientCertificate;

			_Configuration = new MyConfiguration (settings, this);
			_SslState = new SslState (innerStream, null, selectionCallback, encryptionPolicy, _Configuration);
#else
			throw new NotImplementedException ();
#endif
		}

#if MARTIN_FIXME
		/*
		 * Mono-specific version of 'userCertValidationCallbackWrapper'; we're called from ChainValidationHelper.ValidateChain() here.
		 *
		 * Since we're built without the PrebuiltSystem alias, we can't use 'SslPolicyErrors' here.  This prevents us from creating a subclass of 'ChainValidationHelper'
		 * as well as providing a custom 'ServerCertValidationCallback'.
		 */
		bool myUserCertValidationCallbackWrapper (ServerCertValidationCallback callback, X509Certificate certificate, X509Chain chain, MonoSslPolicyErrors sslPolicyErrors)
		{
			m_RemoteCertificateOrBytes = certificate == null ? null : certificate.GetRawCertData ();
			if (callback == null) {
				if (!_SslState.RemoteCertRequired)
					sslPolicyErrors &= ~MonoSslPolicyErrors.RemoteCertificateNotAvailable;

				return (sslPolicyErrors == MonoSslPolicyErrors.None);
			}

			return ChainValidationHelper.InvokeCallback (callback, this, certificate, chain, sslPolicyErrors);
		}
#endif

		class MyConfiguration : SSPIConfiguration
		{
			MonoTlsSettings settings;
			IMonoTlsEventSink eventSink;

			public MyConfiguration (MonoTlsSettings settings, IMonoTlsEventSink eventSink)
			{
				this.settings = settings;
				this.eventSink = eventSink;
			}

			public MonoTlsSettings Settings {
				get { return settings; }
			}

			public IMonoTlsEventSink EventSink {
				get { return eventSink; }
			}
		}
		#endif

		internal bool IsClosed {
			get { return _SslState.IsClosed; }
		}

		internal Exception LastError {
			get { return lastError; }
		}

		#region IMonoTlsEventSink

		Exception lastError;

		void IMonoTlsEventSink.Error (Exception exception)
		{
			Interlocked.CompareExchange<Exception> (ref lastError, exception, null);
		}

		void IMonoTlsEventSink.ReceivedCloseNotify ()
		{
		}

		#endregion

		internal IAsyncResult BeginShutdown (AsyncCallback asyncCallback, object asyncState)
		{
			return _SslState.BeginShutdown (asyncCallback, asyncState);
		}

		internal void EndShutdown (IAsyncResult asyncResult)
		{
			_SslState.EndShutdown (asyncResult);
		}

		internal IAsyncResult BeginRenegotiate (AsyncCallback asyncCallback, object asyncState)
		{
			return _SslState.BeginRenegotiate (asyncCallback, asyncState);
		}

		internal void EndRenegotiate (IAsyncResult asyncResult)
		{
			_SslState.EndRenegotiate (asyncResult);
		}

		internal X509Certificate InternalLocalCertificate {
			get { return _SslState.InternalLocalCertificate; }
		}

		internal MonoTlsConnectionInfo GetMonoConnectionInfo ()
		{
			return _SslState.GetMonoConnectionInfo ();
		}
	}
}
#endif
