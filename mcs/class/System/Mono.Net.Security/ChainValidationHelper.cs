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
#if MONO_X509_ALIAS
extern alias PrebuiltSystem;
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
#if MONO_X509_ALIAS
using XX509CertificateCollection = PrebuiltSystem::System.Security.Cryptography.X509Certificates.X509CertificateCollection;
#else
using XX509CertificateCollection = System.Security.Cryptography.X509Certificates.X509CertificateCollection;
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

	internal class ChainValidationHelper : ICertificateValidator
	{
		readonly object sender;
		readonly MonoTlsSettings settings;
		readonly MonoTlsProvider provider;
		readonly ServerCertValidationCallback certValidationCallback;
		readonly LocalCertSelectionCallback certSelectionCallback;
		readonly ServerCertValidationCallbackWrapper callbackWrapper;
		readonly MonoTlsStream tlsStream;
		readonly HttpWebRequest request;

		static bool is_macosx;
		static bool is_mobile;
#if !MOBILE
		static X509RevocationMode revocation_mode;
#endif

		static ChainValidationHelper ()
		{
#if MONOTOUCH
			is_macosx = true;
			is_mobile = true;
#elif MONODROID
			is_macosx = false;
			is_mobile = true;
#else
			is_macosx = System.IO.File.Exists (OSX509Certificates.SecurityLibrary);
			is_mobile = false;
#endif

#if !MOBILE
			revocation_mode = X509RevocationMode.NoCheck;
			try {
				string str = Environment.GetEnvironmentVariable ("MONO_X509_REVOCATION_MODE");
				if (String.IsNullOrEmpty (str))
					return;
				revocation_mode = (X509RevocationMode)Enum.Parse (typeof(X509RevocationMode), str, true);
			} catch {
			}
#endif
		}

		internal static ICertificateValidator GetDefaultValidator (MonoTlsProvider provider, MonoTlsSettings settings)
		{
			if (settings == null)
				return new ChainValidationHelper (provider, null, false, null, null);
			if (settings.CertificateValidator != null)
				return settings.CertificateValidator;
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

			this.provider = provider;
			this.settings = settings = settings.CloneWithValidator (this);
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
			if (cloneSettings)
				settings = settings.CloneWithValidator (this);

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
				fallbackToSPM = settings.UseServicePointManagerCallback;
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

		static X509Certificate DefaultSelectionCallback (string targetHost, XX509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
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

		public X509Certificate SelectClientCertificate (
			string targetHost, XX509CertificateCollection localCertificates, X509Certificate remoteCertificate,
			string[] acceptableIssuers)
		{
			if (certSelectionCallback == null)
				return null;
			return certSelectionCallback (targetHost, localCertificates, remoteCertificate, acceptableIssuers);
		}

		internal bool ValidateClientCertificate (X509Certificate certificate, MonoSslPolicyErrors errors)
		{
			var certs = new XX509CertificateCollection ();
			certs.Add (new X509Certificate2 (certificate.GetRawCertData ()));

			var result = ValidateChain (string.Empty, true, certs, (SslPolicyErrors)errors);
			if (result == null)
				return false;

			return result.Trusted && !result.UserDenied;
		}

		public ValidationResult ValidateClientCertificate (XX509CertificateCollection certs)
		{
			return ValidateChain (string.Empty, true, certs, 0);
		}

		public ValidationResult ValidateChain (string host, XX509CertificateCollection certs)
		{
			try {
				var result = ValidateChain (host, false, certs, 0);
				if (tlsStream != null)
					tlsStream.CertificateValidationFailed = result == null || !result.Trusted || result.UserDenied;
				return result;
			} catch {
				if (tlsStream != null)
					tlsStream.CertificateValidationFailed = true;
				throw;
			}
		}

		ValidationResult ValidateChain (string host, bool server, XX509CertificateCollection certs, SslPolicyErrors errors)
		{
			// user_denied is true if the user callback is called and returns false
			bool user_denied = false;
			bool result = false;

			var hasCallback = certValidationCallback != null || callbackWrapper != null;

			X509Certificate leaf;
			if (certs == null || certs.Count == 0)
				leaf = null;
			else
				leaf = certs [0];

			if (tlsStream != null)
				request.ServicePoint.SetServerCertificate (leaf);

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

			ICertificatePolicy policy = ServicePointManager.GetLegacyCertificatePolicy ();

			int status11 = 0; // Error code passed to the obsolete ICertificatePolicy callback
			X509Chain chain = null;

			if (provider != null && provider.HasCustomSystemCertificateValidator) {
				if (SystemCertificateValidator.NeedsChain (settings))
					throw new NotSupportedException ("Cannot use MonoTlsProvider.InvokeSystemCertificateValidator() when the X509Chain is required.");
				var xerrors = (MonoSslPolicyErrors)errors;
				result = provider.InvokeSystemCertificateValidator (this, host, server, certs, ref xerrors, ref status11);
				errors = (SslPolicyErrors)xerrors;
			} else {
				result = SystemCertificateValidator.Evaluate (settings, host, certs, ref chain, ref errors, ref status11);
			}

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

		public bool InvokeSystemValidator (string targetHost, bool serverMode, XX509CertificateCollection certificates, ref MonoSslPolicyErrors xerrors, ref int status11)
		{
			if (SystemCertificateValidator.NeedsChain (settings))
				throw new NotSupportedException ("Cannot use ICertificateValidator.InvokeSystemValidator() when the X509Chain is required.");

			X509Chain chain = null;
			var errors = (SslPolicyErrors)xerrors;
			var result = SystemCertificateValidator.Evaluate (settings, targetHost, certificates, ref chain, ref errors, ref status11);
			xerrors = (MonoSslPolicyErrors)errors;
			return result;
		}
	}
}
#endif

