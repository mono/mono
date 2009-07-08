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
	public class PaperSourceTest
	{
#if NET_2_0
		[Test]
		public void KindTest ()
		{
			PaperSource ps = new PaperSource ();

			//
			// Set Custom
			ps.RawKind = (int)PaperSourceKind.Custom;
			Assert.AreEqual (PaperSourceKind.Custom, ps.Kind, "Kind #8");
			Assert.AreEqual (257, ps.RawKind, "RawKind #8");

			//
			// An integer value of 256 and above returns Custom (0x257)
			ps.RawKind = 256;
			Assert.AreEqual (256, ps.RawKind, "out: #" + 256);
			Assert.AreEqual (PaperSourceKind.Custom, ps.Kind, "kind is custom: #" + 256);

			//
			// Zero
			ps.RawKind = 0;
			Assert.AreEqual ((PaperSourceKind)0, ps.Kind, "Kind #1");
			Assert.AreEqual (0, ps.RawKind, "RawKind #1");

			//
			// Well-known
			ps.RawKind = (int)PaperSourceKind.Upper;
			Assert.AreEqual (PaperSourceKind.Upper, ps.Kind, "Kind #2");
			Assert.AreEqual ((int)PaperSourceKind.Upper, ps.RawKind, "RawKind #2");

			//
			ps.RawKind = (int)PaperSourceKind.FormSource;
			Assert.AreEqual (PaperSourceKind.FormSource, ps.Kind, "Kind #3");
			Assert.AreEqual ((int)PaperSourceKind.FormSource, ps.RawKind, "RawKind #3");

			//
			// Too Big
			ps.RawKind = 999999;
			Assert.AreEqual (PaperSourceKind.Custom, ps.Kind, "Kind #4");
			Assert.AreEqual (999999, ps.RawKind, "RawKind #4");

			//
			ps.RawKind = int.MaxValue;
			Assert.AreEqual (PaperSourceKind.Custom, ps.Kind, "Kind #5");
			Assert.AreEqual (int.MaxValue, ps.RawKind, "RawKind #5");

			//
			// Negative -- Looks as if MSFT forgot to check for negative!
			ps.RawKind = -1;
			Assert.AreEqual ((PaperSourceKind)(-1), ps.Kind, "Kind #6");
			Assert.AreEqual (-1, ps.RawKind, "RawKind #6");

			//
			ps.RawKind = int.MinValue;
			Assert.AreEqual ((PaperSourceKind)(int.MinValue), ps.Kind, "Kind #7");
			Assert.AreEqual (int.MinValue, ps.RawKind, "RawKind #7");
		}
#endif

	}

}

