//
// System.Random Test Cases
//
// Authors: 
//	Bob Smith <bob@thestuff.net>
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Reflection;

namespace MonoTests.System {

	[TestFixture]
	public class RandomTest  {

		[Test]
		public void CompareStreamWithSameSeed ()
		{
			Random r = new Random (42);
			Random r2 = new Random (42);
			for (int i=0; i<20; i++) {
				Assert.AreEqual (r.NextDouble (), r2.NextDouble (), i.ToString ());
			}
		}

		[Test]
		public void Next ()
		{
			Random r = new Random ();
			for (int i=0; i<20; i++) {
				long c = r.Next ();
				Assert.IsTrue (c < Int32.MaxValue && c >= 0, "Next(" + i + ")");
			}
		}

		[Test]
		public void NextZero ()
		{
			Random r = new Random ();
			Assert.AreEqual (0, r.Next (0),"Next(0) failed");
		}

		[Test]
		public void NextMax()
		{
			Random r = new Random();
			for (int i=0; i<20; i++) {
				long c = r.Next (10);
				Assert.IsTrue (c < 10 && c >= 0, "NextMax(" + i + ")");
			}
		}

		[Test]
		public void NextMinMax()
		{
			Random r = new Random ();
			Assert.AreEqual (42, r.Next (42, 42), "#1 Failed where min == max");
			Assert.AreEqual (Int32.MaxValue, r.Next (Int32.MaxValue, Int32.MaxValue), "#2 Failed where min == max");
			Assert.AreEqual (Int32.MinValue, r.Next (Int32.MinValue, Int32.MinValue), "#3 Failed where min == max");
			Assert.AreEqual (0, r.Next (0, 0), "#4 Failed where min == max");
			for (int i = 1; i <= Int32.MaxValue / 2; i *= 2) {
				long c = r.Next (i, i * 2);
				Assert.IsTrue (c < i * 2, "At i=" + i + " c < i*2 failed");
				Assert.IsTrue (c >= i, "At i=" + i + " c >= i failed");
			}
			for (int i = -1; i >= Int32.MinValue / 2; i *= 2) {
				long c = r.Next (i * 2, i);
				Assert.IsTrue (c < i, "At i=" + i + " c < i*2 failed");
				Assert.IsTrue (c >= i * 2, "At i=" + i + " c >= i failed");
			}
		}

		class RandomSampleOverride : Random {

			protected override double Sample ()
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void Base_Int ()
		{
			var random = new RandomSampleOverride ();
			// from 2.0+ Next(), Next(int,int) and NextBytes(byte[]) do not call Sample
			// see MSDN's Notes to Inheritors
			random.Next ();
			random.Next (Int32.MinValue, Int32.MaxValue);
			random.NextBytes (new byte[1]);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void Base_Double ()
		{
			var random = new RandomSampleOverride ();
			random.NextDouble ();
		}

		// generate values (one for each 1024 returned values) from the original C implementation
		static uint[] jkiss_values = {
			560241513,	/* 0 */
			1281708802,	/* 1024 */
			1571324528,	/* 2048 */
			1565809406,	/* 3072 */
			1010890569,	/* 4096 */
			1778803435,	/* 5120 */
			903613637,	/* 6144 */
			3496059008,	/* 7168 */
			108603163,	/* 8192 */
			1854081276,	/* 9216 */
			3703232459,	/* 10240 */
			2191562138,	/* 11264 */
			337995793,	/* 12288 */
			1340840062,	/* 13312 */
			2364148985,	/* 14336 */
			2549812361,	/* 15360 */
			563432369,	/* 16384 */
			229365487,	/* 17408 */
			1821397325,	/* 18432 */
			3246092454,	/* 19456 */
			691032417,	/* 20480 */
			86951316,	/* 21504 */
			3029975455,	/* 22528 */
			1261370163,	/* 23552 */
			2539815382,	/* 24576 */
			3017891647,	/* 25600 */
			3877215120,	/* 26624 */
			3142958765,	/* 27648 */
			1080903191,	/* 28672 */
			2837464745,	/* 29696 */
			614275602,	/* 30720 */
			2250626199,	/* 31744 */
			729001311,	/* 32768 */
			3313769017,	/* 33792 */
			2408398670,	/* 34816 */
			3123583383,	/* 35840 */
			3346590423,	/* 36864 */
			1629546563,	/* 37888 */
			251343753,	/* 38912 */
			2695793631,	/* 39936 */
			2768993787,	/* 40960 */
			3688573224,	/* 41984 */
			2897218561,	/* 43008 */
			2725058810,	/* 44032 */
			2142061914,	/* 45056 */
			3983217096,	/* 46080 */
			3609758190,	/* 47104 */
			842060935,	/* 48128 */
			2893482035,	/* 49152 */
			2290461665,	/* 50176 */
			1709481476,	/* 51200 */
			3633857838,	/* 52224 */
			332645044,	/* 53248 */
			3522654497,	/* 54272 */
			2501348469,	/* 55296 */
			1644344287,	/* 56320 */
			3081428084,	/* 57344 */
			3114560766,	/* 58368 */
			489030597,	/* 59392 */
			367291591,	/* 60416 */
			106358682,	/* 61440 */
			3020781303,	/* 62464 */
			1209590375,	/* 63488 */
			1833282169,	/* 64512 */
			61543407,	/* 65536 */
		};

		[Test]
		public void JKISS ()
		{
			// Random.Next() returns a non-negative *signed* integer value - so it can't be used for testing
			var next = typeof(Random).GetMethod ("JKiss", BindingFlags.Instance | BindingFlags.NonPublic);

			// if the method is not present, e.g. on MS.NET, skip this test
			if (next == null)
				Assert.Ignore ("The JKiss method is not present, e.g. on MS.NET.");

			// ensure we match the original JKISS random stream
			// first 64KB but without checking every value (one each KB)
			Random r = new Random (123456789);
			int n = 0;
			int j = 0;
			while (j < 64 * 1024) {
				uint random = (uint) next.Invoke (r, null);
				if (j++ % 1024 == 0) {
					Assert.AreEqual (random, jkiss_values [n], n.ToString ());
					n++;
				}
			}
		}
	}
}
