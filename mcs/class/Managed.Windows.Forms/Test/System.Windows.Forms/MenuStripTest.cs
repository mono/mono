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
	public class MenuStripTest : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			MenuStrip ms = new MenuStrip ();

			Assert.AreEqual (false, ms.CanSelect, "A0");
			Assert.AreEqual (false, ms.CanOverflow, "A1");
			Assert.AreEqual (ToolStripGripStyle.Hidden, ms.GripStyle, "A2");
			Assert.AreEqual (null, ms.MdiWindowListItem, "A3");
			Assert.AreEqual (false, ms.ShowItemToolTips, "A4");
			Assert.AreEqual (true, ms.Stretch, "A5");
			Assert.AreEqual (ToolStripLayoutStyle.HorizontalStackWithOverflow, ms.LayoutStyle, "A6");
			
			Assert.AreEqual ("System.Windows.Forms.MenuStrip+MenuStripAccessibleObject", ms.AccessibilityObject.GetType ().ToString (), "A7");
		}

		[Test]
		public void ControlStyle ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			ControlStyles cs = ControlStyles.ContainerControl;
			cs |= ControlStyles.UserPaint;
			cs |= ControlStyles.StandardClick;
			cs |= ControlStyles.SupportsTransparentBackColor;
			cs |= ControlStyles.StandardDoubleClick;
			cs |= ControlStyles.AllPaintingInWmPaint;
			cs |= ControlStyles.OptimizedDoubleBuffer;
			cs |= ControlStyles.UseTextForAccessibility;

			Assert.AreEqual (cs, epp.GetControlStyles (), "Styles");
		}
		
		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (new Padding (2, 2, 0, 2), epp.DefaultGripMargin, "C1");
			Assert.AreEqual (new Padding (6, 2, 0, 2), epp.DefaultPadding, "C2");
			Assert.AreEqual (false, epp.DefaultShowItemToolTips, "C3");
			Assert.AreEqual (new Size (200, 24), epp.DefaultSize, "C4");
		}

		[Test]
		public void PropertyCanOverflow ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.CanOverflow = true;
			Assert.AreEqual (true, ts.CanOverflow, "B1");
		}

		[Test]
		public void PropertyGripStyle ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.GripStyle = ToolStripGripStyle.Visible;
			Assert.AreEqual (ToolStripGripStyle.Visible, ts.GripStyle, "B1");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyGripStyleIEAE ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.GripStyle = (ToolStripGripStyle)42;
		}

		[Test]
		public void PropertyShowItemToolTips ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.ShowItemToolTips = true;
			Assert.AreEqual (true, ts.ShowItemToolTips, "B1");
		}
		
		[Test]
		public void PropertyStretch ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.Stretch = false;
			Assert.AreEqual (false, ts.Stretch, "B1");
		}

		[Test]
		public void BehaviorMdiWindowMenuItem ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
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
			
			f.Close ();
		}

		[Test] // bug #342358
		public void MdiWindowListItem_NoMdiContainer ()
		{
			Form f = new Form ();
			MenuStrip ms = new MenuStrip ();
			f.MainMenuStrip = ms;
			f.Controls.Add (ms);
			ms.MdiWindowListItem = new ToolStripMenuItem ("Window");
			Assert.IsNotNull (ms.MdiWindowListItem);
		}

		private class ExposeProtectedProperties : MenuStrip
		{
			public new Padding DefaultGripMargin { get { return base.DefaultGripMargin; } }
			public new Padding DefaultPadding { get { return base.DefaultPadding; } }
			public new bool DefaultShowItemToolTips { get { return base.DefaultShowItemToolTips; } }
			public new Size DefaultSize { get { return base.DefaultSize; } }

			public ControlStyles GetControlStyles ()
			{
				ControlStyles retval = (ControlStyles)0;

				foreach (ControlStyles cs in Enum.GetValues (typeof (ControlStyles)))
					if (this.GetStyle (cs) == true)
						retval |= cs;

				return retval;
			}
		}
	}
}
#endif
