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

namespace System.Security.Cryptography.Xml {

	// following the design of WSE
	internal class XmlEncryption {

		public class ElementNames {

			public const string CarriedKeyName = "CarriedKeyName";
			public const string CipherData = "CipherData";
			public const string CipherReference = "CipherReference";
			public const string CipherValue = "CipherValue";
			public const string DataReference = "DataReference";
			public const string EncryptedData = "EncryptedData";
			public const string EncryptedKey = "EncryptedKey";
			public const string EncryptionMethod = "EncryptionMethod";
			public const string EncryptionProperties = "EncryptionProperties";
			public const string EncryptionProperty = "EncryptionProperty";
			public const string KeyReference = "KeyReference";
			public const string KeySize = "KeySize";
			public const string ReferenceList = "ReferenceList";
			public const string Transforms = "Transforms";

			public ElementNames () {}
		}

		public class AttributeNames {

			public const string Algorithm = "Algorithm";
			public const string Encoding = "Encoding";
			public const string Id = "Id";
			public const string MimeType = "MimeType";
			public const string Recipient = "Recipient";
			public const string Target = "Target";
			public const string Type = "Type";
			public const string URI = "URI";

			public AttributeNames () {}
		}

		public const string Prefix = "xenc";

		public XmlEncryption () {}
	}
}

#endif
