//
// SMSecurityTest.cs - NUnit Test Cases for SMSecurity
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class SMSecurityTest : Assertion {

		[Test]
		public void Constructor () 
		{
			SMSecurity o = new SMSecurity ();
			AssertNotNull ("Constructor", o);
			AssertEquals ("PrefixValue", "wsse", o.PrefixValue);
			AssertEquals ("NamespaceURIValue", "http://schemas.xmlsoap.org/ws/2002/12/secext", o.NamespaceURIValue);
		}

		[Test]
		public void PublicConstStrings () 
		{
			AssertEquals ("Prefix", "wsse", SMSecurity.Prefix);
			AssertEquals ("NamespaceURI", "http://schemas.xmlsoap.org/ws/2002/12/secext", SMSecurity.NamespaceURI);
		}
	}
}