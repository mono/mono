//
// Copyright (c) 2005-2006 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//

using System;
using System.Windows.Forms;
using System.Drawing;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
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

			B1 = new Button ();
			ImageList ImageList1 = new ImageList ();
			ImageList1.Images.Add(Image.FromFile ("M.gif"));
			ImageList1.Images.Add(Image.FromFile ("M.gif"));
			Assert.AreEqual (2, ImageList1.Images.Count, "#3b");
			B1.ImageList = ImageList1;
			Assert.AreEqual (-1, B1.ImageIndex, "#3c");


			B1 = new Button ();
			B1.ImageIndex = 1;
			B1.ImageList = ImageList1;
			Assert.AreEqual (1, B1.ImageIndex, "#3d");
			Assert.AreEqual (2, B1.ImageList.Images.Count, "#3e");
			Assert.AreEqual (16, B1.ImageList.ImageSize.Height, "#3f");
			Assert.AreEqual (16, B1.ImageList.ImageSize.Width, "#3g");
		}

		[Test]
		public void IMeModeTest ()
		{
			Button B1 = new Button ();
			Assert.AreEqual (ImeMode.Disable, B1.ImeMode, "#4a");
			B1.ImeMode = ImeMode.Off;
			Assert.AreEqual (ImeMode.Off, B1.ImeMode, "#4b");

			B1 = new Button ();
			Assert.AreEqual (ImeMode.Disable, ((Control)B1).ImeMode, "#4c");
			((Control)B1).ImeMode = ImeMode.Off;
			Assert.AreEqual (ImeMode.Off, ((Control)B1).ImeMode, "#4d");
			Assert.AreEqual (ImeMode.Off, B1.ImeMode, "#4e");
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
			f.ShowInTaskbar = false;
			Button B1 = new Button ();
			B1.Text = "DialogResult";
			B1.DialogResult = DialogResult.No;
			B1.TextAlign = ContentAlignment.BottomRight;
			B1.Visible = true;
			f.Controls.Add (B1);
			Assert.AreEqual (DialogResult.No, B1.DialogResult, "#6");
			f.Dispose ();
		}

		[Test]
		public void PerformClickTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			Button B1 = new Button ();
			B1.Text = "DialogResult";
			B1.Visible = true;
			f.Controls.Add (B1);
			B1.PerformClick ();
			Assert.AreEqual (DialogResult.None, B1.DialogResult, "#7");
			f.Dispose ();
		}

		[Test]
		public void ToStringTest ()
		{
			Button B1 = new Button ();
			Assert.AreEqual ("System.Windows.Forms.Button, Text: " , B1.ToString (), "#9");
		}
	}

	[TestFixture]
	public class ButtonInheritorTest : Button {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Null ()
		{
			new ButtonBaseAccessibleObject (null);
		}

		[Test]
		public void Constructor ()
		{
			ButtonBaseAccessibleObject bbao = new ButtonBaseAccessibleObject (this);
			Assert.IsNotNull (bbao.Owner, "Owner");
			Assert.IsTrue (Object.ReferenceEquals (this, bbao.Owner), "ReferenceEquals");
			Assert.AreEqual ("Press", bbao.DefaultAction, "DefaultAction");
			Assert.IsNull (bbao.Description, "Description");
			Assert.IsNull (bbao.Help, "Help");
			Assert.IsNull (bbao.Name, "Name");
			Assert.AreEqual (AccessibleRole.PushButton, bbao.Role, "Role");
			Assert.AreEqual (AccessibleStates.None, bbao.State, "State");
		}

		[Test]
		public void CreateAccessibilityInstanceTest ()
		{
			AccessibleObject ao = base.CreateAccessibilityInstance ();
			Button.ButtonBaseAccessibleObject bbao = (ao as Button.ButtonBaseAccessibleObject);
			Assert.IsNotNull (bbao, "ButtonBaseAccessibleObject");
			Assert.IsNotNull (bbao.Owner, "Owner");
			Assert.IsTrue (Object.ReferenceEquals (this, bbao.Owner), "ReferenceEquals");
			Assert.AreEqual ("Press", bbao.DefaultAction, "DefaultAction");
			Assert.IsNull (bbao.Description, "Description");
			Assert.IsNull (bbao.Help, "Help");
			Assert.IsNull (bbao.Name, "Name");
			Assert.AreEqual (AccessibleRole.PushButton, bbao.Role, "Role");
			Assert.AreEqual (AccessibleStates.None, bbao.State, "State");
		}
	}
}
