//
// XmlSignature.cs: Handles WS-Security XmlSignature
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

			public ElementNames () {}
		}

		public const string NamespaceURI = "http://www.w3.org/2000/09/xmldsig#";
		public const string Prefix = "ds";

		public XmlSignature () {}
	}
}
