//
// Tests for System.Drawing.SystemFontsTest
//
// Authors:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	public class SystemFontsTest {

		// avoid lots of failures if no fonts are available (e.g. headless systems)
		static bool font_available;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			try {
				Font f = SystemFonts.DefaultFont;
				font_available = true;
			}
			catch (ArgumentException) {
				font_available = false;
			}
		}

		[SetUp]
		public void SetUp ()
		{
			if (!font_available)
				Assert.Ignore ("No font family could be found.");
		}

		[Test]
		public void DefaultFont ()
		{
			Font f = SystemFonts.DefaultFont;
			Assert.IsFalse (f.Bold, "#1");

			Assert.AreEqual (true, f.IsSystemFont, "#3");
			Assert.IsFalse (f.Italic, "#4");
			Assert.AreEqual (8.25, f.Size, 0.01, "#6");
			Assert.AreEqual (8.25, f.SizeInPoints, 0.01, "#7");
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

		[Test]
		public void SystemFontName ()
		{
			Assert.AreEqual ("CaptionFont", SystemFonts.CaptionFont.SystemFontName, "CaptionFont");
			Assert.AreEqual ("DefaultFont", SystemFonts.DefaultFont.SystemFontName, "DefaultFont");
			Assert.AreEqual ("DialogFont", SystemFonts.DialogFont.SystemFontName, "DialogFont");
			Assert.AreEqual ("IconTitleFont", SystemFonts.IconTitleFont.SystemFontName, "IconTitleFont");
			Assert.AreEqual ("MenuFont", SystemFonts.MenuFont.SystemFontName, "MenuFont");
			Assert.AreEqual ("MessageBoxFont", SystemFonts.MessageBoxFont.SystemFontName, "MessageBoxFont");
			Assert.AreEqual ("SmallCaptionFont", SystemFonts.SmallCaptionFont.SystemFontName, "SmallCaptionFont");
			Assert.AreEqual ("StatusFont", SystemFonts.StatusFont.SystemFontName, "StatusFont");
		}

		[Test]
		public void GetFontByName ()
		{
			Assert.AreEqual ("CaptionFont", SystemFonts.GetFontByName ("CaptionFont").SystemFontName, "CaptionFont");
			Assert.AreEqual ("DefaultFont", SystemFonts.GetFontByName ("DefaultFont").SystemFontName, "DefaultFont");
			Assert.AreEqual ("DialogFont", SystemFonts.GetFontByName ("DialogFont").SystemFontName, "DialogFont");
			Assert.AreEqual ("IconTitleFont", SystemFonts.GetFontByName ("IconTitleFont").SystemFontName, "IconTitleFont");
			Assert.AreEqual ("MenuFont", SystemFonts.GetFontByName ("MenuFont").SystemFontName, "MenuFont");
			Assert.AreEqual ("MessageBoxFont", SystemFonts.GetFontByName ("MessageBoxFont").SystemFontName, "MessageBoxFont");
			Assert.AreEqual ("SmallCaptionFont", SystemFonts.GetFontByName ("SmallCaptionFont").SystemFontName, "SmallCaptionFont");
			Assert.AreEqual ("StatusFont", SystemFonts.GetFontByName ("StatusFont").SystemFontName, "StatusFont");
		}

		[Test]
		public void GetFontByName_Invalid ()
		{
			Assert.IsNull (SystemFonts.GetFontByName (null), "null");
			Assert.IsNull (SystemFonts.GetFontByName (String.Empty), "Empty");
			Assert.IsNull (SystemFonts.GetFontByName ("defaultfont"), "lowercase");
			Assert.IsNull (SystemFonts.GetFontByName ("DEFAULTFONT"), "UPPERCASE");
		}

		[Test]
		public void Same ()
		{
			Font f1 = SystemFonts.CaptionFont;
			Font f2 = SystemFonts.CaptionFont;
			Assert.IsFalse (Object.ReferenceEquals (f1, f2), "property-property");
			f2 = SystemFonts.GetFontByName ("CaptionFont");
			Assert.IsFalse (Object.ReferenceEquals (f1, f2), "property-GetFontByName");
		}

		[Test]
		public void Dispose_Instance ()
		{
			Font f1 = SystemFonts.CaptionFont;
			float height = f1.GetHeight (72f);
			f1.Dispose ();
			Assert.Throws<ArgumentException> (() => f1.GetHeight (72f));
		}

		[Test]
		public void Dispose_Property ()
		{
			float height = SystemFonts.CaptionFont.GetHeight (72f);
			SystemFonts.CaptionFont.Dispose ();
			Assert.AreEqual (height, SystemFonts.CaptionFont.GetHeight (72f), "height");
		}
	}
}

