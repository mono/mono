//
// X509Certificate2ImplUnix.cs
//
// Authors:
//	Martin Baulig  <mabaul@microsoft.com>
//
// Copyright (C) 2018 Xamarin, Inc. (http://www.xamarin.com)
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
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Internal.Cryptography;
using Internal.Cryptography.Pal;
using Microsoft.Win32.SafeHandles;

#if MONO_SECURITY_ALIAS
using MX = MonoSecurity::Mono.Security.X509;
#else
using MX = Mono.Security.X509;
#endif

namespace System.Security.Cryptography.X509Certificates
{
	internal abstract class X509Certificate2ImplUnix : X509Certificate2Impl
	{
		bool readCertData;
		CertificateData certData;

		void EnsureCertData ()
		{
			if (readCertData)
				return;

			ThrowIfContextInvalid ();
			certData = new CertificateData (GetRawCertData ());
			readCertData = true;
		}

		protected abstract byte[] GetRawCertData ();

		public sealed override bool Archived {
			get {
				return false;
			}
			set {
				throw new PlatformNotSupportedException (
					SR.Format (SR.Cryptography_Unix_X509_PropertyNotSettable, nameof (Archived)));
			}
		}

		public sealed override string KeyAlgorithm {
			get {
				EnsureCertData ();
				return certData.PublicKeyAlgorithm.AlgorithmId;
			}
		}

		public sealed override byte[] KeyAlgorithmParameters {
			get {
				EnsureCertData ();
				return certData.PublicKeyAlgorithm.Parameters;
			}
		}

		public sealed override byte[] PublicKeyValue {
			get {
				EnsureCertData ();
				return certData.PublicKey;
			}
		}

		public sealed override byte[] SerialNumber {
			get {
				EnsureCertData ();
				return certData.SerialNumber;
			}
		}

		public sealed override string SignatureAlgorithm {
			get {
				EnsureCertData ();
				return certData.SignatureAlgorithm.AlgorithmId;
			}
		}

		public sealed override string FriendlyName {
			get { return ""; }
			set {
				throw new PlatformNotSupportedException (
					SR.Format (SR.Cryptography_Unix_X509_PropertyNotSettable, nameof (FriendlyName)));
			}
		}

		public sealed override int Version {
			get {
				EnsureCertData ();
				return certData.Version + 1;
			}
		}

		public sealed override X500DistinguishedName SubjectName {
			get {
				EnsureCertData ();
				return certData.Subject;
			}
		}

		public sealed override X500DistinguishedName IssuerName {
			get {
				EnsureCertData ();
				return certData.Issuer;
			}
		}

		public sealed override string Subject => SubjectName.Name;

		public sealed override string Issuer => IssuerName.Name;

		public sealed override string LegacySubject => SubjectName.Decode (X500DistinguishedNameFlags.None);

		public sealed override string LegacyIssuer => IssuerName.Decode (X500DistinguishedNameFlags.None);

		public sealed override byte[] RawData {
			get {
				EnsureCertData ();
				return certData.RawData;
			}
		}

		public sealed override byte[] Thumbprint {
			get {
				EnsureCertData ();

				using (SHA1 hash = SHA1.Create ()) {
					return hash.ComputeHash (certData.RawData);
				}
			}
		}

		public sealed override string GetNameInfo (X509NameType nameType, bool forIssuer)
		{
			EnsureCertData ();
			return certData.GetNameInfo (nameType, forIssuer);
		}

		public sealed override IEnumerable<X509Extension> Extensions {
			get {
				EnsureCertData ();
				return certData.Extensions;
			}
		}

		public sealed override DateTime NotAfter {
			get {
				EnsureCertData ();
				return certData.NotAfter.ToLocalTime ();
			}
		}

		public sealed override DateTime NotBefore {
			get {
				EnsureCertData ();
				return certData.NotBefore.ToLocalTime ();
			}
		}

		public sealed override void AppendPrivateKeyInfo (StringBuilder sb)
		{
			if (!HasPrivateKey)
				return;

			// There's nothing really to say about the key, just acknowledge there is one.
			sb.AppendLine ();
			sb.AppendLine ();
			sb.AppendLine ("[Private Key]");
		}

		public override void Reset ()
		{
			readCertData = false;
		}

		public sealed override byte[] Export (X509ContentType contentType, SafePasswordHandle password)
		{
			ThrowIfContextInvalid ();

			Debug.Assert (password != null);
			switch (contentType) {
			case X509ContentType.Cert:
				return RawData;
			case X509ContentType.Pkcs12:
				return ExportPkcs12 (password);
			case X509ContentType.Pkcs7:
				return ExportPkcs12 ((string)null);
			case X509ContentType.SerializedCert:
			case X509ContentType.SerializedStore:
				throw new PlatformNotSupportedException (SR.Cryptography_Unix_X509_SerializedExport);
			default:
				throw new CryptographicException (SR.Cryptography_X509_InvalidContentType);
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
						pfx.AddCertificate (new MX.X509Certificate (IntermediateCertificates[i].RawData));
				}
				var privateKey = PrivateKey;
				if (privateKey != null)
					pfx.AddPkcs8ShroudedKeyBag (privateKey, attrs);
				return pfx.GetBytes ();
			} finally {
				pfx.Password = null;
			}
		}
	}
}
