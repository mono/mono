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

#if SECURITY_DEP

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

#endif

using System.IO;
using System.Text;
using System.Collections;

namespace System.Security.Cryptography.X509Certificates {

	[Serializable]
	public class X509Certificate2 : X509Certificate {
	
#if !SECURITY_DEP
		// Used in Mono.Security HttpsClientStream
		public X509Certificate2 (byte[] rawData)
		{
		}
#endif
#if SECURITY_DEP
		new internal X509Certificate2Impl Impl {
			get {
				var impl2 = base.Impl as X509Certificate2Impl;
				X509Helper2.ThrowIfContextInvalid (impl2);
				return impl2;
			}
		}

		string friendlyName = string.Empty;

		// constructors

		public X509Certificate2 ()
		{
		}

		public X509Certificate2 (byte[] rawData)
		{
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (byte[] rawData, string password)
		{
			Import (rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (byte[] rawData, SecureString password)
		{
			Import (rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (rawData, password, keyStorageFlags);
		}

		public X509Certificate2 (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (rawData, password, keyStorageFlags);
		}

		public X509Certificate2 (string fileName)
		{
			Import (fileName, String.Empty, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (string fileName, string password)
		{
			Import (fileName, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (string fileName, SecureString password)
		{
			Import (fileName, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (fileName, password, keyStorageFlags);
		}

		public X509Certificate2 (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (fileName, password, keyStorageFlags);
		}

		public X509Certificate2 (IntPtr handle) : base (handle) 
		{
			throw new NotImplementedException ();
		}

		public X509Certificate2 (X509Certificate certificate) 
			: base (X509Helper2.Import (certificate))
		{
		}

		internal X509Certificate2 (X509Certificate2Impl impl)
			: base (impl)
		{
		}

		// properties

		public bool Archived {
			get { return Impl.Archived; }
			set { Impl.Archived = true; }
		}

		public X509ExtensionCollection Extensions {
			get { return Impl.Extensions; }
		}

		public string FriendlyName {
			get {
				ThrowIfContextInvalid ();
				return friendlyName;
			}
			set {
				ThrowIfContextInvalid ();
				friendlyName = value;
			}
		}

		public bool HasPrivateKey {
			get { return Impl.HasPrivateKey; }
		}

		public X500DistinguishedName IssuerName {
			get { return Impl.IssuerName; }
		} 

		public DateTime NotAfter {
			get { return Impl.GetValidUntil ().ToLocalTime (); }
		}

		public DateTime NotBefore {
			get { return Impl.GetValidFrom ().ToLocalTime (); }
		}

		public AsymmetricAlgorithm PrivateKey {
			get { return Impl.PrivateKey; }
			set { Impl.PrivateKey = value; }
		} 

		public PublicKey PublicKey {
			get { return Impl.PublicKey; }
		} 

		public byte[] RawData {
			get { return GetRawCertData (); }
		}

		public string SerialNumber {
			get { return GetSerialNumberString (); }
		} 

		public Oid SignatureAlgorithm {
			get { return Impl.SignatureAlgorithm; }
		} 

		public X500DistinguishedName SubjectName {
			get { return Impl.SubjectName; }
		} 

		public string Thumbprint {
			get { return GetCertHashString (); }
		} 

		public int Version {
			get { return Impl.Version; }
		}

		// methods

		[MonoTODO ("always return String.Empty for UpnName, DnsFromAlternativeName and UrlName")]
		public string GetNameInfo (X509NameType nameType, bool forIssuer) 
		{
			return Impl.GetNameInfo (nameType, forIssuer);
		}

		public override void Import (byte[] rawData) 
		{
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		[MonoTODO ("missing KeyStorageFlags support")]
		public override void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			var impl = X509Helper2.Import (rawData, password, keyStorageFlags);
			ImportHandle (impl);
		}

		[MonoTODO ("SecureString is incomplete")]
		public override void Import (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (rawData, (string) null, keyStorageFlags);
		}

		public override void Import (string fileName) 
		{
			byte[] rawData = File.ReadAllBytes (fileName);
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		[MonoTODO ("missing KeyStorageFlags support")]
		public override void Import (string fileName, string password, X509KeyStorageFlags keyStorageFlags) 
		{
			byte[] rawData = File.ReadAllBytes (fileName);
			Import (rawData, password, keyStorageFlags);
		}

		[MonoTODO ("SecureString is incomplete")]
		public override void Import (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags) 
		{
			byte[] rawData = File.ReadAllBytes (fileName);
			Import (rawData, (string)null, keyStorageFlags);
		}

		[MonoTODO ("X509ContentType.SerializedCert is not supported")]
		public override byte[] Export (X509ContentType contentType, string password)
		{
			return Impl.Export (contentType, password);
		}

		public override void Reset () 
		{
			friendlyName = string.Empty;
			base.Reset ();
		}

		public override string ToString ()
		{
			if (!IsValid)
				return "System.Security.Cryptography.X509Certificates.X509Certificate2";
			return base.ToString (true);
		}

		public override string ToString (bool verbose)
		{
			if (!IsValid)
				return "System.Security.Cryptography.X509Certificates.X509Certificate2";

			// the non-verbose X509Certificate2 == verbose X509Certificate
			if (!verbose)
				return base.ToString (true);

			string nl = Environment.NewLine;
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("[Version]{0}  V{1}{0}{0}", nl, Version);
			sb.AppendFormat ("[Subject]{0}  {1}{0}{0}", nl, Subject);
			sb.AppendFormat ("[Issuer]{0}  {1}{0}{0}", nl, Issuer);
			sb.AppendFormat ("[Serial Number]{0}  {1}{0}{0}", nl, SerialNumber);
			sb.AppendFormat ("[Not Before]{0}  {1}{0}{0}", nl, NotBefore);
			sb.AppendFormat ("[Not After]{0}  {1}{0}{0}", nl, NotAfter);
			sb.AppendFormat ("[Thumbprint]{0}  {1}{0}{0}", nl, Thumbprint);
			sb.AppendFormat ("[Signature Algorithm]{0}  {1}({2}){0}{0}", nl, SignatureAlgorithm.FriendlyName, 
				SignatureAlgorithm.Value);

			AsymmetricAlgorithm key = PublicKey.Key;
			sb.AppendFormat ("[Public Key]{0}  Algorithm: ", nl);
			if (key is RSA)
				sb.Append ("RSA");
			else if (key is DSA)
				sb.Append ("DSA");
			else
				sb.Append (key.ToString ());
			sb.AppendFormat ("{0}  Length: {1}{0}  Key Blob: ", nl, key.KeySize);
			AppendBuffer (sb, PublicKey.EncodedKeyValue.RawData);
			sb.AppendFormat ("{0}  Parameters: ", nl);
			AppendBuffer (sb, PublicKey.EncodedParameters.RawData);
			sb.Append (nl);

			return sb.ToString ();
		}

		private static void AppendBuffer (StringBuilder sb, byte[] buffer)
		{
			if (buffer == null)
				return;
			for (int i=0; i < buffer.Length; i++) {
				sb.Append (buffer [i].ToString ("x2"));
				if (i < buffer.Length - 1)
					sb.Append (" ");
			}
		}

		[MonoTODO ("by default this depends on the incomplete X509Chain")]
		public bool Verify ()
		{
			return Impl.Verify (this);
		}

		// static methods

		private static byte[] signedData = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x07, 0x02 };

		[MonoTODO ("Detection limited to Cert, Pfx, Pkcs12, Pkcs7 and Unknown")]
		public static X509ContentType GetCertContentType (byte[] rawData)
		{
			if ((rawData == null) || (rawData.Length == 0))
				throw new ArgumentException ("rawData");

			X509ContentType type = X509ContentType.Unknown;
			try {
				ASN1 data = new ASN1 (rawData);
				if (data.Tag != 0x30) {
					string msg = Locale.GetText ("Unable to decode certificate.");
					throw new CryptographicException (msg);
				}

				if (data.Count == 0)
					return type;

				if (data.Count == 3) {
					switch (data [0].Tag) {
					case 0x30:
						// SEQUENCE / SEQUENCE / BITSTRING
						if ((data [1].Tag == 0x30) && (data [2].Tag == 0x03))
							type = X509ContentType.Cert;
						break;
					case 0x02:
						// INTEGER / SEQUENCE / SEQUENCE
						if ((data [1].Tag == 0x30) && (data [2].Tag == 0x30))
							type = X509ContentType.Pkcs12;
						// note: Pfx == Pkcs12
						break;
					}
				}
				// check for PKCS#7 (count unknown but greater than 0)
				// SEQUENCE / OID (signedData)
				if ((data [0].Tag == 0x06) && data [0].CompareValue (signedData))
					type = X509ContentType.Pkcs7;
			}
			catch (Exception e) {
				string msg = Locale.GetText ("Unable to decode certificate.");
				throw new CryptographicException (msg, e);
			}

			return type;
		}

		[MonoTODO ("Detection limited to Cert, Pfx, Pkcs12 and Unknown")]
		public static X509ContentType GetCertContentType (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");
			if (fileName.Length == 0)
				throw new ArgumentException ("fileName");

			byte[] data = File.ReadAllBytes (fileName);
			return GetCertContentType (data);
		}

		// internal stuff because X509Certificate2 isn't complete enough
		// (maybe X509Certificate3 will be better?)

		[Obsolete ("KILL")]
		internal MX.X509Certificate MonoCertificate {
			get {
				var monoImpl = Impl as X509Certificate2ImplMono;
				if (monoImpl == null)
					throw new NotSupportedException ();
				return monoImpl.MonoCertificate;
			}
		}

#else
		// HACK - this ensure the type X509Certificate2 and PrivateKey property exists in the build before
		// Mono.Security.dll is built. This is required to get working client certificate in SSL/TLS
		public AsymmetricAlgorithm PrivateKey {
			get { return null; }
		}
#endif
	}
}
