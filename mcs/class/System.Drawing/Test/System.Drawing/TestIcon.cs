//
// Icon class testing unit
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
	public class TestIcon {
		
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
			Assert.AreEqual (48, newIcon.Height, "C#1a"); 			
			Assert.AreEqual (48, newIcon.Width, "C#1b");

			newIcon = new Icon (icon, 16, 16);
			Assert.AreEqual (16, newIcon.Height, "C#2a"); 			
			Assert.AreEqual (16, newIcon.Width, "C#2b");
		}				

		[Test]
		public void TestProperties ()
		{
			Assert.AreEqual (32, icon.Height, "P#1");
			Assert.AreEqual (32, icon.Width, "P#2");
			Assert.AreEqual (32, icon.Size.Width, "P#3");
			Assert.AreEqual (32, icon.Size.Height, "P#4");

		}

		[Test]
		public void TestMethods ()
		{
			newIcon = (Icon) icon.Clone ();
			Assert.AreEqual (32, newIcon.Height, "M#1a");
			Assert.AreEqual (32, newIcon.Width, "M#1b");
			
			Bitmap bmp = icon.ToBitmap();
			Assert.AreEqual (32, bmp.Height, "M#2a");
			Assert.AreEqual (32, bmp.Width, "M#2b");
			
			fs = new FileStream ("newIcon.ico", FileMode.Create);
			icon.Save (fs);
			
			Assert.AreEqual (fs1.Length, fs.Length, "M#3");			
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
