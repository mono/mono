//
// Test Font class testing unit
//
// Authors:
// 	Jordi Mas i Hernandez, jordi@ximian.com
// 	Peter Dennis Bartok, pbartok@novell.com
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2007 Novell, Inc (http://www.novell.com)
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
			try {
				using (FontFamily ff = new FontFamily (GenericFontFamilies.Monospace)) {
					name = ff.Name;
				}
			}
			catch (ArgumentException) {
				Assert.Ignore ("No font family could be found.");
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

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		class LOGFONT {
			public int lfHeight;
			public int lfWidth;
			public int lfEscapement;
			public int lfOrientation;
			public int lfWeight;
			public byte lfItalic;
			public byte lfUnderline;
			public byte lfStrikeOut;
			public byte lfCharSet;
			public byte lfOutPrecision;
			public byte lfClipPrecision;
			public byte lfQuality;
			public byte lfPitchAndFamily;
			[ MarshalAs(UnmanagedType.ByValTStr, SizeConst=32) ]
			public string lfFaceName;
		}

		[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct LOGFONT_STRUCT {
			public int lfHeight;
			public int lfWidth;
			public int lfEscapement;
			public int lfOrientation;
			public int lfWeight;
			public byte lfItalic;
			public byte lfUnderline;
			public byte lfStrikeOut;
			public byte lfCharSet;
			public byte lfOutPrecision;
			public byte lfClipPrecision;
			public byte lfQuality;
			public byte lfPitchAndFamily;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 32)]
			public string lfFaceName;
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
			Font f = new Font("Arial", 10);
			LOGFONT	lf = new LOGFONT();

			f.ToLogFont (lf);
			Assert.AreEqual (400, lf.lfWeight, "lfWeight");
			Assert.AreEqual (1, lf.lfCharSet, "lfCharSet");
			Assert.AreEqual (f.Name, lf.lfFaceName, "lfFaceName");

			LOGFONT_STRUCT lfs = new LOGFONT_STRUCT ();
			f.ToLogFont (lfs);
			Assert.AreEqual (0, lfs.lfWeight, "struct-lfWeight");
			Assert.AreEqual (0, lfs.lfCharSet, "struct-lfCharSet");
			Assert.AreEqual (0, lfs.lfHeight, "struct-lfHeight");
			Assert.AreEqual (0, lfs.lfWidth, "struct-lfWidth");
			Assert.AreEqual (0, lfs.lfEscapement, "struct-lfEscapement");
			Assert.AreEqual (0, lfs.lfOrientation, "struct-lfOrientation");
			Assert.AreEqual (0, lfs.lfWeight, "struct-lfWeight");
			Assert.AreEqual (0, lfs.lfItalic, "struct-lfItalic");
			Assert.AreEqual (0, lfs.lfUnderline, "struct-lfUnderline");
			Assert.AreEqual (0, lfs.lfStrikeOut, "struct-lfStrikeOut");
			Assert.AreEqual (0, lfs.lfCharSet, "struct-lfCharSet");
			Assert.AreEqual (0, lfs.lfOutPrecision, "struct-lfOutPrecision");
			Assert.AreEqual (0, lfs.lfClipPrecision, "struct-lfClipPrecision");
			Assert.AreEqual (0, lfs.lfQuality, "struct-lfQuality");
			Assert.AreEqual (0, lfs.lfPitchAndFamily, "struct-lfPitchAndFamily");
			Assert.IsNull (lfs.lfFaceName, "struct-lfFaceName");
		}

		[Test]
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		[ExpectedException (typeof (ArgumentException))]
		public void ToLogFont_TooSmall ()
		{
			Font f = new Font ("Arial", 10);
			object o = new object ();
			f.ToLogFont (o);
			// no PInvoke conversion exists !?!?
		}

		[Test]
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public void ToLogFont_Int ()
		{
			Font f = new Font ("Arial", 10);
			int i = 1;
			f.ToLogFont (i);
			Assert.AreEqual (1, i);
		}

		[Test]
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
	#if NET_2_0
		[ExpectedException (typeof (AccessViolationException))]
	#else
		[ExpectedException (typeof (NullReferenceException))]
	#endif
		public void ToLogFont_Null ()
		{
			Font f = new Font ("Arial", 10);
			f.ToLogFont (null);
		}
#endif
		[Test]
#if ONLY_1_1
		[ExpectedException (typeof (ArgumentNullException))]
#endif
#if TARGET_JVM
		[Category ("NotWorking")]
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
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
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
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
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
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
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
			Assert.IsFalse (f.Strikeout, "Strikeout");
			Assert.IsFalse (f.Underline, "Underline");
			Assert.AreEqual (GraphicsUnit.Pixel, f.Unit, "Unit");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Font_String_Float_FontStyle_GraphicsUnit_Display ()
		{
			new Font (name, 12.5f, FontStyle.Italic, GraphicsUnit.Display);
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
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
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
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
		[ExpectedException (typeof (NullReferenceException))]
		public void Font_FontNull_FontStyle ()
		{
			Font f = null;
			new Font (f, FontStyle.Bold);
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
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
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
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
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
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
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Font_FontFamily_Float_FontStyle_GraphicsUnit_Display ()
		{
			new Font (FontFamily.GenericMonospace, 12.5f, FontStyle.Italic, GraphicsUnit.Display);
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
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
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
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
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Dispose_Height ()
		{
			Font f = new Font (name, 12.5f);
			f.Dispose ();
			Assert.AreEqual (0, f.Height, "Name");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Dispose_ToLogFont ()
		{
#if !TARGET_JVM
			Font f = new Font (name, 12.5f);
			f.Dispose ();
			LOGFONT	lf = new LOGFONT();
			f.ToLogFont (lf);
#endif
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Dispose_ToLogFont_LoopCharSet ()
		{
#if !TARGET_JVM
			Font f = new Font (name, 12.5f);
			f.Dispose ();
			LOGFONT lf = new LOGFONT ();

			for (int i = Byte.MinValue; i < Byte.MaxValue; i++) {
				byte b = (byte) i;
				lf.lfHeight = b;
				lf.lfWidth = b;
				lf.lfEscapement = b;
				lf.lfOrientation = b;
				lf.lfWeight = b;
				lf.lfItalic = b;
				lf.lfUnderline = b;
				lf.lfStrikeOut = b;
				lf.lfCharSet = b;
				lf.lfOutPrecision = b;
				lf.lfClipPrecision = b;
				lf.lfQuality = b;
				lf.lfPitchAndFamily = b;
				lf.lfFaceName = b.ToString ();
				try {
					f.ToLogFont (lf);
				}
				catch (ArgumentException) {
					Assert.AreEqual (b, lf.lfHeight, "lfHeight");
					Assert.AreEqual (b, lf.lfWidth, "lfWidth");
					Assert.AreEqual (b, lf.lfEscapement, "lfEscapement");
					Assert.AreEqual (b, lf.lfOrientation, "lfOrientation");
					Assert.AreEqual (b, lf.lfWeight, "lfWeight");
					Assert.AreEqual (b, lf.lfItalic, "lfItalic");
					Assert.AreEqual (b, lf.lfUnderline, "lfUnderline");
					Assert.AreEqual (b, lf.lfStrikeOut, "lfStrikeOut");
					// special case for 0
					Assert.AreEqual ((i == 0) ? (byte)1 : b, lf.lfCharSet, "lfCharSet");
					Assert.AreEqual (b, lf.lfOutPrecision, "lfOutPrecision");
					Assert.AreEqual (b, lf.lfClipPrecision, "lfClipPrecision");
					Assert.AreEqual (b, lf.lfQuality, "lfQuality");
					Assert.AreEqual (b, lf.lfPitchAndFamily, "lfPitchAndFamily");
					Assert.AreEqual (b.ToString (), lf.lfFaceName, "lfFaceName");
				}
				catch (Exception e) {
					Assert.Fail ("Unexcepted exception {0} at iteration {1}", e, i);
				}
			}
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Dispose_ToHFont ()
		{
#if !TARGET_JVM
			Font f = new Font (name, 12.5f);
			f.Dispose ();
			f.ToHfont ();
#endif
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void UnavailableStyleException ()
		{
			// Marked NotWorking because it is dependent on what fonts/styles are available
			// on the OS.  This test is written for Windows.
			Font f = new Font ("Monotype Corsiva", 8, FontStyle.Regular);
		}

		[Test]
		public void GetHeight_Float ()
		{
			using (Font f = new Font (name, 12.5f)) {
				Assert.AreEqual (0, f.GetHeight (0), "0");
			}
		}

		[Test]
		public void GetHeight_Graphics ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					using (Font f = new Font (name, 12.5f)) {
						float expected = f.GetHeight (g.DpiY);
						Assert.AreEqual (expected, f.GetHeight (g), 0.01f, "Default");
						g.ScaleTransform (2, 4);
						Assert.AreEqual (expected, f.GetHeight (g), 0.01f, "ScaleTransform");
						g.PageScale = 3;
						Assert.AreEqual (expected, f.GetHeight (g), 0.01f, "PageScale");
					}
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetHeight_Graphics_Null ()
		{
			using (Font f = new Font (name, 12.5f)) {
				Assert.AreEqual (0, f.GetHeight (null), "0");
			}
		}

		[Test]
		public void FontUniqueHashCode ()
		{
			Font f1 = new Font ("Arial", 14);
			Font f2 = new Font ("Arial", 12);
			Font f3 = new Font (f1, FontStyle.Bold);

			Assert.IsFalse (f1.GetHashCode () == f2.GetHashCode (), "1) Fonts with different sizes should have different HashCodes");
			Assert.IsFalse (f1.GetHashCode () == f3.GetHashCode (), "2) Fonts with different styles should have different HashCodes");
		}

        [Test]
        public void GetHashCode_UnitDiffers_HashesNotEqual()
        {
            Font f1 = new Font("DejaVu Sans", 8.25F, GraphicsUnit.Point);
            Font f2 = new Font("DejaVu Sans", 8.25F, GraphicsUnit.Pixel);

            Assert.IsFalse(f1.GetHashCode() == f2.GetHashCode(),
                "Hashcodes should differ if _unit member differs");
        }

        [Test]
        public void GetHashCode_NameDiffers_HashesNotEqual()
        {
            Font f1 = new Font("DejaVu Sans", 8.25F, GraphicsUnit.Point);
            Font f2 = new Font("Liberation Sans", 8.25F, GraphicsUnit.Point);

            Assert.IsFalse(f1.GetHashCode() == f2.GetHashCode(),
                "Hashcodes should differ if _name member differs");
        }

        [Test]
        public void GetHashCode_StyleEqualsGdiCharSet_HashesNotEqual()
        {
            Font f1 = new Font("DejaVu Sans", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            Font f2 = new Font("DejaVu Sans", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(1)));

            Assert.IsFalse(f1.GetHashCode() == f2.GetHashCode(),
                "Hashcodes should differ if _style member differs");
        }
	}
}
