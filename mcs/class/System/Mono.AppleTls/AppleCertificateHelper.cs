#if XAMARIN_APPLETLS
//
// AppleCertificateHelper.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//
using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Interface;
using MX = Mono.Security.X509;

using XamCore.Foundation;
using XamCore.CoreFoundation;
using XamCore.ObjCRuntime;
using XamCore.Security;

namespace XamCore.Security.Tls
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
			if (certificate2 != null)
				return SecIdentity.Import (certificate2);

			/*
			 * Otherwise, we require the private key to be in the keychain.
			 */
			using (var secCert = new SecCertificate (certificate)) {
				return SecKeyChain.FindIdentity (secCert, true);
			}
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
