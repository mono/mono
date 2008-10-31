//
// PanelTest.cs: Test cases for PanelTest.
//
// Author:
//   Jonathan Pobst (monkey@jpobst.com)
//
// (C) 2007 Novell, Inc.
//

using System;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PanelTest : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			Panel p = new Panel ();

#if NET_2_0
			Assert.AreEqual (false, p.AutoSize, "A1");
			Assert.AreEqual (AutoSizeMode.GrowOnly, p.AutoSizeMode, "A2");
#endif
			Assert.AreEqual (BorderStyle.None, p.BorderStyle, "A3");
			Assert.AreEqual (false, p.TabStop, "A4");
			Assert.AreEqual (string.Empty, p.Text, "A5");
		}

#if NET_2_0
		[Test]
		public void AutoSize ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Panel p = new Panel ();
			p.AutoSize = true;
			f.Controls.Add (p);
			
			Button b = new Button ();
			b.Size = new Size (200, 200);
			b.Location = new Point (200, 200);
			p.Controls.Add (b);

			f.Show ();

			Assert.AreEqual (new Size (403, 403), p.ClientSize, "A1");
			
			p.Controls.Remove (b);
			Assert.AreEqual (new Size (200, 100), p.ClientSize, "A2");
			
			p.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			Assert.AreEqual (new Size (0, 0), p.ClientSize, "A3");
			f.Dispose ();
		}
#endif
	}
}
