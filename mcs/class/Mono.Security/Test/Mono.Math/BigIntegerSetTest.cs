//
// MonoTests.Mono.Math.BigIntegerSetTest.cs
//
// Authors:
//	Ben Maurer
//
// Copyright (c) 2003 Ben Maurer. All rights reserved
//

using System;
using Mono.Math;
using NUnit.Framework;

namespace MonoTests.Mono.Math {

	[TestFixture]
	public abstract class BigIntegerTestSet {
		
		protected string Name {
			get { return this.GetType ().Name; }
		}

		protected void Expect (BigInteger actual, BigInteger expected) 
		{
			Assert.AreEqual (expected, actual, Name);
		}
	}
}
