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

using MonoTests.Helpers;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ButtonTest : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			Button b = new Button ();
			
			Assert.AreEqual (SystemColors.Control, b.BackColor, "A4");
			Assert.AreEqual (FlatStyle.Standard, b.FlatStyle, "A6");
			Assert.AreEqual (null, b.Image, "A7");
			Assert.AreEqual (ContentAlignment.MiddleCenter, b.ImageAlign, "A8");
			Assert.AreEqual (-1, b.ImageIndex, "A9");
			Assert.AreEqual (null, b.ImageList, "A11");
			Assert.AreEqual (ImeMode.Disable, b.ImeMode, "A12");
			Assert.AreEqual (string.Empty, b.Text, "A13");
			Assert.AreEqual (ContentAlignment.MiddleCenter, b.TextAlign, "A14");

			Assert.AreEqual (false, b.AutoEllipsis, "A1");
			Assert.AreEqual (false, b.AutoSize, "A2");
			Assert.AreEqual (string.Empty, b.ImageKey, "A10");
			Assert.AreEqual (TextImageRelation.Overlay, b.TextImageRelation, "A15");
			Assert.AreEqual (true, b.UseCompatibleTextRendering, "A16");
			Assert.AreEqual (true, b.UseMnemonic, "A17");
			Assert.AreEqual (true, b.UseVisualStyleBackColor, "A18");
		}
		
		[Test]
		public void FlatButtonAppearanceTest ()
		{
			Button B1 = new Button ();
			FlatButtonAppearance flatApp = B1.FlatAppearance;

			Assert.AreEqual (Color.Empty, flatApp.BorderColor, "#A1");
			Assert.AreEqual (1, flatApp.BorderSize, "#A2");
			Assert.AreEqual (Color.Empty, flatApp.CheckedBackColor, "#A3");
			Assert.AreEqual (Color.Empty, flatApp.MouseDownBackColor, "#A4");
			Assert.AreEqual (Color.Empty, flatApp.MouseOverBackColor, "#A5");

			flatApp.BorderColor = Color.Blue;
			Assert.AreEqual (Color.Blue, flatApp.BorderColor, "#B1");
			flatApp.BorderSize = 10;
			Assert.AreEqual (10, flatApp.BorderSize, "#B2");
			flatApp.CheckedBackColor = Color.Blue;
			Assert.AreEqual (Color.Blue, flatApp.CheckedBackColor, "#B3");
			flatApp.MouseDownBackColor = Color.Blue;
			Assert.AreEqual (Color.Blue, flatApp.MouseDownBackColor, "#B4");
			flatApp.MouseOverBackColor = Color.Blue;
			Assert.AreEqual (Color.Blue, flatApp.MouseOverBackColor, "#B5");
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void FlatButtonAppearanceExceptionTest ()
		{
			Button B1 = new Button ();
			FlatButtonAppearance flatApp = B1.FlatAppearance;

			flatApp.BorderSize = -1;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void FlatButtonAppearanceBorderColor ()
		{
			Button B1 = new Button ();
			FlatButtonAppearance flatApp = B1.FlatAppearance;

			flatApp.BorderColor = Color.Transparent;
		}

		[Test]
		public void BehaviorAutoSize ()
		{
			if (TestHelper.RunningOnUnix)
				Assert.Ignore ("Depends on font measurements, corresponds to windows");
				
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			f.Show ();
			
			Image i = new Bitmap (20, 20);
			String s = "My test string";
			
			Button b = new Button ();
			Size s_size = TextRenderer.MeasureText (s, new Button ().Font);
			
			b.UseCompatibleTextRendering = false;
			b.AutoSize = true;
			b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			b.Text = s;
			f.Controls.Add (b);
			
			// Text only
			b.TextImageRelation = TextImageRelation.Overlay;
			Assert.AreEqual (new Size (s_size.Width + 10, s_size.Height + 10), b.Size, "A1");
			b.TextImageRelation = TextImageRelation.ImageAboveText;
			Assert.AreEqual (new Size (s_size.Width + 10, s_size.Height + 10), b.Size, "A2");
			b.TextImageRelation = TextImageRelation.ImageBeforeText;
			Assert.AreEqual (new Size (s_size.Width + 10, s_size.Height + 10), b.Size, "A3");
			b.TextImageRelation = TextImageRelation.TextAboveImage;
			Assert.AreEqual (new Size (s_size.Width + 10, s_size.Height + 10), b.Size, "A4");
			b.TextImageRelation = TextImageRelation.TextBeforeImage;
			Assert.AreEqual (new Size (s_size.Width + 10, s_size.Height + 10), b.Size, "A5");
			
			// Text and Image
			b.Image = i;
			b.TextImageRelation = TextImageRelation.Overlay;
			Assert.AreEqual (new Size (s_size.Width + 10, i.Height + 6), b.Size, "A6");
			b.TextImageRelation = TextImageRelation.ImageAboveText;
			Assert.AreEqual (new Size (s_size.Width + 10, s_size.Height + i.Height + 10), b.Size, "A7");
			b.TextImageRelation = TextImageRelation.ImageBeforeText;
			Assert.AreEqual (new Size (s_size.Width + i.Width + 10, i.Height + 6), b.Size, "A8");
			b.TextImageRelation = TextImageRelation.TextAboveImage;
			Assert.AreEqual (new Size (s_size.Width + 10, s_size.Height + i.Height + 10), b.Size, "A9");
			b.TextImageRelation = TextImageRelation.TextBeforeImage;
			Assert.AreEqual (new Size (s_size.Width + i.Width + 10, i.Height + 6), b.Size, "A10");

			// Image only
			b.Text = string.Empty;
			b.TextImageRelation = TextImageRelation.Overlay;
			Assert.AreEqual (new Size (i.Height + 6, i.Height + 6), b.Size, "A11");
			b.TextImageRelation = TextImageRelation.ImageAboveText;
			Assert.AreEqual (new Size (i.Height + 6, i.Height + 6), b.Size, "A12");
			b.TextImageRelation = TextImageRelation.ImageBeforeText;
			Assert.AreEqual (new Size (i.Height + 6, i.Height + 6), b.Size, "A13");
			b.TextImageRelation = TextImageRelation.TextAboveImage;
			Assert.AreEqual (new Size (i.Height + 6, i.Height + 6), b.Size, "A14");
			b.TextImageRelation = TextImageRelation.TextBeforeImage;
			Assert.AreEqual (new Size (i.Height + 6, i.Height + 6), b.Size, "A15");
			
			// Neither
			b.Image = null;
			b.TextImageRelation = TextImageRelation.Overlay;
			Assert.AreEqual (new Size (6, 6), b.Size, "A16");
			b.TextImageRelation = TextImageRelation.ImageAboveText;
			Assert.AreEqual (new Size (6, 6), b.Size, "A17");
			b.TextImageRelation = TextImageRelation.ImageBeforeText;
			Assert.AreEqual (new Size (6, 6), b.Size, "A18");
			b.TextImageRelation = TextImageRelation.TextAboveImage;
			Assert.AreEqual (new Size (6, 6), b.Size, "A19");
			b.TextImageRelation = TextImageRelation.TextBeforeImage;
			Assert.AreEqual (new Size (6, 6), b.Size, "A20");
			
			// Padding
			b.Padding = new Padding (5, 10, 15, 20);
			Assert.AreEqual (new Size (6 + b.Padding.Horizontal, 6 + b.Padding.Vertical), b.Size, "A21");

			f.Dispose ();
		}

		[Test]
		public void BehaviorImageList ()
		{
			// Basically, this shows that whichever of [Image|ImageIndex|ImageKey]
			// is set last resets the others to their default state
			Button b = new Button ();

			Bitmap i1 = new Bitmap (16, 16);
			i1.SetPixel (0, 0, Color.Blue);
			Bitmap i2 = new Bitmap (16, 16);
			i2.SetPixel (0, 0, Color.Red);
			Bitmap i3 = new Bitmap (16, 16);
			i3.SetPixel (0, 0, Color.Green);

			Assert.AreEqual (null, b.Image, "D1");
			Assert.AreEqual (-1, b.ImageIndex, "D2");
			Assert.AreEqual (string.Empty, b.ImageKey, "D3");

			ImageList il = new ImageList ();
			il.Images.Add ("i2", i2);
			il.Images.Add ("i3", i3);

			b.ImageList = il;

			b.ImageKey = "i3";
			Assert.AreEqual (-1, b.ImageIndex, "D4");
			Assert.AreEqual ("i3", b.ImageKey, "D5");
			Assert.AreEqual (i3.GetPixel (0, 0), (b.Image as Bitmap).GetPixel (0, 0), "D6");

			b.ImageIndex = 0;
			Assert.AreEqual (0, b.ImageIndex, "D7");
			Assert.AreEqual (string.Empty, b.ImageKey, "D8");
			Assert.AreEqual (i2.GetPixel (0, 0), (b.Image as Bitmap).GetPixel (0, 0), "D9");
			
			// Also, Image is not cached, changing the underlying ImageList image is reflected
			il.Images[0] = i1;
			Assert.AreEqual (i1.GetPixel (0, 0), (b.Image as Bitmap).GetPixel (0, 0), "D16");

			// Note: setting Image resets ImageList to null
			b.Image = i1;
			Assert.AreEqual (-1, b.ImageIndex, "D10");
			Assert.AreEqual (string.Empty, b.ImageKey, "D11");
			Assert.AreEqual (i1.GetPixel (0, 0), (b.Image as Bitmap).GetPixel (0, 0), "D12");
			Assert.AreEqual (null, b.ImageList, "D12-2");

			b.Image = null;
			Assert.AreEqual (null, b.Image, "D13");
			Assert.AreEqual (-1, b.ImageIndex, "D14");
			Assert.AreEqual (string.Empty, b.ImageKey, "D15");
		}
		[Test]
		public void ImageTest ()
		{
			Button B1 = new Button ();
			B1.Visible = true;
			B1.Image = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"));
			Assert.AreEqual (ContentAlignment.MiddleCenter, B1.ImageAlign, "#2");
		}

		[Test]
		public void ImageListTest ()
		{
			Button B1 = new Button ();
			B1.Image = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"));
			Assert.AreEqual (null, B1.ImageList, "#3a");

			B1 = new Button ();
			ImageList ImageList1 = new ImageList ();
			ImageList1.Images.Add(Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif")));
			ImageList1.Images.Add(Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif")));
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
			f.Dispose();

			// check cancel button behavior
			f = new Form ();
			f.ShowInTaskbar = false;
			B1 = new Button ();
			f.CancelButton = B1;
			f.Controls.Add (B1);
			Assert.AreEqual (DialogResult.Cancel, B1.DialogResult, "#7");
			f.Dispose ();

			f = new Form ();
			f.ShowInTaskbar = false;
			B1 = new Button ();
			B1.DialogResult = DialogResult.No;
			f.CancelButton = B1;
			f.Controls.Add (B1);
			Assert.AreEqual (DialogResult.No, B1.DialogResult, "#8");
			f.Dispose ();

			f = new Form ();
			f.ShowInTaskbar = false;
			B1 = new Button ();
			B1.DialogResult = DialogResult.No;
			B1.DialogResult = DialogResult.None;
			f.CancelButton = B1;
			f.Controls.Add (B1);
			Assert.AreEqual (DialogResult.Cancel, B1.DialogResult, "#9");
			f.Dispose ();

			// check accept button behavior
			f = new Form ();
			f.ShowInTaskbar = false;
			B1 = new Button ();
			f.AcceptButton = B1;
			f.Controls.Add (B1);
			Assert.AreEqual (DialogResult.None, B1.DialogResult, "#10");
			f.Dispose ();

			f = new Form ();
			f.ShowInTaskbar = false;
			B1 = new Button ();
			B1.DialogResult = DialogResult.No;
			f.AcceptButton = B1;
			f.Controls.Add (B1);
			Assert.AreEqual (DialogResult.No, B1.DialogResult, "#11");
			f.Dispose ();

			f = new Form ();
			f.ShowInTaskbar = false;
			B1 = new Button ();
			B1.DialogResult = DialogResult.No;
			B1.DialogResult = DialogResult.None;
			f.AcceptButton = B1;
			f.Controls.Add (B1);
			Assert.AreEqual (DialogResult.None, B1.DialogResult, "#12");
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
		public void DefaultImeModeTest ()
		{
			Assert.AreEqual (ImeMode.Disable, DefaultImeMode, "1");
		}

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
			Assert.AreEqual (AccessibleStates.Focusable, bbao.State, "State");
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
			Assert.AreEqual (AccessibleStates.Focusable, bbao.State, "State");
		}
	}
}
