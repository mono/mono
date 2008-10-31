#if NET_2_0
using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class SplitContainerTests : TestHelper
	{
		[Test]
		public void TestSplitContainerConstruction ()
		{
			SplitContainer sc = new SplitContainer ();

			Assert.AreEqual (new Size (150, 100), sc.Size, "A1");
			Assert.AreEqual (FixedPanel.None, sc.FixedPanel, "A2");
			Assert.AreEqual (false, sc.IsSplitterFixed, "A3");
			Assert.AreEqual (Orientation.Vertical, sc.Orientation, "A4");
			Assert.AreEqual (false, sc.Panel1Collapsed, "A6");
			Assert.AreEqual (25, sc.Panel1MinSize, "A7");
			Assert.AreEqual (false, sc.Panel2Collapsed, "A9");
			Assert.AreEqual (25, sc.Panel2MinSize, "A10");
			Assert.AreEqual (50, sc.SplitterDistance, "A11");
			Assert.AreEqual (1, sc.SplitterIncrement, "A12");
			Assert.AreEqual (new Rectangle(50, 0, 4, 100), sc.SplitterRectangle, "A13");
			Assert.AreEqual (4, sc.SplitterWidth, "A14");
			Assert.AreEqual (BorderStyle.None, sc.BorderStyle, "A14");
			Assert.AreEqual (DockStyle.None, sc.Dock, "A15");
		}
		
		[Test]
		public void TestProperties ()
		{
			SplitContainer sc = new SplitContainer ();
			
			sc.BorderStyle = BorderStyle.FixedSingle;
			Assert.AreEqual (BorderStyle.FixedSingle, sc.BorderStyle, "C1");

			sc.Dock =  DockStyle.Fill;
			Assert.AreEqual (DockStyle.Fill, sc.Dock, "C2");

			sc.FixedPanel = FixedPanel.Panel1;
			Assert.AreEqual (FixedPanel.Panel1, sc.FixedPanel, "C3");

			sc.IsSplitterFixed = true;
			Assert.AreEqual (true, sc.IsSplitterFixed, "C4");

			sc.Orientation = Orientation.Horizontal;
			Assert.AreEqual (Orientation.Horizontal, sc.Orientation, "C5");

			sc.Panel1Collapsed = true;
			Assert.AreEqual (true, sc.Panel1Collapsed, "C6");
			
			sc.Panel1MinSize = 10;
			Assert.AreEqual (10, sc.Panel1MinSize, "C7");

			sc.Panel2Collapsed = true;
			Assert.AreEqual (true, sc.Panel2Collapsed, "C8");

			sc.Panel2MinSize = 10;
			Assert.AreEqual (10, sc.Panel2MinSize, "C9");

			sc.SplitterDistance = 77;
			Assert.AreEqual (77, sc.SplitterDistance, "C10");
			
			sc.SplitterIncrement = 5;
			Assert.AreEqual (5, sc.SplitterIncrement, "C11");
			
			sc.SplitterWidth = 10;
			Assert.AreEqual (10, sc.SplitterWidth, "C12");
		}
		
		[Test]
		public void TestPanelProperties ()
		{
			SplitContainer sc = new SplitContainer ();
			SplitterPanel p = sc.Panel1;

			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, p.Anchor, "D1");
			p.Anchor = AnchorStyles.None;
			Assert.AreEqual (AnchorStyles.None, p.Anchor, "D1-2");

			Assert.AreEqual (false, p.AutoSize, "D2");
			p.AutoSize = true;
			Assert.AreEqual (true, p.AutoSize, "D2-2");

			Assert.AreEqual (AutoSizeMode.GrowOnly, p.AutoSizeMode, "D3");
			p.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			Assert.AreEqual (AutoSizeMode.GrowOnly, p.AutoSizeMode, "D3-2");

			Assert.AreEqual (BorderStyle.None, p.BorderStyle, "D4");
			p.BorderStyle = BorderStyle.FixedSingle;
			Assert.AreEqual (BorderStyle.FixedSingle, p.BorderStyle, "D4-2");

			Assert.AreEqual (DockStyle.None, p.Dock, "D5");
			p.Dock = DockStyle.Left;
			Assert.AreEqual (DockStyle.Left, p.Dock, "D5-2");

			Assert.AreEqual (new Point (0, 0), p.Location, "D7");
			p.Location = new Point (10, 10);
			Assert.AreEqual (new Point (0, 0), p.Location, "D7-2");

			Assert.AreEqual (new Size (0, 0), p.MaximumSize, "D8");
			p.MaximumSize = new Size (10, 10);
			Assert.AreEqual (new Size (10, 10), p.MaximumSize, "D8-2");

			Assert.AreEqual (new Size (0, 0), p.MinimumSize, "D9");
			p.MinimumSize = new Size (10, 10);
			Assert.AreEqual (new Size (10, 10), p.MinimumSize, "D9-2");

			Assert.AreEqual (String.Empty, p.Name, "D10");
			p.Name = "MyPanel";
			Assert.AreEqual ("MyPanel", p.Name, "D10-2");

			// We set a new max/min size above, so let's start over with new controls
			sc = new SplitContainer();
			p = sc.Panel1;

			Assert.AreEqual (new Size (50, 100), p.Size, "D12");
			p.Size = new Size (10, 10);
			Assert.AreEqual (new Size (50, 100), p.Size, "D12-2");

			//Assert.AreEqual (0, p.TabIndex, "D13");
			p.TabIndex = 4;
			Assert.AreEqual (4, p.TabIndex, "D13-2");

			Assert.AreEqual (false, p.TabStop, "D14");
			p.TabStop = true;
			Assert.AreEqual (true, p.TabStop, "D14-2");

			Assert.AreEqual (true, p.Visible, "D15");
			p.Visible = false;
			Assert.AreEqual (false, p.Visible, "D15-2");
		}
		
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestPanelHeightProperty ()
		{
			SplitContainer sc = new SplitContainer ();
			SplitterPanel p = sc.Panel1;

			Assert.AreEqual (100, p.Height, "E1");
			
			p.Height = 200;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestPanelWidthProperty ()
		{
			SplitContainer sc = new SplitContainer ();
			SplitterPanel p = sc.Panel1;

			Assert.AreEqual (50, p.Width, "F1");

			p.Width = 200;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestPanelParentProperty ()
		{
			SplitContainer sc = new SplitContainer ();
			SplitContainer sc2 = new SplitContainer ();
			SplitterPanel p = sc.Panel1;

			Assert.AreEqual (sc, p.Parent, "G1");

			p.Parent = sc2;
		}

		[Test]
		public void TestSplitterPosition ()
		{
			SplitContainer sc = new SplitContainer ();

			Assert.AreEqual (new Rectangle (50, 0, 4, 100), sc.SplitterRectangle, "H1");
			
			sc.Orientation = Orientation.Horizontal;
			Assert.AreEqual (new Rectangle (0, 50, 150, 4), sc.SplitterRectangle, "H2");
		}

		[Test]
		public void TestFixedPanelNone ()
		{
			SplitContainer sc = new SplitContainer ();

			Assert.AreEqual (50, sc.SplitterDistance, "I1");

			sc.Width = 300;
			Assert.AreEqual (100, sc.SplitterDistance, "I2");
		}
		
		[Test]
		public void TestFixedPanel1 ()
		{
			SplitContainer sc = new SplitContainer ();
			sc.FixedPanel = FixedPanel.Panel1;
			
			Assert.AreEqual (50, sc.SplitterDistance, "J1");

			sc.Width = 300;
			Assert.AreEqual (50, sc.SplitterDistance, "J2");
		}
		
		[Test]
		public void TestFixedPanel2 ()
		{
			SplitContainer sc = new SplitContainer ();
			sc.FixedPanel = FixedPanel.Panel2;

			Assert.AreEqual (50, sc.SplitterDistance, "K1");

			sc.Width = 300;
			Assert.AreEqual (200, sc.SplitterDistance, "K2");
		}

		[Test]
		public void TestSplitterDistance ()
		{
			SplitContainer sc = new SplitContainer ();

			Assert.AreEqual (new Rectangle (50, 0, 4, 100), sc.SplitterRectangle, "L1");

			sc.SplitterDistance = 100;
			Assert.AreEqual (new Rectangle (100, 0, 4, 100), sc.SplitterRectangle, "L2");
		}

		[Test]
		public void TestSplitterWidth ()
		{
			SplitContainer sc = new SplitContainer ();

			Assert.AreEqual (new Rectangle (50, 0, 4, 100), sc.SplitterRectangle, "M1");

			sc.SplitterWidth = 10;
			Assert.AreEqual (new Rectangle (50, 0, 10, 100), sc.SplitterRectangle, "M2");
		}

		[Test]
		public void MethodScaleControl ()
		{
			Form f = new Form ();

			PublicSplitContainer gb = new PublicSplitContainer ();
			gb.Location = new Point (5, 10);
			f.Controls.Add (gb);

			Assert.AreEqual (new Rectangle (5, 10, 150, 100), gb.Bounds, "A1");

			gb.PublicScaleControl (new SizeF (2.0f, 2.0f), BoundsSpecified.All);
			Assert.AreEqual (new Rectangle (10, 20, 300, 200), gb.Bounds, "A2");

			gb.PublicScaleControl (new SizeF (.5f, .5f), BoundsSpecified.Location);
			Assert.AreEqual (new Rectangle (5, 10, 300, 200), gb.Bounds, "A3");

			gb.PublicScaleControl (new SizeF (.5f, .5f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 150, 100), gb.Bounds, "A4");

			gb.PublicScaleControl (new SizeF (3.5f, 3.5f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 525, 350), gb.Bounds, "A5");

			gb.PublicScaleControl (new SizeF (2.5f, 2.5f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 1312, 875), gb.Bounds, "A6");

			gb.PublicScaleControl (new SizeF (.2f, .2f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 262, 175), gb.Bounds, "A7");

			f.Dispose ();
		}

		[Test]
		public void ControlStyle ()
		{
			PublicSplitContainer epp = new PublicSplitContainer ();

			ControlStyles cs = ControlStyles.ContainerControl;
			cs |= ControlStyles.UserPaint;
			cs |= ControlStyles.StandardClick;
			cs |= ControlStyles.SupportsTransparentBackColor;
			cs |= ControlStyles.StandardDoubleClick;
			cs |= ControlStyles.Selectable;
			cs |= ControlStyles.OptimizedDoubleBuffer;
			cs |= ControlStyles.UseTextForAccessibility;

			Assert.AreEqual (cs, epp.GetControlStyles (), "Styles");
		}

		private class PublicSplitContainer : SplitContainer
		{
			public void PublicScaleControl (SizeF factor, BoundsSpecified specified)
			{
				base.ScaleControl (factor, specified);
			}

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