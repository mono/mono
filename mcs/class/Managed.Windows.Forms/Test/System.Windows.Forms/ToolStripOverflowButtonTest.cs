//
// ToolStripOverflowButtonTest.cs
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
	public class ToolStripOverflowButtonTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
		}

		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (new Padding (0, 1, 0, 2), epp.DefaultMargin, "C1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Size2 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Show ();

			ToolStrip ts = new ToolStrip ();
			f.Controls.Add (ts);
			ToolStripOverflowButton tsi = ts.OverflowButton;

			Assert.AreEqual (new Size (16, 25), tsi.Size, "B1");
			Assert.AreEqual (false, tsi.Visible, "B3");
			ToolStripItem test = ts.Items.Add ("test");
			test.Overflow = ToolStripItemOverflow.Always;
			ts.PerformLayout ();

			Assert.AreEqual (Size.Empty, tsi.Size, "B2");
			f.Hide ();
		}

		[Test]
		[Category ("NotWorking")]
		public void MethodGetPreferredSize ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Show ();

			ToolStrip ts = new ToolStrip ();
			f.Controls.Add (ts);
			ToolStripOverflowButton tsi = ts.OverflowButton;

			Assert.AreEqual (Size.Empty, tsi.GetPreferredSize (Size.Empty), "B1");
			Assert.AreEqual (false, tsi.Visible, "B2");
			
			ToolStripItem test = ts.Items.Add ("test");
			test.Overflow = ToolStripItemOverflow.Always;
			ts.PerformLayout ();

			Assert.AreEqual (new Size (16, 25), tsi.GetPreferredSize (new Size (100, 100)), "B3");
			Assert.AreEqual (false, tsi.Visible, "B4");
			f.Hide ();
		}
		
		[Test]
		[Category ("NotWorking")]
		public void BehaviorItemsOnOverflow ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			MyToolStrip ts = new MyToolStrip ();
			f.Controls.Add (ts);
			f.Show ();
			
			Assert.AreEqual (0, ts.Items.Count, "A1");
			Assert.AreEqual (1, ts.PublicDisplayedItems.Count, "A2");
			Assert.AreEqual (false, ts.OverflowButton.Visible, "A3");
			Assert.AreEqual (0, ts.OverflowButton.DropDown.Items.Count, "A3");

			ToolStripItem tsi = ts.Items.Add ("test");

			Assert.AreEqual (1, ts.Items.Count, "A4");
			Assert.AreEqual (2, ts.PublicDisplayedItems.Count, "A5");
			Assert.AreEqual (false, ts.OverflowButton.Visible, "A3");
			Assert.AreEqual (0, ts.OverflowButton.DropDown.Items.Count, "A6");

			tsi.Overflow = ToolStripItemOverflow.Always;

			Assert.AreEqual (1, ts.Items.Count, "A7");
			Assert.AreEqual (2, ts.PublicDisplayedItems.Count, "A8");
			Assert.AreEqual (true, ts.OverflowButton.Visible, "A3");
			Assert.AreEqual (0, ts.OverflowButton.DropDown.Items.Count, "A9");
			Console.WriteLine (ts.PublicDisplayedItems[1].GetType().ToString());
			f.Dispose ();
		}
		
		private class ExposeProtectedProperties : ToolStripButton
		{
			public new Padding DefaultMargin { get { return base.DefaultMargin; } }
		}
		
		private class MyToolStrip : ToolStrip
		{
			public ToolStripItemCollection PublicDisplayedItems {
				get { return base.DisplayedItems; }
			}
		}
	}
}
#endif