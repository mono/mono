//
// MenuStripTest.cs
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
	public class MenuStripTest
	{
		[Test]
		public void BehaviorMdiWindowMenuItem ()
		{
			Form f = new Form ();
			f.IsMdiContainer = true;
			Form c1 = new Form ();
			c1.MdiParent = f;
			Form c2 = new Form ();
			c2.MdiParent = f;				
		
			MenuStrip ms = new MenuStrip ();
			ToolStripMenuItem tsmi = (ToolStripMenuItem)ms.Items.Add ("Window");
			f.Controls.Add (ms);
			ms.MdiWindowListItem = tsmi;
			
			Assert.AreSame (tsmi, ms.MdiWindowListItem, "Q1");
			Assert.AreEqual (0, tsmi.DropDownItems.Count, "Q2");
			
			f.MainMenuStrip = ms;
			Assert.AreEqual (0, tsmi.DropDownItems.Count, "Q3");

			c1.Show ();
			Assert.AreEqual (0, tsmi.DropDownItems.Count, "Q4");

			f.Show ();
			Assert.AreEqual (1, tsmi.DropDownItems.Count, "Q5");
			Assert.AreEqual (true, (tsmi.DropDownItems[0] as ToolStripMenuItem).Checked, "Q6");
			
			c2.Show ();
			Assert.AreEqual (2, tsmi.DropDownItems.Count, "Q7");
			Assert.AreEqual (true, (tsmi.DropDownItems[1] as ToolStripMenuItem).Checked, "Q8");

			Form c3 = new Form ();
			c3.MdiParent = f;
			Assert.AreEqual (2, tsmi.DropDownItems.Count, "Q9");

			c3.Show ();
			Assert.AreEqual (3, tsmi.DropDownItems.Count, "Q10");
			Assert.AreEqual (true, (tsmi.DropDownItems[2] as ToolStripMenuItem).Checked, "Q11");

			c3.Hide ();
			Assert.AreEqual (2, tsmi.DropDownItems.Count, "Q12");
//			Assert.AreEqual (true, (tsmi.DropDownItems[1] as ToolStripMenuItem).Checked, "Q13");

			// Technically, adding the Cascade item adds it to the end of the list until
			// anything regarding Mdi is clicked, which then moves it to the top of
			// the list and adds the separator.  
			// Calling c3.Show() forces the Cascade menu to the top.
			tsmi.DropDownItems.Add ("Cascade");
			c3.Show ();
			Assert.AreEqual (5, tsmi.DropDownItems.Count, "Q14");
			Assert.AreEqual (true, (tsmi.DropDownItems[4] as ToolStripMenuItem).Checked, "Q15");
		}
	}
}
#endif
