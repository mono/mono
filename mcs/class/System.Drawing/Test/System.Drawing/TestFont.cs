//
// Test Font class testing unit
//
// Author:
//
// 	 Jordi Mas i Hernandez, jordi@ximian.com
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
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
