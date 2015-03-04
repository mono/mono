//
// ToolStripControlHostTests.cs
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
	public class ToolStripControlHostTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			Control t = new Control ();
			ToolStripControlHost tsi = new ToolStripControlHost (t);

			Assert.AreEqual (SystemColors.Control, tsi.BackColor, "A1");
			Assert.AreEqual (null, tsi.BackgroundImage, "A2");
			Assert.AreEqual (ImageLayout.Tile, tsi.BackgroundImageLayout, "A3");
			Assert.AreEqual (true, tsi.CanSelect, "A4");
			Assert.AreEqual (true, tsi.CausesValidation, "A5");
			Assert.AreSame (t, tsi.Control, "A6");
			Assert.AreEqual (ContentAlignment.MiddleCenter, tsi.ControlAlign, "A7");
			Assert.AreEqual (true, tsi.Enabled, "A8");
			Assert.AreEqual (false, tsi.Focused, "A9");
			Assert.AreEqual (t.Font, tsi.Font, "A10");
			Assert.AreEqual (SystemColors.ControlText, tsi.ForeColor, "A11");
			Assert.AreEqual (RightToLeft.No, tsi.RightToLeft, "A12");
			Assert.AreEqual (false, tsi.Selected, "A13");
			Assert.AreEqual (null, tsi.Site, "A14");
			Assert.AreEqual (new Size (0, 0), tsi.Size, "A15");
			Assert.AreEqual (string.Empty, tsi.Text, "A16");

			tsi = new ToolStripControlHost (t, "Bob");
			Assert.AreEqual ("Bob", tsi.Name, "A17");
			Assert.AreSame (t, tsi.Control, "A18");
			Assert.AreEqual (string.Empty, tsi.Control.Name, "A19");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorANE ()
		{
			new ToolStripControlHost (null); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorANE2 ()
		{
			new ToolStripControlHost (null, string.Empty);
		}
	
		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (new Size (0, 0), epp.DefaultSize, "C1");
		}

		[Test]
		public void PropertyBackColor ()
		{
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
			EventWatcher ew = new EventWatcher (tsi);

			tsi.BackColor = Color.BurlyWood;
			Assert.AreEqual (Color.BurlyWood, tsi.BackColor, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.BackColor = Color.BurlyWood;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
			
			// BackColor initially comes from hosted control
			tsi = new ToolStripControlHost (new Button ());
			Assert.AreEqual (SystemColors.Control, tsi.BackColor, "B4");

			tsi = new ToolStripControlHost (new TextBox ());
			Assert.AreEqual (SystemColors.Window, tsi.BackColor, "B5");

			tsi = new ToolStripControlHost (new ProgressBar ());
			Assert.AreEqual (SystemColors.Control, tsi.BackColor, "B6");
		}

		[Test]
		public void PropertyBackgroundImage ()
		{
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
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
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
			EventWatcher ew = new EventWatcher (tsi);

			tsi.BackgroundImageLayout = ImageLayout.Zoom;
			Assert.AreEqual (ImageLayout.Zoom, tsi.BackgroundImageLayout, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.BackgroundImageLayout = ImageLayout.Zoom;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyCausesValidation ()
		{
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
			EventWatcher ew = new EventWatcher (tsi);

			tsi.CausesValidation = false;
			Assert.AreEqual (false, tsi.CausesValidation, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.CausesValidation = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyControlAlign ()
		{
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ControlAlign = ContentAlignment.TopRight ;
			Assert.AreEqual (ContentAlignment.TopRight, tsi.ControlAlign, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.ControlAlign = ContentAlignment.TopRight;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyDefaultSize ()
		{
			VariableSizeControl c = new VariableSizeControl ();
			ExposeProtectedProperties epp = new ExposeProtectedProperties (c);

			Assert.AreEqual (new Size (-1, -1), epp.DefaultSize, "#A1");

			c.Size = new Size (666, 666);
			Assert.AreEqual (new Size (666, 666), c.Size, "#B99");
			Assert.AreEqual (new Size (666, 666), epp.DefaultSize, "#B1");
		}

		[Test]
		public void PropertyEnabled ()
		{
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Enabled = false;
			Assert.AreEqual (false, tsi.Enabled, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Enabled = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyFont ()
		{
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
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
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ForeColor = Color.BurlyWood;
			Assert.AreEqual (Color.BurlyWood, tsi.ForeColor, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.ForeColor = Color.BurlyWood;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");

			// CausesValidation initially comes from hosted control
			tsi = new ToolStripControlHost (new Button ());
			Assert.AreEqual (SystemColors.ControlText, tsi.ForeColor, "B4");

			tsi = new ToolStripControlHost (new TextBox ());
			Assert.AreEqual (SystemColors.WindowText, tsi.ForeColor, "B5");
		}

		[Test]
		public void PropertyRightToLeft ()
		{
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
			EventWatcher ew = new EventWatcher (tsi);

			tsi.RightToLeft = RightToLeft.Yes;
			Assert.AreEqual (RightToLeft.Yes, tsi.RightToLeft, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.RightToLeft = RightToLeft.Yes;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertySite ()
		{
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
			EventWatcher ew = new EventWatcher (tsi);

			ISite i = new Form ().Site;
			tsi.Site = i;
			Assert.AreSame (i, tsi.Site, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Site = i;
			Assert.AreSame (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertySize ()
		{
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Size = new Size (42, 42);
			Assert.AreEqual (new Size (42, 42), tsi.Size, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Size = new Size (42, 42);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyText ()
		{
			ToolStripControlHost tsi = new ToolStripControlHost (new Control ());
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Text = "Text";
			Assert.AreEqual ("Text", tsi.Text, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Text = "Text";
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
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
			ToolStripControlHost tsi = new ToolStripControlHost (new Button ());
			AccessibleObject ao = tsi.AccessibilityObject;

			Assert.AreEqual ("Press", ao.DefaultAction, "L2");
			Assert.AreEqual (null, ao.Description, "L3");
			Assert.AreEqual (null, ao.Help, "L4");
			Assert.AreEqual (null, ao.KeyboardShortcut, "L5");
			Assert.AreEqual (null, ao.Name, "L6");
			Assert.AreEqual (AccessibleRole.PushButton, ao.Role, "L8");
			Assert.AreEqual (AccessibleStates.None, ao.State, "L9");
			Assert.AreEqual (null, ao.Value, "L10");

			tsi.Name = "Label1";
			tsi.Text = "Test Label";
			tsi.AccessibleDescription = "Label Desc";

			Assert.AreEqual ("Press", ao.DefaultAction, "L12");
			Assert.AreEqual ("Label Desc", ao.Description, "L13");
			Assert.AreEqual (null, ao.Help, "L14");
			Assert.AreEqual (null, ao.KeyboardShortcut, "L15");
			//Assert.AreEqual ("Test Label", ao.Name, "L16");
			Assert.AreEqual (AccessibleRole.PushButton, ao.Role, "L18");
			Assert.AreEqual (AccessibleStates.None, ao.State, "L19");
			Assert.AreEqual (null, ao.Value, "L20");

			tsi.AccessibleName = "Access Label";
			Assert.AreEqual ("Access Label", ao.Name, "L21");

			tsi.Text = "Test Label";
			Assert.AreEqual ("Access Label", ao.Name, "L22");

			tsi.AccessibleDefaultActionDescription = "AAA";
			Assert.AreEqual ("AAA", tsi.AccessibleDefaultActionDescription, "L23");
		}

		[Test]
		public void BehaviorBackColor ()
		{
			ToolStrip ts = new ToolStrip ();
			ToolStripItem tsi = new ToolStripControlHost (new Control ());

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
		[Ignore ("Need some AutoSize work done in ToolStripControlHost")]
		public void BehaviorSize ()
		{
			Control c = new Control ();
			ToolStripControlHost tsi = new ToolStripControlHost (c);
			ToolStrip ts = new ToolStrip ();
			
			Assert.AreEqual (new Size (0, 0), c.Size, "H1");
			Assert.AreEqual (new Size (0, 0), tsi.Size, "H2");

			c = new TextBox ();
			tsi = new ToolStripControlHost (c);

			Assert.AreEqual (new Size (100, 20), c.Size, "H3");
			Assert.AreEqual (new Size (100, 20), tsi.Size, "H4");

			c = new ComboBox ();
			tsi = new ToolStripControlHost (c);

			Assert.AreEqual (new Size (121, 21), c.Size, "H5");
			Assert.AreEqual (new Size (121, 21), tsi.Size, "H6");

			c = new ProgressBar ();
			tsi = new ToolStripControlHost (c);

			Assert.AreEqual (new Size (100, 23), c.Size, "H7");
			Assert.AreEqual (new Size (100, 23), tsi.Size, "H8");
			
			c = new PictureBox ();
			tsi = new ToolStripControlHost (c);

			Assert.AreEqual (new Size (100, 50), c.Size, "H7");
			Assert.AreEqual (new Size (100, 50), tsi.Size, "H8");
			
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Controls.Add (ts);
			ts.Items.Add (tsi);
			f.Show ();
			Assert.AreEqual (new Size (100, 50), c.Size, "H9");
			Assert.AreEqual (new Size (100, 50), tsi.Size, "H10");
			Assert.AreEqual (new Size (292, 53), ts.Size, "H11");
		}

		[Test]
		public void MethodOnHostedControlResize ()
		{
			ToolStripControlHostChild control_host = new ToolStripControlHostChild (new Control ());
			Control c = control_host.Control;

			Assert.AreEqual (false, control_host.OnHostedControlResizeFired, "#A1");

			c.Size = new Size (666, 666);
			Assert.AreEqual (true, control_host.OnHostedControlResizeFired, "#A2");
		}

		private class ToolStripControlHostChild : ToolStripControlHost {
			bool on_hosted_control_resize_fired;

			public ToolStripControlHostChild (Control c) : base (c)
			{
			}

			protected override void OnHostedControlResize (EventArgs e)
			{
				base.OnHostedControlResize (e);
				on_hosted_control_resize_fired = true;
			}

			public bool OnHostedControlResizeFired
			{
				get
				{
					return on_hosted_control_resize_fired;
				}
			}
		}

		private class EventWatcher
		{
			private string events = string.Empty;
			
			public EventWatcher (ToolStripControlHost tsi)
			{
				tsi.Enter += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Enter;"); });
				tsi.GotFocus += new EventHandler (delegate (Object obj, EventArgs e) { events += ("GotFocus;"); });
				tsi.KeyDown += new KeyEventHandler (delegate (Object obj, KeyEventArgs e) { events += ("KeyDown;"); });
				tsi.KeyPress += new KeyPressEventHandler (delegate (Object obj, KeyPressEventArgs e) { events += ("KeyPress;"); });
				tsi.KeyUp += new KeyEventHandler (delegate (Object obj, KeyEventArgs e) { events += ("KeyUp;"); });
				tsi.Leave += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Leave;"); });
				tsi.LostFocus += new EventHandler (delegate (Object obj, EventArgs e) { events += ("LostFocus;"); });
				tsi.Validated += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Validated;"); });
				tsi.Validating += new CancelEventHandler (delegate (Object obj, CancelEventArgs e) { events += ("Validating;"); });
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
		
		private class ExposeProtectedProperties : ToolStripControlHost
		{
			public ExposeProtectedProperties (Control c) : base (c) {}
			public ExposeProtectedProperties () : base (new Control ()) {}
			
			public new Size DefaultSize { get { return base.DefaultSize; } }
		}

		private class VariableSizeControl : Control 
		{
			public override Size GetPreferredSize (Size proposedSize)
			{
				return new Size (999, 999);
			}

			protected override Size DefaultSize {
				get {
					return new Size (-1, -1);
				}
			}
		}
	}
}
