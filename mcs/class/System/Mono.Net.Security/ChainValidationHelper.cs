//
// System.Net.ServicePointManager
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2003-2010 Novell, Inc (http://www.novell.com)
//

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

#if SECURITY_DEP

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
using MSX = MonoSecurity::Mono.Security.X509;
using MonoSecurity::Mono.Security.X509.Extensions;
#else
using Mono.Security.Interface;
using MSX = Mono.Security.X509;
using Mono.Security.X509.Extensions;
#endif

using System;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Configuration;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;

using System.Globalization;
using System.Net.Security;
using System.Diagnostics;

namespace Mono.Net.Security
{
	internal delegate bool ServerCertValidationCallbackWrapper (ServerCertValidationCallback callback, X509Certificate certificate, X509Chain chain, MonoSslPolicyErrors sslPolicyErrors);

	internal class ChainValidationHelper : ICertificateValidator2
	{
		readonly object sender;
		readonly MonoTlsSettings settings;
		readonly MonoTlsProvider provider;
		readonly ServerCertValidationCallback certValidationCallback;
		readonly LocalCertSelectionCallback certSelectionCallback;
		readonly ServerCertValidationCallbackWrapper callbackWrapper;
		readonly MonoTlsStream tlsStream;
		readonly HttpWebRequest request;

		internal static ICertificateValidator GetInternalValidator (MonoTlsProvider provider, MonoTlsSettings settings)
		{
			if (settings == null)
				return new ChainValidationHelper (provider, null, false, null, null);
			if (settings.CertificateValidator != null)
				return settings.CertificateValidator;
			return new ChainValidationHelper (provider, settings, false, null, null);
		}

		internal static ICertificateValidator GetDefaultValidator (MonoTlsSettings settings)
		{
			var provider = MonoTlsProviderFactory.GetProvider ();
			if (settings == null)
				return new ChainValidationHelper (provider, null, false, null, null);
			if (settings.CertificateValidator != null)
				throw new NotSupportedException ();
			return new ChainValidationHelper (provider, settings, false, null, null);
		}

#region SslStream support

		/*
		 * This is a hack which is used in SslStream - see ReferenceSources/SslStream.cs for details.
		 */
		internal static ChainValidationHelper CloneWithCallbackWrapper (MonoTlsProvider provider, ref MonoTlsSettings settings, ServerCertValidationCallbackWrapper wrapper)
		{
			var helper = (ChainValidationHelper)settings.CertificateValidator;
			if (helper == null)
				helper = new ChainValidationHelper (provider, settings, true, null, wrapper);
			else
				helper = new ChainValidationHelper (helper, provider, settings, wrapper);
			settings = helper.settings;
			return helper;
		}

		internal static bool InvokeCallback (ServerCertValidationCallback callback, object sender, X509Certificate certificate, X509Chain chain, MonoSslPolicyErrors sslPolicyErrors)
		{
			return callback.Invoke (sender, certificate, chain, (SslPolicyErrors)sslPolicyErrors);
		}

#endregion

		ChainValidationHelper (ChainValidationHelper other, MonoTlsProvider provider, MonoTlsSettings settings, ServerCertValidationCallbackWrapper callbackWrapper = null)
		{
			sender = other.sender;
			certValidationCallback = other.certValidationCallback;
			certSelectionCallback = other.certSelectionCallback;
			tlsStream = other.tlsStream;
			request = other.request;

			if (settings == null)
				settings = MonoTlsSettings.DefaultSettings;

			this.provider = provider;
			this.settings = settings.CloneWithValidator (this);
			this.callbackWrapper = callbackWrapper;
		}

		internal static ChainValidationHelper Create (MonoTlsProvider provider, ref MonoTlsSettings settings, MonoTlsStream stream)
		{
			var helper = new ChainValidationHelper (provider, settings, true, stream, null);
			settings = helper.settings;
			return helper;
		}

		ChainValidationHelper (MonoTlsProvider provider, MonoTlsSettings settings, bool cloneSettings, MonoTlsStream stream, ServerCertValidationCallbackWrapper callbackWrapper)
		{
			if (settings == null)
				settings = MonoTlsSettings.CopyDefaultSettings ();
			if (cloneSettings)
				settings = settings.CloneWithValidator (this);
			if (provider == null)
				provider = MonoTlsProviderFactory.GetProvider ();

			this.provider = provider;
			this.settings = settings;
			this.tlsStream = stream;
			this.callbackWrapper = callbackWrapper;

			var fallbackToSPM = false;

			if (settings != null) {
				if (settings.RemoteCertificateValidationCallback != null) {
					var callback = Private.CallbackHelpers.MonoToPublic (settings.RemoteCertificateValidationCallback);
					certValidationCallback = new ServerCertValidationCallback (callback);
				}
				certSelectionCallback = Private.CallbackHelpers.MonoToInternal (settings.ClientCertificateSelectionCallback);
				fallbackToSPM = settings.UseServicePointManagerCallback ?? stream != null;
			}

			if (stream != null) {
				this.request = stream.Request;
				this.sender = request;

				if (certValidationCallback == null)
					certValidationCallback = request.ServerCertValidationCallback;
				if (certSelectionCallback == null)
					certSelectionCallback = new LocalCertSelectionCallback (DefaultSelectionCallback);

				if (settings == null)
					fallbackToSPM = true;
			}

			if (fallbackToSPM && certValidationCallback == null)
				certValidationCallback = ServicePointManager.ServerCertValidationCallback;
		}

		static X509Certificate DefaultSelectionCallback (string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
		{
			X509Certificate clientCertificate;
			if (localCertificates == null || localCertificates.Count == 0)
				clientCertificate = null;
			else
				clientCertificate = localCertificates [0];
			return clientCertificate;
		}

		public MonoTlsProvider Provider {
			get { return provider; }
		}

		public MonoTlsSettings Settings {
			get { return settings; }
		}

		public bool HasCertificateSelectionCallback {
			get { return certSelectionCallback != null; }
		}

		public bool SelectClientCertificate (
			string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate,
			string[] acceptableIssuers, out X509Certificate clientCertificate)
		{
			if (certSelectionCallback == null) {
				clientCertificate = null;
				return false;
			}
			clientCertificate = certSelectionCallback (targetHost, localCertificates, remoteCertificate, acceptableIssuers);
			return true;
		}

		internal X509Certificate SelectClientCertificate (
			string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate,
			string[] acceptableIssuers)
		{
			if (certSelectionCallback == null)
				return null;
			return certSelectionCallback (targetHost, localCertificates, remoteCertificate, acceptableIssuers);
		}

		internal bool ValidateClientCertificate (X509Certificate certificate, MonoSslPolicyErrors errors)
		{
			var certs = new X509CertificateCollection ();
			certs.Add (new X509Certificate2 (certificate.GetRawCertData ()));

			var result = ValidateChain (string.Empty, true, certificate, null, certs, (SslPolicyErrors)errors);
			if (result == null)
				return false;

			return result.Trusted && !result.UserDenied;
		}

		public ValidationResult ValidateCertificate (string host, bool serverMode, X509CertificateCollection certs)
		{
			try {
				X509Certificate leaf;
				if (certs != null && certs.Count != 0)
					leaf = certs [0];
				else
					leaf = null;
				var result = ValidateChain (host, serverMode, leaf, null, certs, 0);
				if (tlsStream != null)
					tlsStream.CertificateValidationFailed = result == null || !result.Trusted || result.UserDenied;
				return result;
			} catch {
				if (tlsStream != null)
					tlsStream.CertificateValidationFailed = true;
				throw;
			}
		}

		public ValidationResult ValidateCertificate (string host, bool serverMode, X509Certificate leaf, X509Chain chain)
		{
			try {
				var result = ValidateChain (host, serverMode, leaf, chain, null, 0);
				if (tlsStream != null)
					tlsStream.CertificateValidationFailed = result == null || !result.Trusted || result.UserDenied;
				return result;
			} catch {
				if (tlsStream != null)
					tlsStream.CertificateValidationFailed = true;
				throw;
			}
		}

		ValidationResult ValidateChain (string host, bool server, X509Certificate leaf,
		                                X509Chain chain, X509CertificateCollection certs,
		                                SslPolicyErrors errors)
		{
			var oldChain = chain;
			var ownsChain = chain == null;
			try {
				var result = ValidateChain (host, server, leaf, ref chain, certs, errors);
				if (chain != oldChain)
					ownsChain = true;

				return result;
			} finally {
				// If ValidateChain() changed the chain, then we need to free it.
				if (ownsChain && chain != null)
					chain.Dispose ();
			}
		}

		ValidationResult ValidateChain (string host, bool server, X509Certificate leaf,
		                                ref X509Chain chain, X509CertificateCollection certs,
		                                SslPolicyErrors errors)
		{
			// user_denied is true if the user callback is called and returns false
			bool user_denied = false;
			bool result = false;

			var hasCallback = certValidationCallback != null || callbackWrapper != null;

			if (tlsStream != null)
				request.ServicePoint.UpdateServerCertificate (leaf);

			if (leaf == null) {
				errors |= SslPolicyErrors.RemoteCertificateNotAvailable;
				if (hasCallback) {
					if (callbackWrapper != null)
						result = callbackWrapper.Invoke (certValidationCallback, leaf, null, (MonoSslPolicyErrors)errors);
					else
						result = certValidationCallback.Invoke (sender, leaf, null, errors);
					user_denied = !result;
				}
				return new ValidationResult (result, user_denied, 0, (MonoSslPolicyErrors)errors);
			}

			// Ignore port number when validating certificates.
			if (!string.IsNullOrEmpty (host)) {
				var pos = host.IndexOf (':');
				if (pos > 0)
					host = host.Substring (0, pos);
			}

			ICertificatePolicy policy = ServicePointManager.GetLegacyCertificatePolicy ();

			int status11 = 0; // Error code passed to the obsolete ICertificatePolicy callback

			bool wantsChain = SystemCertificateValidator.NeedsChain (settings);
			if (!wantsChain && hasCallback) {
				if (settings == null || settings.CallbackNeedsCertificateChain)
					wantsChain = true;
			}

			var xerrors = (MonoSslPolicyErrors)errors;
			result = provider.ValidateCertificate (this, host, server, certs, wantsChain, ref chain, ref xerrors, ref status11);
			errors = (SslPolicyErrors)xerrors;

			if (policy != null && (!(policy is DefaultCertificatePolicy) || certValidationCallback == null)) {
				ServicePoint sp = null;
				if (request != null)
					sp = request.ServicePointNoLock;
				if (status11 == 0 && errors != 0) {
					// TRUST_E_FAIL
					status11 = unchecked ((int)0x800B010B);
				}

				// pre 2.0 callback
				result = policy.CheckValidationResult (sp, leaf, request, status11);
				user_denied = !result && !(policy is DefaultCertificatePolicy);
			}
			// If there's a 2.0 callback, it takes precedence
			if (hasCallback) {
				if (callbackWrapper != null)
					result = callbackWrapper.Invoke (certValidationCallback, leaf, chain, (MonoSslPolicyErrors)errors);
				else
					result = certValidationCallback.Invoke (sender, leaf, chain, errors);
				user_denied = !result;
			}
			return new ValidationResult (result, user_denied, status11, (MonoSslPolicyErrors)errors);
		}

		bool InvokeSystemValidator (string targetHost, bool serverMode, X509CertificateCollection certificates, X509Chain chain, ref MonoSslPolicyErrors xerrors, ref int status11)
		{
			var errors = (SslPolicyErrors)xerrors;
			var result = SystemCertificateValidator.Evaluate (settings, targetHost, certificates, chain, ref errors, ref status11);
			xerrors = (MonoSslPolicyErrors)errors;
			return result;
		}
	}
}
#endif

