//
// XmltokTest.cs - NUnit Test Cases for Xmltok
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
	public class XmltokTest : Assertion {

		[Test]
		public void Constructor () 
		{
			Xmltok x = new Xmltok ();
			AssertNotNull ("Constructor", x);
		}

		[Test]
		public void PublicConstStrings () 
		{
			AssertEquals ("NamespaceURI", "http://schemas.xmlsoap.org/ws/2002/08/xmltok", Xmltok.NamespaceURI);
			// prefix not Prefix (like elsewhere)
			AssertEquals ("Prefix", "tok", Xmltok.prefix);
		}
	}
}