//
// HMACSHA384Test.cs - NUnit Test Cases for HMACSHA384
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
	public class HMACSHA384Test : KeyedHashAlgorithmTest {

		protected HMACSHA384 algo;

		[SetUp]
		protected override void SetUp () 
		{
			algo = new HMACSHA384 ();
			algo.Key = new byte [8];
			hash = algo;
		}

		[Test]
		public void Constructors () 
		{
			algo = new HMACSHA384 ();
			AssertNotNull ("HMACSHA384 ()", algo);

			byte[] key = new byte [8];
			algo = new HMACSHA384 (key);
			AssertNotNull ("HMACSHA384 (key)", algo);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Null () 
		{
			new HMACSHA384 (null);
		}

		[Test]
		public void Invariants () 
		{
			algo = new HMACSHA384 ();
			AssertEquals ("HMACSHA384.CanReuseTransform", true, algo.CanReuseTransform);
			AssertEquals ("HMACSHA384.CanTransformMultipleBlocks", true, algo.CanTransformMultipleBlocks);
			AssertEquals ("HMACSHA384.HashName", "SHA384", algo.HashName);
			AssertEquals ("HMACSHA384.HashSize", 384, algo.HashSize);
			AssertEquals ("HMACSHA384.InputBlockSize", 1, algo.InputBlockSize);
			AssertEquals ("HMACSHA384.OutputBlockSize", 1, algo.OutputBlockSize);
			AssertEquals ("HMACSHA384.ToString()", "System.Security.Cryptography.HMACSHA384", algo.ToString ()); 
		}
	}
}

#endif
