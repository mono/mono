//
// BigIntegerTest.cs - NUnit Test Cases for BigInteger
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004, 2007 Novell, Inc (http://www.novell.com)
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
using myalias = System;
using Mono.Math;

namespace MonoTests.Mono.Math {

	[TestFixture]
	public class BigIntegerTest {

		[Test]
		public void DefaultBitCount () 
		{
			BigInteger bi = new BigInteger ();
			Assert.AreEqual (0, bi.BitCount (), "default BitCount");
			// note: not bit are set so BitCount is zero
		}

		[Test]
		public void DefaultRandom () 
		{
			// based on bugzilla entry #68452
			BigInteger bi = new BigInteger ();
			Assert.AreEqual (0, bi.BitCount (), "before randomize");
			bi.Randomize ();
			// Randomize returns a random number of BitCount length
			// so in this case it will ALWAYS return 0
			Assert.AreEqual (0, bi.BitCount (), "after randomize");
			Assert.AreEqual (new BigInteger (0), bi, "Zero");
		}

		[Test]
		public void ModPow_0_Even ()
		{
			BigInteger x = new BigInteger (1);
			BigInteger y = new BigInteger (0);
			BigInteger z = x.ModPow (y, 1024);
			Assert.AreEqual ("1", z.ToString (), "1 pow 0 == 1");
		}

		[Test]
		public void ModPow_Big_Even ()
		{
			// http://gcc.gnu.org/ml/java/2001-01/msg00150.html
			BigInteger x = BigInteger.Parse ("222556259477882361118129720038750144464896096345697329917462180806109470940281821580712930114298080816996240075704780895407778416354633927929850543336844729388676722554712356733107888579404671103423966348754128720372408391573576775380281687780687492527566938517625657849775850241884119610654472761291507970934");
			BigInteger y = BigInteger.Parse ("110319153937683287453746757581772092163629769182044007837690319614087550020383807943886070460712008994638849038231331120616035703719955147238394349941968802357224177878230564379014395900786093465543114548034361805469457605783731382574787980771957640613447628351175959168798011343064123908688343944150028709336");
			BigInteger z = BigInteger.Parse ("211455809992703561445401788842734346323873054957006050135582190157359001703882707072169880651159563587522668850959539052488297197610540840476872693108381476249027986010074543599432542677282684917897250864056294311624311681558854158430574409491081490219256907243905496547813878640883064959346343865887971384185");
			BigInteger a = z.ModPow (x, y);
			Assert.AreEqual ("89040229313686098274750802637193802904787850353791629688385431482589769348345172944539658366893587456857347312314974124445695423885005533414559099801699612294235861570065774222911180890417009385455826560773741520297884850460324781620974467560905975577765401911117379967692495136423710471201230243826129276993", a.ToString ());
		}

		[Test]
		public void ModPow_2 ()
		{
			// #70169
			BigInteger b = new BigInteger (10);
			BigInteger m = new BigInteger (32);
			// after 40 we start loosing double precision and result will differ
			for (int i=1; i < 40; i++) {
				BigInteger e = new BigInteger (i);
				BigInteger r = e.ModPow (b, m);
				long expected = (long) myalias.Math.Pow (i, 10) % 32;
				Assert.AreEqual (expected.ToString (), r.ToString (), i.ToString ());
			}
		}

		[Test]
		public void ModPow_3 ()
		{
			BigInteger b = new BigInteger (2);
			BigInteger m = new BigInteger (myalias.Int32.MaxValue);
			// after 62 we start loosing double precision and result will differ
			for (int i = 1; i < 62; i++) {
				long expected = (long) myalias.Math.Pow (2, i) % myalias.Int32.MaxValue;
				BigInteger e = new BigInteger (i);
				BigInteger r = b.ModPow (e, m);
				Assert.AreEqual (expected.ToString (), r.ToString (), i.ToString ());
			}
		}

		[Test]
		public void Bug81857 ()
		{
			BigInteger b = BigInteger.Parse ("18446744073709551616");
			BigInteger exp = new BigInteger (2);
			BigInteger mod = BigInteger.Parse ("48112959837082048697");
			BigInteger expected = BigInteger.Parse ("4970597831480284165");

			BigInteger manual = b * b % mod;
			Assert.AreEqual (expected, manual, "b * b % mod");
// fails (inside Barrett reduction)
//			BigInteger actual = b.ModPow (exp, mod);
//			Assert.AreEqual (expected, actual, "b.ModPow (exp, mod)");
		}

		[Test]
		public void IsProbablePrime_Small ()
		{
			// last of the small prime tables
			Assert.IsTrue (new BigInteger (5987).IsProbablePrime (), "5987");
			// small value with exponent == 1
			Assert.IsTrue (new BigInteger (65537).IsProbablePrime (), "65537");
		}
	}
}
