//
// System.Random Test Cases
//
// Authors: 
//	Bob Smith <bob@thestuff.net>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace MonoTests.System {

	[TestFixture]
	public class RandomTest : Assertion {

		[Test]
		public void NextDouble ()
		{
			Random r = new Random ();
			int i;
			double c=0;
			for (i=0; i<20; i++) 
				c += r.NextDouble ();
			c/=i;
			Assert (c.ToString () + " is out of range.", c < .7 && c > .3);
		}

		[Test]
		public void CompareStreamWithSameSeed ()
		{
			Random r = new Random (42);
			Random r2 = new Random (42);
			double c=0, c2=0;
			for (int i=0; i<20; i++) {
				c += r.NextDouble ();
				c2 += r2.NextDouble ();
			}
			AssertEquals ("Compare", c, c2);
		}

		[Test]
		public void Next ()
		{
			Random r = new Random ();
			for (int i=0; i<20; i++) {
				long c = r.Next ();
				Assert ("Next(" + i + ")", c < Int32.MaxValue && c >= 0);
			}
		}

		[Test]
		public void NextMax()
		{
			Random r = new Random();
			for (int i=0; i<20; i++) {
				long c = r.Next (10);
				Assert ("NextMax(" + i + ")", c < 10 && c >= 0);
			}
		}

		[Test]
		public void NextMinMax()
		{
			Random r = new Random ();
			AssertEquals ("#1 Failed where min == max", 42, r.Next (42, 42));
			AssertEquals ("#2 Failed where min == max", Int32.MaxValue, r.Next (Int32.MaxValue, Int32.MaxValue));
			AssertEquals ("#3 Failed where min == max", Int32.MinValue, r.Next (Int32.MinValue, Int32.MinValue));
			AssertEquals ("#4 Failed where min == max", 0, r.Next (0, 0));
			for (int i = 1; i <= Int32.MaxValue / 2; i *= 2) {
				long c = r.Next (i, i * 2);
				Assert ("At i=" + i + " c < i*2 failed", c < i * 2);
				Assert ("At i=" + i + " c >= i failed", c >= i);
			}
			for (int i = -1; i >= Int32.MinValue / 2; i *= 2) {
				long c = r.Next (i * 2, i);
				Assert ("At i=" + i + " c < i*2 failed", c < i);
				Assert ("At i=" + i + " c >= i failed", c >= i * 2);
			}
		}

/* Mono implementation is now compatible with Knuth (not MS) implementation (choice of constants)
		[Test]
		public void CompareWithMS () 
		{
			string[] r = new string [4];
			byte[] buffer = new byte [8];
			int x = 4;
			while (x-- > 0) {
				int seed = (x << x);
				Random random = new Random (seed);
				random.NextBytes (buffer);
				r [x] = BitConverter.ToString (buffer);
			}
			AssertEquals ("Seed(24)", "43-DB-8B-AE-0A-88-A8-7B", r [3]);
			AssertEquals ("Seed(8)", "E7-2A-5C-44-D1-8C-7D-74", r [2]);
			AssertEquals ("Seed(2)", "C5-67-2A-FC-1B-4E-CD-72", r [1]);
			AssertEquals ("Seed(0)", "B9-D1-C4-8E-34-8F-E7-71", r [0]);
		}*/
	}
}
