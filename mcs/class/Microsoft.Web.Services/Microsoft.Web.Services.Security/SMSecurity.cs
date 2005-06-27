//
// SMSecurity.cs - SMSecurity abstract class
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Security {

	public class SMSecurity : SMSecurityBase {

		public const string NamespaceURI = "http://schemas.xmlsoap.org/ws/2002/12/secext";
		public const string Prefix = "wsse";

		public SMSecurity () {}

		public override string NamespaceURIValue {
			get { return NamespaceURI; }
		}

		public override string PrefixValue {
			get { return Prefix; }
		}
	}
}
