// !!! DO NOT EDIT - This file is generated automatically - DO NOT EDIT !!!
// Note: Key and IV will be different each time the file is generated

//
// SymmetricAlgorithmTest.cs - NUnit Test Cases for SymmetricAlgorithmTest
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

[TestFixture]
public class SymmetricAlgorithmTest : Assertion {
public void AssertEquals (string msg, byte[] array1, byte[] array2)
{
	AllTests.AssertEquals (msg, array1, array2);
}

//--8<-- NON GENERATED CODE STARTS HERE --8<----8<----8<----8<----8<----8<--

//-->8-- NON GENERATED CODE ENDS HERE   -->8---->8---->8---->8---->8---->8--

private void Encrypt (ICryptoTransform trans, byte[] input, byte[] output)
{
	int bs = trans.InputBlockSize;
	int full = input.Length / bs;
	int partial = input.Length % bs;
	int pos = 0;
	for (int i=0; i < full; i++) {
		trans.TransformBlock (input, pos, bs, output, pos);
		pos += bs;
	}
	if (partial > 0) {
		byte[] final = trans.TransformFinalBlock (input, pos, partial);
		Array.Copy (final, 0, output, pos, bs);
	}
}

private void Decrypt (ICryptoTransform trans, byte[] input, byte[] output)
{
	int bs = trans.InputBlockSize;
	int full = input.Length / bs;
	int partial = input.Length % bs;
	int pos = 0;
	for (int i=0; i < full; i++) {
		trans.TransformBlock (input, pos, bs, output, pos);
		pos += bs;
	}
	if (partial > 0) {
		byte[] final = trans.TransformFinalBlock (input, pos, partial);
		Array.Copy (final, 0, output, pos, partial);
	}
}

[Test]
public void TestDES_k64b64_ECB_None ()
{
	byte[] key = { 0x12, 0xE7, 0x7B, 0xBF, 0x11, 0x90, 0x9D, 0xB0 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xD2, 0x0E, 0xA7, 0xA4, 0x00, 0xF3, 0x17, 0x69 };
	byte[] expected = { 0x4B, 0x63, 0x6D, 0x2C, 0xA7, 0x0B, 0x77, 0x1C, 0x4B, 0x63, 0x6D, 0x2C, 0xA7, 0x0B, 0x77, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = DES.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("DES_k64b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("DES_k64b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("DES_k64b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestDES_k64b64_ECB_Zeros ()
{
	byte[] key = { 0x2E, 0xCA, 0x2E, 0xC9, 0x1A, 0xB6, 0x9A, 0x5A };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x79, 0x75, 0xD0, 0x3F, 0xFD, 0x1B, 0x12, 0x13 };
	byte[] expected = { 0x9B, 0x58, 0x07, 0x30, 0xE5, 0xDA, 0x3E, 0x7F, 0x9B, 0x58, 0x07, 0x30, 0xE5, 0xDA, 0x3E, 0x7F, 0x9B, 0x58, 0x07, 0x30, 0xE5, 0xDA, 0x3E, 0x7F };

	SymmetricAlgorithm algo = DES.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("DES_k64b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("DES_k64b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("DES_k64b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("DES_k64b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestDES_k64b64_ECB_PKCS7 ()
{
	byte[] key = { 0x32, 0xE8, 0x8D, 0xF7, 0xDC, 0xFC, 0x6C, 0xCD };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x74, 0xB2, 0x5E, 0x33, 0xBD, 0xA3, 0xC1, 0xB8 };
	byte[] expected = { 0x0E, 0xB6, 0xA5, 0x6F, 0x4A, 0xAE, 0xED, 0x95, 0x0E, 0xB6, 0xA5, 0x6F, 0x4A, 0xAE, 0xED, 0x95, 0x45, 0xEC, 0x24, 0x40, 0xF4, 0xB3, 0x97, 0xF3 };

	SymmetricAlgorithm algo = DES.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("DES_k64b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("DES_k64b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("DES_k64b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestDES_k64b64_CBC_None ()
{
	byte[] key = { 0x91, 0xB4, 0x33, 0xB9, 0xA3, 0x7C, 0x47, 0x76 };
	byte[] iv = { 0x96, 0x98, 0xCC, 0x84, 0xDD, 0xC3, 0xA1, 0x14 };
	byte[] expected = { 0x71, 0x8A, 0xD7, 0xC1, 0x3F, 0xBC, 0x0C, 0xB7, 0xB7, 0x91, 0x96, 0x6A, 0xA9, 0xA6, 0xFC, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = DES.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("DES_k64b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("DES_k64b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestDES_k64b64_CBC_Zeros ()
{
	byte[] key = { 0x4A, 0x8B, 0xC7, 0xC5, 0x9C, 0x10, 0xB4, 0x6C };
	byte[] iv = { 0x4B, 0x53, 0x53, 0xEA, 0xAF, 0xCC, 0x5A, 0x2B };
	byte[] expected = { 0xCA, 0xBC, 0xB7, 0xB9, 0xCF, 0x72, 0x63, 0x1F, 0x83, 0x96, 0xA4, 0xB7, 0x95, 0xF7, 0xFE, 0x13, 0x90, 0x6A, 0x4B, 0x74, 0x9E, 0xE0, 0xF9, 0x30 };

	SymmetricAlgorithm algo = DES.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("DES_k64b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("DES_k64b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestDES_k64b64_CBC_PKCS7 ()
{
	byte[] key = { 0xEA, 0x7D, 0x6D, 0x2C, 0xB8, 0x93, 0x33, 0xF4 };
	byte[] iv = { 0x77, 0xE4, 0xAA, 0x7C, 0xFE, 0xA9, 0x0F, 0x94 };
	byte[] expected = { 0x83, 0xB0, 0x83, 0xCA, 0xAC, 0x64, 0xE3, 0xDF, 0x1F, 0x5B, 0xE2, 0x9C, 0x16, 0x3E, 0x68, 0x91, 0x9E, 0xE5, 0xB5, 0x67, 0x80, 0xD2, 0x52, 0xC6 };

	SymmetricAlgorithm algo = DES.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("DES_k64b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("DES_k64b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters DES_k64b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters DES_k64b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters DES_k64b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestDES_k64b64_CFB8_None ()
{
	byte[] key = { 0x52, 0x5E, 0x49, 0x90, 0x10, 0x20, 0x6D, 0x5C };
	byte[] iv = { 0x00, 0x45, 0x9B, 0x7F, 0xC2, 0x9D, 0x90, 0x37 };
	byte[] expected = { 0x9C, 0x9F, 0xE0, 0x9F, 0x2E, 0x0C, 0xE0, 0xBA, 0xD3, 0x2F, 0xF4, 0x54, 0x89, 0x83, 0x82, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = DES.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("DES_k64b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("DES_k64b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestDES_k64b64_CFB8_Zeros ()
{
	byte[] key = { 0xAF, 0x35, 0x0A, 0x91, 0x8F, 0x45, 0x46, 0xAF };
	byte[] iv = { 0x3A, 0xF5, 0xCD, 0x22, 0xDC, 0xEF, 0xF4, 0x61 };
	byte[] expected = { 0xFB, 0x7E, 0xA8, 0xEC, 0xC0, 0x65, 0x30, 0xE3, 0x84, 0xBC, 0x49, 0xB9, 0x1C, 0xFD, 0xF6, 0x81, 0xCE, 0x2A, 0x69, 0x70, 0x73, 0xF0, 0x9A, 0xA8 };

	SymmetricAlgorithm algo = DES.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("DES_k64b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("DES_k64b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestDES_k64b64_CFB8_PKCS7 ()
{
	byte[] key = { 0x5D, 0xAD, 0x6F, 0xFF, 0x48, 0x89, 0x18, 0xE6 };
	byte[] iv = { 0x98, 0x46, 0xD3, 0xFC, 0x1A, 0x59, 0xF6, 0x20 };
	byte[] expected = { 0xC3, 0xAC, 0xCF, 0x49, 0xFF, 0x46, 0x82, 0x21, 0xE8, 0x1F, 0x31, 0x4E, 0x1C, 0x33, 0xEA, 0x49, 0x54, 0x67, 0x3E, 0x9C, 0xFD, 0x77, 0x39, 0x69 };

	SymmetricAlgorithm algo = DES.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("DES_k64b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("DES_k64b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters DES_k64b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters DES_k64b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters DES_k64b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k40b64_ECB_None ()
{
	byte[] key = { 0xC3, 0x69, 0xCB, 0x65, 0x22 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x5E, 0x8E, 0xDB, 0xFD, 0x10, 0x1F, 0x14, 0x90 };
	byte[] expected = { 0xCC, 0x71, 0xF5, 0xC1, 0x2F, 0xAF, 0xB8, 0xF4, 0xCC, 0x71, 0xF5, 0xC1, 0x2F, 0xAF, 0xB8, 0xF4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k40b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k40b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k40b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k40b64_ECB_Zeros ()
{
	byte[] key = { 0x12, 0x66, 0x49, 0x15, 0xBC };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x3C, 0x1C, 0x38, 0x12, 0x1C, 0x78, 0x0C, 0x19 };
	byte[] expected = { 0xDF, 0xD0, 0xD8, 0x24, 0xD8, 0x22, 0x51, 0x7C, 0xDF, 0xD0, 0xD8, 0x24, 0xD8, 0x22, 0x51, 0x7C, 0xDF, 0xD0, 0xD8, 0x24, 0xD8, 0x22, 0x51, 0x7C };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k40b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k40b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k40b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k40b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k40b64_ECB_PKCS7 ()
{
	byte[] key = { 0xC2, 0x76, 0x2F, 0xCE, 0xED };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xB1, 0x88, 0x93, 0x03, 0xDA, 0x23, 0xE6, 0x87 };
	byte[] expected = { 0xE2, 0x9B, 0x89, 0x15, 0xEC, 0x57, 0x0B, 0x05, 0xE2, 0x9B, 0x89, 0x15, 0xEC, 0x57, 0x0B, 0x05, 0x44, 0x77, 0xF0, 0x47, 0x2A, 0x12, 0xEA, 0xA1 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k40b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k40b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k40b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k40b64_CBC_None ()
{
	byte[] key = { 0xD0, 0xE1, 0x4E, 0x9C, 0x58 };
	byte[] iv = { 0x8E, 0x5E, 0x76, 0x18, 0xB8, 0x76, 0xCF, 0x77 };
	byte[] expected = { 0x36, 0x1B, 0x18, 0x98, 0xEE, 0xC6, 0x18, 0xB8, 0x67, 0xC0, 0x92, 0x09, 0x22, 0xDC, 0x65, 0xC5, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k40b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k40b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k40b64_CBC_Zeros ()
{
	byte[] key = { 0xB5, 0x6F, 0xC7, 0x4F, 0xF8 };
	byte[] iv = { 0xB6, 0x95, 0xE9, 0x3E, 0x04, 0x98, 0x39, 0x3D };
	byte[] expected = { 0x32, 0x10, 0x36, 0x24, 0x9F, 0xB6, 0x87, 0x4E, 0x00, 0xB6, 0xEF, 0x33, 0x52, 0x8B, 0xDE, 0x8A, 0x90, 0xE2, 0x0C, 0x60, 0xD3, 0x1A, 0x72, 0xCC };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k40b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k40b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k40b64_CBC_PKCS7 ()
{
	byte[] key = { 0x67, 0xB6, 0xEE, 0xF5, 0x21 };
	byte[] iv = { 0xD3, 0xF1, 0xE7, 0xFF, 0x23, 0x92, 0xDC, 0xD9 };
	byte[] expected = { 0x24, 0x2F, 0x90, 0xAE, 0x75, 0x8E, 0x0C, 0x7F, 0xCA, 0xE4, 0xE7, 0x87, 0x2D, 0xEE, 0x9E, 0x30, 0x49, 0xF0, 0xBB, 0xC4, 0x4C, 0x8D, 0x44, 0x5C };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k40b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k40b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k40b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k40b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k40b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k40b64_CFB8_None ()
{
	byte[] key = { 0x35, 0xCF, 0xA0, 0x20, 0x56 };
	byte[] iv = { 0xC5, 0x47, 0xFA, 0x9D, 0x19, 0x4F, 0xA9, 0x06 };
	byte[] expected = { 0xEF, 0xF9, 0xE1, 0xEE, 0x23, 0x89, 0xF6, 0x6B, 0x1F, 0xA6, 0x07, 0xAC, 0x73, 0x4A, 0xC1, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k40b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k40b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k40b64_CFB8_Zeros ()
{
	byte[] key = { 0xDA, 0xD8, 0xF9, 0x76, 0xE4 };
	byte[] iv = { 0xAA, 0xC5, 0x42, 0xF9, 0x88, 0x42, 0x09, 0xB4 };
	byte[] expected = { 0x49, 0x08, 0xFD, 0x7B, 0x1A, 0xA2, 0xDB, 0xF3, 0xB7, 0x13, 0x01, 0x4F, 0xB8, 0x79, 0x3A, 0x0E, 0xA0, 0x11, 0x1E, 0x27, 0xA7, 0xFE, 0xFA, 0x48 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k40b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k40b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k40b64_CFB8_PKCS7 ()
{
	byte[] key = { 0xDF, 0x8C, 0xC7, 0x3C, 0xDE };
	byte[] iv = { 0x1D, 0x0A, 0x92, 0x74, 0xD6, 0xEB, 0x99, 0x0F };
	byte[] expected = { 0xF9, 0x7A, 0x8E, 0xE1, 0xF2, 0x93, 0xB8, 0xCF, 0xD4, 0x7C, 0xF8, 0x81, 0x7F, 0x53, 0x7C, 0x8F, 0x42, 0x8C, 0xC4, 0xFB, 0x9E, 0x0C, 0x65, 0x53 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k40b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k40b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k40b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k40b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k40b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k48b64_ECB_None ()
{
	byte[] key = { 0xAA, 0x37, 0x60, 0x52, 0x8A, 0xBE };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x0D, 0x5B, 0x94, 0x0F, 0x9A, 0x87, 0x08, 0x56 };
	byte[] expected = { 0xB4, 0xB4, 0x2B, 0x12, 0x9C, 0x07, 0xD4, 0xC9, 0xB4, 0xB4, 0x2B, 0x12, 0x9C, 0x07, 0xD4, 0xC9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k48b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k48b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k48b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k48b64_ECB_Zeros ()
{
	byte[] key = { 0x9B, 0x92, 0x8C, 0xC2, 0x18, 0xA3 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xB7, 0xC2, 0xAD, 0x13, 0x0A, 0x62, 0x0A, 0x50 };
	byte[] expected = { 0x24, 0x74, 0x0F, 0x4B, 0xAA, 0xB1, 0xB8, 0xF5, 0x24, 0x74, 0x0F, 0x4B, 0xAA, 0xB1, 0xB8, 0xF5, 0x24, 0x74, 0x0F, 0x4B, 0xAA, 0xB1, 0xB8, 0xF5 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k48b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k48b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k48b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k48b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k48b64_ECB_PKCS7 ()
{
	byte[] key = { 0x58, 0x1A, 0xD6, 0x96, 0x02, 0x75 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x56, 0x83, 0x39, 0x7F, 0x3B, 0xD9, 0xB0, 0x33 };
	byte[] expected = { 0x87, 0x46, 0x9E, 0xFF, 0x4B, 0xE8, 0xDA, 0xF2, 0x87, 0x46, 0x9E, 0xFF, 0x4B, 0xE8, 0xDA, 0xF2, 0x31, 0x54, 0x04, 0x63, 0xE0, 0x76, 0x74, 0x39 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k48b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k48b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k48b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k48b64_CBC_None ()
{
	byte[] key = { 0x21, 0x9A, 0xD6, 0x31, 0x99, 0x81 };
	byte[] iv = { 0x5E, 0x6E, 0xB6, 0x33, 0xC0, 0x25, 0xAE, 0x5C };
	byte[] expected = { 0x35, 0xFA, 0x8F, 0x4F, 0x75, 0xD1, 0x10, 0x11, 0xC0, 0xA4, 0x73, 0x69, 0xBD, 0xD2, 0xE3, 0x9D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k48b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k48b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k48b64_CBC_Zeros ()
{
	byte[] key = { 0x59, 0x0A, 0xD4, 0x25, 0xA5, 0xB9 };
	byte[] iv = { 0x10, 0x2D, 0x42, 0x54, 0xC8, 0x97, 0xD0, 0xA7 };
	byte[] expected = { 0x4F, 0x1A, 0x5F, 0xD0, 0xA2, 0x54, 0x57, 0x60, 0x55, 0x9B, 0x4D, 0x1B, 0x55, 0xC9, 0x30, 0xA9, 0x7E, 0xF6, 0xAF, 0xFB, 0x50, 0x8B, 0xC0, 0xB6 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k48b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k48b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k48b64_CBC_PKCS7 ()
{
	byte[] key = { 0x39, 0x6C, 0xB3, 0x7B, 0xB5, 0xA9 };
	byte[] iv = { 0x42, 0x56, 0x99, 0x18, 0xA8, 0x96, 0x93, 0x5D };
	byte[] expected = { 0x92, 0x8B, 0x67, 0xC7, 0xAE, 0xF3, 0xF7, 0x03, 0x24, 0x67, 0xAC, 0xEA, 0xFE, 0xB7, 0x6B, 0x1E, 0x53, 0xB3, 0xF5, 0xDB, 0x64, 0x63, 0xB3, 0xE5 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k48b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k48b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k48b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k48b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k48b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k48b64_CFB8_None ()
{
	byte[] key = { 0x06, 0xCE, 0x23, 0x86, 0xEC, 0xB3 };
	byte[] iv = { 0x14, 0xF7, 0xBA, 0xEC, 0xC2, 0x4A, 0x26, 0x6D };
	byte[] expected = { 0x69, 0x7A, 0x1A, 0xCC, 0x40, 0x41, 0x78, 0xC1, 0xFA, 0x89, 0x90, 0x7F, 0xC1, 0x1C, 0x27, 0x4D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k48b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k48b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k48b64_CFB8_Zeros ()
{
	byte[] key = { 0x4B, 0xC8, 0x03, 0x4F, 0x43, 0x27 };
	byte[] iv = { 0x02, 0x24, 0xB8, 0xE9, 0xF6, 0x19, 0xA1, 0x81 };
	byte[] expected = { 0xE2, 0xD2, 0x50, 0x68, 0x56, 0x61, 0x30, 0x72, 0xA2, 0xDE, 0x97, 0xF5, 0x5C, 0xE9, 0xD5, 0xA0, 0x35, 0xD2, 0xC3, 0xEB, 0xC9, 0x2A, 0x64, 0x4D };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k48b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k48b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k48b64_CFB8_PKCS7 ()
{
	byte[] key = { 0x22, 0x94, 0x8C, 0x13, 0x7F, 0x7A };
	byte[] iv = { 0x4B, 0xDF, 0xB8, 0xBF, 0x0D, 0xBE, 0x1E, 0x3D };
	byte[] expected = { 0x24, 0xE9, 0x2B, 0xBF, 0x84, 0x49, 0x4D, 0x2B, 0xC4, 0xD8, 0xEE, 0xAB, 0x52, 0x03, 0xC6, 0xAF, 0x19, 0x0A, 0x5B, 0x38, 0xB6, 0xF1, 0x98, 0x6F };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k48b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k48b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k48b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k48b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k48b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k56b64_ECB_None ()
{
	byte[] key = { 0xCA, 0x6B, 0x7A, 0xA1, 0xB1, 0x6E, 0x4A };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xF0, 0xA9, 0x35, 0xDB, 0x4F, 0xB5, 0x3D, 0xE4 };
	byte[] expected = { 0x23, 0x39, 0x2D, 0xD9, 0x7C, 0xC0, 0xFF, 0x64, 0x23, 0x39, 0x2D, 0xD9, 0x7C, 0xC0, 0xFF, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k56b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k56b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k56b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k56b64_ECB_Zeros ()
{
	byte[] key = { 0x96, 0x43, 0x86, 0xAA, 0x0E, 0x66, 0x95 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xD3, 0xD7, 0x93, 0xED, 0xAF, 0xD6, 0x83, 0x3F };
	byte[] expected = { 0x1C, 0x72, 0x96, 0xCF, 0x7D, 0x18, 0xDB, 0x4B, 0x1C, 0x72, 0x96, 0xCF, 0x7D, 0x18, 0xDB, 0x4B, 0x1C, 0x72, 0x96, 0xCF, 0x7D, 0x18, 0xDB, 0x4B };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k56b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k56b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k56b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k56b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k56b64_ECB_PKCS7 ()
{
	byte[] key = { 0x5A, 0x29, 0xE4, 0x77, 0x99, 0x9D, 0x5B };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xA6, 0x7B, 0x92, 0x40, 0x74, 0x9E, 0x0D, 0xAD };
	byte[] expected = { 0xE1, 0xBB, 0xAA, 0x43, 0x54, 0x2E, 0xFF, 0x3A, 0xE1, 0xBB, 0xAA, 0x43, 0x54, 0x2E, 0xFF, 0x3A, 0x2E, 0xA1, 0x81, 0xF1, 0x85, 0x86, 0x35, 0x97 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k56b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k56b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k56b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k56b64_CBC_None ()
{
	byte[] key = { 0xDD, 0x2F, 0x84, 0x9F, 0xBA, 0xB1, 0xF3 };
	byte[] iv = { 0x97, 0xB2, 0xCD, 0x3F, 0x1E, 0x53, 0xE8, 0xA9 };
	byte[] expected = { 0x63, 0x6E, 0x62, 0xE5, 0x0F, 0x58, 0x86, 0x4A, 0xEF, 0x64, 0x4C, 0xDC, 0x36, 0x5D, 0x29, 0xC6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k56b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k56b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k56b64_CBC_Zeros ()
{
	byte[] key = { 0xED, 0xEE, 0x33, 0x8E, 0x97, 0x20, 0x58 };
	byte[] iv = { 0x0B, 0xAB, 0xAB, 0xED, 0xCC, 0x1C, 0x77, 0xA4 };
	byte[] expected = { 0x8B, 0x2F, 0x52, 0x93, 0x48, 0x7A, 0x54, 0x03, 0x58, 0x6A, 0x9B, 0xC4, 0x13, 0x99, 0xCD, 0xE2, 0x18, 0x31, 0x67, 0x05, 0x27, 0x90, 0x1D, 0xFE };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k56b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k56b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k56b64_CBC_PKCS7 ()
{
	byte[] key = { 0x52, 0xF6, 0xC3, 0xC3, 0x13, 0x9E, 0xF7 };
	byte[] iv = { 0x8E, 0xF8, 0xE5, 0x66, 0x64, 0x1C, 0xE6, 0xE3 };
	byte[] expected = { 0x7B, 0xD1, 0x1A, 0xD0, 0x62, 0x1B, 0x66, 0x5B, 0x92, 0xB0, 0x42, 0xC7, 0x63, 0x3A, 0x95, 0xED, 0x87, 0x6B, 0xA0, 0x88, 0x18, 0xC2, 0x92, 0xB4 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k56b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k56b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k56b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k56b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k56b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k56b64_CFB8_None ()
{
	byte[] key = { 0xEA, 0x1D, 0xB2, 0x0E, 0x17, 0xF0, 0x4A };
	byte[] iv = { 0xB7, 0xEE, 0xEE, 0xFF, 0x36, 0x8C, 0x9B, 0xBB };
	byte[] expected = { 0x49, 0x1D, 0x32, 0xB4, 0x93, 0xEC, 0x96, 0xC9, 0xDC, 0x3B, 0x26, 0x4B, 0x3C, 0xA2, 0xE8, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k56b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k56b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k56b64_CFB8_Zeros ()
{
	byte[] key = { 0x24, 0x6F, 0xE0, 0xC7, 0x3C, 0xC0, 0x4B };
	byte[] iv = { 0xD7, 0x83, 0xCA, 0xB7, 0x9C, 0x6D, 0xC3, 0x25 };
	byte[] expected = { 0x37, 0xF7, 0x35, 0xF4, 0xB2, 0x0C, 0xCB, 0xC4, 0xAE, 0x42, 0x83, 0x99, 0x55, 0xF6, 0x51, 0x5A, 0x1A, 0xE7, 0x7B, 0xFD, 0x4E, 0x78, 0xD7, 0x80 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k56b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k56b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k56b64_CFB8_PKCS7 ()
{
	byte[] key = { 0x58, 0xE4, 0xC8, 0x6F, 0xB4, 0x14, 0xAC };
	byte[] iv = { 0xA1, 0xBC, 0x94, 0xB5, 0xF5, 0x4F, 0x78, 0x19 };
	byte[] expected = { 0xBA, 0x15, 0xE2, 0x73, 0x56, 0x5E, 0xB6, 0x30, 0xA8, 0x50, 0xA2, 0x61, 0x52, 0x2F, 0x61, 0xCC, 0x97, 0x9A, 0x91, 0xB1, 0xF0, 0x87, 0x3F, 0xA7 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k56b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k56b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k56b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k56b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k56b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k64b64_ECB_None ()
{
	byte[] key = { 0x2C, 0x52, 0xB4, 0x93, 0xF1, 0xEA, 0xC8, 0x8F };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xDE, 0x10, 0xA1, 0x1C, 0x5E, 0x43, 0x5F, 0x97 };
	byte[] expected = { 0xDB, 0x1D, 0x72, 0x2C, 0x7C, 0x4A, 0x31, 0xDB, 0xDB, 0x1D, 0x72, 0x2C, 0x7C, 0x4A, 0x31, 0xDB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k64b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k64b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k64b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k64b64_ECB_Zeros ()
{
	byte[] key = { 0x05, 0x0C, 0x49, 0xE3, 0x25, 0x49, 0xFA, 0x35 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x4D, 0x94, 0x32, 0xD2, 0x8B, 0xB6, 0x52, 0x9C };
	byte[] expected = { 0x39, 0x35, 0xCE, 0x5C, 0x75, 0xF5, 0xB7, 0xA1, 0x39, 0x35, 0xCE, 0x5C, 0x75, 0xF5, 0xB7, 0xA1, 0x39, 0x35, 0xCE, 0x5C, 0x75, 0xF5, 0xB7, 0xA1 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k64b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k64b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k64b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k64b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k64b64_ECB_PKCS7 ()
{
	byte[] key = { 0xE6, 0x57, 0xF2, 0x73, 0x3A, 0x20, 0xB0, 0x7E };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x34, 0x25, 0xD2, 0x35, 0x1C, 0xE4, 0x9D, 0xC6 };
	byte[] expected = { 0x7A, 0x3F, 0x95, 0xA0, 0xA1, 0x70, 0xBD, 0xC3, 0x7A, 0x3F, 0x95, 0xA0, 0xA1, 0x70, 0xBD, 0xC3, 0xDA, 0xE7, 0x0C, 0xC3, 0xAD, 0xC3, 0xEA, 0xE9 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k64b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k64b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k64b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k64b64_CBC_None ()
{
	byte[] key = { 0x91, 0x14, 0x49, 0xC4, 0x0D, 0xF9, 0x90, 0x77 };
	byte[] iv = { 0xB9, 0xBD, 0x6B, 0x9E, 0x52, 0xC9, 0x8C, 0xA5 };
	byte[] expected = { 0xF1, 0x7C, 0xDF, 0x18, 0x54, 0xC2, 0xDE, 0x3B, 0x05, 0x20, 0x99, 0x94, 0x8A, 0x5E, 0x29, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k64b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k64b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k64b64_CBC_Zeros ()
{
	byte[] key = { 0x0E, 0xE0, 0xAD, 0xFD, 0x86, 0x22, 0x1D, 0x05 };
	byte[] iv = { 0xDF, 0x41, 0x2B, 0x6E, 0x82, 0x00, 0xCB, 0x38 };
	byte[] expected = { 0x98, 0x43, 0x84, 0x05, 0x68, 0xAE, 0x99, 0x3B, 0xB1, 0xCD, 0x2F, 0x69, 0xD9, 0xDD, 0x54, 0x79, 0x37, 0x36, 0x96, 0xE9, 0xC3, 0x62, 0xC2, 0x35 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k64b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k64b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k64b64_CBC_PKCS7 ()
{
	byte[] key = { 0x2D, 0x70, 0x15, 0xFF, 0x15, 0xEB, 0xDC, 0x33 };
	byte[] iv = { 0x04, 0x33, 0x63, 0x52, 0x5B, 0xA1, 0xAB, 0xAC };
	byte[] expected = { 0x07, 0x9B, 0x58, 0x27, 0xB4, 0x36, 0xDD, 0x9D, 0x7C, 0xC5, 0xE0, 0x83, 0x6A, 0x76, 0x87, 0x08, 0xF1, 0xEF, 0xCB, 0xE2, 0xA1, 0xF6, 0xA9, 0xBE };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k64b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k64b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k64b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k64b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k64b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k64b64_CFB8_None ()
{
	byte[] key = { 0x1B, 0x23, 0x16, 0xEA, 0x19, 0xF0, 0x53, 0xEE };
	byte[] iv = { 0x60, 0x8D, 0x23, 0x2B, 0x0D, 0x56, 0x6F, 0x92 };
	byte[] expected = { 0x0C, 0xE2, 0x26, 0xA8, 0x0A, 0xB8, 0xFE, 0x03, 0x71, 0x2B, 0x56, 0x59, 0xA3, 0x45, 0xC0, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k64b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k64b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k64b64_CFB8_Zeros ()
{
	byte[] key = { 0x49, 0xAD, 0xCD, 0xF8, 0xB6, 0x44, 0xA1, 0x86 };
	byte[] iv = { 0xCA, 0x6A, 0x96, 0xA8, 0x18, 0xA8, 0xF6, 0x77 };
	byte[] expected = { 0x12, 0x88, 0x7D, 0xC4, 0x8A, 0x04, 0x86, 0x09, 0x4A, 0x64, 0xBE, 0x31, 0xD2, 0x1F, 0xF9, 0xA1, 0x80, 0x5D, 0x0B, 0x5A, 0x01, 0x9F, 0x10, 0x6D };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k64b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k64b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k64b64_CFB8_PKCS7 ()
{
	byte[] key = { 0xF6, 0xE6, 0xA0, 0x33, 0xD3, 0x77, 0x0C, 0x28 };
	byte[] iv = { 0x50, 0x31, 0x14, 0xAF, 0x27, 0x92, 0xFC, 0x57 };
	byte[] expected = { 0xFF, 0x4B, 0xA2, 0x37, 0x56, 0xFB, 0x37, 0x4A, 0xB5, 0x6A, 0xCB, 0x27, 0x06, 0xED, 0xC2, 0x38, 0x7C, 0x4B, 0xBE, 0xC0, 0xD5, 0xD7, 0x6A, 0x79 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k64b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k64b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k64b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k64b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k64b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k72b64_ECB_None ()
{
	byte[] key = { 0xEC, 0x93, 0x9A, 0xF0, 0x51, 0x69, 0x59, 0x0B, 0x15 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x36, 0xDB, 0xE8, 0x7F, 0xB5, 0x43, 0x4C, 0xF6 };
	byte[] expected = { 0xD6, 0x8A, 0x11, 0x59, 0x38, 0x6B, 0x93, 0x8F, 0xD6, 0x8A, 0x11, 0x59, 0x38, 0x6B, 0x93, 0x8F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k72b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k72b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k72b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k72b64_ECB_Zeros ()
{
	byte[] key = { 0x19, 0x14, 0x2D, 0xF6, 0x48, 0xED, 0x5A, 0xF3, 0x1F };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x8C, 0x1D, 0x0D, 0xC7, 0xE3, 0x77, 0x68, 0x40 };
	byte[] expected = { 0x38, 0xD4, 0x18, 0x61, 0xF6, 0x8E, 0x55, 0xD7, 0x38, 0xD4, 0x18, 0x61, 0xF6, 0x8E, 0x55, 0xD7, 0x38, 0xD4, 0x18, 0x61, 0xF6, 0x8E, 0x55, 0xD7 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k72b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k72b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k72b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k72b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k72b64_ECB_PKCS7 ()
{
	byte[] key = { 0x1C, 0xAA, 0x46, 0xE7, 0x37, 0x23, 0x14, 0xC9, 0x31 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x3B, 0x0B, 0x1D, 0xE0, 0x3A, 0x6E, 0xF3, 0x1C };
	byte[] expected = { 0x71, 0x04, 0xA2, 0x66, 0xFC, 0xB9, 0x0F, 0x48, 0x71, 0x04, 0xA2, 0x66, 0xFC, 0xB9, 0x0F, 0x48, 0xFA, 0xF7, 0x6F, 0xA9, 0xA0, 0x23, 0xF8, 0x7E };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k72b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k72b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k72b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k72b64_CBC_None ()
{
	byte[] key = { 0xF7, 0x60, 0xC5, 0x87, 0x4E, 0x36, 0xCE, 0x3C, 0xE6 };
	byte[] iv = { 0x60, 0x0E, 0xAC, 0x58, 0x1C, 0x91, 0x1D, 0xAC };
	byte[] expected = { 0xF7, 0xFE, 0xC3, 0x0E, 0x68, 0x6C, 0x15, 0x38, 0xDC, 0x06, 0xD9, 0x3A, 0x02, 0x08, 0xE2, 0xBF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k72b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k72b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k72b64_CBC_Zeros ()
{
	byte[] key = { 0xD2, 0x3C, 0xD2, 0x40, 0xF1, 0x1D, 0x2E, 0xF4, 0x92 };
	byte[] iv = { 0xBE, 0x7C, 0xF7, 0xBE, 0x35, 0x11, 0x94, 0x46 };
	byte[] expected = { 0x7B, 0x6C, 0x73, 0xE4, 0x19, 0x69, 0x32, 0x61, 0x48, 0xE0, 0x21, 0x03, 0xAF, 0xC4, 0x54, 0x61, 0xE7, 0xB7, 0x00, 0x55, 0xDB, 0x57, 0x3C, 0x40 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k72b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k72b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k72b64_CBC_PKCS7 ()
{
	byte[] key = { 0xE6, 0x09, 0x99, 0x96, 0x84, 0x2D, 0x9B, 0xE9, 0x34 };
	byte[] iv = { 0x00, 0xE9, 0x3B, 0x59, 0x6C, 0x5E, 0xF3, 0x8A };
	byte[] expected = { 0xA9, 0x4E, 0x30, 0x5F, 0xEF, 0xF5, 0x77, 0xC5, 0x26, 0x96, 0xDA, 0x3E, 0x53, 0xF5, 0xCB, 0xEC, 0xBC, 0xF9, 0x85, 0x00, 0xF2, 0x0D, 0x32, 0x2D };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k72b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k72b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k72b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k72b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k72b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k72b64_CFB8_None ()
{
	byte[] key = { 0x65, 0x6B, 0x23, 0x3F, 0xB3, 0xE5, 0x6F, 0x30, 0x01 };
	byte[] iv = { 0x10, 0x16, 0x28, 0x20, 0xAB, 0x77, 0x74, 0x46 };
	byte[] expected = { 0x5A, 0x35, 0x9B, 0x9E, 0x7A, 0xD6, 0xED, 0x1D, 0x36, 0xC9, 0x95, 0x0E, 0x04, 0xE1, 0x9C, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k72b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k72b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k72b64_CFB8_Zeros ()
{
	byte[] key = { 0x87, 0xC1, 0x80, 0x41, 0xD6, 0xF1, 0x33, 0xC7, 0x78 };
	byte[] iv = { 0x21, 0x55, 0xCF, 0x6E, 0xF5, 0x3B, 0xF0, 0x6B };
	byte[] expected = { 0x83, 0xFC, 0xD7, 0x43, 0xC0, 0x4F, 0x9F, 0xE0, 0x60, 0xAD, 0x3B, 0x0D, 0x5A, 0xF3, 0xF3, 0x0B, 0x96, 0x25, 0x97, 0x6D, 0x58, 0x8B, 0x5A, 0x26 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k72b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k72b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k72b64_CFB8_PKCS7 ()
{
	byte[] key = { 0xAE, 0xE0, 0x44, 0x66, 0xDA, 0x34, 0xFD, 0xD4, 0x71 };
	byte[] iv = { 0xFA, 0x66, 0x5F, 0x55, 0xBC, 0x1B, 0xC7, 0x83 };
	byte[] expected = { 0xF3, 0xAB, 0x63, 0x11, 0xA0, 0x27, 0x05, 0x42, 0x0A, 0xCD, 0x16, 0xCA, 0x22, 0x4E, 0x0B, 0xCB, 0x96, 0xCA, 0xD9, 0x38, 0x6D, 0x5E, 0x5E, 0x55 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k72b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k72b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k72b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k72b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k72b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k80b64_ECB_None ()
{
	byte[] key = { 0xB8, 0xA4, 0x76, 0xF8, 0x59, 0x86, 0x40, 0x53, 0x33, 0x68 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xFF, 0x5F, 0x8B, 0x5E, 0xCF, 0xB8, 0xA5, 0xCB };
	byte[] expected = { 0x7A, 0x56, 0x73, 0x0A, 0x72, 0x69, 0x95, 0x16, 0x7A, 0x56, 0x73, 0x0A, 0x72, 0x69, 0x95, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k80b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k80b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k80b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k80b64_ECB_Zeros ()
{
	byte[] key = { 0x9A, 0xE1, 0xE1, 0x17, 0xCB, 0x2B, 0x9C, 0x5D, 0x5D, 0x28 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x71, 0x29, 0x89, 0x9C, 0x66, 0xF5, 0x90, 0x63 };
	byte[] expected = { 0x38, 0x83, 0x30, 0xE0, 0xC6, 0x8A, 0x0B, 0x11, 0x38, 0x83, 0x30, 0xE0, 0xC6, 0x8A, 0x0B, 0x11, 0x38, 0x83, 0x30, 0xE0, 0xC6, 0x8A, 0x0B, 0x11 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k80b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k80b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k80b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k80b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k80b64_ECB_PKCS7 ()
{
	byte[] key = { 0x8D, 0xF8, 0xDA, 0xA2, 0x31, 0xEA, 0x86, 0x92, 0x52, 0xBB };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xD3, 0x1C, 0x57, 0x72, 0xDE, 0xFD, 0xCA, 0xC7 };
	byte[] expected = { 0x51, 0xD4, 0x00, 0x54, 0x58, 0xE5, 0xED, 0x5C, 0x51, 0xD4, 0x00, 0x54, 0x58, 0xE5, 0xED, 0x5C, 0xCE, 0xF6, 0xDB, 0x31, 0x10, 0xE9, 0x0E, 0xD8 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k80b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k80b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k80b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k80b64_CBC_None ()
{
	byte[] key = { 0x5B, 0x45, 0x99, 0x10, 0x47, 0x42, 0x89, 0xC8, 0x2A, 0x6C };
	byte[] iv = { 0xE4, 0x8F, 0x2A, 0x4D, 0x25, 0x38, 0x01, 0x04 };
	byte[] expected = { 0xA3, 0x23, 0xE7, 0xCD, 0xC1, 0x5E, 0x4E, 0x1D, 0x2F, 0x7F, 0x8B, 0xA7, 0xD0, 0x42, 0xF2, 0xFC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k80b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k80b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k80b64_CBC_Zeros ()
{
	byte[] key = { 0xD4, 0x47, 0xFF, 0x5A, 0x70, 0xE8, 0x48, 0x0F, 0x23, 0xD1 };
	byte[] iv = { 0x8B, 0xF8, 0x94, 0x02, 0xB3, 0xFB, 0xB0, 0x0D };
	byte[] expected = { 0x88, 0x5C, 0x72, 0x4C, 0x35, 0x7F, 0x73, 0x1C, 0x8A, 0x06, 0x6B, 0x90, 0x82, 0xC5, 0xBC, 0x46, 0x75, 0xC1, 0x87, 0xD9, 0xED, 0x29, 0x1D, 0xB8 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k80b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k80b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k80b64_CBC_PKCS7 ()
{
	byte[] key = { 0x8D, 0x77, 0xC5, 0x6E, 0xC2, 0x8F, 0x10, 0x51, 0xD2, 0x20 };
	byte[] iv = { 0x43, 0xC5, 0x4E, 0x58, 0xF0, 0xD7, 0xB3, 0x92 };
	byte[] expected = { 0xE9, 0xB0, 0x67, 0x7C, 0x6C, 0x77, 0x68, 0x4D, 0xD0, 0xA5, 0x93, 0x9F, 0x84, 0xE0, 0xA0, 0xA9, 0x36, 0x21, 0xD7, 0x07, 0x0B, 0x8D, 0xD7, 0xB9 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k80b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k80b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k80b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k80b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k80b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k80b64_CFB8_None ()
{
	byte[] key = { 0x2A, 0x44, 0xD9, 0x1C, 0x5E, 0x7C, 0x79, 0x3D, 0x88, 0x55 };
	byte[] iv = { 0xA0, 0x48, 0x00, 0x04, 0xA8, 0xB8, 0x83, 0x9F };
	byte[] expected = { 0xEA, 0xD0, 0x3D, 0x9A, 0x62, 0xEA, 0x9C, 0x59, 0xAC, 0xD4, 0xA1, 0xDE, 0xDB, 0x3D, 0xF8, 0x4E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k80b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k80b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k80b64_CFB8_Zeros ()
{
	byte[] key = { 0x30, 0x51, 0xCD, 0x3B, 0x8A, 0x8A, 0x8C, 0xF4, 0x76, 0x64 };
	byte[] iv = { 0xD9, 0x5F, 0xEB, 0x11, 0x8F, 0x0A, 0x7D, 0xDC };
	byte[] expected = { 0x02, 0xB4, 0x0F, 0xB5, 0x79, 0x81, 0xAC, 0xFD, 0xBA, 0x40, 0xF1, 0x61, 0x96, 0x70, 0x09, 0x5B, 0xFF, 0x0D, 0x90, 0xB4, 0x54, 0x27, 0x4A, 0x3C };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k80b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k80b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k80b64_CFB8_PKCS7 ()
{
	byte[] key = { 0xA7, 0x24, 0xA0, 0x14, 0x78, 0xDC, 0x8B, 0x99, 0x77, 0xCD };
	byte[] iv = { 0xB8, 0x68, 0xD0, 0x5A, 0x13, 0x3C, 0xBA, 0x59 };
	byte[] expected = { 0x3B, 0x35, 0xF6, 0x3F, 0x36, 0x7B, 0xF1, 0x7D, 0xCE, 0xC8, 0x62, 0xF8, 0x34, 0xC6, 0x42, 0x6F, 0x77, 0xCF, 0x32, 0x41, 0xF3, 0x0B, 0x28, 0x37 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k80b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k80b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k80b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k80b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k80b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k88b64_ECB_None ()
{
	byte[] key = { 0xCE, 0x12, 0x59, 0x88, 0x7A, 0xCD, 0x57, 0x4C, 0xCD, 0xA9, 0xD2 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x91, 0x4C, 0x2D, 0xB4, 0x6E, 0x19, 0x3F, 0x6F };
	byte[] expected = { 0x74, 0x25, 0xAD, 0x2E, 0x88, 0xA9, 0x3E, 0x1F, 0x74, 0x25, 0xAD, 0x2E, 0x88, 0xA9, 0x3E, 0x1F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k88b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k88b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k88b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k88b64_ECB_Zeros ()
{
	byte[] key = { 0x28, 0xDC, 0x09, 0x80, 0x85, 0x25, 0x95, 0x41, 0x7B, 0xD4, 0x06 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xAE, 0x0D, 0xC1, 0x42, 0x01, 0x1C, 0x6E, 0x5A };
	byte[] expected = { 0x48, 0xD6, 0x9F, 0x9A, 0x7C, 0x93, 0x89, 0x5F, 0x48, 0xD6, 0x9F, 0x9A, 0x7C, 0x93, 0x89, 0x5F, 0x48, 0xD6, 0x9F, 0x9A, 0x7C, 0x93, 0x89, 0x5F };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k88b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k88b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k88b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k88b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k88b64_ECB_PKCS7 ()
{
	byte[] key = { 0xAB, 0x26, 0x7E, 0xD3, 0x3A, 0x0A, 0x3F, 0x50, 0x0B, 0x84, 0x5F };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x28, 0x3C, 0x18, 0x06, 0x3C, 0xF7, 0x83, 0x51 };
	byte[] expected = { 0xE0, 0x60, 0x29, 0xC5, 0xE5, 0xFE, 0x75, 0x95, 0xE0, 0x60, 0x29, 0xC5, 0xE5, 0xFE, 0x75, 0x95, 0xE8, 0x61, 0x0A, 0x2A, 0x79, 0x3F, 0x0A, 0xB7 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k88b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k88b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k88b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k88b64_CBC_None ()
{
	byte[] key = { 0x01, 0x2F, 0x45, 0x5F, 0x2D, 0x9E, 0xDB, 0x29, 0x6C, 0x54, 0xF5 };
	byte[] iv = { 0x4C, 0x6A, 0x4D, 0x77, 0x7E, 0x34, 0xB4, 0x75 };
	byte[] expected = { 0x66, 0x58, 0x7F, 0xE7, 0x6D, 0x3B, 0x6A, 0x97, 0xFC, 0x65, 0x15, 0x8D, 0xAC, 0xB0, 0xB1, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k88b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k88b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k88b64_CBC_Zeros ()
{
	byte[] key = { 0xA9, 0xD1, 0xDA, 0xCB, 0x4C, 0xA7, 0xD3, 0x35, 0x70, 0x1E, 0x15 };
	byte[] iv = { 0xF2, 0x17, 0x14, 0x41, 0x36, 0x58, 0x27, 0x48 };
	byte[] expected = { 0x41, 0xDD, 0xFE, 0x10, 0x56, 0xE2, 0x86, 0xDC, 0xC6, 0x53, 0x69, 0x1A, 0x2D, 0x66, 0x1D, 0x1C, 0xAD, 0x3C, 0x1F, 0xCE, 0xE3, 0xE2, 0x52, 0x13 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k88b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k88b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k88b64_CBC_PKCS7 ()
{
	byte[] key = { 0x07, 0x97, 0xCB, 0xA3, 0xB6, 0xFF, 0x57, 0x30, 0x5A, 0x2E, 0x3E };
	byte[] iv = { 0x78, 0x44, 0xCE, 0xBA, 0xC6, 0xCD, 0x0C, 0xB7 };
	byte[] expected = { 0x07, 0xCC, 0xFD, 0x12, 0x0D, 0x07, 0xED, 0xB2, 0x8C, 0xDA, 0xB9, 0xC3, 0xE7, 0x04, 0x41, 0x5A, 0xA3, 0x9C, 0x50, 0x8B, 0x8F, 0x9D, 0x2E, 0x65 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k88b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k88b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k88b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k88b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k88b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k88b64_CFB8_None ()
{
	byte[] key = { 0x6E, 0x73, 0x03, 0xFD, 0x20, 0xAB, 0x21, 0x9D, 0x54, 0x0C, 0xB9 };
	byte[] iv = { 0x69, 0x6B, 0xF5, 0xD0, 0x10, 0xB5, 0xFE, 0xEF };
	byte[] expected = { 0x12, 0x2B, 0xF0, 0x54, 0xFF, 0x2F, 0xE2, 0xF0, 0x36, 0x9A, 0x3E, 0xFE, 0x57, 0x56, 0x0E, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k88b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k88b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k88b64_CFB8_Zeros ()
{
	byte[] key = { 0x8B, 0x1D, 0xD0, 0x5C, 0x3E, 0xF4, 0x5B, 0xA5, 0x56, 0x87, 0xE8 };
	byte[] iv = { 0x14, 0x01, 0x4B, 0x90, 0x67, 0x02, 0x79, 0x3F };
	byte[] expected = { 0xA1, 0x7D, 0x02, 0x58, 0xBC, 0x3E, 0x56, 0x3E, 0xF6, 0x08, 0x08, 0xB0, 0xD0, 0xD1, 0xAC, 0x9F, 0x29, 0x65, 0x18, 0x76, 0x2C, 0x96, 0xCC, 0x8C };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k88b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k88b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k88b64_CFB8_PKCS7 ()
{
	byte[] key = { 0xCB, 0xD9, 0xE0, 0xD8, 0x82, 0xA0, 0x06, 0xD1, 0x6C, 0x5F, 0x8F };
	byte[] iv = { 0x73, 0x14, 0x81, 0x8C, 0x59, 0xE4, 0x33, 0xDF };
	byte[] expected = { 0x31, 0xA2, 0xA9, 0xCE, 0xAF, 0xF1, 0x8F, 0xA5, 0x02, 0xD8, 0xF5, 0xDC, 0x2C, 0x41, 0x8E, 0x64, 0x81, 0xCA, 0xBE, 0x89, 0xC3, 0x19, 0x24, 0x78 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k88b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k88b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k88b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k88b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k88b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k96b64_ECB_None ()
{
	byte[] key = { 0x72, 0xD8, 0x0A, 0x9D, 0xDA, 0x9D, 0xB1, 0x78, 0x61, 0x9C, 0xD8, 0x57 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x31, 0x21, 0x9D, 0xD9, 0x12, 0x95, 0x79, 0x30 };
	byte[] expected = { 0x41, 0xA6, 0x5B, 0x2D, 0x51, 0x55, 0x1B, 0xE2, 0x41, 0xA6, 0x5B, 0x2D, 0x51, 0x55, 0x1B, 0xE2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k96b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k96b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k96b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k96b64_ECB_Zeros ()
{
	byte[] key = { 0x5D, 0x07, 0x3C, 0x15, 0x3F, 0xE1, 0xB2, 0x72, 0x9F, 0x1A, 0xBE, 0x21 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x76, 0xE9, 0x93, 0x9F, 0xD1, 0x6A, 0xCE, 0x79 };
	byte[] expected = { 0x56, 0xF6, 0xF3, 0xAE, 0xCD, 0x73, 0x4F, 0x12, 0x56, 0xF6, 0xF3, 0xAE, 0xCD, 0x73, 0x4F, 0x12, 0x56, 0xF6, 0xF3, 0xAE, 0xCD, 0x73, 0x4F, 0x12 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k96b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k96b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k96b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k96b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k96b64_ECB_PKCS7 ()
{
	byte[] key = { 0x79, 0xCA, 0xDB, 0xBE, 0x8C, 0x10, 0x1E, 0xEB, 0x8B, 0x16, 0x00, 0x1B };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x17, 0x42, 0x68, 0x21, 0xBC, 0x52, 0x6A, 0xF6 };
	byte[] expected = { 0x86, 0xB2, 0x84, 0xAA, 0x58, 0xCB, 0x3F, 0x19, 0x86, 0xB2, 0x84, 0xAA, 0x58, 0xCB, 0x3F, 0x19, 0x75, 0xB8, 0x91, 0xC8, 0x17, 0xE2, 0x1C, 0x4A };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k96b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k96b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k96b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k96b64_CBC_None ()
{
	byte[] key = { 0x68, 0xC6, 0xF2, 0x13, 0xEA, 0x3D, 0x68, 0x09, 0xAC, 0x07, 0x21, 0x1F };
	byte[] iv = { 0x42, 0x47, 0xE6, 0x98, 0xF8, 0xFE, 0xCD, 0xFE };
	byte[] expected = { 0x7F, 0x9C, 0xCE, 0xC5, 0x2C, 0xB6, 0x60, 0xC3, 0xF3, 0x5F, 0x7E, 0x95, 0x6F, 0xFE, 0x8E, 0xC1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k96b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k96b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k96b64_CBC_Zeros ()
{
	byte[] key = { 0xDF, 0x00, 0x49, 0x93, 0xA1, 0x49, 0x50, 0x03, 0x52, 0x9C, 0x86, 0xF6 };
	byte[] iv = { 0x69, 0xFC, 0x72, 0xA2, 0x60, 0xF7, 0x4C, 0xB0 };
	byte[] expected = { 0x16, 0x07, 0x45, 0x07, 0xF8, 0xAE, 0xD3, 0xEA, 0x94, 0x1E, 0xC9, 0x1A, 0xEF, 0x8D, 0x3E, 0xF7, 0x88, 0x7D, 0x8D, 0xF8, 0xC6, 0x0A, 0xFA, 0x82 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k96b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k96b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k96b64_CBC_PKCS7 ()
{
	byte[] key = { 0x04, 0x2B, 0x2E, 0x98, 0x97, 0x84, 0x72, 0x0A, 0x78, 0x61, 0x02, 0xA9 };
	byte[] iv = { 0x16, 0x0A, 0x00, 0x48, 0xC3, 0x4F, 0x63, 0x05 };
	byte[] expected = { 0xD2, 0xC4, 0xC7, 0x02, 0xC7, 0xDB, 0xFB, 0xF6, 0xC1, 0x4D, 0x2D, 0x62, 0xF6, 0x57, 0x84, 0x84, 0xF2, 0x9B, 0x5C, 0x42, 0x66, 0x9B, 0x33, 0x1D };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k96b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k96b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k96b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k96b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k96b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k96b64_CFB8_None ()
{
	byte[] key = { 0xDE, 0x6E, 0x40, 0xC3, 0x7D, 0x71, 0x0D, 0xCB, 0xA3, 0x62, 0x14, 0x76 };
	byte[] iv = { 0x72, 0x9E, 0xB4, 0xEE, 0x9B, 0x87, 0xAF, 0x12 };
	byte[] expected = { 0x14, 0x20, 0x3B, 0x35, 0xE2, 0x81, 0x84, 0x15, 0x6C, 0xA5, 0x4A, 0x94, 0xB3, 0xC0, 0x8D, 0x6A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k96b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k96b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k96b64_CFB8_Zeros ()
{
	byte[] key = { 0xCF, 0x64, 0x81, 0x8F, 0x7D, 0x75, 0x8D, 0xB2, 0x9D, 0xE7, 0x39, 0xE3 };
	byte[] iv = { 0x30, 0xF2, 0x9E, 0x76, 0x96, 0x13, 0xCB, 0xDF };
	byte[] expected = { 0xC4, 0x0E, 0xE8, 0x61, 0x92, 0xB8, 0x9D, 0xDE, 0x0B, 0x39, 0x47, 0xD4, 0xD8, 0x05, 0x35, 0xF9, 0x0A, 0xAF, 0x63, 0x30, 0x4A, 0x82, 0x8C, 0xF2 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k96b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k96b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k96b64_CFB8_PKCS7 ()
{
	byte[] key = { 0xC5, 0xF4, 0x44, 0xF2, 0xA0, 0xC3, 0xA7, 0x87, 0x64, 0x36, 0x5A, 0xFA };
	byte[] iv = { 0x20, 0xC5, 0x5E, 0x57, 0x5E, 0x0E, 0x2D, 0xDD };
	byte[] expected = { 0x66, 0x93, 0x1E, 0x15, 0x17, 0x5C, 0x3C, 0x07, 0xDB, 0x2F, 0xD9, 0x00, 0x0C, 0x3F, 0x9E, 0xBB, 0xB9, 0x32, 0xDD, 0x2D, 0x57, 0x69, 0x3D, 0xC3 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k96b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k96b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k96b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k96b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k96b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k104b64_ECB_None ()
{
	byte[] key = { 0x04, 0x5B, 0x99, 0xD3, 0xBC, 0x00, 0x27, 0xA3, 0xDC, 0x57, 0x4C, 0x82, 0xD6 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x70, 0x3D, 0xE7, 0xBC, 0x82, 0xFD, 0x8F, 0x03 };
	byte[] expected = { 0x5D, 0xEA, 0x9F, 0x1F, 0x19, 0xBB, 0x3D, 0x26, 0x5D, 0xEA, 0x9F, 0x1F, 0x19, 0xBB, 0x3D, 0x26, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k104b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k104b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k104b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k104b64_ECB_Zeros ()
{
	byte[] key = { 0xA1, 0x3B, 0xDF, 0x6F, 0x6D, 0x2B, 0x7B, 0x0B, 0x13, 0x3E, 0x84, 0x35, 0x3C };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xE6, 0x74, 0x41, 0xB6, 0xB4, 0x31, 0xB2, 0x6A };
	byte[] expected = { 0xAF, 0x46, 0x98, 0xF8, 0xC1, 0x4B, 0x45, 0x09, 0xAF, 0x46, 0x98, 0xF8, 0xC1, 0x4B, 0x45, 0x09, 0xAF, 0x46, 0x98, 0xF8, 0xC1, 0x4B, 0x45, 0x09 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k104b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k104b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k104b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k104b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k104b64_ECB_PKCS7 ()
{
	byte[] key = { 0x28, 0xDF, 0x8C, 0x1B, 0x7E, 0x04, 0xB2, 0x89, 0x72, 0xDA, 0x19, 0x57, 0x81 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xB8, 0x82, 0xA7, 0xBF, 0x99, 0xE9, 0x39, 0x02 };
	byte[] expected = { 0x5D, 0xEB, 0xD8, 0x26, 0x51, 0x86, 0xFB, 0x0E, 0x5D, 0xEB, 0xD8, 0x26, 0x51, 0x86, 0xFB, 0x0E, 0x1C, 0xFD, 0xE2, 0x77, 0xB6, 0x74, 0x55, 0x9C };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k104b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k104b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k104b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k104b64_CBC_None ()
{
	byte[] key = { 0xF8, 0xCE, 0xA2, 0x33, 0xE5, 0x7D, 0x43, 0x72, 0xA9, 0xF5, 0xF1, 0x80, 0xBC };
	byte[] iv = { 0x12, 0xFF, 0x74, 0x3A, 0x36, 0x42, 0xBE, 0x78 };
	byte[] expected = { 0x64, 0xCD, 0x86, 0xA1, 0x1B, 0xB1, 0xD3, 0x9F, 0x8E, 0xFC, 0x42, 0xB8, 0x56, 0x96, 0x56, 0x38, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k104b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k104b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k104b64_CBC_Zeros ()
{
	byte[] key = { 0xEF, 0x4E, 0x02, 0x86, 0x5F, 0xE5, 0x94, 0x05, 0xEF, 0x8D, 0x8D, 0x5D, 0x04 };
	byte[] iv = { 0x98, 0x23, 0x93, 0xF7, 0x6D, 0x02, 0xB1, 0x73 };
	byte[] expected = { 0x50, 0x08, 0xAB, 0x8B, 0x26, 0x0D, 0x5B, 0x73, 0x3F, 0xE7, 0x75, 0x55, 0x4F, 0x9C, 0xDC, 0xFC, 0x17, 0x58, 0x2A, 0xB2, 0xFC, 0x54, 0x15, 0x97 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k104b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k104b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k104b64_CBC_PKCS7 ()
{
	byte[] key = { 0xE3, 0xD2, 0xC2, 0xA0, 0x54, 0xF5, 0xFC, 0xFC, 0x94, 0xA2, 0x6F, 0x6F, 0x52 };
	byte[] iv = { 0xBA, 0x5D, 0x0D, 0xBA, 0x0D, 0x0C, 0x4E, 0x5B };
	byte[] expected = { 0x6C, 0x5B, 0x74, 0x54, 0x0F, 0x86, 0x62, 0x06, 0x11, 0x65, 0xAA, 0x0B, 0x4F, 0x65, 0x34, 0x26, 0xAF, 0x26, 0x0D, 0xF4, 0xCE, 0xB6, 0xEE, 0xF0 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k104b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k104b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k104b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k104b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k104b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k104b64_CFB8_None ()
{
	byte[] key = { 0xB3, 0xE2, 0x4D, 0x91, 0xE9, 0xF8, 0x72, 0xA4, 0x2E, 0x00, 0x0C, 0x08, 0x96 };
	byte[] iv = { 0x48, 0xF8, 0xDD, 0x61, 0xD5, 0x00, 0xD0, 0xE1 };
	byte[] expected = { 0xB9, 0xAA, 0x53, 0xD8, 0xCB, 0x23, 0xA6, 0x41, 0x69, 0x84, 0x2D, 0xD5, 0x4F, 0x45, 0xC2, 0x8D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k104b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k104b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k104b64_CFB8_Zeros ()
{
	byte[] key = { 0xD4, 0x9C, 0x6B, 0x12, 0x41, 0x93, 0xEB, 0xDA, 0xDF, 0x7A, 0x81, 0x23, 0x1F };
	byte[] iv = { 0x3C, 0x0E, 0x48, 0xAA, 0xD8, 0x48, 0xE9, 0xC8 };
	byte[] expected = { 0x66, 0x39, 0x26, 0x0B, 0x81, 0xD8, 0x9A, 0x2F, 0xF1, 0x2C, 0xCF, 0x75, 0x8C, 0x01, 0x4D, 0x6E, 0x2A, 0x67, 0x9D, 0x0D, 0xA5, 0x56, 0x15, 0x41 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k104b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k104b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k104b64_CFB8_PKCS7 ()
{
	byte[] key = { 0x2C, 0x38, 0x19, 0x43, 0x93, 0x38, 0x85, 0xC4, 0xF2, 0x19, 0xC7, 0x1B, 0x76 };
	byte[] iv = { 0xB4, 0x1B, 0x9C, 0x82, 0xB5, 0x6E, 0x42, 0xAF };
	byte[] expected = { 0xC5, 0x56, 0x04, 0x85, 0x0A, 0x52, 0x8B, 0x02, 0x69, 0xB6, 0xCF, 0xC7, 0xA9, 0x35, 0x63, 0xF7, 0x4B, 0x48, 0xF3, 0xD0, 0xFF, 0x74, 0xA7, 0xB5 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k104b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k104b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k104b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k104b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k104b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k112b64_ECB_None ()
{
	byte[] key = { 0xB7, 0x95, 0xA4, 0x42, 0x21, 0x3D, 0x30, 0x51, 0x98, 0x01, 0xA0, 0x6C, 0x45, 0x68 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x3B, 0x36, 0x51, 0x24, 0xF4, 0x1A, 0xC1, 0x91 };
	byte[] expected = { 0x31, 0xAE, 0xBA, 0xFB, 0xB4, 0xFA, 0x78, 0x30, 0x31, 0xAE, 0xBA, 0xFB, 0xB4, 0xFA, 0x78, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k112b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k112b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k112b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k112b64_ECB_Zeros ()
{
	byte[] key = { 0xB1, 0x8E, 0x09, 0xFB, 0x70, 0x03, 0x6A, 0xF2, 0xCF, 0x9D, 0x9B, 0xD7, 0x10, 0xD4 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x64, 0x15, 0x78, 0xB8, 0x25, 0x15, 0xFA, 0xC8 };
	byte[] expected = { 0xB1, 0xC2, 0x27, 0xA8, 0x32, 0xBA, 0x34, 0x06, 0xB1, 0xC2, 0x27, 0xA8, 0x32, 0xBA, 0x34, 0x06, 0xB1, 0xC2, 0x27, 0xA8, 0x32, 0xBA, 0x34, 0x06 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k112b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k112b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k112b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k112b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k112b64_ECB_PKCS7 ()
{
	byte[] key = { 0x4F, 0xE8, 0x2C, 0x62, 0x98, 0x89, 0xEF, 0x11, 0x29, 0xB2, 0xDD, 0x4D, 0xE1, 0x39 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x15, 0xE0, 0x95, 0x29, 0xEB, 0xE5, 0xC7, 0x8E };
	byte[] expected = { 0x43, 0x79, 0x6E, 0xCF, 0x63, 0x68, 0xF0, 0x55, 0x43, 0x79, 0x6E, 0xCF, 0x63, 0x68, 0xF0, 0x55, 0x80, 0x64, 0x15, 0x36, 0x08, 0xD0, 0x76, 0x58 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k112b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k112b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k112b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k112b64_CBC_None ()
{
	byte[] key = { 0xC0, 0x04, 0xA9, 0x3C, 0x94, 0xA1, 0x78, 0xA2, 0x4B, 0x94, 0x6F, 0x19, 0xD1, 0xE1 };
	byte[] iv = { 0x28, 0x94, 0x16, 0x28, 0x69, 0x64, 0xF6, 0x83 };
	byte[] expected = { 0xB7, 0x2F, 0x20, 0x02, 0xAD, 0x97, 0x21, 0x45, 0xDA, 0xC2, 0x0D, 0xD9, 0xEB, 0xCC, 0xA0, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k112b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k112b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k112b64_CBC_Zeros ()
{
	byte[] key = { 0x59, 0xFF, 0xC2, 0xB5, 0x62, 0x84, 0x27, 0x49, 0x4B, 0xFF, 0xFF, 0xCE, 0xBB, 0xBD };
	byte[] iv = { 0x2E, 0x9E, 0xD3, 0xF6, 0xFC, 0xD7, 0xC6, 0x1C };
	byte[] expected = { 0x38, 0xE4, 0x4D, 0xD5, 0x3F, 0x74, 0x44, 0x90, 0x11, 0xCD, 0x6E, 0x13, 0x7A, 0x9A, 0x82, 0xBB, 0xBD, 0xD1, 0x0F, 0x38, 0x0F, 0x5F, 0x97, 0x14 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k112b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k112b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k112b64_CBC_PKCS7 ()
{
	byte[] key = { 0xE4, 0x49, 0xA4, 0xBE, 0x30, 0xE1, 0xB5, 0x21, 0x33, 0xC6, 0x37, 0x88, 0x30, 0xEC };
	byte[] iv = { 0x74, 0xAC, 0x28, 0x92, 0xA5, 0xF1, 0x31, 0xC9 };
	byte[] expected = { 0xE5, 0x7B, 0x53, 0x65, 0x37, 0xD8, 0x29, 0xBD, 0x4B, 0x73, 0x3B, 0x1B, 0x5B, 0x00, 0x04, 0xE2, 0x11, 0x5B, 0x24, 0x6F, 0x6D, 0x7F, 0x1C, 0xE8 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k112b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k112b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k112b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k112b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k112b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k112b64_CFB8_None ()
{
	byte[] key = { 0x70, 0x12, 0xEC, 0xAB, 0x6E, 0x1D, 0xEF, 0x51, 0xEE, 0xA8, 0x81, 0xE1, 0x21, 0xFF };
	byte[] iv = { 0x0E, 0x56, 0xA2, 0xA3, 0x8C, 0x5D, 0x9C, 0x1F };
	byte[] expected = { 0x71, 0x1C, 0x76, 0xB1, 0x61, 0x32, 0x77, 0xB7, 0x98, 0x42, 0x31, 0xF1, 0x0A, 0xE4, 0xC3, 0x83, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k112b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k112b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k112b64_CFB8_Zeros ()
{
	byte[] key = { 0x48, 0x66, 0x16, 0xD6, 0x57, 0xBF, 0x38, 0xB7, 0x22, 0x81, 0x9F, 0x75, 0xE0, 0x88 };
	byte[] iv = { 0x51, 0x2C, 0x6A, 0x59, 0xAB, 0xD2, 0xAE, 0x6E };
	byte[] expected = { 0xF1, 0x9E, 0x85, 0x7A, 0x7D, 0xF0, 0x39, 0x0D, 0x11, 0x47, 0x11, 0xC0, 0x1A, 0x19, 0x21, 0x85, 0x95, 0x40, 0xDA, 0x4A, 0xEE, 0x49, 0xC7, 0x54 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k112b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k112b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k112b64_CFB8_PKCS7 ()
{
	byte[] key = { 0x55, 0x20, 0xA1, 0xD8, 0xFA, 0xE7, 0x0D, 0xF9, 0xB6, 0x4B, 0x90, 0x10, 0xDE, 0xB1 };
	byte[] iv = { 0x26, 0x6C, 0xB0, 0xB4, 0x4D, 0x7F, 0x5C, 0x18 };
	byte[] expected = { 0xC8, 0x00, 0x9F, 0x21, 0x2C, 0xB0, 0x75, 0x6C, 0x62, 0xD8, 0xD0, 0x30, 0x11, 0x93, 0x73, 0x2F, 0xC5, 0xBC, 0xB1, 0xED, 0x2E, 0xBE, 0xCF, 0xBC };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k112b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k112b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k112b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k112b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k112b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k120b64_ECB_None ()
{
	byte[] key = { 0x5D, 0x08, 0xC7, 0xB8, 0xB1, 0xEB, 0x89, 0x1C, 0xC0, 0x3F, 0xE6, 0x2F, 0xC4, 0x79, 0x11 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x76, 0x1C, 0xAC, 0x0F, 0x39, 0x6C, 0x1A, 0x44 };
	byte[] expected = { 0xA4, 0xC1, 0x60, 0x59, 0x6B, 0x45, 0xE0, 0x4C, 0xA4, 0xC1, 0x60, 0x59, 0x6B, 0x45, 0xE0, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k120b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k120b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k120b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k120b64_ECB_Zeros ()
{
	byte[] key = { 0x1D, 0x13, 0x51, 0x02, 0x28, 0xF4, 0xF0, 0x13, 0x90, 0xFD, 0xE4, 0xC0, 0xE5, 0x57, 0x9A };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x9E, 0xC9, 0xA7, 0x52, 0xD2, 0x6E, 0x9B, 0xE4 };
	byte[] expected = { 0x23, 0x58, 0x1C, 0x66, 0x7D, 0x2F, 0x71, 0x4F, 0x23, 0x58, 0x1C, 0x66, 0x7D, 0x2F, 0x71, 0x4F, 0x23, 0x58, 0x1C, 0x66, 0x7D, 0x2F, 0x71, 0x4F };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k120b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k120b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k120b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k120b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k120b64_ECB_PKCS7 ()
{
	byte[] key = { 0x23, 0xF2, 0xFB, 0x09, 0xC1, 0xEF, 0xC1, 0xFF, 0x16, 0xFF, 0x60, 0xC1, 0x3A, 0x94, 0x3E };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xB6, 0x10, 0xE3, 0xE9, 0x24, 0x03, 0xCA, 0xAA };
	byte[] expected = { 0x92, 0xF3, 0xF0, 0x81, 0x13, 0x40, 0x19, 0x61, 0x92, 0xF3, 0xF0, 0x81, 0x13, 0x40, 0x19, 0x61, 0x36, 0xCC, 0xEC, 0x80, 0xF6, 0xF4, 0xCC, 0xB7 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k120b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k120b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k120b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k120b64_CBC_None ()
{
	byte[] key = { 0x12, 0x43, 0xEE, 0x74, 0xE8, 0x4E, 0x3A, 0xF7, 0x24, 0x58, 0x10, 0xC9, 0x41, 0x7E, 0x46 };
	byte[] iv = { 0x7B, 0x57, 0x22, 0x19, 0xFB, 0x30, 0xED, 0x48 };
	byte[] expected = { 0x75, 0xB0, 0x41, 0x19, 0x7F, 0x80, 0x91, 0x4A, 0xCD, 0x03, 0x41, 0x59, 0xE4, 0xC0, 0x92, 0xE7, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k120b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k120b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k120b64_CBC_Zeros ()
{
	byte[] key = { 0x2A, 0xCC, 0xFF, 0xD0, 0x46, 0xAF, 0x74, 0xB2, 0x0E, 0x64, 0xBD, 0xE9, 0x6D, 0xC5, 0xE8 };
	byte[] iv = { 0x10, 0x21, 0xE3, 0xCB, 0x46, 0x02, 0x33, 0x4F };
	byte[] expected = { 0x88, 0x71, 0x0D, 0x01, 0xE9, 0xD3, 0xC7, 0x3F, 0x7E, 0xCA, 0xA7, 0x9A, 0x2D, 0x95, 0xC6, 0xED, 0xDA, 0xAA, 0xE9, 0x23, 0x01, 0x70, 0x6E, 0x59 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k120b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k120b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k120b64_CBC_PKCS7 ()
{
	byte[] key = { 0xE8, 0xE8, 0x44, 0x9D, 0xEA, 0x33, 0x10, 0xCB, 0xEA, 0xEF, 0x69, 0x94, 0xE4, 0x31, 0xF0 };
	byte[] iv = { 0xC7, 0x0F, 0xE1, 0x79, 0x2B, 0x57, 0x5D, 0xA7 };
	byte[] expected = { 0x7E, 0x1F, 0xD6, 0xCF, 0xB1, 0xAE, 0xC0, 0x2C, 0xD6, 0x02, 0x01, 0x62, 0x77, 0x95, 0x02, 0xE8, 0x8D, 0xEC, 0x8D, 0xCC, 0xB2, 0x6B, 0x92, 0x7A };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k120b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k120b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k120b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k120b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k120b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k120b64_CFB8_None ()
{
	byte[] key = { 0x0F, 0x0D, 0x1F, 0x09, 0xC2, 0xEA, 0xC5, 0xFE, 0xD1, 0x5A, 0x4C, 0x39, 0x2E, 0x62, 0xED };
	byte[] iv = { 0xCA, 0x90, 0x74, 0xAD, 0x6B, 0xD5, 0x42, 0xCF };
	byte[] expected = { 0xEB, 0xC3, 0xF4, 0x08, 0xCF, 0x11, 0x3E, 0xC4, 0x98, 0x8A, 0xAB, 0x6F, 0xEE, 0x32, 0xFC, 0x2B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k120b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k120b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k120b64_CFB8_Zeros ()
{
	byte[] key = { 0xDA, 0xAD, 0xD7, 0xFB, 0x36, 0x64, 0x3B, 0xE8, 0x35, 0x64, 0xC8, 0xAF, 0x0D, 0xB3, 0xAC };
	byte[] iv = { 0x6B, 0x99, 0x8D, 0xCA, 0x51, 0xD8, 0x26, 0x48 };
	byte[] expected = { 0xDE, 0xED, 0xF4, 0xA8, 0x9D, 0x5C, 0xCE, 0x22, 0x7A, 0xD5, 0x1B, 0x3F, 0x89, 0x6E, 0x91, 0x61, 0xE1, 0x44, 0x1E, 0x5C, 0xFA, 0xC1, 0x40, 0x97 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k120b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k120b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k120b64_CFB8_PKCS7 ()
{
	byte[] key = { 0x26, 0xA9, 0xE5, 0xE2, 0xE4, 0x48, 0xB5, 0x9F, 0xAC, 0x3E, 0x77, 0xB0, 0xEF, 0x1B, 0x00 };
	byte[] iv = { 0x0E, 0x98, 0x7F, 0xC4, 0xAC, 0x08, 0x94, 0x03 };
	byte[] expected = { 0xAD, 0xEC, 0xD6, 0x71, 0xDF, 0x36, 0x69, 0x80, 0xE6, 0x74, 0x79, 0xC2, 0xE0, 0xDF, 0xCF, 0xD8, 0xB4, 0x3A, 0x22, 0x6F, 0x41, 0xAD, 0x77, 0x4D };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k120b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k120b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k120b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k120b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k120b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRC2_k128b64_ECB_None ()
{
	byte[] key = { 0x4F, 0x02, 0xB1, 0xA6, 0x5E, 0xAE, 0xB9, 0x0C, 0x3A, 0x96, 0xFF, 0x62, 0x90, 0x9A, 0xD8, 0x1B };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xC7, 0x89, 0x19, 0x4F, 0x3C, 0xC3, 0x05, 0x83 };
	byte[] expected = { 0xC8, 0x83, 0x4D, 0xE2, 0x6A, 0xFA, 0x75, 0x41, 0xC8, 0x83, 0x4D, 0xE2, 0x6A, 0xFA, 0x75, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k128b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k128b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k128b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestRC2_k128b64_ECB_Zeros ()
{
	byte[] key = { 0x45, 0xBE, 0xD8, 0x8E, 0x0A, 0xE7, 0xF9, 0xE2, 0x3C, 0x33, 0xE7, 0x93, 0xD4, 0x9D, 0xAE, 0x2B };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x83, 0x27, 0x57, 0x97, 0x06, 0x4F, 0xFE, 0xB3 };
	byte[] expected = { 0x28, 0x59, 0x45, 0xF6, 0x5E, 0x4F, 0x97, 0xF3, 0x28, 0x59, 0x45, 0xF6, 0x5E, 0x4F, 0x97, 0xF3, 0x28, 0x59, 0x45, 0xF6, 0x5E, 0x4F, 0x97, 0xF3 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k128b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k128b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("RC2_k128b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k128b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k128b64_ECB_PKCS7 ()
{
	byte[] key = { 0x6F, 0x04, 0x76, 0x7D, 0x88, 0x01, 0x29, 0x6A, 0xD5, 0x1E, 0x38, 0x9D, 0xED, 0x56, 0xAC, 0x9C };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x82, 0x74, 0xAC, 0xAA, 0x42, 0x29, 0x35, 0x8D };
	byte[] expected = { 0xCB, 0xE5, 0xBB, 0xCC, 0x99, 0x8D, 0x1D, 0xA6, 0xCB, 0xE5, 0xBB, 0xCC, 0x99, 0x8D, 0x1D, 0xA6, 0x5B, 0x35, 0x28, 0xE7, 0xAC, 0xFE, 0xF0, 0xD1 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k128b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("RC2_k128b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k128b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRC2_k128b64_CBC_None ()
{
	byte[] key = { 0x17, 0x3F, 0x40, 0xF3, 0xDC, 0xFF, 0x8F, 0xF2, 0x71, 0x2E, 0x8B, 0x6A, 0xE0, 0x2E, 0x3F, 0x82 };
	byte[] iv = { 0xFA, 0xB4, 0x41, 0x91, 0x34, 0xFC, 0x9B, 0x49 };
	byte[] expected = { 0x05, 0x1B, 0x27, 0x78, 0xF0, 0x3D, 0xC4, 0x77, 0x9E, 0x59, 0x27, 0xEC, 0x2D, 0x1D, 0x7F, 0x56, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k128b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k128b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestRC2_k128b64_CBC_Zeros ()
{
	byte[] key = { 0x49, 0x89, 0x3E, 0x29, 0xCB, 0xB9, 0x06, 0x85, 0x7F, 0x8B, 0x86, 0xEB, 0xD7, 0x47, 0x91, 0x1D };
	byte[] iv = { 0xCB, 0xA1, 0x0F, 0x53, 0x7B, 0x71, 0x04, 0x89 };
	byte[] expected = { 0x17, 0x58, 0xD1, 0xF4, 0x1E, 0x58, 0xB0, 0x10, 0x31, 0x17, 0x40, 0x3F, 0x40, 0x22, 0x75, 0x32, 0x4F, 0xDE, 0x64, 0xE0, 0x66, 0xF4, 0xF7, 0xA0 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k128b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k128b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k128b64_CBC_PKCS7 ()
{
	byte[] key = { 0x22, 0x04, 0xDB, 0x15, 0xD6, 0x2E, 0xEF, 0x6D, 0x5D, 0x6A, 0xDA, 0x55, 0x67, 0x41, 0x4E, 0xFD };
	byte[] iv = { 0xB8, 0xD1, 0xD8, 0x23, 0x00, 0x39, 0x89, 0x83 };
	byte[] expected = { 0xC8, 0x4F, 0xCC, 0x05, 0x7F, 0x44, 0x49, 0xBE, 0x73, 0x78, 0xE8, 0x7B, 0xD9, 0xB1, 0x56, 0xC3, 0x37, 0x1E, 0xBE, 0x4D, 0x2B, 0x2F, 0xC7, 0x9E };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k128b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k128b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k128b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k128b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters RC2_k128b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestRC2_k128b64_CFB8_None ()
{
	byte[] key = { 0x61, 0x93, 0x31, 0x3A, 0xC2, 0x9B, 0x53, 0xB1, 0x26, 0x64, 0x36, 0x03, 0x16, 0x4A, 0xE3, 0x99 };
	byte[] iv = { 0xDD, 0xAD, 0xA4, 0x57, 0xC1, 0x21, 0xF1, 0xA8 };
	byte[] expected = { 0x94, 0xD9, 0x62, 0x83, 0x80, 0x4C, 0x91, 0x90, 0x63, 0x41, 0xBC, 0xBD, 0x8B, 0x7F, 0xD9, 0xB1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k128b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k128b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestRC2_k128b64_CFB8_Zeros ()
{
	byte[] key = { 0x64, 0x09, 0x9A, 0xF0, 0xD2, 0x52, 0x8C, 0x03, 0xF3, 0xBF, 0x1B, 0x9B, 0x92, 0x0E, 0xBA, 0x33 };
	byte[] iv = { 0x15, 0x64, 0xE4, 0xFA, 0xFA, 0x58, 0x54, 0x7B };
	byte[] expected = { 0xC8, 0x8F, 0xCC, 0x77, 0xA3, 0x82, 0x31, 0xD4, 0x7A, 0x68, 0x05, 0x8F, 0xF2, 0x1B, 0x9E, 0xCC, 0xDA, 0x6F, 0x74, 0x1D, 0x43, 0xE0, 0x90, 0x8B };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k128b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k128b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestRC2_k128b64_CFB8_PKCS7 ()
{
	byte[] key = { 0x1F, 0x09, 0xF8, 0x1B, 0xA9, 0xA4, 0x70, 0x8D, 0x53, 0x76, 0x19, 0x4A, 0xAA, 0x62, 0x84, 0x94 };
	byte[] iv = { 0xCC, 0x7B, 0xBE, 0xE9, 0xEE, 0x8E, 0x9C, 0x02 };
	byte[] expected = { 0xA7, 0x1B, 0xD5, 0x4E, 0xDB, 0xF7, 0x84, 0xC2, 0xAA, 0x89, 0xAA, 0x3C, 0x3A, 0x63, 0x8A, 0xB2, 0xEF, 0x0C, 0x5B, 0xB0, 0xF4, 0xD9, 0x0A, 0x46 };

	SymmetricAlgorithm algo = RC2.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("RC2_k128b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("RC2_k128b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters RC2_k128b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k128b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters RC2_k128b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRijndael_k128b128_ECB_None ()
{
	byte[] key = { 0xAF, 0x4D, 0xFE, 0x58, 0x33, 0xAC, 0x91, 0xB2, 0xFA, 0xA3, 0x96, 0x54, 0x0B, 0x68, 0xDD, 0xA1 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xAF, 0x70, 0xC2, 0x2E, 0x2D, 0xF1, 0x0D, 0x7F, 0x52, 0xF4, 0x65, 0x79, 0x78, 0xAC, 0x80, 0xEF };
	byte[] expected = { 0x6D, 0xC2, 0x4A, 0x51, 0x2D, 0xAB, 0x67, 0xCB, 0xD8, 0xD4, 0xD5, 0xE6, 0x0B, 0x24, 0x02, 0x90, 0x6D, 0xC2, 0x4A, 0x51, 0x2D, 0xAB, 0x67, 0xCB, 0xD8, 0xD4, 0xD5, 0xE6, 0x0B, 0x24, 0x02, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b128_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k128b128_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b128_ECB_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k128b128_ECB_Zeros ()
{
	byte[] key = { 0xA4, 0x39, 0x01, 0x00, 0xDB, 0x0A, 0x47, 0xD8, 0xD8, 0xDC, 0x01, 0xF4, 0xBE, 0x96, 0xF4, 0xBB };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xEA, 0xBD, 0x55, 0x85, 0x3F, 0xC1, 0x5F, 0xCB, 0x06, 0x26, 0x3F, 0x88, 0x6A, 0x2D, 0x69, 0x45 };
	byte[] expected = { 0x19, 0x32, 0x7E, 0x79, 0xE3, 0xC1, 0xFE, 0xA0, 0xFD, 0x26, 0x27, 0x61, 0xC0, 0xB8, 0x06, 0xC2, 0x19, 0x32, 0x7E, 0x79, 0xE3, 0xC1, 0xFE, 0xA0, 0xFD, 0x26, 0x27, 0x61, 0xC0, 0xB8, 0x06, 0xC2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k128b128_ECB_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k128b128_ECB_PKCS7 ()
{
	byte[] key = { 0x5C, 0x58, 0x03, 0x1D, 0x05, 0x07, 0xDE, 0x93, 0x8D, 0x85, 0xFD, 0x50, 0x68, 0xA3, 0xD7, 0x6B };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x1C, 0x32, 0xFE, 0x99, 0x95, 0x16, 0x74, 0xC0, 0x6F, 0xE6, 0x01, 0x2C, 0x1F, 0x07, 0x54, 0xE8 };
	byte[] expected = { 0xEE, 0x1C, 0x0B, 0x2F, 0x1E, 0xCE, 0x69, 0xBC, 0xEA, 0xF6, 0xED, 0xA9, 0xF0, 0xE3, 0xE7, 0xC3, 0xEE, 0x1C, 0x0B, 0x2F, 0x1E, 0xCE, 0x69, 0xBC, 0xEA, 0xF6, 0xED, 0xA9, 0xF0, 0xE3, 0xE7, 0xC3, 0x2E, 0xB4, 0x6F, 0x8C, 0xD3, 0x37, 0xF4, 0x8E, 0x6D, 0x08, 0x35, 0x47, 0xD1, 0x1A, 0xB2, 0xA0 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b128_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k128b128_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b128_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRijndael_k128b128_CBC_None ()
{
	byte[] key = { 0xED, 0xE4, 0xD9, 0x97, 0x8E, 0x5C, 0xF8, 0x86, 0xFE, 0x6B, 0xF4, 0xA7, 0x26, 0xDA, 0x70, 0x47 };
	byte[] iv = { 0x06, 0xE1, 0xA5, 0x97, 0x7E, 0x20, 0x0C, 0x47, 0xA4, 0xAF, 0xB8, 0xF3, 0x8D, 0x2E, 0xA9, 0xAC };
	byte[] expected = { 0xB1, 0x73, 0xDA, 0x05, 0x4C, 0x0D, 0x6C, 0x5D, 0x60, 0x72, 0x76, 0x79, 0x64, 0xA6, 0x45, 0x89, 0xA5, 0xCD, 0x35, 0x2C, 0x56, 0x12, 0x7D, 0xA6, 0x84, 0x36, 0xEB, 0xCC, 0xDF, 0x5C, 0xCB, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b128_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b128_CBC_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k128b128_CBC_Zeros ()
{
	byte[] key = { 0x7F, 0x03, 0x95, 0x4E, 0x42, 0x9E, 0x83, 0x85, 0x4B, 0x1A, 0x87, 0x36, 0xA1, 0x5B, 0xA8, 0x24 };
	byte[] iv = { 0x75, 0x49, 0x7B, 0xBE, 0x78, 0x55, 0x5F, 0xE9, 0x67, 0xCB, 0x7E, 0x30, 0x71, 0xD1, 0x36, 0x49 };
	byte[] expected = { 0xC8, 0xE2, 0xE5, 0x14, 0x17, 0x10, 0x14, 0xA5, 0x14, 0x8E, 0x59, 0x82, 0x7C, 0x92, 0x12, 0x91, 0x49, 0xE4, 0x24, 0x2C, 0x38, 0x98, 0x91, 0x0B, 0xD8, 0x5C, 0xD0, 0x79, 0xCD, 0x35, 0x85, 0x6B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k128b128_CBC_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k128b128_CBC_PKCS7 ()
{
	byte[] key = { 0x02, 0xE6, 0xC1, 0xE2, 0x7E, 0x89, 0xB9, 0x04, 0xE7, 0x9A, 0xB8, 0x83, 0xA4, 0xF8, 0x1B, 0x64 };
	byte[] iv = { 0xBC, 0xE4, 0x47, 0x1E, 0xD0, 0xDD, 0x09, 0x0D, 0xFC, 0xA1, 0x44, 0xCD, 0x88, 0x92, 0x41, 0xA5 };
	byte[] expected = { 0xEA, 0xB3, 0x9D, 0xCC, 0xE6, 0x74, 0x22, 0xE5, 0x15, 0xEE, 0x1C, 0xA9, 0x48, 0xB9, 0x55, 0x01, 0xEA, 0x9F, 0x98, 0x8D, 0x5D, 0x59, 0xB1, 0x1C, 0xEC, 0xE5, 0x68, 0xEE, 0x86, 0x22, 0x17, 0xBA, 0x95, 0x7D, 0xEC, 0x06, 0x4B, 0x48, 0x90, 0x0E, 0x75, 0x38, 0xC0, 0x28, 0x7D, 0x72, 0x32, 0xF8 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b128_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b128_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters Rijndael_k128b128_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k128b128_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k128b128_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k128b128_CFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b128_CFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b128_CFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b128_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b128_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b128_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRijndael_k128b192_ECB_None ()
{
	byte[] key = { 0xA5, 0x7F, 0xA2, 0x9F, 0xDA, 0xEE, 0x56, 0x2E, 0xF9, 0x3A, 0xEE, 0x1E, 0x30, 0x46, 0x80, 0x66 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x81, 0xE8, 0x4F, 0x8A, 0xFC, 0xD0, 0x12, 0xB3, 0xF8, 0x1F, 0x30, 0xE2, 0x40, 0x90, 0xFB, 0x96, 0x88, 0xC0, 0xC8, 0xF7, 0x4A, 0x3E, 0xC0, 0x73 };
	byte[] expected = { 0xC1, 0xC5, 0x13, 0x1B, 0x11, 0x93, 0x52, 0xE6, 0x4A, 0xA3, 0xF8, 0xE7, 0x28, 0xDE, 0x02, 0x9A, 0x5D, 0x2B, 0x14, 0x6A, 0x5D, 0x0F, 0x24, 0x8F, 0xC1, 0xC5, 0x13, 0x1B, 0x11, 0x93, 0x52, 0xE6, 0x4A, 0xA3, 0xF8, 0xE7, 0x28, 0xDE, 0x02, 0x9A, 0x5D, 0x2B, 0x14, 0x6A, 0x5D, 0x0F, 0x24, 0x8F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b192_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k128b192_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b192_ECB_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k128b192_ECB_Zeros ()
{
	byte[] key = { 0xDF, 0x1B, 0x73, 0xA3, 0xE3, 0x53, 0x75, 0x92, 0x2B, 0xD0, 0x44, 0x35, 0x94, 0xF5, 0xB2, 0xE7 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x21, 0x82, 0x61, 0x4A, 0x57, 0xC0, 0x7D, 0x96, 0xFF, 0xC2, 0x08, 0xC1, 0x6C, 0xDF, 0x7C, 0x65, 0xC1, 0x8B, 0xFE, 0x5E, 0xD5, 0x82, 0xAD, 0x98 };
	byte[] expected = { 0xC9, 0x4E, 0xE0, 0x8F, 0x95, 0x55, 0x52, 0x1A, 0x75, 0xA9, 0x92, 0x1D, 0xFA, 0x30, 0xBD, 0xB8, 0x55, 0xA7, 0x8B, 0xF9, 0x58, 0xE9, 0x1B, 0x4C, 0xC9, 0x4E, 0xE0, 0x8F, 0x95, 0x55, 0x52, 0x1A, 0x75, 0xA9, 0x92, 0x1D, 0xFA, 0x30, 0xBD, 0xB8, 0x55, 0xA7, 0x8B, 0xF9, 0x58, 0xE9, 0x1B, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k128b192_ECB_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k128b192_ECB_PKCS7 ()
{
	byte[] key = { 0x78, 0x75, 0x1F, 0xE7, 0xFA, 0x1F, 0xF4, 0x2D, 0x31, 0x36, 0x14, 0xA5, 0xB8, 0x31, 0x97, 0x47 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x91, 0x2F, 0xDC, 0x19, 0xC7, 0x6C, 0x67, 0x4A, 0x51, 0xE7, 0x08, 0xA5, 0xF9, 0xC6, 0xC3, 0x56, 0xF2, 0xED, 0xBD, 0xC9, 0x71, 0x9F, 0x02, 0xAF };
	byte[] expected = { 0xB1, 0x0D, 0xFD, 0xB0, 0x89, 0x3C, 0xF5, 0x52, 0x62, 0x22, 0x41, 0x20, 0xE4, 0x34, 0x03, 0x78, 0x37, 0xC2, 0xB1, 0xF9, 0x26, 0x0A, 0x7F, 0x0E, 0xB1, 0x0D, 0xFD, 0xB0, 0x89, 0x3C, 0xF5, 0x52, 0x62, 0x22, 0x41, 0x20, 0xE4, 0x34, 0x03, 0x78, 0x37, 0xC2, 0xB1, 0xF9, 0x26, 0x0A, 0x7F, 0x0E, 0xF9, 0x7A, 0x2D, 0xF9, 0x5C, 0xD5, 0xEA, 0x06, 0x18, 0xC9, 0x06, 0xD4, 0xD0, 0x0B, 0xD6, 0x19, 0x4E, 0x7E, 0x9C, 0x5F, 0xDE, 0x3D, 0xB4, 0x2A };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b192_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k128b192_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b192_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRijndael_k128b192_CBC_None ()
{
	byte[] key = { 0xBD, 0x01, 0x0F, 0x53, 0x53, 0x14, 0x90, 0x58, 0x22, 0x81, 0x6F, 0x79, 0x8C, 0x68, 0x21, 0x21 };
	byte[] iv = { 0xEE, 0x7B, 0xC0, 0x5F, 0x32, 0x59, 0x56, 0xB6, 0x7C, 0x17, 0x04, 0xC5, 0x64, 0x6A, 0xA1, 0x35, 0x6F, 0xAC, 0xB8, 0xCE, 0xFA, 0xCC, 0x76, 0xBE };
	byte[] expected = { 0x5D, 0xF5, 0x03, 0xD7, 0x17, 0xEE, 0x05, 0x18, 0x63, 0x99, 0xAB, 0x58, 0xBB, 0xC0, 0x04, 0x0A, 0x52, 0x1D, 0x4E, 0xA4, 0x8B, 0x68, 0xA3, 0x63, 0x7A, 0xBD, 0xAF, 0x0C, 0x85, 0x5D, 0xF8, 0x0D, 0x7A, 0x01, 0xF0, 0x76, 0x24, 0xF1, 0x8A, 0x95, 0x8B, 0xB2, 0xC0, 0xF7, 0x1D, 0xC5, 0x0E, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b192_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b192_CBC_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k128b192_CBC_Zeros ()
{
	byte[] key = { 0xE2, 0x9C, 0x2A, 0xAA, 0xD0, 0x02, 0xDD, 0xDF, 0xFE, 0xD7, 0xB0, 0x21, 0x1E, 0x52, 0xE5, 0x25 };
	byte[] iv = { 0xED, 0xF5, 0xD7, 0xF7, 0x8D, 0xB6, 0x91, 0x00, 0x81, 0x88, 0x75, 0x8C, 0x61, 0x13, 0x84, 0x46, 0x2A, 0x53, 0x02, 0xE9, 0xBB, 0x01, 0xF8, 0x24 };
	byte[] expected = { 0x55, 0x48, 0x90, 0x63, 0x5B, 0x93, 0x09, 0xA7, 0xF7, 0xB2, 0xC0, 0x4D, 0xB1, 0x1A, 0xF7, 0xC7, 0xF7, 0xC0, 0xB6, 0x29, 0x7A, 0x50, 0x4E, 0x52, 0x2F, 0x68, 0x49, 0x92, 0x80, 0x0D, 0xBD, 0x89, 0x34, 0x84, 0x60, 0x87, 0x2C, 0x50, 0x65, 0xFF, 0xAE, 0x0E, 0x7B, 0x30, 0x3D, 0xFA, 0x93, 0xE6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k128b192_CBC_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k128b192_CBC_PKCS7 ()
{
	byte[] key = { 0x14, 0x6C, 0x36, 0x5E, 0x22, 0xE9, 0x25, 0x1E, 0xC9, 0x1F, 0xA7, 0xC9, 0xA5, 0x19, 0x2C, 0x09 };
	byte[] iv = { 0xE2, 0x6F, 0xA7, 0xDC, 0x36, 0x32, 0xF7, 0x28, 0x8B, 0x09, 0x78, 0xB9, 0x30, 0x6A, 0x3F, 0xD0, 0xA8, 0x5E, 0x1F, 0x7D, 0x8F, 0xDE, 0x5B, 0xA4 };
	byte[] expected = { 0x9D, 0x08, 0xFD, 0xDE, 0x64, 0x97, 0x1D, 0x88, 0xB4, 0xCD, 0x70, 0xDD, 0xCC, 0x95, 0x1C, 0xAE, 0x01, 0x4B, 0x14, 0x19, 0x69, 0x58, 0xCE, 0x14, 0xA6, 0xF6, 0xD0, 0x25, 0xCE, 0xD6, 0xBB, 0xD5, 0x8C, 0xF6, 0xBF, 0x54, 0x66, 0x1D, 0xAE, 0x03, 0x6C, 0x81, 0xBF, 0xC6, 0x06, 0xB3, 0x64, 0x39, 0x73, 0x0A, 0x54, 0xB8, 0x3F, 0x3D, 0x1D, 0xFA, 0xB8, 0xBB, 0x53, 0x34, 0xEC, 0x69, 0xBD, 0xC3, 0xC1, 0xB2, 0x8D, 0x7D, 0x08, 0xE4, 0xFA, 0x82 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b192_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b192_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters Rijndael_k128b192_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k128b192_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k128b192_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k128b192_CFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b192_CFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b192_CFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b192_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b192_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b192_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRijndael_k128b256_ECB_None ()
{
	byte[] key = { 0xD5, 0xB9, 0x92, 0x27, 0xC0, 0xBB, 0x86, 0x06, 0x19, 0xD9, 0xA4, 0x1B, 0x9E, 0x7A, 0xF0, 0x3D };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x3C, 0x72, 0xD4, 0xBA, 0xC8, 0xCA, 0xAD, 0x8B, 0x94, 0x00, 0xF3, 0x4E, 0xE9, 0xAC, 0xFB, 0x15, 0xA2, 0x06, 0xFE, 0xA3, 0x33, 0x18, 0x48, 0x55, 0xD5, 0x6B, 0x8F, 0x13, 0xEF, 0xB6, 0x34, 0xF8 };
	byte[] expected = { 0x9A, 0x86, 0x3A, 0xE6, 0x23, 0x50, 0x4D, 0xBD, 0x4B, 0xD3, 0x1A, 0xDE, 0x83, 0x13, 0x4A, 0x82, 0xEF, 0x99, 0x7D, 0x19, 0xB0, 0x01, 0x4E, 0x46, 0x4B, 0xCF, 0x99, 0x66, 0x10, 0x23, 0x6E, 0x6C, 0x9A, 0x86, 0x3A, 0xE6, 0x23, 0x50, 0x4D, 0xBD, 0x4B, 0xD3, 0x1A, 0xDE, 0x83, 0x13, 0x4A, 0x82, 0xEF, 0x99, 0x7D, 0x19, 0xB0, 0x01, 0x4E, 0x46, 0x4B, 0xCF, 0x99, 0x66, 0x10, 0x23, 0x6E, 0x6C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b256_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k128b256_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b256_ECB_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k128b256_ECB_Zeros ()
{
	byte[] key = { 0x3C, 0xA6, 0xD7, 0xDA, 0xE3, 0x4D, 0x32, 0x67, 0xA8, 0xF5, 0xFF, 0xFF, 0xEE, 0xE8, 0xD4, 0xB2 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xC8, 0x0A, 0x40, 0x30, 0x7C, 0x7E, 0x75, 0xDE, 0x71, 0x64, 0x59, 0xCE, 0x03, 0x40, 0x8F, 0x50, 0xC7, 0x5E, 0xA2, 0x27, 0x5F, 0x12, 0x57, 0xF4, 0xB7, 0xAD, 0x95, 0xAD, 0x95, 0x84, 0xBE, 0x3C };
	byte[] expected = { 0x6D, 0x57, 0xCA, 0xED, 0x29, 0xBA, 0xA6, 0x3A, 0x3D, 0x02, 0xE1, 0x21, 0x39, 0xB0, 0x34, 0x41, 0xFC, 0xAC, 0x55, 0x8C, 0x61, 0xAE, 0x18, 0x7D, 0x7A, 0x41, 0x81, 0x1C, 0x53, 0x5F, 0x3D, 0xB1, 0x6D, 0x57, 0xCA, 0xED, 0x29, 0xBA, 0xA6, 0x3A, 0x3D, 0x02, 0xE1, 0x21, 0x39, 0xB0, 0x34, 0x41, 0xFC, 0xAC, 0x55, 0x8C, 0x61, 0xAE, 0x18, 0x7D, 0x7A, 0x41, 0x81, 0x1C, 0x53, 0x5F, 0x3D, 0xB1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k128b256_ECB_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k128b256_ECB_PKCS7 ()
{
	byte[] key = { 0xED, 0xBA, 0x84, 0x92, 0x50, 0x93, 0x9B, 0xE4, 0xC4, 0x83, 0x31, 0x8E, 0x11, 0x86, 0xAE, 0xC9 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x43, 0x98, 0x73, 0xFE, 0x77, 0x4D, 0x75, 0x79, 0xC7, 0xEF, 0x5C, 0x89, 0xFA, 0x5E, 0x07, 0x85, 0x0B, 0x21, 0x59, 0x8B, 0x8A, 0x1D, 0x11, 0x07, 0xA0, 0xC4, 0x3E, 0x11, 0x7F, 0x5D, 0xFE, 0xEE };
	byte[] expected = { 0xA0, 0x56, 0xD6, 0x6B, 0x48, 0x77, 0xCC, 0x51, 0x0F, 0x04, 0x58, 0x16, 0x46, 0x04, 0x36, 0x66, 0xBB, 0x4D, 0x88, 0x71, 0xFF, 0x65, 0x0B, 0xFD, 0x52, 0x8D, 0xE8, 0xAF, 0x97, 0x78, 0xBD, 0x82, 0xA0, 0x56, 0xD6, 0x6B, 0x48, 0x77, 0xCC, 0x51, 0x0F, 0x04, 0x58, 0x16, 0x46, 0x04, 0x36, 0x66, 0xBB, 0x4D, 0x88, 0x71, 0xFF, 0x65, 0x0B, 0xFD, 0x52, 0x8D, 0xE8, 0xAF, 0x97, 0x78, 0xBD, 0x82, 0x66, 0x2C, 0x2B, 0x59, 0xC8, 0x47, 0x3E, 0xE0, 0xC4, 0xA5, 0x22, 0x79, 0x6C, 0xCF, 0x18, 0x10, 0xDA, 0xB5, 0xE9, 0xB1, 0x21, 0xCA, 0xCC, 0xD6, 0xF7, 0xDC, 0xA5, 0xD4, 0x29, 0x10, 0x8A, 0xA4 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b256_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k128b256_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b256_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRijndael_k128b256_CBC_None ()
{
	byte[] key = { 0x23, 0x09, 0x30, 0xC7, 0x01, 0x81, 0x1D, 0x2E, 0xD6, 0x6A, 0xC9, 0x99, 0x0D, 0x3D, 0x99, 0x79 };
	byte[] iv = { 0x24, 0x2B, 0xCF, 0xFF, 0x81, 0x8C, 0xBE, 0x55, 0x1D, 0x8A, 0xDA, 0xF8, 0x81, 0xA7, 0x5A, 0xD1, 0xA6, 0x88, 0xC6, 0x90, 0xC4, 0x33, 0xCD, 0x37, 0x11, 0xCC, 0x64, 0x42, 0xD8, 0x2C, 0xA6, 0xE0 };
	byte[] expected = { 0xEF, 0xA5, 0xAB, 0xDB, 0x71, 0xE3, 0x9A, 0x33, 0x45, 0x74, 0xB7, 0x90, 0xED, 0xD8, 0xDE, 0x33, 0x56, 0xEA, 0x75, 0xE0, 0x42, 0x51, 0xAD, 0xEE, 0x9C, 0x74, 0xC8, 0x6B, 0x99, 0x88, 0xD2, 0x13, 0xB2, 0x80, 0x5E, 0xB3, 0xDC, 0xE3, 0x49, 0x43, 0x86, 0x10, 0xC7, 0xCC, 0xE2, 0xE8, 0xCD, 0x79, 0x5C, 0x69, 0x19, 0xD0, 0xE2, 0x70, 0xB1, 0x25, 0x21, 0xB5, 0xC0, 0x69, 0xAB, 0x3D, 0x25, 0x9A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b256_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b256_CBC_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k128b256_CBC_Zeros ()
{
	byte[] key = { 0xB6, 0xE5, 0xA0, 0x6F, 0x35, 0xA9, 0x25, 0x31, 0x5B, 0x8C, 0x52, 0x87, 0x26, 0x80, 0xB1, 0x42 };
	byte[] iv = { 0xFD, 0x8E, 0xD8, 0x17, 0xEB, 0x9F, 0xC6, 0x5B, 0xD7, 0x42, 0xF4, 0x79, 0x68, 0x38, 0xEE, 0xC6, 0x15, 0x83, 0xFF, 0x18, 0xA5, 0x24, 0x80, 0x65, 0xCE, 0xF3, 0xED, 0xA8, 0x0E, 0x60, 0xB4, 0xA0 };
	byte[] expected = { 0xC6, 0x0C, 0xE3, 0x6A, 0x8A, 0x98, 0xC2, 0xF7, 0x77, 0x59, 0x2C, 0x77, 0x88, 0x3F, 0xCE, 0x12, 0xFB, 0xFB, 0xB0, 0x20, 0xE5, 0xBC, 0xDB, 0x30, 0xE8, 0x1C, 0x19, 0xEA, 0x4C, 0x3A, 0x2E, 0xAF, 0x57, 0x4B, 0x05, 0xE8, 0xD4, 0xC9, 0xB2, 0xC4, 0x00, 0x35, 0xE0, 0x57, 0x7D, 0xAF, 0x11, 0xB4, 0xB2, 0x84, 0xCD, 0x7F, 0x6C, 0x6E, 0xD0, 0xDA, 0x58, 0x90, 0xF6, 0x9A, 0x51, 0x2C, 0x74, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k128b256_CBC_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k128b256_CBC_PKCS7 ()
{
	byte[] key = { 0xAE, 0x7A, 0xD9, 0x55, 0xBF, 0x55, 0xB2, 0x40, 0x4A, 0x48, 0x5F, 0x06, 0xAA, 0x04, 0x45, 0x0A };
	byte[] iv = { 0xB9, 0xD7, 0xC5, 0x09, 0x93, 0xED, 0x68, 0xC4, 0x5A, 0x82, 0x8F, 0xBD, 0x2F, 0xB4, 0x3B, 0x84, 0xBA, 0xE4, 0x46, 0x51, 0xAD, 0xAB, 0xA5, 0xCC, 0xB7, 0x59, 0x31, 0x9E, 0xBB, 0xFA, 0x54, 0x10 };
	byte[] expected = { 0xAC, 0xD7, 0x42, 0x01, 0x60, 0x36, 0xD3, 0xE1, 0xAE, 0x60, 0xC1, 0x5E, 0xAD, 0x4E, 0x81, 0xE1, 0x65, 0xFB, 0xF0, 0x06, 0x89, 0xC5, 0xAD, 0x71, 0x62, 0x81, 0x41, 0xC7, 0xC7, 0xC2, 0xAA, 0x1E, 0x76, 0x88, 0x41, 0x23, 0xFB, 0xFF, 0x44, 0x01, 0xA4, 0xB9, 0x61, 0xC0, 0x1B, 0x54, 0x09, 0x45, 0x1C, 0x17, 0xE3, 0x0A, 0x4A, 0x0A, 0xC5, 0x6F, 0x77, 0xB0, 0xDB, 0xE1, 0xD4, 0xCD, 0x28, 0xD6, 0xA6, 0x40, 0x8F, 0x2B, 0x49, 0x2C, 0xDF, 0x4D, 0x6D, 0x78, 0x24, 0x65, 0x37, 0x61, 0x05, 0xCD, 0xBC, 0x15, 0x37, 0x67, 0x65, 0xEF, 0xCB, 0x8A, 0xEE, 0x53, 0x9D, 0x29, 0x62, 0x73, 0x51, 0xD2 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k128b256_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k128b256_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters Rijndael_k128b256_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k128b256_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k128b256_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k128b256_CFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b256_CFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b256_CFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b256_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b256_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k128b256_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRijndael_k192b128_ECB_None ()
{
	byte[] key = { 0xA4, 0x51, 0x15, 0x32, 0xE7, 0xFC, 0x6F, 0x22, 0x73, 0x72, 0xB0, 0xAD, 0x67, 0x4C, 0x84, 0xB4, 0xB2, 0xAF, 0x50, 0x74, 0x5A, 0x4D, 0xB7, 0x2A };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x83, 0x22, 0x1B, 0x6C, 0x66, 0x1F, 0x4A, 0xB7, 0x55, 0xAF, 0x5B, 0xBF, 0x4A, 0x05, 0x73, 0x24 };
	byte[] expected = { 0x6A, 0x1D, 0xA5, 0xBE, 0x7F, 0x6C, 0x0A, 0x98, 0x2A, 0x09, 0x4B, 0x70, 0xC1, 0xA1, 0xBC, 0x75, 0x6A, 0x1D, 0xA5, 0xBE, 0x7F, 0x6C, 0x0A, 0x98, 0x2A, 0x09, 0x4B, 0x70, 0xC1, 0xA1, 0xBC, 0x75, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b128_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k192b128_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b128_ECB_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k192b128_ECB_Zeros ()
{
	byte[] key = { 0xB4, 0x65, 0x79, 0x30, 0x92, 0x6A, 0xEC, 0x78, 0xBA, 0x9B, 0x8B, 0x36, 0x7C, 0x8F, 0x6B, 0x8A, 0x79, 0x7F, 0x8A, 0xDA, 0xB4, 0x06, 0x23, 0x4C };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x43, 0xBA, 0x1C, 0xFB, 0x33, 0xB4, 0x3B, 0x38, 0x5C, 0x21, 0x13, 0xDD, 0x9A, 0x3A, 0xF1, 0xEE };
	byte[] expected = { 0xB1, 0x45, 0x70, 0xFC, 0xB5, 0x82, 0x49, 0x9F, 0xEA, 0x50, 0x0C, 0xEA, 0xFD, 0x13, 0xA8, 0xE8, 0xB1, 0x45, 0x70, 0xFC, 0xB5, 0x82, 0x49, 0x9F, 0xEA, 0x50, 0x0C, 0xEA, 0xFD, 0x13, 0xA8, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k192b128_ECB_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k192b128_ECB_PKCS7 ()
{
	byte[] key = { 0x06, 0xC3, 0x07, 0x6A, 0x36, 0xE5, 0xF3, 0xCF, 0x33, 0x87, 0x22, 0x03, 0x5A, 0xFA, 0x4F, 0x25, 0x9D, 0xE4, 0x81, 0xA4, 0x9E, 0xB4, 0x5D, 0x84 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xB0, 0xF9, 0x9F, 0x2D, 0x8D, 0xD0, 0x2D, 0xA1, 0x51, 0xDB, 0x07, 0xA3, 0x34, 0x28, 0x4F, 0x25 };
	byte[] expected = { 0xE9, 0xB9, 0xE5, 0x89, 0x0E, 0xF7, 0x3C, 0xCF, 0x63, 0x6B, 0xCD, 0x33, 0x85, 0x81, 0x02, 0x75, 0xE9, 0xB9, 0xE5, 0x89, 0x0E, 0xF7, 0x3C, 0xCF, 0x63, 0x6B, 0xCD, 0x33, 0x85, 0x81, 0x02, 0x75, 0xE8, 0x31, 0x03, 0x87, 0xFF, 0x9D, 0x7A, 0xAB, 0x81, 0x82, 0x63, 0x6B, 0xAA, 0x6F, 0x20, 0x21 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b128_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k192b128_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b128_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRijndael_k192b128_CBC_None ()
{
	byte[] key = { 0x8F, 0x85, 0x39, 0xC2, 0xAC, 0x25, 0xBD, 0x54, 0xDE, 0x89, 0x2A, 0x67, 0x2C, 0xF0, 0xE5, 0x7E, 0xAA, 0x7E, 0xC4, 0xFB, 0xCD, 0x31, 0xD9, 0xFA };
	byte[] iv = { 0xCA, 0xC4, 0x8D, 0x38, 0x28, 0x29, 0xC2, 0xBF, 0xD8, 0x7A, 0xCA, 0x56, 0xBF, 0x59, 0x6B, 0xCE };
	byte[] expected = { 0x22, 0x66, 0xB0, 0x6C, 0xC1, 0x18, 0xBB, 0x43, 0x6B, 0xB9, 0x42, 0x16, 0x4D, 0xFB, 0x96, 0x7C, 0xEC, 0xCA, 0xB8, 0x09, 0x02, 0x8C, 0x2E, 0x4D, 0x4D, 0x90, 0x03, 0xEA, 0x0F, 0x69, 0x20, 0xA2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b128_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b128_CBC_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k192b128_CBC_Zeros ()
{
	byte[] key = { 0xA7, 0x3E, 0xEE, 0x4B, 0xF5, 0x0E, 0x05, 0x03, 0xE2, 0x50, 0xF1, 0xBC, 0xEB, 0x57, 0x60, 0x79, 0x83, 0x5D, 0xFC, 0x42, 0x65, 0x41, 0xCF, 0x48 };
	byte[] iv = { 0xC9, 0x76, 0xCE, 0x21, 0xDF, 0x46, 0xB0, 0x23, 0x19, 0xB6, 0xD5, 0x80, 0x1F, 0xBA, 0x15, 0xDB };
	byte[] expected = { 0x63, 0xED, 0x15, 0xBE, 0xB9, 0x4E, 0x9E, 0x30, 0xB1, 0xC5, 0x31, 0xCB, 0x02, 0x88, 0xB4, 0x8F, 0xF5, 0xB0, 0x53, 0x8D, 0xD1, 0x35, 0xB7, 0x85, 0xED, 0x02, 0x79, 0x03, 0xC1, 0x13, 0xCE, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k192b128_CBC_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k192b128_CBC_PKCS7 ()
{
	byte[] key = { 0x0F, 0x00, 0x54, 0xCD, 0x2A, 0x66, 0x21, 0xF0, 0x74, 0x64, 0x65, 0xC6, 0xE1, 0xC6, 0xCD, 0x11, 0x05, 0x04, 0xA7, 0x23, 0x48, 0x4E, 0xB3, 0x84 };
	byte[] iv = { 0xDA, 0xE6, 0x7F, 0x27, 0x8A, 0xE6, 0x8E, 0x13, 0x9D, 0x15, 0x0D, 0x80, 0x4B, 0xC4, 0x9F, 0x08 };
	byte[] expected = { 0x0D, 0x7E, 0x32, 0xE0, 0xFA, 0x25, 0xB1, 0x52, 0x37, 0x27, 0xF3, 0x99, 0xA7, 0x08, 0x7F, 0x8E, 0xAA, 0x98, 0x36, 0x42, 0x21, 0xCF, 0x3B, 0xF1, 0x95, 0x99, 0xF4, 0x00, 0x36, 0x47, 0x0F, 0x25, 0x43, 0x36, 0x43, 0x68, 0x40, 0xB1, 0x1A, 0xFA, 0xDC, 0x43, 0x94, 0xD7, 0x16, 0x28, 0xFD, 0xDD };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b128_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b128_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters Rijndael_k192b128_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k192b128_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k192b128_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k192b128_CFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b128_CFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b128_CFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b128_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b128_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b128_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRijndael_k192b192_ECB_None ()
{
	byte[] key = { 0x33, 0x09, 0x20, 0xF4, 0x69, 0x76, 0x98, 0x57, 0x93, 0x1A, 0x37, 0x31, 0xFA, 0x2D, 0x49, 0xEA, 0xE4, 0xD4, 0x6C, 0xA5, 0x91, 0x2A, 0xD8, 0x54 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x7F, 0x2E, 0xE0, 0x80, 0x52, 0x2F, 0x63, 0x3F, 0x8F, 0x09, 0x85, 0x3D, 0x21, 0x73, 0x40, 0x45, 0xB0, 0x85, 0xDE, 0xB9, 0xC0, 0xA1, 0x06, 0xB2 };
	byte[] expected = { 0x93, 0x0B, 0xF0, 0xA0, 0x0C, 0x79, 0x99, 0x40, 0x17, 0x62, 0xD6, 0xD8, 0x1C, 0x3B, 0xB3, 0x18, 0x57, 0xA6, 0x01, 0x68, 0xEA, 0x73, 0x9A, 0x0A, 0x93, 0x0B, 0xF0, 0xA0, 0x0C, 0x79, 0x99, 0x40, 0x17, 0x62, 0xD6, 0xD8, 0x1C, 0x3B, 0xB3, 0x18, 0x57, 0xA6, 0x01, 0x68, 0xEA, 0x73, 0x9A, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b192_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k192b192_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b192_ECB_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k192b192_ECB_Zeros ()
{
	byte[] key = { 0xB5, 0x06, 0x72, 0x5F, 0x4E, 0x37, 0x62, 0x8F, 0x68, 0xE5, 0x0A, 0x80, 0xC6, 0x39, 0xB9, 0x13, 0xC7, 0xD8, 0x74, 0x1F, 0xE8, 0xD1, 0x99, 0x9E };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x11, 0x49, 0xA6, 0x58, 0x8F, 0xF1, 0x8E, 0xB3, 0x19, 0x81, 0xFE, 0xB8, 0x09, 0x69, 0x3D, 0x01, 0x21, 0x08, 0xCD, 0x1D, 0xEB, 0x98, 0xA7, 0xF1 };
	byte[] expected = { 0x42, 0xD5, 0xF0, 0x37, 0xFF, 0xBB, 0x81, 0xC1, 0x6F, 0x12, 0xCF, 0x65, 0x29, 0xC5, 0x88, 0xBE, 0x08, 0x88, 0xBF, 0x6F, 0xDF, 0x23, 0x82, 0x5E, 0x42, 0xD5, 0xF0, 0x37, 0xFF, 0xBB, 0x81, 0xC1, 0x6F, 0x12, 0xCF, 0x65, 0x29, 0xC5, 0x88, 0xBE, 0x08, 0x88, 0xBF, 0x6F, 0xDF, 0x23, 0x82, 0x5E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k192b192_ECB_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k192b192_ECB_PKCS7 ()
{
	byte[] key = { 0x40, 0xE3, 0xF1, 0x90, 0xC2, 0xA9, 0x59, 0xB8, 0x01, 0x72, 0x01, 0x1F, 0x10, 0x11, 0x0E, 0x8F, 0xA1, 0xF2, 0x62, 0xD7, 0x0A, 0x65, 0xCD, 0xC4 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x06, 0x08, 0x07, 0xB3, 0x8F, 0x84, 0xD9, 0xB3, 0xF9, 0x11, 0xFC, 0x0B, 0x9C, 0xC4, 0x6E, 0x41, 0xE1, 0xCC, 0x6F, 0x26, 0x6D, 0x70, 0xC6, 0x47 };
	byte[] expected = { 0xCD, 0x70, 0x93, 0x83, 0x82, 0xB1, 0xA3, 0x74, 0x8A, 0xBD, 0x0C, 0x0D, 0x8B, 0x9F, 0x3C, 0xDF, 0xBC, 0x8E, 0x64, 0x6E, 0xF7, 0xF5, 0x10, 0x0E, 0xCD, 0x70, 0x93, 0x83, 0x82, 0xB1, 0xA3, 0x74, 0x8A, 0xBD, 0x0C, 0x0D, 0x8B, 0x9F, 0x3C, 0xDF, 0xBC, 0x8E, 0x64, 0x6E, 0xF7, 0xF5, 0x10, 0x0E, 0x2D, 0xB2, 0xBD, 0xA1, 0x21, 0x56, 0xD1, 0x33, 0x00, 0x1C, 0x71, 0xAF, 0x9A, 0x48, 0x24, 0x00, 0xED, 0xA1, 0xE4, 0x2B, 0xF4, 0xF3, 0xD2, 0x5F };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b192_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k192b192_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b192_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRijndael_k192b192_CBC_None ()
{
	byte[] key = { 0x21, 0x15, 0x8D, 0x66, 0x7D, 0x81, 0xD6, 0xBD, 0xFF, 0x6D, 0x3F, 0x44, 0x43, 0x0E, 0xD7, 0x07, 0xC9, 0x5F, 0xFF, 0x0A, 0x88, 0x2D, 0xC1, 0xC4 };
	byte[] iv = { 0x43, 0x68, 0xF9, 0x7E, 0xD4, 0x6D, 0xB9, 0xA7, 0x9D, 0xFF, 0x68, 0x7F, 0x4F, 0xBB, 0x14, 0x4D, 0x29, 0x4F, 0x94, 0x8A, 0x83, 0x02, 0x77, 0x1E };
	byte[] expected = { 0x13, 0xD5, 0x9A, 0x4A, 0x96, 0x7E, 0x4F, 0x67, 0x12, 0x31, 0x9B, 0xF5, 0xC5, 0x5A, 0x81, 0xC2, 0x43, 0x51, 0x57, 0x6D, 0xA2, 0xFC, 0x5F, 0x00, 0x49, 0x5A, 0x4E, 0x82, 0x3C, 0xE0, 0x7A, 0x89, 0x2F, 0x36, 0xB3, 0x84, 0x6E, 0x9B, 0x9A, 0xAA, 0x48, 0x1B, 0x0D, 0xA1, 0x42, 0xAD, 0x6F, 0x75, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b192_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b192_CBC_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k192b192_CBC_Zeros ()
{
	byte[] key = { 0x81, 0x6F, 0xD7, 0x01, 0xCF, 0x7E, 0x73, 0x8E, 0x18, 0xB7, 0x91, 0x85, 0x70, 0x3B, 0x87, 0xCE, 0xA7, 0xB5, 0xB9, 0xFA, 0x30, 0x3D, 0x26, 0x28 };
	byte[] iv = { 0x5B, 0x34, 0x00, 0xA3, 0x3F, 0xEA, 0x2C, 0xAF, 0x87, 0xA3, 0xB9, 0x15, 0xF8, 0x61, 0x4A, 0x5C, 0x23, 0x2A, 0xF3, 0xA6, 0x7B, 0xFB, 0xEA, 0x1E };
	byte[] expected = { 0xF4, 0x87, 0x7B, 0xC8, 0x41, 0x2C, 0x8E, 0x2C, 0x58, 0x50, 0x6E, 0xE5, 0x79, 0xD1, 0xE8, 0x54, 0xE2, 0x13, 0x55, 0x91, 0x60, 0xF0, 0x35, 0x2D, 0xDB, 0x3A, 0x69, 0x92, 0x3B, 0xD1, 0x6D, 0x89, 0x57, 0x17, 0x2F, 0x31, 0xA1, 0xD9, 0xB1, 0x00, 0x41, 0x54, 0x0C, 0xFC, 0xA4, 0xE0, 0x7F, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k192b192_CBC_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k192b192_CBC_PKCS7 ()
{
	byte[] key = { 0xF4, 0x68, 0x87, 0x59, 0x32, 0x8D, 0x10, 0xA8, 0xC1, 0x32, 0xD0, 0xEC, 0xE5, 0x4A, 0x8A, 0x11, 0x3E, 0x8E, 0x11, 0x48, 0x88, 0xE9, 0xC1, 0x1A };
	byte[] iv = { 0x72, 0xD8, 0x59, 0x64, 0xD0, 0x23, 0x1E, 0x6F, 0xF9, 0x16, 0x98, 0x61, 0x09, 0xE1, 0x33, 0xE2, 0x62, 0xB7, 0x9D, 0xD2, 0xCD, 0x5B, 0x47, 0xD8 };
	byte[] expected = { 0x0B, 0x3C, 0xDD, 0x1F, 0xCA, 0x36, 0x1C, 0x44, 0x0D, 0xC6, 0xC9, 0xF8, 0xE9, 0x96, 0x33, 0x52, 0x89, 0x66, 0x73, 0x9C, 0x43, 0x27, 0x76, 0xE4, 0x84, 0x4F, 0xEF, 0x68, 0x04, 0x83, 0x68, 0x1A, 0x08, 0xA5, 0x6C, 0x22, 0x83, 0x64, 0xD5, 0x9E, 0x58, 0x00, 0x5F, 0xEB, 0x6A, 0xEF, 0x36, 0xDD, 0xD4, 0xF4, 0x21, 0x9F, 0xAB, 0x87, 0xB3, 0xD0, 0x29, 0x04, 0x19, 0x14, 0xD1, 0xD1, 0x66, 0x37, 0x54, 0xBC, 0x40, 0x43, 0xF6, 0xF1, 0x8A, 0x67 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b192_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b192_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters Rijndael_k192b192_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k192b192_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k192b192_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k192b192_CFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b192_CFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b192_CFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b192_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b192_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b192_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRijndael_k192b256_ECB_None ()
{
	byte[] key = { 0x07, 0xD5, 0xDE, 0x67, 0xAA, 0x99, 0x89, 0x35, 0x41, 0xAA, 0x04, 0x7B, 0xBB, 0x25, 0x91, 0x88, 0xDA, 0xA9, 0x5F, 0xD6, 0x05, 0xA4, 0xF4, 0x7B };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x21, 0x43, 0xAF, 0xF7, 0x20, 0x60, 0x95, 0x40, 0x42, 0x57, 0x2E, 0x1D, 0xAC, 0x95, 0x39, 0x71, 0x88, 0xDA, 0xC2, 0x22, 0xF4, 0xEA, 0xC8, 0x6F, 0x3B, 0x73, 0xBC, 0xA5, 0xC9, 0x56, 0x2B, 0x38 };
	byte[] expected = { 0xDA, 0xB8, 0xB7, 0xA7, 0x7D, 0x50, 0x08, 0x6A, 0x57, 0x3C, 0x1E, 0xA4, 0xED, 0xDD, 0x3F, 0x93, 0x99, 0x7E, 0xFC, 0x06, 0x3A, 0x9E, 0xAC, 0x82, 0x16, 0xCA, 0xE5, 0x79, 0x2C, 0xA1, 0xAC, 0x5D, 0xDA, 0xB8, 0xB7, 0xA7, 0x7D, 0x50, 0x08, 0x6A, 0x57, 0x3C, 0x1E, 0xA4, 0xED, 0xDD, 0x3F, 0x93, 0x99, 0x7E, 0xFC, 0x06, 0x3A, 0x9E, 0xAC, 0x82, 0x16, 0xCA, 0xE5, 0x79, 0x2C, 0xA1, 0xAC, 0x5D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b256_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k192b256_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b256_ECB_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k192b256_ECB_Zeros ()
{
	byte[] key = { 0xE4, 0x87, 0x99, 0x8B, 0xD1, 0x33, 0x03, 0x25, 0x1A, 0xE4, 0x10, 0x6F, 0xC7, 0x7F, 0xC2, 0xDA, 0xAC, 0x99, 0x02, 0xFF, 0x34, 0xEF, 0x10, 0xC0 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x67, 0xA7, 0x6E, 0xF5, 0xD8, 0xE2, 0xC3, 0xCB, 0x03, 0xF4, 0x6A, 0x01, 0x71, 0x8E, 0x02, 0xC7, 0x71, 0x73, 0xCF, 0x22, 0x76, 0x15, 0x87, 0x4F, 0x0D, 0x07, 0x43, 0xA6, 0x26, 0xAD, 0x15, 0xDA };
	byte[] expected = { 0xAB, 0x82, 0x14, 0x0D, 0x94, 0x36, 0x61, 0x9D, 0xF9, 0x39, 0xDA, 0x44, 0x34, 0xBA, 0x0D, 0xF5, 0xE6, 0xD2, 0x68, 0x53, 0x60, 0xC6, 0x98, 0x39, 0x4C, 0x90, 0xBE, 0xF6, 0x6E, 0xD8, 0xCB, 0xAA, 0xAB, 0x82, 0x14, 0x0D, 0x94, 0x36, 0x61, 0x9D, 0xF9, 0x39, 0xDA, 0x44, 0x34, 0xBA, 0x0D, 0xF5, 0xE6, 0xD2, 0x68, 0x53, 0x60, 0xC6, 0x98, 0x39, 0x4C, 0x90, 0xBE, 0xF6, 0x6E, 0xD8, 0xCB, 0xAA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k192b256_ECB_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k192b256_ECB_PKCS7 ()
{
	byte[] key = { 0x15, 0x40, 0x0B, 0xA3, 0xFC, 0x69, 0xF7, 0x2B, 0x55, 0x6F, 0xE9, 0x2C, 0xDA, 0xF8, 0x49, 0xAA, 0x41, 0xB3, 0x3B, 0x61, 0xCA, 0x88, 0x58, 0x19 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xCB, 0x37, 0xA1, 0x13, 0x44, 0x0D, 0x72, 0xC0, 0x8B, 0x0E, 0x62, 0xDB, 0xAF, 0x8D, 0x00, 0xC1, 0xF6, 0xF7, 0x2B, 0x60, 0x58, 0x09, 0x46, 0x95, 0x28, 0x9C, 0x87, 0x30, 0xE9, 0xA2, 0x95, 0x80 };
	byte[] expected = { 0xBE, 0x93, 0xB9, 0xEF, 0xC7, 0x57, 0x71, 0xD9, 0xFA, 0x17, 0x6F, 0x9D, 0xBE, 0x2A, 0xF2, 0xE8, 0x17, 0x39, 0x61, 0x6A, 0xEE, 0x51, 0x6D, 0x65, 0xEE, 0x27, 0x50, 0x82, 0xFB, 0x91, 0xFC, 0xDB, 0xBE, 0x93, 0xB9, 0xEF, 0xC7, 0x57, 0x71, 0xD9, 0xFA, 0x17, 0x6F, 0x9D, 0xBE, 0x2A, 0xF2, 0xE8, 0x17, 0x39, 0x61, 0x6A, 0xEE, 0x51, 0x6D, 0x65, 0xEE, 0x27, 0x50, 0x82, 0xFB, 0x91, 0xFC, 0xDB, 0x72, 0x86, 0xCA, 0xC3, 0x5C, 0x0F, 0x55, 0x79, 0x32, 0x96, 0x07, 0x86, 0xD7, 0xF3, 0x23, 0x53, 0xFC, 0x63, 0xBC, 0xD1, 0x76, 0x33, 0x7F, 0x72, 0xF1, 0x0A, 0x60, 0x7F, 0xB2, 0x6A, 0xBA, 0x0B };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b256_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k192b256_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b256_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRijndael_k192b256_CBC_None ()
{
	byte[] key = { 0x3E, 0xFE, 0x6E, 0xF9, 0x4A, 0xCE, 0x96, 0xB7, 0xDD, 0x34, 0x15, 0x20, 0x85, 0xEA, 0x4B, 0x41, 0xEC, 0xFC, 0xDD, 0x37, 0xD9, 0xF1, 0x9A, 0xE4 };
	byte[] iv = { 0x04, 0x89, 0x29, 0x3F, 0x6A, 0x54, 0xED, 0xF3, 0x8D, 0x1F, 0x62, 0xC8, 0x8C, 0x05, 0x89, 0x62, 0xC2, 0x5E, 0xDB, 0xCA, 0x60, 0xE0, 0x17, 0x03, 0xE5, 0x69, 0x6B, 0x84, 0x44, 0x2C, 0x68, 0xB0 };
	byte[] expected = { 0xA5, 0xCB, 0x68, 0xA8, 0x8A, 0xE0, 0xFD, 0x68, 0xB3, 0x75, 0x51, 0xB8, 0x46, 0x08, 0xEC, 0xE3, 0xDA, 0xE9, 0xBF, 0x49, 0x65, 0x74, 0x84, 0xB7, 0x9A, 0x60, 0x89, 0x43, 0xF2, 0x35, 0xC2, 0xAB, 0x3F, 0xD3, 0x0A, 0x9A, 0x6A, 0x3D, 0xB4, 0x2C, 0xB0, 0x8B, 0x32, 0x28, 0x2B, 0x57, 0x8F, 0x2E, 0xCF, 0x37, 0x24, 0x9B, 0xB5, 0x3B, 0xE6, 0x5E, 0xA7, 0xB9, 0x10, 0x99, 0x36, 0xA7, 0x9C, 0x92, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b256_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b256_CBC_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k192b256_CBC_Zeros ()
{
	byte[] key = { 0xB6, 0x93, 0x96, 0xA4, 0xD3, 0xE5, 0x73, 0x81, 0x17, 0x7B, 0x68, 0x92, 0x3A, 0xAF, 0x20, 0x45, 0x75, 0xBA, 0x43, 0x3C, 0x5E, 0x46, 0xF6, 0x15 };
	byte[] iv = { 0x17, 0x23, 0x3C, 0x0C, 0x51, 0xE2, 0x02, 0x8C, 0xC8, 0xD5, 0x5B, 0x00, 0x20, 0xE0, 0x2A, 0xC4, 0x4F, 0xCF, 0x4C, 0x1A, 0xCD, 0x59, 0x6C, 0x2D, 0x50, 0x8E, 0xF9, 0xA0, 0x3F, 0xFD, 0x81, 0xB5 };
	byte[] expected = { 0x93, 0xF0, 0xFC, 0x25, 0x3D, 0x6D, 0x74, 0x1F, 0x88, 0xC9, 0x9F, 0xE6, 0x3A, 0x24, 0x13, 0xE1, 0x7C, 0xEF, 0x79, 0xC6, 0x56, 0x87, 0xCB, 0xD0, 0xB7, 0x15, 0x91, 0x21, 0x7E, 0x17, 0xA2, 0xF1, 0xA6, 0xDA, 0xCA, 0xDF, 0x14, 0x88, 0x5C, 0x35, 0x13, 0x1E, 0xCD, 0x2E, 0xB0, 0xC8, 0x7E, 0x4A, 0xBE, 0xD9, 0x3B, 0x15, 0x8D, 0xC9, 0x2A, 0xC5, 0x2D, 0x7C, 0x24, 0xF3, 0xB4, 0x43, 0xDE, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k192b256_CBC_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k192b256_CBC_PKCS7 ()
{
	byte[] key = { 0x5B, 0x58, 0xA2, 0xF7, 0x12, 0x9B, 0xF1, 0x09, 0x14, 0x98, 0x6F, 0x75, 0x69, 0xF0, 0xB5, 0x02, 0xDE, 0x7E, 0xF3, 0xBF, 0x56, 0x69, 0xEC, 0x5C };
	byte[] iv = { 0x2E, 0x75, 0x1D, 0x3D, 0x2C, 0x01, 0x0B, 0x7A, 0xE6, 0x7C, 0x63, 0xB4, 0x1A, 0xF2, 0x48, 0x62, 0xF2, 0x7A, 0xF0, 0xFA, 0xC9, 0xAD, 0xFF, 0x88, 0x45, 0xE4, 0xFE, 0x5A, 0xA2, 0x87, 0x7A, 0x16 };
	byte[] expected = { 0xD2, 0x9B, 0x71, 0x41, 0xAF, 0xD2, 0x66, 0x52, 0xB1, 0x45, 0xEA, 0x7C, 0xFD, 0xF8, 0xD5, 0x13, 0xAE, 0x3E, 0xCE, 0x84, 0x5B, 0x2A, 0xBB, 0xEA, 0x11, 0xFC, 0x45, 0x98, 0x71, 0xC0, 0x2A, 0x9B, 0xD4, 0x4B, 0xDA, 0xC9, 0xED, 0x8A, 0x86, 0x0B, 0xC4, 0x53, 0x32, 0x46, 0x00, 0x59, 0x12, 0x58, 0x12, 0x8E, 0x95, 0x20, 0xA8, 0xE0, 0x96, 0xEB, 0x62, 0xAF, 0x09, 0x04, 0xE7, 0x00, 0xCE, 0x14, 0x7D, 0x62, 0xE2, 0xE8, 0x85, 0x35, 0x7B, 0x11, 0xCD, 0xA9, 0xA4, 0x48, 0x28, 0x9A, 0xA1, 0x5A, 0x3A, 0x0D, 0x24, 0x00, 0x14, 0xEE, 0x1D, 0x99, 0x46, 0x29, 0x57, 0x56, 0x12, 0x63, 0x08, 0xB1 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k192b256_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k192b256_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters Rijndael_k192b256_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k192b256_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k192b256_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k192b256_CFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b256_CFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b256_CFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b256_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b256_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k192b256_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRijndael_k256b128_ECB_None ()
{
	byte[] key = { 0x5B, 0xA0, 0xA9, 0x6B, 0x20, 0x14, 0xF4, 0x4E, 0x2E, 0x9A, 0x34, 0x84, 0xD3, 0xB9, 0x62, 0x45, 0xB1, 0x98, 0x35, 0xAE, 0xA7, 0xED, 0x80, 0x67, 0xE2, 0x77, 0xC4, 0xD5, 0x6B, 0xBD, 0x6E, 0xCF };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xF5, 0xBD, 0x6D, 0xDF, 0x0C, 0x8E, 0xC5, 0x39, 0x25, 0xBE, 0x1A, 0x80, 0xF8, 0x79, 0xEC, 0x93 };
	byte[] expected = { 0x54, 0xF5, 0x87, 0xE7, 0x73, 0xB7, 0x04, 0xBF, 0xBB, 0x16, 0x3D, 0x5A, 0xC0, 0x68, 0x7C, 0x17, 0x54, 0xF5, 0x87, 0xE7, 0x73, 0xB7, 0x04, 0xBF, 0xBB, 0x16, 0x3D, 0x5A, 0xC0, 0x68, 0x7C, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b128_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k256b128_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b128_ECB_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k256b128_ECB_Zeros ()
{
	byte[] key = { 0x77, 0xE1, 0xB2, 0xF9, 0x14, 0xF0, 0x77, 0xCE, 0xDB, 0x28, 0xD4, 0xA5, 0x0E, 0xA6, 0x73, 0x23, 0xD8, 0x46, 0xB7, 0x1A, 0x16, 0x92, 0xDB, 0x7E, 0x80, 0xDF, 0x5E, 0x9A, 0x16, 0x08, 0xFF, 0x6D };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x48, 0xEC, 0x4A, 0x12, 0xAC, 0x9C, 0xB5, 0x72, 0xEB, 0x12, 0x14, 0xFB, 0xE1, 0x6D, 0xCF, 0xA3 };
	byte[] expected = { 0x82, 0x6C, 0xC7, 0xA6, 0xC2, 0x57, 0x07, 0xF9, 0x2F, 0x92, 0x95, 0x90, 0x65, 0xFA, 0x1D, 0xFA, 0x82, 0x6C, 0xC7, 0xA6, 0xC2, 0x57, 0x07, 0xF9, 0x2F, 0x92, 0x95, 0x90, 0x65, 0xFA, 0x1D, 0xFA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k256b128_ECB_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k256b128_ECB_PKCS7 ()
{
	byte[] key = { 0x19, 0xC2, 0x2D, 0x12, 0x57, 0x2B, 0xEF, 0x0C, 0xA2, 0xC7, 0x26, 0x7E, 0x35, 0xAD, 0xC5, 0x12, 0x53, 0x5D, 0xEE, 0xD7, 0x69, 0xC3, 0xB4, 0x0D, 0x9B, 0xEF, 0x36, 0xF7, 0xB2, 0xF2, 0xB0, 0x37 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xCF, 0x8D, 0xBE, 0xE0, 0x41, 0xC6, 0xB9, 0xB5, 0x2D, 0x8A, 0x59, 0x92, 0x82, 0xF4, 0xE8, 0x74 };
	byte[] expected = { 0xAD, 0x99, 0x9A, 0xE2, 0x5B, 0xE7, 0xFB, 0x74, 0xE8, 0xAB, 0xEE, 0x5D, 0xCA, 0x0F, 0x0A, 0x7A, 0xAD, 0x99, 0x9A, 0xE2, 0x5B, 0xE7, 0xFB, 0x74, 0xE8, 0xAB, 0xEE, 0x5D, 0xCA, 0x0F, 0x0A, 0x7A, 0x8F, 0xAD, 0xBB, 0xC2, 0x18, 0xB8, 0xF0, 0xFF, 0x59, 0x7D, 0xF8, 0xF1, 0x6A, 0x21, 0x9C, 0xF3 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b128_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k256b128_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b128_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRijndael_k256b128_CBC_None ()
{
	byte[] key = { 0xE8, 0x74, 0x24, 0x77, 0x2B, 0xBE, 0x6C, 0x99, 0x2E, 0xFC, 0xB5, 0x85, 0xC9, 0xA1, 0xD7, 0x9C, 0x24, 0xF1, 0x86, 0x0B, 0xEA, 0xAB, 0xCB, 0x06, 0x47, 0x2E, 0x26, 0x6C, 0xAF, 0x24, 0x87, 0xA7 };
	byte[] iv = { 0x15, 0x7E, 0xA5, 0xE5, 0x47, 0xFA, 0x40, 0x30, 0x0A, 0xAA, 0x9E, 0x68, 0x8E, 0x4D, 0x2D, 0xA4 };
	byte[] expected = { 0xEF, 0x05, 0x1C, 0x5C, 0xEA, 0xED, 0x34, 0x28, 0x9E, 0x21, 0x9C, 0x2C, 0x96, 0xF5, 0xF7, 0xDA, 0x55, 0xD4, 0x88, 0x0A, 0x73, 0xF1, 0x8D, 0xBC, 0x8F, 0x17, 0x26, 0x86, 0x8A, 0xC1, 0x4B, 0x68, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b128_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b128_CBC_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k256b128_CBC_Zeros ()
{
	byte[] key = { 0x50, 0x54, 0x8C, 0x92, 0xE5, 0xFD, 0x08, 0x03, 0xEA, 0x15, 0xBB, 0xB9, 0x39, 0x8B, 0x6E, 0xF0, 0xF5, 0x64, 0x49, 0x0E, 0x0F, 0x8F, 0x41, 0xF9, 0xA6, 0x1E, 0xD4, 0xD2, 0xB6, 0xF2, 0xB6, 0x4B };
	byte[] iv = { 0x32, 0x9B, 0x60, 0xF7, 0xBE, 0x0F, 0x5F, 0xA5, 0xD2, 0x7A, 0x1F, 0xB4, 0x01, 0x76, 0xD1, 0xCD };
	byte[] expected = { 0x6C, 0x55, 0xAD, 0x57, 0xEE, 0x78, 0x1D, 0x69, 0x82, 0x8D, 0xE5, 0x52, 0x4C, 0x76, 0xD7, 0xF1, 0xFA, 0xFC, 0xD1, 0x2D, 0xDC, 0x0F, 0xE4, 0x4F, 0xF0, 0xE5, 0xB0, 0x2B, 0x28, 0xBF, 0x07, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k256b128_CBC_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k256b128_CBC_PKCS7 ()
{
	byte[] key = { 0x8B, 0x8B, 0x4C, 0x04, 0x8C, 0x16, 0x16, 0x91, 0xBE, 0x79, 0x35, 0xF6, 0x26, 0x01, 0xF8, 0x06, 0x8F, 0xC7, 0x6D, 0xD6, 0xFE, 0xDE, 0xCF, 0xD8, 0xDC, 0xE1, 0x97, 0x9D, 0xA9, 0xD0, 0x96, 0x86 };
	byte[] iv = { 0xA0, 0xF5, 0x25, 0xE5, 0x17, 0xEA, 0x37, 0x18, 0x17, 0x56, 0x26, 0x1C, 0x63, 0x95, 0xC3, 0xAD };
	byte[] expected = { 0x42, 0x33, 0x8E, 0xDE, 0x2E, 0xDA, 0xC9, 0xC6, 0x97, 0xA2, 0xAE, 0xE1, 0x15, 0x00, 0xDE, 0x4A, 0x39, 0x0B, 0xEB, 0xC8, 0xF9, 0x9F, 0x00, 0x05, 0xCF, 0xB5, 0x32, 0x46, 0x91, 0xFC, 0x28, 0x23, 0xF4, 0xC5, 0xCE, 0x42, 0x63, 0x3F, 0x82, 0x7D, 0x2A, 0xC4, 0xB5, 0x09, 0x67, 0xC7, 0x33, 0x3F };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 128;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b128_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b128_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters Rijndael_k256b128_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k256b128_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k256b128_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k256b128_CFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b128_CFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b128_CFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b128_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b128_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b128_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRijndael_k256b192_ECB_None ()
{
	byte[] key = { 0xE3, 0x43, 0x35, 0xDB, 0xB7, 0xC8, 0x24, 0xBF, 0x25, 0xD2, 0xA3, 0xCD, 0x70, 0xEB, 0x6B, 0xB7, 0x6D, 0x64, 0xF4, 0xB8, 0xA0, 0x56, 0x52, 0xFB, 0x3A, 0x09, 0xD4, 0xD9, 0x4F, 0x09, 0x19, 0xAF };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xDB, 0x11, 0xE4, 0x50, 0x12, 0x29, 0xC8, 0x63, 0x61, 0xEC, 0xFE, 0xD3, 0xFE, 0xA2, 0x19, 0xE0, 0xEC, 0x2F, 0x56, 0x69, 0xB7, 0x41, 0x56, 0xB0 };
	byte[] expected = { 0x66, 0xD0, 0x72, 0x3B, 0xFA, 0x3F, 0x27, 0x81, 0xB6, 0x91, 0x78, 0x7A, 0x4C, 0xD0, 0xA0, 0x4C, 0x93, 0x56, 0x51, 0xA3, 0xE0, 0x69, 0x63, 0xAF, 0x66, 0xD0, 0x72, 0x3B, 0xFA, 0x3F, 0x27, 0x81, 0xB6, 0x91, 0x78, 0x7A, 0x4C, 0xD0, 0xA0, 0x4C, 0x93, 0x56, 0x51, 0xA3, 0xE0, 0x69, 0x63, 0xAF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b192_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k256b192_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b192_ECB_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k256b192_ECB_Zeros ()
{
	byte[] key = { 0xCF, 0xAC, 0xFC, 0x30, 0x6C, 0x01, 0x16, 0x8A, 0x82, 0x52, 0x52, 0xC0, 0xC6, 0xAC, 0x1E, 0x60, 0x93, 0x17, 0x0A, 0x0C, 0x87, 0xE1, 0x4A, 0x78, 0xD9, 0xA6, 0x6B, 0xAF, 0x24, 0xF7, 0x8F, 0xED };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x99, 0x2B, 0x6B, 0x30, 0x56, 0x13, 0x2E, 0xE3, 0x3B, 0x2B, 0xC1, 0xA9, 0x4B, 0x3B, 0xD9, 0xC3, 0x7B, 0xA7, 0x4F, 0x26, 0xC9, 0x62, 0xC9, 0x66 };
	byte[] expected = { 0x22, 0x6B, 0xFA, 0x34, 0x8E, 0x09, 0xC2, 0xDF, 0xCA, 0x6C, 0xF5, 0x1F, 0xD2, 0xDC, 0x01, 0xC6, 0x3B, 0x73, 0x3F, 0x64, 0x91, 0x9F, 0xF6, 0xD3, 0x22, 0x6B, 0xFA, 0x34, 0x8E, 0x09, 0xC2, 0xDF, 0xCA, 0x6C, 0xF5, 0x1F, 0xD2, 0xDC, 0x01, 0xC6, 0x3B, 0x73, 0x3F, 0x64, 0x91, 0x9F, 0xF6, 0xD3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k256b192_ECB_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k256b192_ECB_PKCS7 ()
{
	byte[] key = { 0x17, 0xF9, 0x4A, 0x56, 0x22, 0x77, 0x20, 0x33, 0x48, 0xCB, 0x06, 0x86, 0x44, 0x02, 0xCF, 0x52, 0xDA, 0x22, 0x36, 0x07, 0xE9, 0x9F, 0x3A, 0x28, 0x3E, 0xCB, 0x49, 0x51, 0xA4, 0x67, 0x60, 0xF3 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x07, 0x77, 0x47, 0xC3, 0x49, 0x85, 0x7D, 0xB7, 0xED, 0xF3, 0x0D, 0x3F, 0x0F, 0xDC, 0xA6, 0x3E, 0x01, 0x53, 0x4D, 0x61, 0xEC, 0x06, 0xB4, 0xA0 };
	byte[] expected = { 0xA0, 0x34, 0x6F, 0xFD, 0x84, 0xA3, 0x54, 0xC0, 0x7E, 0xCC, 0x7D, 0x02, 0xE5, 0xDA, 0x79, 0x4E, 0xC6, 0xEB, 0xCE, 0x42, 0xD2, 0xBE, 0x68, 0x0F, 0xA0, 0x34, 0x6F, 0xFD, 0x84, 0xA3, 0x54, 0xC0, 0x7E, 0xCC, 0x7D, 0x02, 0xE5, 0xDA, 0x79, 0x4E, 0xC6, 0xEB, 0xCE, 0x42, 0xD2, 0xBE, 0x68, 0x0F, 0xBC, 0x22, 0x09, 0x5B, 0xFA, 0x92, 0x7E, 0xD8, 0xFF, 0x6A, 0xDD, 0x43, 0x63, 0x72, 0x23, 0xBA, 0xF9, 0xC8, 0x06, 0x3F, 0x51, 0xE8, 0x14, 0xE7 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b192_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k256b192_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b192_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRijndael_k256b192_CBC_None ()
{
	byte[] key = { 0x7A, 0x26, 0xAB, 0x32, 0x31, 0x49, 0x69, 0x3D, 0x68, 0x5A, 0xAC, 0x1B, 0x63, 0x85, 0x5A, 0x3D, 0xC4, 0xDE, 0xA8, 0x76, 0x00, 0x26, 0x78, 0x31, 0xB6, 0x30, 0xD8, 0xCB, 0x7E, 0xE7, 0xE9, 0x5B };
	byte[] iv = { 0x9D, 0x7B, 0xD5, 0x59, 0xCA, 0x42, 0xCB, 0x2F, 0x02, 0x65, 0xFE, 0x85, 0x63, 0xAE, 0x14, 0x4F, 0x69, 0xAA, 0xC2, 0xAF, 0x06, 0xF0, 0x48, 0x4F };
	byte[] expected = { 0x6C, 0x03, 0x84, 0x1C, 0x4E, 0xE0, 0x05, 0x67, 0xEA, 0x8D, 0x1C, 0x41, 0xFD, 0xC2, 0x90, 0x0E, 0xB9, 0xAA, 0xE5, 0xA0, 0x41, 0x62, 0xFE, 0xD8, 0x57, 0xA1, 0xCE, 0x33, 0x22, 0x09, 0xDB, 0x3B, 0xD7, 0x0A, 0x68, 0x61, 0x76, 0xB9, 0x8F, 0x7E, 0xE8, 0xD9, 0xA0, 0x46, 0x2B, 0x15, 0xC3, 0xF9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b192_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b192_CBC_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k256b192_CBC_Zeros ()
{
	byte[] key = { 0x35, 0x14, 0xF8, 0xDB, 0xB0, 0x84, 0x94, 0xD3, 0xDD, 0xE1, 0xB3, 0x21, 0x44, 0xE2, 0x9C, 0x65, 0x0A, 0x4A, 0x28, 0x7C, 0xD7, 0xD4, 0x9F, 0x49, 0x05, 0x23, 0x2C, 0xB2, 0x65, 0x17, 0x44, 0x2E };
	byte[] iv = { 0xD8, 0xA5, 0x77, 0x5C, 0x54, 0x79, 0x57, 0xE2, 0xBD, 0xF7, 0xD1, 0xF1, 0x6F, 0x52, 0x99, 0xBE, 0x04, 0x5E, 0x75, 0x51, 0xA6, 0x7D, 0xB9, 0x88 };
	byte[] expected = { 0xC8, 0x93, 0x1E, 0xED, 0x3F, 0x9F, 0x79, 0x34, 0x6C, 0x3F, 0x99, 0x4A, 0x25, 0xAF, 0x86, 0xDF, 0xDF, 0x19, 0x65, 0xE8, 0xAD, 0x75, 0x43, 0x1B, 0xCD, 0x1B, 0x15, 0x23, 0xC4, 0x49, 0x07, 0x31, 0x3E, 0xA2, 0x34, 0x58, 0xA0, 0x82, 0x9F, 0xF8, 0xB7, 0xB1, 0xBE, 0x59, 0xF1, 0x09, 0x5E, 0x61, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k256b192_CBC_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k256b192_CBC_PKCS7 ()
{
	byte[] key = { 0x18, 0x60, 0x4C, 0x76, 0x3D, 0x08, 0x05, 0x18, 0x66, 0xA8, 0xA5, 0x59, 0x9E, 0xB1, 0x12, 0x83, 0x70, 0x81, 0x40, 0x82, 0x09, 0xE4, 0x36, 0x41, 0xBB, 0x72, 0x53, 0xF3, 0xB6, 0x23, 0xAE, 0xB9 };
	byte[] iv = { 0xA9, 0xC1, 0x7A, 0x1D, 0xAF, 0x14, 0xFA, 0x7D, 0xEF, 0x7F, 0xDE, 0x9E, 0xE9, 0xD6, 0x1D, 0x61, 0x46, 0x2B, 0xC9, 0x24, 0x40, 0x0A, 0xE9, 0x9C };
	byte[] expected = { 0x9B, 0xE4, 0x1F, 0x94, 0xB2, 0x6B, 0x3E, 0x70, 0x69, 0x18, 0xCD, 0x65, 0xB7, 0xD9, 0xD9, 0x8E, 0xBB, 0xDA, 0xED, 0x5C, 0x84, 0xBA, 0x52, 0x4C, 0xA2, 0x66, 0xB8, 0x20, 0xEC, 0xB4, 0x16, 0xF1, 0x4C, 0xA2, 0xD0, 0x5F, 0x48, 0xDF, 0xA1, 0xDA, 0xEF, 0x75, 0xA8, 0x02, 0xCA, 0x57, 0x2E, 0x61, 0x94, 0x6A, 0x63, 0xFF, 0xBF, 0x2D, 0x44, 0x29, 0x38, 0x24, 0x50, 0x16, 0xE4, 0x41, 0x12, 0xBB, 0xF6, 0x67, 0x0A, 0xCF, 0x0A, 0xC9, 0x89, 0x55 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 192;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b192_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b192_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters Rijndael_k256b192_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k256b192_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k256b192_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k256b192_CFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b192_CFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b192_CFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b192_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b192_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b192_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestRijndael_k256b256_ECB_None ()
{
	byte[] key = { 0x04, 0x93, 0xC7, 0x1A, 0x3A, 0x62, 0x1E, 0x8B, 0x82, 0x6A, 0x20, 0x26, 0x5E, 0x29, 0x15, 0x0D, 0xCB, 0xD9, 0x49, 0x8A, 0x3E, 0x91, 0xE0, 0x8C, 0xE0, 0x9D, 0x8E, 0x15, 0x43, 0xE3, 0x1F, 0x9A };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x41, 0x3B, 0xE7, 0x01, 0x40, 0xB6, 0xB9, 0x54, 0x24, 0x38, 0x38, 0xB5, 0x8C, 0x90, 0x8D, 0x90, 0x9D, 0x68, 0xE6, 0x9C, 0x92, 0xCD, 0x95, 0x77, 0x96, 0xC6, 0xE8, 0xD5, 0xA5, 0x3E, 0xBD, 0xB9 };
	byte[] expected = { 0x2F, 0x30, 0x0F, 0xA2, 0x9C, 0x0E, 0xCA, 0x38, 0xD5, 0x43, 0xB6, 0xD4, 0xF9, 0x16, 0x65, 0xB8, 0xAA, 0x29, 0xB8, 0x16, 0xB7, 0x62, 0xE5, 0xFD, 0xC3, 0x4C, 0xA7, 0x7B, 0xC7, 0xF5, 0x5C, 0x1E, 0x2F, 0x30, 0x0F, 0xA2, 0x9C, 0x0E, 0xCA, 0x38, 0xD5, 0x43, 0xB6, 0xD4, 0xF9, 0x16, 0x65, 0xB8, 0xAA, 0x29, 0xB8, 0x16, 0xB7, 0x62, 0xE5, 0xFD, 0xC3, 0x4C, 0xA7, 0x7B, 0xC7, 0xF5, 0x5C, 0x1E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b256_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k256b256_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b256_ECB_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k256b256_ECB_Zeros ()
{
	byte[] key = { 0x52, 0x21, 0xDF, 0x3C, 0x96, 0x67, 0x86, 0x28, 0x80, 0x97, 0x12, 0xBB, 0xDD, 0x80, 0xE1, 0x04, 0xC8, 0x4B, 0x12, 0x3E, 0x28, 0x3F, 0x32, 0x38, 0xC8, 0xA0, 0x12, 0xFA, 0xFE, 0x8C, 0x0C, 0xEC };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xA9, 0x41, 0xB0, 0xE2, 0x23, 0x9A, 0x75, 0x56, 0x5F, 0x5D, 0xB8, 0x0B, 0xB1, 0xF1, 0x0F, 0xC2, 0x50, 0xBF, 0xA7, 0x3B, 0x8A, 0x26, 0xD4, 0x82, 0x33, 0xE1, 0x77, 0x84, 0xCC, 0x47, 0xCB, 0x85 };
	byte[] expected = { 0xB0, 0xC4, 0x5A, 0xDA, 0x21, 0x69, 0x9A, 0x80, 0xFC, 0xF4, 0xD1, 0xA5, 0xEE, 0x43, 0x44, 0x27, 0x4F, 0x42, 0x38, 0xFE, 0xC4, 0x2C, 0x75, 0x00, 0x60, 0x66, 0x1E, 0x86, 0xD0, 0xFC, 0x4B, 0x23, 0xB0, 0xC4, 0x5A, 0xDA, 0x21, 0x69, 0x9A, 0x80, 0xFC, 0xF4, 0xD1, 0xA5, 0xEE, 0x43, 0x44, 0x27, 0x4F, 0x42, 0x38, 0xFE, 0xC4, 0x2C, 0x75, 0x00, 0x60, 0x66, 0x1E, 0x86, 0xD0, 0xFC, 0x4B, 0x23, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k256b256_ECB_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k256b256_ECB_PKCS7 ()
{
	byte[] key = { 0xC6, 0x74, 0x58, 0xA6, 0xE0, 0xAD, 0xA2, 0x2F, 0x36, 0xC1, 0xD7, 0xAC, 0xAD, 0x8E, 0x66, 0x18, 0x8B, 0xEF, 0xBF, 0x1B, 0x75, 0xF0, 0xB0, 0x96, 0xBB, 0x07, 0xE9, 0x67, 0x25, 0x1B, 0xD0, 0x46 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x3B, 0x34, 0x5E, 0x47, 0xE3, 0x51, 0xC4, 0xE4, 0x9A, 0x66, 0xD6, 0x42, 0x1B, 0x45, 0xAB, 0x03, 0x35, 0x9A, 0x52, 0xD8, 0x1E, 0xA3, 0xC8, 0xD8, 0xBB, 0x3E, 0xD1, 0x35, 0x2C, 0x90, 0xB1, 0xC7 };
	byte[] expected = { 0x48, 0xD6, 0xD0, 0x25, 0xC7, 0x71, 0x0E, 0x10, 0xB9, 0x05, 0xE4, 0xC9, 0xEF, 0xAD, 0xB8, 0x2B, 0x14, 0xAF, 0x10, 0x53, 0x27, 0x8F, 0x32, 0x2C, 0x25, 0x9D, 0xCE, 0x64, 0x22, 0x52, 0x29, 0xCB, 0x48, 0xD6, 0xD0, 0x25, 0xC7, 0x71, 0x0E, 0x10, 0xB9, 0x05, 0xE4, 0xC9, 0xEF, 0xAD, 0xB8, 0x2B, 0x14, 0xAF, 0x10, 0x53, 0x27, 0x8F, 0x32, 0x2C, 0x25, 0x9D, 0xCE, 0x64, 0x22, 0x52, 0x29, 0xCB, 0xDF, 0x29, 0xD6, 0xDD, 0xFB, 0x89, 0x4B, 0xD7, 0x24, 0x88, 0x8E, 0x74, 0x95, 0x79, 0xBD, 0xFB, 0x80, 0xCF, 0x34, 0x7C, 0xEC, 0x2A, 0xDF, 0xBB, 0x18, 0xF6, 0xB6, 0x41, 0x00, 0xA5, 0x00, 0x55 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b256_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("Rijndael_k256b256_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b256_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestRijndael_k256b256_CBC_None ()
{
	byte[] key = { 0x2E, 0x1E, 0x55, 0x9B, 0xA8, 0x5A, 0x1D, 0x2A, 0x6B, 0x4D, 0x95, 0x8E, 0x7C, 0xFC, 0x33, 0xCE, 0x00, 0xA3, 0xFA, 0xCE, 0x9F, 0xF6, 0xED, 0x0C, 0xD5, 0x3C, 0xB0, 0xF4, 0x87, 0x26, 0x1E, 0x12 };
	byte[] iv = { 0xB2, 0xCC, 0xA6, 0x99, 0x96, 0x9C, 0xC1, 0x20, 0x2A, 0xB1, 0x00, 0x28, 0x85, 0xE1, 0xB7, 0x74, 0x66, 0x02, 0xF5, 0x69, 0xE3, 0x1F, 0xA4, 0xF4, 0xFB, 0x90, 0x3F, 0xB2, 0x7E, 0x56, 0xC9, 0x6E };
	byte[] expected = { 0x4D, 0x77, 0x53, 0xBE, 0xDB, 0xB7, 0x4D, 0x1B, 0x9B, 0x1F, 0x65, 0x7A, 0xF1, 0x8F, 0x40, 0x0D, 0x60, 0x46, 0x08, 0x8B, 0x36, 0x83, 0x91, 0x8E, 0xDC, 0x23, 0x48, 0x1F, 0x4B, 0xCB, 0x09, 0x31, 0xDB, 0x73, 0xA6, 0xF3, 0xDB, 0x98, 0x06, 0xE9, 0xFA, 0x72, 0x4F, 0xDC, 0x3A, 0xF1, 0x08, 0x7B, 0x42, 0x1E, 0xD3, 0xDB, 0x91, 0xC3, 0x2C, 0x3D, 0xD7, 0x79, 0x17, 0x2A, 0xE1, 0x3C, 0x21, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b256_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b256_CBC_None Decrypt", input, original);
}


[Test]
public void TestRijndael_k256b256_CBC_Zeros ()
{
	byte[] key = { 0xEE, 0x9F, 0xAB, 0x79, 0x11, 0x3F, 0x53, 0x56, 0x4C, 0xB4, 0xC3, 0x70, 0x29, 0x03, 0xB8, 0x26, 0x8C, 0x30, 0x2A, 0xD3, 0xF2, 0x1E, 0xA3, 0x42, 0xF4, 0xE6, 0x79, 0x5B, 0x0D, 0x93, 0xCF, 0x1B };
	byte[] iv = { 0xB0, 0x2A, 0x0F, 0x47, 0x4E, 0x47, 0xDB, 0x4A, 0xF2, 0xC7, 0xEB, 0xC3, 0xFA, 0xD3, 0x89, 0x0B, 0x46, 0x17, 0xDE, 0xB9, 0x18, 0x37, 0x6E, 0x83, 0x95, 0xD6, 0xF9, 0x25, 0xB5, 0xAC, 0x86, 0x9B };
	byte[] expected = { 0x6F, 0x0B, 0x2F, 0x3E, 0x9B, 0x07, 0xDE, 0x8B, 0xE9, 0xE7, 0xD7, 0x10, 0x09, 0xAF, 0x8E, 0x84, 0xB7, 0xBA, 0xD1, 0x79, 0x37, 0xF1, 0x25, 0xB6, 0xD7, 0xFC, 0xFB, 0x62, 0x83, 0x86, 0x8A, 0xD1, 0xC6, 0xDD, 0x98, 0x59, 0xE3, 0xEE, 0x9C, 0xA6, 0x73, 0x03, 0xE6, 0xB2, 0x72, 0xD0, 0x35, 0x39, 0xBB, 0x1C, 0x8F, 0x08, 0x8C, 0x70, 0x4C, 0x0C, 0xAD, 0xCB, 0x4F, 0x9D, 0xB7, 0x6A, 0x5F, 0xE9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	// some exception can be normal... other not so!
	try {
		Encrypt (encryptor, input, output);
	}
	catch (Exception e) {
		if (e.Message != "Input buffer contains insufficient data. ")
			Fail ("Rijndael_k256b256_CBC_Zeros: This isn't the expected exception: " + e.ToString ());
	}
}


[Test]
public void TestRijndael_k256b256_CBC_PKCS7 ()
{
	byte[] key = { 0x63, 0x95, 0x5F, 0x23, 0xFE, 0x8B, 0x49, 0x09, 0xBD, 0x05, 0x0D, 0x47, 0xCE, 0x48, 0x86, 0x02, 0x58, 0x44, 0x78, 0x21, 0x28, 0x75, 0x2E, 0x3A, 0x80, 0xE4, 0x41, 0x97, 0x0F, 0xB8, 0xA4, 0xB1 };
	byte[] iv = { 0xE1, 0xC3, 0x6B, 0x5D, 0x4F, 0x86, 0x0D, 0x44, 0xD6, 0x73, 0x21, 0x50, 0x11, 0xD3, 0x41, 0x61, 0x33, 0x04, 0x1A, 0xF8, 0x50, 0x33, 0x93, 0x4A, 0x7F, 0x9F, 0x48, 0x27, 0x8C, 0x25, 0x90, 0x93 };
	byte[] expected = { 0x1F, 0x18, 0x81, 0x2B, 0xEA, 0xE1, 0x05, 0x56, 0xF5, 0x71, 0x73, 0x8C, 0x84, 0x9C, 0x46, 0xF9, 0x18, 0xEE, 0x08, 0xB1, 0x4B, 0x96, 0xC9, 0xC9, 0x70, 0xC8, 0x3B, 0xEC, 0x15, 0x40, 0x5C, 0xA0, 0x3A, 0xD1, 0x09, 0x0C, 0xD8, 0x6F, 0xAA, 0xF5, 0x34, 0x52, 0x3A, 0x51, 0x8F, 0x3A, 0xB0, 0x3E, 0xFB, 0x31, 0x43, 0x97, 0xA3, 0x05, 0xC6, 0xF2, 0x7F, 0x2A, 0xF0, 0x4F, 0xA8, 0x64, 0xE7, 0x06, 0xFB, 0x59, 0xD3, 0xFB, 0x9E, 0x72, 0x3B, 0x11, 0xEE, 0x88, 0xEC, 0x29, 0xB2, 0x51, 0xD9, 0x58, 0x42, 0x79, 0xFC, 0x35, 0xE2, 0xF1, 0x81, 0x45, 0x8F, 0x7E, 0xE1, 0xBA, 0x95, 0xC9, 0xDD, 0x76 };

	SymmetricAlgorithm algo = Rijndael.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 256;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("Rijndael_k256b256_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("Rijndael_k256b256_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters Rijndael_k256b256_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k256b256_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k256b256_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters Rijndael_k256b256_CFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b256_CFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b256_CFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b256_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b256_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters Rijndael_k256b256_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestTripleDES_k128b64_ECB_None ()
{
	byte[] key = { 0x31, 0x29, 0x5A, 0x2D, 0x18, 0xDF, 0x78, 0xB1, 0xB3, 0x30, 0xB4, 0x2E, 0x08, 0x2A, 0xB5, 0x00 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xDE, 0x87, 0xFF, 0xA6, 0x30, 0x76, 0x39, 0x89 };
	byte[] expected = { 0x74, 0xD2, 0x61, 0x01, 0xF0, 0x86, 0x74, 0xE8, 0x74, 0xD2, 0x61, 0x01, 0xF0, 0x86, 0x74, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k128b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("TripleDES_k128b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k128b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestTripleDES_k128b64_ECB_Zeros ()
{
	byte[] key = { 0xFB, 0xC1, 0xA8, 0x04, 0x47, 0x10, 0x09, 0x09, 0xA8, 0x3D, 0x97, 0x18, 0x11, 0x3C, 0x28, 0x80 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xA2, 0x1F, 0x63, 0x49, 0x33, 0xCA, 0xEE, 0xDA };
	byte[] expected = { 0xDB, 0x4E, 0x92, 0x3D, 0xE3, 0x26, 0x0B, 0x16, 0xDB, 0x4E, 0x92, 0x3D, 0xE3, 0x26, 0x0B, 0x16, 0xDB, 0x4E, 0x92, 0x3D, 0xE3, 0x26, 0x0B, 0x16 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k128b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("TripleDES_k128b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("TripleDES_k128b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k128b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestTripleDES_k128b64_ECB_PKCS7 ()
{
	byte[] key = { 0x78, 0x52, 0xAE, 0x73, 0x24, 0x0A, 0xDF, 0x80, 0x1A, 0xDE, 0x32, 0x90, 0x3C, 0x01, 0xBA, 0x12 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xF6, 0x11, 0x79, 0x5E, 0xEC, 0xDC, 0x5E, 0x19 };
	byte[] expected = { 0x83, 0xDE, 0x8A, 0xDA, 0x7A, 0x46, 0xDC, 0x07, 0x83, 0xDE, 0x8A, 0xDA, 0x7A, 0x46, 0xDC, 0x07, 0x4B, 0x79, 0x8C, 0x46, 0x0A, 0xB7, 0x40, 0x6C };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k128b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("TripleDES_k128b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k128b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestTripleDES_k128b64_CBC_None ()
{
	byte[] key = { 0x9B, 0x97, 0x95, 0xA2, 0x6D, 0x90, 0x1D, 0xAE, 0xE8, 0xFC, 0xA1, 0xA2, 0x06, 0x6E, 0x75, 0xE8 };
	byte[] iv = { 0x52, 0xF8, 0x0E, 0xA9, 0x8C, 0xD9, 0x46, 0x63 };
	byte[] expected = { 0xD3, 0x37, 0x2D, 0x9B, 0x69, 0x35, 0xB7, 0x80, 0xD1, 0x13, 0xBB, 0xEB, 0x47, 0xB6, 0xDA, 0xF2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k128b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k128b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestTripleDES_k128b64_CBC_Zeros ()
{
	byte[] key = { 0x21, 0x87, 0x57, 0xF4, 0xE5, 0xE9, 0x91, 0xC7, 0x3A, 0x64, 0x14, 0xF2, 0x2B, 0x06, 0x0E, 0x2E };
	byte[] iv = { 0x23, 0x86, 0x58, 0x7B, 0x49, 0x23, 0xF6, 0x7F };
	byte[] expected = { 0xEF, 0x1B, 0x0B, 0xDD, 0xD0, 0x07, 0x5E, 0x22, 0x9D, 0xB9, 0xCC, 0x52, 0xB4, 0xD9, 0x88, 0x1F, 0x5D, 0xE3, 0x51, 0x51, 0xBF, 0x7C, 0xB5, 0xB3 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k128b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k128b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestTripleDES_k128b64_CBC_PKCS7 ()
{
	byte[] key = { 0x06, 0x33, 0x4B, 0x5A, 0xF0, 0xC6, 0xAE, 0x71, 0x8C, 0x41, 0xB3, 0x72, 0x43, 0x4B, 0x82, 0x31 };
	byte[] iv = { 0x40, 0x7F, 0x60, 0x5B, 0x5C, 0x22, 0x8D, 0x5D };
	byte[] expected = { 0x9C, 0x3F, 0x6A, 0x1D, 0xBD, 0x92, 0x1A, 0xFA, 0xD4, 0xA5, 0xEA, 0xB3, 0x77, 0xA0, 0x8B, 0xB0, 0x7E, 0x11, 0xFA, 0xA9, 0x45, 0x46, 0x16, 0x33 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k128b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k128b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters TripleDES_k128b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters TripleDES_k128b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters TripleDES_k128b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestTripleDES_k128b64_CFB8_None ()
{
	byte[] key = { 0x49, 0x9D, 0x94, 0x9C, 0x79, 0xD9, 0xEE, 0x92, 0x75, 0xE8, 0x8C, 0x78, 0xE3, 0xB5, 0x49, 0x81 };
	byte[] iv = { 0x80, 0x0A, 0x45, 0x55, 0xCB, 0xC7, 0x17, 0xA1 };
	byte[] expected = { 0xA5, 0x0F, 0xFF, 0xE6, 0xA0, 0x59, 0x58, 0x81, 0xB0, 0xFE, 0x19, 0x40, 0xF4, 0x04, 0x0B, 0xE7, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k128b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k128b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestTripleDES_k128b64_CFB8_Zeros ()
{
	byte[] key = { 0x47, 0xD4, 0x00, 0xC6, 0x0B, 0xCE, 0x0D, 0x6B, 0xD6, 0xEB, 0xBF, 0x74, 0xE3, 0xB9, 0x61, 0x14 };
	byte[] iv = { 0x63, 0xB1, 0xCE, 0xEF, 0x06, 0x14, 0xD6, 0x4B };
	byte[] expected = { 0x02, 0xB8, 0xB8, 0x49, 0xA8, 0x3B, 0x6B, 0x05, 0x74, 0x79, 0x91, 0xFE, 0x7B, 0x74, 0x0A, 0xF8, 0x95, 0x80, 0x5A, 0xF1, 0xE9, 0xD7, 0xD3, 0x32 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k128b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k128b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestTripleDES_k128b64_CFB8_PKCS7 ()
{
	byte[] key = { 0x70, 0x9E, 0x39, 0x1A, 0x45, 0xA4, 0x18, 0x30, 0xAC, 0xE6, 0x1E, 0x0E, 0xD7, 0x43, 0x39, 0x5F };
	byte[] iv = { 0x26, 0xF3, 0x46, 0x6A, 0x35, 0xC8, 0xBF, 0x03 };
	byte[] expected = { 0x88, 0x21, 0x01, 0x82, 0x88, 0x2E, 0x93, 0xC5, 0xCD, 0xA2, 0xC9, 0x38, 0x45, 0x68, 0x91, 0x82, 0xA5, 0x78, 0x6B, 0x08, 0x3F, 0x7C, 0xB8, 0x5F };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k128b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k128b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters TripleDES_k128b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters TripleDES_k128b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters TripleDES_k128b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */

[Test]
public void TestTripleDES_k192b64_ECB_None ()
{
	byte[] key = { 0x02, 0xFE, 0x15, 0x59, 0xD7, 0xE9, 0xB5, 0x2A, 0xA7, 0x9B, 0xB3, 0xA6, 0xFA, 0xAA, 0xC7, 0x97, 0xD4, 0x1B, 0xE4, 0x2D, 0xE4, 0xC5, 0x89, 0xC2 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0x13, 0xBF, 0xF3, 0xA0, 0xD3, 0xA1, 0x2F, 0x23 };
	byte[] expected = { 0xC8, 0x09, 0x6E, 0xD6, 0xC8, 0xD8, 0xF3, 0x6A, 0xC8, 0x09, 0x6E, 0xD6, 0xC8, 0xD8, 0xF3, 0x6A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k192b64_ECB_None Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("TripleDES_k192b64_ECB_None b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k192b64_ECB_None Decrypt", input, original);
}


[Test]
public void TestTripleDES_k192b64_ECB_Zeros ()
{
	byte[] key = { 0x0B, 0xB5, 0x02, 0xE8, 0xC3, 0x2E, 0x24, 0xD9, 0xF0, 0x29, 0x15, 0x10, 0x19, 0x88, 0xFC, 0xD2, 0x60, 0xCA, 0x30, 0x51, 0x0D, 0xD6, 0x80, 0xAC };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xF6, 0xC5, 0xBD, 0xA2, 0x4D, 0xA8, 0x19, 0x78 };
	byte[] expected = { 0xE0, 0x52, 0xCB, 0xC6, 0xBB, 0x43, 0x8F, 0x3B, 0xE0, 0x52, 0xCB, 0xC6, 0xBB, 0x43, 0x8F, 0x3B, 0xE0, 0x52, 0xCB, 0xC6, 0xBB, 0x43, 0x8F, 0x3B };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k192b64_ECB_Zeros Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("TripleDES_k192b64_ECB_Zeros b1==b2", block1, block2);

	// also if padding is Zeros then all three blocks should be equals
	byte[] block3 = new byte[blockLength];
	Array.Copy (output, blockLength, block3, 0, blockLength);
	AssertEquals ("TripleDES_k192b64_ECB_Zeros b1==b3", block1, block3);

	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k192b64_ECB_Zeros Decrypt", input, original);
}


[Test]
public void TestTripleDES_k192b64_ECB_PKCS7 ()
{
	byte[] key = { 0x41, 0xAD, 0x00, 0xE4, 0x53, 0x0A, 0x09, 0x8C, 0x1F, 0x86, 0x91, 0x46, 0x41, 0xEC, 0xE3, 0x70, 0x35, 0xE5, 0x65, 0x10, 0x0D, 0x38, 0x4F, 0xE3 };
	// not used for ECB but make the code more uniform
	byte[] iv = { 0xB0, 0x71, 0x70, 0xFC, 0x57, 0xC2, 0x26, 0xF9 };
	byte[] expected = { 0xA3, 0xB3, 0x91, 0x00, 0x99, 0x7A, 0x15, 0xB4, 0xA3, 0xB3, 0x91, 0x00, 0x99, 0x7A, 0x15, 0xB4, 0x53, 0x35, 0xE6, 0x2D, 0x0D, 0xD1, 0x16, 0xE6 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.ECB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k192b64_ECB_PKCS7 Encrypt", expected, output);

	// in ECB the first 2 blocks should be equals (as the IV is not used)
	byte[] block1 = new byte[blockLength];
	Array.Copy (output, 0, block1, 0, blockLength);
	byte[] block2 = new byte[blockLength];
	Array.Copy (output, blockLength, block2, 0, blockLength);
	AssertEquals ("TripleDES_k192b64_ECB_PKCS7 b1==b2", block1, block2);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k192b64_ECB_PKCS7 Decrypt", input, original);
}


[Test]
public void TestTripleDES_k192b64_CBC_None ()
{
	byte[] key = { 0xA5, 0xA5, 0x3B, 0x8E, 0x59, 0x5B, 0xDD, 0xEC, 0x15, 0x22, 0x95, 0x53, 0xCB, 0xEC, 0xE3, 0x63, 0x78, 0x25, 0xF5, 0xE5, 0x52, 0xAD, 0x50, 0x1A };
	byte[] iv = { 0xBD, 0x69, 0xAC, 0xA6, 0xCF, 0x17, 0xFC, 0x8A };
	byte[] expected = { 0xA6, 0xA8, 0x8E, 0x09, 0xCF, 0xD2, 0x66, 0x4A, 0x20, 0xE8, 0xC3, 0x56, 0x8F, 0x2F, 0x42, 0x75, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k192b64_CBC_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k192b64_CBC_None Decrypt", input, original);
}


[Test]
public void TestTripleDES_k192b64_CBC_Zeros ()
{
	byte[] key = { 0x40, 0x3D, 0xEC, 0xE5, 0xB4, 0x2A, 0x4B, 0x5E, 0x81, 0x88, 0x3A, 0x53, 0x3F, 0xFD, 0xE7, 0x55, 0x50, 0x21, 0xAA, 0x0A, 0xB4, 0x3B, 0x26, 0xC0 };
	byte[] iv = { 0x09, 0x50, 0xF5, 0x6F, 0x18, 0xD1, 0x4C, 0x9E };
	byte[] expected = { 0x85, 0xFA, 0xBF, 0x39, 0x5C, 0x17, 0x13, 0xF1, 0x27, 0x47, 0x17, 0x97, 0xBA, 0xCD, 0x69, 0x8E, 0x0D, 0x7D, 0xC5, 0xE2, 0x8F, 0xDF, 0xFC, 0x2B };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k192b64_CBC_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k192b64_CBC_Zeros Decrypt", input, original);
}


[Test]
public void TestTripleDES_k192b64_CBC_PKCS7 ()
{
	byte[] key = { 0x31, 0x9E, 0x55, 0x57, 0x3F, 0x77, 0xBC, 0x27, 0x79, 0x45, 0x7E, 0xAA, 0x4F, 0xF1, 0x2E, 0xBB, 0x98, 0xAE, 0xFD, 0xBE, 0x22, 0xB8, 0x69, 0xD9 };
	byte[] iv = { 0xF7, 0xD8, 0x8E, 0xB2, 0xC5, 0x5F, 0x49, 0x91 };
	byte[] expected = { 0x0D, 0xB8, 0xC7, 0x8F, 0x89, 0x26, 0x42, 0x50, 0x5E, 0x3A, 0x3B, 0x4D, 0xC8, 0x0E, 0x7E, 0x0F, 0xDA, 0x79, 0x37, 0x89, 0x2A, 0xF6, 0x10, 0x76 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CBC;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k192b64_CBC_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k192b64_CBC_PKCS7 Decrypt", input, original);
}


/* Invalid parameters TripleDES_k192b64_CTS_None. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters TripleDES_k192b64_CTS_Zeros. Why? Specified cipher mode is not valid for this algorithm. */

/* Invalid parameters TripleDES_k192b64_CTS_PKCS7. Why? Specified cipher mode is not valid for this algorithm. */

[Test]
public void TestTripleDES_k192b64_CFB8_None ()
{
	byte[] key = { 0x6C, 0x11, 0xA9, 0xC8, 0x04, 0xB3, 0x74, 0x8A, 0xA0, 0xC7, 0x43, 0x9A, 0x1F, 0x4C, 0x79, 0x08, 0x4D, 0xB4, 0x7B, 0xAC, 0xA2, 0xF8, 0x2C, 0x22 };
	byte[] iv = { 0x2E, 0xF8, 0x02, 0x62, 0x15, 0xE2, 0x8F, 0xB1 };
	byte[] expected = { 0x95, 0x55, 0x48, 0xF1, 0x6D, 0x6F, 0x36, 0x25, 0xAE, 0x02, 0x0B, 0x6E, 0xC3, 0x04, 0xC5, 0x93, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.None;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k192b64_CFB8_None Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k192b64_CFB8_None Decrypt", input, original);
}


[Test]
public void TestTripleDES_k192b64_CFB8_Zeros ()
{
	byte[] key = { 0x34, 0x38, 0x7F, 0x40, 0xBA, 0x64, 0x88, 0xAC, 0x50, 0xE5, 0x0D, 0x9D, 0xC4, 0x0B, 0xDF, 0xE8, 0xB7, 0xCB, 0x9D, 0x38, 0xFD, 0x4E, 0x17, 0xDA };
	byte[] iv = { 0xC0, 0x32, 0xAE, 0xA8, 0xEB, 0x67, 0x74, 0xC4 };
	byte[] expected = { 0x8A, 0xE3, 0xAD, 0x43, 0x06, 0xAC, 0xC7, 0xE7, 0xCC, 0x03, 0xCE, 0xB1, 0x8F, 0x9F, 0x7A, 0x9E, 0xEB, 0x05, 0x74, 0x04, 0xF4, 0xFD, 0x76, 0x51 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.Zeros;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k192b64_CFB8_Zeros Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k192b64_CFB8_Zeros Decrypt", input, original);
}


[Test]
public void TestTripleDES_k192b64_CFB8_PKCS7 ()
{
	byte[] key = { 0xBC, 0x48, 0x95, 0x9F, 0x13, 0xFF, 0xCB, 0x33, 0x6D, 0xA5, 0x84, 0x93, 0x33, 0x54, 0xAD, 0xF4, 0x5F, 0x99, 0xA3, 0x0F, 0x0E, 0x91, 0x88, 0x0E };
	byte[] iv = { 0x0E, 0xC5, 0xA8, 0xB2, 0xDD, 0x83, 0xAE, 0x8C };
	byte[] expected = { 0xB5, 0x72, 0x20, 0x82, 0x45, 0x70, 0x83, 0xE5, 0xF0, 0xA6, 0xFC, 0xFC, 0xB6, 0xF4, 0x7D, 0x3B, 0x71, 0x94, 0x2A, 0x9F, 0x01, 0x46, 0x90, 0x56 };

	SymmetricAlgorithm algo = TripleDES.Create ();
	algo.Mode = CipherMode.CFB;
	algo.Padding = PaddingMode.PKCS7;
	algo.BlockSize = 64;
	algo.FeedbackSize = 8;
	int blockLength = (algo.BlockSize >> 3);
	byte[] input = new byte [blockLength * 2 + (blockLength >> 1)];
	byte[] output = new byte [blockLength * 3];
	ICryptoTransform encryptor = algo.CreateEncryptor(key, iv);
	Encrypt (encryptor, input, output);
	AssertEquals ("TripleDES_k192b64_CFB8_PKCS7 Encrypt", expected, output);
	byte[] reverse = new byte [blockLength * 3];
	ICryptoTransform decryptor = algo.CreateDecryptor(key, iv);
	Decrypt (decryptor, output, reverse);
	byte[] original = new byte [input.Length];
	Array.Copy (reverse, 0, original, 0, original.Length);
	AssertEquals ("TripleDES_k192b64_CFB8_PKCS7 Decrypt", input, original);
}


/* Invalid parameters TripleDES_k192b64_OFB8_None. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters TripleDES_k192b64_OFB8_Zeros. Why? Output feedback mode (OFB) is not supported by this implementation. */

/* Invalid parameters TripleDES_k192b64_OFB8_PKCS7. Why? Output feedback mode (OFB) is not supported by this implementation. */


// Number of test cases: 189
// Number of invalid (non-generated) test cases: 171
}
}

