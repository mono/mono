//
// DecryptionKeyProviderTest.cs - NUnit Test Cases for DecryptionKeyProvider
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace MonoTests.MS.Web.Services.Security {

	// Note: Test ONLY for WSE1 - the class is obsolete in WSE2 and throw an error when compiling

	[TestFixture]
	public class DecryptionKeyProviderTest : Assertion {

		[Test]
		public void NullAlgorithmUri () 
		{
			DecryptionKeyProvider dkp = new DecryptionKeyProvider ();
			DecryptionKey key = dkp.GetDecryptionKey (null, new KeyInfo ());
			AssertNull ("NullAlgorithmUri", key);
		}

		[Test]
		[Ignore ("Only works if you have the required certificate in your own store")]
		public void RSAPkcs1v15 () 
		{
			DecryptionKeyProvider dkp = new DecryptionKeyProvider ();
			DecryptionKey key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.RSA15, new KeyInfo ());
		}

		[Test]
		public void UnsupportedAlgorithmUri () 
		{
			DecryptionKeyProvider dkp = new DecryptionKeyProvider ();
			DecryptionKey key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.AES128, new KeyInfo ());
			AssertNull ("AES128", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.AES128KeyWrap, new KeyInfo ());
			AssertNull ("AES128KeyWrap", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.AES192, new KeyInfo ());
			AssertNull ("AES192", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.AES192KeyWrap, new KeyInfo ());
			AssertNull ("AES192KeyWrap", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.AES256, new KeyInfo ());
			AssertNull ("AES256", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.AES256KeyWrap, new KeyInfo ());
			AssertNull ("AES256KeyWrap", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.DES, new KeyInfo ());
			AssertNull ("DES", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.RSAOAEP, new KeyInfo ());
			AssertNull ("RSAOAEP", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.SHA1, new KeyInfo ());
			AssertNull ("SHA1", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.SHA256, new KeyInfo ());
			AssertNull ("SHA256", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.SHA512, new KeyInfo ());
			AssertNull ("SHA512", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.TripleDES, new KeyInfo ());
			AssertNull ("TripleDES", key);
			key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.TripleDESKeyWrap, new KeyInfo ());
			AssertNull ("TripleDESKeyWrap", key);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ConstructorByteArrayNull () 
		{
			DecryptionKeyProvider dkp = new DecryptionKeyProvider ();
			DecryptionKey key = dkp.GetDecryptionKey (XmlEncryption.AlgorithmURI.RSA15, null);
		}
	}
}