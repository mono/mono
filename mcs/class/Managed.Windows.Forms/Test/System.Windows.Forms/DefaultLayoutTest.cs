using System;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DefaultLayoutTest
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
			Assert.AreEqual (null, most_recent_args.AffectedProperty, "5");

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
			Assert.AreEqual (null, most_recent_args.AffectedProperty, "10");

			/* with the anchor style set to something, resize the child */
			event_count = 0;
			b.Size = new Size (100, 100);
			Assert.AreEqual (1, event_count, "11");
			Assert.AreEqual ("Bounds", most_recent_args.AffectedProperty, "12");

			/* and again with layout suspended */
			event_count = 0;
			p.SuspendLayout ();
			b.Size = new Size (200, 200);
			Assert.AreEqual (0, event_count, "13");
			p.ResumeLayout ();
			Assert.AreEqual (1, event_count, "14");
			Assert.AreEqual (null, most_recent_args.AffectedProperty, "15");
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
	}

	[TestFixture]	
	public class DockingTests
	{
		Form form;
		Panel panel;

		int event_count;

		[SetUp]
		public void Init ()
		{
			form = new Form ();
			form.ShowInTaskbar = false;
			form.Size = new Size (400, 400);
			panel = new Panel ();
			form.Controls.Add (panel);
			event_count = 0;
		}

		[TearDown]
		public void Cleanup ()
		{
			form.Dispose ();
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

	}

	[TestFixture]	
	public class UndockingTests
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
		public void Init ()
		{
			form = new Form ();
			form.ShowInTaskbar = false;
			form.Size = new Size (400, 400);
			panel = new TestPanel ();
			form.Controls.Add (panel);
		}

		[TearDown]
		public void Cleanup ()
		{
			form.Dispose ();
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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

	}
}
