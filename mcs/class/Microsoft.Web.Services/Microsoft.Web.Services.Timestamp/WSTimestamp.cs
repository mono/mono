//
// WSTimestamp.cs: WSTimestamp definitions
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

namespace Microsoft.Web.Services.Timestamp {

	public class WSTimestamp {

		public class AttributeNames {

			public const string Actor = "Actor";
			public const string Delay = "Delay";
			public const string Id = "Id";
			public const string ValueType = "ValueType";

			public AttributeNames () {}
		}

		public class ElementNames {

			public const string Created = "Created";
			public const string Expires = "Expires";
			public const string Received = "Received";
			public const string Timestamp = "Timestamp";

			public ElementNames () {}
		}

		public const string NamespaceURI = "http://schemas.xmlsoap.org/ws/2002/07/utility";
		public const string Prefix = "wsu";
		public const string TimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

		public WSTimestamp () {}
	}
}
