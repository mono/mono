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

#if NET_2_0

using System.IO;
using System.Text;
#if SECURITY_DEP || MOONLIGHT
using Mono.Security;
using Mono.Security.Cryptography;
using MX = Mono.Security.X509;
#endif

namespace System.Security.Cryptography.X509Certificates {

	public class X509Certificate2 : X509Certificate {
#if !SECURITY_DEP && !MOONLIGHT
		// Used in Mono.Security HttpsClientStream
		public X509Certificate2 (byte[] rawData)
		{
		}
#endif
#if SECURITY_DEP || MOONLIGHT
		private bool _archived;
		private X509ExtensionCollection _extensions;
		private string _name = String.Empty;
		private string _serial;
		private PublicKey _publicKey;
		private X500DistinguishedName issuer_name;
		private X500DistinguishedName subject_name;
		private Oid signature_algorithm;

		private MX.X509Certificate _cert;

		private static string empty_error = Locale.GetText ("Certificate instance is empty.");

		// constructors

		public X509Certificate2 ()
		{
			_cert = null;
		}

		public X509Certificate2 (byte[] rawData)
		{
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (byte[] rawData, string password)
		{
			Import (rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}
#if !MOONLIGHT
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
#endif
		public X509Certificate2 (IntPtr handle) : base (handle) 
		{
			_cert = new MX.X509Certificate (base.GetRawCertData ());
		}

		public X509Certificate2 (X509Certificate certificate) 
			: base (certificate)
		{
			_cert = new MX.X509Certificate (base.GetRawCertData ());
		}

		// properties

		public bool Archived {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				return _archived;
			}
			set {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				_archived = value;
			}
		}

		public X509ExtensionCollection Extensions {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				if (_extensions == null)
					_extensions = new X509ExtensionCollection (_cert);
				return _extensions;
			}
		}

		public string FriendlyName {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				return _name;
			}
			set {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				_name = value;
			}
		}

		// FIXME - Could be more efficient
		public bool HasPrivateKey {
			get { return PrivateKey != null; }
		}

		public X500DistinguishedName IssuerName {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				if (issuer_name == null)
					issuer_name = new X500DistinguishedName (_cert.GetIssuerName ().GetBytes ());
				return issuer_name;
			}
		} 

		public DateTime NotAfter {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				return _cert.ValidUntil.ToLocalTime ();
			}
		}

		public DateTime NotBefore {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				return _cert.ValidFrom.ToLocalTime ();
			}
		}

		public AsymmetricAlgorithm PrivateKey {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				try {
					if (_cert.RSA != null) {
#if !MOONLIGHT
						RSACryptoServiceProvider rcsp = _cert.RSA as RSACryptoServiceProvider;
						if (rcsp != null)
							return rcsp.PublicOnly ? null : rcsp;
#endif
						RSAManaged rsam = _cert.RSA as RSAManaged;
						if (rsam != null)
							return rsam.PublicOnly ? null : rsam;

						_cert.RSA.ExportParameters (true);
						return _cert.RSA;
					} else if (_cert.DSA != null) {
#if !MOONLIGHT
						DSACryptoServiceProvider dcsp = _cert.DSA as DSACryptoServiceProvider;
						if (dcsp != null)
							return dcsp.PublicOnly ? null : dcsp;
#endif	
						_cert.DSA.ExportParameters (true);
						return _cert.DSA;
					}
				}
				catch {
				}
				return null;
			}
			set {
				if (_cert == null)
					throw new CryptographicException (empty_error);

				// allow NULL so we can "forget" the key associated to the certificate
				// e.g. in case we want to export it in another format (see bug #396620)
				if (value == null) {
					_cert.RSA = null;
					_cert.DSA = null;
				} else 	if (value is RSA)
					_cert.RSA = (RSA) value;
				else if (value is DSA)
					_cert.DSA = (DSA) value;
				else
					throw new NotSupportedException ();
			}
		} 

		public PublicKey PublicKey {
			get { 
				if (_cert == null)
					throw new CryptographicException (empty_error);

				if (_publicKey == null) {
					try {
						_publicKey = new PublicKey (_cert);
					}
					catch (Exception e) {
						string msg = Locale.GetText ("Unable to decode public key.");
						throw new CryptographicException (msg, e);
					}
				}
				return _publicKey;
			}
		} 

		public byte[] RawData {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);

				return base.GetRawCertData ();
			}
		} 

		public string SerialNumber {
			get { 
				if (_cert == null)
					throw new CryptographicException (empty_error);

				if (_serial == null) {
					StringBuilder sb = new StringBuilder ();
					byte[] serial = _cert.SerialNumber;
					for (int i=serial.Length - 1; i >= 0; i--)
						sb.Append (serial [i].ToString ("X2"));
					_serial = sb.ToString ();
				}
				return _serial; 
			}
		} 

		public Oid SignatureAlgorithm {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);

				if (signature_algorithm == null)
					signature_algorithm = new Oid (_cert.SignatureAlgorithm);
				return signature_algorithm;
			}
		} 

		public X500DistinguishedName SubjectName {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);

				if (subject_name == null)
					subject_name = new X500DistinguishedName (_cert.GetSubjectName ().GetBytes ());
				return subject_name;
			}
		} 

		public string Thumbprint {
			get { return base.GetCertHashString (); }
		} 

		public int Version {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				return _cert.Version;
			}
		}

		// methods

		[MonoTODO ("always return String.Empty for UpnName, DnsFromAlternativeName and UrlName")]
		public string GetNameInfo (X509NameType nameType, bool forIssuer) 
		{
			switch (nameType) {
			case X509NameType.SimpleName:
				if (_cert == null)
					throw new CryptographicException (empty_error);
				// return CN= or, if missing, the first part of the DN
				ASN1 sn = forIssuer ? _cert.GetIssuerName () : _cert.GetSubjectName ();
				ASN1 dn = Find (commonName, sn);
				if (dn != null)
					return GetValueAsString (dn);
				if (sn.Count == 0)
					return String.Empty;
				ASN1 last_entry = sn [sn.Count - 1];
				if (last_entry.Count == 0)
					return String.Empty;
				return GetValueAsString (last_entry [0]);
			case X509NameType.EmailName:
				// return the E= part of the DN (if present)
				ASN1 e = Find (email, forIssuer ? _cert.GetIssuerName () : _cert.GetSubjectName ());
				if (e != null)
					return GetValueAsString (e);
				return String.Empty;
			case X509NameType.UpnName:
				// FIXME - must find/create test case
				return String.Empty;
			case X509NameType.DnsName:
				// return the CN= part of the DN (if present)
				ASN1 cn = Find (commonName, forIssuer ? _cert.GetIssuerName () : _cert.GetSubjectName ());
				if (cn != null)
					return GetValueAsString (cn);
				return String.Empty;
			case X509NameType.DnsFromAlternativeName:
				// FIXME - must find/create test case
				return String.Empty;
			case X509NameType.UrlName:
				// FIXME - must find/create test case
				return String.Empty;
			default:
				throw new ArgumentException ("nameType");
			}
		}

		static byte[] commonName = { 0x55, 0x04, 0x03 };
		static byte[] email = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x09, 0x01 };

		private ASN1 Find (byte[] oid, ASN1 dn)
		{
			if (dn.Count == 0)
				return null;

			// process SET
			for (int i = 0; i < dn.Count; i++) {
				ASN1 set = dn [i];
				for (int j = 0; j < set.Count; j++) {
					ASN1 pair = set [j];
					if (pair.Count != 2)
						continue;

					ASN1 poid = pair [0];
					if (poid == null)
						continue;

					if (poid.CompareValue (oid))
						return pair;
				}
			}
			return null;
		}

		private string GetValueAsString (ASN1 pair)
		{
			if (pair.Count != 2)
				return String.Empty;

			ASN1 value = pair [1];
			if ((value.Value == null) || (value.Length == 0))
				return String.Empty;

			if (value.Tag == 0x1E) {
				// BMPSTRING
				StringBuilder sb = new StringBuilder ();
				for (int j = 1; j < value.Value.Length; j += 2)
					sb.Append ((char)value.Value [j]);
				return sb.ToString ();
			} else {
				return Encoding.UTF8.GetString (value.Value);
			}
		}

		private void ImportPkcs12 (byte[] rawData, string password)
		{
			MX.PKCS12 pfx = (password == null) ? new MX.PKCS12 (rawData) : new MX.PKCS12 (rawData, password);
			if (pfx.Certificates.Count > 0) {
				_cert = pfx.Certificates [0];
			} else {
				_cert = null;
			}
			if (pfx.Keys.Count > 0) {
				_cert.RSA = (pfx.Keys [0] as RSA);
				_cert.DSA = (pfx.Keys [0] as DSA);
			}
		}

		public override void Import (byte[] rawData) 
		{
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		[MonoTODO ("missing KeyStorageFlags support")]
		public override void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			base.Import (rawData, password, keyStorageFlags);
			if (password == null) {
				try {
					_cert = new Mono.Security.X509.X509Certificate (rawData);
				}
				catch (Exception e) {
					try {
						ImportPkcs12 (rawData, null);
					}
					catch {
						string msg = Locale.GetText ("Unable to decode certificate.");
						// inner exception is the original (not second) exception
						throw new CryptographicException (msg, e);
					}
				}
			} else {
				// try PKCS#12
				try {
					ImportPkcs12 (rawData, password);
				}
				catch {
					// it's possible to supply a (unrequired/unusued) password
					// fix bug #79028
					_cert = new Mono.Security.X509.X509Certificate (rawData);
				}
			}
		}

#if !MOONLIGHT
		[MonoTODO ("SecureString is incomplete")]
		public override void Import (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (rawData, (string) null, keyStorageFlags);
		}

		public override void Import (string fileName) 
		{
			byte[] rawData = Load (fileName);
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		[MonoTODO ("missing KeyStorageFlags support")]
		public override void Import (string fileName, string password, X509KeyStorageFlags keyStorageFlags) 
		{
			byte[] rawData = Load (fileName);
			Import (rawData, password, keyStorageFlags);
		}

		[MonoTODO ("SecureString is incomplete")]
		public override void Import (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags) 
		{
			byte[] rawData = Load (fileName);
			Import (rawData, (string)null, keyStorageFlags);
		}
#endif
		private static byte[] Load (string fileName)
		{
			byte[] data = null;
			using (FileStream fs = File.OpenRead (fileName)) {
				data = new byte [fs.Length];
				fs.Read (data, 0, data.Length);
				fs.Close ();
			}
			return data;
		}

		public override void Reset () 
		{
			_cert = null;
			_archived = false;
			_extensions = null;
			_name = String.Empty;
			_serial = null;
			_publicKey = null;
			issuer_name = null;
			subject_name = null;
			signature_algorithm = null;
			base.Reset ();
		}

		public override string ToString ()
		{
			if (_cert == null)
				return "System.Security.Cryptography.X509Certificates.X509Certificate2";

			return base.ToString (true);
		}

		public override string ToString (bool verbose)
		{
			if (_cert == null)
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
			if (_cert == null)
				throw new CryptographicException (empty_error);

			X509Chain chain = (X509Chain) CryptoConfig.CreateFromName ("X509Chain");
			if (!chain.Build (this))
				return false;
			// TODO - check chain and other stuff ???
			return true;
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
#if !MOONLIGHT
					case 0x02:
						// INTEGER / SEQUENCE / SEQUENCE
						if ((data [1].Tag == 0x30) && (data [2].Tag == 0x30))
							type = X509ContentType.Pkcs12;
						// note: Pfx == Pkcs12
						break;
#endif
					}
				}
#if !MOONLIGHT
				// check for PKCS#7 (count unknown but greater than 0)
				// SEQUENCE / OID (signedData)
				if ((data [0].Tag == 0x06) && data [0].CompareValue (signedData))
					type = X509ContentType.Pkcs7;
#endif
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

			byte[] data = Load (fileName);
			return GetCertContentType (data);
		}

		// internal stuff because X509Certificate2 isn't complete enough
		// (maybe X509Certificate3 will be better?)

		internal MX.X509Certificate MonoCertificate {
			get { return _cert; }
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
#endif
