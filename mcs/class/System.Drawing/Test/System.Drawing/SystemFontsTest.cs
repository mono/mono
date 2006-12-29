//
// Tests for System.Drawing.SystemFontsTest.cs 
//
// Authors:
//	Gert Driesen  <drieseng@users.sourceforge.net>
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

#if NET_2_0
using System;
using System.Drawing;

using NUnit.Framework;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	public class SystemFontsTest
	{
		[Test]
		public void DefaultFont ()
		{
			Font f = SystemFonts.DefaultFont;
			Assert.IsFalse (f.Bold, "#1");

			Assert.AreEqual (true, f.IsSystemFont, "#3");
			Assert.IsFalse (f.Italic, "#4");
			Assert.AreEqual (8.25, f.Size, "#6");
			Assert.AreEqual (8.25, f.SizeInPoints, "#7");
			Assert.IsFalse (f.Strikeout, "#8");
			Assert.IsFalse (f.Underline, "#9");
			Assert.AreEqual (GraphicsUnit.Point, f.Unit, "#10");
		}

		[Test]
		[Category ("NotWorking")] // on Unix mapping is done to Bitstream Vera Sans
		public void DefaultFont_Names ()
		{
			Font f = SystemFonts.DefaultFont;
			Assert.AreEqual ("Microsoft Sans Serif", f.FontFamily.Name, "#1");
			Assert.AreEqual ("Microsoft Sans Serif", f.Name, "#2");
		}
	}
}
#endif
