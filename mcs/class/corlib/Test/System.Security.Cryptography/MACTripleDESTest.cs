//
// MACTripleDESTest.cs - NUnit Test Cases for MACTripleDES
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

[TestFixture]
public class MACTripleDESTest {

	protected MACTripleDES algo;

	private static byte[] key1 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
	private static byte[] key2 = { 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0x01 };
	private static byte[] key3 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };

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

	[Test]
	public void ConstructorEmpty () 
	{
		algo = new MACTripleDES ();
	}

	[Test]
	public void ConstructorKey () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		algo = new MACTripleDES (key);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void ConstructorKeyNull () 
	{
		algo = new MACTripleDES (null);
	}

	[Test]
	public void ConstructorNameKey () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		algo = new MACTripleDES ("TripleDES", key);
	}

	[Test]
	// LAMESPEC: [ExpectedException (typeof (ArgumentNullException))]
	public void ConstructorNameNullKey () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		// funny null is a valid name!
		algo = new MACTripleDES (null, key);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void ConstructorNameNullKeyNull () 
	{
		algo = new MACTripleDES (null, null);
	}

	[Test]
	public void Invariants () 
	{
		algo = new MACTripleDES ();
		Assert.IsTrue (algo.CanReuseTransform, "MACTripleDES.CanReuseTransform");
		Assert.IsTrue (algo.CanTransformMultipleBlocks, "MACTripleDES.CanTransformMultipleBlocks");
		Assert.AreEqual (64, algo.HashSize, "MACTripleDES.HashSize");
		Assert.AreEqual (1, algo.InputBlockSize, "MACTripleDES.InputBlockSize");
		Assert.AreEqual (1, algo.OutputBlockSize, "MACTripleDES.OutputBlockSize");
		Assert.AreEqual ("System.Security.Cryptography.MACTripleDES", algo.ToString (), "MACTripleDES.ToString()");
		Assert.IsNotNull (algo.Key, "MACTripleDES.Key");
	}

	[Test]
	[ExpectedException (typeof (InvalidCastException))]
	public void InvalidAlgorithmName () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		algo = new MACTripleDES ("DES", key);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void ObjectDisposed () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		algo = new MACTripleDES (key);
		algo.Clear ();
		algo.ComputeHash (new byte[1]);
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
	[Test]
	public void SmallerThanOneBlockSize () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		byte[] expected = { 0x86, 0xE9, 0x65, 0xBD, 0x1E, 0xC4, 0x44, 0x61 };
		byte[] data = new byte [7];
		Check ("3DESMAC-A1", key, data, expected);
	}

	// Here data is exactly one 3DES block size (8 bytes)
	[Test]
	public void ExactlyOneBlockSize () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		byte [] expected = { 0x86, 0xE9, 0x65, 0xBD, 0x1E, 0xC4, 0x44, 0x61 };
		byte[] data = new byte [8];
		Check ("3DESMAC-A2", key, data, expected);
	}

	// Here data is more then one 3DES block size (8 bytes)
	[Test]
	public void MoreThanOneBlockSize () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		// note: same result as A2 because of the Zero padding (and that
		// we use zeros as data
		byte[] expected = { 0x23, 0xD6, 0x92, 0xA0, 0x80, 0x6E, 0xC9, 0x30 };
		byte[] data = new byte [14];
		Check ("3DESMAC-A3", key, data, expected);
	}

	// Here data is a multiple of 3DES block size (8 bytes)
	[Test]
	public void ExactMultipleBlockSize () 
	{
		byte[] key = CombineKeys (key1, key2, key3);
		byte [] expected = { 0xA3, 0x0E, 0x34, 0x26, 0x8B, 0x49, 0xEF, 0x49 };
		byte[] data = new byte [48];
		Check ("3DESMAC-A4", key, data, expected);
	}
}

}