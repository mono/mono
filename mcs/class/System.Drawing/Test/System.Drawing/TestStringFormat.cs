//
// StringFormatFlags class testing unit
//
// Author:
//
// 	 Jordi Mas i Hernàndez (jordi@ximian.com)
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
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class StringFormatTest {
		
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
		
		}
		
		[Test]
		public void TestSpecialConstructors() 
		{				
			StringFormat smf = StringFormat.GenericDefault;			
			smf = StringFormat.GenericTypographic;											
		}	
		
		[Test]
		public void TestClone() 
		{						
			StringFormat smf = new StringFormat();						
			StringFormat smfclone = (StringFormat) smf.Clone();			
			
			Assert.AreEqual (smf.LineAlignment, smfclone.LineAlignment);			
			Assert.AreEqual (smf.FormatFlags, smfclone.FormatFlags);			
			Assert.AreEqual (smf.LineAlignment, smfclone.LineAlignment);			
			Assert.AreEqual (smf.Alignment, smfclone.Alignment);			
			Assert.AreEqual (smf.Trimming, smfclone.Trimming);			
		}
			
		[Test]
		public void TestAlignment() 
		{					
			StringFormat	smf = new StringFormat ();
			
			smf.LineAlignment = StringAlignment.Center;									
			Assert.AreEqual (StringAlignment.Center, smf.LineAlignment);			
			
			smf.Alignment = StringAlignment.Far;									
			Assert.AreEqual (StringAlignment.Far, smf.Alignment);						 
		}		
			
		[Test]
		public void TestFormatFlags() 
		{				
			StringFormat	smf = new StringFormat ();
			
			smf.FormatFlags = StringFormatFlags.DisplayFormatControl;									
			Assert.AreEqual (StringFormatFlags.DisplayFormatControl, smf.FormatFlags);						 
		}		
		
		[Test]
		public void TabsStops() 
		{				
			StringFormat	smf = new StringFormat ();
			
			float firstTabOffset;			
			float[] tabsSrc = {100, 200, 300, 400};
			float[] tabStops;
			
			smf.SetTabStops(200, tabsSrc);
			tabStops = smf.GetTabStops(out firstTabOffset);
			
			Assert.AreEqual (200, firstTabOffset);						 
			Assert.AreEqual (tabsSrc.Length, tabStops.Length);						 
			Assert.AreEqual (tabsSrc[0], tabStops[0]);					
			Assert.AreEqual (tabsSrc[1], tabStops[1]);					
			Assert.AreEqual (tabsSrc[2], tabStops[2]);					
			Assert.AreEqual (tabsSrc[3], tabStops[3]);					
		}	
		
	}
}
