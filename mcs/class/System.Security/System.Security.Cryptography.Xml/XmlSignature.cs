//
// XmlSignature.cs: Handles Xml Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell Inc.
//

using System;
using System.Collections;
using System.Xml;

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
#if NET_1_0
			// RetrievalMethod vs RetrievalElement -> BUG in MS Framework 1.0
			public const string RetrievalMethod = "RetrievalElement";
#else
			public const string RetrievalMethod = "RetrievalMethod";
#endif
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

		public class AlgorithmNamespaces {
			public const string XmlDsigBase64Transform = "http://www.w3.org/2000/09/xmldsig#base64";
			public const string XmlDsigC14NTransform = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
			public const string XmlDsigC14NWithCommentsTransform = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";
			public const string XmlDsigEnvelopedSignatureTransform = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
			public const string XmlDsigXPathTransform = "http://www.w3.org/TR/1999/REC-xpath-19991116";
			public const string XmlDsigXsltTransform =  "http://www.w3.org/TR/1999/REC-xslt-19991116";
		}

		public const string NamespaceURI = "http://www.w3.org/2000/09/xmldsig#";
		public const string Prefix = "ds";

		public XmlSignature () 
		{
		}

		public static XmlElement GetChildElement (XmlElement xel, string element, string ns)
		{
			for (int i = 0; i < xel.ChildNodes.Count; i++) {
				XmlNode n = xel.ChildNodes [i];
				if (n.NodeType == XmlNodeType.Element && n.LocalName == element && n.NamespaceURI == ns)
					return n as XmlElement;
			}
			return null;
		}

		public static string GetAttributeFromElement (XmlElement xel, string attribute, string element) 
		{
			XmlElement el = GetChildElement (xel, element, XmlSignature.NamespaceURI);
			return el != null ? el.GetAttribute (attribute) : null;
		}

		public static XmlElement [] GetChildElements (XmlElement xel, string element)
		{
			ArrayList al = new ArrayList ();
			for (int i = 0; i < xel.ChildNodes.Count; i++) {
				XmlNode n = xel.ChildNodes [i];
				if (n.NodeType == XmlNodeType.Element && n.LocalName == element && n.NamespaceURI == XmlSignature.NamespaceURI)
					al.Add (n);
			}
			return al.ToArray (typeof (XmlElement)) as XmlElement [];
		}
	}
}
