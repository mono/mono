//
// System.Security.Cryptography.X509Certificate2 class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell Inc. (http://www.novell.com)
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
using MonoSecurity::Mono.Security;
using MonoSecurity::Mono.Security.Cryptography;
using MX = MonoSecurity::Mono.Security.X509;
#else
using Mono.Security;
using Mono.Security.Cryptography;
using MX = Mono.Security.X509;
#endif

using System.IO;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using Microsoft.Win32.SafeHandles;
using Internal.Cryptography;
using Mono;

namespace System.Security.Cryptography.X509Certificates
{
	[Serializable]
	public class X509Certificate2 : X509Certificate
	{
		volatile byte[] lazyRawData;
		volatile Oid lazySignatureAlgorithm;
		volatile int lazyVersion;
		volatile X500DistinguishedName lazySubjectName;
		volatile X500DistinguishedName lazyIssuerName;
		volatile PublicKey lazyPublicKey;
		volatile AsymmetricAlgorithm lazyPrivateKey;
		volatile X509ExtensionCollection lazyExtensions;

		public override void Reset ()
		{
			lazyRawData = null;
			lazySignatureAlgorithm = null;
			lazyVersion = 0;
			lazySubjectName = null;
			lazyIssuerName = null;
			lazyPublicKey = null;
			lazyPrivateKey = null;
			lazyExtensions = null;

			base.Reset ();
		}

		public X509Certificate2 ()
			: base ()
		{
		}

		public X509Certificate2 (byte[] rawData)
			: base (rawData)
		{
			// MONO: temporary hack until `X509CertificateImplApple` derives from
			//       `X509Certificate2Impl`.
			if (rawData != null && rawData.Length != 0) {
				using (var safePasswordHandle = new SafePasswordHandle ((string)null)) {
					var impl = X509Helper.Import (rawData, safePasswordHandle, X509KeyStorageFlags.DefaultKeySet);
					ImportHandle (impl);
				}
			}
		}

		public X509Certificate2 (byte[] rawData, string password)
			: base (rawData, password)
		{
		}

		[CLSCompliantAttribute (false)]
		public X509Certificate2 (byte[] rawData, SecureString password)
			: base (rawData, password)
		{
		}

		public X509Certificate2 (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
			: base (rawData, password, keyStorageFlags)
		{
		}

		[CLSCompliantAttribute (false)]
		public X509Certificate2 (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
			: base (rawData, password, keyStorageFlags)
		{
		}

		public X509Certificate2 (IntPtr handle)
		    : base (handle)
		{
		}

		internal X509Certificate2 (X509Certificate2Impl impl)
		    : base (impl)
		{
		}

		public X509Certificate2 (string fileName)
			: base (fileName)
		{
		}

		public X509Certificate2 (string fileName, string password)
			: base (fileName, password)
		{
		}

		public X509Certificate2 (string fileName, SecureString password)
			: base (fileName, password)
		{
		}

		public X509Certificate2 (string fileName, string password, X509KeyStorageFlags keyStorageFlags)
			: base (fileName, password, keyStorageFlags)
		{
		}

		public X509Certificate2 (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
			: base (fileName, password, keyStorageFlags)
		{
		}

		public X509Certificate2 (X509Certificate certificate) 
			: base (certificate)
		{
		}

		protected X509Certificate2 (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			throw new PlatformNotSupportedException ();
		}

		public bool Archived {
			get {
				ThrowIfInvalid ();
				return Impl.Archived;
			}

			set {
				ThrowIfInvalid ();
				Impl.Archived = value;
			}
		}

		public X509ExtensionCollection Extensions {
			get {
				ThrowIfInvalid ();

				X509ExtensionCollection extensions = lazyExtensions;
				if (extensions == null) {
					extensions = new X509ExtensionCollection ();
					foreach (X509Extension extension in Impl.Extensions) {
						X509Extension customExtension = CreateCustomExtensionIfAny (extension.Oid);
						if (customExtension == null) {
							extensions.Add (extension);
						} else {
							customExtension.CopyFrom (extension);
							extensions.Add (customExtension);
						}
					}
					lazyExtensions = extensions;
				}
				return extensions;
			}
		}

		public string FriendlyName {
			get {
				ThrowIfInvalid ();
				return Impl.FriendlyName;
			}

			set {
				ThrowIfInvalid ();
				Impl.FriendlyName = value;
			}
		}

		public bool HasPrivateKey {
			get {
				ThrowIfInvalid ();
				return Impl.HasPrivateKey;
			}
		}

		public AsymmetricAlgorithm PrivateKey {
			get {
				ThrowIfInvalid ();

				if (!HasPrivateKey)
					return null;

				if (lazyPrivateKey == null) {
					switch (GetKeyAlgorithm ()) {
					case Oids.RsaRsa:
						lazyPrivateKey = Impl.GetRSAPrivateKey ();
						break;
					case Oids.DsaDsa:
						lazyPrivateKey = Impl.GetDSAPrivateKey ();
						break;
					default:
						// This includes ECDSA, because an Oids.Ecc key can be
						// many different algorithm kinds, not necessarily with mutual exclusion.
						//
						// Plus, .NET Framework only supports RSA and DSA in this property.
						throw new NotSupportedException (SR.NotSupported_KeyAlgorithm);
					}
				}

				return lazyPrivateKey;
			}
			set {
				throw new PlatformNotSupportedException ();
			}
		}

		public X500DistinguishedName IssuerName {
			get {
				ThrowIfInvalid ();

				X500DistinguishedName issuerName = lazyIssuerName;
				if (issuerName == null)
					issuerName = lazyIssuerName = Impl.IssuerName;
				return issuerName;
			}
		}

		public DateTime NotAfter {
			get { return GetNotAfter (); }
		}

		public DateTime NotBefore {
			get { return GetNotBefore (); }
		}

		public PublicKey PublicKey {
			get {
				ThrowIfInvalid ();

				PublicKey publicKey = lazyPublicKey;
				if (publicKey == null) {
					string keyAlgorithmOid = GetKeyAlgorithm ();
					byte[] parameters = GetKeyAlgorithmParameters ();
					byte[] keyValue = GetPublicKey ();
					Oid oid = new Oid (keyAlgorithmOid);
					publicKey = lazyPublicKey = new PublicKey (oid, new AsnEncodedData (oid, parameters), new AsnEncodedData (oid, keyValue));
				}
				return publicKey;
			}
		}

		public byte[] RawData {
			get {
				ThrowIfInvalid ();

				byte[] rawData = lazyRawData;
				if (rawData == null)
					rawData = lazyRawData = Impl.RawData;
				return rawData.CloneByteArray ();
			}
		}

		public string SerialNumber {
			get {
				return GetSerialNumberString ();
			}
		}

		public Oid SignatureAlgorithm {
			get {
				ThrowIfInvalid ();

				Oid signatureAlgorithm = lazySignatureAlgorithm;
				if (signatureAlgorithm == null) {
					string oidValue = Impl.SignatureAlgorithm;
					signatureAlgorithm = lazySignatureAlgorithm = Oid.FromOidValue (oidValue, OidGroup.SignatureAlgorithm);
				}
				return signatureAlgorithm;
			}
		}

		public X500DistinguishedName SubjectName {
			get {
				ThrowIfInvalid ();

				X500DistinguishedName subjectName = lazySubjectName;
				if (subjectName == null)
					subjectName = lazySubjectName = Impl.SubjectName;
				return subjectName;
			}
		}

		public string Thumbprint {
			get {
				byte[] thumbPrint = GetCertHash ();
				return thumbPrint.ToHexStringUpper ();
			}
		}

		public int Version {
			get {
				ThrowIfInvalid ();

				int version = lazyVersion;
				if (version == 0)
					version = lazyVersion = Impl.Version;
				return version;
			}
		}

		public static X509ContentType GetCertContentType (byte[] rawData)
		{
			if (rawData == null || rawData.Length == 0)
				throw new ArgumentException (SR.Arg_EmptyOrNullArray, nameof (rawData));

			return X509Pal.Instance.GetCertContentType (rawData);
		}

		public static X509ContentType GetCertContentType (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			// Desktop compat: The desktop CLR expands the filename to a full path for the purpose of performing a CAS permission check. While CAS is not present here,
			// we still need to call GetFullPath() so we get the same exception behavior if the fileName is bad.
			string fullPath = Path.GetFullPath (fileName);

			return X509Pal.Instance.GetCertContentType (fileName);
		}

		public string GetNameInfo (X509NameType nameType, bool forIssuer)
		{
			return Impl.GetNameInfo (nameType, forIssuer);
		}

		public override string ToString ()
		{
			return base.ToString (fVerbose: true);
		}

		public override string ToString (bool verbose)
		{
			if (verbose == false || !IsValid)
				return ToString ();

			StringBuilder sb = new StringBuilder ();

			// Version
			sb.AppendLine ("[Version]");
			sb.Append ("  V");
			sb.Append (Version);

			// Subject
			sb.AppendLine ();
			sb.AppendLine ();
			sb.AppendLine ("[Subject]");
			sb.Append ("  ");
			sb.Append (SubjectName.Name);
			string simpleName = GetNameInfo (X509NameType.SimpleName, false);
			if (simpleName.Length > 0) {
				sb.AppendLine ();
				sb.Append ("  ");
				sb.Append ("Simple Name: ");
				sb.Append (simpleName);
			}
			string emailName = GetNameInfo (X509NameType.EmailName, false);
			if (emailName.Length > 0) {
				sb.AppendLine ();
				sb.Append ("  ");
				sb.Append ("Email Name: ");
				sb.Append (emailName);
			}
			string upnName = GetNameInfo (X509NameType.UpnName, false);
			if (upnName.Length > 0) {
				sb.AppendLine ();
				sb.Append ("  ");
				sb.Append ("UPN Name: ");
				sb.Append (upnName);
			}
			string dnsName = GetNameInfo (X509NameType.DnsName, false);
			if (dnsName.Length > 0) {
				sb.AppendLine ();
				sb.Append ("  ");
				sb.Append ("DNS Name: ");
				sb.Append (dnsName);
			}

			// Issuer
			sb.AppendLine ();
			sb.AppendLine ();
			sb.AppendLine ("[Issuer]");
			sb.Append ("  ");
			sb.Append (IssuerName.Name);
			simpleName = GetNameInfo (X509NameType.SimpleName, true);
			if (simpleName.Length > 0) {
				sb.AppendLine ();
				sb.Append ("  ");
				sb.Append ("Simple Name: ");
				sb.Append (simpleName);
			}
			emailName = GetNameInfo (X509NameType.EmailName, true);
			if (emailName.Length > 0) {
				sb.AppendLine ();
				sb.Append ("  ");
				sb.Append ("Email Name: ");
				sb.Append (emailName);
			}
			upnName = GetNameInfo (X509NameType.UpnName, true);
			if (upnName.Length > 0) {
				sb.AppendLine ();
				sb.Append ("  ");
				sb.Append ("UPN Name: ");
				sb.Append (upnName);
			}
			dnsName = GetNameInfo (X509NameType.DnsName, true);
			if (dnsName.Length > 0) {
				sb.AppendLine ();
				sb.Append ("  ");
				sb.Append ("DNS Name: ");
				sb.Append (dnsName);
			}

			// Serial Number
			sb.AppendLine ();
			sb.AppendLine ();
			sb.AppendLine ("[Serial Number]");
			sb.Append ("  ");
			sb.AppendLine (SerialNumber);

			// NotBefore
			sb.AppendLine ();
			sb.AppendLine ("[Not Before]");
			sb.Append ("  ");
			sb.AppendLine (FormatDate (NotBefore));

			// NotAfter
			sb.AppendLine ();
			sb.AppendLine ("[Not After]");
			sb.Append ("  ");
			sb.AppendLine (FormatDate (NotAfter));

			// Thumbprint
			sb.AppendLine ();
			sb.AppendLine ("[Thumbprint]");
			sb.Append ("  ");
			sb.AppendLine (Thumbprint);

			// Signature Algorithm
			sb.AppendLine ();
			sb.AppendLine ("[Signature Algorithm]");
			sb.Append ("  ");
			sb.Append (SignatureAlgorithm.FriendlyName);
			sb.Append ('(');
			sb.Append (SignatureAlgorithm.Value);
			sb.AppendLine (")");

			// Public Key
			sb.AppendLine ();
			sb.Append ("[Public Key]");
			// It could throw if it's some user-defined CryptoServiceProvider
			try {
				PublicKey pubKey = PublicKey;

				sb.AppendLine ();
				sb.Append ("  ");
				sb.Append ("Algorithm: ");
				sb.Append (pubKey.Oid.FriendlyName);
				// So far, we only support RSACryptoServiceProvider & DSACryptoServiceProvider Keys
				try {
					sb.AppendLine ();
					sb.Append ("  ");
					sb.Append ("Length: ");

					using (RSA pubRsa = this.GetRSAPublicKey ()) {
						if (pubRsa != null) {
							sb.Append (pubRsa.KeySize);
						}
					}
				} catch (NotSupportedException) {
				}

				sb.AppendLine ();
				sb.Append ("  ");
				sb.Append ("Key Blob: ");
				sb.AppendLine (pubKey.EncodedKeyValue.Format (true));

				sb.Append ("  ");
				sb.Append ("Parameters: ");
				sb.Append (pubKey.EncodedParameters.Format (true));
			} catch (CryptographicException) {
			}

			// Private key
			Impl.AppendPrivateKeyInfo (sb);

			// Extensions
			X509ExtensionCollection extensions = Extensions;
			if (extensions.Count > 0) {
				sb.AppendLine ();
				sb.AppendLine ();
				sb.Append ("[Extensions]");
				foreach (X509Extension extension in extensions) {
					try {
						sb.AppendLine ();
						sb.Append ("* ");
						sb.Append (extension.Oid.FriendlyName);
						sb.Append ('(');
						sb.Append (extension.Oid.Value);
						sb.Append ("):");

						sb.AppendLine ();
						sb.Append ("  ");
						sb.Append (extension.Format (true));
					} catch (CryptographicException) {
					}
				}
			}

			sb.AppendLine ();
			return sb.ToString ();
		}

		public override void Import (byte[] rawData)
		{
			base.Import (rawData);
		}

		public override void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			base.Import (rawData, password, keyStorageFlags);
		}

		[CLSCompliantAttribute (false)]
		public override void Import (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			base.Import (rawData, password, keyStorageFlags);
		}

		public override void Import (string fileName)
		{
			base.Import (fileName);
		}

		public override void Import (string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		{
			base.Import (fileName, password, keyStorageFlags);
		}

		[CLSCompliantAttribute (false)]
		public override void Import (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			base.Import (fileName, password, keyStorageFlags);
		}

		#region Mono Implementation

		public bool Verify ()
		{
			return Impl.Verify (this);
		}

		#endregion

		static X509Extension CreateCustomExtensionIfAny (Oid oid)
		{
			string oidValue = oid.Value;
			switch (oidValue) {
			case Oids.BasicConstraints:
				return X509Pal.Instance.SupportsLegacyBasicConstraintsExtension ?
				    new X509BasicConstraintsExtension () :
				    null;

			case Oids.BasicConstraints2:
				return new X509BasicConstraintsExtension ();

			case Oids.KeyUsage:
				return new X509KeyUsageExtension ();

			case Oids.EnhancedKeyUsage:
				return new X509EnhancedKeyUsageExtension ();

			case Oids.SubjectKeyIdentifier:
				return new X509SubjectKeyIdentifierExtension ();

			default:
				return null;
			}
		}

		//
		// MARTIN CHECK POINT
		//

		new internal X509Certificate2Impl Impl {
			get {
				var impl2 = base.Impl as X509Certificate2Impl;
				X509Helper.ThrowIfContextInvalid (impl2);
				return impl2;
			}
		}
	}
}
