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
		public void AutoSize ()
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
	}
}
