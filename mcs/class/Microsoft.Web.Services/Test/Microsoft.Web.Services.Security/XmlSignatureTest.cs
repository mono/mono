//
// XmlSignatureTest.cs - NUnit Test Cases for XmlSignature
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
	public class XmlSignatureTest : Assertion {

		[Test]
		public void Constructor () 
		{
			XmlSignature xs = new XmlSignature ();
			AssertNotNull ("Constructor", xs);
		}

		[Test]
		public void PublicConstStrings () 
		{
			AssertEquals ("NamespaceURI", "http://www.w3.org/2000/09/xmldsig#", XmlSignature.NamespaceURI);
			AssertEquals ("Prefix", "ds", XmlSignature.Prefix);
		}

		[Test]
		public void ElementNamesConstructor () 
		{
			// test constructor
			XmlSignature.ElementNames xsen = new XmlSignature.ElementNames ();
			AssertNotNull ("ElementNames Constructor", xsen);
		}

		[Test]
		public void ElementNames () 
		{
			// test public const strings
			// LAMESPEC: AssertEquals ("BinarySecurityToken", "BinarySecurityToken", XmlSignature.ElementNames.BinarySecurityToken);
			// LAMESPEC: AssertEquals ("CipherData", "CipherData", XmlSignature.ElementNames.CipherData);
			// LAMESPEC: AssertEquals ("CipherValue", "CipherValue", XmlSignature.ElementNames.CipherValue);
			// LAMESPEC: AssertEquals ("DataReference", "DataReference", XmlSignature.ElementNames.DataReference);
			// LAMESPEC: AssertEquals ("EncryptedData", "EncryptedData", XmlSignature.ElementNames.EncryptedData);
			// LAMESPEC: AssertEquals ("EncryptedKey", "EncryptedKey", XmlSignature.ElementNames.EncryptedKey);
			// LAMESPEC: AssertEquals ("EncryptionMethod", "EncryptionMethod", XmlSignature.ElementNames.EncryptionMethod);
			// LAMESPEC: AssertEquals ("KeyIdentifier", "KeyIdentifier", XmlSignature.ElementNames.KeyIdentifier);
			AssertEquals ("KeyInfo", "KeyInfo", XmlSignature.ElementNames.KeyInfo);
			AssertEquals ("KeyName", "KeyName", XmlSignature.ElementNames.KeyName);
			// LAMESPEC: AssertEquals ("Nonce", "Nonce", XmlSignature.ElementNames.Nonce);
			// LAMESPEC: AssertEquals ("Password", "Password", XmlSignature.ElementNames.Password);
			// LAMESPEC: AssertEquals ("Reference", "Reference", XmlSignature.ElementNames.Reference);
			// LAMESPEC: AssertEquals ("ReferenceList", "ReferenceList", XmlSignature.ElementNames.ReferenceList);
			// LAMESPEC: AssertEquals ("Security", "Security", XmlSignature.ElementNames.Security);
			// LAMESPEC: AssertEquals ("SecurityTokenReference", "SecurityTokenReference", XmlSignature.ElementNames.SecurityTokenReference);
			AssertEquals ("Signature", "Signature", XmlSignature.ElementNames.Signature);
			// LAMESPEC: AssertEquals ("Username", "Username", XmlSignature.ElementNames.Username);
			// LAMESPEC: AssertEquals ("UsernameToken", "UsernameToken", XmlSignature.ElementNames.UsernameToken);
		}
	}
}