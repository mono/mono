//
// AsymmetricDecryptionKeyTest.cs 
//	- NUnit Test Cases for AsymmetricDecryptionKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Security.Cryptography;
using System.Web.Services.Protocols;
using System.Xml;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class AsymmetricDecryptionKeyTest : Assertion {

		[Test]
		//[ExpectedException (typeof (SecurityFault))] 
		public void ConstructorNull () 
		{
			try {
				AsymmetricDecryptionKey aek = new AsymmetricDecryptionKey (null);
			}
			catch (SoapHeaderException) {
				// SecurityFault is internal
			}
			catch (Exception e) {
				Fail ("Expected SecurityFault but got " + e.ToString ());
			}
		}

		[Test]
		// [ExpectedException (typeof (SecurityFault))]
		public void ConstructorDSA () 
		{
			try {
				DSA dsa = DSA.Create ();
				dsa.ImportParameters (AllTests.GetDSAKey (false));
				AsymmetricDecryptionKey aek = new AsymmetricDecryptionKey (dsa);
			}
			catch (SoapHeaderException) {
				// SecurityFault is internal
			}
			catch (Exception e) {
				Fail ("Expected SecurityFault but got " + e.ToString ());
			}
		}

		[Test]
		public void ConstructorRSA () 
		{
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRSAKey (false));
			AsymmetricDecryptionKey aek = new AsymmetricDecryptionKey (rsa);
			AssertNotNull ("Constructor(RSA)", aek);
		}

		[Test]
		public void Name () 
		{
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRSAKey (false));
			AsymmetricDecryptionKey aek = new AsymmetricDecryptionKey (rsa);
			// null by default (not empty)
			AssertNull (aek.Name);
			// can assign any string (not format)
			string keyName = "My Key";
			aek.Name = keyName;
			AssertEquals ("Name", keyName, aek.Name);
			// can be set to null
			aek.Name = null;
			AssertNull (aek.Name);
		}
	}
}