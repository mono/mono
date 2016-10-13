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
#if SECURITY_DEP
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
using Mono.Security.Cryptography;

namespace Mono.Btls
{
	class X509CertificateImplBtls : X509Certificate2Impl
	{
		MonoBtlsX509 x509;
		MonoBtlsKey privateKey;
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
			privateKey = other.privateKey != null ? other.privateKey.Copy () : null;
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
				return privateKey;
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

		protected override byte[] GetCertHash (bool lazy)
		{
			return X509.GetCertHash ();
		}

		public override byte[] GetRawCertData ()
		{
			return X509.GetRawData (MonoBtlsX509Format.DER);
		}

		public override string GetSubjectName (bool legacyV1Mode)
		{
			if (legacyV1Mode)
				return SubjectName.Decode (X500DistinguishedNameFlags.None);
			return SubjectName.Name;
		}

		public override string GetIssuerName (bool legacyV1Mode)
		{
			if (legacyV1Mode)
				return IssuerName.Decode (X500DistinguishedNameFlags.None);
			return IssuerName.Name;
		}

		public override DateTime GetValidFrom ()
		{
			return X509.GetNotBefore ().ToLocalTime ();
		}

		public override DateTime GetValidUntil ()
		{
			return X509.GetNotAfter ().ToLocalTime ();
		}

		public override byte[] GetPublicKey ()
		{
			return X509.GetPublicKeyData ();
		}

		public override byte[] GetSerialNumber ()
		{
			return X509.GetSerialNumber (true);
		}

		public override string GetKeyAlgorithm ()
		{
			return PublicKey.Oid.Value;
		}

		public override byte[] GetKeyAlgorithmParameters ()
		{
			return PublicKey.EncodedParameters.RawData;
		}

		public override byte[] Export (X509ContentType contentType, byte[] password)
		{
			ThrowIfContextInvalid ();

			switch (contentType) {
			case X509ContentType.Cert:
				return GetRawCertData ();
			case X509ContentType.Pfx: // this includes Pkcs12
				// TODO
				throw new NotSupportedException ();
			case X509ContentType.SerializedCert:
				// TODO
				throw new NotSupportedException ();
			default:
				string msg = Locale.GetText ("This certificate format '{0}' cannot be exported.", contentType);
				throw new CryptographicException (msg);
			}
		}

		internal override X509CertificateImplCollection IntermediateCertificates {
			get { return intermediateCerts; }
		}

		public override string ToString (bool full)
		{
			ThrowIfContextInvalid ();

			if (!full) {
				var summary = GetSubjectName (false);
				return string.Format ("[X509Certificate: {0}]", summary);
			}

			string nl = Environment.NewLine;
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("[Subject]{0}  {1}{0}{0}", nl, GetSubjectName (false));

			sb.AppendFormat ("[Issuer]{0}  {1}{0}{0}", nl, GetIssuerName (false));
			sb.AppendFormat ("[Not Before]{0}  {1}{0}{0}", nl, GetValidFrom ().ToLocalTime ());
			sb.AppendFormat ("[Not After]{0}  {1}{0}{0}", nl, GetValidUntil ().ToLocalTime ());
			sb.AppendFormat ("[Thumbprint]{0}  {1}{0}", nl, X509Helper.ToHexString (GetCertHash ()));

			sb.Append (nl);
			return sb.ToString ();
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
			fallback = X509Helper2.Import (GetRawCertData (), null, X509KeyStorageFlags.DefaultKeySet, true);
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
			get { return privateKey != null; }
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
				if (privateKey == null || !privateKey.IsRsa)
					return null;
				var bytes = privateKey.GetBytes (true);
				return PKCS8.PrivateKeyInfo.DecodeRSA (bytes);
			}
			set { FallbackImpl.PrivateKey = value; }
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

		public override void Import (byte[] data, string password, X509KeyStorageFlags keyStorageFlags)
		{
			if (password == null) {
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
			// Does it look like PEM?
			if ((data.Length > 0) && (data [0] != 0x30))
				x509 = MonoBtlsX509.LoadFromData (data, MonoBtlsX509Format.PEM);
			else
				x509 = MonoBtlsX509.LoadFromData (data, MonoBtlsX509Format.DER);
		}

		void ImportPkcs12 (byte[] data, string password)
		{
			using (var pkcs12 = new MonoBtlsPkcs12 ()) {
				if (string.IsNullOrEmpty (password)) {
					try {
						// Support both unencrypted PKCS#12..
						pkcs12.Import (data, null);
					} catch {
						// ..and PKCS#12 encrypted with an empty password
						pkcs12.Import (data, string.Empty);
					}
				} else {
					pkcs12.Import (data, password);
				}

				x509 = pkcs12.GetCertificate (0);
				if (pkcs12.HasPrivateKey)
					privateKey = pkcs12.GetPrivateKey ();
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

		public override byte[] Export (X509ContentType contentType, string password)
		{
			ThrowIfContextInvalid ();

			switch (contentType) {
			case X509ContentType.Cert:
				return GetRawCertData ();
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
				pfx.AddCertificate (new MX.X509Certificate (GetRawCertData ()), attrs);
				if (IntermediateCertificates != null) {
					for (int i = 0; i < IntermediateCertificates.Count; i++)
						pfx.AddCertificate (new MX.X509Certificate (IntermediateCertificates [i].GetRawCertData ()));
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
			if (privateKey != null) {
				privateKey = null;
				privateKey = null;
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
