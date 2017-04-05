#if SECURITY_DEP && MONO_FEATURE_APPLETLS
//
// AppleCertificateHelper.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

namespace Mono.AppleTls
{
	static class AppleCertificateHelper
	{
		public static SecIdentity GetIdentity (X509Certificate certificate)
		{
			/*
			 * If we got an 'X509Certificate2', then we require it to have a private key
			 * and import it.
			 */
			var certificate2 = certificate as X509Certificate2;
			if (certificate2 != null) {
				var access = SecAccess.Create ("MartinTest");
				return SecIdentity.Import (certificate2, null, access);
			}

#if MOBILE
			/*
			 * Otherwise, we require the private key to be in the keychain.
			 *
			 * When using Xamarin.Mac or Xamarin.iOS, we try to search the keychain
			 * for the certificate and private key.
			 *
			 * On Xamarin.iOS, this is easy because each app has its own keychain.
			 *
			 * On Xamarin.Mac, the `.app` package needs to be trusted via code-sign
			 * to get permission to access the user's keychain.  [FIXME: I still have to
			 * research how to actually do that.]  Without this, you will get a popup
			 * message each time, asking you whether you want to allow the app to access
			 * the keychain, but you can make these go away by selecting "Trust always".
			 *
			 * On Desktop Mono, this is problematic because selecting "Trust always"
			 * give the 'mono' binary (and thus everything you'll ever run with Mono)
			 * permission to retrieve the private key from the keychain.
			 *
			 * The following code would also trigger constant keychain popup messages,
			 * which could only be suppressed by granting full trust.
			 *
			 * Since you do not need to use the keychain for AppleTls, we currently
			 * disabled it on Desktop Mono until we have a better understanding of how
			 * these mechanisms work to provide a smooth solution.
			 *
			 */
			using (var secCert = new SecCertificate (certificate)) {
				return SecKeyChain.FindIdentity (secCert, true);
			}
#else
			return null;
#endif
		}

		public static SecIdentity GetIdentity (X509Certificate certificate, out SecCertificate[] intermediateCerts)
		{
			var identity = GetIdentity (certificate);

			var impl2 = certificate.Impl as X509Certificate2Impl;
			if (impl2 == null || impl2.IntermediateCertificates == null) {
				intermediateCerts = new SecCertificate [0];
				return identity;
			}

			try {
				intermediateCerts = new SecCertificate [impl2.IntermediateCertificates.Count];
				for (int i = 0; i < intermediateCerts.Length; i++)
					intermediateCerts [i] = new SecCertificate (impl2.IntermediateCertificates [i]);

				return identity;
			} catch {
				identity.Dispose ();
				throw;
			}
		}

		public static bool InvokeSystemCertificateValidator (
			ICertificateValidator2 validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates,
			ref MonoSslPolicyErrors errors, ref int status11)
		{
			if (certificates == null) {
				errors |= MonoSslPolicyErrors.RemoteCertificateNotAvailable;
				return false;
			}

			if (!string.IsNullOrEmpty (targetHost)) {
				var pos = targetHost.IndexOf (':');
				if (pos > 0)
					targetHost = targetHost.Substring (0, pos);
			}

			var policy = SecPolicy.CreateSslPolicy (!serverMode, targetHost);
			var trust = new SecTrust (certificates, policy);

			if (validator.Settings.TrustAnchors != null) {
				var status = trust.SetAnchorCertificates (validator.Settings.TrustAnchors);
				if (status != SecStatusCode.Success)
					throw new InvalidOperationException (status.ToString ());
				trust.SetAnchorCertificatesOnly (false);
			}

			var result = trust.Evaluate ();
			if (result == SecTrustResult.Unspecified)
				return true;

			errors |= MonoSslPolicyErrors.RemoteCertificateChainErrors;
			return false;
		}
	}
}
#endif
