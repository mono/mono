//
// XmlSignature.cs: Handles Xml Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography.Xml {

	// following the design of WSE
	internal class XmlSignature {

		public class ElementNames {

			public const string CanonicalizationMethod = "CanonicalizationMethod";
			public const string DigestMethod = "DigestMethod";
			public const string DigestValue = "DigestValue";
			public const string DSAKeyValue = "DSAKeyValue";
			public const string HMACOutputLength = "HMACOutputLength";
			public const string KeyInfo = "KeyInfo";
			public const string KeyName = "KeyName";
			public const string KeyValue = "KeyValue";
			public const string Object = "Object";
			public const string Reference = "Reference";
			// RetrievalMethod - RetrievalElement ??? seems like a BUG to me ?
			public const string RetrievalMethod = "RetrievalElement";
			public const string RSAKeyValue = "RSAKeyValue";
			public const string Signature = "Signature";
			public const string SignatureMethod = "SignatureMethod";
			public const string SignatureValue = "SignatureValue";
			public const string SignedInfo = "SignedInfo";
			public const string Transform = "Transform";
			public const string Transforms = "Transforms";
			public const string X509Data = "X509Data";
			public const string X509IssuerSerial = "X509IssuerSerial";
			public const string X509IssuerName = "X509IssuerName";
			public const string X509SerialNumber = "X509SerialNumber";
			public const string X509SKI = "X509SKI";
			public const string X509SubjectName = "X509SubjectName";
			public const string X509Certificate = "X509Certificate";
			public const string X509CRL = "X509CRL";

			public ElementNames () {}
		}

		public class AttributeNames {

			public const string Algorithm = "Algorithm";
			public const string Encoding = "Encoding";
			public const string Id = "Id";
			public const string MimeType = "MimeType";
			public const string Type = "Type";
			public const string URI = "URI";

			public AttributeNames () {}
		}

		public const string NamespaceURI = "http://www.w3.org/2000/09/xmldsig#";
		public const string Prefix = "ds";

		public XmlSignature () {}
	}
}
