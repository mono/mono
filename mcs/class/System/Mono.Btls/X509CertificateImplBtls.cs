﻿//
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
#else
using MX = Mono.Security.X509;
#endif

using System;
using System.Text;
using System.Collections;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using Mono.Security.Cryptography;
using Microsoft.Win32.SafeHandles;

namespace Mono.Btls
{
	class X509CertificateImplBtls : X509Certificate2Impl
	{
		MonoBtlsX509 x509;
		MonoBtlsKey nativePrivateKey;
		X500DistinguishedName subjectName;
		X500DistinguishedName issuerName;
		X509CertificateImplCollection intermediateCerts;
		PublicKey publicKey;
		bool archived;
		bool disallowFallback;

		internal X509CertificateImplBtls (bool disallowFallback = false)
		{
			this.disallowFallback = disallowFallback;
		}

		internal X509CertificateImplBtls (MonoBtlsX509 x509, bool disallowFallback = false)
		{
			this.disallowFallback = disallowFallback;
			this.x509 = x509.Copy ();
		}

		X509CertificateImplBtls (X509CertificateImplBtls other)
		{
			disallowFallback = other.disallowFallback;
			x509 = other.x509 != null ? other.x509.Copy () : null;
			nativePrivateKey = other.nativePrivateKey != null ? other.nativePrivateKey.Copy () : null;
			fallback = other.fallback != null ? (X509Certificate2Impl)other.fallback.Clone () : null;
			if (other.intermediateCerts != null)
				intermediateCerts = other.intermediateCerts.Clone ();
		}

		internal X509CertificateImplBtls (byte[] data, MonoBtlsX509Format format, bool disallowFallback = false)
		{
			this.disallowFallback = disallowFallback;
			x509 = MonoBtlsX509.LoadFromData (data, format);
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
				if (nativePrivateKey == null && FallbackImpl.HasPrivateKey) {
					var key = FallbackImpl.PrivateKey as RSA;
					if (key == null)
						throw new NotSupportedException ("Currently only supports RSA private keys.");
					nativePrivateKey = MonoBtlsKey.CreateFromRSAPrivateKey (key);
				}
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

		public override byte[] Thumbprint => X509.GetCertHash ();

		public override byte[] RawData => X509.GetRawData (MonoBtlsX509Format.DER);

		public override string Subject => SubjectName.Name;

		public override string Issuer => IssuerName.Name;

		public override string LegacySubject => SubjectName.Decode (X500DistinguishedNameFlags.None);

		public override string LegacyIssuer => IssuerName.Decode (X500DistinguishedNameFlags.None);

		public override DateTime NotBefore => X509.GetNotBefore ().ToLocalTime ();

		public override DateTime NotAfter => X509.GetNotAfter ().ToLocalTime ();

		public override byte[] PublicKeyValue => X509.GetPublicKeyData ();

		public override byte[] SerialNumber => X509.GetSerialNumber (true);

		public override string KeyAlgorithm => PublicKey.Oid.Value;

		public override byte[] KeyAlgorithmParameters => PublicKey.EncodedParameters.RawData;

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

		X509Certificate2Impl fallback;

		void MustFallback ()
		{
			if (disallowFallback)
				throw new InvalidOperationException ();
			if (fallback != null)
				return;
			fallback = SystemDependencyProvider.Instance.CertificateProvider.Import (
				RawData, null, X509KeyStorageFlags.DefaultKeySet,
				CertificateImportFlags.DisableNativeBackend);
		}

		internal override X509Certificate2Impl FallbackImpl {
			get {
				MustFallback ();
				return fallback;
			}
		}

		[MonoTODO]
		public override bool Archived {
			get {
				ThrowIfContextInvalid ();
				return archived;
			}
			set {
				ThrowIfContextInvalid ();
				archived = value;
			}
		}

		public override X509ExtensionCollection Extensions {
			get { return FallbackImpl.Extensions; }
		}

		public override bool HasPrivateKey {
			get { return nativePrivateKey != null || FallbackImpl.HasPrivateKey; }
		}

		public override X500DistinguishedName IssuerName {
			get {
				ThrowIfContextInvalid ();
				if (issuerName == null) {
					using (var xname = x509.GetIssuerName ()) {
						var encoding = xname.GetRawData (false);
						var canonEncoding = xname.GetRawData (true);
						var name = MonoBtlsUtils.FormatName (xname, true, ", ", true);
						issuerName = new X500DistinguishedName (encoding, canonEncoding, name);
					}
				}
				return issuerName;
			}
		}

		public override AsymmetricAlgorithm PrivateKey {
			get {
				if (nativePrivateKey == null || !nativePrivateKey.IsRsa)
					return FallbackImpl.PrivateKey;
				var bytes = nativePrivateKey.GetBytes (true);
				return PKCS8.PrivateKeyInfo.DecodeRSA (bytes);
			}
			set {
				if (nativePrivateKey != null)
					nativePrivateKey.Dispose ();
				nativePrivateKey = null;
				FallbackImpl.PrivateKey = value;
			}
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

		public override Oid SignatureAlgorithm {
			get {
				ThrowIfContextInvalid ();
				return X509.GetSignatureAlgorithm ();
			}
		}

		public override X500DistinguishedName SubjectName {
			get {
				ThrowIfContextInvalid ();
				if (subjectName == null) {
					using (var xname = x509.GetSubjectName ()) {
						var encoding = xname.GetRawData (false);
						var canonEncoding = xname.GetRawData (true);
						var name = MonoBtlsUtils.FormatName (xname, true, ", ", true);
						subjectName = new X500DistinguishedName (encoding, canonEncoding, name);
					}
				}
				return subjectName;
			}
		}

		public override int Version {
			get { return X509.GetVersion (); }
		}

		public override string GetNameInfo (X509NameType nameType, bool forIssuer)
		{
			return FallbackImpl.GetNameInfo (nameType, forIssuer);
		}

		public override void Import (byte[] data, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags)
		{
			Reset ();
			if (password == null || password.IsInvalid) {
				try {
					Import (data);
				} catch (Exception e) {
					try {
						 ImportPkcs12 (data, null);
					} catch {
						string msg = Locale.GetText ("Unable to decode certificate.");
						// inner exception is the original (not second) exception
						throw new CryptographicException (msg, e);
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
						string msg = Locale.GetText ("Unable to decode certificate.");
						// inner exception is the original (not second) exception
						throw new CryptographicException (msg, e);
					}
				}
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
							var impl = new X509CertificateImplBtls (ic, true);
							intermediateCerts.Add (impl, true);
						}
					}
				}
			}
		}

		public override byte[] Export (X509ContentType contentType, SafePasswordHandle password)
		{
			ThrowIfContextInvalid ();

			switch (contentType) {
			case X509ContentType.Cert:
				return RawData;
			case X509ContentType.Pfx: // this includes Pkcs12
				return ExportPkcs12 (password);
			case X509ContentType.SerializedCert:
				// TODO
				throw new NotSupportedException ();
			default:
				string msg = Locale.GetText ("This certificate format '{0}' cannot be exported.", contentType);
				throw new CryptographicException (msg);
			}
		}

		byte[] ExportPkcs12 (SafePasswordHandle password)
		{
			if (password == null || password.IsInvalid)
				return ExportPkcs12 ((string)null);
			var passwordString = password.Mono_DangerousGetString ();
			return ExportPkcs12 (passwordString);
		}

		byte[] ExportPkcs12 (string password)
		{
			var pfx = new MX.PKCS12 ();
			try {
				var attrs = new Hashtable ();
				var localKeyId = new ArrayList ();
				localKeyId.Add (new byte[] { 1, 0, 0, 0 });
				attrs.Add (MX.PKCS9.localKeyId, localKeyId);
				if (password != null)
					pfx.Password = password;
				pfx.AddCertificate (new MX.X509Certificate (RawData), attrs);
				if (IntermediateCertificates != null) {
					for (int i = 0; i < IntermediateCertificates.Count; i++)
						pfx.AddCertificate (new MX.X509Certificate (IntermediateCertificates [i].RawData));
				}
				var privateKey = PrivateKey;
				if (privateKey != null)
					pfx.AddPkcs8ShroudedKeyBag (privateKey, attrs);
				return pfx.GetBytes ();
			} finally {
				pfx.Password = null;
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
			subjectName = null;
			issuerName = null;
			archived = false;
			publicKey = null;
			intermediateCerts = null;
			if (fallback != null)
				fallback.Reset ();
		}

#endregion
	}
}
#endif
