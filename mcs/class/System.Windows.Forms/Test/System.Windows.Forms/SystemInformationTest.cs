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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//


using System;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class SystemInformationTest : TestHelper
	{

		[Test]
		public void IconSizeTest ()
    		{
			Size expected = new Size (32, 32);
			Assert.AreEqual (expected, SystemInformation.IconSize, "#1");
		}

		[Test]
		public void IconSpacingTest ()
		{
			Size expected = new Size (75, 75);
			Assert.AreEqual (expected, SystemInformation.IconSpacingSize, "#1");
		}

		[Test]
		public void MenuFont_Clone ()
		{
			Font mf1 = SystemInformation.MenuFont;
			Font mf2 = SystemInformation.MenuFont;
			Assert.IsFalse (Object.ReferenceEquals (mf1, mf2), "ReferenceEquals");
			// yep, it's a clone
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MenuFont_Dispose ()
		{
			Font mf = SystemInformation.MenuFont;
			mf.Dispose ();
			Assert.AreEqual (SystemInformation.MenuFont.Height, mf.Height, "Height");
			// Font.Height can't be accessed after Dispose (see Font unit tests)
		}
	}
}
