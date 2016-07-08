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
using NUnit.Framework;

namespace MonoTests.Mono.Math {

	public abstract class PrimeTesting_Base : BigIntegerTestSet {

		BigInteger P1, P2, P3;

		public PrimeTesting_Base () 
		{
			P1 = new BigInteger (p1);
			P2 = new BigInteger (p2);
			P3 = new BigInteger (p3);
		}

		public abstract uint[] p1 {
			get;
		}

		public abstract uint[] p2 {
			get;
		}

		public abstract uint[] p3 {
			get;
		}
		
		[Test]
		public void p1prime ()
		{
			ExpectPrime (P1);
		}

		[Test]
		public void p2prime ()
		{
			ExpectPrime (P2);
		}

		[Test]
		public void p3prime ()
		{
			ExpectPrime (P3);
		}

		[Test]
		public void p1p2composite ()
		{
			ExpectComposite (P1 * P2);
		}

		[Test]
		public void p1p3composite ()
		{
			ExpectComposite (P1 * P3);
		}

		[Test]
		public void p2p3composite ()
		{
			ExpectComposite (P2 * P3);
		}

		[Test]
		public void p1p2p3composite ()
		{
			ExpectComposite (P1 * P2 * P3);
		}

		private void ExpectComposite (BigInteger bi)
		{
			Assert.AreEqual (false, bi.isProbablePrime ());
		}

		private void ExpectPrime (BigInteger bi)
		{
			Assert.AreEqual (true, bi.isProbablePrime ());
		}
	}

	public class PrimeTesting_Rand1024 : PrimeTesting_Base {

		public override uint[] p1 {
			get {
				return new uint[] {
					0x48595126, 0xd1e0c772, 0x87f352a7, 0xeb3c496c, 0xce17d7ff, 0xce260883,
					0x4892835e, 0x4457170e, 0xb90a0893, 0x2a1bfd80, 0x56665a9c, 0x36b06f35,
					0x61988d45, 0xa04e18c2, 0xa2308414, 0xa0be5e2c, 0x423fad73, 0x7117b883,
					0x3977c11c, 0xf34c2c20, 0x045713c9, 0x0c82ea36, 0x3811b550, 0x7b03aafb,
					0xbc31f3c4, 0x8667b5a5, 0x3a5697f7, 0x064169e8, 0xd70dbae4, 0x9bb2a4f8,
					0xba6a1c1c, 0x7c6db863
				};
			}
		}

		public override uint[] p2 {
			get {
				return new uint[] {
					0x884462b0, 0x8295cefd, 0x444cbcb7, 0xd3916039, 0x45b1e26d, 0x02b3d8d5,
					0x3547b6ee, 0x0791ef10, 0x6da42d3e, 0xee537c9f, 0x339ee744, 0x97d328c7,
					0xebc9055a, 0xf3e1835c, 0xd9cff3db, 0xfe5f33d8, 0x45234644, 0x4af5031b,
					0x27f41403, 0x1d9d751b, 0xb711ddc7, 0xb331784f, 0x992b4148, 0x50a8ac7d,
					0x5c3f1fbb, 0x209d76e3, 0xfbd05088, 0xacf87776, 0xad214d60, 0x1f2ab42d,
					0xe9bc81fc, 0xe997d55b
				};
			}
		}

		public override uint[] p3 {
			get {
				return new uint[] {
					0xf732ee, 0x019ec52e, 0xfc360881, 0x4fd07211, 0x77d44ed0, 0xc27a4b3d,
					0xde2a9500, 0x2d4a2a70, 0x834e5d32, 0x715f5884, 0xc5922ca1, 0x94d48b60,
					0xb0262fce, 0x72040eb9, 0x5a4fd41c, 0x4e095cba, 0x3a840a36, 0x0175b3b4,
					0x64363623, 0xc03bd892, 0x39231a04, 0x521eee6c, 0x560e7c10, 0xa8476256,
					0xeefc3f37, 0xadd4c5ee, 0xf8407afc, 0x30e9c52c, 0x026849d3, 0x040533df,
					0xc286e00b, 0x9c377705
				};
			}
		}
	}

	public class PrimeTesting_Rand512 : PrimeTesting_Base {

		public override uint[] p1 {
			get {
				return new uint[] {
					0x99d95780, 0xd02a33bb, 0x980c079b, 0xbc43c3c2, 0xca501ce0, 0x3fc4bd85,
					0x51035dcc, 0x11dd4c8e, 0x59696b91, 0xcdc7cbc0, 0x29e5c884, 0xae628e88,
					0x908855b7, 0xab6218f3, 0x6abd6fb5, 0x3ca12af7
				};
			}
		}

		public override uint[] p2 {
			get {
				return new uint[] {
					0xc77a6a36, 0xfe547705, 0x98a57094, 0xc0dd1e8b, 0x78b62bc9, 0x19aea0da,
					0xb91b141b, 0xe4d34402, 0xdd16b9c6, 0x0ec73ea4, 0x8ad59ae5, 0x0d4b0f09,
					0x1fd1858d, 0xaac2891c, 0xbd56c29f, 0xb398ffa5
				};
			}
		}

		public override uint[] p3 {
			get {
				return new uint[] {
					0xb98e9b3a, 0x197d7671, 0x104d6b15, 0xe8c76058, 0xed9fcb77, 0x65c38af7,
					0xdd660b8e, 0x412c5bbb, 0x80b5f777, 0x70c1a458, 0xc9ad52ae, 0x489bae51,
					0x795f99a7, 0x2f2cb4ae, 0xc902c3ad, 0x9d96456f
				};
			}
		}
	}
	
	public class PrimeTesting_Rand128 : PrimeTesting_Base {

		public override uint[] p1 {
			get {
				return new uint[] {
					0x28480536, 0xeaf326bc, 0x2957b03b, 0xa1549e59
				};
			}
		}

		public override uint[] p2 {
			get {
				return new uint[] {
					0xd9ce28be, 0x6a279407, 0x8da0afbc, 0xa57eb9b3
				};
			}
		}

		public override uint[] p3 {
			get {
				return new uint[] {
					0x1d777a45, 0x957a0fad, 0x25d049a7, 0x4f73383b
				};
			}
		}
	}
}
