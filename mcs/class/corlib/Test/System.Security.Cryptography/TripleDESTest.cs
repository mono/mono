//
// TripleDESTest.cs - NUnit Test Cases for TripleDES
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class TripleDESTest : Assertion {

		[Test]
		public void Key ()
		{
			TripleDES algo = TripleDES.Create ();
			algo.GenerateKey ();
			algo.GenerateIV ();
			AssertEquals ("Key Size", 192, algo.KeySize);
			AssertEquals ("Key Length", 24, algo.Key.Length);
			AssertEquals ("IV Length", 8, algo.IV.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void KeyNull () 
		{
			TripleDES algo = TripleDES.Create ();
			algo.Key = null;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void KeyWeak128bits () 
		{
			byte[] wk128 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
			TripleDES algo = TripleDES.Create ();
			algo.Key = wk128;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void KeyWeak192bits_AB () 
		{
			byte[] wk192 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD };
			TripleDES algo = TripleDES.Create ();
			algo.Key = wk192;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void KeyWeak192bits_BC () 
		{
			byte[] wk192 = { 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
			TripleDES algo = TripleDES.Create ();
			algo.Key = wk192;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void KeyWrongLength () 
		{
			byte[] wk64 = { 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD };
			TripleDES algo = TripleDES.Create ();
			algo.Key = wk64;
		}
	}
}
