using System;
using System.Text;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture()]	
	public class FlowPanelTests : TestHelper
	{
		[Test]
		public void TestConstruction()
		{
			FlowLayoutPanel p = new FlowLayoutPanel();
			
			Assert.AreEqual(FlowDirection.LeftToRight, p.FlowDirection, "A1");
			Assert.AreEqual(true, p.WrapContents, "A2");
			Assert.AreEqual("System.Windows.Forms.Layout.FlowLayout", p.LayoutEngine.ToString(), "A3");
			
			p.FlowDirection = FlowDirection.BottomUp;
			p.WrapContents = false;

			Assert.AreEqual (FlowDirection.BottomUp, p.FlowDirection, "A4");
			Assert.AreEqual (false, p.WrapContents, "A5");
		}
		
		[Test]
		public void TestExtenderProvider()
		{
			FlowLayoutPanel p = new FlowLayoutPanel ();
			Button b = new Button();
			
			Assert.AreEqual(false, p.GetFlowBreak(b), "B1");
			
			p.SetFlowBreak(b, true);

			Assert.AreEqual (true, p.GetFlowBreak (b), "B2");
		}

		#region LeftToRight Tests
		[Test]
		public void LeftToRightLayoutTest1 ()
		{
			// 2 Normal Buttons
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[0].Bounds, "C1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "C2");
		}

		[Test]
		public void LeftToRightLayoutTest2 ()
		{
			// Dock Fill and Normal
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[0].Bounds, "D1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "D2");
		}

		[Test]
		public void LeftToRightLayoutTest3 ()
		{
			// Anchored: Top/Bottom and Normal
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[0].Bounds, "E1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "E2");
		}

		[Test]
		public void LeftToRightLayoutTest4 ()
		{
			// Anchored: Top/Bottom and Dock Fill
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 0), p.Controls[0].Bounds, "F1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 0), p.Controls[1].Bounds, "F2");
		}

		[Test]
		public void LeftToRightLayoutTest5 ()
		{
			// 2 Anchored: Top/Bottom
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Bottom));

			Assert.AreEqual (new Rectangle (0, 0, 100, 0), p.Controls[0].Bounds, "G1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 0), p.Controls[1].Bounds, "G2");
		}

		[Test]
		public void LeftToRightLayoutTest6 ()
		{
			// 2 Dock Fill
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 0), p.Controls[0].Bounds, "H1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 0), p.Controls[1].Bounds, "H2");
		}

		[Test]
		public void LeftToRightLayoutTest7 ()
		{
			// Dock Top
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.Top, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 50), p.Controls[0].Bounds, "I1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "I2");
		}

		[Test]
		public void LeftToRightLayoutTest8 ()
		{
			// Dock Bottom
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.Bottom, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 50, 100, 50), p.Controls[0].Bounds, "J1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "J2");
		}

		[Test]
		public void LeftToRightLayoutTest9 ()
		{
			// Anchor Bottom
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 50, 100, 50), p.Controls[0].Bounds, "K1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "K2");
		}

		[Test]
		public void LeftToRightLayoutTest10 ()
		{
			// No Dock or Anchor
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.None));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 25, 100, 50), p.Controls[0].Bounds, "L1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "L2");
		}

		[Test]
		public void LeftToRightLayoutTest11 ()
		{
			// WrapContents = true
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 50), p.Controls[0].Bounds, "M1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "M2");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[2].Bounds, "M3");
			Assert.AreEqual (new Rectangle (100, 100, 100, 100), p.Controls[3].Bounds, "M4");
		}

		[Test]
		public void LeftToRightLayoutTest12 ()
		{
			// WrapContents = false
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.WrapContents = false;
			
			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 50), p.Controls[0].Bounds, "N1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "N2");
			Assert.AreEqual (new Rectangle (200, 0, 100, 100), p.Controls[2].Bounds, "N3");
			Assert.AreEqual (new Rectangle (300, 0, 100, 100), p.Controls[3].Bounds, "N4");
		}

		[Test]
		public void LeftToRightLayoutTest13 ()
		{
			// SetFlowBreak 1, 3
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Size (400, 100), p.PreferredSize, "O1");

			p.SetFlowBreak (p.Controls[0], true);
			p.SetFlowBreak (p.Controls[2], true);

			Assert.AreEqual (new Size (200, 300), p.PreferredSize, "O2");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[0].Bounds, "O3");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "O4");
			Assert.AreEqual (new Rectangle (100, 100, 100, 100), p.Controls[2].Bounds, "O5");
			Assert.AreEqual (new Rectangle (0, 200, 100, 100), p.Controls[3].Bounds, "O6");
		}

		[Test]
		public void LeftToRightLayoutTest14 ()
		{
			// Margins
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (1,3,5,2), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (7,3,12,5), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (14,7,1,3), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Size (248, 60), p.PreferredSize, "P1");
			Assert.AreEqual (new Rectangle (1, 3, 50, 50), p.Controls[0].Bounds, "P2");
			Assert.AreEqual (new Rectangle (63, 3, 50, 50), p.Controls[1].Bounds, "P3");
			Assert.AreEqual (new Rectangle (139, 7, 50, 50), p.Controls[2].Bounds, "P4");
			Assert.AreEqual (new Rectangle (4, 64, 50, 50), p.Controls[3].Bounds, "P5");
		}

		[Test]
		public void LeftToRightLayoutTest15 ()
		{
			// Margins and Different Sizes
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (25, 45, false, DockStyle.None, new Padding (6), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (60, 20, false, DockStyle.None, new Padding (9), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (15, 85, false, DockStyle.None, new Padding (2), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 20, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Size (192, 89), p.PreferredSize, "Q1");
			Assert.AreEqual (new Rectangle (6, 6, 25, 45), p.Controls[0].Bounds, "Q2");
			Assert.AreEqual (new Rectangle (46, 9, 60, 20), p.Controls[1].Bounds, "Q3");
			Assert.AreEqual (new Rectangle (117, 2, 15, 85), p.Controls[2].Bounds, "Q4");
			Assert.AreEqual (new Rectangle (138, 4, 50, 20), p.Controls[3].Bounds, "Q5");
		}

		[Test]
		public void LeftToRightLayoutTest16 ()
		{
			// Random Complex Layout 1
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (25, 45, false, DockStyle.None, new Padding (6), AnchorStyles.Right | AnchorStyles.Top));
			p.Controls.Add (CreateButton (60, 20, false, DockStyle.Fill, new Padding (9), AnchorStyles.Bottom | AnchorStyles.Top));
			p.Controls.Add (CreateButton (15, 85, false, DockStyle.None, new Padding (2), AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (50, 20, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (13, 22, false, DockStyle.None, new Padding (12), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (73, 28, false, DockStyle.Top, new Padding (6), AnchorStyles.None));

			Assert.AreEqual (new Size (314, 57), p.PreferredSize, "R1");
			Assert.AreEqual (new Rectangle (6, 6, 25, 45), p.Controls[0].Bounds, "R2");
			Assert.AreEqual (new Rectangle (46, 9, 60, 39), p.Controls[1].Bounds, "R3");
			Assert.AreEqual (new Rectangle (117, 2, 15, 53), p.Controls[2].Bounds, "R4");
			Assert.AreEqual (new Rectangle (138, 33, 50, 20), p.Controls[3].Bounds, "R5");
			Assert.AreEqual (new Rectangle (12, 69, 13, 22), p.Controls[4].Bounds, "R6");
			Assert.AreEqual (new Rectangle (43, 63, 73, 28), p.Controls[5].Bounds, "R7");
		}

		[Test]
		public void LeftToRightLayoutTest17 ()
		{
			// Random Complex Layout 2
			FlowLayoutPanel p = new FlowLayoutPanel ();

			p.Controls.Add (CreateButton (12, 345, false, DockStyle.Bottom, new Padding (1, 2, 3, 4), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (9, 44, false, DockStyle.Top, new Padding (6, 3, 2, 7), AnchorStyles.Right | AnchorStyles.Top));
			p.Controls.Add (CreateButton (78, 14, false, DockStyle.None, new Padding (5, 1, 2, 4), AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right));
			p.Controls.Add (CreateButton (21, 64, false, DockStyle.Top, new Padding (3, 3, 3, 1), AnchorStyles.None));
			p.Controls.Add (CreateButton (14, 14, false, DockStyle.Fill, new Padding (11, 4, 6, 3), AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (132, 6, false, DockStyle.Fill, new Padding (5, 5, 4, 5), AnchorStyles.Top | AnchorStyles.Bottom));

			p.SetFlowBreak (p.Controls[0], true);
			p.SetFlowBreak (p.Controls[2], true);

			Assert.AreEqual (new Rectangle (1, 2, 12, 345), p.Controls[0].Bounds, "S1");
			Assert.AreEqual (new Rectangle (6, 354, 9, 44), p.Controls[1].Bounds, "S2");
			Assert.AreEqual (new Rectangle (22, 352, 78, 49), p.Controls[2].Bounds, "S3");
			Assert.AreEqual (new Rectangle (3, 408, 21, 64), p.Controls[3].Bounds, "S4");
			Assert.AreEqual (new Rectangle (38, 409, 14, 61), p.Controls[4].Bounds, "S5");
			Assert.AreEqual (new Rectangle (63, 410, 132, 58), p.Controls[5].Bounds, "S6");
		}
		
		[Test]
		public void LeftToRightLayoutTest18 ()
		{
			// SetFlowBreak has no effect when WrapContents = false
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.WrapContents = false;

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			p.SetFlowBreak(p.Controls[0], true);
			
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[0].Bounds, "T1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "T2");
		}
		#endregion

		#region RightToLeft Tests
		[Test]
		public void RightToLeftLayoutTest1 ()
		{
			// 2 Normal Buttons
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;
			
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[0].Bounds, "AC1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "AC2");
		}

		[Test]
		public void RightToLeftLayoutTest2 ()
		{
			// Dock Fill and Normal
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[0].Bounds, "AD1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "AD2");
		}

		[Test]
		public void RightToLeftLayoutTest3 ()
		{
			// Anchored: Top/Bottom and Normal
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[0].Bounds, "AE1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "AE2");
		}

		[Test]
		public void RightToLeftLayoutTest4 ()
		{
			// Anchored: Top/Bottom and Dock Fill
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 0, 100, 0), p.Controls[0].Bounds, "AF1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 0), p.Controls[1].Bounds, "AF2");
		}

		[Test]
		public void RightToLeftLayoutTest5 ()
		{
			// 2 Anchored: Top/Bottom
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Bottom));

			Assert.AreEqual (new Rectangle (100, 0, 100, 0), p.Controls[0].Bounds, "AG1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 0), p.Controls[1].Bounds, "AG2");
		}

		[Test]
		public void RightToLeftLayoutTest6 ()
		{
			// 2 Dock Fill
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 0, 100, 0), p.Controls[0].Bounds, "AH1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 0), p.Controls[1].Bounds, "AH2");
		}

		[Test]
		public void RightToLeftLayoutTest7 ()
		{
			// Dock Top
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.Top, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 0, 100, 50), p.Controls[0].Bounds, "AI1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "AI2");
		}

		[Test]
		public void RightToLeftLayoutTest8 ()
		{
			// Dock Bottom
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.Bottom, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 50, 100, 50), p.Controls[0].Bounds, "AJ1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "AJ2");
		}

		[Test]
		public void RightToLeftLayoutTest9 ()
		{
			// Anchor Bottom
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 50, 100, 50), p.Controls[0].Bounds, "AK1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "AK2");
		}

		[Test]
		public void RightToLeftLayoutTest10 ()
		{
			// No Dock or Anchor
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.None));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 25, 100, 50), p.Controls[0].Bounds, "AL1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "AL2");
		}

		[Test]
		public void RightToLeftLayoutTest11 ()
		{
			// WrapContents = true
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 0, 100, 50), p.Controls[0].Bounds, "AM1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "AM2");
			Assert.AreEqual (new Rectangle (100, 100, 100, 100), p.Controls[2].Bounds, "AM3");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[3].Bounds, "AM4");
		}

		[Test]
		public void RightToLeftLayoutTest12 ()
		{
			// WrapContents = false
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.WrapContents = false;
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 50, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (100, 0, 100, 50), p.Controls[0].Bounds, "AN1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "AN2");
			Assert.AreEqual (new Rectangle (-100, 0, 100, 100), p.Controls[2].Bounds, "AN3");
			Assert.AreEqual (new Rectangle (-200, 0, 100, 100), p.Controls[3].Bounds, "AN4");
		}

		[Test]
		public void RightToLeftLayoutTest13 ()
		{
			// SetFlowBreak 1, 3
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			p.SetFlowBreak (p.Controls[0], true);
			p.SetFlowBreak (p.Controls[2], true);

			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[0].Bounds, "AO1");
			Assert.AreEqual (new Rectangle (100, 100, 100, 100), p.Controls[1].Bounds, "AO2");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[2].Bounds, "AO3");
			Assert.AreEqual (new Rectangle (100, 200, 100, 100), p.Controls[3].Bounds, "AO4");
		}

		[Test]
		public void RightToLeftLayoutTest14 ()
		{
			// Margins
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (1, 3, 5, 2), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (7, 3, 12, 5), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (14, 7, 1, 3), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (145, 3, 50, 50), p.Controls[0].Bounds, "AP1");
			Assert.AreEqual (new Rectangle (82, 3, 50, 50), p.Controls[1].Bounds, "AP2");
			Assert.AreEqual (new Rectangle (24, 7, 50, 50), p.Controls[2].Bounds, "AP3");
			Assert.AreEqual (new Rectangle (146, 64, 50, 50), p.Controls[3].Bounds, "AP4");
		}

		[Test]
		public void RightToLeftLayoutTest15 ()
		{
			// Margins and Different Sizes
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (25, 45, false, DockStyle.None, new Padding (6), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (60, 20, false, DockStyle.None, new Padding (9), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (15, 85, false, DockStyle.None, new Padding (2), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 20, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (169, 6, 25, 45), p.Controls[0].Bounds, "AQ1");
			Assert.AreEqual (new Rectangle (94, 9, 60, 20), p.Controls[1].Bounds, "AQ2");
			Assert.AreEqual (new Rectangle (68, 2, 15, 85), p.Controls[2].Bounds, "AQ3");
			Assert.AreEqual (new Rectangle (12, 4, 50, 20), p.Controls[3].Bounds, "AQ4");
		}

		[Test]
		public void RightToLeftLayoutTest16 ()
		{
			// Random Complex Layout 1
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (25, 45, false, DockStyle.None, new Padding (6), AnchorStyles.Right | AnchorStyles.Top));
			p.Controls.Add (CreateButton (60, 20, false, DockStyle.Fill, new Padding (9), AnchorStyles.Bottom | AnchorStyles.Top));
			p.Controls.Add (CreateButton (15, 85, false, DockStyle.None, new Padding (2), AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (50, 20, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (13, 22, false, DockStyle.None, new Padding (12), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (73, 28, false, DockStyle.Top, new Padding (6), AnchorStyles.None));

			Assert.AreEqual (new Rectangle (169, 6, 25, 45), p.Controls[0].Bounds, "AR1");
			Assert.AreEqual (new Rectangle (94, 9, 60, 39), p.Controls[1].Bounds, "AR2");
			Assert.AreEqual (new Rectangle (68, 2, 15, 53), p.Controls[2].Bounds, "AR3");
			Assert.AreEqual (new Rectangle (12, 33, 50, 20), p.Controls[3].Bounds, "AR4");
			Assert.AreEqual (new Rectangle (175, 69, 13, 22), p.Controls[4].Bounds, "AR5");
			Assert.AreEqual (new Rectangle (84, 63, 73, 28), p.Controls[5].Bounds, "AR6");
		}

		[Test]
		public void RightToLeftLayoutTest17 ()
		{
			// Random Complex Layout 2
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (12, 345, false, DockStyle.Bottom, new Padding (1, 2, 3, 4), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (9, 44, false, DockStyle.Top, new Padding (6, 3, 2, 7), AnchorStyles.Right | AnchorStyles.Top));
			p.Controls.Add (CreateButton (78, 14, false, DockStyle.None, new Padding (5, 1, 2, 4), AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right));
			p.Controls.Add (CreateButton (21, 64, false, DockStyle.Top, new Padding (3, 3, 3, 1), AnchorStyles.None));
			p.Controls.Add (CreateButton (14, 14, false, DockStyle.Fill, new Padding (11, 4, 6, 3), AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (132, 6, false, DockStyle.Fill, new Padding (5, 5, 4, 5), AnchorStyles.Top | AnchorStyles.Bottom));

			p.SetFlowBreak (p.Controls[0], true);
			p.SetFlowBreak (p.Controls[2], true);

			Assert.AreEqual (new Rectangle (185, 2, 12, 345), p.Controls[0].Bounds, "AS1");
			Assert.AreEqual (new Rectangle (189, 354, 9, 44), p.Controls[1].Bounds, "AS2");
			Assert.AreEqual (new Rectangle (103, 352, 78, 49), p.Controls[2].Bounds, "AS3");
			Assert.AreEqual (new Rectangle (176, 408, 21, 64), p.Controls[3].Bounds, "AS4");
			Assert.AreEqual (new Rectangle (153, 409, 14, 61), p.Controls[4].Bounds, "AS5");
			Assert.AreEqual (new Rectangle (6, 410, 132, 58), p.Controls[5].Bounds, "AS6");
		}

		[Test]
		public void RightToLeftLayoutTest18 ()
		{
			// SetFlowBreak has no effect when WrapContents = false
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.WrapContents = false;
			p.FlowDirection = FlowDirection.RightToLeft;

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			p.SetFlowBreak (p.Controls[0], true);

			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[0].Bounds, "AT1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "AT2");
		}
		#endregion

		#region TopDown Tests
		[Test]
		public void TopDownLayoutTest1 ()
		{
			// 2 Normal Buttons
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[0].Bounds, "BC1");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "BC2");
		}

		[Test]
		public void TopDownLayoutTest2 ()
		{
			// Dock Fill and Normal
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[0].Bounds, "BD1");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "BD2");
		}

		[Test]
		public void TopDownLayoutTest3 ()
		{
			// Anchored: Left/Right and Normal
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[0].Bounds, "BE1");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "BE2");
		}

		[Test]
		public void TopDownLayoutTest4 ()
		{
			// Anchored: Left/Right and Dock Fill
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 0, 100), p.Controls[0].Bounds, "BF1");
			Assert.AreEqual (new Rectangle (0, 100, 0, 100), p.Controls[1].Bounds, "BF2");
		}

		[Test]
		public void TopDownLayoutTest5 ()
		{
			// 2 Anchored: Left/Right
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Right));

			Assert.AreEqual (new Rectangle (0, 0, 0, 100), p.Controls[0].Bounds, "BG1");
			Assert.AreEqual (new Rectangle (0, 100, 0, 100), p.Controls[1].Bounds, "BG2");
		}

		[Test]
		public void TopDownLayoutTest6 ()
		{
			// 2 Dock Fill
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 0, 100), p.Controls[0].Bounds, "BH1");
			Assert.AreEqual (new Rectangle (0, 100, 0, 100), p.Controls[1].Bounds, "BH2");
		}

		[Test]
		public void TopDownLayoutTest7 ()
		{
			// Dock Left
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.Left, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 50, 100), p.Controls[0].Bounds, "BI1");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "BI2");
		}

		[Test]
		public void TopDownLayoutTest8 ()
		{
			// Dock Right
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.Right, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (50, 0, 50, 100), p.Controls[0].Bounds, "BJ1");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "BJ2");
		}

		[Test]
		public void TopDownLayoutTest9 ()
		{
			// Anchor Right
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Right));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Left));

			Assert.AreEqual (new Rectangle (50, 0, 50, 100), p.Controls[0].Bounds, "BK1");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "BK2");
		}

		[Test]
		public void TopDownLayoutTest10 ()
		{
			// No Dock or Anchor
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.None));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (25, 0, 50, 100), p.Controls[0].Bounds, "BL1");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "BL2");
		}

		[Test]
		public void TopDownLayoutTest11 ()
		{
			// WrapContents = true
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 50, 100), p.Controls[0].Bounds, "BM1");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "BM2");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[2].Bounds, "BM3");
			Assert.AreEqual (new Rectangle (100, 100, 100, 100), p.Controls[3].Bounds, "BM4");
		}

		[Test]
		public void TopDownLayoutTest12 ()
		{
			// WrapContents = false
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.WrapContents = false;
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 0, 50, 100), p.Controls[0].Bounds, "BN1");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "BN2");
			Assert.AreEqual (new Rectangle (0, 200, 100, 100), p.Controls[2].Bounds, "BN3");
			Assert.AreEqual (new Rectangle (0, 300, 100, 100), p.Controls[3].Bounds, "BN4");
		}

		[Test]
		public void TopDownLayoutTest13 ()
		{
			// SetFlowBreak 1, 3
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			p.SetFlowBreak (p.Controls[0], true);
			p.SetFlowBreak (p.Controls[2], true);

			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[0].Bounds, "BO1");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[1].Bounds, "BO2");
			Assert.AreEqual (new Rectangle (100, 100, 100, 100), p.Controls[2].Bounds, "BO3");
			Assert.AreEqual (new Rectangle (200, 0, 100, 100), p.Controls[3].Bounds, "BO4");
		}

		[Test]
		public void TopDownLayoutTest14 ()
		{
			// Margins
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (1, 3, 5, 2), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (7, 3, 12, 5), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (14, 7, 1, 3), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (1, 3, 50, 50), p.Controls[0].Bounds, "BP1");
			Assert.AreEqual (new Rectangle (7, 58, 50, 50), p.Controls[1].Bounds, "BP2");
			Assert.AreEqual (new Rectangle (14, 120, 50, 50), p.Controls[2].Bounds, "BP3");
			Assert.AreEqual (new Rectangle (73, 4, 50, 50), p.Controls[3].Bounds, "BP4");
		}

		[Test]
		public void TopDownLayoutTest15 ()
		{
			// Margins and Different Sizes
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (25, 45, false, DockStyle.None, new Padding (6), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (60, 20, false, DockStyle.None, new Padding (9), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (15, 85, false, DockStyle.None, new Padding (2), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 20, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (6, 6, 25, 45), p.Controls[0].Bounds, "BQ1");
			Assert.AreEqual (new Rectangle (9, 66, 60, 20), p.Controls[1].Bounds, "BQ2");
			Assert.AreEqual (new Rectangle (2, 97, 15, 85), p.Controls[2].Bounds, "BQ3");
			Assert.AreEqual (new Rectangle (82, 4, 50, 20), p.Controls[3].Bounds, "BQ4");
		}

		[Test]
		public void TopDownLayoutTest16 ()
		{
			// Random Complex Layout 1
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (25, 45, false, DockStyle.None, new Padding (6), AnchorStyles.Right | AnchorStyles.Top));
			p.Controls.Add (CreateButton (60, 20, false, DockStyle.Fill, new Padding (9), AnchorStyles.Bottom | AnchorStyles.Top));
			p.Controls.Add (CreateButton (15, 85, false, DockStyle.None, new Padding (2), AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (50, 20, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (13, 22, false, DockStyle.None, new Padding (12), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (73, 28, false, DockStyle.Left, new Padding (6), AnchorStyles.None));

			Assert.AreEqual (new Rectangle (6, 6, 25, 45), p.Controls[0].Bounds, "BR1");
			Assert.AreEqual (new Rectangle (9, 66, 19, 20), p.Controls[1].Bounds, "BR2");
			Assert.AreEqual (new Rectangle (2, 97, 15, 85), p.Controls[2].Bounds, "BR3");
			Assert.AreEqual (new Rectangle (41, 4, 50, 20), p.Controls[3].Bounds, "BR4");
			Assert.AreEqual (new Rectangle (49, 40, 61, 22), p.Controls[4].Bounds, "BR5");
			Assert.AreEqual (new Rectangle (43, 80, 73, 28), p.Controls[5].Bounds, "BR6");
		}

		[Test]
		public void TopDownLayoutTest17 ()
		{
			// Random Complex Layout 2
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (12, 345, false, DockStyle.Right, new Padding (1, 2, 3, 4), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (9, 44, false, DockStyle.Left, new Padding (6, 3, 2, 7), AnchorStyles.Right | AnchorStyles.Top));
			p.Controls.Add (CreateButton (78, 14, false, DockStyle.None, new Padding (5, 1, 2, 4), AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right));
			p.Controls.Add (CreateButton (21, 64, false, DockStyle.Left, new Padding (3, 3, 3, 1), AnchorStyles.None));
			p.Controls.Add (CreateButton (14, 14, false, DockStyle.Fill, new Padding (11, 4, 6, 3), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (132, 6, false, DockStyle.Fill, new Padding (5, 5, 4, 5), AnchorStyles.Left | AnchorStyles.Right));

			p.SetFlowBreak (p.Controls[0], true);
			p.SetFlowBreak (p.Controls[2], true);

			Assert.AreEqual (new Rectangle (1, 2, 12, 345), p.Controls[0].Bounds, "BS1");
			Assert.AreEqual (new Rectangle (22, 3, 9, 44), p.Controls[1].Bounds, "BS2");
			Assert.AreEqual (new Rectangle (21, 55, 10, 14), p.Controls[2].Bounds, "BS3");
			Assert.AreEqual (new Rectangle (36, 3, 21, 64), p.Controls[3].Bounds, "BS4");
			Assert.AreEqual (new Rectangle (44, 72, 10, 14), p.Controls[4].Bounds, "BS5");
			Assert.AreEqual (new Rectangle (38, 94, 18, 6), p.Controls[5].Bounds, "BS6");
		}

		[Test]
		public void TopDownLayoutTest18 ()
		{
			// SetFlowBreak has no effect when WrapContents = false
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.WrapContents = false;
			p.FlowDirection = FlowDirection.TopDown;

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			p.SetFlowBreak (p.Controls[0], true);

			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[0].Bounds, "BT1");
			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[1].Bounds, "BT2");
		}
		#endregion

		#region BottomUp Tests
		[Test]
		public void BottomUpLayoutTest1 ()
		{
			// 2 Normal Buttons
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[0].Bounds, "CC1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "CC2");
		}

		[Test]
		public void BottomUpLayoutTest2 ()
		{
			// Dock Fill and Normal
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[0].Bounds, "CD1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "CD2");
		}

		[Test]
		public void BottomUpLayoutTest3 ()
		{
			// Anchored: Left/Right and Normal
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[0].Bounds, "CE1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "CE2");
		}

		[Test]
		public void BottomUpLayoutTest4 ()
		{
			// Anchored: Left/Right and Dock Fill
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 100, 0, 100), p.Controls[0].Bounds, "CF1");
			Assert.AreEqual (new Rectangle (0, 0, 0, 100), p.Controls[1].Bounds, "CF2");
		}

		[Test]
		public void BottomUpLayoutTest5 ()
		{
			// 2 Anchored: Left/Right
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Right));

			Assert.AreEqual (new Rectangle (0, 100, 0, 100), p.Controls[0].Bounds, "CG1");
			Assert.AreEqual (new Rectangle (0, 0, 0, 100), p.Controls[1].Bounds, "CG2");
		}

		[Test]
		public void BottomUpLayoutTest6 ()
		{
			// 2 Dock Fill
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.Fill, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 100, 0, 100), p.Controls[0].Bounds, "CH1");
			Assert.AreEqual (new Rectangle (0, 0, 0, 100), p.Controls[1].Bounds, "CH2");
		}

		[Test]
		public void BottomUpLayoutTest7 ()
		{
			// Dock Left
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.Left, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 100, 50, 100), p.Controls[0].Bounds, "CI1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "CI2");
		}

		[Test]
		public void BottomUpLayoutTest8 ()
		{
			// Dock Right
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.Right, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (50, 100, 50, 100), p.Controls[0].Bounds, "CJ1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "CJ2");
		}

		[Test]
		public void BottomUpLayoutTest9 ()
		{
			// Anchor Right
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Right));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Top | AnchorStyles.Left));

			Assert.AreEqual (new Rectangle (50, 100, 50, 100), p.Controls[0].Bounds, "CK1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "CK2");
		}

		[Test]
		public void BottomUpLayoutTest10 ()
		{
			// No Dock or Anchor
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.None));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (25, 100, 50, 100), p.Controls[0].Bounds, "CL1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "CL2");
		}

		[Test]
		public void BottomUpLayoutTest11 ()
		{
			// WrapContents = true
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 100, 50, 100), p.Controls[0].Bounds, "CM1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "CM2");
			Assert.AreEqual (new Rectangle (100, 100, 100, 100), p.Controls[2].Bounds, "CM3");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[3].Bounds, "CM4");
		}

		[Test]
		public void BottomUpLayoutTest12 ()
		{
			// WrapContents = false
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.WrapContents = false;
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (0, 100, 50, 100), p.Controls[0].Bounds, "CN1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "CN2");
			Assert.AreEqual (new Rectangle (0, -100, 100, 100), p.Controls[2].Bounds, "CN3");
			Assert.AreEqual (new Rectangle (0, -200, 100, 100), p.Controls[3].Bounds, "CN4");
		}

		[Test]
		public void BottomUpLayoutTest13 ()
		{
			// SetFlowBreak 1, 3
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			p.SetFlowBreak (p.Controls[0], true);
			p.SetFlowBreak (p.Controls[2], true);

			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[0].Bounds, "CO1");
			Assert.AreEqual (new Rectangle (100, 100, 100, 100), p.Controls[1].Bounds, "CO2");
			Assert.AreEqual (new Rectangle (100, 0, 100, 100), p.Controls[2].Bounds, "CO3");
			Assert.AreEqual (new Rectangle (200, 100, 100, 100), p.Controls[3].Bounds, "CO4");
		}

		[Test]
		public void BottomUpLayoutTest14 ()
		{
			// Margins
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (1, 3, 5, 2), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (7, 3, 12, 5), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (14, 7, 1, 3), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 50, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (1, 148, 50, 50), p.Controls[0].Bounds, "CP1");
			Assert.AreEqual (new Rectangle (7, 90, 50, 50), p.Controls[1].Bounds, "CP2");
			Assert.AreEqual (new Rectangle (14, 34, 50, 50), p.Controls[2].Bounds, "CP3");
			Assert.AreEqual (new Rectangle (73, 146, 50, 50), p.Controls[3].Bounds, "CP4");
		}

		[Test]
		public void BottomUpLayoutTest15 ()
		{
			// Margins and Different Sizes
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (25, 45, false, DockStyle.None, new Padding (6), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (60, 20, false, DockStyle.None, new Padding (9), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (15, 85, false, DockStyle.None, new Padding (2), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (50, 20, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Top));

			Assert.AreEqual (new Rectangle (6, 149, 25, 45), p.Controls[0].Bounds, "CQ1");
			Assert.AreEqual (new Rectangle (9, 114, 60, 20), p.Controls[1].Bounds, "CQ2");
			Assert.AreEqual (new Rectangle (2, 18, 15, 85), p.Controls[2].Bounds, "CQ3");
			Assert.AreEqual (new Rectangle (82, 176, 50, 20), p.Controls[3].Bounds, "CQ4");
		}

		[Test]
		public void BottomUpLayoutTest16 ()
		{
			// Random Complex Layout 1
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (25, 45, false, DockStyle.None, new Padding (6), AnchorStyles.Right | AnchorStyles.Top));
			p.Controls.Add (CreateButton (60, 20, false, DockStyle.Fill, new Padding (9), AnchorStyles.Bottom | AnchorStyles.Top));
			p.Controls.Add (CreateButton (15, 85, false, DockStyle.None, new Padding (2), AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (50, 20, false, DockStyle.None, new Padding (4), AnchorStyles.Left | AnchorStyles.Bottom));
			p.Controls.Add (CreateButton (13, 22, false, DockStyle.None, new Padding (12), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (73, 28, false, DockStyle.Left, new Padding (6), AnchorStyles.None));

			Assert.AreEqual (new Rectangle (6, 149, 25, 45), p.Controls[0].Bounds, "CR1");
			Assert.AreEqual (new Rectangle (9, 114, 19, 20), p.Controls[1].Bounds, "CR2");
			Assert.AreEqual (new Rectangle (2, 18, 15, 85), p.Controls[2].Bounds, "CR3");
			Assert.AreEqual (new Rectangle (41, 176, 50, 20), p.Controls[3].Bounds, "CR4");
			Assert.AreEqual (new Rectangle (49, 138, 61, 22), p.Controls[4].Bounds, "CR5");
			Assert.AreEqual (new Rectangle (43, 92, 73, 28), p.Controls[5].Bounds, "CR6");
		}

		[Test]
		public void BottomUpLayoutTest17 ()
		{
			// Random Complex Layout 2
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (12, 345, false, DockStyle.Right, new Padding (1, 2, 3, 4), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (9, 44, false, DockStyle.Left, new Padding (6, 3, 2, 7), AnchorStyles.Right | AnchorStyles.Top));
			p.Controls.Add (CreateButton (78, 14, false, DockStyle.None, new Padding (5, 1, 2, 4), AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right));
			p.Controls.Add (CreateButton (21, 64, false, DockStyle.Left, new Padding (3, 3, 3, 1), AnchorStyles.None));
			p.Controls.Add (CreateButton (14, 14, false, DockStyle.Fill, new Padding (11, 4, 6, 3), AnchorStyles.Left | AnchorStyles.Right));
			p.Controls.Add (CreateButton (132, 6, false, DockStyle.Fill, new Padding (5, 5, 4, 5), AnchorStyles.Left | AnchorStyles.Right));

			p.SetFlowBreak (p.Controls[0], true);
			p.SetFlowBreak (p.Controls[2], true);

			Assert.AreEqual (new Rectangle (1, -149, 12, 345), p.Controls[0].Bounds, "CS1");
			Assert.AreEqual (new Rectangle (22, 149, 9, 44), p.Controls[1].Bounds, "CS2");
			Assert.AreEqual (new Rectangle (21, 128, 10, 14), p.Controls[2].Bounds, "CS3");
			Assert.AreEqual (new Rectangle (36, 135, 21, 64), p.Controls[3].Bounds, "CS4");
			Assert.AreEqual (new Rectangle (44, 115, 10, 14), p.Controls[4].Bounds, "CS5");
			Assert.AreEqual (new Rectangle (38, 100, 18, 6), p.Controls[5].Bounds, "CS6");
		}

		[Test]
		public void BottomUpLayoutTest18 ()
		{
			// SetFlowBreak has no effect when WrapContents = false
			FlowLayoutPanel p = new FlowLayoutPanel ();
			p.Size = new Size (100, 200);
			p.WrapContents = false;
			p.FlowDirection = FlowDirection.BottomUp;

			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));
			p.Controls.Add (CreateButton (100, 100, false, DockStyle.None, new Padding (), AnchorStyles.Left | AnchorStyles.Top));

			p.SetFlowBreak (p.Controls[0], true);

			Assert.AreEqual (new Rectangle (0, 100, 100, 100), p.Controls[0].Bounds, "CT1");
			Assert.AreEqual (new Rectangle (0, 0, 100, 100), p.Controls[1].Bounds, "CT2");
		}
		#endregion

		private Button CreateButton (int width, int height, bool autosize, DockStyle dock, Padding margin, AnchorStyles anchor)
		{
			Button b = new Button ();
			b.Size = new Size(width, height);
			b.AutoSize = autosize;
			b.Anchor = anchor;
			b.Dock = dock;
			b.Margin = margin;
			
			return b;
		}

		#region PreferredSize
		[Test]
		public void PreferredSize ()
		{
			FlowLayoutPanel panel = new FlowLayoutPanel ();
			panel.Controls.AddRange (new Control [] { new PreferredSizeControl (), new PreferredSizeControl () });
			Assert.AreEqual (new Size (212, 106), panel.PreferredSize, "1");
			Assert.AreEqual (new Size (106, 212), panel.GetPreferredSize (new Size (150, 150)), "2");
			Assert.AreEqual (new Size (212, 106), panel.GetPreferredSize (new Size (1000, 1000)) , "3");
			Assert.AreEqual (new Size (212, 106), panel.GetPreferredSize (new Size (0, 0)), "4");
			Assert.AreEqual (new Size (106, 212), panel.GetPreferredSize (new Size (1, 1)), "5");
			Assert.AreEqual (new Size (212, 106), panel.GetPreferredSize (new Size (0, 150)), "6");
			Assert.AreEqual (new Size (106, 212), panel.GetPreferredSize (new Size (150, 0)), "7");
			panel.WrapContents = false;
			Assert.AreEqual (new Size (212, 106), panel.PreferredSize, "1, WrapContents");
			Assert.AreEqual (new Size (212, 106), panel.GetPreferredSize (new Size (150, 150)), "2, WrapContents");
			Assert.AreEqual (new Size (212, 106), panel.GetPreferredSize (new Size (1000, 1000)) , "3, WrapContents");
			Assert.AreEqual (new Size (212, 106), panel.GetPreferredSize (new Size (0, 0)), "4, WrapContents");
			Assert.AreEqual (new Size (212, 106), panel.GetPreferredSize (new Size (1, 1)), "5, WrapContents");
			Assert.AreEqual (new Size (212, 106), panel.GetPreferredSize (new Size (0, 150)), "6, WrapContents");
			Assert.AreEqual (new Size (212, 106), panel.GetPreferredSize (new Size (150, 0)), "7, WrapContents");
		}

		class PreferredSizeControl : Control
		{
			protected override Size DefaultSize {
				get {
					return new Size (100, 100);
				}
			}
		}
		#endregion
		
		[Test]
		public void Padding ()
		{
			Form f = new Form ();
			
			FlowLayoutPanel flp = new FlowLayoutPanel ();
			flp.Padding = new Padding (20);
			flp.Size = new Size (100, 100);

			Button b = new Button ();
			b.Size = new Size (50, 50);

			Button b2 = new Button ();
			b2.Size = new Size (50, 50);

			flp.Controls.Add (b);
			flp.Controls.Add (b2);

			f.Controls.Add (flp);
			
			Assert.AreEqual (new Rectangle (23, 23, 50, 50), b.Bounds, "A1");
			Assert.AreEqual (new Rectangle (23, 79, 50, 50), b2.Bounds, "A2");
		}
	}

	[TestFixture]
	public class FlowPanelTests_AutoSize: TestHelper
	{
		private Form f;
		protected override void SetUp ()
		{
			base.SetUp ();
			f = new Form ();
			f.AutoSize = false;
			f.ClientSize = new Size (100, 300);
			f.ShowInTaskbar = false;
			f.Show ();
		}

		protected override void TearDown ()
		{
			f.Dispose ();
			base.TearDown ();
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfLarger ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 10, 10);
			panel.Dock = DockStyle.None;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (192, panel.Width, "1"); // 2 * 90 + 4 * 3 margin
			Assert.AreEqual (25, panel.Height, "2");
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfLarger_DockBottom ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 10, 10);
			panel.Dock = DockStyle.Bottom;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (250, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (50, panel.Height, "3");
		}

		[Test]
		public void AutoSizeGrowOnly_DontResizeIfSmaller ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.None;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (100, panel.Width, "1");
			Assert.AreEqual (100, panel.Height, "2");
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfSmaller_DockTop ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.Top;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (0, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (25, panel.Height, "3");
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfSmaller_DockBottom ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.Bottom;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (275, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (25, panel.Height, "3");
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfSmaller_DockLeft ()
		{
			f.ClientSize = new Size (300, 100);

			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.Left;

			var c = new Label ();
			c.Size = new Size (25, 90);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (0, panel.Left, "1");
			Assert.AreEqual (f.ClientRectangle.Height, panel.Height, "2");
			Assert.AreEqual (31, panel.Width, "3"); // 25 + 2*3 margin
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfSmaller_DockRight ()
		{
			f.ClientSize = new Size (300, 100);

			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.Right;

			var c = new Label ();
			c.Size = new Size (25, 90);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (269, panel.Left, "1");
			Assert.AreEqual (f.ClientRectangle.Height, panel.Height, "2");
			Assert.AreEqual (31, panel.Width, "3"); // 25 + 2*3 margin
		}

		[Test]
		public void AutoSizeGrowAndShrink_ResizeIfSmaller ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 100, 100);
			panel.Dock = DockStyle.None;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (96, panel.Width, "1"); // 90 + 2*3 margin
			Assert.AreEqual (25, panel.Height, "2");
		}

		[Test]
		public void AutoSizeGrowAndShrink_ResizeIfSmaller_DockBottom ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 100, 100);
			panel.Dock = DockStyle.Bottom;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (275, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (25, panel.Height, "3");
		}

		[Test]
		public void NoAutoSize_DontResizeIfLarger ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = false;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 10, 10);
			panel.Dock = DockStyle.None;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (10, panel.Width, "1");
			Assert.AreEqual (10, panel.Height, "2");
		}

		[Test]
		public void NoAutoSize_DontResizeIfLarger_DockBottom ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = false;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 10, 10);
			panel.Dock = DockStyle.Bottom;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (290, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (10, panel.Height, "3");
		}

		[Test]
		public void NoAutoSize_DontResizeIfSmaller ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = false;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.None;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (100, panel.Width, "1");
			Assert.AreEqual (100, panel.Height, "2");
		}

		[Test]
		public void NoAutoSize_DontResizeIfSmaller_DockBottom ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = false;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.Bottom;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (200, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (100, panel.Height, "3");
		}
	}
}
