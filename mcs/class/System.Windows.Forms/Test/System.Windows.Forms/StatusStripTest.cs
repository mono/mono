//
// StatusStripTests.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
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
	public class StatusStripTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			StatusStrip ts = new StatusStrip ();

			Assert.AreEqual (false, ts.CanOverflow, "A1");
			Assert.AreEqual (new Rectangle (1, 0, 185, 22), ts.DisplayRectangle, "A2");
			Assert.AreEqual (DockStyle.Bottom, ts.Dock, "A3");
			Assert.AreEqual (ToolStripGripStyle.Hidden, ts.GripStyle, "A4");
			Assert.AreEqual (ToolStripLayoutStyle.Table, ts.LayoutStyle, "A5");
			Assert.AreEqual (new Padding (1, 0, 14, 0), ts.Padding, "A6");
			Assert.AreEqual (false, ts.ShowItemToolTips, "A7");
			Assert.AreEqual (new Rectangle (188, 0, 12, 22), ts.SizeGripBounds, "A8");
			Assert.AreEqual (true, ts.SizingGrip, "A9");
			Assert.AreEqual (true, ts.Stretch, "A10");
			Assert.AreEqual (ToolStripRenderMode.System, ts.RenderMode, "A11");
			
			Assert.AreEqual ("System.Windows.Forms.StatusStrip+StatusStripAccessibleObject", ts.AccessibilityObject.GetType ().ToString ());
			Assert.AreEqual ("System.Windows.Forms.Layout.TableLayout", ts.LayoutEngine.ToString ());
		}

		[Test]
		public void ControlStyle ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();
		
			ControlStyles cs = ControlStyles.ContainerControl;
			cs |= ControlStyles.UserPaint;
			cs |= ControlStyles.ResizeRedraw;
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

			Assert.AreEqual (DockStyle.Bottom, epp.DefaultDock, "C1");
			Assert.AreEqual (new Padding (1, 0, 14, 0), epp.DefaultPadding, "C2");
			Assert.AreEqual (false, epp.DefaultShowItemToolTips, "C3");
			Assert.AreEqual (new Size (200, 22), epp.DefaultSize, "C4");
		}

		[Test]
		public void PropertyCanOverflow ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.CanOverflow = true;
			Assert.AreEqual (true, ts.CanOverflow, "B1");
		}

		[Test]
		public void PropertyDock ()
		{
			StatusStrip ts = new StatusStrip ();
			
			ts.Dock = DockStyle.Top;
			Assert.AreEqual (DockStyle.Top, ts.Dock, "B1");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyDockIEAE ()
		{
			StatusStrip ts = new StatusStrip ();
			ts.Dock = (DockStyle)42;
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

			ts.GripStyle = (ToolStripGripStyle) 42;
		}

		[Test]
		public void PropertyLayoutStyle ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
			Assert.AreEqual (ToolStripLayoutStyle.VerticalStackWithOverflow, ts.LayoutStyle, "B1");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyLayoutStyleIEAE ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.LayoutStyle = (ToolStripLayoutStyle) 42;
		}

		[Test]
		public void PropertyPadding ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.Padding = new Padding (7);
			Assert.AreEqual (new Padding (7), ts.Padding, "B1");
		}

		[Test]
		public void PropertyShowItemToolTips ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.ShowItemToolTips = true;
			Assert.AreEqual (true, ts.ShowItemToolTips, "B1");
		}

		[Test]
		public void PropertySizingGrip ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.SizingGrip = false;
			Assert.AreEqual (false, ts.SizingGrip, "B1");
		}

		[Test]
		public void PropertyStretch ()
		{
			StatusStrip ts = new StatusStrip ();

			ts.Stretch = false;
			Assert.AreEqual (false, ts.Stretch, "B1");
		}
		
		[Test]
		public void Layout ()
		{
			StatusStrip ss = new StatusStrip();
			ToolStripStatusLabel label;

			ss.SuspendLayout ();
			ss.Items.Add ("");
			ss.Items.Add (label = new ToolStripStatusLabel(""));
			ss.Items.Add ("");
			ss.ResumeLayout ();

			Assert.AreEqual (new Rectangle (0, 0, 200, 22), ss.Bounds);
			Assert.AreEqual (new Rectangle (188, 0, 12, 22), ss.SizeGripBounds);
			Assert.AreEqual (new Size (0, 17), ss.Items[0].Size);
			Assert.AreEqual (new Size (0, 17), label.Size);

			Assert.AreEqual (new Rectangle (1, 3, 0, 17), ss.Items[0].Bounds);
			Assert.AreEqual (new Rectangle (1, 3, 0, 17), ss.Items[1].Bounds);
			Assert.AreEqual (new Rectangle (1, 3, 0, 17), ss.Items[2].Bounds);

			label.Spring = true;

			Assert.AreEqual (new Rectangle(1, 3, 0, 17), ss.Items[0].Bounds);
			Assert.AreEqual (new Rectangle(1, 3, 185, 17), ss.Items[1].Bounds);
			Assert.AreEqual (new Rectangle(186, 3, 0, 17), ss.Items[2].Bounds);
		}

		private class ExposeProtectedProperties : StatusStrip
		{
			public new DockStyle DefaultDock { get { return base.DefaultDock; } }
			public new Padding DefaultPadding { get { return base.DefaultPadding; } }
			public new bool DefaultShowItemToolTips { get { return base.DefaultShowItemToolTips; } }
			public new Size DefaultSize { get { return base.DefaultSize; } }
			
			public ControlStyles GetControlStyles ()
			{
				ControlStyles retval = (ControlStyles) 0;
				
				foreach (ControlStyles cs in Enum.GetValues (typeof (ControlStyles)))
					if (this.GetStyle (cs) == true)
						retval |= cs;
						
				return retval;
			}
		}
	}
}
