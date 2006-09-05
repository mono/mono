//
// MonoTests.Mono.Math.PrimeGenerationTest.cs
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

	public abstract class PrimeGeneration_Base : BigIntegerTestSet {

		BigInteger s, e;

		public PrimeGeneration_Base ()
		{
			s = new BigInteger (Start);
			e = new BigInteger (Expected);
		}

		public abstract uint[] Start {
			get;
		}
		
		public abstract uint[] Expected {
			get;
		}
		
		[Test]
		public void GeneratePrime ()
		{
			BigInteger r = BigInteger.NextHighestPrime (s);
			Expect (r, e);
		}
	}
	
	public class PrimeGeneration_Rand1024 : PrimeGeneration_Base {

		public override uint[] Start {
			get {
				return new uint[] {
					0x7eaceb59, 0x344f3bcd, 0xffc5c003, 0xe17e9912, 0x05653294, 0x5383551f,
					0x0f604c65, 0x308f77ec, 0xf86d18d8, 0xad262661, 0x35f021eb, 0x36d3c53c,
					0x5e1ec5a9, 0x1c3dd738, 0x67b49061, 0x294a5a65, 0x0754a346, 0xe9ee0bf2,
					0x181c5e1d, 0x1eb719fa, 0xc4372255, 0x0bdee9bb, 0xbf9cfe74, 0x1b2be7f5,
					0xcf81e022, 0xa43d2698, 0x9c59f568, 0xc45d024d, 0x62eb1a3f, 0x125ad0cf,
					0x11e6834e, 0x09745d93
				};
			}
		}

		public override uint[] Expected {
			get {
				return new uint[] {
					0x7eaceb59, 0x344f3bcd, 0xffc5c003, 0xe17e9912, 0x05653294, 0x5383551f,
					0x0f604c65, 0x308f77ec, 0xf86d18d8, 0xad262661, 0x35f021eb, 0x36d3c53c,
					0x5e1ec5a9, 0x1c3dd738, 0x67b49061, 0x294a5a65, 0x0754a346, 0xe9ee0bf2,
					0x181c5e1d, 0x1eb719fa, 0xc4372255, 0x0bdee9bb, 0xbf9cfe74, 0x1b2be7f5,
					0xcf81e022, 0xa43d2698, 0x9c59f568, 0xc45d024d, 0x62eb1a3f, 0x125ad0cf,
					0x11e6834e, 0x09745fe9
				};
			}
		}
	}
	
	public class PrimeGeneration_512 : PrimeGeneration_Base {

		public override uint[] Start {
			get {
				return new uint[] {
					0x5629a00f, 0x3b672419, 0x85891da8, 0x2d63ff0c, 0xd8b91375, 0xfb57f659,
					0x07e2de17, 0x7de561be, 0xc29d6912, 0x198cb7fd, 0x251d33ad, 0x6bc20a0a,
					0xdd1e4060, 0x809d5fb2, 0x20fcd816, 0xafc2ddb2
				};
			}
		}

		public override uint[] Expected {
			get {
				return new uint[] {
					0x5629a00f, 0x3b672419, 0x85891da8, 0x2d63ff0c, 0xd8b91375, 0xfb57f659,
					0x07e2de17, 0x7de561be, 0xc29d6912, 0x198cb7fd, 0x251d33ad, 0x6bc20a0a,
					0xdd1e4060, 0x809d5fb2, 0x20fcd816, 0xafc2dfd5
				};
			}
		}
	}
}
