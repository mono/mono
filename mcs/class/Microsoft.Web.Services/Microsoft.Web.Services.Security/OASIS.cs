//
// OASIS.cs - OASIS
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Security {

	public class OASIS : SMSecurityBase {

		private string _namespace;

		public const string Prefix = "wsse";

		public OASIS (string ns) 
		{
			_namespace = ns;
		}

		public override string NamespaceURIValue {
			get { return _namespace; }
		}

		public override string PrefixValue {
			get { return Prefix; }
		}
	} 
}
