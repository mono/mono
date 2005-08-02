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

[TestFixture]
public class ImageListTest
{
	[Test]
	public void ImageListPropertyTest ()
	{
		Form myform = new Form ();
		ImageList myimagelist = new ImageList ();
		Assert.AreEqual (ColorDepth.Depth8Bit, myimagelist.ColorDepth, "#1");
		myimagelist.ColorDepth = ColorDepth.Depth32Bit;
		Assert.AreEqual (false, myimagelist.HandleCreated, "#2");
		myimagelist.Handle.ToInt32 ();
		Assert.AreEqual (true, myimagelist.HandleCreated, "#3");
		Assert.AreEqual (ColorDepth.Depth32Bit, myimagelist.ColorDepth, "#4");
		Assert.AreEqual ("System.IntPtr", myimagelist.Handle.GetType ().FullName, "#5");
		Assert.AreEqual (0, myimagelist.Images.Count, "#6");
		System.Drawing.Image myImage = 	Image.FromFile("M.gif");
		myimagelist.Images.Add (myImage);
		Assert.AreEqual (1, myimagelist.Images.Count, "#7");
		Assert.AreEqual (16, myimagelist.ImageSize.Height, "#8");
		Assert.AreEqual (16, myimagelist.ImageSize.Width, "#9");
		Assert.AreEqual (Color.Transparent, myimagelist.TransparentColor, "#10");
	}
}
