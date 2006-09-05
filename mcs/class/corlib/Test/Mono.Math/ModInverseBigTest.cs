//
// MonoTests.Mono.Math.ModInverseBigTest.cs
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

	public abstract class ModInverseBig_Base : BigIntegerTestSet {

		BigInteger A, B, AmodinvB;

		public ModInverseBig_Base () 
		{
			A = new BigInteger (a);
			B = new BigInteger (b);
			AmodinvB = new BigInteger (ExpectedAmodinvB);
		}
		
		public abstract uint[] a {
			get;
		}
		public abstract uint[] b {
			get;
		}

		public abstract uint[] ExpectedAmodinvB {
			get;
		}

		[Test]
		public void ModInvPP () 
		{
			Expect (A.modInverse (B), AmodinvB);
		}
	}
	
	public class ModInverseBig_Rand512a1024b : ModInverseBig_Base {

	
		public override uint[] a {
			get {
				return new uint[] {
					0x48fd8f2e, 0xa791b900, 0x19e53aaa, 0x6d45758a, 0xeb8be610, 0x25c42285,
					0xabff3066, 0xcdcb9969, 0xa08fd7c1, 0x1c382419, 0xcd6b685b, 0x23e8bcaf,
					0x592a0f82, 0x2b54b60b, 0xbb67f3c2, 0x64313461
				};
			}
		}

		public override uint[] b {
			get {
				return new uint[] {
					0x3617f5e4, 0xd32a40dc, 0x358f7c09, 0x976b345b, 0x4dc05c63, 0x91ed990d,
					0xfc66b0a6, 0x0a36dd7e, 0x7a5ea721, 0xf28b577e, 0xf014cbf6, 0x597b4094,
					0x177f8253, 0x6587a352, 0x67fedcf0, 0x61ee389e, 0xff86cc7d, 0x9817cd3a,
					0x632d6730, 0x3082112a, 0x48509e74, 0x198c7802, 0x69d8c4c6, 0x727313e9,
					0x1e11ae51, 0xa72a33bb, 0xc9059cc7, 0xc9fc6268, 0x34ed466b, 0x11e49879,
					0x8eb7ebf5, 0x98b53108
				};
			}
		}

		public override uint[] ExpectedAmodinvB {
			get {
				return new uint[] {
					0x045fbad9, 0x61867c14, 0xb30ff1f7, 0x5deaf4c1, 0xd19e0b62, 0xc4ed73af,
					0x9501dbd4, 0x0b052e1c, 0xd3e944c3, 0xeddc333b, 0x1444c1f9, 0x38ca61b7,
					0xceec8a0d, 0xec8f2814, 0xa2099df4, 0x2a0ddbd0, 0x9193985d, 0x09f89197,
					0xb58e7229, 0x45c1f891, 0x93553056, 0x462dbe6a, 0xb70c95d0, 0x7cf80ae9,
					0x7833e1bf, 0x88329c50, 0xdbde3ef8, 0x7a426200, 0x4335234a, 0x2556ba2c,
					0x94cc2109, 0x046645c1
				};
			}
		}
	}

	public class ModInverseBig_Rand256a1024b : ModInverseBig_Base {

	
		public override uint[] a {
			get {
				return new uint[] {
					0xcc2e79fa, 0x6901026e, 0xc4fdb0d4, 0xb4173ce7, 0xf7f96af1, 0x8339780b,
					0x22268620, 0x382c75a4
				};
			}
		}

		public override uint[] b {
			get {
				return new uint[] {
					0x4b23ac44, 0xbddfc733, 0x3ea2a85b, 0x02daa1d8, 0x2b31cf00, 0x2503a376,
					0xa3d47c77, 0x829266d7, 0x1e29fad8, 0x4f0e3788, 0xc9c128d7, 0x13ea53eb,
					0x85fc86b1, 0xbad2c0c4, 0xd87dcdad, 0x0a09ba8c, 0x126a0ede, 0x1fe390c7,
					0x12c5a679, 0xf23557e1, 0x8e7b1934, 0xc102f83b, 0x934de6d6, 0x254aee03,
					0x34f02315, 0x33edbb07, 0x97a87ecf, 0xbf534337, 0xe347ae90, 0xf2eb1176,
					0x5459c63b, 0x8f3b0f75
				};
			}
		}

		public override uint[] ExpectedAmodinvB {
			get {
				return new uint[] {
					0x12d80123, 0x9b288afc, 0x466ed241, 0x8bf1804d, 0x73cd667f, 0x8eb1eb34,
					0x513df007, 0x464c7245, 0xf3b97899, 0xd5cb92c7, 0xdefdb611, 0x5258e545,
					0xe8b66c76, 0xd11c58e3, 0xab1fc29a, 0x9718099e, 0xa4040d4e, 0x29980874,
					0xda2b4e0d, 0xfed020de, 0x6bde01e6, 0x15b084af, 0x9657aa64, 0x760c64f0,
					0x6bba8099, 0xef1a409e, 0xf80b1ec7, 0x4a69256b, 0xf867ec36, 0xd3659a2a,
					0xf23ec3a5, 0x04349da9
				};
			}
		}
	}
	
	public class ModInverseBig_Rand256a512b : ModInverseBig_Base {

		public override uint[] a {
			get {
				return new uint[] {
					0x2e9342c8, 0x1ac6b23d, 0xeb18d3f9, 0x2b076025, 0x030232ee, 0xd1cb7f22,
					0xfbfe74df, 0xabadc589
				};
			}
		}

		public override uint[] b {
			get {
				return new uint[] {
					0x7e0a43ca, 0xf05d9c52, 0x28e68cf6, 0xf168b591, 0x88c17e79, 0xcb075c3b,
					0x92a16680, 0xd7dccd53, 0xe6da1248, 0xe71811b7, 0x4d0a3c42, 0x1ebb46cc,
					0x71d4dd69, 0x07a642d9, 0x8eae29d0, 0xcbd278b4
				};
			}
		}

		public override uint[] ExpectedAmodinvB {
			get {
				return new uint[] {
					0x77dc2534, 0xf81a1bc7, 0xfbd6b350, 0x809b2c31, 0x3e04ad9f, 0x5101b59f,
					0xcee28213, 0x726356fe, 0xeb7d0a6b, 0x01ed7bd7, 0x27ff2f04, 0xa3cbd6a4,
					0xcd6a849d, 0x029c9a79, 0xb82d5da2, 0x87af9e81
				};
			}
		}
	}

	public class ModInverseBig_Rand1024a5b : ModInverseBig_Base {

		public override uint[] a {
			get {
				return new uint[] {
					0x60cc0502, 0xebe19e1b, 0xfc64fd47, 0xfc34fbac, 0x81b3b346, 0x57e9ebf8,
					0x96501b67, 0xc95eb1cc, 0x2e126045, 0xa56ec13b, 0x2f812165, 0xb4391e46,
					0xb245069a, 0xfeb836b6, 0xebeceb62, 0xedd9f9bc, 0x9bdd63ba, 0xac491f92,
					0xb8ab6898, 0x8a5ea88d, 0xf7f24993, 0x75e86618, 0x4e939376, 0x7a2ac365,
					0xb270f14c, 0x416fb9bc, 0x77af8352, 0x488e1a5f, 0xe22e8cda, 0xcaa72806,
					0xf649f663, 0xefee082d
				};
			}
		}

		public override uint[] b {
			get {
				return new uint[] {
					0x11
				};
			}
		}

		public override uint[] ExpectedAmodinvB {
			get {
				return new uint[] {
					0x2
				};
			}
		}
	}

	public class ModInverseBig_Rand3a1024b : ModInverseBig_Base {

		public override uint[] a {
			get {
				return new uint[] {
					0x5
				};
			}
		}

		public override uint[] b {
			get {
				return new uint[] {
					0x3919ec8a, 0x8c713779, 0xb87d2db0, 0x7922df91, 0x34bf77e1, 0x49c08156,
					0x9ffb8ed5, 0x522a42e9, 0xdf18b1ff, 0x1cfdc432, 0x8564555b, 0x2b800684,
					0xa46bad82, 0x175a04ea, 0x87e4d513, 0xfb956ebc, 0xb74745e6, 0x85f45bf5,
					0xe580bb11, 0x290bfc35, 0x8d8782d8, 0x1054ea4e, 0x93eb86bb, 0xe7ea2e42,
					0x762c2945, 0x23e59e46, 0x833fe0b2, 0x3797025f, 0xabc64408, 0x94d0c8ac,
					0x2a31e00b, 0xd4d28ab0
				};
			}
		}

		public override uint[] ExpectedAmodinvB {
			get {
				return new uint[] {
					0x2dae56d5, 0x3d275f94, 0x939757c0, 0x60e8b2da, 0x90992cb4, 0x3b006778,
					0x7ffc7244, 0x41bb68bb, 0x18e08e65, 0xb0cb035b, 0x9de9dde2, 0x8933386a,
					0x1d22f134, 0xdf7b3722, 0x0650aa76, 0x62ddf230, 0x929f6b1e, 0xd1904991,
					0x8466fc0d, 0xba6ffcf7, 0xa46c68ac, 0xd9dd883e, 0xdcbc6bc9, 0x8654f1ce,
					0xc4f02104, 0x1cb7b1d2, 0x0299808e, 0x92df3519, 0x5638366d, 0x43da3a23,
					0x54f4b33c, 0xaa42088d
				};
			}
		}
	}	
}
