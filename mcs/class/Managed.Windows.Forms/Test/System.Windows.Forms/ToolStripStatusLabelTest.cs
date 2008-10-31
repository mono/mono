//
// ToolStripStatusLabelTests.cs
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
	public class ToolStripStatusLabelTests : TestHelper
	{
		//[Test]
		//public void Constructor ()
		//{
		//        ToolStripLabel tsi = new ToolStripLabel ();

		//        Assert.AreEqual (Color.Red, tsi.ActiveLinkColor, "A1");
		//        Assert.AreEqual (false, tsi.CanSelect, "A2");
		//        Assert.AreEqual (false, tsi.IsLink, "A3");
		//        Assert.AreEqual (LinkBehavior.SystemDefault, tsi.LinkBehavior, "A4");
		//        Assert.AreEqual (Color.FromArgb (0,0,255), tsi.LinkColor, "A5");
		//        Assert.AreEqual (false, tsi.LinkVisited, "A6");
		//        Assert.AreEqual (Color.FromArgb (128, 0, 128), tsi.VisitedLinkColor, "A7");

		//        int count = 0;
		//        EventHandler oc = new EventHandler (delegate (object sender, EventArgs e) { count++; });
		//        Image i = new Bitmap (1,1);

		//        tsi = new ToolStripLabel (i);
		//        tsi.PerformClick();
		//        Assert.AreEqual (null, tsi.Text, "A8");
		//        Assert.AreSame (i, tsi.Image, "A9");
		//        Assert.AreEqual (false, tsi.IsLink, "A10");
		//        Assert.AreEqual (0, count, "A11");
		//        Assert.AreEqual (string.Empty, tsi.Name, "A12");

		//        tsi = new ToolStripLabel ("Text");
		//        tsi.PerformClick ();
		//        Assert.AreEqual ("Text", tsi.Text, "A13");
		//        Assert.AreSame (null, tsi.Image, "A14");
		//        Assert.AreEqual (false, tsi.IsLink, "A15");
		//        Assert.AreEqual (0, count, "A16");
		//        Assert.AreEqual (string.Empty, tsi.Name, "A17");

		//        tsi = new ToolStripLabel ("Text", i);
		//        tsi.PerformClick ();
		//        Assert.AreEqual ("Text", tsi.Text, "A18");
		//        Assert.AreSame (i, tsi.Image, "A19");
		//        Assert.AreEqual (false, tsi.IsLink, "A20");
		//        Assert.AreEqual (0, count, "A21");
		//        Assert.AreEqual (string.Empty, tsi.Name, "A22");

		//        tsi = new ToolStripLabel ("Text", i, true);
		//        tsi.PerformClick ();
		//        Assert.AreEqual ("Text", tsi.Text, "A23");
		//        Assert.AreSame (i, tsi.Image, "A24");
		//        Assert.AreEqual (true, tsi.IsLink, "A25");
		//        Assert.AreEqual (0, count, "A26");
		//        Assert.AreEqual (string.Empty, tsi.Name, "A27");

		//        tsi = new ToolStripLabel ("Text", i, true, oc);
		//        tsi.PerformClick ();
		//        Assert.AreEqual ("Text", tsi.Text, "A28");
		//        Assert.AreSame (i, tsi.Image, "A29");
		//        Assert.AreEqual (true, tsi.IsLink, "A30");
		//        Assert.AreEqual (1, count, "A31");
		//        Assert.AreEqual (string.Empty, tsi.Name, "A32");

		//        tsi = new ToolStripLabel ("Text", i, true, oc, "Name");
		//        tsi.PerformClick ();
		//        Assert.AreEqual ("Text", tsi.Text, "A33");
		//        Assert.AreSame (i, tsi.Image, "A34");
		//        Assert.AreEqual (true, tsi.IsLink, "A35");
		//        Assert.AreEqual (2, count, "A36");
		//        Assert.AreEqual ("Name", tsi.Name, "A37");
		//}

		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (new Padding (0, 3, 0, 2), epp.DefaultMargin, "C3");
		}

		//[Test]
		//public void PropertyActiveLinkColor ()
		//{
		//        ToolStripLabel tsi = new ToolStripLabel ();

		//        tsi.ActiveLinkColor = Color.Green;
		//        Assert.AreEqual (Color.Green, tsi.ActiveLinkColor, "B1");
		//}

		//[Test]
		//public void PropertyIsLink ()
		//{
		//        ToolStripLabel tsi = new ToolStripLabel ();

		//        tsi.IsLink = true;
		//        Assert.AreEqual (true, tsi.IsLink, "B1");
		//}

		//[Test]
		//public void PropertyLinkBehavior ()
		//{
		//        ToolStripLabel tsi = new ToolStripLabel ();

		//        tsi.LinkBehavior = LinkBehavior.NeverUnderline;
		//        Assert.AreEqual (LinkBehavior.NeverUnderline, tsi.LinkBehavior, "B1");
		//}

		//[Test]
		//public void PropertyLinkColor ()
		//{
		//        ToolStripLabel tsi = new ToolStripLabel ();

		//        tsi.LinkColor = Color.Green;
		//        Assert.AreEqual (Color.Green, tsi.LinkColor, "B1");
		//}

		//[Test]
		//public void PropertyLinkVisited ()
		//{
		//        ToolStripLabel tsi = new ToolStripLabel ();

		//        tsi.LinkVisited = true;
		//        Assert.AreEqual (true, tsi.LinkVisited, "B1");
		//}

		//[Test]
		//public void PropertyVisitedLinkColor ()
		//{
		//        ToolStripLabel tsi = new ToolStripLabel ();

		//        tsi.VisitedLinkColor = Color.Green;
		//        Assert.AreEqual (Color.Green, tsi.VisitedLinkColor, "B1");
		//}


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

		private class ExposeProtectedProperties : ToolStripStatusLabel
		{
			public new Padding DefaultMargin { get { return base.DefaultMargin; } }
		}
	}
}
#endif