//
// MonoTests.Mono.Math.GcdBigTest.cs
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

	public abstract class GcdBig_Base : BigIntegerTestSet {

		BigInteger A, B, aGcdB;

		public GcdBig_Base() 
		{
			A = new BigInteger(a);
			B = new BigInteger(b);
			aGcdB = new BigInteger(ExpectedAgcdB);
		}
		
		public abstract uint[] a {
			get;
		}
		public abstract uint[] b {
			get;
		}

		public abstract uint[] ExpectedAgcdB {
			get;
		}

		[Test]
		public void GcdPP()
		{
			Expect( A.gcd(B), aGcdB );
		}

	}

	public class GcdBig_Rand2048a512b : GcdBig_Base {
	
		public override uint[] a {
			get {
				return new uint[] {
					0xaae18fa1, 0x58e1e9fc, 0x836350a0, 0x23d2a12d, 0x1aec1bdc, 0xad4a3b30,
					0x40dc1d27, 0x625277fb, 0xddfbee25, 0xc1820dac, 0x4418603a, 0x5aec122c,
					0x58b70181, 0x129d6b33, 0x6c4ed37e, 0x70808dd0, 0xed55b079, 0x706f15f3,
					0x1a84b3ac, 0x088f1679, 0xcbf2be66, 0xb97a885e, 0xa2c95b95, 0xd44ebb83,
					0x69351a38, 0x21d3cdb2, 0x30844c5c, 0x5abf8d7a, 0xb663c3de, 0x3ce2fcc5,
					0x80b42a05, 0xa0a8aca3, 0x31d42948, 0x469d8ad5, 0xe0f66f7e, 0x3250dac1,
					0xb4450067, 0xb247ad34, 0xbfd74c70, 0xfd1e29fa, 0x4050dc77, 0x827763b9,
					0xba41410d, 0x42494b62, 0x99ef13a1, 0x55c957ec, 0x1e7dd0fc, 0x6c4cccdf,
					0x981128e0, 0xc7c01688, 0x4ae5116c, 0x9b24d3c8, 0x623af290, 0x31ac1c1e,
					0x0891f2f1, 0x678ab7ee, 0x6169af02, 0x2479511c, 0x7cea2114, 0xf13d541c,
					0x0f54f253, 0xf5dd2553, 0xc0ea9613, 0x84a0c7b2
				};
			}
		}

		public override uint[] b {
			get {
				return new uint[] {
					0x83ab64ef, 0x1350b335, 0xbf5f150d, 0x399a2487, 0xc1b76d63, 0xdf3e59ad,
					0x2ed3b4fa, 0x9a6ad972, 0xb6e791c5, 0xdd7a8664, 0x802f8364, 0xf1a8617f,
					0xb036a74a, 0x452ac130, 0x727e194f, 0x3dd8cfe8
				};
			}
		}

		public override uint[] ExpectedAgcdB {
			get {
				return new uint[] {
					0x00000006
				};
			}
		}
	}

	public class GcdBig_Rand512a512b : GcdBig_Base {

		public override uint[] a {
			get {
				return new uint[] {
					0x7bc1fb0e, 0x0335547e, 0x0e85b746, 0x1e7554d2, 0x77515958, 0xadf072c8,
					0xdb03eb22, 0xdcedf7e2, 0x8f923f6a, 0xf124052d, 0x55d623f4, 0x29083f62,
					0x66eaa78e, 0xfe819e63, 0xf8229bde, 0xe155c05b
				};
			}
		}

		public override uint[] b {
			get {
				return new uint[] {
					0xe5320f2e, 0x379803d7, 0x24363d84, 0xfd61c43c, 0xa4e6ca6f, 0x16628f59,
					0xddeb6557, 0xd904639c, 0x55f59c2f, 0x6ba21e54, 0x81107aa9, 0x47a4653c,
					0xcc0a5889, 0xb6abcaec, 0xf6b58d60, 0xf8cc7eeb
				};
			}
		}

		public override uint[] ExpectedAgcdB {
			get {
				return new uint[] {
					0x00000001
				};
			}
		}
	}
}
