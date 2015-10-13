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
		readonly ServerCertValidationCallback certValidationCallback;
		readonly LocalCertSelectionCallback certSelectionCallback;
		readonly ServerCertValidationCallbackWrapper callbackWrapper;
		readonly MonoTlsStream tlsStream;
		readonly HttpWebRequest request;

		static bool is_macosx;
		static bool is_mobile;
#if !MONOTOUCH
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

#if !MONOTOUCH
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

		internal static ICertificateValidator GetDefaultValidator (MonoTlsSettings settings)
		{
			if (settings.CertificateValidator == null)
				settings.CertificateValidator = new ChainValidationHelper (settings, false, null, null);
			return settings.CertificateValidator;
		}

#region SslStream support

		/*
		 * This is a hack which is used in SslStream - see ReferenceSources/SslStream.cs for details.
		 */
		internal static ChainValidationHelper CloneWithCallbackWrapper (ref MonoTlsSettings settings, ServerCertValidationCallbackWrapper wrapper)
		{
			var helper = (ChainValidationHelper)settings.CertificateValidator;
			if (helper == null)
				helper = new ChainValidationHelper (settings, true, null, wrapper);
			else
				helper = new ChainValidationHelper (helper, settings, wrapper);
			settings = helper.settings;
			return helper;
		}

		internal static bool InvokeCallback (ServerCertValidationCallback callback, object sender, X509Certificate certificate, X509Chain chain, MonoSslPolicyErrors sslPolicyErrors)
		{
			return callback.Invoke (sender, certificate, chain, (SslPolicyErrors)sslPolicyErrors);
		}

#endregion

		ChainValidationHelper (ChainValidationHelper other, MonoTlsSettings settings, ServerCertValidationCallbackWrapper callbackWrapper = null)
		{
			sender = other.sender;
			certValidationCallback = other.certValidationCallback;
			certSelectionCallback = other.certSelectionCallback;
			tlsStream = other.tlsStream;
			request = other.request;

			this.settings = settings = settings.CloneWithValidator (this);
			this.callbackWrapper = callbackWrapper;
		}

		internal static ChainValidationHelper Create (ref MonoTlsSettings settings, MonoTlsStream stream)
		{
			var helper = new ChainValidationHelper (settings, true, stream, null);
			settings = helper.settings;
			return helper;
		}

		ChainValidationHelper (MonoTlsSettings settings, bool cloneSettings, MonoTlsStream stream, ServerCertValidationCallbackWrapper callbackWrapper)
		{
			if (cloneSettings)
				settings = settings.CloneWithValidator (this);

			this.settings = settings;
			this.tlsStream = stream;
			this.callbackWrapper = callbackWrapper;

			var fallbackToSPM = false;

			if (settings != null) {
				if (settings.ServerCertificateValidationCallback != null) {
					var callback = Private.CallbackHelpers.MonoToPublic (settings.ServerCertificateValidationCallback);
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
					certSelectionCallback = new LocalCertSelectionCallback (stream.SelectClientCertificate);

				if (settings == null)
					fallbackToSPM = true;
			}

			if (fallbackToSPM && certValidationCallback == null)
				certValidationCallback = ServicePointManager.ServerCertValidationCallback;
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
			var certs2 = new X509Certificate2Collection ();
			certs2.Add (new X509Certificate2 (certificate.GetRawCertData ()));

			var result = ValidateChain (null, certs2, (SslPolicyErrors)errors);
			if (result == null)
				return false;

			return result.Trusted && !result.UserDenied;
		}

		static X509Certificate2Collection Convert (MSX.X509CertificateCollection certificates)
		{
			if (certificates == null)
				return null;

			var certs2 = new X509Certificate2Collection ();
			for (int i = 0; i < certificates.Count; i++)
				certs2.Add (new X509Certificate2 (certificates [i].RawData));
			return certs2;
		}

		static X509Certificate2Collection Convert (XX509CertificateCollection certificates)
		{
			var certs2 = (object)certificates as X509Certificate2Collection;
			if (certs2 != null || certificates == null)
				return certs2;

			certs2 = new X509Certificate2Collection ();
			for (int i = 0; i < certificates.Count; i++)
				certs2.Add ((X509Certificate2)certificates [i]);
			return certs2;
		}

		public ValidationResult ValidateClientCertificate (XX509CertificateCollection certs)
		{
			var certs2 = Convert (certs);
			return ValidateChain (null, certs2, 0);
		}

		public ValidationResult ValidateChain (string host, XX509CertificateCollection certs)
		{
			try {
				var certs2 = Convert (certs);
				var result = ValidateChain (host, certs2, 0);
				if (tlsStream != null)
					tlsStream.CertificateValidationFailed = result == null || !result.Trusted || result.UserDenied;
				return result;
			} catch {
				if (tlsStream != null)
					tlsStream.CertificateValidationFailed = true;
				throw;
			}
		}

		internal ValidationResult ValidateChain (string host, MSX.X509CertificateCollection certs)
		{
			try {
				var certs2 = Convert (certs);
				var result = ValidateChain (host, certs2, 0);
				if (tlsStream != null)
					tlsStream.CertificateValidationFailed = result == null || !result.Trusted || result.UserDenied;
				return result;
			} catch {
				if (tlsStream != null)
					tlsStream.CertificateValidationFailed = true;
				throw;
			}
		}

		ValidationResult ValidateChain (string host, X509Certificate2Collection certs, SslPolicyErrors errors)
		{
			// user_denied is true if the user callback is called and returns false
			bool user_denied = false;
			bool result = false;

			var hasCallback = certValidationCallback != null || callbackWrapper != null;

			X509Certificate2 leaf;
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

			bool needsChain;
			bool skipSystemValidators = false;
			if (!CertificateValidationHelper.SupportsX509Chain || is_mobile || is_macosx) {
				needsChain = false;
			} else if (settings != null) {
				skipSystemValidators = settings.SkipSystemValidators;
				needsChain = !settings.SkipSystemValidators || settings.CallbackNeedsCertificateChain;
			} else {
				needsChain = true;
			}

			ICertificatePolicy policy = ServicePointManager.GetLegacyCertificatePolicy ();

			int status11 = 0; // Error code passed to the obsolete ICertificatePolicy callback
			X509Chain chain = null;

			if (needsChain) {
				chain = new X509Chain ();
				chain.ChainPolicy = new X509ChainPolicy ();


#if !MONOTOUCH
				chain.ChainPolicy.RevocationMode = revocation_mode;
#endif
				for (int i = 1; i < certs.Count; i++) {
					chain.ChainPolicy.ExtraStore.Add (certs [i]);
				}
			}

#if !MONOTOUCH
			if (needsChain) {
				try {
					if (!chain.Build (leaf))
						errors |= GetErrorsFromChain (chain);
				} catch (Exception e) {
					Console.Error.WriteLine ("ERROR building certificate chain: {0}", e);
					Console.Error.WriteLine ("Please, report this problem to the Mono team");
					errors |= SslPolicyErrors.RemoteCertificateChainErrors;
				}
			}

			// for OSX and iOS we're using the native API to check for the SSL server policy and host names
			if (!is_macosx) {
				if (!CheckCertificateUsage (leaf)) {
					errors |= SslPolicyErrors.RemoteCertificateChainErrors;
					status11 = -2146762490; //CERT_E_PURPOSE 0x800B0106
				}

				if (host != null && !CheckServerIdentity (leaf, host)) {
					errors |= SslPolicyErrors.RemoteCertificateNameMismatch;
					status11 = -2146762481; // CERT_E_CN_NO_MATCH 0x800B010F
				}
			}
#endif

			if (is_macosx && !skipSystemValidators) {
				// Attempt to use OSX certificates
				// Ideally we should return the SecTrustResult
				OSX509Certificates.SecTrustResult trustResult = OSX509Certificates.SecTrustResult.Deny;
				try {
					trustResult = OSX509Certificates.TrustEvaluateSsl (certs, host);
					// We could use the other values of trustResult to pass this extra information
					// to the .NET 2 callback for values like SecTrustResult.Confirm
					result = (trustResult == OSX509Certificates.SecTrustResult.Proceed ||
					trustResult == OSX509Certificates.SecTrustResult.Unspecified);
				} catch {
					// Ignore
				}
					
				if (result) {
					// TrustEvaluateSsl was successful so there's no trust error
					// IOW we discard our own chain (since we trust OSX one instead)
					errors = 0;
				} else {
					// callback and DefaultCertificatePolicy needs this since 'result' is not specified
					status11 = (int)trustResult;
					errors |= SslPolicyErrors.RemoteCertificateChainErrors;
				}
			}


#if MONODROID && SECURITY_DEP
			if (!skipSystemValidators) {
				result = AndroidPlatform.TrustEvaluateSsl (certs, sender, leaf, chain, errors);
				if (result) {
					// chain.Build() + GetErrorsFromChain() (above) will ALWAYS fail on
					// Android (there are no mozroots or preinstalled root certificates),
					// thus `errors` will ALWAYS have RemoteCertificateChainErrors.
					// Android just verified the chain; clear RemoteCertificateChainErrors.
					errors  &= ~SslPolicyErrors.RemoteCertificateChainErrors;
				}
			}
#endif
	
			if (policy != null && (!(policy is DefaultCertificatePolicy) || certValidationCallback == null)) {
				ServicePoint sp = null;
				if (request != null)
					sp = request.ServicePointNoLock;
				if (status11 == 0 && errors != 0)
					status11 = GetStatusFromChain (chain);

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

		static int GetStatusFromChain (X509Chain chain)
		{
			long result = 0;
			foreach (var status in chain.ChainStatus) {
				X509ChainStatusFlags flags = status.Status;
				if (flags == X509ChainStatusFlags.NoError)
					continue;

				// CERT_E_EXPIRED
				if ((flags & X509ChainStatusFlags.NotTimeValid) != 0)
					result = 0x800B0101;
					// CERT_E_VALIDITYPERIODNESTING
					else if ((flags & X509ChainStatusFlags.NotTimeNested) != 0)
					result = 0x800B0102;
					// CERT_E_REVOKED
					else if ((flags & X509ChainStatusFlags.Revoked) != 0)
					result = 0x800B010C;
					// TRUST_E_CERT_SIGNATURE
					else if ((flags & X509ChainStatusFlags.NotSignatureValid) != 0)
					result = 0x80096004;
					// CERT_E_WRONG_USAGE
					else if ((flags & X509ChainStatusFlags.NotValidForUsage) != 0)
					result = 0x800B0110;
					// CERT_E_UNTRUSTEDROOT
					else if ((flags & X509ChainStatusFlags.UntrustedRoot) != 0)
					result = 0x800B0109;
					// CRYPT_E_NO_REVOCATION_CHECK
					else if ((flags & X509ChainStatusFlags.RevocationStatusUnknown) != 0)
					result = 0x80092012;
					// CERT_E_CHAINING
					else if ((flags & X509ChainStatusFlags.Cyclic) != 0)
					result = 0x800B010A;
					// TRUST_E_FAIL - generic
					else if ((flags & X509ChainStatusFlags.InvalidExtension) != 0)
					result = 0x800B010B;
					// CERT_E_UNTRUSTEDROOT
					else if ((flags & X509ChainStatusFlags.InvalidPolicyConstraints) != 0)
					result = 0x800B010D;
					// TRUST_E_BASIC_CONSTRAINTS
					else if ((flags & X509ChainStatusFlags.InvalidBasicConstraints) != 0)
					result = 0x80096019;
					// CERT_E_INVALID_NAME
					else if ((flags & X509ChainStatusFlags.InvalidNameConstraints) != 0)
					result = 0x800B0114;
					// CERT_E_INVALID_NAME
					else if ((flags & X509ChainStatusFlags.HasNotSupportedNameConstraint) != 0)
					result = 0x800B0114;
					// CERT_E_INVALID_NAME
					else if ((flags & X509ChainStatusFlags.HasNotDefinedNameConstraint) != 0)
					result = 0x800B0114;
					// CERT_E_INVALID_NAME
					else if ((flags & X509ChainStatusFlags.HasNotPermittedNameConstraint) != 0)
					result = 0x800B0114;
					// CERT_E_INVALID_NAME
					else if ((flags & X509ChainStatusFlags.HasExcludedNameConstraint) != 0)
					result = 0x800B0114;
					// CERT_E_CHAINING
					else if ((flags & X509ChainStatusFlags.PartialChain) != 0)
					result = 0x800B010A;
					// CERT_E_EXPIRED
					else if ((flags & X509ChainStatusFlags.CtlNotTimeValid) != 0)
					result = 0x800B0101;
					// TRUST_E_CERT_SIGNATURE
					else if ((flags & X509ChainStatusFlags.CtlNotSignatureValid) != 0)
					result = 0x80096004;
					// CERT_E_WRONG_USAGE
					else if ((flags & X509ChainStatusFlags.CtlNotValidForUsage) != 0)
					result = 0x800B0110;
					// CRYPT_E_NO_REVOCATION_CHECK
					else if ((flags & X509ChainStatusFlags.OfflineRevocation) != 0)
					result = 0x80092012;
					// CERT_E_ISSUERCHAINING
					else if ((flags & X509ChainStatusFlags.NoIssuanceChainPolicy) != 0)
					result = 0x800B0107;
				else
					result = 0x800B010B; // TRUST_E_FAIL - generic

				break; // Exit the loop on the first error
			}
			return (int)result;
		}


#if !MONOTOUCH
		static SslPolicyErrors GetErrorsFromChain (X509Chain chain)
		{
			SslPolicyErrors errors = SslPolicyErrors.None;
			foreach (var status in chain.ChainStatus) {
				if (status.Status == X509ChainStatusFlags.NoError)
					continue;
				errors |= SslPolicyErrors.RemoteCertificateChainErrors;
				break;
			}
			return errors;
		}

		static X509KeyUsageFlags s_flags = X509KeyUsageFlags.DigitalSignature |
		                                    X509KeyUsageFlags.KeyAgreement |
		                                    X509KeyUsageFlags.KeyEncipherment;
		// Adapted to System 2.0+ from TlsServerCertificate.cs
		//------------------------------
		// Note: this method only works for RSA certificates
		// DH certificates requires some changes - does anyone use one ?
		static bool CheckCertificateUsage (X509Certificate2 cert)
		{
			try {
				// certificate extensions are required for this
				// we "must" accept older certificates without proofs
				if (cert.Version < 3)
					return true;

				X509KeyUsageExtension kux = (cert.Extensions ["2.5.29.15"] as X509KeyUsageExtension);
				X509EnhancedKeyUsageExtension eku = (cert.Extensions ["2.5.29.37"] as X509EnhancedKeyUsageExtension);
				if (kux != null && eku != null) {
					// RFC3280 states that when both KeyUsageExtension and 
					// ExtendedKeyUsageExtension are present then BOTH should
					// be valid
					if ((kux.KeyUsages & s_flags) == 0)
						return false;
					return eku.EnhancedKeyUsages ["1.3.6.1.5.5.7.3.1"] != null ||
					eku.EnhancedKeyUsages ["2.16.840.1.113730.4.1"] != null;
				} else if (kux != null) {
					return ((kux.KeyUsages & s_flags) != 0);
				} else if (eku != null) {
					// Server Authentication (1.3.6.1.5.5.7.3.1) or
					// Netscape Server Gated Crypto (2.16.840.1.113730.4)
					return eku.EnhancedKeyUsages ["1.3.6.1.5.5.7.3.1"] != null ||
					eku.EnhancedKeyUsages ["2.16.840.1.113730.4.1"] != null;
				}

				// last chance - try with older (deprecated) Netscape extensions
				X509Extension ext = cert.Extensions ["2.16.840.1.113730.1.1"];
				if (ext != null) {
					string text = ext.NetscapeCertType (false);
					return text.IndexOf ("SSL Server Authentication", StringComparison.Ordinal) != -1;
				}
				return true;
			} catch (Exception e) {
				Console.Error.WriteLine ("ERROR processing certificate: {0}", e);
				Console.Error.WriteLine ("Please, report this problem to the Mono team");
				return false;
			}
		}

		// RFC2818 - HTTP Over TLS, Section 3.1
		// http://www.ietf.org/rfc/rfc2818.txt
		//
		// 1.	if present MUST use subjectAltName dNSName as identity
		// 1.1.		if multiples entries a match of any one is acceptable
		// 1.2.		wildcard * is acceptable
		// 2.	URI may be an IP address -> subjectAltName.iPAddress
		// 2.1.		exact match is required
		// 3.	Use of the most specific Common Name (CN=) in the Subject
		// 3.1		Existing practice but DEPRECATED
		static bool CheckServerIdentity (X509Certificate2 cert, string targetHost)
		{
			try {
				var mcert = new MSX.X509Certificate (cert.RawData);
				MSX.X509Extension ext = mcert.Extensions ["2.5.29.17"];
				// 1. subjectAltName
				if (ext != null) {
					SubjectAltNameExtension subjectAltName = new SubjectAltNameExtension (ext);
					// 1.1 - multiple dNSName
					foreach (string dns in subjectAltName.DNSNames) {
						// 1.2 TODO - wildcard support
						if (Match (targetHost, dns))
							return true;
					}
					// 2. ipAddress
					foreach (string ip in subjectAltName.IPAddresses) {
						// 2.1. Exact match required
						if (ip == targetHost)
							return true;
					}
				}
				// 3. Common Name (CN=)
				return CheckDomainName (mcert.SubjectName, targetHost);
			} catch (Exception e) {
				Console.Error.WriteLine ("ERROR processing certificate: {0}", e);
				Console.Error.WriteLine ("Please, report this problem to the Mono team");
				return false;
			}
		}

		static bool CheckDomainName (string subjectName, string targetHost)
		{
			string	domainName = String.Empty;
			Regex search = new Regex (@"CN\s*=\s*([^,]*)");
			MatchCollection	elements = search.Matches (subjectName);
			if (elements.Count == 1) {
				if (elements [0].Success)
					domainName = elements [0].Groups [1].Value.ToString ();
			}

			return Match (targetHost, domainName);
		}

		// ensure the pattern is valid wrt to RFC2595 and RFC2818
		// http://www.ietf.org/rfc/rfc2595.txt
		// http://www.ietf.org/rfc/rfc2818.txt
		static bool Match (string hostname, string pattern)
		{
			// check if this is a pattern
			int index = pattern.IndexOf ('*');
			if (index == -1) {
				// not a pattern, do a direct case-insensitive comparison
				return (String.Compare (hostname, pattern, true, CultureInfo.InvariantCulture) == 0);
			}

			// check pattern validity
			// A "*" wildcard character MAY be used as the left-most name component in the certificate.

			// unless this is the last char (valid)
			if (index != pattern.Length - 1) {
				// then the next char must be a dot .'.
				if (pattern [index + 1] != '.')
					return false;
			}

			// only one (A) wildcard is supported
			int i2 = pattern.IndexOf ('*', index + 1);
			if (i2 != -1)
				return false;

			// match the end of the pattern
			string end = pattern.Substring (index + 1);
			int length = hostname.Length - end.Length;
			// no point to check a pattern that is longer than the hostname
			if (length <= 0)
				return false;

			if (String.Compare (hostname, length, end, 0, end.Length, true, CultureInfo.InvariantCulture) != 0)
				return false;

			// special case, we start with the wildcard
			if (index == 0) {
				// ensure we hostname non-matched part (start) doesn't contain a dot
				int i3 = hostname.IndexOf ('.');
				return ((i3 == -1) || (i3 >= (hostname.Length - end.Length)));
			}

			// match the start of the pattern
			string start = pattern.Substring (0, index);
			return (String.Compare (hostname, 0, start, 0, start.Length, true, CultureInfo.InvariantCulture) == 0);
		}
#endif
	}
}
#endif

