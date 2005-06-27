//
// SaML.cs - Security Assertion Markup Language
//	http://www.oasis-open.org/committees/tc_home.php?wg_abbrev=security
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Security {

	public class SaML {

		public class ElementNames {

			public const string Assertion = "Assertion";
			public const string AssertionIDReference = "AssertionIDReference";
	 
			public ElementNames () {}
		}
	
		public const string NamespaceURI = "urn:oasis:names:tc:SAML:1.0:assertion";
		public const string Prefix = "saml";
	
		public SaML () {}
	}
}
