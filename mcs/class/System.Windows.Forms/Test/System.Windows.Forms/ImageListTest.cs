//
// ImageImageListTest.cs: Test cases for ImageImageList.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ImageListTest : TestHelper
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
			Image myImage =	Image.FromFile(TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"));
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
		public void ImageListComponentModelTest ()
		{
			PropertyDescriptor colordepth_prop = TypeDescriptor.GetProperties (typeof (ImageList))["ColorDepth"];
			PropertyDescriptor imagesize_prop = TypeDescriptor.GetProperties (typeof (ImageList))["ImageSize"];
			PropertyDescriptor transparentcolor_prop = TypeDescriptor.GetProperties (typeof (ImageList))["TransparentColor"];

			// create a blank ImageList
			ImageList il = new ImageList ();

			// test its defaults
			Assert.IsTrue (colordepth_prop.ShouldSerializeValue (il), "1");
			Assert.IsTrue (colordepth_prop.CanResetValue (il), "2");
			Assert.IsTrue (imagesize_prop.ShouldSerializeValue (il), "3");
			Assert.IsTrue (imagesize_prop.CanResetValue (il), "4");
			Assert.IsTrue (transparentcolor_prop.ShouldSerializeValue (il), "5");
			Assert.IsTrue (transparentcolor_prop.CanResetValue (il), "6");

			// test what happens when we set the transparent color to LightGray
			il.TransparentColor = Color.LightGray;
			Assert.IsFalse (transparentcolor_prop.ShouldSerializeValue (il), "7");
			Assert.IsFalse (transparentcolor_prop.CanResetValue (il), "8");

			// test what happens when we set the depth to something other than the default
			il.ColorDepth = ColorDepth.Depth16Bit;
			Assert.IsTrue (colordepth_prop.ShouldSerializeValue (il), "9");
			Assert.IsTrue (colordepth_prop.CanResetValue (il), "10");
			// same test for ImageSize
			il.ImageSize = new Size (32, 32);
			Assert.IsTrue (imagesize_prop.ShouldSerializeValue (il), "11");
			Assert.IsTrue (imagesize_prop.CanResetValue (il), "12");

			// create an ImageList containing an image
			il = new ImageList ();
			il.Images.Add (Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif")));

			Assert.IsFalse (colordepth_prop.ShouldSerializeValue (il), "13");
			Assert.IsFalse (colordepth_prop.CanResetValue (il), "14");
			Assert.IsFalse (imagesize_prop.ShouldSerializeValue (il), "15");
			Assert.IsFalse (imagesize_prop.CanResetValue (il), "16");
			Assert.IsTrue (transparentcolor_prop.ShouldSerializeValue (il), "17");
			Assert.IsTrue (transparentcolor_prop.CanResetValue (il), "18");

			// test what happens when we set the transparent color to LightGray
			il.TransparentColor = Color.LightGray;
			Assert.IsFalse (transparentcolor_prop.ShouldSerializeValue (il), "19");
			Assert.IsFalse (transparentcolor_prop.CanResetValue (il), "20");

			// test what happens when we set the depth to something other than the default
			il.ColorDepth = ColorDepth.Depth16Bit;
			Assert.IsFalse (colordepth_prop.ShouldSerializeValue (il), "21");
			Assert.IsFalse (colordepth_prop.CanResetValue (il), "22");

			// same test for ImageSize
			il.ImageSize = new Size (32, 32);
			Assert.IsFalse (imagesize_prop.ShouldSerializeValue (il), "23");
			Assert.IsFalse (imagesize_prop.CanResetValue (il), "24");
		}

		[Test]
		public void ToStringMethodTest () 
		{
			ImageList myimagelist = new ImageList ();
			Assert.AreEqual ("System.Windows.Forms.ImageList Images.Count: 0, ImageSize: {Width=16, Height=16}",
					 myimagelist.ToString (), "#T3");
		}

		[Test] // bug #409169
		public void ICollection_CopyTo ()
		{
			ImageList imgList = new ImageList ();
			ImageList.ImageCollection coll = imgList.Images;

			Image gif = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"));
			coll.Add (gif);
			Bitmap bmp = new Bitmap (10, 10);
			coll.Add (bmp);

			const int dstOffset = 5;
			object [] dst = new object [dstOffset + coll.Count + 1];
			((ICollection) coll).CopyTo (dst, dstOffset);

			Assert.IsNull (dst [0], "#1");
			Assert.IsNull (dst [1], "#2");
			Assert.IsNull (dst [2], "#3");
			Assert.IsNull (dst [3], "#4");
			Assert.IsNull (dst [4], "#5");
			Assert.IsNotNull (dst [5], "#6a");
			Assert.IsFalse (object.ReferenceEquals (gif, dst [5]), "#6b");
			Assert.AreEqual (typeof (Bitmap), dst [5].GetType (), "#6c");
			Assert.IsNotNull (dst [6], "#7a");
			Assert.IsFalse (object.ReferenceEquals (bmp, dst [6]), "#7b");
			Assert.AreEqual (typeof (Bitmap), dst [6].GetType (), "#7c");
			Assert.IsNull (dst [7], "#8");

			((Image) dst [5]).Dispose ();
			((Image) dst [6]).Dispose ();

			coll [0].RotateFlip (RotateFlipType.Rotate90FlipY);
			coll [1].RotateFlip (RotateFlipType.Rotate90FlipY);
		}

		[TestFixture]
		public class ImageListRecreateHandleEventClass : TestHelper
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
				myform.ShowInTaskbar = false;
				Graphics mygraphics = null;
				ImageList myimagelist = new ImageList ();
				Image myImage =	Image.FromFile(TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"));
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
				myform.Dispose ();
			}
		}
	}
}
