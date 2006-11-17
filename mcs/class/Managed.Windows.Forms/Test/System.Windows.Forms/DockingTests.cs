using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture()]	
	public class DockingTests
	{
		Form form;
		Panel panel;

		int event_count;

		[SetUp]
		public void Init ()
		{
			form = new Form ();
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
		[Category ("NotWorking")]
		public void TestDockSizeChangedEvent ()
		{
			panel.SizeChanged += new EventHandler (IncrementEventCount);
			panel.Dock = DockStyle.Bottom;
			Assert.AreEqual (1, event_count);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestDockLocationChangedEvent ()
		{
			panel.LocationChanged += new EventHandler (IncrementEventCount);
			panel.Dock = DockStyle.Bottom;
			Assert.AreEqual (1, event_count);
		}

	}

	[TestFixture()]	
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

	}
}

