//
// HashAlgorithmTest.cs - NUnit Test Cases for HashAlgorithm
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.Text;

namespace MonoTests.System.Security.Cryptography {

// HashAlgorithm is a abstract class - so most of it's functionality wont
// be tested here (but will be in its descendants).

[TestFixture]
public class HashAlgorithmTest : Assertion {
	protected HashAlgorithm hash;

	[SetUp]
	protected virtual void SetUp () 
	{
		hash = HashAlgorithm.Create ();
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	// Note: These tests will only be valid without a "machine.config" file
	// or a "machine.config" file that do not modify the default algorithm
	// configuration.
	private const string defaultSHA1 = "System.Security.Cryptography.SHA1CryptoServiceProvider";
	private const string defaultMD5 = "System.Security.Cryptography.MD5CryptoServiceProvider";
	private const string defaultSHA256 = "System.Security.Cryptography.SHA256Managed";
	private const string defaultSHA384 = "System.Security.Cryptography.SHA384Managed";
	private const string defaultSHA512 = "System.Security.Cryptography.SHA512Managed";
	private const string defaultHash = defaultSHA1;

	[Test]
	public virtual void Create () 
	{
		// try the default hash algorithm (created in SetUp)
		AssertEquals( "HashAlgorithm.Create()", defaultHash, hash.ToString());

		// try to build all hash algorithms
		hash = HashAlgorithm.Create ("SHA");
		AssertEquals ("HashAlgorithm.Create('SHA')", defaultSHA1, hash.ToString ());
		hash = HashAlgorithm.Create ("SHA1");
		AssertEquals ("HashAlgorithm.Create('SHA1')", defaultSHA1, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.SHA1");
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.SHA1')", defaultSHA1, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.HashAlgorithm" );
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.HashAlgorithm')", defaultHash, hash.ToString ());

		hash = HashAlgorithm.Create ("MD5");
		AssertEquals ("HashAlgorithm.Create('MD5')", defaultMD5, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.MD5");
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.MD5')", defaultMD5, hash.ToString ());

		hash = HashAlgorithm.Create ("SHA256");
		AssertEquals ("HashAlgorithm.Create('SHA256')", defaultSHA256, hash.ToString ());
		hash = HashAlgorithm.Create ("SHA-256");
		AssertEquals ("HashAlgorithm.Create('SHA-256')", defaultSHA256, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.SHA256");
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.SHA256')", defaultSHA256, hash.ToString ());
	
		hash = HashAlgorithm.Create ("SHA384");
		AssertEquals ("HashAlgorithm.Create('SHA384')", defaultSHA384, hash.ToString ());
		hash = HashAlgorithm.Create ("SHA-384");
		AssertEquals ("HashAlgorithm.Create('SHA-384')", defaultSHA384, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.SHA384");
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.SHA384')", defaultSHA384, hash.ToString ());
	
		hash = HashAlgorithm.Create ("SHA512");
		AssertEquals ("HashAlgorithm.Create('SHA512')", defaultSHA512, hash.ToString ());
		hash = HashAlgorithm.Create ("SHA-512");
		AssertEquals ("HashAlgorithm.Create('SHA-512')", defaultSHA512, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.SHA512");
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.SHA512')", defaultSHA512, hash.ToString ());
	
		// try to build invalid implementation
		hash = HashAlgorithm.Create ("InvalidHash");
		AssertNull ("HashAlgorithm.Create('InvalidHash')", hash);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public virtual void CreateNull () 
	{
		// try to build null implementation
		hash = HashAlgorithm.Create (null);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void Clear () 
	{
		byte[] inputABC = Encoding.Default.GetBytes ("abc");
		hash.ComputeHash (inputABC);
		hash.Clear ();
		// cannot use a disposed object
		hash.ComputeHash (inputABC);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void Clear2 () 
	{
		byte[] inputABC = Encoding.Default.GetBytes ("abc");
		MemoryStream ms = new MemoryStream (inputABC);
		hash.ComputeHash (ms);
		hash.Clear ();
		// cannot use a disposed object
		hash.ComputeHash (ms);
	}

	[Test]
	[ExpectedException (typeof (NullReferenceException))]
	public void NullStream () 
	{
		Stream s = null;
		byte[] result = hash.ComputeHash (s);
	}

	[Test]
	public void Disposable () 
	{
		using (HashAlgorithm hash = HashAlgorithm.Create ()) {
			byte[] data = hash.ComputeHash (new byte [0]);
		}
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void InitializeDisposed () 
	{
		hash.ComputeHash (new byte [0]);
		hash.Clear (); // disposed
		hash.Initialize ();
		hash.ComputeHash (new byte [0]);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void ComputeHash_ArrayNull ()
	{
		byte[] array = null;
		hash.ComputeHash (array);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void ComputeHash_ArrayNullIntInt ()
	{
		byte[] array = null;
		hash.ComputeHash (array, 0, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void ComputeHash_OffsetNegative ()
	{
		byte[] array = new byte [0];
		hash.ComputeHash (array, -1, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void ComputeHash_OffsetOverflow ()
	{
		byte[] array = new byte [1];
		hash.ComputeHash (array, Int32.MaxValue, 1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void ComputeHash_CountNegative ()
	{
		byte[] array = new byte [0];
		hash.ComputeHash (array, 0, -1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void ComputeHash_CountOverflow ()
	{
		byte[] array = new byte [1];
		hash.ComputeHash (array, 1, Int32.MaxValue);
	}

	[Test]
// not checked in Fx 1.1
//	[ExpectedException (typeof (ObjectDisposedException))]
	public void TransformBlock_Disposed () 
	{
		hash.ComputeHash (new byte [0]);
		hash.Initialize ();
		byte[] input = new byte [8];
		byte[] output = new byte [8];
		hash.TransformBlock (input, 0, input.Length, output, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TransformBlock_InputBuffer_Null ()
	{
		byte[] output = new byte [8];
		hash.TransformBlock (null, 0, output.Length, output, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void TransformBlock_InputOffset_Negative ()
	{
		byte[] input = new byte [8];
		byte[] output = new byte [8];
		hash.TransformBlock (input, -1, input.Length, output, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TransformBlock_InputOffset_Overflow ()
	{
		byte[] input = new byte [8];
		byte[] output = new byte [8];
		hash.TransformBlock (input, Int32.MaxValue, input.Length, output, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TransformBlock_InputCount_Negative ()
	{
		byte[] input = new byte [8];
		byte[] output = new byte [8];
		hash.TransformBlock (input, 0, -1, output, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TransformBlock_InputCount_Overflow ()
	{
		byte[] input = new byte [8];
		byte[] output = new byte [8];
		hash.TransformBlock (input, 0, Int32.MaxValue, output, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
#if ! NET_2_0
	[Category ("NotDotNet")] // System.ExecutionEngineException on MS runtime (1.1)
#endif
	public void TransformBlock_OutputBuffer_Null ()
	{
		byte[] input = new byte [8];
		hash.TransformBlock (input, 0, input.Length, null, 0);
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
#else
	[ExpectedException (typeof (IndexOutOfRangeException))]
#endif
	public void TransformBlock_OutputOffset_Negative ()
	{
		byte[] input = new byte [8];
		byte[] output = new byte [8];
		hash.TransformBlock (input, 0, input.Length, output, -1);
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentException))]
#else
	[ExpectedException (typeof (IndexOutOfRangeException))]
#endif
	public void TransformBlock_OutputOffset_Overflow ()
	{
		byte[] input = new byte [8];
		byte[] output = new byte [8];
		hash.TransformBlock (input, 0, input.Length, output, Int32.MaxValue);
	}

	[Test]
// not checked in Fx 1.1
//	[ExpectedException (typeof (ObjectDisposedException))]
	public void TransformFinalBlock_Disposed () 
	{
		hash.ComputeHash (new byte [0]);
		hash.Initialize ();
		byte[] input = new byte [8];
		hash.TransformFinalBlock (input, 0, input.Length);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TransformFinalBlock_InputBuffer_Null ()
	{
		hash.TransformFinalBlock (null, 0, 8);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void TransformFinalBlock_InputOffset_Negative ()
	{
		byte[] input = new byte [8];
		hash.TransformFinalBlock (input, -1, input.Length);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TransformFinalBlock_InputOffset_Overflow ()
	{
		byte[] input = new byte [8];
		hash.TransformFinalBlock (input, Int32.MaxValue, input.Length);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TransformFinalBlock_InputCount_Negative ()
	{
		byte[] input = new byte [8];
		hash.TransformFinalBlock (input, 0, -1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void TransformFinalBlock_InputCount_Overflow ()
	{
		byte[] input = new byte [8];
		hash.TransformFinalBlock (input, 0, Int32.MaxValue);
	}
}

}
