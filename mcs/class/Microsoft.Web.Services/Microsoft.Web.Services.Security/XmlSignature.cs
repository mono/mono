//
// XmlSignature.cs: Handles WS-Security XmlSignature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Security {

	public sealed class XmlSignature {

		public class ElementNames {

			// LAMESPEC: public const string BinarySecurityToken = "";
			// LAMESPEC: public const string CipherData = "";
			// LAMESPEC: public const string CipherValue = "";
			// LAMESPEC: public const string DataReference = "";
			// LAMESPEC: public const string EncryptedData = "";
			// LAMESPEC: public const string EncryptedKey = "";
			// LAMESPEC: public const string EncryptionMethod = "";
			// LAMESPEC: public const string KeyIdentifier = "";
			public const string KeyInfo = "KeyInfo";
			public const string KeyName = "KeyName";
			// LAMESPEC: public const string Nonce = "";
			// LAMESPEC: public const string Password = "";
			// LAMESPEC: public const string Reference = "";
			// LAMESPEC: public const string ReferenceList = "";
			// LAMESPEC: public const string Security = "";
			// LAMESPEC: public const string SecurityTokenReference = "";
			public const string Signature = "Signature";
			// LAMESPEC: public const string Username = "";
			// LAMESPEC: public const string UsernameToken = "";

			// to be compatible with Mono implementation of System.Security
			internal const string CanonicalizationMethod = "CanonicalizationMethod";
			internal const string HMACOutputLength = "HMACOutputLength";
			internal const string Reference = "Reference";
			internal const string SignatureMethod = "SignatureMethod";
			internal const string SignedInfo = "SignedInfo";
			internal const string Transform = "Transform";
			internal const string Transforms = "Transforms";
			internal const string DigestMethod = "DigestMethod";
			internal const string DigestValue = "DigestValue";
			internal const string SignatureValue = "SignatureValue";
			internal const string Object = "Object";

			public ElementNames () {}
		}

		internal class AttributeNames {

			internal const string Algorithm = "Algorithm";
			internal const string Id = "Id";
			internal const string URI = "URI";
			internal const string Type = "Type";

			public AttributeNames () {}
		}

		public const string NamespaceURI = "http://www.w3.org/2000/09/xmldsig#";
		public const string Prefix = "ds";

		public XmlSignature () {}
	}
}
