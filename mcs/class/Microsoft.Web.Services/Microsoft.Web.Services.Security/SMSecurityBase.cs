//
// SMSecurityBase.cs - SMSecurityBase abstract class
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Security {

	public abstract class SMSecurityBase {

		protected SMSecurityBase () {}

		public abstract string NamespaceURIValue {
			get;
		}

		public abstract string PrefixValue {
			get;
		}
	}
}
