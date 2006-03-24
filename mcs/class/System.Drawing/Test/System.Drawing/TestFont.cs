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
using System.Drawing.Imaging;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace MonoTests.System.Drawing{

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class FontTest {
		
		
		// Test basic Font clone, properties and contructor
		[Test]
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
		public void TestToLogFont() {
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
	}
}
