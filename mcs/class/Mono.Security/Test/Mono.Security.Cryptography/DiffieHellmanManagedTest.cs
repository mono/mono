//
// DiffieHellmanManagedTest.cs - NUnit Test Cases for DH (PKCS#3)
//
// Authors:
//	Pieter Philippaerts (Pieter@mentalis.org)
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 The Mentalis.org Team (http://www.mentalis.org/)
// (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.IO;
using Mono.Security.Cryptography;
using System.Text;

namespace MonoTests.Mono.Security.Cryptography {

	// References:
	// a.	PKCS #3: Diffie-Hellman Key-Agreement Standard (version 1.4)
	//	ftp://ftp.rsasecurity.com/pub/pkcs/ascii/pkcs-3.asc
	// b.	Diffie-Hellman Key Agreement Method
	//	http://www.ietf.org/rfc/rfc2631.txt

	[TestFixture]
	public class DiffieHellmanManagedTest {

		// because most crypto stuff works with byte[] buffers
		static public void AssertEquals (string msg, byte[] array1, byte[] array2) 
		{
			if ((array1 == null) && (array2 == null))
				return;
			if (array1 == null)
				Assert.Fail (msg + " -> First array is NULL");
			if (array2 == null)
				Assert.Fail (msg + " -> Second array is NULL");

			bool a = (array1.Length == array2.Length);
			if (a) {
				for (int i = 0; i < array1.Length; i++) {
					if (array1 [i] != array2 [i]) {
						a = false;
						break;
					}
				}
			}
			if (array1.Length > 0) {
				msg += " -> Expected " + BitConverter.ToString (array1, 0);
				msg += " is different than " + BitConverter.ToString (array2, 0);
			}
			Assert.IsTrue (a, msg);
		}

		[Test]
		public void KeyExchange ()
		{
			// create a new DH instance
			DiffieHellman dh1 = new DiffieHellmanManaged ();
			// export the public parameters of the first DH instance
			DHParameters dhp = dh1.ExportParameters (false);
			// create a second DH instance and initialize it with the public parameters of the first instance
			DiffieHellman dh2 = new DiffieHellmanManaged (dhp.P, dhp.G, 160);
			// generate the public key of the first DH instance
			byte[] ke1 = dh1.CreateKeyExchange ();
			// generate the public key of the second DH instance
			byte[] ke2 = dh2.CreateKeyExchange ();
			// let the first DH instance compute the shared secret using the second DH public key
			byte[] dh1k = dh1.DecryptKeyExchange (ke2);
			// let the second DH instance compute the shared secret using the first DH public key
			byte[] dh2k = dh2.DecryptKeyExchange (ke1);
			// both shared secrets are the same
			Assert.AreEqual (dh1k, dh2k, "Shared Secret");
		}

		// TODO: More is needed !
	}
}
