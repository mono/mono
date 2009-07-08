//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
// Author:
// 	Andy Hume <andyhume32@yahoo.co.uk>
//

using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Printing;

namespace MonoTests.System.Drawing.Printing
{
	[TestFixture]
	public class PaperSizeTest
	{
#if NET_2_0
		[Test]
		public void PaperSizeKindTest()
		{
			// set_RawKind seems to accept any value (no ArgEx seen), but get_Kind 
			// returns "Custom" when it's set to a value bigger than the biggest enum.
			//
			PaperSize ps = new PaperSize ("foo", 100, 100);

			//
			// Zero == Custom
			Assert.AreEqual(PaperKind.Custom, ps.Kind, "Kind #1");
			Assert.AreEqual(0, ps.RawKind, "RawKind #1");

			try {
				ps.Height = 1;
				Assert.AreEqual (1 , ps.Height, "get_Height #1");
			} catch (ArgumentException) {
				Assert.Fail ("should not have thrown #1");
			}

			//
			// Well-known
			ps.RawKind = (int)PaperKind.A4;
			Assert.AreEqual (PaperKind.A4, ps.Kind, "Kind #2");
			Assert.AreEqual ((int)PaperKind.A4, ps.RawKind, "RawKind #2");

			try {
				ps.Height = 2;
				Assert.Fail("should have thrown #2");
			} catch (ArgumentException) {
			}

			//
			ps.RawKind = (int)PaperKind.JapaneseEnvelopeKakuNumber3;
			Assert.AreEqual (PaperKind.JapaneseEnvelopeKakuNumber3, ps.Kind, "Kind #3");
			Assert.AreEqual ((int)PaperKind.JapaneseEnvelopeKakuNumber3, ps.RawKind, "RawKind #3");

			//
			// Too Big
			ps.RawKind = 999999;
			Assert.AreEqual (PaperKind.Custom, ps.Kind, "Kind #4");
			Assert.AreEqual (999999, ps.RawKind, "RawKind #4");

			// The properties can be changed only when the *real* Kind is Custom 
			// and not when is 'effectively' Custom.
			try {
				ps.Height = 4;
				Assert.Fail("should have thrown #4");
			} catch (ArgumentException) {
			}

			//
			ps.RawKind = int.MaxValue;
			Assert.AreEqual (PaperKind.Custom, ps.Kind, "Kind #5");
			Assert.AreEqual (int.MaxValue, ps.RawKind, "RawKind #5");

			//
			// Negative -- Looks as if MSFT forgot to check for negative!
			ps.RawKind = -1;
			Assert.AreEqual ((PaperKind)(-1), ps.Kind, "Kind #6");
			Assert.AreEqual (-1, ps.RawKind, "RawKind #6");

			//
			ps.RawKind = int.MinValue;
			Assert.AreEqual ((PaperKind)(int.MinValue), ps.Kind, "Kind #7");
			Assert.AreEqual (int.MinValue, ps.RawKind, "RawKind #7");

			//
			// Where's the top limit?
			ps.RawKind = (int)PaperKind.PrcEnvelopeNumber10Rotated;
			Assert.AreEqual (PaperKind.PrcEnvelopeNumber10Rotated, ps.Kind, "Kind #8");
			Assert.AreEqual ((int)PaperKind.PrcEnvelopeNumber10Rotated, ps.RawKind, "RawKind #8");

			// +1
			ps.RawKind = 1 + (int)PaperKind.PrcEnvelopeNumber10Rotated;
			Assert.AreEqual (PaperKind.Custom, ps.Kind, "Kind #9");
			Assert.AreEqual (1 + (int)PaperKind.PrcEnvelopeNumber10Rotated, ps.RawKind, "RawKind #9");

			try {
				ps.Height = 9;
				Assert.Fail("should have thrown #9");
			} catch (ArgumentException) {
			}

			// Set Custom
			ps.RawKind = (int)PaperKind.Custom;
			Assert.AreEqual (PaperKind.Custom, ps.Kind, "Kind #1b");
			Assert.AreEqual (0, ps.RawKind, "RawKind #1b");

			try {
				ps.Height = 1;
				Assert.AreEqual (1 , ps.Height, "get_Height #1b");
			} catch (ArgumentException) {
				Assert.Fail ("should not have thrown #1b");
			}
		}
#endif
	
	}
}

