//
// CryptoConvertTest.cs - NUnit Test Cases for CryptoConvert
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using Mono.Security.Cryptography;

namespace MonoTests.Mono.Security.Cryptography {

	[TestFixture]
	public class CryptoConvertTest {

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
			msg += " -> Expected " + BitConverter.ToString (array1, 0);
			msg += " is different than " + BitConverter.ToString (array2, 0);
			Assert.IsTrue (a, msg);
		}

		// strongname generated using "sn -k unit.snk"
		static byte[] strongName = { 
			0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x32, 
			0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x7F, 0x7C, 0xEA, 0x4A, 
			0x28, 0x33, 0xD8, 0x3C, 0x86, 0x90, 0x86, 0x91, 0x11, 0xBB, 0x30, 0x0D, 
			0x3D, 0x69, 0x04, 0x4C, 0x48, 0xF5, 0x4F, 0xE7, 0x64, 0xA5, 0x82, 0x72, 
			0x5A, 0x92, 0xC4, 0x3D, 0xC5, 0x90, 0x93, 0x41, 0xC9, 0x1D, 0x34, 0x16, 
			0x72, 0x2B, 0x85, 0xC1, 0xF3, 0x99, 0x62, 0x07, 0x32, 0x98, 0xB7, 0xE4, 
			0xFA, 0x75, 0x81, 0x8D, 0x08, 0xB9, 0xFD, 0xDB, 0x00, 0x25, 0x30, 0xC4, 
			0x89, 0x13, 0xB6, 0x43, 0xE8, 0xCC, 0xBE, 0x03, 0x2E, 0x1A, 0x6A, 0x4D, 
			0x36, 0xB1, 0xEB, 0x49, 0x26, 0x6C, 0xAB, 0xC4, 0x29, 0xD7, 0x8F, 0x25, 
			0x11, 0xA4, 0x7C, 0x81, 0x61, 0x97, 0xCB, 0x44, 0x2D, 0x80, 0x49, 0x93, 
			0x48, 0xA7, 0xC9, 0xAB, 0xDB, 0xCF, 0xA3, 0x34, 0xCB, 0x6B, 0x86, 0xE0, 
			0x4D, 0x27, 0xFC, 0xA7, 0x4F, 0x36, 0xCA, 0x13, 0x42, 0xD3, 0x83, 0xC4, 
			0x06, 0x6E, 0x12, 0xE0, 0xA1, 0x3D, 0x9F, 0xA9, 0xEC, 0xD1, 0xC6, 0x08, 
			0x1B, 0x3D, 0xF5, 0xDB, 0x4C, 0xD4, 0xF0, 0x2C, 0xAA, 0xFC, 0xBA, 0x18, 
			0x6F, 0x48, 0x7E, 0xB9, 0x47, 0x68, 0x2E, 0xF6, 0x1E, 0x67, 0x1C, 0x7E, 
			0x0A, 0xCE, 0x10, 0x07, 0xC0, 0x0C, 0xAD, 0x5E, 0xC1, 0x53, 0x70, 0xD5, 
			0xE7, 0x25, 0xCA, 0x37, 0x5E, 0x49, 0x59, 0xD0, 0x67, 0x2A, 0xBE, 0x92, 
			0x36, 0x86, 0x8A, 0xBF, 0x3E, 0x17, 0x04, 0xFB, 0x1F, 0x46, 0xC8, 0x10, 
			0x5C, 0x93, 0x02, 0x43, 0x14, 0x96, 0x6A, 0xD9, 0x87, 0x17, 0x62, 0x7D, 
			0x3A, 0x45, 0xBE, 0x35, 0xDE, 0x75, 0x0B, 0x2A, 0xCE, 0x7D, 0xF3, 0x19, 
			0x85, 0x4B, 0x0D, 0x6F, 0x8D, 0x15, 0xA3, 0x60, 0x61, 0x28, 0x55, 0x46, 
			0xCE, 0x78, 0x31, 0x04, 0x18, 0x3C, 0x56, 0x4A, 0x3F, 0xA4, 0xC9, 0xB1, 
			0x41, 0xED, 0x22, 0x80, 0xA1, 0xB3, 0xE2, 0xC7, 0x1B, 0x62, 0x85, 0xE4, 
			0x81, 0x39, 0xCB, 0x1F, 0x95, 0xCC, 0x61, 0x61, 0xDF, 0xDE, 0xF3, 0x05, 
			0x68, 0xB9, 0x7D, 0x4F, 0xFF, 0xF3, 0xC0, 0x0A, 0x25, 0x62, 0xD9, 0x8A, 
			0x8A, 0x9E, 0x99, 0x0B, 0xFB, 0x85, 0x27, 0x8D, 0xF6, 0xD4, 0xE1, 0xB9, 
			0xDE, 0xB4, 0x16, 0xBD, 0xDF, 0x6A, 0x25, 0x9C, 0xAC, 0xCD, 0x91, 0xF7, 
			0xCB, 0xC1, 0x81, 0x22, 0x0D, 0xF4, 0x7E, 0xEC, 0x0C, 0x84, 0x13, 0x5A, 
			0x74, 0x59, 0x3F, 0x3E, 0x61, 0x00, 0xD6, 0xB5, 0x4A, 0xA1, 0x04, 0xB5, 
			0xA7, 0x1C, 0x29, 0xD0, 0xE1, 0x11, 0x19, 0xD7, 0x80, 0x5C, 0xEE, 0x08, 
			0x15, 0xEB, 0xC9, 0xA8, 0x98, 0xF5, 0xA0, 0xF0, 0x92, 0x2A, 0xB0, 0xD3, 
			0xC7, 0x8C, 0x8D, 0xBB, 0x88, 0x96, 0x4F, 0x18, 0xF0, 0x8A, 0xF9, 0x31, 
			0x9E, 0x44, 0x94, 0x75, 0x6F, 0x78, 0x04, 0x10, 0xEC, 0xF3, 0xB0, 0xCE, 
			0xA0, 0xBE, 0x7B, 0x25, 0xE1, 0xF7, 0x8A, 0xA8, 0xD4, 0x63, 0xC2, 0x65, 
			0x47, 0xCC, 0x5C, 0xED, 0x7D, 0x8B, 0x07, 0x4D, 0x76, 0x29, 0x53, 0xAC, 
			0x27, 0x8F, 0x5D, 0x78, 0x56, 0xFA, 0x99, 0x45, 0xA2, 0xCC, 0x65, 0xC4, 
			0x54, 0x13, 0x9F, 0x38, 0x41, 0x7A, 0x61, 0x0E, 0x0D, 0x34, 0xBC, 0x11, 
			0xAF, 0xE2, 0xF1, 0x8B, 0xFA, 0x2B, 0x54, 0x6C, 0xA3, 0x6C, 0x09, 0x1F, 
			0x0B, 0x43, 0x9B, 0x07, 0x95, 0x83, 0x3F, 0x97, 0x99, 0x89, 0xF5, 0x51, 
			0x41, 0xF6, 0x8E, 0x5D, 0xEF, 0x6D, 0x24, 0x71, 0x41, 0x7A, 0xAF, 0xBE, 
			0x81, 0x71, 0xAB, 0x76, 0x2F, 0x1A, 0x5A, 0xBA, 0xF3, 0xA6, 0x65, 0x7A, 
			0x80, 0x50, 0xCE, 0x23, 0xC3, 0xC7, 0x53, 0xB0, 0x7C, 0x97, 0x77, 0x27, 
			0x70, 0x98, 0xAE, 0xB5, 0x24, 0x66, 0xE1, 0x60, 0x39, 0x41, 0xDA, 0x54, 
			0x01, 0x64, 0xFB, 0x10, 0x33, 0xCE, 0x8B, 0xBE, 0x27, 0xD4, 0x21, 0x57, 
			0xCC, 0x0F, 0x1A, 0xC1, 0x3D, 0xF3, 0xCC, 0x39, 0xF0, 0x2F, 0xAE, 0xF1, 
			0xC0, 0xCD, 0x3B, 0x23, 0x87, 0x49, 0x7E, 0x40, 0x32, 0x6A, 0xD3, 0x96, 
			0x4A, 0xE5, 0x5E, 0x6E, 0x26, 0xFD, 0x8A, 0xCF, 0x7E, 0xFC, 0x37, 0xDE, 
			0x39, 0x0C, 0x53, 0x81, 0x75, 0x08, 0xAF, 0x6B, 0x39, 0x6C, 0xFB, 0xC9, 
			0x79, 0xC0, 0x9B, 0x5F, 0x34, 0x86, 0xB2, 0xDE, 0xC4, 0x19, 0x84, 0x5F, 
			0x0E, 0xED, 0x9B, 0xB8, 0xD3, 0x17, 0xDA, 0x78 };

		static string strongNameString = "<RSAKeyValue><Modulus>4BJuBsSD00ITyjZPp/wnTeCGa8s0o8/bq8mnSJNJgC1Ey5dhgXykESWP1ynEq2wmSeuxNk1qGi4DvszoQ7YTicQwJQDb/bkIjYF1+uS3mDIHYpnzwYUrchY0HclBk5DFPcSSWnKCpWTnT/VITARpPQ0wuxGRhpCGPNgzKErqfH8=</Modulus><Exponent>AQAB</Exponent><P>+wQXPr+KhjaSvipn0FlJXjfKJefVcFPBXq0MwAcQzgp+HGce9i5oR7l+SG8YuvyqLPDUTNv1PRsIxtHsqZ89oQ==</P><Q>5IViG8fis6GAIu1BscmkP0pWPBgEMXjORlUoYWCjFY1vDUuFGfN9zioLdd41vkU6fWIXh9lqlhRDApNcEMhGHw==</Q><DP>Pj9ZdFoThAzsfvQNIoHBy/eRzaycJWrfvRa03rnh1PaNJ4X7C5meiorZYiUKwPP/T325aAXz3t9hYcyVH8s5gQ==</DP><DQ>qIr34SV7vqDOsPPsEAR4b3WURJ4x+YrwGE+WiLuNjMfTsCqS8KD1mKjJ6xUI7lyA1xkR4dApHKe1BKFKtdYAYQ==</DQ><InverseQ>UfWJmZc/g5UHm0MLHwlso2xUK/qL8eKvEbw0DQ5hekE4nxNUxGXMokWZ+lZ4XY8nrFMpdk0Hi33tXMxHZcJj1A==</InverseQ><D>eNoX07ib7Q5fhBnE3rKGNF+bwHnJ+2w5a68IdYFTDDneN/x+z4r9Jm5e5UqW02oyQH5JhyM7zcDxri/wOczzPcEaD8xXIdQnvovOMxD7ZAFU2kE5YOFmJLWumHAnd5d8sFPHwyPOUIB6ZabzuloaL3arcYG+r3pBcSRt712O9kE=</D></RSAKeyValue>";

		// strongname public key extracted using "sn -p unit.snk unit.pub"
		static byte[] strongNamePublicKey = { 
			0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 
			0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 
			0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x7F, 0x7C, 0xEA, 0x4A, 
			0x28, 0x33, 0xD8, 0x3C, 0x86, 0x90, 0x86, 0x91, 0x11, 0xBB, 0x30, 0x0D, 
			0x3D, 0x69, 0x04, 0x4C, 0x48, 0xF5, 0x4F, 0xE7, 0x64, 0xA5, 0x82, 0x72, 
			0x5A, 0x92, 0xC4, 0x3D, 0xC5, 0x90, 0x93, 0x41, 0xC9, 0x1D, 0x34, 0x16, 
			0x72, 0x2B, 0x85, 0xC1, 0xF3, 0x99, 0x62, 0x07, 0x32, 0x98, 0xB7, 0xE4, 
			0xFA, 0x75, 0x81, 0x8D, 0x08, 0xB9, 0xFD, 0xDB, 0x00, 0x25, 0x30, 0xC4, 
			0x89, 0x13, 0xB6, 0x43, 0xE8, 0xCC, 0xBE, 0x03, 0x2E, 0x1A, 0x6A, 0x4D, 
			0x36, 0xB1, 0xEB, 0x49, 0x26, 0x6C, 0xAB, 0xC4, 0x29, 0xD7, 0x8F, 0x25, 
			0x11, 0xA4, 0x7C, 0x81, 0x61, 0x97, 0xCB, 0x44, 0x2D, 0x80, 0x49, 0x93, 
			0x48, 0xA7, 0xC9, 0xAB, 0xDB, 0xCF, 0xA3, 0x34, 0xCB, 0x6B, 0x86, 0xE0, 
			0x4D, 0x27, 0xFC, 0xA7, 0x4F, 0x36, 0xCA, 0x13, 0x42, 0xD3, 0x83, 0xC4, 
			0x06, 0x6E, 0x12, 0xE0 };

		static string strongNamePublicKeyString = "<RSAKeyValue><Modulus>4BJuBsSD00ITyjZPp/wnTeCGa8s0o8/bq8mnSJNJgC1Ey5dhgXykESWP1ynEq2wmSeuxNk1qGi4DvszoQ7YTicQwJQDb/bkIjYF1+uS3mDIHYpnzwYUrchY0HclBk5DFPcSSWnKCpWTnT/VITARpPQ0wuxGRhpCGPNgzKErqfH8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

		static 	byte[] strongNameNUnit = { 
			0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x32, 
			0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0xCF, 0x4A, 0x0B, 0xBF, 
			0x35, 0x4B, 0x6D, 0x8D, 0x0C, 0x39, 0xE7, 0xBC, 0x40, 0xDD, 0x0B, 0xE1, 
			0x6A, 0x32, 0xBA, 0x9D, 0x76, 0x3E, 0x8D, 0x04, 0xFD, 0x95, 0x91, 0xB9, 
			0x2D, 0x72, 0x69, 0xDD, 0x09, 0xC2, 0xC6, 0x5E, 0xC7, 0x56, 0x3C, 0xE3, 
			0x93, 0xAC, 0xA7, 0x19, 0x13, 0xBE, 0xA1, 0x3D, 0xD6, 0xA2, 0x0D, 0x67, 
			0x6E, 0xD7, 0xDD, 0xC7, 0x26, 0xF8, 0x46, 0xFC, 0xE6, 0x68, 0x00, 0xBB, 
			0x03, 0x49, 0x03, 0x61, 0x9A, 0x1B, 0xAA, 0x52, 0x0F, 0x5F, 0x75, 0x89, 
			0x46, 0xCF, 0x2B, 0x4A, 0xF6, 0xBA, 0x7C, 0x31, 0x0D, 0x02, 0xD0, 0x92, 
			0xA5, 0xCF, 0x51, 0xBE, 0x6D, 0x52, 0xE8, 0x86, 0x33, 0xF5, 0x02, 0x47, 
			0x4B, 0x4F, 0x46, 0x1D, 0x85, 0x0B, 0x63, 0x21, 0x9C, 0x09, 0xA3, 0x37, 
			0x3A, 0x23, 0xD7, 0x31, 0x56, 0xEE, 0x03, 0xD6, 0xC7, 0xB3, 0x8C, 0x36, 
			0xD1, 0x21, 0x1F, 0xAC, 0xCD, 0xA7, 0x7F, 0x90, 0x33, 0x0B, 0x49, 0x62, 
			0xA6, 0xAD, 0xD1, 0xF5, 0x65, 0xC2, 0x78, 0x94, 0x0F, 0xB5, 0xC4, 0x4C, 
			0x3A, 0xC3, 0x06, 0xD1, 0x6B, 0x7C, 0x87, 0x2B, 0x57, 0xE2, 0xBB, 0x5D, 
			0x10, 0x85, 0x6E, 0xD7, 0xFC, 0x2D, 0x5F, 0xF4, 0x8A, 0xEA, 0xA7, 0xD7, 
			0x39, 0x84, 0x22, 0x12, 0xCF, 0x6E, 0x13, 0xC6, 0x45, 0x3B, 0xDB, 0xFD, 
			0xCE, 0xBD, 0x2B, 0x5A, 0x18, 0x29, 0xDE, 0xD9, 0x0B, 0x69, 0xAC, 0x30, 
			0x7B, 0x19, 0x2C, 0x35, 0x38, 0xFE, 0x5A, 0x73, 0x72, 0x32, 0xA5, 0x47, 
			0x48, 0xEA, 0xD7, 0x05, 0x83, 0x93, 0x5A, 0xAC, 0x59, 0xDC, 0x08, 0xE2, 
			0x44, 0x67, 0xAA, 0x0E, 0xB1, 0xA0, 0x73, 0xAC, 0xFB, 0x62, 0x2C, 0x31, 
			0x15, 0xE7, 0x83, 0xB5, 0x3F, 0xCF, 0xA4, 0x4C, 0x23, 0x57, 0x3B, 0x61, 
			0x59, 0x23, 0x50, 0x0E, 0xE7, 0xAE, 0x8E, 0x69, 0x78, 0x41, 0x3F, 0xCA, 
			0x95, 0xAC, 0x41, 0x59, 0x71, 0x25, 0xDA, 0x58, 0x91, 0x04, 0x8B, 0xBA, 
			0xF9, 0x5B, 0xF1, 0x33, 0xD4, 0x4F, 0x43, 0x99, 0x10, 0x6A, 0x2A, 0x4D, 
			0x78, 0xE7, 0x21, 0xE9, 0x47, 0x65, 0x81, 0xE9, 0x74, 0xB2, 0x6F, 0xE5, 
			0xFA, 0xB9, 0xEC, 0x37, 0x5B, 0x1D, 0x21, 0x31, 0x92, 0x5C, 0xCF, 0xFF, 
			0xBC, 0x34, 0xA5, 0x44, 0x48, 0xF7, 0xE3, 0xF1, 0x28, 0xE1, 0xC6, 0x39, 
			0x8F, 0x00, 0xC9, 0x70, 0x4B, 0x06, 0x0B, 0x0C, 0x66, 0x1E, 0xCF, 0x54, 
			0xEA, 0xE2, 0xA8, 0xFC, 0xE4, 0xBA, 0x1C, 0xA0, 0xA9, 0x71, 0x16, 0x51, 
			0x97, 0xA8, 0xBC, 0x4A, 0x95, 0x42, 0x71, 0x9F, 0x01, 0x5B, 0xEC, 0x07, 
			0x69, 0x7E, 0xB1, 0xB6, 0x92, 0x3D, 0x55, 0xE1, 0x48, 0xA6, 0x8F, 0x47, 
			0x5A, 0xBF, 0x47, 0x00, 0xF8, 0x1E, 0x2F, 0xE4, 0x62, 0x9F, 0xDD, 0x2F, 
			0x33, 0x2F, 0x9B, 0xF1, 0x5C, 0x93, 0x3E, 0x83, 0x65, 0xEA, 0x12, 0x4E, 
			0x9E, 0xDA, 0x6F, 0x6A, 0x51, 0x03, 0x8C, 0x2F, 0x47, 0xEB, 0x5C, 0x5B, 
			0x40, 0xC2, 0xE8, 0x4D, 0xC5, 0xA3, 0xC4, 0x8D, 0x30, 0x9A, 0xD4, 0x8E, 
			0x7D, 0x4D, 0xA6, 0x89, 0x81, 0x72, 0x82, 0x47, 0x5F, 0xAA, 0x4B, 0xBB, 
			0xD5, 0x8C, 0x75, 0x78, 0x21, 0x0F, 0x4B, 0xAA, 0x2E, 0x12, 0xF9, 0xF5, 
			0x81, 0x88, 0x72, 0x22, 0xD7, 0x77, 0xB4, 0x5F, 0x85, 0x12, 0xE5, 0xC7, 
			0x31, 0x2F, 0x4E, 0x3C, 0x63, 0xE9, 0x47, 0x79, 0x3C, 0x21, 0x5B, 0xDD, 
			0xED, 0x1C, 0x6A, 0xFD, 0x87, 0x01, 0xD2, 0x34, 0x0C, 0xEC };

		[Test]
		public void FromCapiKeyBlob () 
		{
			// keypair
			RSA rsa = CryptoConvert.FromCapiKeyBlob (strongName, 0);
			Assert.AreEqual (strongNameString, rsa.ToXmlString (true), "KeyPair");
			Assert.AreEqual (strongNamePublicKeyString, rsa.ToXmlString (false), "PublicKey-1");

			// public key (direct)
			rsa = CryptoConvert.FromCapiKeyBlob (strongNamePublicKey, 12);
			Assert.AreEqual (strongNamePublicKeyString, rsa.ToXmlString (false), "PublicKey-2");

			// public key (indirect - inside header)
			rsa = CryptoConvert.FromCapiKeyBlob (strongNamePublicKey, 0);
			Assert.AreEqual (strongNamePublicKeyString, rsa.ToXmlString (false), "PublicKey-3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromCapiKeyBlob_Null () 
		{
			RSA rsa = CryptoConvert.FromCapiKeyBlob (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromCapiKeyBlob_InvalidOffset () 
		{
			RSA rsa = CryptoConvert.FromCapiKeyBlob (new byte [0], 0);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void FromCapiKeyBlob_UnknownBlob () 
		{
			byte[] blob = new byte [160];
			RSA rsa = CryptoConvert.FromCapiKeyBlob (blob, 12);
		}

		[Test]
		public void FromCapiPrivateKeyBlob () 
		{
			RSA rsa = CryptoConvert.FromCapiPrivateKeyBlob (strongName, 0);
			Assert.AreEqual (strongNameString, rsa.ToXmlString (true), "KeyPair");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromCapiPrivateKeyBlob_Null () 
		{
			RSA rsa = CryptoConvert.FromCapiPrivateKeyBlob (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromCapiPrivateKeyBlob_InvalidOffset () 
		{
			RSA rsa = CryptoConvert.FromCapiPrivateKeyBlob (new byte [0], 0);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void FromCapiPrivateKeyBlob_Invalid () 
		{
			RSA rsa = CryptoConvert.FromCapiPrivateKeyBlob (strongNamePublicKey, 12);
		}

		[Test]
		public void FromCapiPublicKeyBlob () 
		{
			RSA rsa = CryptoConvert.FromCapiPublicKeyBlob (strongNamePublicKey, 12);
			Assert.AreEqual (strongNamePublicKeyString, rsa.ToXmlString (false), "PublicKey");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromCapiPublicKeyBlob_Null () 
		{
			RSA rsa = CryptoConvert.FromCapiPublicKeyBlob (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromCapiPublicKeyBlob_InvalidOffset () 
		{
			RSA rsa = CryptoConvert.FromCapiPublicKeyBlob (new byte [0], 0);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void FromCapiPublicKeyBlob_Invalid () 
		{
			RSA rsa = CryptoConvert.FromCapiPublicKeyBlob (strongName, 0);
		}

		[Test]
		public void ToCapiKeyBlob_AsymmetricAlgorithm () 
		{
			AsymmetricAlgorithm rsa = RSA.Create ();
			rsa.FromXmlString (strongNameString);
			byte[] keypair = CryptoConvert.ToCapiKeyBlob (rsa, true);
			AssertEquals ("RSA-KeyPair", strongName, keypair);

			byte[] publicKey = CryptoConvert.ToCapiKeyBlob (rsa, false);
			Assert.AreEqual (BitConverter.ToString (strongNamePublicKey, 12), BitConverter.ToString (publicKey), "RSA-PublicKey");
			
			AsymmetricAlgorithm dsa = DSA.Create ();
			dsa.FromXmlString (dsaKeyPairString);
			AssertEquals ("DSA-KeyPair", dsaPrivBlob, CryptoConvert.ToCapiKeyBlob (dsa, true));
			Assert.AreEqual (BitConverter.ToString (dsaPubBlob), BitConverter.ToString (CryptoConvert.ToCapiKeyBlob (dsa, false)), "DSA-PublicKey");
		}

		[Test]
		public void ToCapiKeyBlob_RSA () 
		{
			RSA rsa = RSA.Create ();
			rsa.FromXmlString (strongNameString);
			byte[] keypair = CryptoConvert.ToCapiKeyBlob (rsa, true);
			AssertEquals ("KeyPair", strongName, keypair);

			byte[] publicKey = CryptoConvert.ToCapiKeyBlob (rsa, false);
			Assert.AreEqual (BitConverter.ToString (strongNamePublicKey, 12), BitConverter.ToString (publicKey), "PublicKey");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToCapiKeyBlob_AsymmetricNull () 
		{
			AsymmetricAlgorithm aa = null;
			CryptoConvert.ToCapiKeyBlob (aa, false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToCapiKeyBlob_RSANull () 
		{
			RSA rsa = null;
			CryptoConvert.ToCapiKeyBlob (rsa, false);
		}

		[Test]
		public void ToCapiPrivateKeyBlob () 
		{
			RSA rsa = RSA.Create ();
			rsa.FromXmlString (strongNameString);
			byte[] keypair = CryptoConvert.ToCapiPrivateKeyBlob (rsa);
			AssertEquals ("KeyPair", strongName, keypair);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ToCapiPrivateKeyBlob_PublicKeyOnly () 
		{
			RSA rsa = RSA.Create ();
			rsa.FromXmlString (strongNamePublicKeyString);
			byte[] publicKey = CryptoConvert.ToCapiPrivateKeyBlob (rsa);
		}

		[Test]
		public void ToCapiPublicKeyBlob () 
		{
			RSA rsa = RSA.Create ();
			// full keypair
			rsa.FromXmlString (strongNameString);
			byte[] publicKey = CryptoConvert.ToCapiPublicKeyBlob (rsa);
			Assert.AreEqual (BitConverter.ToString (strongNamePublicKey, 12), BitConverter.ToString (publicKey), "PublicKey-1");
			// public key only
			rsa.FromXmlString (strongNamePublicKeyString);
			publicKey = CryptoConvert.ToCapiPublicKeyBlob (rsa);
			Assert.AreEqual (BitConverter.ToString (strongNamePublicKey, 12), BitConverter.ToString (publicKey), "PublicKey-2");
		}

		/* DSA key tests */
		static byte[] dsaPrivBlob = { 7, 2, 0, 0, 0, 34, 0, 0, 68, 83,
			83, 50, 0, 4, 0, 0, 69, 144, 99, 249,
			41, 174, 97, 185, 66, 236, 179, 197, 182, 101,
			146, 165, 47, 36, 234, 199, 170, 99, 97, 8,
			224, 141, 189, 97, 86, 96, 240, 53, 69, 135,
			123, 169, 165, 64, 50, 51, 144, 131, 158, 151,
			218, 224, 159, 194, 166, 107, 132, 201, 148, 74,
			38, 62, 231, 221, 157, 216, 239, 66, 248, 68,
			26, 23, 123, 253, 157, 123, 65, 199, 109, 138,
			231, 217, 247, 170, 81, 51, 43, 252, 66, 210,
			75, 127, 68, 147, 141, 213, 174, 251, 109, 152,
			244, 113, 14, 194, 198, 222, 69, 157, 146, 154,
			224, 158, 46, 181, 204, 251, 10, 124, 153, 26,
			239, 105, 199, 53, 43, 51, 255, 118, 213, 58,
			111, 212, 166, 235, 29, 143, 53, 193, 210, 7,
			78, 198, 7, 3, 219, 0, 57, 81, 179, 46,
			58, 180, 61, 222, 145, 109, 165, 23, 119, 162,
			91, 55, 48, 230, 133, 54, 103, 58, 139, 99,
			146, 149, 90, 197, 167, 60, 164, 35, 90, 168,
			150, 138, 107, 17, 219, 191, 163, 4, 98, 13,
			109, 98, 122, 178, 247, 46, 73, 124, 53, 228,
			137, 21, 20, 45, 214, 217, 202, 51, 87, 45,
			78, 190, 19, 209, 249, 13, 31, 88, 52, 108,
			196, 110, 54, 19, 252, 189, 80, 216, 191, 222,
			192, 10, 112, 231, 67, 104, 154, 205, 1, 172,
			194, 226, 187, 60, 252, 104, 176, 27, 87, 244,
			217, 166, 140, 245, 97, 187, 64, 188, 103, 129,
			194, 56, 206, 61, 169, 66, 171, 49, 234, 206,
			29, 141, 249, 110, 171, 127, 135, 23, 20, 58,
			156, 16, 252, 185, 148, 20, 202, 87, 124, 160,
			65, 169, 243, 32, 164, 19, 59, 58, 188, 109,
			43, 1, 150, 0, 0, 0, 203, 217, 189, 181,
			208, 230, 19, 165, 199, 206, 44, 204, 209, 156,
			80, 26, 199, 66, 198, 13 };

		static byte[] dsaPubBlob = { 6, 2, 0, 0, 0, 34, 0, 0, 68, 83,
			83, 49, 0, 4, 0, 0, 69, 144, 99, 249,
			41, 174, 97, 185, 66, 236, 179, 197, 182, 101,
			146, 165, 47, 36, 234, 199, 170, 99, 97, 8,
			224, 141, 189, 97, 86, 96, 240, 53, 69, 135,
			123, 169, 165, 64, 50, 51, 144, 131, 158, 151,
			218, 224, 159, 194, 166, 107, 132, 201, 148, 74,
			38, 62, 231, 221, 157, 216, 239, 66, 248, 68,
			26, 23, 123, 253, 157, 123, 65, 199, 109, 138,
			231, 217, 247, 170, 81, 51, 43, 252, 66, 210,
			75, 127, 68, 147, 141, 213, 174, 251, 109, 152,
			244, 113, 14, 194, 198, 222, 69, 157, 146, 154,
			224, 158, 46, 181, 204, 251, 10, 124, 153, 26,
			239, 105, 199, 53, 43, 51, 255, 118, 213, 58,
			111, 212, 166, 235, 29, 143, 53, 193, 210, 7,
			78, 198, 7, 3, 219, 0, 57, 81, 179, 46,
			58, 180, 61, 222, 145, 109, 165, 23, 119, 162,
			91, 55, 48, 230, 133, 54, 103, 58, 139, 99,
			146, 149, 90, 197, 167, 60, 164, 35, 90, 168,
			150, 138, 107, 17, 219, 191, 163, 4, 98, 13,
			109, 98, 122, 178, 247, 46, 73, 124, 53, 228,
			137, 21, 20, 45, 214, 217, 202, 51, 87, 45,
			78, 190, 19, 209, 249, 13, 31, 88, 52, 108,
			196, 110, 54, 19, 252, 189, 80, 216, 191, 222,
			192, 10, 112, 231, 67, 104, 154, 205, 1, 172,
			194, 226, 187, 60, 252, 104, 176, 27, 87, 244,
			217, 166, 140, 245, 97, 187, 64, 188, 103, 129,
			194, 56, 206, 61, 169, 66, 171, 49, 234, 206,
			29, 141, 249, 110, 171, 127, 135, 23, 20, 58,
			156, 16, 185, 163, 1, 154, 216, 44, 43, 101,
			67, 65, 35, 30, 70, 97, 44, 194, 46, 9,
			182, 125, 162, 93, 231, 223, 50, 55, 14, 218,
			93, 6, 176, 10, 195, 91, 83, 98, 73, 65,
			88, 250, 7, 120, 0, 155, 35, 138, 54, 37,
			80, 125, 44, 51, 25, 29, 198, 18, 107, 84,
			60, 27, 227, 218, 32, 74, 62, 76, 222, 6,
			76, 129, 254, 197, 53, 189, 4, 243, 203, 94,
			73, 190, 102, 196, 88, 170, 17, 199, 119, 180,
			205, 151, 184, 12, 168, 236, 81, 117, 49, 223,
			204, 69, 50, 246, 230, 124, 57, 208, 75, 5,
			178, 58, 7, 193, 224, 103, 60, 233, 2, 242,
			82, 53, 252, 157, 202, 146, 231, 255, 250, 38,
			150, 0, 0, 0, 203, 217, 189, 181, 208, 230,
			19, 165, 199, 206, 44, 204, 209, 156, 80, 26,
			199, 66, 198, 13 };

		static string dsaKeyPairString = "<DSAKeyValue><P>66bUbzrVdv8zKzXHae8amXwK+8y1Lp7gmpKdRd7Gwg5x9Jht+67VjZNEf0vSQvwrM1Gq99nnim3HQXud/XsXGkT4Qu/Ynd3nPiZKlMmEa6bCn+Dal56DkDMyQKWpe4dFNfBgVmG9jeAIYWOqx+okL6WSZbbFs+xCuWGuKfljkEU=</P><Q>3j20Oi6zUTkA2wMHxk4H0sE1jx0=</Q><G>EJw6FBeHf6tu+Y0dzuoxq0KpPc44woFnvEC7YfWMptn0VxuwaPw8u+LCrAHNmmhD53AKwN6/2FC9/BM2bsRsNFgfDfnRE75OLVczytnWLRQVieQ1fEku97J6Ym0NYgSjv9sRa4qWqFojpDynxVqVkmOLOmc2heYwN1uidxelbZE=</G><Y>Jvr/55LKnfw1UvIC6Txn4MEHOrIFS9A5fOb2MkXM3zF1UeyoDLiXzbR3xxGqWMRmvkley/MEvTXF/oFMBt5MPkog2uMbPFRrEsYdGTMsfVAlNoojmwB4B/pYQUliU1vDCrAGXdoONzLf512ifbYJLsIsYUYeI0FDZSss2JoBo7k=</Y><Seed>DcZCxxpQnNHMLM7HpRPm0LW92cs=</Seed><PgenCounter>lg==</PgenCounter><X>ASttvDo7E6Qg86lBoHxXyhSUufw=</X></DSAKeyValue>";
		static string dsaPubKeyString =  "<DSAKeyValue><P>66bUbzrVdv8zKzXHae8amXwK+8y1Lp7gmpKdRd7Gwg5x9Jht+67VjZNEf0vSQvwrM1Gq99nnim3HQXud/XsXGkT4Qu/Ynd3nPiZKlMmEa6bCn+Dal56DkDMyQKWpe4dFNfBgVmG9jeAIYWOqx+okL6WSZbbFs+xCuWGuKfljkEU=</P><Q>3j20Oi6zUTkA2wMHxk4H0sE1jx0=</Q><G>EJw6FBeHf6tu+Y0dzuoxq0KpPc44woFnvEC7YfWMptn0VxuwaPw8u+LCrAHNmmhD53AKwN6/2FC9/BM2bsRsNFgfDfnRE75OLVczytnWLRQVieQ1fEku97J6Ym0NYgSjv9sRa4qWqFojpDynxVqVkmOLOmc2heYwN1uidxelbZE=</G><Y>Jvr/55LKnfw1UvIC6Txn4MEHOrIFS9A5fOb2MkXM3zF1UeyoDLiXzbR3xxGqWMRmvkley/MEvTXF/oFMBt5MPkog2uMbPFRrEsYdGTMsfVAlNoojmwB4B/pYQUliU1vDCrAGXdoONzLf512ifbYJLsIsYUYeI0FDZSss2JoBo7k=</Y><Seed>DcZCxxpQnNHMLM7HpRPm0LW92cs=</Seed><PgenCounter>lg==</PgenCounter></DSAKeyValue>";

		[Test]
		public void FromCapiKeyBlobDSA ()
		{
			DSA dsa = CryptoConvert.FromCapiKeyBlobDSA (dsaPrivBlob);
			Assert.AreEqual (dsaKeyPairString, dsa.ToXmlString (true), "KeyPair");
			Assert.AreEqual (dsaPubKeyString, dsa.ToXmlString (false), "PublicKey");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromCapiKeyBlobDSA_Null ()
		{
			DSA dsa = CryptoConvert.FromCapiKeyBlobDSA (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromCapiKeyBlobDSA_InvalidOffset ()
		{
			DSA dsa = CryptoConvert.FromCapiKeyBlobDSA (new byte [0], 0);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void FromCapiKeyBlobDSA_UnknownBlob ()
		{
			byte[] blob = new byte [334];
			DSA dsa = CryptoConvert.FromCapiKeyBlobDSA (blob, 0);
		}

		[Test]
		public void FromCapiPrivateKeyBlobDSA ()
		{
			DSA dsa = CryptoConvert.FromCapiPrivateKeyBlobDSA (dsaPrivBlob, 0);
			Assert.AreEqual (dsaKeyPairString, dsa.ToXmlString (true), "KeyPair");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromCapiPrivateKeyBlobDSA_Null ()
		{
			DSA dsa = CryptoConvert.FromCapiPrivateKeyBlobDSA (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromCapiPrivateKeyBlobDSA_InvalidOffset ()
		{
			DSA dsa = CryptoConvert.FromCapiPrivateKeyBlobDSA (new byte[0], 0);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void FromCapiPrivateKeyBlobDSA_Invalid ()
		{
			byte[] blob = new byte[334];
			DSA dsa = CryptoConvert.FromCapiPrivateKeyBlobDSA (blob, 0);
		}

		[Test]
		public void FromCapiPublicKeyBlobDSA ()
		{
			DSA dsa = CryptoConvert.FromCapiPublicKeyBlobDSA (dsaPubBlob, 0);
			Assert.AreEqual (dsaPubKeyString, dsa.ToXmlString (false), "PublicKey");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromCapiPublicKeyBlobDSA_Null ()
		{
			DSA dsa = CryptoConvert.FromCapiPublicKeyBlobDSA (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromCapiPublicKeyBlobDSA_InvalidOffset ()
		{
			DSA dsa = CryptoConvert.FromCapiPublicKeyBlobDSA (new byte[0], 0);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void FromCapiPublicKeyBlobDSA_Invalid ()
		{
			byte[] blob = new byte[400];
			DSA dsa = CryptoConvert.FromCapiPublicKeyBlobDSA (blob, 0);
		}

		[Test]
		public void ToCapiKeyBlob_DSA ()
		{
			DSA dsa = DSA.Create ();
			dsa.FromXmlString (dsaKeyPairString);
			byte[] keypair = CryptoConvert.ToCapiKeyBlob (dsa, true);
			AssertEquals ("KeyPair", dsaPrivBlob, keypair);

			byte[] pubkey = CryptoConvert.ToCapiKeyBlob (dsa, false);
			Assert.AreEqual (BitConverter.ToString (dsaPubBlob), BitConverter.ToString (pubkey), "PublicKey");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToCapiKeyBlob_DSANull ()
		{
			DSA dsa = null;
			CryptoConvert.ToCapiKeyBlob (dsa, false);
		}

		[Test]
		public void ToCapiPrivateKeyBlob_DSA ()
		{
			DSA dsa = DSA.Create ();
			dsa.FromXmlString (dsaKeyPairString);
			byte[] keypair = CryptoConvert.ToCapiPrivateKeyBlob (dsa);
			AssertEquals ("KeyPair", dsaPrivBlob, keypair);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ToCapiPrivateKeyBlob_PublicKeyOnly_DSA ()
		{
			DSA dsa = DSA.Create ();
			dsa.FromXmlString (dsaPubKeyString);
			byte[] pubkey = CryptoConvert.ToCapiPrivateKeyBlob (dsa);
		}

		[Test]
		public void ToCapiPublicKeyBlob_DSA ()
		{
			DSA dsa = DSA.Create ();
			// full keypair
			dsa.FromXmlString (dsaKeyPairString);
			byte[] pubkey = CryptoConvert.ToCapiPublicKeyBlob (dsa);
			Assert.AreEqual (BitConverter.ToString (dsaPubBlob), BitConverter.ToString (pubkey), "PublicKey-1");

			// public key only
			dsa.FromXmlString (dsaPubKeyString);
			pubkey = CryptoConvert.ToCapiPublicKeyBlob (dsa);
			Assert.AreEqual (BitConverter.ToString (dsaPubBlob), BitConverter.ToString (pubkey), "PublicKey-2");
		}

		[Test]
		public void FromHex () 
		{
			Assert.IsNull (CryptoConvert.FromHex (null), "FromHex(null)");
			string result = BitConverter.ToString (CryptoConvert.FromHex ("0123456789aBcDeF"));
			Assert.AreEqual ("01-23-45-67-89-AB-CD-EF", result, "0123456789abcdef");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromHex_NonHexChars () 
		{
			CryptoConvert.FromHex ("abcdefgh");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromHex_NonMultipleOf2 () 
		{
			CryptoConvert.FromHex ("abc");
		}

		[Test]
		public void ToHex () 
		{
			Assert.IsNull (CryptoConvert.FromHex (null), "FromHex(null)");
			byte[] data = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
			Assert.AreEqual ("0123456789ABCDEF", CryptoConvert.ToHex (data), "0123456789abcdef");
		}
		
		[Test]
		public void NUnitKey_Broken ()
		{
			// for some strange reason nunit.snk hasn't the same 
			// size as other strongname. I wonder how it was 
			// generated ?
			RSA rsa = CryptoConvert.FromCapiKeyBlob (strongNameNUnit, 0);
			// note the bad D parameters !!!
			// this only works because CRT is being used
			Assert.AreEqual ("<RSAKeyValue><Modulus>rB8h0TaMs8fWA+5WMdcjOjejCZwhYwuFHUZPS0cC9TOG6FJtvlHPpZLQAg0xfLr2SivPRol1Xw9SqhuaYQNJA7sAaOb8Rvgmx93XbmcNotY9ob4TGaesk+M8VsdexsIJ3WlyLbmRlf0EjT52nboyauEL3UC85zkMjW1LNb8LSs8=</Modulus><Exponent>AQAB</Exponent><P>2d4pGForvc792ztFxhNuzxIihDnXp+qK9F8t/NduhRBdu+JXK4d8a9EGwzpMxLUPlHjCZfXRraZiSQszkH+nzQ==</P><Q>yj9BeGmOrucOUCNZYTtXI0ykzz+1g+cVMSxi+6xzoLEOqmdE4gjcWaxak4MF1+pIR6UycnNa/jg1LBl7MKxpCw==</Q><DP>cMkAjznG4Sjx4/dIRKU0vP/PXJIxIR1bN+y5+uVvsnTpgWVH6SHneE0qahCZQ0/UM/Fb+bqLBJFY2iVxWUGslQ==</DP><DQ>gz6TXPGbLzMv3Z9i5C8e+ABHv1pHj6ZI4VU9kraxfmkH7FsBn3FClUq8qJdRFnGpoBy65Pyo4upUzx5mDAsGSw==</DQ><InverseQ>x+UShV+0d9cicoiB9fkSLqpLDyF4dYzVu0uqX0eCcoGJpk19jtSaMI3Eo8VN6MJAW1zrRy+MA1Fqb9qeThLqZQ==</InverseQ><D>AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=</D></RSAKeyValue>", rsa.ToXmlString (true), "KeyPair");
			Assert.AreEqual ("<RSAKeyValue><Modulus>rB8h0TaMs8fWA+5WMdcjOjejCZwhYwuFHUZPS0cC9TOG6FJtvlHPpZLQAg0xfLr2SivPRol1Xw9SqhuaYQNJA7sAaOb8Rvgmx93XbmcNotY9ob4TGaesk+M8VsdexsIJ3WlyLbmRlf0EjT52nboyauEL3UC85zkMjW1LNb8LSs8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>", rsa.ToXmlString (false), "PublicKey");
		}
	}
}
