//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	[Ignore ("This test has to be completly reviewed")]
	public class ButtonTest
	{
		[Test]
		public void FlatStyleTest ()
		{
			Button B1 = new Button ();
			Assert.AreEqual (FlatStyle.Standard, B1.FlatStyle, "#1");
		}

		[Test]
		public void ImageTest ()
		{
			Button B1 = new Button ();
			B1.Visible = true;
			B1.Image = Image.FromFile ("M.gif");
			Assert.AreEqual (ContentAlignment.MiddleCenter, B1.ImageAlign, "#2");
		}

		[Test]
		public void ImageListTest ()
		{
			Button B1 = new Button ();
			B1.Image = Image.FromFile ("M.gif");
			Assert.AreEqual (null, B1.ImageList, "#3a");
			ImageList ImageList1 = new ImageList ();
			ImageList1.Images.Add(Image.FromFile ("M.gif"));
			B1.ImageList = ImageList1;
			Assert.AreEqual (-1, B1.ImageIndex, "#3b");
			B1.ImageIndex = 0;
			Assert.AreEqual (1, B1.ImageList.Images.Count, "#3c");
			Assert.AreEqual (16, B1.ImageList.ImageSize.Height, "#3d");
			Assert.AreEqual (16, B1.ImageList.ImageSize.Width, "#3e");
		}

		[Test]
		public void IMeModeTest ()
		{
			Button B1 = new Button ();
			Assert.AreEqual (ImeMode.Disable, B1.ImeMode, "#4");
		}

		[Test]
		public void TextAlignTest ()
		{
			Button B1 = new Button ();
			Assert.AreEqual (ContentAlignment.MiddleCenter, B1.TextAlign, "#5");
		}

		[Test]
		public void DialogResultTest ()
		{
			Form f = new Form ();
			Button B1 = new Button ();
			B1.Text = "DialogResult";
			B1.DialogResult = DialogResult.No;
			B1.TextAlign = ContentAlignment.BottomRight;
			B1.Visible = true;
			f.Controls.Add (B1);
			Assert.AreEqual (DialogResult.No, B1.DialogResult, "#6");
		}

		[Test]
		public void PerformClickTest ()
		{
			Form f = new Form ();
			Button B1 = new Button ();
			B1.Text = "DialogResult";
			B1.Visible = true;
			f.Controls.Add (B1);
			B1.PerformClick ();
			Assert.AreEqual (DialogResult.None, B1.DialogResult, "#7");
		}

		[Test]
		public void NotifyDefaultTest ()
		{
			Button B1 = new Button ();
			Assert.AreEqual ("System.Windows.Forms.Button, Text: ", B1.ToString (), "#8");
		}

		[Test]
		public void ToStringTest ()
		{
			Button B1 = new Button ();
			Assert.AreEqual ("System.Windows.Forms.Button, Text: " , B1.ToString (), "#9");
		}
	}
}
