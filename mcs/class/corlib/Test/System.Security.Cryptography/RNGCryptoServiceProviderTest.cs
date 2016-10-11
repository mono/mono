//
// TestSuite.System.Security.Cryptography.RNGCryptoServiceProviderTest.cs
//
// Authors:
//      Mark Crichton (crichton@gimp.org)
//	Sebastien Pouliot  (sebastien@ximian.com)
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class RNGCryptoServiceProviderTest {

		private RNGCryptoServiceProvider _algo;
		
		[SetUp]
		public void SetUp () 
		{
			_algo = new RNGCryptoServiceProvider ();
		}

		[Test]
		public void ConstructorByteArray () 
		{
			byte[] array = new byte [16];
			byte[] seed = (byte[]) array.Clone ();
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider (seed);
			Assert.AreEqual (BitConverter.ToString (array), BitConverter.ToString (seed), "Seed");
		}

		[Test]
		public void ConstructorByteArray_Null () 
		{
			byte[] array = null;
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider (array);
		}

		[Test]
		public void ConstructorCsp_Null () 
		{
			CspParameters csp = null;
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider (csp);
		}

		[Test]
		public void ConstructorString () 
		{
			string s = "Mono seed";
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider (s);
		}

		[Test]
		public void ConstructorString_Null () 
		{
			string s = null;
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider (s);
		}

		[Test]
		public void GetBytes () 
		{
			byte[] random = new byte [25];
			// The C code doesn't throw an exception yet.
			_algo.GetBytes (random);
		}

		[Test]
		public void GetNonZeroBytes () 
		{
			byte[] random = new byte [25];
			// This one we can check...
			_algo.GetNonZeroBytes (random);
			
			foreach (Byte rnd_byte in random) {
				Assert.IsTrue(rnd_byte != 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetBytesNull () 
		{
			_algo.GetBytes (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetNonZeroBytesNull () 
		{
			_algo.GetNonZeroBytes (null);
		}
	}
}
