//
// XrMLTest.cs - NUnit Test Cases for XrML
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
	public class XrMLTest : Assertion {

		[Test]
		public void Constructor () 
		{
			XrML x = new XrML ();
			AssertNotNull ("Constructor", x);
		}

		[Test]
		public void PublicConstStrings () 
		{
			AssertEquals ("NamespaceURI", "urn:oasis:names:tc:WSS:1.0:bindings:WSS-XrML-binding", XrML.NamespaceURI);
			AssertEquals ("Prefix", "xrml", XrML.Prefix);
		}

		[Test]
		public void AttributeNamesConstructor () 
		{
			// test constructor
			XrML.AttributeNames xan = new XrML.AttributeNames ();
			AssertNotNull ("AttributeNames Constructor", xan);
		}

		[Test]
		public void AttributeNames () 
		{
			// test public const strings
			AssertEquals ("RefType", "RefType", XrML.AttributeNames.RefType);
		}

		[Test]
		public void ElementNamesConstructor () 
		{
			// test constructor
			XrML.ElementNames xen = new XrML.ElementNames ();
			AssertNotNull ("ElementNames Constructor", xen);
		}

		[Test]
		public void ElementNames () 
		{
			// test public const strings
			AssertEquals ("License", "license", XrML.ElementNames.License);
		}
	}
}