//
// Bitmap class testing unit
//
// Author:
//
// 	 Sanjay Gupta <gsanjay@novell.com>
//
// (C) 2004 Novell, Inc.  http://www.novell.com
//
using System;
using System.Drawing;
using NUnit.Framework;
using System.IO;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	public class TestIcon : Assertion {
		
		Icon icon;
		Icon newIcon;
		FileStream fs;
		FileStream fs1;
		
		[SetUp]
		public void SetUp ()		
		{
			String path = TestBitmap.getInFile ("bitmaps/smiley.ico");
			icon = new Icon (path);			
			fs1 = new FileStream (path, FileMode.Open);
		}

		[Test]
		public void TestConstructors ()
		{
			newIcon = new Icon (fs1, 48, 48);
			AssertEquals ("C#1a", 48, newIcon.Height); 			
			AssertEquals ("C#1b", 48, newIcon.Width);

			newIcon = new Icon (icon, 16, 16);
			AssertEquals ("C#2a", 16, newIcon.Height); 			
			AssertEquals ("C#2b", 16, newIcon.Width);
		}				

		[Test]
		public void TestProperties ()
		{
			AssertEquals ("P#1", 32, icon.Height);
			AssertEquals ("P#2", 32, icon.Width);
			AssertEquals ("P#3", 32, icon.Size.Width);
			AssertEquals ("P#4", 32, icon.Size.Height);

		}

		[Test]
		public void TestMethods ()
		{
			newIcon = (Icon) icon.Clone ();
			AssertEquals ("M#1a", 32, newIcon.Height);
			AssertEquals ("M#1b", 32, newIcon.Width);
			
			Bitmap bmp = icon.ToBitmap();
			AssertEquals ("M#2a", 32, bmp.Height);
			AssertEquals ("M#2b", 32, bmp.Width);
			
			fs = new FileStream ("newIcon.ico", FileMode.Create);
			icon.Save (fs);

			AssertEquals ("M#3", fs1.Length, fs.Length);			
		}

		[TearDown]
		public void TearDown () 
		{
			if (fs != null)
				fs.Close();
			if (fs1 != null)
				fs1.Close();
			if (File.Exists ("newIcon.ico"))
				File.Delete("newIcon.ico");
		}
	}
}
