//
// StringFormatFlags class testing unit
//
// Author:
//
// 	 Jordi Mas i Hernàndez (jordi@ximian.com)
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	public class StringFormatTest : Assertion {
		
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
			
			AssertEquals (smf.LineAlignment, smfclone.LineAlignment);			
			AssertEquals (smf.FormatFlags, smfclone.FormatFlags);			
			AssertEquals (smf.LineAlignment, smfclone.LineAlignment);			
			AssertEquals (smf.Alignment, smfclone.Alignment);			
			AssertEquals (smf.Trimming, smfclone.Trimming);			
		}
			
		[Test]
		public void TestAlignment() 
		{					
			StringFormat	smf = new StringFormat ();
			
			smf.LineAlignment = StringAlignment.Center;									
			AssertEquals (StringAlignment.Center, smf.LineAlignment);			
			
			smf.Alignment = StringAlignment.Far;									
			AssertEquals (StringAlignment.Far, smf.Alignment);						 
		}		
			
		[Test]
		public void TestFormatFlags() 
		{				
			StringFormat	smf = new StringFormat ();
			
			smf.FormatFlags = StringFormatFlags.DisplayFormatControl;									
			AssertEquals (StringFormatFlags.DisplayFormatControl, smf.FormatFlags);						 
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
			
			AssertEquals (200, firstTabOffset);						 
			AssertEquals (tabsSrc.Length, tabStops.Length);						 
			AssertEquals (tabsSrc[0], tabStops[0]);					
			AssertEquals (tabsSrc[1], tabStops[1]);					
			AssertEquals (tabsSrc[2], tabStops[2]);					
			AssertEquals (tabsSrc[3], tabStops[3]);					
		}	
		
	}
}
