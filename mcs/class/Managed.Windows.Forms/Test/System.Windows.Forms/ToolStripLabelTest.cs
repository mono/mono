//
// ToolStripLabelTests.cs
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
#if NET_2_0
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolStripLabelTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripLabel tsi = new ToolStripLabel ();

			Assert.AreEqual (Color.Red, tsi.ActiveLinkColor, "A1");
			Assert.AreEqual (false, tsi.CanSelect, "A2");
			Assert.AreEqual (false, tsi.IsLink, "A3");
			Assert.AreEqual (LinkBehavior.SystemDefault, tsi.LinkBehavior, "A4");
			Assert.AreEqual (Color.FromArgb (0,0,255), tsi.LinkColor, "A5");
			Assert.AreEqual (false, tsi.LinkVisited, "A6");
			Assert.AreEqual (Color.FromArgb (128, 0, 128), tsi.VisitedLinkColor, "A7");

			int count = 0;
			EventHandler oc = new EventHandler (delegate (object sender, EventArgs e) { count++; });
			Image i = new Bitmap (1,1);

			tsi = new ToolStripLabel (i);
			tsi.PerformClick();
			Assert.AreEqual (null, tsi.Text, "A8");
			Assert.AreSame (i, tsi.Image, "A9");
			Assert.AreEqual (false, tsi.IsLink, "A10");
			Assert.AreEqual (0, count, "A11");
			Assert.AreEqual (string.Empty, tsi.Name, "A12");

			tsi = new ToolStripLabel ("Text");
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A13");
			Assert.AreSame (null, tsi.Image, "A14");
			Assert.AreEqual (false, tsi.IsLink, "A15");
			Assert.AreEqual (0, count, "A16");
			Assert.AreEqual (string.Empty, tsi.Name, "A17");

			tsi = new ToolStripLabel ("Text", i);
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A18");
			Assert.AreSame (i, tsi.Image, "A19");
			Assert.AreEqual (false, tsi.IsLink, "A20");
			Assert.AreEqual (0, count, "A21");
			Assert.AreEqual (string.Empty, tsi.Name, "A22");

			tsi = new ToolStripLabel ("Text", i, true);
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A23");
			Assert.AreSame (i, tsi.Image, "A24");
			Assert.AreEqual (true, tsi.IsLink, "A25");
			Assert.AreEqual (0, count, "A26");
			Assert.AreEqual (string.Empty, tsi.Name, "A27");

			tsi = new ToolStripLabel ("Text", i, true, oc);
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A28");
			Assert.AreSame (i, tsi.Image, "A29");
			Assert.AreEqual (true, tsi.IsLink, "A30");
			Assert.AreEqual (1, count, "A31");
			Assert.AreEqual (string.Empty, tsi.Name, "A32");

			tsi = new ToolStripLabel ("Text", i, true, oc, "Name");
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A33");
			Assert.AreSame (i, tsi.Image, "A34");
			Assert.AreEqual (true, tsi.IsLink, "A35");
			Assert.AreEqual (2, count, "A36");
			Assert.AreEqual ("Name", tsi.Name, "A37");
		}

		[Test]
		public void PropertyActiveLinkColor ()
		{
			ToolStripLabel tsi = new ToolStripLabel ();

			tsi.ActiveLinkColor = Color.Green;
			Assert.AreEqual (Color.Green, tsi.ActiveLinkColor, "B1");
		}

		[Test]
		public void PropertyIsLink ()
		{
			ToolStripLabel tsi = new ToolStripLabel ();

			tsi.IsLink = true;
			Assert.AreEqual (true, tsi.IsLink, "B1");
		}

		[Test]
		public void PropertyLinkBehavior ()
		{
			ToolStripLabel tsi = new ToolStripLabel ();

			tsi.LinkBehavior = LinkBehavior.NeverUnderline;
			Assert.AreEqual (LinkBehavior.NeverUnderline, tsi.LinkBehavior, "B1");
		}

		[Test]
		public void PropertyLinkColor ()
		{
			ToolStripLabel tsi = new ToolStripLabel ();

			tsi.LinkColor = Color.Green;
			Assert.AreEqual (Color.Green, tsi.LinkColor, "B1");
		}

		[Test]
		public void PropertyLinkVisited ()
		{
			ToolStripLabel tsi = new ToolStripLabel ();

			tsi.LinkVisited = true;
			Assert.AreEqual (true, tsi.LinkVisited, "B1");
		}

		[Test]
		public void PropertyVisitedLinkColor ()
		{
			ToolStripLabel tsi = new ToolStripLabel ();

			tsi.VisitedLinkColor = Color.Green;
			Assert.AreEqual (Color.Green, tsi.VisitedLinkColor, "B1");
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
		[Ignore ("Accessibility still needs some work")]
		public void Accessibility ()
		{
			ToolStripLabel tsi = new ToolStripLabel ();
			AccessibleObject ao = tsi.AccessibilityObject;

			Assert.AreEqual ("ToolStripItemAccessibleObject: Owner = " + tsi.ToString (), ao.ToString (), "L");
			Assert.AreEqual (Rectangle.Empty, ao.Bounds, "L1");
			Assert.AreEqual (string.Empty, ao.DefaultAction, "L2");
			Assert.AreEqual (null, ao.Description, "L3");
			Assert.AreEqual (null, ao.Help, "L4");
			Assert.AreEqual (string.Empty, ao.KeyboardShortcut, "L5");
			Assert.AreEqual (string.Empty, ao.Name, "L6");
			Assert.AreEqual (null, ao.Parent, "L7");
			Assert.AreEqual (AccessibleRole.StaticText, ao.Role, "L8");
			Assert.AreEqual (AccessibleStates.ReadOnly, ao.State, "L9");
			Assert.AreEqual (string.Empty, ao.Value, "L10");

			tsi.Name = "Label1";
			tsi.Text = "Test Label";
			tsi.AccessibleDescription = "Label Desc";

			Assert.AreEqual (Rectangle.Empty, ao.Bounds, "L11");
			Assert.AreEqual (string.Empty, ao.DefaultAction, "L12");
			Assert.AreEqual ("Label Desc", ao.Description, "L13");
			Assert.AreEqual (null, ao.Help, "L14");
			Assert.AreEqual (string.Empty, ao.KeyboardShortcut, "L15");
			Assert.AreEqual ("Test Label", ao.Name, "L16");
			Assert.AreEqual (null, ao.Parent, "L17");
			Assert.AreEqual (AccessibleRole.StaticText, ao.Role, "L18");
			Assert.AreEqual (AccessibleStates.ReadOnly, ao.State, "L19");
			Assert.AreEqual (string.Empty, ao.Value, "L20");

			tsi.AccessibleName = "Access Label";
			Assert.AreEqual ("Access Label", ao.Name, "L21");

			tsi.Text = "Test Label";
			Assert.AreEqual ("Access Label", ao.Name, "L22");

			tsi.AccessibleDefaultActionDescription = "AAA";
			Assert.AreEqual ("AAA", tsi.AccessibleDefaultActionDescription, "L23");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]	// Font dependent, values are for win32
		public void BehaviorAutoSize ()
		{
			// Lots of things depend on this, it needs to be 100% correct...
			ToolStripItem tsi = new ToolStripLabel ();

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

			Assert.AreEqual (new Size (0, 0), tsi.GetPreferredSize (Size.Empty), "K1");

			// Text only
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size.Width, string1size.Height), tsi.GetPreferredSize (Size.Empty), "K2");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width, string1size.Height), tsi.GetPreferredSize (Size.Empty), "K3");

			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width, string1size2.Height), tsi.GetPreferredSize (Size.Empty), "K4");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width, string1size2.Height), tsi.GetPreferredSize (Size.Empty), "K5");

			// Text and image
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size.Width + 16, string1size.Height + 3), tsi.GetPreferredSize (Size.Empty), "K6");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 16, string2size.Height + 3), tsi.GetPreferredSize (Size.Empty), "K7");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 22, Math.Max (string1size2.Height, 22)), tsi.GetPreferredSize (Size.Empty), "K8");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 22, Math.Max (string1size2.Height, 22)), tsi.GetPreferredSize (Size.Empty), "K9");

			// Image only
			tsi.Image = i;
			tsi.Text = string.Empty;

			Assert.AreEqual (new Size (16, 16), tsi.GetPreferredSize (Size.Empty), "K10");

			tsi.Image = i2;
			Assert.AreEqual (new Size (22, 22), tsi.GetPreferredSize (Size.Empty), "K11");

			// DisplayStyle = text
			tsi.Image = null;
			tsi.Text = string1;
			tsi.Font = f1;
			tsi.DisplayStyle = ToolStripItemDisplayStyle.Text;

			Assert.AreEqual (new Size (string1size.Width, string1size.Height), tsi.GetPreferredSize (Size.Empty), "K12");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width, string1size.Height), tsi.GetPreferredSize (Size.Empty), "K13");

			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width, string1size2.Height), tsi.GetPreferredSize (Size.Empty), "K14");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width, string1size2.Height), tsi.GetPreferredSize (Size.Empty), "K15");

			// DisplayStyle = image
			tsi.Image = i;
			tsi.Text = string.Empty;
			tsi.DisplayStyle = ToolStripItemDisplayStyle.Image;

			Assert.AreEqual (new Size (16, 16), tsi.GetPreferredSize (Size.Empty), "K16");

			tsi.Image = i2;
			Assert.AreEqual (new Size (22, 22), tsi.GetPreferredSize (Size.Empty), "K17");

			// DisplayStyle = textandimage, imagebeforetext
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			tsi.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
			Assert.AreEqual (new Size (string1size.Width + 16, 16), tsi.GetPreferredSize (Size.Empty), "K18");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 16, 16), tsi.GetPreferredSize (Size.Empty), "K19");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 22, Math.Max (string1size2.Height, 22)), tsi.GetPreferredSize (Size.Empty), "K20");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 22, Math.Max (string1size2.Height, 22)), tsi.GetPreferredSize (Size.Empty), "K21");

			// DisplayStyle = textandimage, TextBeforeImage
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			tsi.TextImageRelation = TextImageRelation.TextBeforeImage;
			Assert.AreEqual (new Size (string1size.Width + 16, 16), tsi.GetPreferredSize (Size.Empty), "K22");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width + 16, 16), tsi.GetPreferredSize (Size.Empty), "K23");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width + 22, Math.Max (string1size2.Height, 22)), tsi.GetPreferredSize (Size.Empty), "K24");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width + 22, Math.Max (string1size2.Height, 22)), tsi.GetPreferredSize (Size.Empty), "K25");

			// DisplayStyle = textandimage, overlay
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			tsi.TextImageRelation = TextImageRelation.Overlay;
			Assert.AreEqual (new Size (string1size.Width, 16), tsi.GetPreferredSize (Size.Empty), "K26");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width, 16), tsi.GetPreferredSize (Size.Empty), "K27");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width, Math.Max (string1size2.Height, 22)), tsi.GetPreferredSize (Size.Empty), "K28");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width, Math.Max (string1size2.Height, 22)), tsi.GetPreferredSize (Size.Empty), "K29");

			// DisplayStyle = textandimage, TextAboveImage
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			tsi.TextImageRelation = TextImageRelation.TextAboveImage;
			Assert.AreEqual (new Size (string1size.Width, string1size.Height + tsi.Image.Height), tsi.GetPreferredSize (Size.Empty), "K30");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width, string2size.Height + tsi.Image.Height), tsi.GetPreferredSize (Size.Empty), "K31");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width, string1size2.Height + tsi.Image.Height), tsi.GetPreferredSize (Size.Empty), "K32");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width, string2size2.Height + tsi.Image.Height), tsi.GetPreferredSize (Size.Empty), "K33");

			// DisplayStyle = textandimage, ImageAboveText
			tsi.Image = i;
			tsi.Font = f1;
			tsi.Text = string1;
			tsi.TextImageRelation = TextImageRelation.ImageAboveText;
			Assert.AreEqual (new Size (string1size.Width, string1size.Height + tsi.Image.Height), tsi.GetPreferredSize (Size.Empty), "K34");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size.Width, string2size.Height + tsi.Image.Height), tsi.GetPreferredSize (Size.Empty), "K35");

			tsi.Image = i2;
			tsi.Font = f2;
			tsi.Text = string1;
			Assert.AreEqual (new Size (string1size2.Width, string1size2.Height + tsi.Image.Height), tsi.GetPreferredSize (Size.Empty), "K36");

			tsi.Text = string2;
			Assert.AreEqual (new Size (string2size2.Width, string2size2.Height + tsi.Image.Height), tsi.GetPreferredSize (Size.Empty), "K37");
		}
	}
}
#endif