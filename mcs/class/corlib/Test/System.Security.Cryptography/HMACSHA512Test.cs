//
// HMACSHA512Test.cs - NUnit Test Cases for HMACSHA512
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class HMACSHA512Test : KeyedHashAlgorithmTest {

		protected HMACSHA512 algo;

		[SetUp]
		protected override void SetUp () 
		{
			algo = new HMACSHA512 ();
			algo.Key = new byte [8];
			hash = algo;
		}

		[Test]
		public void Constructors () 
		{
			algo = new HMACSHA512 ();
			AssertNotNull ("HMACSHA512 ()", algo);

			byte[] key = new byte [8];
			algo = new HMACSHA512 (key);
			AssertNotNull ("HMACSHA512 (key)", algo);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Null () 
		{
			new HMACSHA512 (null);
		}

		[Test]
		public void Invariants () 
		{
			algo = new HMACSHA512 ();
			AssertEquals ("HMACSHA512.CanReuseTransform", true, algo.CanReuseTransform);
			AssertEquals ("HMACSHA512.CanTransformMultipleBlocks", true, algo.CanTransformMultipleBlocks);
			AssertEquals ("HMACSHA512.HashName", "SHA512", algo.HashName);
			AssertEquals ("HMACSHA512.HashSize", 512, algo.HashSize);
			AssertEquals ("HMACSHA512.InputBlockSize", 1, algo.InputBlockSize);
			AssertEquals ("HMACSHA512.OutputBlockSize", 1, algo.OutputBlockSize);
			AssertEquals ("HMACSHA512.ToString()", "System.Security.Cryptography.HMACSHA512", algo.ToString ()); 
		}
	}
}

#endif
