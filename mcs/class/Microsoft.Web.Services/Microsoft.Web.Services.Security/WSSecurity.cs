//
// WSSecurity.cs: Handles WS-Security WSSecurity
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

	public sealed class WSSecurity {

		// LAMESPEC AttributeNames aren't documented
		public class AttributeNames {

			public const string EncodingType = "EncodingType";
			public const string IdentifierType = "IdentifierType"; 
			public const string TokenType = "TokenType";
			public const string Type = "Type"; 
			public const string Uri = "URI"; 
			public const string ValueType = "ValueType"; 

			public AttributeNames () {}
		}

		// LAMESPEC ElementNames aren't documented
		public class ElementNames {

			public const string BinarySecurityToken = "BinarySecurityToken"; 
			public const string KeyIdentifier = "KeyIdentifier"; 
			public const string Nonce = "Nonce"; 
			public const string Password = "Password"; 
			public const string Reference = "Reference"; 
			public const string Security = "Security"; 
			public const string SecurityTokenReference = "SecurityTokenReference";
			public const string Username = "Username"; 
			public const string UsernameToken = "UsernameToken";

			public ElementNames () {}
		}

		public const string NamespaceURI = "http://schemas.xmlsoap.org/ws/2002/07/secext";
		public const string Prefix = "wsse";

		public WSSecurity () {}
	}
}
