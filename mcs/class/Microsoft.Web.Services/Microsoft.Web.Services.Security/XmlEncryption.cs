//
// XmlEncryption.cs: Handles WS-Security XmlEncryption
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

namespace Microsoft.Web.Services.Security {

	public sealed class XmlEncryption {

		public sealed class AlgorithmURI {

			public const string AES128 = "http://www.w3.org/2001/04/xmlenc#aes128-cbc";
			public const string AES128KeyWrap = "http://www.w3.org/2001/04/xmlenc#kw-aes128";
			public const string AES192 = "http://www.w3.org/2001/04/xmlenc#aes192-cbc";
			public const string AES192KeyWrap = "http://www.w3.org/2001/04/xmlenc#kw-aes192";
			public const string AES256 = "http://www.w3.org/2001/04/xmlenc#aes256-cbc";
			public const string AES256KeyWrap = "http://www.w3.org/2001/04/xmlenc#kw-aes256";
			public const string DES = "http://www.w3.org/2001/04/xmlenc#des-cbc";
			public const string RSA15 = "http://www.w3.org/2001/04/xmlenc#rsa-1_5";
			public const string RSAOAEP = "http://www.w3.org/2001/04/xmlenc#rsa-aoep-mgf1pl";
			public const string SHA1 = "http://www.w3.org/2000/09/xmldsig#sha1";
			public const string SHA256 = "http://www.w3.org/2001/04/xmlenc#sha256";
			public const string SHA512 = "http://www.w3.org/2001/04/xmlenc#sha512";
			public const string TripleDES = "http://www.w3.org/2001/04/xmlenc#tripledes-cbc";
			public const string TripleDESKeyWrap = "http://www.w3.org/2001/04/xmlenc#kw-tripledes";

			public AlgorithmURI () {}
		}

		public sealed class AttributeNames {

			public const string Algorithm = "Algorithm";
			//LAMESPEC public const string EncodingType = "EncodingType";
			public const string Id = "Id";
			//LAMESPEC public const string IdentifierType = "IdentifierType";
			//LAMESPEC public const string TokenType = "TokenType";
			public const string Type = "Type";
			//LAMESPEC public const string Uri = "Uri";
			public const string URI = "URI";
			//LAMESPEC public const string ValueType = "ValueType";

			public AttributeNames () {}
		}

		// LAMESPEC ElementNames aren't documented
		public sealed class ElementNames {

			public const string CipherData = "CipherData";
			public const string CipherValue = "CipherValue";
			public const string DataReference = "DataReference";
			public const string EncryptedData = "EncryptedData";
			public const string EncryptedKey = "EncryptedKey";
			public const string EncryptionMethod = "EncryptionMethod";
			public const string ReferenceList = "ReferenceList";

			public ElementNames () {}
		}

		public sealed class TypeURI {

			public const string Content = "http://www.w3.org/2001/04/xmlenc#Content";
			public const string Element = "http://www.w3.org/2001/04/xmlenc#Element";
			public const string EncryptedKey = "http://www.w3.org/2001/04/xmlenc#EncryptedKey";

			public TypeURI () {}
		}

		public const string NamespaceURI = "http://www.w3.org/2001/04/xmlenc#";
		public const string Prefix = "xenc";

		public XmlEncryption () {}
	}
}
