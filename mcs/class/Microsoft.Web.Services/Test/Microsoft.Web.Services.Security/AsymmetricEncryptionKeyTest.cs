//
// AsymmetricEncryptionKeyTest.cs 
//	- NUnit Test Cases for AsymmetricEncryptionKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Security.Cryptography;
using System.Xml;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class AsymmetricEncryptionKeyTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull () 
		{
			AsymmetricEncryptionKey aek = new AsymmetricEncryptionKey (null);
		}

		[Test]
		public void ConstructorDSA () 
		{
			DSA dsa = DSA.Create ();
			dsa.ImportParameters (AllTests.GetDSAKey (false));
			AsymmetricEncryptionKey aek = new AsymmetricEncryptionKey (dsa);
			AssertNotNull("DSA-KeyInfo", aek.KeyInfo);
		}

		[Test]
		public void ConstructorRSA () 
		{
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRSAKey (false));
			AsymmetricEncryptionKey aek = new AsymmetricEncryptionKey (rsa);
			AssertNotNull ("RSA-KeyInfo", aek.KeyInfo);
		}
	}
}