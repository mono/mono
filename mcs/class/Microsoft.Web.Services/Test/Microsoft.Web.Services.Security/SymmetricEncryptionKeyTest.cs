//
// SymmetricEncryptionKeyTest.cs - NUnit Test Cases for SymmetricEncryptionKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Security.Cryptography;
using System.Xml;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class SymmetricEncryptionKeyTest : Assertion {

		static private string emptyKeyInfo = "<KeyInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />";

		[Test]
		public void EmptyConstructor () 
		{
			SymmetricEncryptionKey sek = new SymmetricEncryptionKey ();
			AssertEquals ("KeyInfo()", emptyKeyInfo, sek.KeyInfo.GetXml ().OuterXml);
		}

		[Test]
		public void AlgoConstructor () 
		{
			SymmetricAlgorithm sa = SymmetricAlgorithm.Create ("RC2");
			SymmetricEncryptionKey sek = new SymmetricEncryptionKey (sa);
			AssertEquals ("KeyInfo()", emptyKeyInfo, sek.KeyInfo.GetXml ().OuterXml);
		}

		[Test]
		// should be ArgumentNullException
		[ExpectedException (typeof (NullReferenceException))]
		public void NullAlgoConstructor () 
		{
			SymmetricEncryptionKey sek = new SymmetricEncryptionKey (null);
		}

		[Test]
		public void AlgoWithKeyConstructor () 
		{
			SymmetricAlgorithm sa = SymmetricAlgorithm.Create ("RC2");
			byte[] key = new byte [32]; // 256 bits (invalid size for RC2)
			SymmetricEncryptionKey sek = new SymmetricEncryptionKey (sa, key);
			AssertEquals ("KeyInfo()", emptyKeyInfo, sek.KeyInfo.GetXml ().OuterXml);
		}

		[Test]
		public void NullAlgoWithKeyConstructor () 
		{
			SymmetricAlgorithm sa = SymmetricAlgorithm.Create ("RC2");
			byte[] key = new byte [32]; // 256 bits (invalid size for RC2)

			SymmetricEncryptionKey sek = new SymmetricEncryptionKey (null, key);
			AssertEquals ("KeyInfo()", emptyKeyInfo, sek.KeyInfo.GetXml ().OuterXml);

			sek = new SymmetricEncryptionKey (sa, null);
			AssertEquals ("KeyInfo()", emptyKeyInfo, sek.KeyInfo.GetXml ().OuterXml);
		}
	}
}