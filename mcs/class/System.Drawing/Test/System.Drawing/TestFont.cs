//
// Test Font class testing unit
//
// Author:
//
// 	 Jordi Mas i Hernandez, jordi@ximian.com
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
//

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
using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	public class FontTest : Assertion {
		
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
		
		}
			
		// Test basic Font clone, properties and contructor
		[Test]
		public void TestClone()
		{		
			Font f = new Font("Arial",12);	
			Font f2 = (Font) f.Clone();
			
			AssertEquals (f.Bold, f2.Bold);
			AssertEquals (f.FontFamily, f2.FontFamily);
			AssertEquals (f.GdiCharSet, f2.GdiCharSet);
			AssertEquals (f.GdiVerticalFont, f2.GdiVerticalFont);
			AssertEquals (f.Height, f2.Height);
			AssertEquals (f.Italic, f2.Italic);
			AssertEquals (f.Name, f2.Name);
			AssertEquals (f.Size, f2.Size);
			AssertEquals (f.SizeInPoints, f2.SizeInPoints);
			AssertEquals (f.Strikeout, f2.Strikeout);
			AssertEquals (f.Style, f2.Style);
			AssertEquals (f.Underline, f2.Underline);
			AssertEquals (f.Unit, f2.Unit);
		}
		
	}
}
