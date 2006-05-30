//
// Test Font class testing unit
//
// Author:
//
// 	 Jordi Mas i Hernandez, jordi@ximian.com
// 	 Peter Dennis Bartok, pbartok@novell.com
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.Drawing.Text;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace MonoTests.System.Drawing{

	[TestFixture]
	public class FontTest {

		private string name;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			using (FontFamily ff = new FontFamily (GenericFontFamilies.Monospace)) {
				name = ff.Name;
			}
		}

		// Test basic Font clone, properties and contructor
		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		public void TestClone()
		{		
			Font f = new Font("Arial",12);	
			Font f2 = (Font) f.Clone();
			
			Assert.AreEqual (f.Bold, f2.Bold, "Bold");
			Assert.AreEqual (f.FontFamily, f2.FontFamily, "FontFamily");
			Assert.AreEqual (f.GdiCharSet, f2.GdiCharSet, "GdiCharSet");
			Assert.AreEqual (f.GdiVerticalFont, f2.GdiVerticalFont, "GdiVerticalFont");
			Assert.AreEqual (f.Height, f2.Height, "Height");
			Assert.AreEqual (f.Italic, f2.Italic, "Italic");
			Assert.AreEqual (f.Name, f2.Name, "Name");
			Assert.AreEqual (f.Size, f2.Size, "Size");
			Assert.AreEqual (f.SizeInPoints, f2.SizeInPoints, "SizeInPoints");
			Assert.AreEqual (f.Strikeout, f2.Strikeout, "Strikeout");
			Assert.AreEqual (f.Style, f2.Style, "Style");
			Assert.AreEqual (f.Underline, f2.Underline, "Underline");
			Assert.AreEqual (f.Unit, f2.Unit, "Unit");
		}

		[ StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto) ]
		class LOGFONT {
			public int lfHeight = 0;
			public int lfWidth = 0;
			public int lfEscapement = 0;
			public int lfOrientation = 0;
			public int lfWeight = 0;
			public byte lfItalic = 0;
			public byte lfUnderline = 0;
			public byte lfStrikeOut = 0;
			public byte lfCharSet = 0;
			public byte lfOutPrecision = 0;
			public byte lfClipPrecision = 0;
			public byte lfQuality = 0;
			public byte lfPitchAndFamily = 0;
			[ MarshalAs(UnmanagedType.ByValTStr, SizeConst=32) ]
			public string lfFaceName = null;
		}
#if !TARGET_JVM
		[Test]
		[Category ("CAS")]
		[ExpectedException (typeof (SecurityException))]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		public void ToLogFont_DenyUnmanagedCode ()
		{
			Font f;
			LOGFONT	lf;

			lf = new LOGFONT();
			f = new Font("Arial", 10);

			f.ToLogFont(lf);
		}

		[Test]
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public void ToLogFont_AssertUnmanagedCode ()
		{
			Font f;
			LOGFONT	lf;

			lf = new LOGFONT();
			f = new Font("Arial", 10);

			f.ToLogFont(lf);
			Assert.AreEqual (-13, lf.lfHeight, "lfHeight");
			Assert.AreEqual (400, lf.lfWeight, "lfWeight");
			Assert.AreEqual (1, lf.lfCharSet, "lfCharSet");
			Assert.AreEqual (f.Name, lf.lfFaceName, "lfFaceName");
		}
#endif
		[Test]
#if ONLY_1_1
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void Font_StringNull_Float ()
		{
			string family = null;
			Font f = new Font (family, 12.5f);
			Assert.AreEqual (FontFamily.GenericSansSerif, f.FontFamily, "FontFamily");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (12.5f, f.SizeInPoints, "SizeInPoints");
			Assert.AreEqual (GraphicsUnit.Point, f.Unit, "Unit");
		}

		[Test]
		public void Font_String_Float ()
		{
			Font f = new Font (name, 12.5f);
			Assert.AreEqual (FontFamily.GenericMonospace, f.FontFamily, "FontFamily");
			Assert.IsFalse (f.Bold, "Bold");
			Assert.AreEqual (1, f.GdiCharSet, "GdiCharSet");
			Assert.IsFalse (f.GdiVerticalFont, "GdiVerticalFont");
			Assert.IsTrue (f.Height > 0, "Height");
			Assert.IsFalse (f.Italic, "Italic");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (12.5f, f.SizeInPoints, "SizeInPoints");
			Assert.IsFalse (f.Strikeout, "Strikeout");
			Assert.IsFalse (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Point, f.Unit, "Unit");
		}

		[Test]
		public void Font_String_Float_FontStyle ()
		{
			Font f = new Font (name, 12.5f, FontStyle.Bold);
			Assert.AreEqual (FontFamily.GenericMonospace, f.FontFamily, "FontFamily");
			Assert.IsTrue (f.Bold, "Bold");
			Assert.AreEqual (1, f.GdiCharSet, "GdiCharSet");
			Assert.IsFalse (f.GdiVerticalFont, "GdiVerticalFont");
			Assert.IsTrue (f.Height > 0, "Height");
			Assert.IsFalse (f.Italic, "Italic");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (12.5f, f.SizeInPoints, "SizeInPoints");
			Assert.IsFalse (f.Strikeout, "Strikeout");
			Assert.IsFalse (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Point, f.Unit, "Unit");
		}

		[Test]
		public void Font_String_Float_FontStyle_GraphicsUnit ()
		{
			Font f = new Font (name, 12.5f, FontStyle.Italic, GraphicsUnit.Pixel);
			Assert.IsFalse (f.Bold, "Bold");
			Assert.AreEqual (1, f.GdiCharSet, "GdiCharSet");
			Assert.IsFalse (f.GdiVerticalFont, "GdiVerticalFont");
			Assert.IsTrue (f.Height > 0, "Height");
			Assert.IsTrue (f.Italic, "Italic");
			Assert.AreEqual (FontFamily.GenericMonospace, f.FontFamily, "FontFamily");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (9.375f, f.SizeInPoints, "SizeInPoints");
			Assert.IsFalse (f.Strikeout, "Strikeout");
			Assert.IsFalse (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Pixel, f.Unit, "Unit");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Font_String_Float_FontStyle_GraphicsUnit_Display ()
		{
			new Font (name, 12.5f, FontStyle.Italic, GraphicsUnit.Display);
		}

		[Test]
		public void Font_String_Float_FontStyle_GraphicsUnit_Byte ()
		{
			Font f = new Font (name, 12.5f, FontStyle.Strikeout, GraphicsUnit.Inch, Byte.MaxValue);
			Assert.IsFalse (f.Bold, "Bold");
			Assert.AreEqual (Byte.MaxValue, f.GdiCharSet, "GdiCharSet");
			Assert.IsFalse (f.GdiVerticalFont, "GdiVerticalFont");
			Assert.IsTrue (f.Height > 0, "Height");
			Assert.IsFalse (f.Italic, "Italic");
			Assert.AreEqual (FontFamily.GenericMonospace, f.FontFamily, "FontFamily");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (900f, f.SizeInPoints, "SizeInPoints");
			Assert.IsTrue (f.Strikeout, "Strikeout");
			Assert.IsFalse (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Inch, f.Unit, "Unit");
		}

		[Test]
		public void Font_String_Float_FontStyle_GraphicsUnit_Byte_Bool ()
		{
			Font f = new Font (name, 12.5f, FontStyle.Underline, GraphicsUnit.Document, Byte.MinValue, true);
			Assert.IsFalse (f.Bold, "Bold");
			Assert.AreEqual (Byte.MinValue, f.GdiCharSet, "GdiCharSet");
			Assert.IsTrue (f.GdiVerticalFont, "GdiVerticalFont");
			Assert.IsTrue (f.Height > 0, "Height");
			Assert.IsFalse (f.Italic, "Italic");
			Assert.AreEqual (FontFamily.GenericMonospace, f.FontFamily, "FontFamily");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (3f, f.SizeInPoints, "SizeInPoints");
			Assert.IsFalse (f.Strikeout, "Strikeout");
			Assert.IsTrue (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Document, f.Unit, "Unit");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Font_FontFamilyNull_Float ()
		{
			FontFamily ff = null;
			new Font (ff, 12.5f);
		}

		[Test]
		public void Font_FontFamily_Float ()
		{
			Font f = new Font (FontFamily.GenericMonospace, 12.5f);
			Assert.AreEqual (FontFamily.GenericMonospace, f.FontFamily, "FontFamily");
			Assert.IsFalse (f.Bold, "Bold");
			Assert.AreEqual (1, f.GdiCharSet, "GdiCharSet");
			Assert.IsFalse (f.GdiVerticalFont, "GdiVerticalFont");
			Assert.IsTrue (f.Height > 0, "Height");
			Assert.IsFalse (f.Italic, "Italic");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (12.5f, f.SizeInPoints, "SizeInPoints");
			Assert.IsFalse (f.Strikeout, "Strikeout");
			Assert.IsFalse (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Point, f.Unit, "Unit");
		}

		[Test]
		public void Font_FontFamily_Float_FontStyle ()
		{
			Font f = new Font (FontFamily.GenericMonospace, 12.5f, FontStyle.Bold);
			Assert.AreEqual (FontFamily.GenericMonospace, f.FontFamily, "FontFamily");
			Assert.IsTrue (f.Bold, "Bold");
			Assert.AreEqual (1, f.GdiCharSet, "GdiCharSet");
			Assert.IsFalse (f.GdiVerticalFont, "GdiVerticalFont");
			Assert.IsTrue (f.Height > 0, "Height");
			Assert.IsFalse (f.Italic, "Italic");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (12.5f, f.SizeInPoints, "SizeInPoints");
			Assert.IsFalse (f.Strikeout, "Strikeout");
			Assert.IsFalse (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Point, f.Unit, "Unit");
		}

		[Test]
		public void Font_FontFamily_Float_FontStyle_GraphicsUnit ()
		{
			Font f = new Font (FontFamily.GenericMonospace, 12.5f, FontStyle.Italic, GraphicsUnit.Millimeter);
			Assert.IsFalse (f.Bold, "Bold");
			Assert.AreEqual (1, f.GdiCharSet, "GdiCharSet");
			Assert.IsFalse (f.GdiVerticalFont, "GdiVerticalFont");
			Assert.IsTrue (f.Height > 0, "Height");
			Assert.IsTrue (f.Italic, "Italic");
			Assert.AreEqual (FontFamily.GenericMonospace, f.FontFamily, "FontFamily");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (35.43307f, f.SizeInPoints, "SizeInPoints");
			Assert.IsFalse (f.Strikeout, "Strikeout");
			Assert.IsFalse (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Millimeter, f.Unit, "Unit");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Font_FontFamily_Float_FontStyle_GraphicsUnit_Display ()
		{
			new Font (FontFamily.GenericMonospace, 12.5f, FontStyle.Italic, GraphicsUnit.Display);
		}

		[Test]
		public void Font_FontFamily_Float_FontStyle_GraphicsUnit_Byte ()
		{
			Font f = new Font (FontFamily.GenericMonospace, 12.5f, FontStyle.Strikeout, GraphicsUnit.Inch, Byte.MaxValue);
			Assert.IsFalse (f.Bold, "Bold");
			Assert.AreEqual (Byte.MaxValue, f.GdiCharSet, "GdiCharSet");
			Assert.IsFalse (f.GdiVerticalFont, "GdiVerticalFont");
			Assert.IsTrue (f.Height > 0, "Height");
			Assert.IsFalse (f.Italic, "Italic");
			Assert.AreEqual (FontFamily.GenericMonospace, f.FontFamily, "FontFamily");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (900f, f.SizeInPoints, "SizeInPoints");
			Assert.IsTrue (f.Strikeout, "Strikeout");
			Assert.IsFalse (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Inch, f.Unit, "Unit");
		}

		[Test]
		public void Font_FontFamily_Float_FontStyle_GraphicsUnit_Byte_Bool ()
		{
			Font f = new Font (FontFamily.GenericMonospace, 12.5f, FontStyle.Underline, GraphicsUnit.Document, Byte.MinValue, true);
			Assert.IsFalse (f.Bold, "Bold");
			Assert.AreEqual (Byte.MinValue, f.GdiCharSet, "GdiCharSet");
			Assert.IsTrue (f.GdiVerticalFont, "GdiVerticalFont");
			Assert.IsTrue (f.Height > 0, "Height");
			Assert.IsFalse (f.Italic, "Italic");
			Assert.AreEqual (FontFamily.GenericMonospace, f.FontFamily, "FontFamily");
			Assert.AreEqual (f.Name, f.FontFamily.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
			Assert.AreEqual (3f, f.SizeInPoints, "SizeInPoints");
			Assert.IsFalse (f.Strikeout, "Strikeout");
			Assert.IsTrue (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Document, f.Unit, "Unit");
		}

		[Test]
		public void Dispose_Double ()
		{
			Font f = new Font (name, 12.5f);
			f.Dispose ();
			f.Dispose ();
		}

		[Test]
		public void Dispose_UseAfter_Works ()
		{
			Font f = new Font (name, 12.5f);
			string fname = f.Name;
			f.Dispose ();
			// most properties don't throw, everything seems to be cached
			Assert.AreEqual (fname, f.Name, "Name");
			Assert.AreEqual (12.5f, f.Size, "Size");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Dispose_Height ()
		{
			Font f = new Font (name, 12.5f);
			f.Dispose ();
			Assert.AreEqual (0, f.Height, "Name");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Dispose_ToLogFont ()
		{
			Font f = new Font (name, 12.5f);
			f.Dispose ();
			LOGFONT	lf = new LOGFONT();
			f.ToLogFont (lf);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Dispose_ToHFont ()
		{
			Font f = new Font (name, 12.5f);
			f.Dispose ();
			f.ToHfont ();
		}
	}
}
