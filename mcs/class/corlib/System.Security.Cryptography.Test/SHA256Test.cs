//
// System.Security.Cryptography.Test SHA256 NUnit test classes.
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
	public class SHA256HashingTest : TestCase {
		byte[] _dataToHash;
		byte[] _expectedHash;
		
		public SHA256HashingTest (string name, byte[] dataToHash, byte[] expectedHash) : base(name) {
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
			SHA256 sha = new SHA256Managed ();
			byte[] hash;
			
			hash = sha.ComputeHash (_dataToHash);
			
			Assert (ArrayEquals (hash, _expectedHash));
		}

	}
	
	public class SHA256TestSet {
		public SHA256TestSet () {

		}

		public static void Main () {

		}

		public static ITest Suite { 
			get {
				TestSuite suite = new TestSuite ();

				byte[] hash0 = {0xBA, 0x78, 0x16, 0xBF, 0x8F, 0x01, 0xCF, 0xEA, 0x41, 0x41, 0x40, 0xDE, 0x5D, 0xAE, 0x22, 0x23, 0xB0, 0x03, 0x61, 0xA3, 0x96, 0x17, 0x7A, 0x9C, 0xB4, 0x10, 0xFF, 0x61, 0xF2, 0x00, 0x15, 0xAD};
				suite.AddTest (new SHA256HashingTest ("abc", Encoding.UTF8.GetBytes("abc"), hash0));

				byte[] hash1 = {0x24, 0x8D, 0x6A, 0x61, 0xD2, 0x06, 0x38, 0xB8, 0xE5, 0xC0, 0x26, 0x93, 0x0C, 0x3E, 0x60, 0x39, 0xA3, 0x3C, 0xE4, 0x59, 0x64, 0xFF, 0x21, 0x67, 0xF6, 0xEC, 0xED, 0xD4, 0x19, 0xDB, 0x06, 0xC1};
				suite.AddTest (new SHA256HashingTest ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq", Encoding.UTF8.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"), hash1));

				//byte[] hash2 = {};
				//suite.AddTest (new SHA256HashingTest ("abc", Encoding.UTF8.GetBytes("abc"), hash2));

				//byte[] hash3 = {};
				//suite.AddTest (new SHA256HashingTest ("abcdefghijklmnopqrstuvwxyz", Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz"), hash3));

				//byte[] hash4 = {};
				//suite.AddTest (new SHA256HashingTest ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq", Encoding.UTF8.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"), hash4));

				return suite;
			}
		}
	}
}

