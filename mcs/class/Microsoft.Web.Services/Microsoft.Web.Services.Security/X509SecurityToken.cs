//
// X509SecurityToken.cs: Handles WS-Security X509SecurityToken
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Licensed under MIT X11 (see LICENSE) with this specific addition:
//
// “This source code may incorporate intellectual property owned by Microsoft 
// Corporation. Our provision of this source code does not include any licenses
// or any other rights to you under any Microsoft intellectual property. If you
// would like a license from Microsoft (e.g. rebrand, redistribute), you need 
// to contact Microsoft directly.” 
//

using System;
using System.Web.Services.Protocols;
using System.Xml;
using Microsoft.Web.Services.Security.X509;

namespace Microsoft.Web.Services.Security {

	public sealed class X509SecurityToken : BinarySecurityToken {

		private X509Certificate x509;

		public X509SecurityToken (X509Certificate certificate) : base ((XmlElement)null)
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

				if (x509.SupportsDigitalSignature)
					return new AuthenticationKey (x509.PublicKey);
				else
					throw new InvalidOperationException ("not SupportsDigitalSignature");
			}
		}

		public X509Certificate Certificate {
			get { return x509; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");
				x509 = value;
			}
		}

		public override DecryptionKey DecryptionKey {
			get {
				if (x509 == null)
					throw new InvalidOperationException ("null certificate");
				return new AsymmetricDecryptionKey (x509.PublicKey);
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
			get { return x509.GetRawCertData(); }
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
				return new SignatureKey (x509.PublicKey);
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

		// TODO
		public override XmlQualifiedName ValueType {
			get { return base.ValueType; }
			set { base.ValueType = value; }
		}

		public override void Verify() 
		{
			if (x509 == null)
				throw new SecurityFault ("null certificate", null);
			if (!x509.IsCurrent)
				throw new SecurityFault ("certificate not current", null);
		}
	}
}
