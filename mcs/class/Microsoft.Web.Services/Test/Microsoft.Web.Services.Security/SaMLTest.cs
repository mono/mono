//
// SaMLTest.cs - NUnit Test Cases for SaML
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
	public class SaMLTest : Assertion {

		[Test]
		public void Constructor () 
		{
			SaML s = new SaML ();
			AssertNotNull ("Constructor", s);
		}

		[Test]
		public void PublicConstStrings () 
		{
			AssertEquals ("NamespaceURI", "urn:oasis:names:tc:SAML:1.0:assertion", SaML.NamespaceURI);
			AssertEquals ("Prefix", "saml", SaML.Prefix);
		}

		[Test]
		public void ElementNamesConstructor () 
		{
			// test constructor
			SaML.ElementNames sen = new SaML.ElementNames ();
			AssertNotNull ("ElementNames Constructor", sen);
		}

		[Test]
		public void ElementNames () 
		{
			// test public const strings
			AssertEquals ("Assertion", "Assertion", SaML.ElementNames.Assertion);
			AssertEquals ("AssertionIDReference", "AssertionIDReference", SaML.ElementNames.AssertionIDReference);
		}
	}
}