//
// XrML.cs - eXtensible rights Markup Language (http://www.xrml.org/)
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Security {

	public class XrML {

		public class AttributeNames {

			public const string RefType = "RefType";

			public AttributeNames () {}
		}
	
		public class ElementNames {

			public const string License = "license";

			public ElementNames () {}
		}

		public const string NamespaceURI = "urn:oasis:names:tc:WSS:1.0:bindings:WSS-XrML-binding";
		public const string Prefix = "xrml";

		public XrML () {}
	}
}
