//
// XmlEncryptionTest.cs - NUnit Test Cases for XmlEncryption
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
	public class XmlEncryptionTest : Assertion {

		[Test]
		public void Constructor () 
		{
			XmlEncryption xe = new XmlEncryption ();
			AssertNotNull ("Constructor", xe);
		}

		[Test]
		public void PublicConstStrings () 
		{
			AssertEquals ("NamespaceURI", "http://www.w3.org/2001/04/xmlenc#", XmlEncryption.NamespaceURI);
			AssertEquals ("Prefix", "xenc", XmlEncryption.Prefix);
		}

		[Test]
		public void AlgorithmURIConstructor () 
		{
			// test constructor
			XmlEncryption.AlgorithmURI xeau = new XmlEncryption.AlgorithmURI ();
			AssertNotNull ("AlgorithmURI Constructor", xeau);
		}

		[Test]
		public void AlgorithmURI () 
		{
			AssertEquals ("AES128", "http://www.w3.org/2001/04/xmlenc#aes128-cbc", XmlEncryption.AlgorithmURI.AES128);
			AssertEquals ("AES128KeyWrap", "http://www.w3.org/2001/04/xmlenc#kw-aes128", XmlEncryption.AlgorithmURI.AES128KeyWrap);
			AssertEquals ("AES192", "http://www.w3.org/2001/04/xmlenc#aes192-cbc", XmlEncryption.AlgorithmURI.AES192);
			AssertEquals ("AES192KeyWrap", "http://www.w3.org/2001/04/xmlenc#kw-aes192", XmlEncryption.AlgorithmURI.AES192KeyWrap);
			AssertEquals ("AES256", "http://www.w3.org/2001/04/xmlenc#aes256-cbc", XmlEncryption.AlgorithmURI.AES256);
			AssertEquals ("AES256KeyWrap", "http://www.w3.org/2001/04/xmlenc#kw-aes256", XmlEncryption.AlgorithmURI.AES256KeyWrap);
			AssertEquals ("DES", "http://www.w3.org/2001/04/xmlenc#des-cbc", XmlEncryption.AlgorithmURI.DES);
			AssertEquals ("RSA15", "http://www.w3.org/2001/04/xmlenc#rsa-1_5", XmlEncryption.AlgorithmURI.RSA15);
			AssertEquals ("RSAOAEP", "http://www.w3.org/2001/04/xmlenc#rsa-aoep-mgf1pl", XmlEncryption.AlgorithmURI.RSAOAEP);
			AssertEquals ("SHA1", "http://www.w3.org/2000/09/xmldsig#sha1", XmlEncryption.AlgorithmURI.SHA1);
			AssertEquals ("SHA256", "http://www.w3.org/2001/04/xmlenc#sha256", XmlEncryption.AlgorithmURI.SHA256);
			AssertEquals ("SHA512", "http://www.w3.org/2001/04/xmlenc#sha512", XmlEncryption.AlgorithmURI.SHA512);
			AssertEquals ("TripleDES", "http://www.w3.org/2001/04/xmlenc#tripledes-cbc", XmlEncryption.AlgorithmURI.TripleDES);
			AssertEquals ("TripleDESKeyWrap", "http://www.w3.org/2001/04/xmlenc#kw-tripledes", XmlEncryption.AlgorithmURI.TripleDESKeyWrap);
		}

		[Test]
		public void AttributeNamesConstructor () 
		{
			// test constructor
			XmlEncryption.AttributeNames xean = new XmlEncryption.AttributeNames ();
			AssertNotNull ("AttributeNames Constructor", xean);
		}

		[Test]
		public void AttributeNames () 
		{
			AssertEquals ("Algorithm ", "Algorithm", XmlEncryption.AttributeNames.Algorithm);
			// LAMESPEC AssertEquals ("EncodingType", "EncodingType", XmlEncryption.AttributeNames.EncodingType);
			AssertEquals ("Id", "Id", XmlEncryption.AttributeNames.Id);
			// LAMESPEC AssertEquals ("IdentifierType", "IdentifierType", XmlEncryption.AttributeNames.IdentifierType);
			// LAMESPEC AssertEquals ("TokenType", "TokenType", XmlEncryption.AttributeNames.TokenType);
			AssertEquals ("Type", "Type", XmlEncryption.AttributeNames.Type);
			// LAMESPEC AssertEquals ("Uri", "Uri", XmlEncryption.AttributeNames.Uri);
			AssertEquals ("URI", "URI", XmlEncryption.AttributeNames.URI);
			// LAMESPEC AssertEquals ("ValueType", "ValueType", XmlEncryption.AttributeNames.ValueType);
		}

		// LAMESPEC ElementNames aren't documented
		[Test]
		public void ElementNamesConstructor () 
		{
			// test constructor
			XmlEncryption.ElementNames xeen = new XmlEncryption.ElementNames ();
			AssertNotNull ("ElementNames Constructor", xeen);
		}

		// LAMESPEC ElementNames aren't documented
		[Test]
		public void ElementNames () 
		{
			// test public const strings
			AssertEquals ("CipherData", "CipherData", XmlEncryption.ElementNames.CipherData);
			AssertEquals ("CipherValue", "CipherValue", XmlEncryption.ElementNames.CipherValue);
			AssertEquals ("DataReference", "DataReference", XmlEncryption.ElementNames.DataReference);
			AssertEquals ("EncryptedData", "EncryptedData", XmlEncryption.ElementNames.EncryptedData);
			AssertEquals ("EncryptedKey", "EncryptedKey", XmlEncryption.ElementNames.EncryptedKey);
			AssertEquals ("EncryptionMethod", "EncryptionMethod", XmlEncryption.ElementNames.EncryptionMethod);
			AssertEquals ("ReferenceList", "ReferenceList", XmlEncryption.ElementNames.ReferenceList);
		}

		[Test]
		public void TypeURIConstructor () 
		{
			// test constructor
			XmlEncryption.TypeURI xetu = new XmlEncryption.TypeURI ();
			AssertNotNull ("TypeURI Constructor", xetu);
		}

		[Test]
		public void TypeURI () 
		{
			// test public const strings
			AssertEquals ("Content", "http://www.w3.org/2001/04/xmlenc#Content", XmlEncryption.TypeURI.Content);
			AssertEquals ("Element", "http://www.w3.org/2001/04/xmlenc#Element", XmlEncryption.TypeURI.Element);
			AssertEquals ("EncryptedKey", "http://www.w3.org/2001/04/xmlenc#EncryptedKey", XmlEncryption.TypeURI.EncryptedKey);
		}
	}
}