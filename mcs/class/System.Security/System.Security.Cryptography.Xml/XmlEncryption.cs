//
// XmlEncryption.cs: Handles Xml Encryption
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Xml {

	// following the design of WSE
	internal class XmlEncryption {

		public class ElementNames {

			public const string CipherData = "CipherData";
			public const string CipherReference = "CipherReference";
			public const string CipherValue = "CipherValue";
			public const string EncryptedData = "EncryptedData";
			public const string EncryptionMethod = "EncryptionMethod";
			public const string EncryptionProperties = "EncryptionProperties";
			public const string EncryptionProperty = "EncryptionProperty";
			public const string KeySize = "KeySize";
			public const string Transforms = "Transforms";

			public ElementNames () {}
		}

		public class AttributeNames {

			public const string Algorithm = "Algorithm";
			public const string Encoding = "Encoding";
			public const string Id = "Id";
			public const string MimeType = "MimeType";
			public const string Target = "Target";
			public const string Type = "Type";
			public const string URI = "URI";

			public AttributeNames () {}
		}

		public const string NamespaceURI = "http://www.w3.org/2001/04/xmlenc#";
		public const string Prefix = "xenc";

		public XmlEncryption () {}
	}
}

#endif
