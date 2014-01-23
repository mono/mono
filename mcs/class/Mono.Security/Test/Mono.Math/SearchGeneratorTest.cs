//
// MonoTests.Mono.Math.PrimeTestingTest.cs
//
// Authors:
//	Ben Maurer
//
// Copyright (c) 2003 Ben Maurer. All rights reserved
//

using System;
using Mono.Math;
using Mono.Math.Prime;
using Mono.Math.Prime.Generator;
using NUnit.Framework;

namespace MonoTests.Mono.Math {

	[TestFixture]
	public class SearchGenerator_Test : SequentialSearchPrimeGeneratorBase {

		struct ContextData {
			public ContextData (int bits, uint testData)
			{
				this.bits = bits; this.testData = testData;
			}
			public int bits;
			public uint testData;
		}

		protected override BigInteger GenerateSearchBase (int bits, object Context)
		{
			BigInteger ret = base.GenerateSearchBase (bits, Context);

			ContextData ctx = (ContextData)Context;

			
			Assertion.AssertEquals (ctx.bits, bits);
			uint d = ctx.testData;

			for (uint i = (uint)bits - 2; d > 0; i--, d >>= 1)
				ret.SetBit (i, (d&1) == 1);

			return ret;
			
		}

		public override PrimalityTest PrimalityTest {
			get {
				return new PrimalityTest (PrimalityTests.SmallPrimeSppTest);
			}
		}

		protected override bool IsPrimeAcceptable (BigInteger bi, object Context)
		{
			return bi.TestBit (1);
		}

		[Test]
		public void TestPrimeGeneration ()
		{
			Random r = new Random ();
			for (int i = 0; i < 5; i++) {
				ContextData ctx = new ContextData (128, (uint)r.Next (int.MinValue, int.MaxValue));
				BigInteger p = GenerateNewPrime (128, ctx);
				Assert.IsTrue (p.TestBit (1));
				uint d = ctx.testData;
				for (uint j = 128 - 2; d > 0; j--, d >>= 1)
					Assertion.AssertEquals ((d&1) == 1, p.TestBit (j));
			}
		}
	}
}
