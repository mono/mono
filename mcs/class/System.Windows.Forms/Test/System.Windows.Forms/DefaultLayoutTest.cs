using System;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;
using System.Collections.Generic;
namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DefaultLayoutTest : TestHelper
	{
		int event_count;
		LayoutEventArgs most_recent_args;

		void p_Layout (object sender, LayoutEventArgs e)
		{
			event_count ++;
			most_recent_args = e;
		}

		[Test]
		public void AnchorLayoutEvents ()
		{
			Panel p;
			Button b;

			p = new Panel ();

			b = new Button ();
			p.Controls.Add (b);

			p.Layout += new LayoutEventHandler (p_Layout);

			/* set the button's anchor to something different */
			b.Anchor = AnchorStyles.Bottom;
			Assert.AreEqual (1, event_count, "1");
			Assert.AreEqual ("Anchor", most_recent_args.AffectedProperty, "2");

			/* reset it to something new with the panel's layout suspended */
			event_count = 0;
			p.SuspendLayout ();
			b.Anchor = AnchorStyles.Top;
			Assert.AreEqual (0, event_count, "3");
			p.ResumeLayout ();
			Assert.AreEqual (1, event_count, "4");
			Assert.AreEqual ("Anchor", most_recent_args.AffectedProperty, "5");

			/* with the anchor style set to something, resize the parent */
			event_count = 0;
			p.Size = new Size (500, 500);
			Assert.AreEqual (1, event_count, "6");
			Assert.AreEqual ("Bounds", most_recent_args.AffectedProperty, "7");

			/* now try it with layout suspended */
			event_count = 0;
			p.SuspendLayout ();
			p.Size = new Size (400, 400);
			Assert.AreEqual (0, event_count, "8");
			p.ResumeLayout ();
			Assert.AreEqual (1, event_count, "9");
			Assert.AreEqual ("Bounds", most_recent_args.AffectedProperty, "10");

			/* with the anchor style set to something, resize the child */
			event_count = 0;
			b.Size = new Size (100, 100);
			// On .NET Framework PerformLayout is called twice; on Mono only once
			//Assert.AreEqual (2, event_count, "11");
			Assert.AreEqual ("Bounds", most_recent_args.AffectedProperty, "12");

			/* and again with layout suspended */
			event_count = 0;
			p.SuspendLayout ();
			b.Size = new Size (200, 200);
			Assert.AreEqual (0, event_count, "13");
			p.ResumeLayout ();
			Assert.AreEqual (1, event_count, "14");
			Assert.AreEqual ("Bounds", most_recent_args.AffectedProperty, "15");

			/* change two properties when suspended */
			event_count = 0;
			p.SuspendLayout ();
			b.Anchor = AnchorStyles.Left;
			b.Size = new Size (150, 150);
			Assert.AreEqual (0, event_count, "15");
			p.ResumeLayout ();
			Assert.AreEqual (1, event_count, "16");
			Assert.AreEqual ("Bounds", most_recent_args.AffectedProperty, "17");

			/* and now in the opposite order */
			event_count = 0;
			p.SuspendLayout ();
			b.Size = new Size (100, 100);
			b.Anchor = AnchorStyles.Top;
			Assert.AreEqual (0, event_count, "18");
			p.ResumeLayout ();
			Assert.AreEqual (1, event_count, "19");
			Assert.AreEqual ("Bounds", most_recent_args.AffectedProperty, "20");
		}

		[Test]
		public void AnchorTopLeftTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.Size = new Size (200, 200);

			Button b = new Button ();
			b.Size = new Size (100, 100);
			b.Anchor = AnchorStyles.Top | AnchorStyles.Left;

			f.Controls.Add (b);

			Assert.AreEqual (0, b.Left, "1");
			Assert.AreEqual (0, b.Top, "2");
			f.Size = new Size (300, 300);

			Assert.AreEqual (0, b.Left, "3");
			Assert.AreEqual (0, b.Top, "4");
			
			f.Dispose ();
		}

		[Test]
		public void AnchorTopRightTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.Size = new Size (200, 200);

			Button b = new Button ();
			b.Size = new Size (100, 100);
			b.Anchor = AnchorStyles.Top | AnchorStyles.Right;

			f.Controls.Add (b);

			Assert.AreEqual (0, b.Left, "1");
			Assert.AreEqual (0, b.Top, "2");

			f.Size = new Size (300, 300);

			Assert.AreEqual (100, b.Left, "3");
			Assert.AreEqual (0, b.Top, "4");
			
			f.Dispose ();
		}

		[Test]
		public void AnchorLeftRightTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.Size = new Size (200, 200);

			Button b = new Button ();
			b.Size = new Size (100, 100);
			b.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			f.Controls.Add (b);

			Assert.AreEqual (0, b.Left, "1");
			Assert.AreEqual (100, b.Right, "2");

			f.Size = new Size (300, 300);

			Assert.AreEqual (0, b.Left, "3");
			Assert.AreEqual (200, b.Right, "4");
			
			f.Dispose ();
		}

		[Test]
		public void AnchorBottomLeftTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.Size = new Size (200, 200);

			Button b = new Button ();
			b.Size = new Size (100, 100);
			b.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

			f.Controls.Add (b);

			Assert.AreEqual (0, b.Left, "1");
			Assert.AreEqual (0, b.Top, "2");

			f.Size = new Size (300, 300);

			Assert.AreEqual (0, b.Left, "3");
			Assert.AreEqual (100, b.Top, "4");
			
			f.Dispose ();
		}

		[Test]
		public void AnchorBottomRightTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.Size = new Size (200, 200);

			Button b = new Button ();
			b.Size = new Size (100, 100);
			b.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

			f.Controls.Add (b);

			Assert.AreEqual (0, b.Left, "1");
			Assert.AreEqual (0, b.Top, "2");

			f.Size = new Size (300, 300);

			Assert.AreEqual (100, b.Left, "3");
			Assert.AreEqual (100, b.Top, "4");
			
			f.Dispose ();
		}

		[Test]
		public void AnchorTopBottomTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.Size = new Size (200, 200);

			Button b = new Button ();
			b.Size = new Size (100, 100);
			b.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

			f.Controls.Add (b);

			Assert.AreEqual (0, b.Top, "1");
			Assert.AreEqual (100, b.Bottom, "2");

			f.Size = new Size (300, 300);

			Assert.AreEqual (0, b.Top, "3");
			Assert.AreEqual (200, b.Bottom, "4");
			
			f.Dispose ();
		}

		// Unit test version of the test case in bug #80336
		[Test]
		public void AnchorSuspendLayoutTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.SuspendLayout ();

			Button b = new Button ();
			b.Size = new Size (100, 100);

			f.Controls.Add (b);

			f.Size = new Size (200, 200);

			b.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			Assert.AreEqual (0, b.Top, "1");
			Assert.AreEqual (0, b.Left, "2");

			f.Size = new Size (300, 300);

			Assert.AreEqual (0, b.Top, "3");
			Assert.AreEqual (0, b.Left, "4");

			f.ResumeLayout();

			Assert.AreEqual (100, b.Top, "5");
			Assert.AreEqual (100, b.Left, "6");
			
			f.Dispose ();
		}

		// another variant of AnchorSuspendLayoutTest1, with
		// the SuspendLayout moved after the Anchor
		// assignment.
		[Test]
		public void AnchorSuspendLayoutTest2 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Button b = new Button ();
			b.Size = new Size (100, 100);

			f.Controls.Add (b);

			f.Size = new Size (200, 200);

			b.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			Assert.AreEqual (0, b.Top, "1");
			Assert.AreEqual (0, b.Left, "2");

			f.SuspendLayout ();

			f.Size = new Size (300, 300);

			Assert.AreEqual (0, b.Top, "3");
			Assert.AreEqual (0, b.Left, "4");

			f.ResumeLayout();

			Assert.AreEqual (100, b.Top, "5");
			Assert.AreEqual (100, b.Left, "6");
			
			f.Dispose ();
		}

		// yet another variant, this time with no Suspend/Resume.
		[Test]
		public void AnchorSuspendLayoutTest3 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Button b = new Button ();
			b.Size = new Size (100, 100);

			f.Controls.Add (b);

			f.Size = new Size (200, 200);

			b.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			Assert.AreEqual (0, b.Top, "1");
			Assert.AreEqual (0, b.Left, "2");

			f.Size = new Size (300, 300);

			Assert.AreEqual (100, b.Top, "5");
			Assert.AreEqual (100, b.Left, "6");
			
			f.Dispose ();
		}

		private string event_raised = string.Empty;

		[Test]
		public void TestAnchorDockInteraction ()
		{
			Panel p = new Panel ();
			p.DockChanged += new EventHandler (DockChanged_Handler);

			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, p.Anchor, "A1");
			Assert.AreEqual (DockStyle.None, p.Dock, "A2");

			p.Dock = DockStyle.Right;
			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, p.Anchor, "A3");
			Assert.AreEqual (DockStyle.Right, p.Dock, "A4");
			Assert.AreEqual ("DockStyleChanged", event_raised, "A5");
			event_raised = string.Empty;

			p.Anchor = AnchorStyles.Bottom;
			Assert.AreEqual (AnchorStyles.Bottom, p.Anchor, "A6");
			Assert.AreEqual (DockStyle.None, p.Dock, "A7");
			Assert.AreEqual (string.Empty, event_raised, "A8");

			p.Dock = DockStyle.Fill;
			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, p.Anchor, "A9");
			Assert.AreEqual (DockStyle.Fill, p.Dock, "A10");
			Assert.AreEqual ("DockStyleChanged", event_raised, "A11");
			event_raised = string.Empty;

			p.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, p.Anchor, "A12");
			Assert.AreEqual (DockStyle.Fill, p.Dock, "A13");
			Assert.AreEqual (string.Empty, event_raised, "A14");

			p.Dock = DockStyle.None;
			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, p.Anchor, "A15");
			Assert.AreEqual (DockStyle.None, p.Dock, "A16");
			Assert.AreEqual ("DockStyleChanged", event_raised, "A17");
			event_raised = string.Empty;

			p.Anchor = AnchorStyles.Bottom;
			p.Dock = DockStyle.None;
			Assert.AreEqual (AnchorStyles.Bottom, p.Anchor, "A18");
			Assert.AreEqual (DockStyle.None, p.Dock, "A19");
			Assert.AreEqual (string.Empty, event_raised, "A20");
		}

		public void DockChanged_Handler (object sender, EventArgs e)
		{
			event_raised += "DockStyleChanged";
		}

		[Test]	// bug #80917
		public void BehaviorOverriddenDisplayRectangle ()
		{
			Control c = new Control ();
			c.Anchor |= AnchorStyles.Bottom;
			c.Size = new Size (100, 100);

			Form f = new DisplayRectangleForm ();
			f.Controls.Add (c);
			f.ShowInTaskbar = false;
			f.Show ();
			
			Assert.AreEqual (new Size (100, 100), c.Size, "A1");
			
			f.Dispose ();
		}

		private class DisplayRectangleForm : Form
		{
			public override Rectangle DisplayRectangle
			{
				get { return Rectangle.Empty; }
			}
		}
		
		[Test]  // bug 80912
		public void AnchoredControlWithZeroWidthAndHeight ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			Control c = new Control ();
			c.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			f.Controls.Add (c);

			Assert.AreEqual (new Rectangle (0, 0, 0, 0), c.Bounds, "N1");
		}

		[Test] // bug 81694
		public void TestNestedControls ()
		{
			MainForm f = new MainForm ();
			f.ShowInTaskbar = false;
			
			f.Show ();
			Assert.AreEqual (new Rectangle (210, 212, 75, 23), f._userControl._button2.Bounds, "K1");
			
			f.Dispose ();
		}

		[Test] // bug 81695
		public void TestNestedControls2 ()
		{
			MainForm f = new MainForm ();
			f.ShowInTaskbar = false;

			f.Show ();
			
			Size s = f.Size;
			f.Size = new Size (10, 10);
			f.Size = s;
			
			Assert.AreEqual (new Rectangle (210, 212, 75, 23), f._userControl._button2.Bounds, "K1");

			f.Dispose ();
		}

		private class MainForm : Form
		{
			public UserControl1 _userControl;
			
			public MainForm ()
			{
				SuspendLayout ();
				// 
				// _userControl
				// 
				_userControl = new UserControl1 ();
				_userControl.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right);
				_userControl.BackColor = Color.White;
				_userControl.Location = new Point (8, 8);
				_userControl.Size = new Size (288, 238);
				_userControl.TabIndex = 0;
				Controls.Add (_userControl);
				// 
				// MainForm
				// 
				ClientSize = new Size (304, 280);
				Location = new Point (250, 100);
				StartPosition = FormStartPosition.Manual;
				Text = "bug #81694";
				ResumeLayout (false);
			}
		}
		
		private class UserControl1 : UserControl
		{
			private Button _button1;
			public Button _button2;

			public UserControl1 ()
			{
				SuspendLayout ();
				// 
				// _button1
				// 
				_button1 = new Button ();
				_button1.Location = new Point (4, 4);
				_button1.Size = new Size (75, 23);
				_button1.TabIndex = 0;
				_button1.Text = "Button1";
				Controls.Add (_button1);
				// 
				// _button2
				// 
				_button2 = new Button ();
				_button2.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
				_button2.Location = new Point (210, 212);
				_button2.Size = new Size (75, 23);
				_button2.TabIndex = 1;
				_button2.Text = "Button2";
				Controls.Add (_button2);
				// 
				// UserControl1
				// 
				BackColor = Color.White;
				ClientSize = new Size (288, 238);
				ResumeLayout (false);
			}
		}

		[Test]
		public void TestDockFillWithPadding ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Padding = new Padding (15, 15, 15, 15);

			Control c = new Control ();
			c.Dock = DockStyle.Fill;
			f.Controls.Add (c);

			f.Show ();
			Assert.AreEqual (new Size (f.ClientSize.Width - 30, f.ClientSize.Height - 30), c.Size, "K1");

			f.Dispose ();
		}

		[Test]
		public void Bug82762 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.ClientSize = new Size (284, 264);
			
			Button b = new Button ();
			b.Size = new Size (100, 100);
			b.Anchor = AnchorStyles.None;
			f.Controls.Add (b);
			
			f.Show ();
			
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), b.Bounds, "A1");
			
			f.ClientSize = new Size (600, 600);

			Assert.AreEqual (new Rectangle (158, 168, 100, 100), b.Bounds, "A2");

			f.Close ();
			f.Dispose ();
		}
		
		[Test]
		public void Bug82805 ()
		{
			Control c1 = new Control ();
			c1.Size = new Size (100, 100);
			Control c2 = new Control ();
			c2.Size = new Size (100, 100);

			c2.SuspendLayout ();
			c1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			c2.Controls.Add (c1);
			c2.Size = new Size (200, 200);
			c2.ResumeLayout ();

			Assert.AreEqual (200, c1.Width, "A1");
		}
		
		[Test]
		public void DockedAutoSizeControls ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			Button b = new Button ();
			b.Text = "Yo";
			b.AutoSize = true;
			b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			b.Width = 200;
			b.Dock = DockStyle.Left;
			f.Controls.Add (b);
			
			f.Show ();
			
			if (b.Width >= 200)
				Assert.Fail ("button should be less than 200 width: actual {0}", b.Width);
			
			f.Close ();
			f.Dispose ();
		}
		
		[Test]  // bug #81199
		public void NestedControls ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			MyUserControl c = new MyUserControl ();
			c.Dock = DockStyle.Fill;
			c.Size = new Size (500, 500);
			
			f.SuspendLayout ();
			f.Controls.Add (c);
			f.ClientSize = new Size (500, 500);
			f.ResumeLayout (false);
			
			f.Show ();
			
			Assert.AreEqual (new Size (600, 600), c.lv.Size, "I1");
			f.Close ();
		}
		
		private class MyUserControl : UserControl
		{
			public ListView lv;
			
			public MyUserControl ()
			{
				lv = new ListView ();
				SuspendLayout ();
				lv.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
				lv.Size = new Size (300, 300);
				
				Controls.Add (lv);
				Size = new Size (200, 200);
				ResumeLayout (false);
			}
		}
	}

	[TestFixture]	
	public class DockingTests : TestHelper
	{
		Form form;
		Panel panel;

		int event_count;

		[SetUp]
		protected override void SetUp () {
			form = new Form ();
			form.ShowInTaskbar = false;
			form.Size = new Size (400, 400);
			panel = new Panel ();
			form.Controls.Add (panel);
			event_count = 0;
			base.SetUp ();
		}

		[TearDown]
		protected override void TearDown () {
			form.Dispose ();
			base.TearDown ();
		}

		void IncrementEventCount (object o, EventArgs args)
		{
			event_count++;
		}

		[Test]
		public void TestDockSizeChangedEvent ()
		{
			panel.SizeChanged += new EventHandler (IncrementEventCount);
			panel.Dock = DockStyle.Bottom;
			Assert.AreEqual (1, event_count);
		}

		[Test]
		public void TestDockLocationChangedEvent ()
		{
			panel.LocationChanged += new EventHandler (IncrementEventCount);
			panel.Dock = DockStyle.Bottom;
			Assert.AreEqual (1, event_count);
		}

		[Test]
		public void TestDockFillFirst ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Panel b1 = new Panel ();
			Panel b2 = new Panel ();

			b1.Dock = DockStyle.Fill;
			b2.Dock = DockStyle.Left;

			f.Controls.Add (b1);
			f.Controls.Add (b2);

			f.Show ();
			Assert.AreEqual (new Rectangle (b2.Width, 0, f.ClientRectangle.Width - b2.Width, f.ClientRectangle.Height), b1.Bounds, "A1");
			Assert.AreEqual (new Rectangle (0, 0, 200, f.ClientRectangle.Height), b2.Bounds, "A2");
			f.Dispose ();
		}

		[Test]
		public void TestDockFillLast ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Panel b1 = new Panel ();
			Panel b2 = new Panel ();

			b1.Dock = DockStyle.Fill;
			b2.Dock = DockStyle.Left;

			f.Controls.Add (b2);
			f.Controls.Add (b1);

			f.Show ();
			Assert.AreEqual (new Rectangle (0, 0, f.ClientRectangle.Width, f.ClientRectangle.Height), b1.Bounds, "B1");
			Assert.AreEqual (new Rectangle (0, 0, 200, f.ClientRectangle.Height), b2.Bounds, "B2");
			f.Dispose ();
		}
		
		[Test]  // bug #81397
		public void TestDockingWithCustomDisplayRectangle ()
		{
			MyControl mc = new MyControl ();
			mc.Size = new Size (200, 200);
			
			Control c = new Control ();
			c.Dock = DockStyle.Fill;
			
			mc.Controls.Add (c);
			
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			f.Controls.Add (mc);
			f.Show ();
			
			Assert.AreEqual (new Point (20, 20), c.Location, "A1");
			Assert.AreEqual (new Size (160, 160), c.Size, "A2");
			
			f.Dispose ();
		}
		
		private class MyControl : Control
		{
			public override Rectangle DisplayRectangle { 
				get { return new Rectangle (20, 20, this.Width - 40, this.Height - 40); }
			}
		}

		[Test]
		public void DockingPreferredSize ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.ClientSize = new Size (300, 300);

			C1 c1 = new C1 ();
			c1.Size = new Size (100, 100);
			c1.Dock = DockStyle.Left;

			f.Controls.Add (c1);
			f.Show ();

			Assert.AreEqual (new Size (100, 300), c1.Size, "A1");

			f.Controls.Clear ();
			C2 c2 = new C2 ();
			c2.Size = new Size (100, 100);
			c2.Dock = DockStyle.Left;

			f.Controls.Add (c2);
			Assert.AreEqual (new Size (100, 300), c1.Size, "A2");

			f.Dispose ();
		}

		private class C1 : Panel
		{
			public override Size GetPreferredSize (Size proposedSize)
			{
				Console.WriteLine ("HOYO!");
				return new Size (200, 200);
			}
		}

		private class C2 : Panel
		{
			public override Size GetPreferredSize (Size proposedSize)
			{
				return Size.Empty;
			}
		}

		[Test]
		public void ResettingDockToNone ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.ClientSize = new Size (300, 300);
			
			Control c = new Control ();
			c.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			
			f.Controls.Add (c);
			
			f.Show ();
			
			f.ClientSize = new Size (350, 350);

			Assert.IsTrue (c.Left > 0, string.Format ("A1: c.Left ({0}) must be greater than 0", c.Left));
			Assert.IsTrue (c.Top > 0, string.Format ("A2: c.Top ({0}) must be greater than 0", c.Top));
			
			c.Dock = DockStyle.None;
			Assert.IsTrue (c.Left > 0, string.Format ("A3: c.Left ({0}) must be greater than 0", c.Left));
			Assert.IsTrue (c.Top > 0, string.Format ("A4: c.Top ({0}) must be greater than 0", c.Top));
			
			f.ClientSize = new Size (400, 400);
			Assert.IsTrue (c.Left > 70, string.Format ("A5: c.Left ({0}) must be greater than 70", c.Left));
			Assert.IsTrue (c.Top > 70, string.Format ("A6: c.Top ({0}) must be greater than 70", c.Top));
			
			f.Dispose ();
		}
	}

	[TestFixture]	
	public class UndockingTests : TestHelper
	{
		class TestPanel : Panel {

			public void InvokeSetBoundsCore ()
			{
				SetBoundsCore (37, 37, 37, 37, BoundsSpecified.All);
			}

			public void InvokeUpdateBounds ()
			{
				UpdateBounds (37, 37, 37, 37);
			}
		}

		Form form;
		TestPanel panel;

		[SetUp]
		protected override void SetUp () {
			form = new Form ();
			form.ShowInTaskbar = false;
			form.Size = new Size (400, 400);
			panel = new TestPanel ();
			form.Controls.Add (panel);
			base.SetUp ();
		}

		[TearDown]
		protected override void TearDown ()
		{
			form.Dispose ();
			base.TearDown ();
		}

		[Test]
		public void TestUndockDefaultLocation ()
		{
			Point loc = panel.Location;
			panel.Dock = DockStyle.Bottom;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (loc, panel.Location);
		}

		[Test]
		public void TestUndockDefaultLocationVisible ()
		{
			form.Show ();
			Point loc = panel.Location;
			panel.Dock = DockStyle.Bottom;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (loc, panel.Location);
		}

		[Test]
		public void TestUndockExplicitLeft ()
		{
			panel.Left = 150;
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (150, panel.Left);
		}

		[Test]
		public void TestUndockExplicitTop ()
		{
			panel.Top = 150;
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (150, panel.Top);
		}

		[Test]
		public void TestUndockExplicitLocation ()
		{
			panel.Location = new Point (50, 50);
			Point loc = panel.Location;
			panel.Dock = DockStyle.Bottom;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (loc, panel.Location);
		}

		[Test]
		public void TestUndockExplicitLeftVisible ()
		{
			form.Show ();
			panel.Left = 150;
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (150, panel.Left);
		}

		[Test]
		public void TestUndockExplicitTopVisible ()
		{
			form.Show ();
			panel.Top = 150;
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (150, panel.Top);
		}

		[Test]
		public void TestUndockExplicitLocationVisible ()
		{
			form.Show ();
			panel.Location = new Point (50, 50);
			Point loc = panel.Location;
			panel.Dock = DockStyle.Bottom;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (loc, panel.Location);
		}

		[Test]
		public void TestUndockDefaultSize ()
		{
			Size sz = panel.Size;
			panel.Dock = DockStyle.Fill;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (sz, panel.Size);
		}

		[Test]
		public void TestUndockExplicitHeight ()
		{
			panel.Height = 50;
			panel.Dock = DockStyle.Left;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (50, panel.Height);
		}

		[Test]
		public void TestUndockExplicitSize ()
		{
			panel.Size = new Size (50, 50);
			Size sz = panel.Size;
			panel.Dock = DockStyle.Fill;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (sz, panel.Size);
		}

		[Test]
		public void TestUndockExplicitWidth ()
		{
			panel.Width = 50;
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (50, panel.Width);
		}

		[Test]
		public void TestUndockExplicitHeightVisible ()
		{
			form.Show ();
			panel.Height = 50;
			panel.Dock = DockStyle.Left;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (50, panel.Height);
		}

		[Test]
		public void TestUndockExplicitSizeVisible ()
		{
			form.Show ();
			panel.Size = new Size (50, 50);
			Size sz = panel.Size;
			panel.Dock = DockStyle.Fill;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (sz, panel.Size);
		}

		[Test]
		public void TestUndockExplicitWidthVisible ()
		{
			form.Show ();
			panel.Width = 50;
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (50, panel.Width);
		}

		[Test]
		public void TestUndockSetBounds ()
		{
			panel.SetBounds (50, 50, 50, 50, BoundsSpecified.All);
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (50, panel.Height, "Height");
			Assert.AreEqual (50, panel.Left, "Left");
			Assert.AreEqual (50, panel.Top, "Top");
			Assert.AreEqual (50, panel.Width, "Width");
		}

		[Test]
		public void TestUndockSetBoundsVisible ()
		{
			form.Show ();
			panel.SetBounds (50, 50, 50, 50, BoundsSpecified.All);
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (50, panel.Height, "Height");
			Assert.AreEqual (50, panel.Left, "Left");
			Assert.AreEqual (50, panel.Top, "Top");
			Assert.AreEqual (50, panel.Width, "Width");
		}

		[Test]
		public void TestUndockSetBoundsCore ()
		{
			panel.InvokeSetBoundsCore ();
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (37, panel.Height, "Height");
			Assert.AreEqual (37, panel.Left, "Left");
			Assert.AreEqual (37, panel.Top, "Top");
			Assert.AreEqual (37, panel.Width, "Width");
		}

		[Test]
		public void TestUndockSetBoundsCoreVisible ()
		{
			form.Show ();
			panel.InvokeSetBoundsCore ();
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (37, panel.Height, "Height");
			Assert.AreEqual (37, panel.Left, "Left");
			Assert.AreEqual (37, panel.Top, "Top");
			Assert.AreEqual (37, panel.Width, "Width");
		}

		[Test]
		public void TestUndockUpdateBounds ()
		{
			panel.InvokeUpdateBounds ();
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (37, panel.Height, "Height");
			Assert.AreEqual (37, panel.Left, "Left");
			Assert.AreEqual (37, panel.Top, "Top");
			Assert.AreEqual (37, panel.Width, "Width");
		}

		[Test]
		public void TestUndockUpdateBoundsVisible ()
		{
			form.Show ();
			panel.InvokeUpdateBounds ();
			panel.Dock = DockStyle.Top;
			panel.Dock = DockStyle.None;
			Assert.AreEqual (37, panel.Height, "Height");
			Assert.AreEqual (37, panel.Left, "Left");
			Assert.AreEqual (37, panel.Top, "Top");
			Assert.AreEqual (37, panel.Width, "Width");
		}

		[Test]
		public void AutoSizeGrowOnlyControls_ShrinkWhenDocked ()
		{
			// For most controls that are AutoSized and support the
			// AutoSizeMode property, if they are set to GrowOnly,
			// they should not shrink.  However, they will shrink 
			// even in GrowOnly mode if they are docked.
			// Button is one exception to this rule.
			Form f = new Form ();
			f.ClientSize = new Size (300, 300);
			f.ShowInTaskbar = false;
			f.Show ();

			List<Type> types = new List<Type> ();
			types.Add (typeof (TableLayoutPanel));
			types.Add (typeof (FlowLayoutPanel));
			types.Add (typeof (ToolStripContentPanel));
			types.Add (typeof (Panel));
			types.Add (typeof (GroupBox));
			types.Add (typeof (UserControl));
			foreach (Type t in types) {
				Control c = t.GetConstructor (Type.EmptyTypes).Invoke (null) as Control;
				c.AutoSize = true;
				t.GetProperty ("AutoSizeMode").SetValue (c, AutoSizeMode.GrowOnly, null);
				c.Bounds = new Rectangle (5, 5, 100, 100);
				f.Controls.Add (c);
				Assert.AreEqual (100, c.Height, "1 " + t.Name);
				c.Dock = DockStyle.Top;
				Assert.IsFalse (100 == c.Height, "2 " + t.Name);
				f.Controls.Remove (c);
				c.Dispose ();
			}

			f.Dispose ();
		}

		[Test]
		public void AutoSizeGrowOnlyButtons_DoNotShrinkWhenDocked ()
		{
			// For most controls that are AutoSized and support the
			// AutoSizeMode property, if they are set to GrowOnly,
			// they should not shrink.  However, they will shrink 
			// even in GrowOnly mode if they are docked.
			// Button is one exception to this rule.
			Form f = new Form ();
			f.ClientSize = new Size (300, 300);
			f.ShowInTaskbar = false;
			f.Show ();

			List<Type> types = new List<Type> ();
			types.Add (typeof (Button));
			foreach (Type t in types) {
				Control c = t.GetConstructor (Type.EmptyTypes).Invoke (null) as Control;
				c.AutoSize = true;
				t.GetProperty ("AutoSizeMode").SetValue (c, AutoSizeMode.GrowOnly, null);
				c.Bounds = new Rectangle (5, 5, 100, 100);
				f.Controls.Add (c);
				Assert.AreEqual (100, c.Height, "1 " + t.Name);
				c.Dock = DockStyle.Top;
				Assert.AreEqual (100, c.Height, "2 " + t.Name);
				f.Controls.Remove (c);
				c.Dispose ();
			}

			f.Dispose ();
		}

		[Test]
		public void AnchoredAutoSizedControls_SizeInCorrectDirection ()
		{
			Form f = new Form ();
			f.ClientSize = new Size (300, 300);
			f.ShowInTaskbar = false;

			Panel p1 = new Panel ();
			p1.Bounds = new Rectangle (150, 150, 0, 0);
			p1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			p1.AutoSize = true;
			f.Controls.Add (p1);

			Panel p2 = new Panel ();
			p2.Bounds = new Rectangle (150, 150, 0, 0);
			p2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			p2.AutoSize = true;
			f.Controls.Add (p2);

			Panel p3 = new Panel ();
			p3.Bounds = new Rectangle (150, 150, 0, 0);
			p3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
			p3.AutoSize = true;
			f.Controls.Add (p3);

			Panel p4 = new Panel ();
			p4.Bounds = new Rectangle (150, 150, 0, 0);
			p4.Anchor = AnchorStyles.None;
			p4.AutoSize = true;
			f.Controls.Add (p4);

			f.Show ();
			// cause the panels to grow
			p1.Controls.Add (new TextBox ());
			p2.Controls.Add (new TextBox ());
			p3.Controls.Add (new TextBox ());
			p4.Controls.Add (new TextBox ());
			f.PerformLayout ();

			Assert.AreEqual (150, p1.Top, "1");
			Assert.AreEqual (150, p1.Left, "2");
			Assert.AreEqual (150, p2.Bottom, "3");
			Assert.AreEqual (150, p2.Right, "4");
			Assert.AreEqual (150, p3.Top, "5");
			Assert.AreEqual (150, p3.Left, "6");
			Assert.AreEqual (150, p4.Top, "7");
			Assert.AreEqual (150, p4.Left, "8");

			f.Dispose ();
		}
	}
}
