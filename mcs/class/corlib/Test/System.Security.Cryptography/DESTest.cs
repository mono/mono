//
// TestSuite.System.Security.Cryptography.AsymmetricAlgorithmTest.cs
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class DESTest : Assertion {

		static byte[] wk1 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
		static byte[] wk2 = { 0x1E, 0x1E, 0x1E, 0x1E, 0x0F, 0x0F, 0x0F, 0x0F };
		static byte[] wk3 = { 0xE1, 0xE1, 0xE1, 0xE1, 0xF0, 0xF0, 0xF0, 0xF0 };
		static byte[] wk4 = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

		// parity adjusted
		static byte[] wk1p = { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 };
		static byte[] wk2p = { 0x1F, 0x1F, 0x1F, 0x1F, 0x0E, 0x0E, 0x0E, 0x0E };
		static byte[] wk3p = { 0xE0, 0xE0, 0xE0, 0xE0, 0xF1, 0xF1, 0xF1, 0xF1 };
		static byte[] wk4p = { 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE };

		[Test]
		public void WeakKeys_WithoutParity () 
		{
			Assert ("WK-1", DES.IsWeakKey (wk1));
			Assert ("WK-2", DES.IsWeakKey (wk2));
			Assert ("WK-3", DES.IsWeakKey (wk3));
			Assert ("WK-4", DES.IsWeakKey (wk4));
		}

		[Test]
		public void WeakKeys_WithParity () 
		{
			Assert ("WK-1P", DES.IsWeakKey (wk1p));
			Assert ("WK-2P", DES.IsWeakKey (wk2p));
			Assert ("WK-3P", DES.IsWeakKey (wk3p));
			Assert ("WK-4P", DES.IsWeakKey (wk4p));
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void IsWeakKey_WrongKeyLength () 
		{
			byte[] key = new byte [16]; // 128 bits
			DES.IsWeakKey (key);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void IsWeakKey_Null () 
		{
			DES.IsWeakKey (null);
		}

		static byte[] swk1   = { 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF };
		static byte[] swk2   = { 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00 };
		static byte[] swk3   = { 0x1E, 0xE1, 0x1E, 0xE1, 0x0F, 0xF0, 0x0F, 0xF0 };
		static byte[] swk4   = { 0xE1, 0x1E, 0xE1, 0x1E, 0xF0, 0x0F, 0xF0, 0x0F };
		static byte[] swk5   = { 0x00, 0xE1, 0x00, 0xE1, 0x00, 0xF0, 0x00, 0xF0 };
		static byte[] swk6   = { 0xE1, 0x00, 0xE1, 0x00, 0xF0, 0x00, 0xF0, 0x00 };
		static byte[] swk7   = { 0x1E, 0xFF, 0x1E, 0xFF, 0x0F, 0xFF, 0x0F, 0xFF };
		static byte[] swk8   = { 0xFF, 0x1E, 0xFF, 0x1E, 0xFF, 0x0F, 0xFF, 0x0F };
		static byte[] swk9   = { 0x00, 0x1E, 0x00, 0x1E, 0x00, 0x0F, 0x00, 0x0F };
		static byte[] swk10  = { 0x1E, 0x00, 0x1E, 0x00, 0x0F, 0x00, 0x0F, 0x00 };
		static byte[] swk11  = { 0xE1, 0xFF, 0xE1, 0xFF, 0xF0, 0xFF, 0xF0, 0xFF };
		static byte[] swk12  = { 0xFF, 0xE1, 0xFF, 0xE1, 0xFF, 0xF0, 0xFF, 0xF0 };

		static byte[] swk1p  = { 0x01, 0xFE, 0x01, 0xFE, 0x01, 0xFE, 0x01, 0xFE };
		static byte[] swk2p  = { 0xFE, 0x01, 0xFE, 0x01, 0xFE, 0x01, 0xFE, 0x01 };
		static byte[] swk3p  = { 0x1F, 0xE0, 0x1F, 0xE0, 0x0E, 0xF1, 0x0E, 0xF1 };
		static byte[] swk4p  = { 0xE0, 0x1F, 0xE0, 0x1F, 0xF1, 0x0E, 0xF1, 0x0E };
		static byte[] swk5p  = { 0x01, 0xE0, 0x01, 0xE0, 0x01, 0xF1, 0x01, 0xF1 };
		static byte[] swk6p  = { 0xE0, 0x01, 0xE0, 0x01, 0xF1, 0x01, 0xF1, 0x01 };
		static byte[] swk7p  = { 0x1F, 0xFE, 0x1F, 0xFE, 0x0E, 0xFE, 0x0E, 0xFE };
		static byte[] swk8p  = { 0xFE, 0x1F, 0xFE, 0x1F, 0xFE, 0x0E, 0xFE, 0x0E };
		static byte[] swk9p  = { 0x01, 0x1F, 0x01, 0x1F, 0x01, 0x0E, 0x01, 0x0E };
		static byte[] swk10p = { 0x1F, 0x01, 0x1F, 0x01, 0x0E, 0x01, 0x0E, 0x01 };
		static byte[] swk11p = { 0xE0, 0xFE, 0xE0, 0xFE, 0xF1, 0xFE, 0xF1, 0xFE };
		static byte[] swk12p = { 0xFE, 0xE0, 0xFE, 0xE0, 0xFE, 0xF1, 0xFE, 0xF1 };

		[Test]
		public void SemiWeakKeys_WithoutParity () 
		{
			Assert ("SWK-01", DES.IsSemiWeakKey (swk1));
			Assert ("SWK-02", DES.IsSemiWeakKey (swk2));
			Assert ("SWK-03", DES.IsSemiWeakKey (swk3));
			Assert ("SWK-04", DES.IsSemiWeakKey (swk4));
			Assert ("SWK-05", DES.IsSemiWeakKey (swk5));
			Assert ("SWK-06", DES.IsSemiWeakKey (swk6));
			Assert ("SWK-07", DES.IsSemiWeakKey (swk7));
			Assert ("SWK-08", DES.IsSemiWeakKey (swk8));
			Assert ("SWK-09", DES.IsSemiWeakKey (swk9));
			Assert ("SWK-10", DES.IsSemiWeakKey (swk10));
			Assert ("SWK-11", DES.IsSemiWeakKey (swk11));
			Assert ("SWK-12", DES.IsSemiWeakKey (swk12));
		}

		[Test]
		public void SemiWeakKeys_WithParity () 
		{
			Assert ("SWK-01P", DES.IsSemiWeakKey (swk1p));
			Assert ("SWK-02P", DES.IsSemiWeakKey (swk2p));
			Assert ("SWK-03P", DES.IsSemiWeakKey (swk3p));
			Assert ("SWK-04P", DES.IsSemiWeakKey (swk4p));
			Assert ("SWK-05P", DES.IsSemiWeakKey (swk5p));
			Assert ("SWK-06P", DES.IsSemiWeakKey (swk6p));
			Assert ("SWK-07P", DES.IsSemiWeakKey (swk7p));
			Assert ("SWK-08P", DES.IsSemiWeakKey (swk8p));
			Assert ("SWK-09P", DES.IsSemiWeakKey (swk9p));
			Assert ("SWK-10P", DES.IsSemiWeakKey (swk10p));
			Assert ("SWK-11P", DES.IsSemiWeakKey (swk11p));
			Assert ("SWK-12P", DES.IsSemiWeakKey (swk12p));
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void IsSemiWeakKey_WrongKeyLength () 
		{
			byte[] key = new byte [16]; // 128 bits
			DES.IsSemiWeakKey (key);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void IsSemiWeakKey_Null () 
		{
			DES.IsSemiWeakKey (null);
		}

		[Test]
		public void GetKey () 
		{
			DES des = DES.Create ();
			byte[] key = des.Key;
			AssertEquals ("64 bits", 8, key.Length);

			// we get a copy of the key (not the original)
			string s = BitConverter.ToString (key);
			des.Clear ();
			AssertEquals ("Copy", s, BitConverter.ToString (key));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetKey_Null () 
		{
			DES des = DES.Create ();
			des.Key = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetKey_WrongLength () 
		{
			DES des = DES.Create ();
			des.Key = new byte [16]; // 128 bits
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void SetKey_Weak () 
		{
			DES des = DES.Create ();
			des.Key = wk1;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void SetKey_SemiWeak () 
		{
			DES des = DES.Create ();
			des.Key = swk1;
		}
	}
}
