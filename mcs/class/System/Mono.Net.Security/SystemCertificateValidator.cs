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
	internal static class SystemCertificateValidator
	{
		static bool is_macosx;
#if !MOBILE
		static X509RevocationMode revocation_mode;
#endif

		static SystemCertificateValidator ()
		{
#if MONOTOUCH
			is_macosx = true;
#elif (MONODROID || ORBIS) && !MOBILE_DESKTOP_HOST
			is_macosx = false;
#else
			is_macosx = Environment.OSVersion.Platform != PlatformID.Win32NT && System.IO.File.Exists (OSX509Certificates.SecurityLibrary);
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

		public static X509Chain CreateX509Chain (X509CertificateCollection certs)
		{
			var chain = new X509Chain ();
			chain.ChainPolicy = new X509ChainPolicy ((X509CertificateCollection)(object)certs);

#if !MOBILE
			chain.ChainPolicy.RevocationMode = revocation_mode;
#endif

			return chain;
		}

		static bool BuildX509Chain (X509CertificateCollection certs, X509Chain chain, ref SslPolicyErrors errors, ref int status11)
		{
#if MOBILE
			return false;
#else
			if (is_macosx)
				return false;

			var leaf = (X509Certificate2)certs [0];

			bool ok;
			try {
				ok = chain.Build (leaf);
				if (!ok)
					errors |= GetErrorsFromChain (chain);
			} catch (Exception e) {
				Console.Error.WriteLine ("ERROR building certificate chain: {0}", e);
				Console.Error.WriteLine ("Please, report this problem to the Mono team");
				errors |= SslPolicyErrors.RemoteCertificateChainErrors;
				ok = false;
			}

			try {
				status11 = GetStatusFromChain (chain);
			} catch {
				status11 = -2146762485; // TRUST_E_FAIL - generic
			}

			return ok;
#endif
		}

		static bool CheckUsage (X509CertificateCollection certs, string host, ref SslPolicyErrors errors, ref int status11)
		{
#if !MONOTOUCH
			var leaf = certs[0] as X509Certificate2;
			if (leaf == null)
				leaf = new X509Certificate2 (certs[0]);
			// for OSX and iOS we're using the native API to check for the SSL server policy and host names
			if (!is_macosx) {
				if (!CheckCertificateUsage (leaf)) {
					errors |= SslPolicyErrors.RemoteCertificateChainErrors;
					status11 = -2146762490; //CERT_E_PURPOSE 0x800B0106
					return false;
				}

				if (!string.IsNullOrEmpty (host) && !CheckServerIdentity (leaf, host)) {
					errors |= SslPolicyErrors.RemoteCertificateNameMismatch;
					status11 = -2146762481; // CERT_E_CN_NO_MATCH 0x800B010F
					return false;
				}
			}
#endif
			return true;
		}

		static bool EvaluateSystem (X509CertificateCollection certs, X509CertificateCollection anchors, string host, X509Chain chain, ref SslPolicyErrors errors, ref int status11)
		{
			var leaf = certs [0];
			bool result;

#if MONODROID && !MOBILE_DESKTOP_HOST
			try {
				result = AndroidPlatform.TrustEvaluateSsl (certs);
				if (result) {
					// FIXME: check whether this is still correct.
					//
					// chain.Build() + GetErrorsFromChain() (above) will ALWAYS fail on
					// Android (there are no mozroots or preinstalled root certificates),
					// thus `errors` will ALWAYS have RemoteCertificateChainErrors.
					// Android just verified the chain; clear RemoteCertificateChainErrors.
					errors  &= ~SslPolicyErrors.RemoteCertificateChainErrors;
				} else {
					errors |= SslPolicyErrors.RemoteCertificateChainErrors;
					status11 = unchecked((int)0x800B010B);
				}
			} catch {
				result = false;
				errors |= SslPolicyErrors.RemoteCertificateChainErrors;
				status11 = unchecked((int)0x800B010B);
				// Ignore
			}
#else
			if (is_macosx) {
#if !ORBIS
				// Attempt to use OSX certificates
				// Ideally we should return the SecTrustResult
				OSX509Certificates.SecTrustResult trustResult = OSX509Certificates.SecTrustResult.Deny;
				try {
					trustResult = OSX509Certificates.TrustEvaluateSsl (certs, anchors, host);
					// We could use the other values of trustResult to pass this extra information
					// to the .NET 2 callback for values like SecTrustResult.Confirm
					result = (trustResult == OSX509Certificates.SecTrustResult.Proceed ||
						trustResult == OSX509Certificates.SecTrustResult.Unspecified);
				} catch {
					result = false;
					errors |= SslPolicyErrors.RemoteCertificateChainErrors;
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
#else
				throw new PlatformNotSupportedException ();
#endif
			} else {
				result = BuildX509Chain (certs, chain, ref errors, ref status11);
			}
#endif

			return result;
		}

		public static bool Evaluate (
			MonoTlsSettings settings, string host, X509CertificateCollection certs,
			X509Chain chain, ref SslPolicyErrors errors, ref int status11)
		{
			if (!CheckUsage (certs, host, ref errors, ref status11))
				return false;

			if (settings != null && settings.SkipSystemValidators)
				return false;

			var anchors = settings != null ? settings.TrustAnchors : null;
			return EvaluateSystem (certs, anchors, host, chain, ref errors, ref status11);
		}

		internal static bool NeedsChain (MonoTlsSettings settings)
		{
#if MOBILE
			return false;
#else
			if (!is_macosx)
				return true;
			if (!CertificateValidationHelper.SupportsX509Chain)
				return false;
			if (settings != null)
				return !settings.SkipSystemValidators || settings.CallbackNeedsCertificateChain;
			else
				return true;
#endif
		}

#if !MOBILE
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
#endif

#if !MONOTOUCH
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

