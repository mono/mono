//
// X509SecurityToken.cs: Handles WS-Security X509SecurityToken
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Web.Services.Protocols;
using System.Xml;
using Microsoft.Web.Services.Security.X509;

namespace Microsoft.Web.Services.Security {

	public sealed class X509SecurityToken : BinarySecurityToken {

		private const string vname = "X509v3";
		private X509Certificate x509;

		public X509SecurityToken (X509Certificate certificate) 
			: base (new XmlQualifiedName (vname, WSSecurity.NamespaceURI))
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			x509 = certificate;
		}

		public X509SecurityToken (XmlElement element) : base (element)
		{
			// ancestor will call LoadXml (element)
		}

		public override AuthenticationKey AuthenticationKey {
			get {
				if (x509 == null)
					throw new InvalidOperationException ("null certificate");
				if (!x509.SupportsDigitalSignature)
					throw new InvalidOperationException ("not SupportsDigitalSignature");
				return new AuthenticationKey (x509.PublicKey);
			}
		}

		public X509Certificate Certificate {
			get { return x509; }
			set { 
// LAMESPEC			if (value == null)
//					throw new ArgumentNullException ("value");
// Note: this (probable bug) means we have to check for null everytime we use a certificate
				x509 = value;
			}
		}

		public override DecryptionKey DecryptionKey {
			get {
				if (x509 == null)
					throw new InvalidOperationException ("null certificate");
				return new AsymmetricDecryptionKey (x509.Key);
			}
		}

		public override EncryptionKey EncryptionKey {
			get {
				if (x509 == null)
					throw new InvalidOperationException ("null certificate");
				return new AsymmetricEncryptionKey (x509.PublicKey);
			}
		}

		public override byte[] RawData {
			get { 
				if (x509 == null)
					return null;
				return x509.GetRawCertData(); 
			}
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");
				x509 = new X509Certificate (value);
			}
		}

		public override SignatureKey SignatureKey {
			get {
				if (x509 == null)
					throw new InvalidOperationException ("null certificate");
				if (!x509.SupportsDigitalSignature)
					throw new InvalidOperationException ("not SupportsDigitalSignature");
				return new SignatureKey (x509.Key);
			}
		}

		public override bool SupportsDataEncryption {
			get {
				if (x509 == null)
					throw new InvalidOperationException ("null certificate");
				return x509.SupportsDataEncryption;
			}
		}

		public override bool SupportsDigitalSignature {
			get {
				if (x509 == null)
					throw new InvalidOperationException ("null certificate");
				return x509.SupportsDigitalSignature;
			}
		}

		public override XmlQualifiedName ValueType {
			get { return valueType; }
			set {
				if ((value.Name != vname) || (value.Namespace != WSSecurity.NamespaceURI))
					throw new SecurityFormatException ("Invalid Qualified Name");
				valueType = value; 
			}
		}

#if WSE1
		public override void Verify ()
		{
			if (x509 == null)
				throw new SecurityFault ("null certificate", null);
			if (!x509.IsCurrent)
				throw new SecurityFault ("certificate not current", null);
			// more ???
			// it's assumed valid if no exception is thrown
		}
#else
		[MonoTODO ("need to compare results with WSE2")]
		public override int GetHashCode () 
                {
                    return x509.GetHashCode();
                }

		[MonoTODO ("need to compare results with WSE2")]
		public override bool Equals (SecurityToken token) 
		{
                    return false;
		}

		public override bool IsCurrent {
			get { return false; }
		}
#endif
	}
}
