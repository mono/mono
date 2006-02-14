//
// ImageImageListTest.cs: Test cases for ImageImageList.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;
using System.Threading;


namespace MonoTests.System.Windows.Forms
{

	[TestFixture]
	[Ignore ("This test has to be completly reviewed")]
	public class ImageListTest
	{
		[Test]
		public void ImageListPropertyTest ()
		{
			ImageList myimagelist = new ImageList ();

			// C
			Assert.AreEqual (ColorDepth.Depth8Bit, myimagelist.ColorDepth, "#C1");
			myimagelist.ColorDepth = ColorDepth.Depth32Bit;
			Assert.AreEqual (ColorDepth.Depth32Bit, myimagelist.ColorDepth, "#C2");
			Assert.AreEqual (0, myimagelist.Images.Count, "#C3");
			// H
			Assert.AreEqual (false, myimagelist.HandleCreated, "#H1");
			myimagelist.Handle.ToInt32 ();
			Assert.AreEqual (true, myimagelist.HandleCreated, "#H2");
			Assert.AreEqual ("System.IntPtr", myimagelist.Handle.GetType ().FullName, "#H3");
		
			// I
			Image myImage =	Image.FromFile("M.gif");
			myimagelist.Images.Add (myImage);
			Assert.AreEqual (1, myimagelist.Images.Count, "#I1");
			Assert.AreEqual (16, myimagelist.ImageSize.Height, "#I2");
			Assert.AreEqual (16, myimagelist.ImageSize.Width, "#I3");
			// [MonoTODO ("Add test for ImageStream")]
			// [MonoTODO ("Test for Draw Method (visual test)")]
						
			// T
			Assert.AreEqual (Color.Transparent, myimagelist.TransparentColor, "#T1");
		}
		
		[Test]
		public void ToStringMethodTest () 
		{
			ImageList myimagelist = new ImageList ();
			Assert.AreEqual ("System.Windows.Forms.ImageList Images.Count: 0, ImageSize: {Width=16, Height=16}",                                          myimagelist.ToString (), "#T3");
		}
		
		[TestFixture]
		[Ignore ("This test has to be completly reviewed")]
		public class ImageListRecreateHandleEventClass
		{
			static bool eventhandled = false;
			public static void RecreateHandle_EventHandler (object sender, EventArgs e)
			{
				eventhandled = true;
			}

			[Test]
			public void RecreateHandleEvenTest ()
			{
				Form myform = new Form ();
				Graphics mygraphics = null;
				ImageList myimagelist = new ImageList ();
				Image myImage =	Image.FromFile("M.gif");
				myimagelist.Images.Add (myImage);
	                        myimagelist.ColorDepth = ColorDepth.Depth8Bit;
				myimagelist.ImageSize = new Size (50,50);
				myimagelist.RecreateHandle += new EventHandler (RecreateHandle_EventHandler);
				mygraphics = Graphics.FromHwnd(myform.Handle);
				myimagelist.Draw(mygraphics, new Point(5, 5), 0);
				myimagelist.ImageSize = new Size (100,100);
				Assert.AreEqual (true, eventhandled, "#1");
				eventhandled = false;
				myimagelist.Images.Add (myImage);
				myimagelist.ColorDepth = ColorDepth.Depth32Bit;
				Assert.AreEqual (true, eventhandled, "#2");
			}

		}
	}
}
