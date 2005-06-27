//
// WSRouting.cs: WSRouting definitions
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Routing {

	public class WSRouting {

		public class ActionUri {

			public const string Fault = "http://schemas.xmlsoap.org/soap/fault";
 
			public ActionUri () {}
		}

		public class ElementNames {

			public const string Action = "action";
			public const string Code = "code";
			public const string Endpoint = "endpoint";
			public const string Fault = "fault";
			public const string Found = "found";
			public const string From = "from";
			public const string Fwd = "fwd";
			public const string Id = "id";
			public const string MaxSize = "maxsize";
			public const string MaxTime = "maxtime";
			public const string Path = "path";
			public const string Reason = "reason";
			public const string RelatesTo = "relatesTo";
			public const string RetryAfter = "retryAfter";
			public const string Rev = "rev";
			public const string To = "to";
			public const string Via = "via";
 
			public ElementNames () {}
		}
 
		public const string NamespaceURI = "http://schemas.xmlsoap.org/rp";
		public const string Prefix = "wsrp";

		public WSRouting () {}
	} 
}
