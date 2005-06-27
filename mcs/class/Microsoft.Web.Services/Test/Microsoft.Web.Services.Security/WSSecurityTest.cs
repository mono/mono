//
// WSSecurityTest.cs - NUnit Test Cases for WSSecurity
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class WSSecurityTest : Assertion {

		[Test]
		public void Constructor () 
		{
			WSSecurity wss = new WSSecurity ();
			Assertion.AssertNotNull ("Constructor", wss);
		}

		[Test]
		public void PublicConstStrings () 
		{
#if WSE1
			AssertEquals ("NamespaceURI", "http://schemas.xmlsoap.org/ws/2002/07/secext", WSSecurity.NamespaceURI);
#else
			AssertEquals ("NamespaceURI", "http://schemas.xmlsoap.org/ws/2002/12/secext", WSSecurity.NamespaceURI);
#endif
			AssertEquals ("Prefix", "wsse", WSSecurity.Prefix);
		}

		// LAMESPEC AttributeNames aren't documented
		[Test]
		public void AttributeNamesConstructor () 
		{
			// test constructor
			WSSecurity.AttributeNames wsan = new WSSecurity.AttributeNames ();
			AssertNotNull ("AttributeNames Constructor", wsan);
		}

		// LAMESPEC AttributeNames aren't documented
		[Test]
		public void AttributeNames () 
		{
			// test public const strings
			AssertEquals ("EncodingType", "EncodingType", WSSecurity.AttributeNames.EncodingType);
			AssertEquals ("IdentifierType", "IdentifierType", WSSecurity.AttributeNames.IdentifierType);
#if WSE1
			AssertEquals ("TokenType", "TokenType", WSSecurity.AttributeNames.TokenType);
#endif
			AssertEquals ("Type", "Type", WSSecurity.AttributeNames.Type);
			AssertEquals ("Uri", "URI", WSSecurity.AttributeNames.Uri);
			AssertEquals ("ValueType", "ValueType", WSSecurity.AttributeNames.ValueType);
		}

		// LAMESPEC ElementNames aren't documented
		[Test]
		public void ElementNamesConstructor () 
		{
			// test constructor
			WSSecurity.ElementNames wsen = new WSSecurity.ElementNames ();
			AssertNotNull ("ElementNames Constructor", wsen);
		}

		// LAMESPEC ElementNames aren't documented
		[Test]
		public void TestElementNames () 
		{
			// test public const strings
			AssertEquals ("BinarySecurityToken", "BinarySecurityToken", WSSecurity.ElementNames.BinarySecurityToken);
			AssertEquals ("KeyIdentifier", "KeyIdentifier", WSSecurity.ElementNames.KeyIdentifier);
			AssertEquals ("Nonce", "Nonce", WSSecurity.ElementNames.Nonce);
			AssertEquals ("Password", "Password", WSSecurity.ElementNames.Password);
			AssertEquals ("Reference", "Reference", WSSecurity.ElementNames.Reference);
			AssertEquals ("Security", "Security", WSSecurity.ElementNames.Security);
			AssertEquals ("SecurityTokenReference", "SecurityTokenReference", WSSecurity.ElementNames.SecurityTokenReference);
			AssertEquals ("Username", "Username", WSSecurity.ElementNames.Username);
			AssertEquals ("UsernameToken", "UsernameToken", WSSecurity.ElementNames.UsernameToken);
		}
	}
}