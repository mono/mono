//
// MACTripleDESTest.cs - NUnit Test Cases for MACTripleDES
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

public class MACTripleDESTest : TestCase {

	protected MACTripleDES algo;

	private static byte[] key1 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
	private static byte[] key2 = { 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0x01 };
	private static byte[] key3 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };

	protected override void SetUp () {}

	protected override void TearDown () {}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	protected byte[] CombineKeys (byte[] key1, byte[] key2, byte[] key3) 
	{
		int k1l = key1.Length;
		int k2l = key2.Length;
		int k3l = key3.Length;
		byte[] key = new byte [k1l + k2l + k3l];
		Array.Copy (key1, 0, key, 0, k1l);
		Array.Copy (key2, 0, key, k1l, k2l);
		Array.Copy (key3, 0, key, k1l + k2l, k3l);
		return key;
	}

	public void TestConstructors () 
	{
		algo = new MACTripleDES ();
		AssertNotNull ("MACTripleDES ()", algo);

		byte[] key = new byte [24];
		key [0] = 1;  // so this isn't a weak key
		key [23] = 1;
		algo = new MACTripleDES (key);
		AssertNotNull ("MACTripleDES (key)", algo);

		try {
			algo = new MACTripleDES (null);
			Fail ("MACTripleDES (null) - Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("MACTripleDES (null) - Expected ArgumentNullException but got: " + e.ToString ());
		}

		algo = new MACTripleDES ("TripleDES", key);
		AssertNotNull ("MACTripleDES ('TripleDES',key)", algo);

		try {
			algo = new MACTripleDES ("TripleDES", null);
			Fail ("MACTripleDES ('TripleDES', null) - Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("MACTripleDES ('TripleDES', null) - Expected ArgumentNullException but got: " + e.ToString ());
		}

		// funny null is a valid name!
		algo = new MACTripleDES (null, key);
		AssertNotNull ("MACTripleDES (null,key)", algo);

		try {
			algo = new MACTripleDES (null, null);
			Fail ("MACTripleDES (null, null) - Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("MACTripleDES (null, null) - Expected ArgumentNullException but got: " + e.ToString ());
		}
	}

	public void TestInvariants () 
	{
		algo = new MACTripleDES ();
		AssertEquals ("MACTripleDES.CanReuseTransform", true, algo.CanReuseTransform);
		AssertEquals ("MACTripleDES.CanTransformMultipleBlocks", true, algo.CanTransformMultipleBlocks);
		AssertEquals ("MACTripleDES.HashSize", 64, algo.HashSize);
		AssertEquals ("MACTripleDES.InputBlockSize", 1, algo.InputBlockSize);
		AssertEquals ("MACTripleDES.OutputBlockSize", 1, algo.OutputBlockSize);
		AssertEquals ("MACTripleDES.ToString()", "System.Security.Cryptography.MACTripleDES", algo.ToString ()); 
		AssertNotNull ("MACTripleDES.Key", algo.Key);
	}

	public void TestExceptions () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		try {
			algo = new MACTripleDES ("DES", key);
			Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected InvalidCastException but got " + e.ToString ());
		}

		algo = new MACTripleDES (key);
		algo.Clear ();
		try {
			algo.ComputeHash (new byte[1]);
			Fail ("Expected ObjectDisposedException but got none");
		}
		catch (ObjectDisposedException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected ObjectDisposedException but got " + e.ToString ());
		}
	}

	public void Check (string testName, byte[] key, byte[] data, byte[] result) 
	{
		string classTestName = "MACTripleDES-" + testName;
		CheckA (testName, key, data, result);
		CheckB (testName, key, data, result);
		CheckC (testName, key, data, result);
		CheckD (testName, key, data, result);
		CheckE (testName, key, data, result);
	}

	// - Test constructor #1 ()
	// - Test ComputeHash (byte[]);
	public void CheckA (string testName, byte[] key, byte[] data, byte[] result) 
	{
		algo = new MACTripleDES ();
		algo.Key = key;
		byte[] hmac = algo.ComputeHash (data);
		AssertEquals (testName + "a1", result, hmac);
		AssertEquals (testName + "a2", result, algo.Hash);
	}

	// - Test constructor #2 (byte[])
	// - Test ComputeHash (byte[], int, int);
	public void CheckB (string testName, byte[] key, byte[] data, byte[] result) 
	{
		algo = new MACTripleDES (key);
		byte[] hmac = algo.ComputeHash (data, 0, data.Length);
		AssertEquals (testName + "b1", result, hmac);
		AssertEquals (testName + "b2", result, algo.Hash);
	}
	
	// - Test constructor #3 (string, byte[])
	// - Test ComputeHash (stream);
	public void CheckC (string testName, byte[] key, byte[] data, byte[] result) 
	{
		algo = new MACTripleDES ("TripleDES", key);
		algo.Key = key;
		MemoryStream ms = new MemoryStream (data);
		byte[] hmac = algo.ComputeHash (ms);
		AssertEquals (testName + "c1", result, hmac);
		AssertEquals (testName + "c2", result, algo.Hash);
	}

	// - Test TransformFinalBlock - alone;
	public void CheckD (string testName, byte[] key, byte[] data, byte[] result) 
	{
		algo = new MACTripleDES ();
		algo.Key = key;
		// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
		algo.TransformFinalBlock (data, 0, data.Length);
		AssertEquals (testName + "d", result, algo.Hash);
	}

	// - Test TransformBlock/TransformFinalBlock
	public void CheckE (string testName, byte[] key, byte[] data, byte[] result) 
	{
		algo = new MACTripleDES ();
		algo.Key = key;
		byte[] copy = new byte [data.Length];
		// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
		for (int i=0; i < data.Length - 1; i++)
			algo.TransformBlock (data, i, 1, copy, i);
		algo.TransformFinalBlock (data, data.Length - 1, 1);
		AssertEquals (testName + "e", result, algo.Hash);
	}

	// Here data is smaller than the 3DES block size (8 bytes)
	public void Test_A1 () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		byte[] expected = { 0x86, 0xE9, 0x65, 0xBD, 0x1E, 0xC4, 0x44, 0x61 };
		byte[] data = new byte [7];
		Check ("3DESMAC-A1", key, data, expected);
	}

	// Here data is exactly one 3DES block size (8 bytes)
#if NET_1_1
	[Ignore("Believe it or not, MACTripleDES returns different values in 1.1")]
#endif
	public void Test_A2 () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		byte[] expected = { 0x23, 0xD6, 0x92, 0xA0, 0x80, 0x6E, 0xC9, 0x30 };
		byte[] data = new byte [8];
		Check ("3DESMAC-A2", key, data, expected);
	}

	// Here data is more then one 3DES block size (8 bytes)
	public void Test_A3 () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		// note: same result as A2 because of the Zero padding (and that
		// we use zeros as data
		byte[] expected = { 0x23, 0xD6, 0x92, 0xA0, 0x80, 0x6E, 0xC9, 0x30 };
		byte[] data = new byte [14];
		Check ("3DESMAC-A3", key, data, expected);
	}

	// Here data is a multiple of 3DES block size (8 bytes)
#if NET_1_1
	[Ignore("Believe it or not, MACTripleDES returns different values in 1.1")]
#endif
	public void Test_A4 () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		byte[] expected = { 0xD6, 0x6D, 0x75, 0xD4, 0x75, 0xF1, 0x01, 0x71 };
		byte[] data = new byte [48];
		Check ("3DESMAC-A4", key, data, expected);
	}
}

}