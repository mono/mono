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
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

using MNS = Mono.Net.Security;
#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

namespace Mono.AppleTls
{
	static class AppleCertificateHelper
	{
		public static SafeSecIdentityHandle GetIdentity (X509Certificate certificate)
		{
			/*
			 * If we got an 'X509Certificate2', then we require it to have a private key
			 * and import it.
			 */
			var certificate2 = certificate as X509Certificate2;
			if (certificate2 != null)
				return MonoCertificatePal.ImportIdentity (certificate2);

			/*
			 * Reading Certificates from the Mac Keychain
			 * ==========================================
			 *
			 * Reading the private key from the keychain is a new feature introduced with
			 * AppleTls on XamMac and iOS. On Desktop Mono, this new feature has several
			 * known issues and it also did not received any testing yet. We go back to the old
			 * way of doing things, which is to explicitly provide an X509Certificate2 with a
			 * private key.
			 * 
			 * Keychain Dialog Popups
			 * ======================
			 * 
			 * When using Xamarin.Mac or Xamarin.iOS, we try to search the keychain
			 * for the certificate and private key.
			 * 
			 * On Xamarin.iOS, this is easy because each app has its own keychain.
			 * 
			 * On Xamarin.Mac, the .app package needs to be trusted via code-sign
			 * to get permission to access the user's keychain. [FIXME: I still have to
			 * research how to actually do that.] Without this, you will get a popup
			 * message each time, asking you whether you want to allow the app to access
			 * the keychain, but you can make these go away by selecting "Trust always".
			 * 
			 * On Desktop Mono, this is problematic because selecting "Trust always"
			 * give the 'mono' binary (and thus everything you'll ever run with Mono)
			 * permission to retrieve the private key from the keychain.
			 * 
			 * This code would also trigger constant keychain popup messages,
			 * which could only be suppressed by granting full trust. It also makes it
			 * impossible to run Mono in headless mode.
			 * 
			 * SecIdentityCreate
			 * =================
			 * 
			 * To avoid these problems, we are currently using an undocumented API
			 * called SecIdentityRef() to avoid using the Mac keychain whenever a
			 * X509Certificate2 with a private key is used.
			 * 
			 * On iOS and XamMac, you can still provide the X509Certificate without
			 * a private key - in this case, a keychain search will be performed (and you
			 * may get a popup message on XamMac).
			 */

#if MOBILE
			using (var secCert = MonoCertificatePal.FromOtherCertificate (certificate))
				return MonoCertificatePal.FindIdentity (secCert, true);
#else
			return new SafeSecIdentityHandle ();
#endif
		}

		public static SafeSecIdentityHandle GetIdentity (X509Certificate certificate, out SafeSecCertificateHandle[] intermediateCerts)
		{
			var identity = GetIdentity (certificate);

			var impl2 = certificate.Impl as X509Certificate2Impl;
			if (impl2 == null || impl2.IntermediateCertificates == null) {
				intermediateCerts = new SafeSecCertificateHandle [0];
				return identity;
			}

			intermediateCerts = new SafeSecCertificateHandle [impl2.IntermediateCertificates.Count];

			try {
				for (int i = 0; i < intermediateCerts.Length; i++)
					intermediateCerts [i] = MonoCertificatePal.FromOtherCertificate (impl2.IntermediateCertificates[i]);

				return identity;
			} catch {
				for (int i = 0; i < intermediateCerts.Length; i++) {
					intermediateCerts [i]?.Dispose ();
				}
				identity?.Dispose ();
				throw;
			}
		}

		public static bool InvokeSystemCertificateValidator (
			MNS.ChainValidationHelper validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates,
			ref SslPolicyErrors errors, ref int status11)
		{
			if (certificates == null) {
				errors |= SslPolicyErrors.RemoteCertificateNotAvailable;
				return false;
			}

			if (!string.IsNullOrEmpty (targetHost)) {
				var pos = targetHost.IndexOf (':');
				if (pos > 0)
					targetHost = targetHost.Substring (0, pos);
			}

			using (var policy = SecPolicy.CreateSslPolicy (!serverMode, targetHost))
			using (var trust = new SecTrust (certificates, policy)) {
				if (validator.Settings.TrustAnchors != null) {
					var status = trust.SetAnchorCertificates (validator.Settings.TrustAnchors);
					if (status != SecStatusCode.Success)
						throw new InvalidOperationException (status.ToString ());
					trust.SetAnchorCertificatesOnly (false);
				}

				if (validator.Settings.CertificateValidationTime != null) {
					var status = trust.SetVerifyDate (validator.Settings.CertificateValidationTime.Value);
					if (status != SecStatusCode.Success)
						throw new InvalidOperationException (status.ToString ());
				}

				var result = trust.Evaluate ();
				if (result == SecTrustResult.Unspecified || result == SecTrustResult.Proceed)
					return true;

				errors |= SslPolicyErrors.RemoteCertificateChainErrors;
				return false;
			}
		}
	}
}
