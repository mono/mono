//
// System.Security.Cryptography.Test MD5 NUnit test classes.
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
//


using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace System.Security.Cryptography.Test {
	public class MD5HashingTest : TestCase {
		byte[] _dataToHash;
		byte[] _expectedHash;
		
		public MD5HashingTest (string name, byte[] dataToHash, byte[] expectedHash) : base(name) {
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
			MD5 md5 = new MD5CryptoServiceProvider ();
			byte[] hash;
			
			hash = md5.ComputeHash (_dataToHash);
			
			Assert (ArrayEquals (hash, _expectedHash));
		}

	}
	
	public class MD5TestSet {
		public MD5TestSet () {

		}

		public static void Main () {

		}

		public static ITest Suite { 
			get {
				TestSuite suite = new TestSuite ();

				byte[] hash0 = {0xD4, 0x1D, 0x8C, 0xD9, 0x8F, 0x00, 0xB2, 0x04, 0xE9, 0x80, 0x09, 0x98, 0xEC, 0xF8, 0x42, 0x7E};
				suite.AddTest (new MD5HashingTest ("(blank hash)", new byte[0], hash0));

				byte[] hash1 = {0x0C, 0xC1, 0x75, 0xB9, 0xC0, 0xF1, 0xB6, 0xA8, 0x31, 0xC3, 0x99, 0xE2, 0x69, 0x77, 0x26, 0x61};
				suite.AddTest (new MD5HashingTest ("a", Encoding.UTF8.GetBytes("a"), hash1));

				byte[] hash2 = {0x90, 0x01, 0x50, 0x98, 0x3C, 0xD2, 0x4F, 0xB0, 0xD6, 0x96, 0x3F, 0x7D, 0x28, 0xE1, 0x7F, 0x72};
				suite.AddTest (new MD5HashingTest ("abc", Encoding.UTF8.GetBytes("abc"), hash2));

				byte[] hash3 = {0xC3, 0xFC, 0xD3, 0xD7, 0x61, 0x92, 0xE4, 0x00, 0x7D, 0xFB, 0x49, 0x6C, 0xCA, 0x67, 0xE1, 0x3B};
				suite.AddTest (new MD5HashingTest ("abcdefghijklmnopqrstuvwxyz", Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz"), hash3));

				byte[] hash4 = {0xd1, 0x74, 0xab, 0x98, 0xd2, 0x77, 0xd9, 0xf5, 0xa5, 0x61, 0x1c, 0x2c, 0x9f, 0x41, 0x9d, 0x9f};
				suite.AddTest (new MD5HashingTest ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"), hash4));

				return suite;
			}
		}
	}
}

