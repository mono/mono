//
// WmfPlaceableFileHeader class testing unit
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class WmfPlaceableFileHeaderTest {

		[Test]
		public void DefaultValues ()
		{
			WmfPlaceableFileHeader wh = new WmfPlaceableFileHeader ();
			Assert.AreEqual (0, wh.BboxBottom, "BboxBottom");
			Assert.AreEqual (0, wh.BboxLeft, "BboxLeft");
			Assert.AreEqual (0, wh.BboxRight, "BboxRight");
			Assert.AreEqual (0, wh.BboxTop, "BboxTop");
			Assert.AreEqual (0, wh.Checksum, "Checksum");
			Assert.AreEqual (0, wh.Hmf, "Hmf");
			Assert.AreEqual (0, wh.Inch, "Inch");
			Assert.AreEqual (unchecked ((int)0x9AC6CDD7), wh.Key, "Key"); // always (from documentation)
			Assert.AreEqual (0, wh.Reserved, "Reserved");
		}

		[Test]
		public void Min ()
		{
			WmfPlaceableFileHeader wh = new WmfPlaceableFileHeader ();
			wh.BboxBottom = short.MinValue;
			Assert.AreEqual (short.MinValue, wh.BboxBottom, "BboxBottom");
			wh.BboxLeft = short.MinValue;
			Assert.AreEqual (short.MinValue, wh.BboxLeft, "BboxLeft");
			wh.BboxRight = short.MinValue;
			Assert.AreEqual (short.MinValue, wh.BboxRight, "BboxRight");
			wh.BboxTop = short.MinValue;
			Assert.AreEqual (short.MinValue, wh.BboxTop, "BboxTop");
			wh.Checksum = short.MinValue;
			Assert.AreEqual (short.MinValue, wh.Checksum, "Checksum");
			wh.Hmf = short.MinValue;
			Assert.AreEqual (short.MinValue, wh.Hmf, "Hmf");
			wh.Inch = short.MinValue;
			Assert.AreEqual (short.MinValue, wh.Inch, "Inch");
			wh.Key = int.MinValue;
			Assert.AreEqual (int.MinValue, wh.Key, "Key");
			wh.Reserved = int.MinValue;
			Assert.AreEqual (int.MinValue, wh.Reserved, "Reserved");
		}

		[Test]
		public void Max ()
		{
			WmfPlaceableFileHeader wh = new WmfPlaceableFileHeader ();
			wh.BboxBottom = short.MaxValue;
			Assert.AreEqual (short.MaxValue, wh.BboxBottom, "BboxBottom");
			wh.BboxLeft = short.MaxValue;
			Assert.AreEqual (short.MaxValue, wh.BboxLeft, "BboxLeft");
			wh.BboxRight = short.MaxValue;
			Assert.AreEqual (short.MaxValue, wh.BboxRight, "BboxRight");
			wh.BboxTop = short.MaxValue;
			Assert.AreEqual (short.MaxValue, wh.BboxTop, "BboxTop");
			wh.Checksum = short.MaxValue;
			Assert.AreEqual (short.MaxValue, wh.Checksum, "Checksum");
			wh.Hmf = short.MaxValue;
			Assert.AreEqual (short.MaxValue, wh.Hmf, "Hmf");
			wh.Inch = short.MaxValue;
			Assert.AreEqual (short.MaxValue, wh.Inch, "Inch");
			wh.Key = int.MaxValue;
			Assert.AreEqual (int.MaxValue, wh.Key, "Key");
			wh.Reserved = int.MaxValue;
			Assert.AreEqual (int.MaxValue, wh.Reserved, "Reserved");
		}
	}
}
