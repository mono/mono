//
// X509CertificateEx.cs - System.Security.Cryptography.X509CertificateEx
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell Inc. (http://www.novell.com)
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

using System;
using System.Text;

using MX = Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates {

	public class X509CertificateEx : X509Certificate {

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

		[MonoTODO]
		public X500DistinguishedName IssuerName {
			get { return null; }
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
			[MonoTODO]
			set { throw new NotImplementedException (); }
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

		[MonoTODO]
		public X500DistinguishedName SubjectName {
			get { return null; }
		} 

		public string Thumbprint {
			get { return base.GetCertHashString (); }
		} 

		public int Version {
			get { return _cert.Version; }
		}

		// methods

		[MonoTODO]
		public void Display ()
		{
		}

		[MonoTODO]
		public void Display (IntPtr hwndParent) 
		{
		}

		[MonoTODO]
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

		[MonoTODO]
		public override string ToString ()
		{
			return null;
		}

		[MonoTODO]
		public override string ToString (bool verbose)
		{
			return null;
		}

		// static methods

		[MonoTODO]
		public static X509ContentType GetCertContentType (byte[] rawData)
		{
			return X509ContentType.Unknown;
		}

		[MonoTODO]
		public static X509ContentType GetCertContentType (string fileName)
		{
			return X509ContentType.Unknown;
		}
	}
}

#endif
