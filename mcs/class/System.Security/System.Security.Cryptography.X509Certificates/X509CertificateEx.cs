//
// X509CertificateEx.cs - System.Security.Cryptography.X509CertificateEx
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Text;

using MX = Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class X509CertificateEx : X509Certificate {

		private bool _archived;
		private X509ExtensionCollection _extensions;
		private string _name;
		private string _serial;
		private PublicKey _publicKey;

		private MX.X509Certificate _cert;

		// constructors

		public X509CertificateEx () : base () 
		{
			_cert = new MX.X509Certificate (this.RawData);
		}

		public X509CertificateEx (byte[] rawData) : base (rawData) 
		{
			_cert = new MX.X509Certificate (this.RawData);
		}

		public X509CertificateEx (byte[] rawData, string password) : base (rawData, password) 
		{
			_cert = new MX.X509Certificate (this.RawData);
		}

		public X509CertificateEx (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
			: base (rawData, password, keyStorageFlags) 
		{
			_cert = new MX.X509Certificate (this.RawData);
		}

		public X509CertificateEx (string fileName) : base (fileName) 
		{
			_cert = new MX.X509Certificate (this.RawData);
		}

		public X509CertificateEx (string fileName, string password) 
		{
			_cert = new MX.X509Certificate (this.RawData);
		}

		public X509CertificateEx (string fileName, string password, X509KeyStorageFlags keyStorageFlags)
			: base (fileName, password, keyStorageFlags) 
		{
			_cert = new MX.X509Certificate (this.RawData);
		}

		public X509CertificateEx (IntPtr handle) : base (handle) 
		{
			_cert = new MX.X509Certificate (this.RawData);
		}

		public X509CertificateEx (X509CertificateEx certificate) 
		{
			_cert = new MX.X509Certificate (this.RawData);
		}

		// properties

		public bool Archived {
			get { return _archived; }
			set { _archived = value; }
		}

		public X509ExtensionCollection Extensions {
			get { return _extensions; }
		}

		public string FriendlyName {
			get { return _name; }
			set { _name = value; }
		}

		public string Issuer {
			get { return _cert.IssuerName; }
		} 

		public DateTime NotAfter {
			get { return _cert.ValidUntil; }
		}

		public DateTime NotBefore {
			get { return _cert.ValidFrom; }
		}

		public AsymmetricAlgorithm PrivateKey {
			get { 
				return _cert.RSA; 
			}
		} 

		public PublicKey PublicKey {
			get { 
				if (_publicKey == null) {
					_publicKey = new PublicKey (_cert);
				}
				return _publicKey;
			}
		} 

		public byte[] RawData {
			get { return base.GetRawCertData (); }
		} 

		public string SerialNumber {
			get { 
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
			get { return null; }
		} 

		public string Subject {
			get { return _cert.SubjectName; }
		} 

		public string Thumbprint {
			get { return base.GetCertHashString (); }
		} 

		public int Version {
			get { return _cert.Version; }
		}

		// methods

		public void Display () {}

		public void Display (IntPtr hwndParent) {}

		public string GetNameInfo (X509NameType nameType, bool forIssuer) 
		{
			return null;
		}

		public override void Import (byte[] rawData) 
		{
			base.Import (rawData);
		}

		public override void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
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

		public override void Reset () 
		{
			_serial = null;
			_publicKey = null;
			base.Reset ();
		}

		public override string ToString ()
		{
			return null;
		}

		public override string ToString (bool verbose)
		{
			return null;
		}

		// static methods

		public static X509ContentType GetCertContentType (byte[] rawData)
		{
			return X509ContentType.Unknown;
		}

		public static X509ContentType GetCertContentType (string fileName)
		{
			return X509ContentType.Unknown;
		}
	}
}

#endif
