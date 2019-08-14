//
// X509CertificateImplBtls.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if MONO_FEATURE_BTLS
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MX = MonoSecurity::Mono.Security.X509;
using MonoSecurity::Mono.Security.Cryptography;
using MonoSecurity::Mono.Security.Authenticode;
#else
using MX = Mono.Security.X509;
using Mono.Security.Cryptography;
using Mono.Security.Authenticode;
#endif

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Mono.Btls
{
	class X509CertificateImplBtls : X509Certificate2ImplUnix
	{
		MonoBtlsX509 x509;
		MonoBtlsKey nativePrivateKey;
		X509CertificateImplCollection intermediateCerts;
		PublicKey publicKey;

		internal X509CertificateImplBtls ()
		{
		}

		internal X509CertificateImplBtls (MonoBtlsX509 x509)
		{
			this.x509 = x509.Copy ();
		}

		X509CertificateImplBtls (X509CertificateImplBtls other)
		{
			x509 = other.x509 != null ? other.x509.Copy () : null;
			nativePrivateKey = other.nativePrivateKey != null ? other.nativePrivateKey.Copy () : null;
			if (other.intermediateCerts != null)
				intermediateCerts = other.intermediateCerts.Clone ();
		}

		internal X509CertificateImplBtls (byte[] data, MonoBtlsX509Format format)
		{
			x509 = MonoBtlsX509.LoadFromData (data, format);
		}

		internal X509CertificateImplBtls (byte[] data, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags)
		{
			if (password == null || password.IsInvalid) {
				try {
					Import (data);
				} catch (Exception e) {
					try {
						 ImportPkcs12 (data, null);
					} catch {
						try {
							ImportAuthenticode (data);
						} catch {
							string msg = Locale.GetText ("Unable to decode certificate.");
							// inner exception is the original (not second) exception
							throw new CryptographicException (msg, e);
						}
					}
				}
			} else {
				// try PKCS#12
				try {
					ImportPkcs12 (data, password);
				} catch (Exception e) {
					try {
						// it's possible to supply a (unrequired/unusued) password
						// fix bug #79028
						Import (data);
					} catch {
						try {
							ImportAuthenticode (data);
						} catch {
							string msg = Locale.GetText ("Unable to decode certificate.");
							// inner exception is the original (not second) exception
							throw new CryptographicException (msg, e);
						}
					}
				}
			}
		}

		public override bool IsValid {
			get { return x509 != null && x509.IsValid; }
		}

		public override IntPtr Handle {
			get { return x509.Handle.DangerousGetHandle (); }
		}

		public override IntPtr GetNativeAppleCertificate ()
		{
			return IntPtr.Zero;
		}

		internal MonoBtlsX509 X509 {
			get {
				ThrowIfContextInvalid ();
				return x509;
			}
		}

		internal MonoBtlsKey NativePrivateKey {
			get {
				ThrowIfContextInvalid ();
				return nativePrivateKey;
			}
		}

		public override X509CertificateImpl Clone ()
		{
			ThrowIfContextInvalid ();
			return new X509CertificateImplBtls (this);
		}

		public override bool Equals (X509CertificateImpl other, out bool result)
		{
			var otherBoringImpl = other as X509CertificateImplBtls;
			if (otherBoringImpl == null) {
				result = false;
				return false;
			}

			result = MonoBtlsX509.Compare (X509, otherBoringImpl.X509) == 0;
			return true;
		}

		protected override byte[] GetRawCertData ()
		{
			ThrowIfContextInvalid ();
			return X509.GetRawData (MonoBtlsX509Format.DER);
		}

		internal override X509CertificateImplCollection IntermediateCertificates {
			get { return intermediateCerts; }
		}

		protected override void Dispose (bool disposing)
		{
			if (x509 != null) {
				x509.Dispose ();
				x509 = null;
			}
		}

#region X509Certificate2Impl

		internal override X509Certificate2Impl FallbackImpl => throw new InvalidOperationException ();

		public override bool HasPrivateKey => nativePrivateKey != null;

		public override AsymmetricAlgorithm PrivateKey {
			get {
				if (nativePrivateKey == null)
					return null;
				var bytes = nativePrivateKey.GetBytes (true);
				return PKCS8.PrivateKeyInfo.DecodeRSA (bytes);
			}
			set {
				if (nativePrivateKey != null)
					nativePrivateKey.Dispose ();
				try {
					// FIXME: there doesn't seem to be a public API to check whether it actually
					//        contains a private key (apart from RSAManaged.PublicOnly).
					if (value != null)
						nativePrivateKey = MonoBtlsKey.CreateFromRSAPrivateKey ((RSA)value);
				} catch {
					nativePrivateKey = null;
				}
			}
		}

		public override RSA GetRSAPrivateKey ()
		{
			if (nativePrivateKey == null)
				return null;
			var bytes = nativePrivateKey.GetBytes (true);
			return PKCS8.PrivateKeyInfo.DecodeRSA (bytes);
		}

		public override DSA GetDSAPrivateKey ()
		{
			throw new PlatformNotSupportedException ();
		}

		public override PublicKey PublicKey {
			get {
				ThrowIfContextInvalid ();
				if (publicKey == null) {
					var keyAsn = X509.GetPublicKeyAsn1 ();
					var keyParamAsn = X509.GetPublicKeyParameters ();
					publicKey = new PublicKey (keyAsn.Oid, keyParamAsn, keyAsn);
				}
				return publicKey;
			}
		}

		void Import (byte[] data)
		{
			if (data != null) {
				// Does it look like PEM?
				if ((data.Length > 0) && (data [0] != 0x30))
					x509 = MonoBtlsX509.LoadFromData (data, MonoBtlsX509Format.PEM);
				else
					x509 = MonoBtlsX509.LoadFromData (data, MonoBtlsX509Format.DER);
			}
		}

		void ImportPkcs12 (byte[] data, SafePasswordHandle password)
		{
			using (var pkcs12 = new MonoBtlsPkcs12 ()) {
				if (password == null || password.IsInvalid) {
					try {
						// Support both unencrypted PKCS#12..
						pkcs12.Import (data, null);
					} catch {
						// ..and PKCS#12 encrypted with an empty password
						using (var empty = new SafePasswordHandle (string.Empty))
							pkcs12.Import (data, empty);
					}
				} else {
					pkcs12.Import (data, password);
				}

				x509 = pkcs12.GetCertificate (0);
				if (pkcs12.HasPrivateKey)
					nativePrivateKey = pkcs12.GetPrivateKey ();
				if (pkcs12.Count > 1) {
					intermediateCerts = new X509CertificateImplCollection ();
					for (int i = 0; i < pkcs12.Count; i++) {
						using (var ic = pkcs12.GetCertificate (i)) {
							if (MonoBtlsX509.Compare (ic, x509) == 0)
								continue;
							var impl = new X509CertificateImplBtls (ic);
							intermediateCerts.Add (impl, true);
						}
					}
				}
			}
		}

		void ImportAuthenticode (byte[] data)
		{
			if (data != null) {
				AuthenticodeDeformatter ad = new AuthenticodeDeformatter (data);
				Import (ad.SigningCertificate.RawData);
			}
		}

		public override bool Verify (X509Certificate2 thisCertificate)
		{
			using (var chain = new MonoBtlsX509Chain ()) {
				chain.AddCertificate (x509.Copy ());
				if (intermediateCerts != null) {
					for (int i = 0; i < intermediateCerts.Count; i++) {
						var intermediate = (X509CertificateImplBtls)intermediateCerts [i];
						chain.AddCertificate (intermediate.x509.Copy ());
					}
				}
				return MonoBtlsProvider.ValidateCertificate (chain, null);
			}
		}

		public override void Reset ()
		{
			if (x509 != null) {
				x509.Dispose ();
				x509 = null;
			}
			if (nativePrivateKey != null) {
				nativePrivateKey.Dispose ();
				nativePrivateKey = null;
			}
			publicKey = null;
			intermediateCerts = null;
		}

#endregion
	}
}
#endif
