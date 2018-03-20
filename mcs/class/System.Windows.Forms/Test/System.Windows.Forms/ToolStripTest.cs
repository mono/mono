//
// ToolStripTests.cs
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolStripTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStrip ts = new ToolStrip ();

			Assert.AreEqual (false, ts.AllowDrop, "A1");
			//Assert.AreEqual (false, ts.AllowItemReorder, "A2");
			Assert.AreEqual (true, ts.AllowMerge, "A3");
			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, ts.Anchor, "A4");
			Assert.AreEqual (true, ts.AutoSize, "A5");
			Assert.AreEqual (SystemColors.Control, ts.BackColor, "A6");
			Assert.AreEqual (null, ts.BindingContext, "A7");
			Assert.AreEqual (false, ts.CanSelect, "A7-1");
			Assert.AreEqual (true, ts.CanOverflow, "A8");
			Assert.AreEqual (false, ts.CausesValidation, "A9");
			Assert.AreEqual (Cursors.Default, ts.Cursor, "A10");
			Assert.AreEqual (ToolStripDropDownDirection.BelowRight, ts.DefaultDropDownDirection, "A11");
			Assert.AreEqual (new Rectangle (7, 0, 92, 25), ts.DisplayRectangle, "A12");
			Assert.AreEqual (DockStyle.Top, ts.Dock, "A13");
			Assert.AreEqual (SystemFonts.MessageBoxFont, ts.Font, "A14");
			Assert.AreEqual (SystemColors.ControlText, ts.ForeColor, "A15");
			Assert.AreEqual (ToolStripGripDisplayStyle.Vertical, ts.GripDisplayStyle, "A16");
			Assert.AreEqual (new Padding (2), ts.GripMargin, "A17");
			Assert.AreEqual (new Rectangle (2, 0, 3, 25), ts.GripRectangle, "A18");
			Assert.AreEqual (ToolStripGripStyle.Visible, ts.GripStyle, "A19");
			Assert.AreEqual (null, ts.ImageList, "A20");
			Assert.AreEqual (new Size (16, 16), ts.ImageScalingSize, "A21");
			//Assert.AreEqual (false, ts.IsCurrentlyDragging, "A22");
			Assert.AreEqual (false, ts.IsDropDown, "A23");
			Assert.AreEqual ("System.Windows.Forms.ToolStripItemCollection", ts.Items.ToString (), "A24");
			Assert.AreEqual ("System.Windows.Forms.ToolStripSplitStackLayout", ts.LayoutEngine.ToString (), "A25");
			Assert.AreEqual (null, ts.LayoutSettings, "A26");
			Assert.AreEqual (ToolStripLayoutStyle.HorizontalStackWithOverflow, ts.LayoutStyle, "A27");
			Assert.AreEqual (Orientation.Horizontal, ts.Orientation, "A28");
			Assert.AreEqual ("System.Windows.Forms.ToolStripOverflowButton", ts.OverflowButton.GetType ().ToString (), "A29");
			Assert.AreEqual ("System.Windows.Forms.ToolStripProfessionalRenderer", ts.Renderer.ToString (), "A30");
			Assert.AreEqual (ToolStripRenderMode.ManagerRenderMode, ts.RenderMode, "A31");
			Assert.AreEqual (true, ts.ShowItemToolTips, "A32");
			Assert.AreEqual (false, ts.Stretch, "A33");
			Assert.AreEqual (false, ts.TabStop, "A34");
			Assert.AreEqual (ToolStripTextDirection.Horizontal, ts.TextDirection, "A35");
			
			ts = new ToolStrip (new ToolStripButton (), new ToolStripSeparator (), new ToolStripButton ());
			Assert.AreEqual (3, ts.Items.Count, "A36");
		}

		[Test]
		public void ControlStyle ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();
			f.Controls.Add (epp);
			
			ControlStyles cs = ControlStyles.ContainerControl;
			cs |= ControlStyles.UserPaint;
			cs |= ControlStyles.StandardClick;
			cs |= ControlStyles.SupportsTransparentBackColor;
			cs |= ControlStyles.StandardDoubleClick;
			cs |= ControlStyles.AllPaintingInWmPaint;
			cs |= ControlStyles.OptimizedDoubleBuffer;
			cs |= ControlStyles.UseTextForAccessibility;

			Assert.AreEqual (cs, epp.GetControlStyles (), "Styles");
			
			epp.TabStop = true;
			
			cs |= ControlStyles.Selectable;

			Assert.AreEqual (cs, epp.GetControlStyles (), "Styles-2");
			
			epp.TabStop = false;
			
			cs &= ~ControlStyles.Selectable;

			Assert.AreEqual (cs, epp.GetControlStyles (), "Styles-3");
			
			f.Close ();
			f.Dispose ();
		}

		[Test] // bug #80762
		public void DockSize ()
		{
			ToolStrip ts = new ToolStrip();
			Assert.AreEqual (new Size (100, 25), ts.Size, "#1");
			ts.Dock = DockStyle.None;
			Assert.AreEqual (new Size (100, 25), ts.Size, "#2");
		}

		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (DockStyle.Top, epp.DefaultDock, "C1");
			Assert.AreEqual (new Padding (2), epp.DefaultGripMargin, "C2");
			Assert.AreEqual (new Padding (0), epp.DefaultMargin, "C3");
			Assert.AreEqual (new Padding (0,0,1,0), epp.DefaultPadding, "C4");
			Assert.AreEqual (true, epp.DefaultShowItemToolTips, "C5");
			Assert.AreEqual (new Size (100, 25), epp.DefaultSize, "C6");
			Assert.AreEqual (new Size (92, 25), epp.MaxItemSize, "C7");
			
			epp.Size = new Size (300, 100);
			Assert.AreEqual (new Size (292, 100), epp.MaxItemSize, "C8");
			
			epp.GripStyle = ToolStripGripStyle.Hidden;
			Assert.AreEqual (new Size (299, 100), epp.MaxItemSize, "C9");
		}
		
		[Test]
		public void PropertyAllowDrop ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);
			
			ts.AllowDrop = true;
			Assert.AreEqual (true, ts.AllowDrop, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		//[Test]	
		//public void PropertyAllowItemReorder ()
		//{
		//        ToolStrip ts = new ToolStrip ();
		//        EventWatcher ew = new EventWatcher (ts);

		//        ts.AllowItemReorder = true;
		//        Assert.AreEqual (true, ts.AllowItemReorder, "B1");
		//        Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		//}

		//[Test]
		//[ExpectedException (typeof (ArgumentException))]
		//public void PropertyAllowDropAndAllowItemReorderAE ()
		//{
		//        ToolStrip ts = new ToolStrip ();
		//        EventWatcher ew = new EventWatcher (ts);

		//        ts.AllowDrop = true;
		//        ts.AllowItemReorder = true;
		//}

		//[Test]
		//[ExpectedException (typeof (ArgumentException))]
		//public void PropertyAllowDropAndAllowItemReorderAE2 ()
		//{
		//        ToolStrip ts = new ToolStrip ();
		//        EventWatcher ew = new EventWatcher (ts);

		//        ts.AllowItemReorder = true;
		//        ts.AllowDrop = true;
		//}

		[Test]
		public void PropertyAllowMerge ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.AllowMerge = false;
			Assert.AreEqual (false, ts.AllowMerge, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		[Test]
		public void PropertyAnchorAndDocking ()
		{
			ToolStrip ts = new ToolStrip ();

			ts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Bottom, ts.Anchor, "A1");
			Assert.AreEqual (DockStyle.None, ts.Dock, "A2");
			Assert.AreEqual (Orientation.Horizontal, ts.Orientation, "A3");

			ts.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			Assert.AreEqual (AnchorStyles.Left | AnchorStyles.Right, ts.Anchor, "A1");
			Assert.AreEqual (DockStyle.None, ts.Dock, "A2");
			Assert.AreEqual (Orientation.Horizontal, ts.Orientation, "A3");

			ts.Dock = DockStyle.Left;

			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, ts.Anchor, "A1");
			Assert.AreEqual (DockStyle.Left, ts.Dock, "A2");
			Assert.AreEqual (Orientation.Vertical, ts.Orientation, "A3");

			ts.Dock = DockStyle.None;

			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, ts.Anchor, "A1");
			Assert.AreEqual (DockStyle.None, ts.Dock, "A2");
			Assert.AreEqual (Orientation.Horizontal, ts.Orientation, "A3");

			ts.Dock = DockStyle.Top;

			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, ts.Anchor, "A1");
			Assert.AreEqual (DockStyle.Top, ts.Dock, "A2");
			Assert.AreEqual (Orientation.Horizontal, ts.Orientation, "A3");
		}

		[Test]
		public void PropertyAutoSize ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.AutoSize = false;
			Assert.AreEqual (false, ts.AutoSize, "B1");
			Assert.AreEqual ("AutoSizeChanged", ew.ToString (), "B2");
			
			ew.Clear ();
			ts.AutoSize = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyBackColor ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.BackColor = Color.BurlyWood;
			Assert.AreEqual (Color.BurlyWood, ts.BackColor, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		[Test]
		public void PropertyBindingContext ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			BindingContext b = new BindingContext ();
			ts.BindingContext = b;
			Assert.AreSame (b, ts.BindingContext, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		[Test]
		public void PropertyCanOverflow ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.CanOverflow = false;
			Assert.AreEqual (false, ts.CanOverflow, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		[Test]
		public void PropertyCausesValidation ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.CausesValidation = true;
			Assert.AreEqual (true, ts.CausesValidation, "B1");
			Assert.AreEqual ("CausesValidationChanged", ew.ToString (), "B2");

			ew.Clear ();
			ts.CausesValidation = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyCursor ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.Cursor = Cursors.Cross;
			Assert.AreEqual (Cursors.Cross, ts.Cursor, "B1");
			Assert.AreEqual ("CursorChanged", ew.ToString (), "B2");

			ew.Clear ();
			ts.Cursor = Cursors.Cross;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyDefaultDropDownDirection ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.DefaultDropDownDirection = ToolStripDropDownDirection.AboveLeft;
			Assert.AreEqual (ToolStripDropDownDirection.AboveLeft, ts.DefaultDropDownDirection, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyDefaultDropDownDirectionIEAE ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.DefaultDropDownDirection = (ToolStripDropDownDirection)42;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyDockIEAE ()
		{
			ToolStrip ts = new ToolStrip ();
			ts.Dock = (DockStyle)42;
		}

		[Test]
		public void PropertyFont ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			Font f = new Font ("Arial", 12);
			
			ts.Font = f;
			Assert.AreSame (f, ts.Font, "B1");
			Assert.AreEqual ("LayoutCompleted", ew.ToString (), "B2");

			ew.Clear ();
			ts.Font = f;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyForeColor ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.ForeColor = Color.BurlyWood;
			Assert.AreEqual (Color.BurlyWood, ts.ForeColor, "B1");
			Assert.AreEqual ("ForeColorChanged", ew.ToString (), "B2");

			ew.Clear ();
			ts.ForeColor = Color.BurlyWood;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyGripMargin ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.GripMargin = new Padding (6);
			Assert.AreEqual (new Padding (6), ts.GripMargin, "B1");
			Assert.AreEqual ("LayoutCompleted", ew.ToString (), "B2");

			ew.Clear ();
			ts.GripMargin = new Padding (6);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyGripStyle ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.GripStyle = ToolStripGripStyle.Hidden;
			Assert.AreEqual (ToolStripGripStyle.Hidden, ts.GripStyle, "B1");
			Assert.AreEqual ("LayoutCompleted", ew.ToString (), "B2");

			ew.Clear ();
			ts.GripStyle = ToolStripGripStyle.Hidden;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyGripStyleIEAE ()
		{
			ToolStrip ts = new ToolStrip ();

			ts.GripStyle = (ToolStripGripStyle) 42;
		}

		[Test]
		public void PropertyImageList ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ImageList il = new ImageList ();

			ts.ImageList = il;
			Assert.AreSame (il, ts.ImageList, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		[Test]
		public void PropertyImageScalingSize ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.ImageScalingSize = new Size (32, 32);
			Assert.AreEqual (new Size (32, 32), ts.ImageScalingSize, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		[Test]
		public void PropertyLayoutStyle ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
			Assert.AreEqual (ToolStripLayoutStyle.VerticalStackWithOverflow, ts.LayoutStyle, "B1");
			Assert.AreEqual ("LayoutCompleted;LayoutStyleChanged", ew.ToString (), "B2");

			ew.Clear ();
			ts.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");

			ew.Clear ();
			ts.LayoutStyle = ToolStripLayoutStyle.Flow;
			Assert.AreEqual ("LayoutCompleted;LayoutStyleChanged", ew.ToString (), "B4");
			Assert.AreEqual (typeof (FlowLayoutSettings), ts.LayoutSettings.GetType(), "B5");

			ew.Clear ();
			ts.LayoutStyle = ToolStripLayoutStyle.Table;
			Assert.AreEqual ("LayoutCompleted;LayoutStyleChanged", ew.ToString (), "B6");
			Assert.AreEqual (typeof (TableLayoutSettings), ts.LayoutSettings.GetType(), "B7");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyLayoutStyleIEAE ()
		{
			ToolStrip ts = new ToolStrip ();

			ts.LayoutStyle = (ToolStripLayoutStyle) 42;
		}

		[Test]
		public void PropertyRenderer ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ToolStripProfessionalRenderer pr = new ToolStripProfessionalRenderer ();

			ts.Renderer = pr;
			Assert.AreSame (pr, ts.Renderer, "B1");
			Assert.AreEqual ("LayoutCompleted;RendererChanged", ew.ToString (), "B2");
			Assert.AreEqual (ToolStripRenderMode.Custom, ts.RenderMode, "B4");
			
			ew.Clear ();
			ts.Renderer = pr;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyRenderMode ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.RenderMode = ToolStripRenderMode.System;
			Assert.AreEqual (ToolStripRenderMode.System, ts.RenderMode, "B1");
			Assert.AreEqual ("LayoutCompleted;RendererChanged", ew.ToString (), "B2");

			ew.Clear ();
			ts.RenderMode = ToolStripRenderMode.System;
			Assert.AreEqual ("LayoutCompleted;RendererChanged", ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void PropertyRenderModeNSE ()
		{
			ToolStrip ts = new ToolStrip ();

			ts.RenderMode = ToolStripRenderMode.Custom;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyRenderModeIEAE ()
		{
			ToolStrip ts = new ToolStrip ();

			ts.RenderMode = (ToolStripRenderMode) 42;
		}

		[Test]
		public void PropertyShowItemToolTips ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.ShowItemToolTips = false;
			Assert.AreEqual (false, ts.ShowItemToolTips, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		[Test]
		public void PropertyStretch ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.Stretch = true;
			Assert.AreEqual (true, ts.Stretch, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		[Test]
		public void PropertyTabStop ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.TabStop = true;
			Assert.AreEqual (true, ts.TabStop, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");
		}

		//[Test]
		//public void PropertyTextDirection ()
		//{
		//        ToolStrip ts = new ToolStrip ();
		//        EventWatcher ew = new EventWatcher (ts);

		//        ts.TextDirection = ToolStripTextDirection.Vertical270;
		//        Assert.AreEqual (ToolStripTextDirection.Vertical270, ts.TextDirection, "B1");
		//        Assert.AreEqual ("LayoutCompleted", ew.ToString (), "B2");

		//        ew.Clear ();
		//        ts.TextDirection = ToolStripTextDirection.Vertical270;
		//        Assert.AreEqual ("LayoutCompleted", ew.ToString (), "B3");
		//}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void PropertyTextDirectionIEAE ()
		{
			ToolStrip ts = new ToolStrip ();
			EventWatcher ew = new EventWatcher (ts);

			ts.TextDirection = (ToolStripTextDirection)42;
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void BehaviorDisplayRectangleAndOverflow ()
		{
			// WM decoration size dependent
			Form f = new Form ();
			f.ShowInTaskbar = false;
			ToolStrip ts = new ToolStrip ();
			f.Controls.Add (ts);
			f.Show ();

			Assert.AreEqual (false, ts.OverflowButton.Visible, "D1");
			Assert.AreEqual (new Rectangle (7, 0, 284, 25), ts.DisplayRectangle, "D2");

			ts.Items.Add (new ToolStripButton ("hello11111111111"));
			ts.Items.Add (new ToolStripButton ("hello11111111111"));
			ts.Items.Add (new ToolStripButton ("hello11111111111"));
			ts.Items.Add (new ToolStripButton ("hello11111111111"));
			ts.Items.Add (new ToolStripButton ("hello11111111111"));
			ts.Items.Add (new ToolStripButton ("hello11111111111"));
			
			Assert.AreEqual (true, ts.OverflowButton.Visible, "D3");
			Assert.AreEqual (new Rectangle (7, 0, 284, 25), ts.DisplayRectangle, "D4");
			f.Dispose ();
		}
	
		[Test]
		public void BehaviorGripAndOverflowWithFlowLayout ()
		{
			ToolStrip ts = new ToolStrip ();
			ts.LayoutStyle = ToolStripLayoutStyle.Flow;
			
			Assert.AreEqual (ToolStripGripStyle.Visible, ts.GripStyle, "A1");
			Assert.AreEqual (false, ts.OverflowButton.Visible, "A2");
			Assert.AreEqual ("System.Windows.Forms.Layout.FlowLayout", ts.LayoutEngine.ToString (), "A3");			
		}
	
		[Test]
		public void BehaviorDockAndOrientation ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			ToolStrip ts = new ToolStrip ();
			ts.Dock = DockStyle.Left;
			
			f.Controls.Add (ts);
			f.Show ();
			
			Assert.AreEqual (ToolStripLayoutStyle.VerticalStackWithOverflow, ts.LayoutStyle, "A1");
			Assert.AreEqual (Orientation.Vertical, ts.Orientation, "A2");

			ts.LayoutStyle = ToolStripLayoutStyle.StackWithOverflow;
			Assert.AreEqual (ToolStripLayoutStyle.VerticalStackWithOverflow, ts.LayoutStyle, "A3");
			Assert.AreEqual (Orientation.Vertical, ts.Orientation, "A4");

			ts.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
			Assert.AreEqual (ToolStripLayoutStyle.HorizontalStackWithOverflow, ts.LayoutStyle, "A5");
			Assert.AreEqual (Orientation.Horizontal, ts.Orientation, "A6");
			
			ts.LayoutStyle = ToolStripLayoutStyle.Flow;
			Assert.AreEqual (ToolStripLayoutStyle.Flow, ts.LayoutStyle, "A7");
			Assert.AreEqual (Orientation.Horizontal, ts.Orientation, "A8");

			ts.LayoutStyle = ToolStripLayoutStyle.StackWithOverflow;
			Assert.AreEqual (ToolStripLayoutStyle.VerticalStackWithOverflow, ts.LayoutStyle, "A9");
			Assert.AreEqual (Orientation.Vertical, ts.Orientation, "A10");
			
			f.Close ();
		}
		
		[Test]
		public void MethodCreateLayoutSettings ()
		{
			ExposeProtectedProperties ts = new ExposeProtectedProperties ();

			Assert.AreEqual ("System.Windows.Forms.FlowLayoutSettings", ts.PublicCreateLayoutSettings (ToolStripLayoutStyle.Flow).ToString (), "A1");
			Assert.AreEqual (null, ts.PublicCreateLayoutSettings (ToolStripLayoutStyle.HorizontalStackWithOverflow), "A2");
			Assert.AreEqual (null, ts.PublicCreateLayoutSettings (ToolStripLayoutStyle.StackWithOverflow), "A3");
			//Assert.AreEqual ("System.Windows.Forms.TableLayoutSettings", ts.PublicCreateLayoutSettings (ToolStripLayoutStyle.Table).ToString (), "A4");
			Assert.AreEqual (null, ts.PublicCreateLayoutSettings (ToolStripLayoutStyle.VerticalStackWithOverflow), "A5");
		}

		[Test]
		public void MethodDipose()
		{
			ToolStrip ts = new ToolStrip ();
			ToolStripItem item_a = ts.Items.Add ("A");
			ToolStripItem item_b = ts.Items.Add ("B");
			ToolStripItem item_c = ts.Items.Add ("C");

			Assert.AreEqual (3, ts.Items.Count, "A1");

			ts.Dispose ();

			Assert.AreEqual (0, ts.Items.Count, "A2");
			Assert.IsTrue (item_a.IsDisposed, "A3");
			Assert.IsTrue (item_b.IsDisposed, "A4");
			Assert.IsTrue (item_c.IsDisposed, "A5");
		}

		[Test]
		public void MethodGetNextItem ()
		{
			ToolStrip ts = new ToolStrip ();
			ts.Items.Add ("Test Item 1");
			
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Controls.Add (ts);
			f.Show ();

			Assert.AreEqual (ts.Items[0], ts.GetNextItem (null, ArrowDirection.Right), "A1");
			Assert.AreEqual (ts.Items[0], ts.GetNextItem (ts.Items[0], ArrowDirection.Right), "A2");

			ts.Items.Add ("Test Item 2");
			
			Assert.AreEqual (ts.Items[0], ts.GetNextItem (null, ArrowDirection.Right), "A3");
			Assert.AreEqual (ts.Items[1], ts.GetNextItem (ts.Items[0], ArrowDirection.Right), "A4");

			f.Dispose ();
		}
		
		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void MethodGetNextItemIEAE ()
		{
			ToolStrip ts = new ToolStrip ();
			ts.GetNextItem (null, (ArrowDirection)42);
		}
		
		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void MethodResetMinimumSize ()
		{
			ToolStrip ts = new ToolStrip ();
			ts.Size = new Size (600, 600);
			
			Assert.AreEqual (new Size (0, 0), ts.MinimumSize, "M0");
			
			ts.MinimumSize = new Size (400, 400);

			Assert.AreEqual (new Size (600, 600), ts.Size, "M1");
			Assert.AreEqual (new Size (400, 400), ts.MinimumSize, "M2");
			
			ts.ResetMinimumSize ();
			Assert.AreEqual (new Size (600, 600), ts.Size, "M3");
			Assert.AreEqual (new Size (-1, -1), ts.MinimumSize, "M4");
		}
		
		[Test]
		public void TestToolStrip ()
		{
			ToolStrip ts = new ToolStrip ();

			ts.Items.Add (new ToolStripButton ());
			Assert.AreEqual (new Rectangle (0, 0, 100, 25), ts.Bounds, "D1");
			Assert.AreEqual (new Rectangle (7, 0, 92, 25), ts.DisplayRectangle, "D2");
			Assert.AreEqual (new Rectangle (2, 0, 3, 25), ts.GripRectangle, "D4");
			Assert.AreEqual (new Padding (2), ts.GripMargin, "D5");

			ts.GripStyle = ToolStripGripStyle.Hidden;

			Assert.AreEqual (new Rectangle (0, 0, 0, 0), ts.GripRectangle, "D8");
			Assert.AreEqual (new Rectangle (0, 0, 99, 25), ts.DisplayRectangle, "D3");
			Assert.AreEqual (new Padding (2), ts.GripMargin, "D5");

			ts.BackColor = Color.Aquamarine;
			Assert.AreEqual (Color.Aquamarine, ts.BackColor, "A2");

			ts.ForeColor = Color.LightSalmon;
			Assert.AreEqual (Color.LightSalmon, ts.ForeColor, "A5");

			ts.GripMargin = new Padding (3);
			Assert.AreEqual (new Padding (3), ts.GripMargin, "A7");
		}
		
		[Test]
		[Ignore ("Accessibility still needs some work")]
		public void Accessibility ()
		{
			ToolStrip ts = new ToolStrip ();
			AccessibleObject ao = ts.AccessibilityObject;

			Assert.AreEqual ("ControlAccessibleObject: Owner = " + ts.ToString (), ao.ToString (), "L");
			//Assert.AreEqual (Rectangle.Empty, ao.Bounds, "L1");
			Assert.AreEqual (null, ao.DefaultAction, "L2");
			Assert.AreEqual (null, ao.Description, "L3");
			Assert.AreEqual (null, ao.Help, "L4");
			Assert.AreEqual (null, ao.KeyboardShortcut, "L5");
			Assert.AreEqual (null, ao.Name, "L6");
			//Assert.AreEqual (null, ao.Parent, "L7");
			Assert.AreEqual (AccessibleRole.ToolBar, ao.Role, "L8");
			Assert.AreEqual (AccessibleStates.None, ao.State, "L9");
			Assert.AreEqual (null, ao.Value, "L10");

			ts.Name = "Label1";
			ts.Text = "Test Label";
			ts.AccessibleDescription = "Label Desc";

			//Assert.AreEqual (Rectangle.Empty, ao.Bounds, "L11");
			Assert.AreEqual (null, ao.DefaultAction, "L12");
			Assert.AreEqual ("Label Desc", ao.Description, "L13");
			Assert.AreEqual (null, ao.Help, "L14");
			Assert.AreEqual (null, ao.KeyboardShortcut, "L15");
			//Assert.AreEqual ("Test Label", ao.Name, "L16");
			//Assert.AreEqual (null, ao.Parent, "L17");
			Assert.AreEqual (AccessibleRole.ToolBar, ao.Role, "L18");
			Assert.AreEqual (AccessibleStates.None, ao.State, "L19");
			Assert.AreEqual (null, ao.Value, "L20");

			ts.AccessibleName = "Access Label";
			Assert.AreEqual ("Access Label", ao.Name, "L21");

			ts.Text = "Test Label";
			Assert.AreEqual ("Access Label", ao.Name, "L22");

			ts.AccessibleDefaultActionDescription = "AAA";
			Assert.AreEqual ("AAA", ts.AccessibleDefaultActionDescription, "L23");
		}
		
		private class EventWatcher
		{
			private string events = string.Empty;
			
			public EventWatcher (ToolStrip ts)
			{
				ts.AutoSizeChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("AutoSizeChanged;"); });
				//ts.BeginDrag += new EventHandler (delegate (Object obj, EventArgs e) { events += ("BeginDrag;"); });
				ts.CausesValidationChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("CausesValidationChanged;"); });
				ts.CursorChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("CursorChanged;"); });
				//ts.EndDrag += new EventHandler (delegate (Object obj, EventArgs e) { events += ("EndDrag;"); });
				ts.ForeColorChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("ForeColorChanged;"); });
				ts.ItemAdded += new ToolStripItemEventHandler (delegate (Object obj, ToolStripItemEventArgs e) { events += ("ItemAdded;"); });
				ts.ItemClicked += new ToolStripItemClickedEventHandler (delegate (Object obj, ToolStripItemClickedEventArgs e) { events += ("ItemClicked;"); });
				ts.ItemRemoved += new ToolStripItemEventHandler (delegate (Object obj, ToolStripItemEventArgs e) { events += ("ItemRemoved;"); });
				ts.LayoutCompleted += new EventHandler (delegate (Object obj, EventArgs e) { events += ("LayoutCompleted;"); });
				ts.LayoutStyleChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("LayoutStyleChanged;"); });
				ts.PaintGrip += new PaintEventHandler (delegate (Object obj, PaintEventArgs e) { events += ("PaintGrip;"); });
				ts.RendererChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("RendererChanged;"); });
			}

			public override string ToString ()
			{
				return events.TrimEnd (';');
			}
			
			public void Clear ()
			{
				events = string.Empty;
			}
		}
		
		private class ExposeProtectedProperties : ToolStrip
		{
			public new DockStyle DefaultDock { get { return base.DefaultDock; } }
			public new Padding DefaultGripMargin { get { return base.DefaultGripMargin; } }
			public new Padding DefaultMargin { get { return base.DefaultMargin; } }
			public new Padding DefaultPadding { get { return base.DefaultPadding; } }
			public new bool DefaultShowItemToolTips { get { return base.DefaultShowItemToolTips; } }
			public new Size DefaultSize { get { return base.DefaultSize; } }
			public new Size MaxItemSize { get { return base.MaxItemSize; } }
			
			public ControlStyles GetControlStyles ()
			{
				ControlStyles retval = (ControlStyles) 0;
				
				foreach (ControlStyles cs in Enum.GetValues (typeof (ControlStyles)))
					if (this.GetStyle (cs) == true)
						retval |= cs;
						
				return retval;
			}
			
			public LayoutSettings PublicCreateLayoutSettings (ToolStripLayoutStyle layoutStyle) { return base.CreateLayoutSettings (layoutStyle); }
		}
	}
}
