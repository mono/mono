//
// Pkcs7Recipient.cs - System.Security.Cryptography.Pkcs.Pkcs7Recipient
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {

	public class Pkcs7Recipient {

		private SubjectIdentifierType _recipient;
		private X509CertificateEx _certificate;

		// constructor

		public Pkcs7Recipient (SubjectIdentifierType recipientIdentifierType, X509CertificateEx certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (recipientIdentifierType == SubjectIdentifierType.Unknown)
				_recipient = SubjectIdentifierType.IssuerAndSerialNumber;
			else
				_recipient = recipientIdentifierType;
			_certificate = certificate;
		}

		// properties

		public X509CertificateEx Certificate {
			get { return _certificate; }
		}

		public SubjectIdentifierType RecipientIdentifierType {
			get { return _recipient; }
		}
	}
}

#endif