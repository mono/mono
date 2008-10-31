//
// ToolStripButtonTests.cs
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
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolStripButtonTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripButton tsi = new ToolStripButton ();

			Assert.AreEqual (true, tsi.AutoToolTip, "A1");
			Assert.AreEqual (true, tsi.CanSelect, "A2");
			Assert.AreEqual (false, tsi.Checked, "A3");
			Assert.AreEqual (false, tsi.CheckOnClick, "A4");
			Assert.AreEqual (CheckState.Unchecked, tsi.CheckState, "A5");

			int count = 0;
			EventHandler oc = new EventHandler (delegate (object sender, EventArgs e) { count++; });
			Image i = new Bitmap (1,1);
			
			tsi = new ToolStripButton (i);
			tsi.PerformClick();
			Assert.AreEqual (null, tsi.Text, "A6");
			Assert.AreSame (i, tsi.Image, "A7");
			Assert.AreEqual (0, count, "A8");
			Assert.AreEqual (string.Empty, tsi.Name, "A9");

			tsi = new ToolStripButton ("Text");
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A10");
			Assert.AreSame (null, tsi.Image, "A11");
			Assert.AreEqual (0, count, "A12");
			Assert.AreEqual (string.Empty, tsi.Name, "A13");

			tsi = new ToolStripButton ("Text", i);
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A14");
			Assert.AreSame (i, tsi.Image, "A15");
			Assert.AreEqual (0, count, "A16");
			Assert.AreEqual (string.Empty, tsi.Name, "A17");

			tsi = new ToolStripButton ("Text", i, oc);
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A18");
			Assert.AreSame (i, tsi.Image, "A19");
			Assert.AreEqual (1, count, "A20");
			Assert.AreEqual (string.Empty, tsi.Name, "A21");

			tsi = new ToolStripButton ("Text", i, oc, "Name");
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A22");
			Assert.AreSame (i, tsi.Image, "A23");
			Assert.AreEqual (2, count, "A24");
			Assert.AreEqual ("Name", tsi.Name, "A25");
		}

		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (true, epp.DefaultAutoToolTip, "C1");
		}

		[Test]
		public void PropertyAutoToolTip ()
		{
			ToolStripButton tsi = new ToolStripButton ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.AutoToolTip = true;
			Assert.AreEqual (true, tsi.AutoToolTip, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.AutoToolTip = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyChecked ()
		{
			ToolStripButton tsi = new ToolStripButton ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Checked = true;
			Assert.AreEqual (true, tsi.Checked, "B1");
			Assert.AreEqual ("CheckedChanged;CheckStateChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.Checked = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyCheckOnClick ()
		{
			ToolStripButton tsi = new ToolStripButton ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.CheckOnClick = true;
			Assert.AreEqual (true, tsi.CheckOnClick, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.CheckOnClick = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyCheckState ()
		{
			ToolStripButton tsi = new ToolStripButton ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.CheckState = CheckState.Checked;
			Assert.AreEqual (CheckState.Checked, tsi.CheckState, "B1");
			Assert.AreEqual ("CheckedChanged;CheckStateChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.CheckState = CheckState.Checked;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyCheckStateIEAE ()
		{
			ToolStripButton tsi = new ToolStripButton ();
			tsi.CheckState = (CheckState)42;
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
			ToolStripButton tsi = new ToolStripButton ();
			AccessibleObject ao = tsi.AccessibilityObject;

			Assert.AreEqual ("ToolStripItemAccessibleObject: Owner = " + tsi.ToString (), ao.ToString (), "L");
			Assert.AreEqual (Rectangle.Empty, ao.Bounds, "L1");
			Assert.AreEqual ("Press", ao.DefaultAction, "L2");
			Assert.AreEqual (null, ao.Description, "L3");
			Assert.AreEqual (null, ao.Help, "L4");
			Assert.AreEqual (string.Empty, ao.KeyboardShortcut, "L5");
			Assert.AreEqual (string.Empty, ao.Name, "L6");
			Assert.AreEqual (null, ao.Parent, "L7");
			Assert.AreEqual (AccessibleRole.PushButton, ao.Role, "L8");
			Assert.AreEqual (AccessibleStates.Focusable, ao.State, "L9");
			Assert.AreEqual (string.Empty, ao.Value, "L10");

			tsi.Name = "Label1";
			tsi.Text = "Test Label";
			tsi.AccessibleDescription = "Label Desc";

			Assert.AreEqual (Rectangle.Empty, ao.Bounds, "L11");
			Assert.AreEqual ("Press", ao.DefaultAction, "L12");
			Assert.AreEqual ("Label Desc", ao.Description, "L13");
			Assert.AreEqual (null, ao.Help, "L14");
			Assert.AreEqual (string.Empty, ao.KeyboardShortcut, "L15");
			Assert.AreEqual ("Test Label", ao.Name, "L16");
			Assert.AreEqual (null, ao.Parent, "L17");
			Assert.AreEqual (AccessibleRole.PushButton, ao.Role, "L18");
			Assert.AreEqual (AccessibleStates.Focusable, ao.State, "L19");
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
			ToolStripItem tsi = new ToolStripButton ();

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

			Assert.AreEqual (new Size (23, 4), tsi.GetPreferredSize (Size.Empty), "K1");

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

			Assert.AreEqual (new Size (23, 20), tsi.GetPreferredSize (Size.Empty), "K10");

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

			Assert.AreEqual (new Size (23, 20), tsi.GetPreferredSize (Size.Empty), "K16");

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

		private class EventWatcher
		{
			private string events = string.Empty;
			
			public EventWatcher (ToolStripButton tsi)
			{
				tsi.CheckedChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("CheckedChanged;"); });
				tsi.CheckStateChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("CheckStateChanged;"); });
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
		
		private class ExposeProtectedProperties : ToolStripButton
		{
			public new bool DefaultAutoToolTip { get { return base.DefaultAutoToolTip; } }
		}
	}
}
#endif