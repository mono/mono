//
// System.Security.Cryptography.Test SHA1 NUnit test classes.
//
// Author:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace System.Security.Cryptography.Test {
	public class SHA1HashingTest : TestCase {
		byte[] _dataToHash;
		byte[] _expectedHash;
		
		public SHA1HashingTest (string name, byte[] dataToHash, byte[] expectedHash) : base(name) {
			_dataToHash = dataToHash;
			_expectedHash = expectedHash;
		}
		
		public static bool ArrayEquals (byte[] arr1, byte[] arr2) {
			int i;
			if (arr1.GetLength(0) != arr2.GetLength(0))
				return false;
		
			for (i=0; i<arr1.GetLength(0); i++) {
				if (arr1[i] != arr2[i])
					return false;
			}
		
			return true;
		}

		protected override void RunTest () {
			SHA1 sha = new SHA1CryptoServiceProvider ();
			byte[] hash;
			
			hash = sha.ComputeHash (_dataToHash);
			
			Assert (ArrayEquals (hash, _expectedHash));
		}

	}
	
	public class SHA1TestSet {
		public SHA1TestSet () {

		}

		public static void Main () {

		}

		public static ITest Suite { 
			get {
				TestSuite suite = new TestSuite ();

				byte[] hash0 = {0xDA, 0x39, 0xA3, 0xEE, 0x5E, 0x6B, 0x4B, 0x0D, 0x32, 0x55, 0xBF, 0xEF, 0x95, 0x60, 0x18, 0x90, 0xAF, 0xD8, 0x07, 0x09};
				suite.AddTest (new SHA1HashingTest ("(blank hash)", new byte[0], hash0));

				byte[] hash1 = {0x86, 0xF7, 0xE4, 0x37, 0xFA, 0xA5, 0xA7, 0xFC, 0xE1, 0x5D, 0x1D, 0xDC, 0xB9, 0xEA, 0xEA, 0xEA, 0x37, 0x76, 0x67, 0xB8};
				suite.AddTest (new SHA1HashingTest ("a", Encoding.UTF8.GetBytes("a"), hash1));

				byte[] hash2 = {0xA9, 0x99, 0x3E, 0x36, 0x47, 0x06, 0x81, 0x6A, 0xBA, 0x3E, 0x25, 0x71, 0x78, 0x50, 0xC2, 0x6C, 0x9C, 0xD0, 0xD8, 0x9D};
				suite.AddTest (new SHA1HashingTest ("abc", Encoding.UTF8.GetBytes("abc"), hash2));

				byte[] hash3 = {0x32, 0xD1, 0x0C, 0x7B, 0x8C, 0xF9, 0x65, 0x70, 0xCA, 0x04, 0xCE, 0x37, 0xF2, 0xA1, 0x9D, 0x84, 0x24, 0x0D, 0x3A, 0x89};
				suite.AddTest (new SHA1HashingTest ("abcdefghijklmnopqrstuvwxyz", Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz"), hash3));

				byte[] hash4 = {0x84, 0x98, 0x3E, 0x44, 0x1C, 0x3B, 0xD2, 0x6E, 0xBA, 0xAE, 0x4A, 0xA1, 0xF9, 0x51, 0x29, 0xE5, 0xE5, 0x46, 0x70, 0xF1};
				suite.AddTest (new SHA1HashingTest ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq", Encoding.UTF8.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"), hash4));

				return suite;
			}
		}
	}
}

