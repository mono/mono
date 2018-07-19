//
// ToolStripItemTests.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolStripItemTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripItem tsi = new NullToolStripItem ();

			Assert.AreEqual (ToolStripItemAlignment.Left, tsi.Alignment, "A1");
			Assert.AreEqual (false, tsi.AllowDrop, "A2");
			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, tsi.Anchor, "A3");
			Assert.AreEqual (true, tsi.AutoSize, "A4");
			Assert.AreEqual (false, tsi.AutoToolTip, "A5");
			Assert.AreEqual (true, tsi.Available, "A6");
			Assert.AreEqual (SystemColors.Control, tsi.BackColor, "A7");
			Assert.AreEqual (null, tsi.BackgroundImage, "A8");
			Assert.AreEqual (ImageLayout.Tile, tsi.BackgroundImageLayout, "A9");
			Assert.AreEqual (new Rectangle (0,0,23,23), tsi.Bounds, "A10");
			Assert.AreEqual (true, tsi.CanSelect, "A11");
			Assert.AreEqual (new Rectangle (2, 2, 19, 19), tsi.ContentRectangle, "A12");
			Assert.AreEqual (ToolStripItemDisplayStyle.ImageAndText, tsi.DisplayStyle, "A13");
			Assert.AreEqual (DockStyle.None, tsi.Dock, "A14");
			Assert.AreEqual (false, tsi.DoubleClickEnabled, "A15");
			Assert.AreEqual (true, tsi.Enabled, "A16");
			//Assert.AreEqual (new Font ("Tahoma", 8.25f), tsi.Font, "A17");
			Assert.AreEqual (SystemColors.ControlText, tsi.ForeColor, "A18");
			Assert.AreEqual (23, tsi.Height, "A19");
			Assert.AreEqual (null, tsi.Image, "A20");
			Assert.AreEqual (ContentAlignment.MiddleCenter, tsi.ImageAlign, "A21");
			Assert.AreEqual (-1, tsi.ImageIndex, "A22");
			Assert.AreEqual (string.Empty, tsi.ImageKey, "A22-1");
			Assert.AreEqual (ToolStripItemImageScaling.SizeToFit, tsi.ImageScaling, "A23");
			Assert.AreEqual (Color.Empty, tsi.ImageTransparentColor, "A24");
			Assert.AreEqual (false, tsi.IsDisposed, "A25");
			Assert.AreEqual (false, tsi.IsOnDropDown, "A26");
			Assert.AreEqual (false, tsi.IsOnOverflow, "A27");
			Assert.AreEqual (new Padding(0,1,0,2), tsi.Margin, "A28");
			Assert.AreEqual (MergeAction.Append, tsi.MergeAction, "A29");
			Assert.AreEqual (-1, tsi.MergeIndex, "A30");
			Assert.AreEqual (string.Empty, tsi.Name, "A31");
			Assert.AreEqual (ToolStripItemOverflow.AsNeeded, tsi.Overflow, "A32");
			Assert.AreEqual (null, tsi.Owner, "A33");
			Assert.AreEqual (null, tsi.OwnerItem, "A34");
			Assert.AreEqual (new Padding(0), tsi.Padding, "A35");
			Assert.AreEqual (ToolStripItemPlacement.None, tsi.Placement, "A36");
			Assert.AreEqual (false, tsi.Pressed, "A37");
			Assert.AreEqual (RightToLeft.Inherit, tsi.RightToLeft, "A38");
			Assert.AreEqual (false, tsi.RightToLeftAutoMirrorImage, "A39");
			Assert.AreEqual (false, tsi.Selected, "A40");
			Assert.AreEqual (new Size (23,23), tsi.Size, "A41");
			Assert.AreEqual (null, tsi.Tag, "A42");
			Assert.AreEqual (string.Empty, tsi.Text, "A43");
			Assert.AreEqual (ContentAlignment.MiddleCenter, tsi.TextAlign, "A44");
			Assert.AreEqual (ToolStripTextDirection.Horizontal, tsi.TextDirection, "A45");
			Assert.AreEqual (TextImageRelation.ImageBeforeText, tsi.TextImageRelation, "A46");
			Assert.AreEqual (null, tsi.ToolTipText, "A47");
			Assert.AreEqual (false, tsi.Visible, "A48");
			Assert.AreEqual (23, tsi.Width, "A49");

			Image i = new Bitmap (1,1);
			int count = 0;
			EventHandler oc = new EventHandler (delegate (object sender, EventArgs e) { count++; });
			
			tsi = new NullToolStripItem ("MyText", i, oc);
			tsi.PerformClick();
			Assert.AreEqual ("MyText", tsi.Text, "A50");
			Assert.AreSame (i, tsi.Image, "A51");
			Assert.AreEqual (1, count, "A52");
			
			tsi = new NullToolStripItem ("MyText", i, oc, "MyName");
			tsi.PerformClick ();
			Assert.AreEqual ("MyText", tsi.Text, "A53");
			Assert.AreSame (i, tsi.Image, "A54");
			Assert.AreEqual (2, count, "A55");
			Assert.AreEqual ("MyName", tsi.Name, "A56");
		}
		
		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (false, epp.DefaultAutoToolTip, "C1");
			Assert.AreEqual (ToolStripItemDisplayStyle.ImageAndText, epp.DefaultDisplayStyle, "C2");
			Assert.AreEqual (new Padding (0,1,0,2), epp.DefaultMargin, "C3");
			Assert.AreEqual (new Padding (0), epp.DefaultPadding, "C4");
			Assert.AreEqual (new Size (23, 23), epp.DefaultSize, "C5");
			Assert.AreEqual (true, epp.DismissWhenClicked, "C6");
			Assert.AreEqual (null, epp.Parent, "C7");
			Assert.AreEqual (false, epp.ShowKeyboardCues, "C8");
		}

		[Test]
		public void PropertyAlignment ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Alignment = ToolStripItemAlignment.Right;
			Assert.AreEqual (ToolStripItemAlignment.Right, tsi.Alignment, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Alignment = ToolStripItemAlignment.Right;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyAlignmentIEAE ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			tsi.Alignment = (ToolStripItemAlignment) 42;
		}

		[Test]
		public void PropertyAllowDrop ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.AllowDrop = true;
			Assert.AreEqual (true, tsi.AllowDrop, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.AllowDrop = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyAutoSize ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.AutoSize = false;
			Assert.AreEqual (false, tsi.AutoSize, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.AutoSize = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyAutoToolTip ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.AutoToolTip = true;
			Assert.AreEqual (true, tsi.AutoToolTip, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.AutoToolTip = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyAvailable ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Available = false;
			Assert.AreEqual (false, tsi.Available, "B1");
			Assert.AreEqual ("AvailableChanged;VisibleChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.Available = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyBackColor ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.BackColor = Color.BurlyWood;
			Assert.AreEqual (Color.BurlyWood, tsi.BackColor, "B1");
			Assert.AreEqual ("BackColorChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.BackColor = Color.BurlyWood;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyBackgroundImage ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			Image i = new Bitmap (1, 1);
			tsi.BackgroundImage = i;
			Assert.AreSame (i, tsi.BackgroundImage, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.BackgroundImage = i;
			Assert.AreSame (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyBackgroundImageLayout ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.BackgroundImageLayout = ImageLayout.Zoom;
			Assert.AreEqual (ImageLayout.Zoom, tsi.BackgroundImageLayout, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.BackgroundImageLayout = ImageLayout.Zoom;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyDisplayStyle ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.DisplayStyle = ToolStripItemDisplayStyle.Image;
			Assert.AreEqual (ToolStripItemDisplayStyle.Image, tsi.DisplayStyle, "B1");
			Assert.AreEqual ("DisplayStyleChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.DisplayStyle = ToolStripItemDisplayStyle.Image;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyDoubleClickEnabled ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.DoubleClickEnabled = true;
			Assert.AreEqual (true, tsi.DoubleClickEnabled, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.DoubleClickEnabled = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyEnabled ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Enabled = false;
			Assert.AreEqual (false, tsi.Enabled, "B1");
			Assert.AreEqual ("EnabledChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.Enabled = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyDockIEAE ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			tsi.Dock = (DockStyle)42;
		}

		[Test]
		public void PropertyFont ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			Font f = new Font ("Arial", 12);

			tsi.Font = f;
			Assert.AreSame (f, tsi.Font, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Font = f;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyForeColor ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ForeColor = Color.BurlyWood;
			Assert.AreEqual (Color.BurlyWood, tsi.ForeColor, "B1");
			Assert.AreEqual ("ForeColorChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.ForeColor = Color.BurlyWood;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyHeight ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Height = 42;
			Assert.AreEqual (42, tsi.Height, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Height = 42;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyImage ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			Image i = new Bitmap (1, 1);
			tsi.Image = i;
			Assert.AreSame (i, tsi.Image, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Image = i;
			Assert.AreSame (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyImageAlign ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ImageAlign = ContentAlignment.TopRight;
			Assert.AreEqual (ContentAlignment.TopRight, tsi.ImageAlign, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.ImageAlign = ContentAlignment.TopRight;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyImageAlignIEAE ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			tsi.ImageAlign = (ContentAlignment)42;
		}

		[Test]
		public void PropertyImageIndex ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ImageIndex = 42;
			Assert.AreEqual (42, tsi.ImageIndex, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.ImageIndex = 42;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void PropertyImageIndexAE ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			tsi.ImageIndex = -2;
		}

		[Test]
		public void PropertyImageKey ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ImageKey = "open";
			Assert.AreEqual ("open", tsi.ImageKey, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.ImageKey = "open";
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyImageScaling ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ImageScaling = ToolStripItemImageScaling.None;
			Assert.AreEqual (ToolStripItemImageScaling.None, tsi.ImageScaling, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.ImageScaling = ToolStripItemImageScaling.None;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyImageTransparentColor ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ImageTransparentColor = Color.BurlyWood;
			Assert.AreEqual (Color.BurlyWood, tsi.ImageTransparentColor, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.ImageTransparentColor = Color.BurlyWood;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyMargin ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Margin = new Padding (6);
			Assert.AreEqual (new Padding (6), tsi.Margin, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Margin = new Padding (6);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyMergeAction ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.MergeAction = MergeAction.Replace;
			Assert.AreEqual (MergeAction.Replace, tsi.MergeAction, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.MergeAction = MergeAction.Replace;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyMergeActionIEAE ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			tsi.MergeAction = (MergeAction)42;
		}

		[Test]
		public void PropertyMergeIndex ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.MergeIndex = 42;
			Assert.AreEqual (42, tsi.MergeIndex, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.MergeIndex = 42;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyName ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Name = "MyName";
			Assert.AreEqual ("MyName", tsi.Name, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Name = "MyName";
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyOverflow ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Overflow = ToolStripItemOverflow.Never;
			Assert.AreEqual (ToolStripItemOverflow.Never, tsi.Overflow, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Overflow = ToolStripItemOverflow.Never;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyOverflowIEAE ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			tsi.Overflow = (ToolStripItemOverflow)42;
		}

		[Test]
		public void PropertyOwner ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			ToolStrip ts = new ToolStrip ();
			tsi.Owner = ts;
			Assert.AreSame (ts, tsi.Owner, "B1");
			Assert.AreEqual ("OwnerChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.Owner = ts;
			Assert.AreSame (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyPadding ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Padding = new Padding (6);
			Assert.AreEqual (new Padding (6), tsi.Padding, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Padding = new Padding (6);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyRightToLeft ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.RightToLeft = RightToLeft.No;
			Assert.AreEqual (RightToLeft.No, tsi.RightToLeft, "B1");
			Assert.AreEqual ("RightToLeftChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.RightToLeft = RightToLeft.No;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyRightToLeftAutoMirrorImage ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.RightToLeftAutoMirrorImage = true;
			Assert.AreEqual (true, tsi.RightToLeftAutoMirrorImage, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.RightToLeftAutoMirrorImage = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertySize ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Size = new Size (42, 42);
			Assert.AreEqual (new Size (42, 42), tsi.Size, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Size = new Size (42, 42);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyTag ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Tag = "tag";
			Assert.AreSame ("tag", tsi.Tag, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Tag = "tag";
			Assert.AreSame (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyText ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Text = "Text";
			Assert.AreEqual ("Text", tsi.Text, "B1");
			Assert.AreEqual ("TextChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.Text = "Text";
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyTextAlign ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.TextAlign = ContentAlignment.TopRight;
			Assert.AreEqual (ContentAlignment.TopRight, tsi.TextAlign, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.TextAlign = ContentAlignment.TopRight;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyTextAlignIEAE ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			tsi.TextAlign = (ContentAlignment)42;
		}

		[Test]
		public void PropertyTextImageRelation ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.TextImageRelation = TextImageRelation.Overlay;
			Assert.AreEqual (TextImageRelation.Overlay, tsi.TextImageRelation, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.TextImageRelation = TextImageRelation.Overlay;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyToolTipText ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ToolTipText = "Text";
			Assert.AreEqual ("Text", tsi.ToolTipText, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.ToolTipText = "Text";
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyVisible ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Visible = true;
			Assert.AreEqual (false, tsi.Visible, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Visible = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyWidth ()
		{
			ToolStripItem tsi = new NullToolStripItem ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Width = 42;
			Assert.AreEqual (42, tsi.Width, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Width = 42;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void MethodDispose ()
		{
			ToolStrip ts = new ToolStrip ();
			NullToolStripItem tsi = new NullToolStripItem ();
			
			ts.Items.Add (tsi);
			
			Assert.AreEqual (false, tsi.IsDisposed, "A1");
			Assert.AreEqual (1, ts.Items.Count, "A2");
			Assert.AreEqual (ts, tsi.Owner, "A3");

			tsi.Dispose ();
			Assert.AreEqual (true, tsi.IsDisposed, "A4");
			Assert.AreEqual (0, ts.Items.Count, "A5");
			Assert.AreEqual (null, tsi.Owner, "A6");
		}

		[Test]
		public void MethodProcessMnemonic ()
		{
			NullToolStripItem tsi = new NullToolStripItem ();
			tsi.Text = "&ABC";

			Assert.AreEqual (true, tsi.PublicProcessMnemonic ('A'), "A1");
			
			tsi.Text = "ABC";
			Assert.AreEqual (true, tsi.PublicProcessMnemonic ('A'), "A2");
			Assert.AreEqual (true, tsi.PublicProcessMnemonic ('B'), "A3");
			Assert.AreEqual (true, tsi.PublicProcessMnemonic ('X'), "A4");
		}
		//[Test]
		//public void PropertyAnchorAndDocking ()
		//{
		//        ToolStripItem ts = new NullToolStripItem ();

		//        ts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

		//        Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Bottom, ts.Anchor, "A1");
		//        Assert.AreEqual (DockStyle.None, ts.Dock, "A2");

		//        ts.Anchor = AnchorStyles.Left | AnchorStyles.Right;

		//        Assert.AreEqual (AnchorStyles.Left | AnchorStyles.Right, ts.Anchor, "A1");
		//        Assert.AreEqual (DockStyle.None, ts.Dock, "A2");

		//        ts.Dock = DockStyle.Left;

		//        Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, ts.Anchor, "A1");
		//        Assert.AreEqual (DockStyle.Left, ts.Dock, "A2");

		//        ts.Dock = DockStyle.None;

		//        Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, ts.Anchor, "A1");
		//        Assert.AreEqual (DockStyle.None, ts.Dock, "A2");

		//        ts.Dock = DockStyle.Top;

		//        Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, ts.Anchor, "A1");
		//        Assert.AreEqual (DockStyle.Top, ts.Dock, "A2");
		//}
		
		[Test]
		[NUnit.Framework.Category ("NotWorking")]	// Accessibility still needs some work
		public void Accessibility ()
		{
			ToolStrip ts = new ToolStrip ();
			AccessibleObject ao = ts.AccessibilityObject;

			Assert.AreEqual ("ControlAccessibleObject: Owner = " + ts.ToString (), ao.ToString (), "L");
			//Assert.AreEqual (Rectangle.Empty, ao.Bounds, "L1");
			Assert.AreEqual (null, ao.DefaultAction, "L2");
			Assert.AreEqual (null, ao.Description, "L3");
			Assert.AreEqual (null, ao.Help, "L4");
			Assert.AreEqual (null, ao.KeyboardShortcut, "L5");
			Assert.AreEqual (null, ao.Name, "L6");
			//Assert.AreEqual (null, ao.Parent, "L7");
			Assert.AreEqual (AccessibleRole.ToolBar, ao.Role, "L8");
			Assert.AreEqual (AccessibleStates.Focusable, ao.State, "L9");
			Assert.AreEqual (null, ao.Value, "L10");

			ts.Name = "Label1";
			ts.Text = "Test Label";
			ts.AccessibleDescription = "Label Desc";

			//Assert.AreEqual (Rectangle.Empty, ao.Bounds, "L11");
			Assert.AreEqual (null, ao.DefaultAction, "L12");
			Assert.AreEqual ("Label Desc", ao.Description, "L13");
			Assert.AreEqual (null, ao.Help, "L14");
			Assert.AreEqual (null, ao.KeyboardShortcut, "L15");
			//Assert.AreEqual ("Test Label", ao.Name, "L16");
			//Assert.AreEqual (null, ao.Parent, "L17");
			Assert.AreEqual (AccessibleRole.ToolBar, ao.Role, "L18");
			Assert.AreEqual (AccessibleStates.Focusable, ao.State, "L19");
			Assert.AreEqual (null, ao.Value, "L20");

			ts.AccessibleName = "Access Label";
			Assert.AreEqual ("Access Label", ao.Name, "L21");

			ts.Text = "Test Label";
			Assert.AreEqual ("Access Label", ao.Name, "L22");

			ts.AccessibleDefaultActionDescription = "AAA";
			Assert.AreEqual ("AAA", ts.AccessibleDefaultActionDescription, "L23");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]	// Font dependent, values are for win32
		public void BehaviorAutoSize ()
		{
			// Lots of things depend on this, it needs to be 100% correct...
			ToolStripItem tsi = new NullToolStripItem ();

			string string1 = "ABCDEFG";
			string string2 = "qwertyuiop--123456";
			Font f1 = tsi.Font;
			Font f2 = new Font ("Arial", 14);
			Size string1size = TextRenderer.MeasureText (string1, f1);
			Size string2size = TextRenderer.MeasureText (string2, f1);
			Size string1size2 = TextRenderer.MeasureText (string1, f2);
			Size string2size2 = TextRenderer.MeasureText (string2, f2);
			Image i = new Bitmap (16, 16);
			Image i2 = new Bitmap (22, 22);

			Assert.AreEqual (new Size (4, 4), tsi.GetPreferredSize (Size.Empty), "K1");

			// Text only
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size.Width + 4, string1size.Height + 4), tsi.GetPreferredSize (Size.Empty), "K2");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 4, string1size.Height + 4), tsi.GetPreferredSize (Size.Empty), "K3");

			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 4, string1size2.Height + 4), tsi.GetPreferredSize (Size.Empty), "K4");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 4, string1size2.Height + 4), tsi.GetPreferredSize (Size.Empty), "K5");

			// Text and image
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size.Width + 20, string1size.Height + 7), tsi.GetPreferredSize (Size.Empty), "K6");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 20, string2size.Height + 7), tsi.GetPreferredSize (Size.Empty), "K7");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 26, Math.Max (string1size2.Height + 4, 26)), tsi.GetPreferredSize (Size.Empty), "K8");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 26, Math.Max (string1size2.Height + 4, 26)), tsi.GetPreferredSize (Size.Empty), "K9");

			// Image only
			tsi.Image = i;
			tsi.Text = string.Empty;

			Assert.AreEqual (new Size (20, 20), tsi.GetPreferredSize (Size.Empty), "K10");

			tsi.Image = i2;
			Assert.AreEqual (new Size (26, 26), tsi.GetPreferredSize (Size.Empty), "K11");

			// DisplayStyle = text
			tsi.Image = null;
			tsi.Text = string1;
			tsi.Font = f1;
			tsi.DisplayStyle = ToolStripItemDisplayStyle.Text;

			Assert.AreEqual (new Size (string1size.Width + 4, string1size.Height + 4), tsi.GetPreferredSize (Size.Empty), "K12");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 4, string1size.Height + 4), tsi.GetPreferredSize (Size.Empty), "K13");

			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 4, string1size2.Height + 4), tsi.GetPreferredSize (Size.Empty), "K14");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 4, string1size2.Height + 4), tsi.GetPreferredSize (Size.Empty), "K15");

			// DisplayStyle = image
			tsi.Image = i;
			tsi.Text = string.Empty;
			tsi.DisplayStyle = ToolStripItemDisplayStyle.Image;

			Assert.AreEqual (new Size (20, 20), tsi.GetPreferredSize (Size.Empty), "K16");

			tsi.Image = i2;
			Assert.AreEqual (new Size (26, 26), tsi.GetPreferredSize (Size.Empty), "K17");

			// DisplayStyle = textandimage, imagebeforetext
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			tsi.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
			Assert.AreEqual (new Size (string1size.Width + 20, 20), tsi.GetPreferredSize (Size.Empty), "K18");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 20, 20), tsi.GetPreferredSize (Size.Empty), "K19");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 26, Math.Max (string1size2.Height + 4, 26)), tsi.GetPreferredSize (Size.Empty), "K20");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 26, Math.Max (string1size2.Height + 4, 26)), tsi.GetPreferredSize (Size.Empty), "K21");

			// DisplayStyle = textandimage, TextBeforeImage
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			tsi.TextImageRelation = TextImageRelation.TextBeforeImage;
			Assert.AreEqual (new Size (string1size.Width + 20, 20), tsi.GetPreferredSize (Size.Empty), "K22");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 20, 20), tsi.GetPreferredSize (Size.Empty), "K23");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 26, Math.Max (string1size2.Height + 4, 26)), tsi.GetPreferredSize (Size.Empty), "K24");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 26, Math.Max (string1size2.Height + 4, 26)), tsi.GetPreferredSize (Size.Empty), "K25");

			// DisplayStyle = textandimage, overlay
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			tsi.TextImageRelation = TextImageRelation.Overlay;
			Assert.AreEqual (new Size (string1size.Width + 4, 20), tsi.GetPreferredSize (Size.Empty), "K26");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 4, 20), tsi.GetPreferredSize (Size.Empty), "K27");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 4, Math.Max (string1size2.Height + 4, 26)), tsi.GetPreferredSize (Size.Empty), "K28");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 4, Math.Max (string1size2.Height + 4, 26)), tsi.GetPreferredSize (Size.Empty), "K29");

			// DisplayStyle = textandimage, TextAboveImage
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			tsi.TextImageRelation = TextImageRelation.TextAboveImage;
			Assert.AreEqual (new Size (string1size.Width + 4, string1size.Height + tsi.Image.Height + 4), tsi.GetPreferredSize (Size.Empty), "K30");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 4, string2size.Height + tsi.Image.Height + 4), tsi.GetPreferredSize (Size.Empty), "K31");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 4, string1size2.Height + tsi.Image.Height + 4), tsi.GetPreferredSize (Size.Empty), "K32");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 4, string2size2.Height + tsi.Image.Height + 4), tsi.GetPreferredSize (Size.Empty), "K33");

			// DisplayStyle = textandimage, ImageAboveText
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			tsi.TextImageRelation = TextImageRelation.ImageAboveText;
			Assert.AreEqual (new Size (string1size.Width + 4, string1size.Height + tsi.Image.Height + 4), tsi.GetPreferredSize (Size.Empty), "K34");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 4, string2size.Height + tsi.Image.Height + 4), tsi.GetPreferredSize (Size.Empty), "K35");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 4, string1size2.Height + tsi.Image.Height + 4), tsi.GetPreferredSize (Size.Empty), "K36");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 4, string2size2.Height + tsi.Image.Height + 4), tsi.GetPreferredSize (Size.Empty), "K37");
		}

		[Test]
		public void BehaviorAvailableAndVisible ()
		{
			ToolStrip ts = new ToolStrip ();
			ToolStripItem tsi = new NullToolStripItem ();

			Assert.AreEqual (true, ts.Visible, "B1");
			Assert.AreEqual (false, tsi.Visible, "B2");
			Assert.AreEqual (true, tsi.Available, "B3");

			tsi.Available = false;

			Assert.AreEqual (false, tsi.Visible, "B4");
			Assert.AreEqual (false, tsi.Available, "B5");

			tsi.Visible = true;

			Assert.AreEqual (false, tsi.Visible, "B6");
			Assert.AreEqual (true, tsi.Available, "B7");

			tsi = new NullToolStripItem ();
			ts.Items.Add (tsi);

			Assert.AreEqual (true, ts.Visible, "B8");
			
			// FIXME: I don't understand this, the parent is visible, the item
			// is available, but yet it isn't visible?
			//Assert.AreEqual (false, tsi.Visible, "B9");
			Assert.AreEqual (true, tsi.Available, "B10");

			ts.Visible = false;

			Assert.AreEqual (false, ts.Visible, "B11");
			Assert.AreEqual (false, tsi.Visible, "B12");
			Assert.AreEqual (true, tsi.Available, "B13");

			ts.Visible = true;
			tsi.Visible = false;

			Assert.AreEqual (true, ts.Visible, "B14");
			Assert.AreEqual (false, tsi.Visible, "B15");
			Assert.AreEqual (false, tsi.Available, "B16");

			tsi.Visible = true;

			Assert.AreEqual (true, tsi.Visible, "B17");
			Assert.AreEqual (true, tsi.Available, "B18");

			tsi.Available = false;

			Assert.AreEqual (false, tsi.Visible, "B19");
			Assert.AreEqual (false, tsi.Available, "B20");
		}
		
		[Test]
		public void BehaviorBackColor ()
		{
			ToolStrip ts = new ToolStrip ();
			ToolStripItem tsi = new NullToolStripItem ();

			ts.Items.Add (tsi);

			Assert.AreEqual (SystemColors.Control, ts.BackColor, "C1");
			Assert.AreEqual (SystemColors.Control, tsi.BackColor, "C2");

			ts.BackColor = Color.BlueViolet;

			Assert.AreEqual (Color.BlueViolet, ts.BackColor, "C3");
			Assert.AreEqual (SystemColors.Control, tsi.BackColor, "C4");

			tsi.BackColor = Color.Snow;

			Assert.AreEqual (Color.BlueViolet, ts.BackColor, "C5");
			Assert.AreEqual (Color.Snow, tsi.BackColor, "C6");

			tsi.ResetBackColor ();

			Assert.AreEqual (SystemColors.Control, tsi.BackColor, "C7");
		}

		[Test]
		public void BehaviorEnabled ()
		{
			ToolStrip ts = new ToolStrip ();
			ToolStripItem tsi = new NullToolStripItem ();

			ts.Items.Add (tsi);
			
			Assert.AreEqual (true, ts.Enabled, "A1");
			Assert.AreEqual (true, tsi.Enabled, "A2");
			
			tsi.Enabled = false;

			Assert.AreEqual (true, ts.Enabled, "A3");
			Assert.AreEqual (false, tsi.Enabled, "A4");

			ts.Enabled = false;

			Assert.AreEqual (false, ts.Enabled, "A5");
			Assert.AreEqual (false, tsi.Enabled, "A6");
			
			tsi.Enabled = true;

			Assert.AreEqual (false, ts.Enabled, "A7");
			Assert.AreEqual (false, tsi.Enabled, "A8");
		}
		
		[Test]
		public void BehaviorImageList ()
		{
			// Basically, this shows that whichever of [Image|ImageIndex|ImageKey]
			// is set last resets the others to their default state
			ToolStripItem tsi = new NullToolStripItem ();
			
			Bitmap i1 = new Bitmap (16, 16);
			i1.SetPixel (0, 0, Color.Blue);
			Bitmap i2 = new Bitmap (16, 16);
			i2.SetPixel (0, 0, Color.Red);
			Bitmap i3 = new Bitmap (16, 16);
			i3.SetPixel (0, 0, Color.Green);
			
			Assert.AreEqual (null, tsi.Image, "D1");
			Assert.AreEqual (-1, tsi.ImageIndex, "D2");
			Assert.AreEqual (string.Empty, tsi.ImageKey, "D3");
			
			ImageList il = new ImageList ();
			il.Images.Add ("i2", i2);
			il.Images.Add ("i3", i3);
			
			ToolStrip ts = new ToolStrip ();
			ts.ImageList = il;
			
			ts.Items.Add (tsi);
	
			tsi.ImageKey = "i3";
			Assert.AreEqual (-1, tsi.ImageIndex, "D4");
			Assert.AreEqual ("i3", tsi.ImageKey, "D5");
			Assert.AreEqual (i3.GetPixel (0, 0), (tsi.Image as Bitmap).GetPixel (0, 0), "D6");

			tsi.ImageIndex = 0;
			Assert.AreEqual (0, tsi.ImageIndex, "D7");
			Assert.AreEqual (string.Empty, tsi.ImageKey, "D8");
			Assert.AreEqual (i2.GetPixel (0, 0), (tsi.Image as Bitmap).GetPixel (0, 0), "D9");

			tsi.Image = i1;
			Assert.AreEqual (-1, tsi.ImageIndex, "D10");
			Assert.AreEqual (string.Empty, tsi.ImageKey, "D11");
			Assert.AreEqual (i1.GetPixel (0, 0), (tsi.Image as Bitmap).GetPixel (0, 0), "D12");
			
			tsi.Image = null;
			Assert.AreEqual (null, tsi.Image, "D13");
			Assert.AreEqual (-1, tsi.ImageIndex, "D14");
			Assert.AreEqual (string.Empty, tsi.ImageKey, "D15");
			
			// Also, Image is not cached, changing the underlying ImageList image is reflected
			tsi.ImageIndex = 0;
			il.Images[0] = i1;
			Assert.AreEqual (i1.GetPixel (0, 0), (tsi.Image as Bitmap).GetPixel (0, 0), "D16");
		}
		
		[Test]	// This should not crash
		public void BehaviorImageListBadIndex ()
		{
			Form f = new Form ();
			ToolStrip ts = new ToolStrip ();
			ImageList il = new ImageList ();

			ts.ImageList = il;

			ts.Items.Add ("Hey").ImageIndex = 3;

			Image i = ts.Items[0].Image;
			
			f.Controls.Add (ts);

			f.Show ();
			f.Dispose ();
		}
		
		private class EventWatcher
		{
			private string events = string.Empty;
			
			public EventWatcher (ToolStripItem tsi)
			{
				tsi.AvailableChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("AvailableChanged;"); });
				tsi.BackColorChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("BackColorChanged;"); });
				tsi.Click += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Click;"); });
				tsi.DisplayStyleChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("DisplayStyleChanged;"); });
				tsi.DoubleClick += new EventHandler (delegate (Object obj, EventArgs e) { events += ("DoubleClick;"); });
				//tsi.DragDrop += new DragEventHandler (delegate (Object obj, DragEventArgs e) { events += ("DragDrop;"); });
				//tsi.DragEnter += new DragEventHandler (delegate (Object obj, DragEventArgs e) { events += ("DragEnter;"); });
				//tsi.DragLeave += new EventHandler (delegate (Object obj, EventArgs e) { events += ("DragLeave;"); });
				//tsi.DragOver += new DragEventHandler (delegate (Object obj, DragEventArgs e) { events += ("DragOver;"); });
				tsi.EnabledChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("EnabledChanged;"); });
				tsi.ForeColorChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("ForeColorChanged;"); });
				//tsi.GiveFeedback += new GiveFeedbackEventHandler (delegate (Object obj, GiveFeedbackEventArgs e) { events += ("GiveFeedback;"); });
				tsi.LocationChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("LocationChanged;"); });
				tsi.MouseDown += new MouseEventHandler (delegate (Object obj, MouseEventArgs e) { events += ("MouseDown;"); });
				tsi.MouseEnter += new EventHandler (delegate (Object obj, EventArgs e) { events += ("MouseEnter;"); });
				tsi.MouseHover += new EventHandler (delegate (Object obj, EventArgs e) { events += ("MouseHover;"); });
				tsi.MouseLeave += new EventHandler (delegate (Object obj, EventArgs e) { events += ("MouseLeave;"); });
				tsi.MouseMove += new MouseEventHandler (delegate (Object obj, MouseEventArgs e) { events += ("MouseMove;"); });
				tsi.MouseUp += new MouseEventHandler (delegate (Object obj, MouseEventArgs e) { events += ("MouseUp;"); });
				tsi.OwnerChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("OwnerChanged;"); });
				tsi.Paint += new PaintEventHandler (delegate (Object obj, PaintEventArgs e) { events += ("Paint;"); });
				//tsi.QueryAccessibilityHelp += new QueryAccessibilityHelpEventHandler (delegate (Object obj, QueryAccessibilityHelpEventArgs e) { events += ("QueryAccessibilityHelp;"); });
				//tsi.QueryContinueDrag += new QueryContinueDragEventHandler (delegate (Object obj, QueryContinueDragEventArgs e) { events += ("QueryContinueDrag;"); });
				tsi.RightToLeftChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("RightToLeftChanged;"); });
				tsi.TextChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("TextChanged;"); });
				tsi.VisibleChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("VisibleChanged;"); });
			}

			public override string ToString ()
			{
				return events.TrimEnd (';');
			}
			
			public void Clear ()
			{
				events = string.Empty;
			}
		}
		
		private class NullToolStripItem : ToolStripItem
		{
			public NullToolStripItem () : base () {}
			public NullToolStripItem (string text, Image image, EventHandler onClick) : base (text, image, onClick) { }
			public NullToolStripItem (string text, Image image, EventHandler onClick, string name) : base (text, image, onClick, name) { }
			
			public bool PublicProcessMnemonic (char charCode) { return base.ProcessMnemonic (charCode); }
		}
		
		private class ExposeProtectedProperties : ToolStripItem
		{
			public new bool DefaultAutoToolTip { get { return base.DefaultAutoToolTip; } }
			public new ToolStripItemDisplayStyle DefaultDisplayStyle { get { return base.DefaultDisplayStyle; } }
			public new Padding DefaultMargin { get { return base.DefaultMargin; } }
			public new Padding DefaultPadding { get { return base.DefaultPadding; } }
			public new Size DefaultSize { get { return base.DefaultSize; } }
			public new bool DismissWhenClicked { get { return base.DismissWhenClicked; } }
			public new ToolStrip Parent { get { return base.Parent; } set { base.Parent = value; } }
			public new bool ShowKeyboardCues { get { return base.ShowKeyboardCues; } }
		}
	}
}
