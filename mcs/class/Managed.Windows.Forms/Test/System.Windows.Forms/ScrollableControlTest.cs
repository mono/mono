//
// ScrollableControlTest.cs: Test cases for ScrollableControl.
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Gert Driesen
//

using System;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ScrollableControlTest
	{
		[Test]
		public void ResizeAnchoredTest ()
		{
			ScrollableControl sc = new ScrollableControl ();
			object h = sc.Handle;
			sc.Size = new Size (23, 45);
			Label lbl = new Label ();
			lbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lbl.Size = sc.ClientSize;
			sc.Controls.Add (lbl);
			sc.Height *= 2;
			sc.Height *= 2;
			Assert.AreEqual (lbl.Location, Point.Empty, "#1");
			Assert.AreEqual (lbl.Size, sc.ClientSize, "#2");
			
			TestHelper.RemoveWarning (h);
		}
		[Test]
		public void AutoScroll ()
		{
			ScrollableControl sc = new ScrollableControl ();
			Assert.IsFalse (sc.AutoScroll, "#A1");
			Assert.AreEqual (0, sc.Controls.Count, "#A2");

			sc.AutoScroll = true;
			Assert.IsTrue(sc.AutoScroll, "#B1");
			Assert.AreEqual (0, sc.Controls.Count, "#B2");

			sc.AutoScroll = false;
			Assert.IsFalse (sc.AutoScroll, "#C1");
			Assert.AreEqual (0, sc.Controls.Count, "#C2");
		}

		[Test]
		public void AutoScrollMinSize ()
		{
			ScrollableControl sc = new ScrollableControl ();
			Assert.AreEqual (Size.Empty, sc.AutoScrollMinSize, "#A1");
			Assert.IsFalse (sc.AutoScroll, "#A2");

			sc.AutoScrollMinSize = Size.Empty;
			Assert.AreEqual (Size.Empty, sc.AutoScrollMinSize, "#B1");
			Assert.IsFalse (sc.AutoScroll, "#B2");

			sc.AutoScrollMinSize = new Size (10, 20);
			Assert.AreEqual (new Size (10, 20), sc.AutoScrollMinSize, "#C1");
			Assert.IsTrue (sc.AutoScroll, "#C2");

			sc.AutoScroll = false;
			Assert.AreEqual (new Size (10, 20), sc.AutoScrollMinSize, "#D1");
			Assert.IsFalse (sc.AutoScroll, "#D2");

			sc.AutoScrollMinSize = new Size (10, 20);
			Assert.AreEqual (new Size (10, 20), sc.AutoScrollMinSize, "#E1");
			Assert.IsFalse (sc.AutoScroll, "#E2");

			sc.AutoScrollMinSize = new Size (20, 20);
			Assert.AreEqual (new Size (20, 20), sc.AutoScrollMinSize, "#F1");
			Assert.IsTrue (sc.AutoScroll, "#F2");

			sc.AutoScroll = false;
			Assert.AreEqual (new Size (20, 20), sc.AutoScrollMinSize, "#G1");
			Assert.IsFalse (sc.AutoScroll, "#G2");

			sc.AutoScrollMinSize = Size.Empty;
			Assert.AreEqual (Size.Empty, sc.AutoScrollMinSize, "#H1");
			Assert.IsTrue (sc.AutoScroll, "#H2");

			sc.AutoScrollMinSize = new Size (10, 20);
			Assert.AreEqual (new Size (10, 20), sc.AutoScrollMinSize, "#I1");
			Assert.IsTrue (sc.AutoScroll, "#I2");

			sc.AutoScrollMinSize = Size.Empty;
			Assert.AreEqual (Size.Empty, sc.AutoScrollMinSize, "#J1");
			Assert.IsTrue (sc.AutoScroll, "#J2");
		}

#if NET_2_0
		[Test]
		public void MethodScrollToControl ()
		{
			if (TestHelper.RunningOnUnix)
				Assert.Ignore ("Depends of scrollbar width, values are for Windows");

			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Show ();
			
			PublicScrollableControl sc = new PublicScrollableControl ();
			sc.Size = new Size (200, 200);
			sc.AutoScroll = true;
			
			f.Controls.Add (sc);
			
			Button b = new Button ();
			b.Top = 15;
			sc.Controls.Add (b);
			
			Button b2 = new Button ();
			b2.Top = 340;
			sc.Controls.Add (b2);
			
			Button b3 = new Button ();
			b3.Left = 280;
			sc.Controls.Add (b3);

			Assert.AreEqual (new Point (0, 0), sc.PublicScrollToControl (b), "A1");
			Assert.AreEqual (new Point (0, -180), sc.PublicScrollToControl (b2), "A2");
			Assert.AreEqual (new Point (-172, 0), sc.PublicScrollToControl (b3), "A3");

			sc.AutoScrollPosition = new Point (50, 70);

			Assert.AreEqual (new Point (0, -15), sc.PublicScrollToControl (b), "A4");
			Assert.AreEqual (new Point (0, -180), sc.PublicScrollToControl (b2), "A5");
			Assert.AreEqual (new Point (-172, 0), sc.PublicScrollToControl (b3), "A6");

			sc.AutoScrollPosition = new Point (150, 150);

			Assert.AreEqual (new Point (0, -15), sc.PublicScrollToControl (b), "A7");
			Assert.AreEqual (new Point (0, -180), sc.PublicScrollToControl (b2), "A8");
			Assert.AreEqual (new Point (-172, 0), sc.PublicScrollToControl (b3), "A9");
			
			f.Dispose ();
		}
		
		private class PublicScrollableControl : ScrollableControl
		{
			public Point PublicScrollToControl (Control activeControl)
			{
				return base.ScrollToControl (activeControl);
			}
		}
#endif
	}
}
