//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Runtime.Remoting;

using NUnit.Framework;

namespace MWF.MonoTest
{
	[TestFixture]
	public class ControlTest
	{
		internal static void TestAccessibility(Control c, string Default, string Description, string Name, AccessibleRole Role) {
			Assert.AreEqual(false, c.AccessibilityObject == null, "Acc1");
			Assert.AreEqual(Default, c.AccessibleDefaultActionDescription, "Acc2");
			Assert.AreEqual(Description, c.AccessibleDescription, "Acc3");
			Assert.AreEqual(Name, c.AccessibleName, "Acc4");
			Assert.AreEqual(Role, c.AccessibleRole, "Acc5");
		}

		[Test]
		public void PubPropTest()
    		{
			Control c = new Control();

			TestAccessibility(c, null, null, null, AccessibleRole.Default);

			// A
			Assert.AreEqual(false, c.AllowDrop , "A1");
			Assert.AreEqual(AnchorStyles.Top | AnchorStyles.Left, c.Anchor, "A2");

			// B
			Assert.AreEqual("Control", c.BackColor.Name , "B1");
			Assert.AreEqual(null, c.BackgroundImage, "B2");
			Assert.AreEqual(null, c.BindingContext, "B3");
			Assert.AreEqual(0, c.Bottom, "B4");
			Assert.AreEqual (new Rectangle (0,0, 0, 0) , c.Bounds , "B5");

			// C
			Assert.AreEqual(false, c.CanFocus, "C1");
			Assert.AreEqual(true, c.CanSelect, "C2");
			Assert.AreEqual(false, c.Capture, "C3");
			Assert.AreEqual(true, c.CausesValidation, "C4");
			Assert.AreEqual (new Rectangle(0, 0, 0, 0) , c.ClientRectangle , "C5");
			Assert.AreEqual (new Size(0, 0), c.ClientSize , "C6");

			string name = c.CompanyName;
			if (!name.Equals("Mono Project, Novell, Inc.") && !name.Equals("Microsoft Corporation")) {
				Assert.Fail("CompanyName property does not match any accepted value - C7");
			}
			Assert.AreEqual(null, c.Container, "C8");
			Assert.AreEqual(false, c.ContainsFocus, "C9");
			Assert.AreEqual(null, c.ContextMenu, "C10");
			Assert.AreEqual(0, c.Controls.Count, "C11");
			Assert.AreEqual(true, c.Created, "C12");
			Assert.AreEqual(Cursors.Default, c.Cursor, "C13");

			// D
			Assert.AreEqual(false, c.DataBindings == null, "D1");
			Assert.AreEqual("Control", Control.DefaultBackColor.Name, "D2");
			Assert.AreEqual("ControlText", Control.DefaultForeColor.Name, "D3");
			Assert.AreEqual(FontStyle.Regular, Control.DefaultFont.Style, "D4");
			Assert.AreEqual (new Rectangle(0, 0, 0, 0), c.DisplayRectangle , "D5");
			Assert.AreEqual(false, c.Disposing, "D6");
			Assert.AreEqual(DockStyle.None, c.Dock, "D7");

			// E
			Assert.AreEqual(true, c.Enabled, "E1");

			// F
			Assert.AreEqual(false, c.Focused, "F1");
			Assert.AreEqual(FontStyle.Regular, c.Font.Style, "F2");
			Assert.AreEqual(SystemColors.ControlText, c.ForeColor, "F3");

			// G

			// H
			Assert.AreEqual (((IWin32Window)c).Handle, c.Handle, "H1");
			Assert.AreEqual(false, c.HasChildren, "H2");
			Assert.AreEqual(0, c.Height, "H3");

			// I
			Assert.AreEqual (ImeMode.NoControl, c.ImeMode, "I1");
			Assert.AreEqual(false, c.InvokeRequired, "I2");
			Assert.AreEqual(false, c.IsAccessible, "I3");
			Assert.AreEqual(false, c.IsDisposed, "I4");
			Assert.AreEqual(true, c.IsHandleCreated, "I5");

			// J

			// K

			// L
			Assert.AreEqual(0, c.Left, "L1");
			Assert.AreEqual(Point.Empty, c.Location, "L2");

			// M
			Assert.AreEqual(Keys.None, Control.ModifierKeys, "M1");
			Assert.AreEqual(false, Control.MousePosition.IsEmpty, "M2");
			Assert.AreEqual(MouseButtons.None, Control.MouseButtons, "M3");

			// N
			Assert.AreEqual("", c.Name, "N1");

			// O

			// P
			Assert.AreEqual(null, c.Parent, "P1");
			name = c.ProductName;
			if (!name.Equals("Novell Mono MWF") && !name.Equals("Microsoft (R) .NET Framework"))
				Assert.Fail("ProductName property does not match any accepted value - P2");

			name = c.ProductVersion;
			if (!name.Equals("1.1.4322.2032")) {
				Assert.Fail("This test is being run against the wrong framework version.\nExpected is Net 1.1sp1. - P3");
			}

			// R
			Assert.AreEqual(false, c.RecreatingHandle, "R1");
			Assert.AreEqual(null, c.Region, "R2");
			Assert.AreEqual(0, c.Right, "R3");
			Assert.AreEqual(RightToLeft.No, c.RightToLeft, "R4");

			// S
			Assert.AreEqual(null, c.Site, "S1");
			Assert.AreEqual (new Size(0, 0), c.Size, "S2");

			// T
			Assert.AreEqual(0, c.TabIndex , "T1");
			Assert.AreEqual(true, c.TabStop, "T2");
			Assert.AreEqual(null, c.Tag, "T3");
			Assert.AreEqual("", c.Text, "T4");
			Assert.AreEqual(0, c.Top, "T5");
			Assert.AreEqual(null, c.TopLevelControl, "T6");

			// U

			// V
			Assert.AreEqual(true, c.Visible, "V1");

			// W
			Assert.AreEqual(0, c.Width, "W1");

			// XYZ
		}

		[Test]
		public void RelationTest() {
			Control c1;
			Control c2;

			c1 = new Control();
			c2 = new Control();

			Assert.AreEqual(true , c1.Visible , "Rel1");
			Assert.AreEqual(false, c1.Contains(c2) , "Rel2");
			Assert.AreEqual("System.Windows.Forms.Control", c1.ToString() , "Rel3");

			c1.Controls.Add(c2);
			Assert.AreEqual(true , c2.Visible , "Rel4");
			Assert.AreEqual(true, c1.Contains(c2) , "Rel5");

			c1.Anchor = AnchorStyles.Top;
			c1.SuspendLayout ();
			c1.Anchor = AnchorStyles.Left ;
			c1.ResumeLayout ();
			Assert.AreEqual(AnchorStyles.Left , c1.Anchor, "Rel6");

			c1.SetBounds(10, 20, 30, 40) ;
			Assert.AreEqual(new Rectangle(10, 20, 30, 40), c1.Bounds, "Rel7");

			Assert.AreEqual(c1, c2.Parent, "Rel8");
		}

		private string TestControl(Control container, Control start, bool forward) {
			Control	ctl;

			ctl = container.GetNextControl(start, forward);

			if (ctl == null) {
				return null;
			}

			return ctl.Text;
		}

		[Test]
		public void TabOrder() {
			Form		form;
			Control		active;

			Label		label1 = new Label();		// To test non-tabstop items as well
			Label		label2 = new Label();

			GroupBox	group1 = new GroupBox();
			GroupBox	group2 = new GroupBox();
			GroupBox	group3 = new GroupBox();

			TextBox		text1 = new TextBox();

			RadioButton	radio11 = new RadioButton();
			RadioButton	radio12 = new RadioButton();
			RadioButton	radio13 = new RadioButton();
			RadioButton	radio14 = new RadioButton();
			RadioButton	radio21 = new RadioButton();
			RadioButton	radio22 = new RadioButton();
			RadioButton	radio23 = new RadioButton();
			RadioButton	radio24 = new RadioButton();
			RadioButton	radio31 = new RadioButton();
			RadioButton	radio32 = new RadioButton();
			RadioButton	radio33 = new RadioButton();
			RadioButton	radio34 = new RadioButton();

			form = new Form();

			form.ClientSize = new System.Drawing.Size (520, 520);
			Assert.AreEqual(new Size(520, 520), form.ClientSize, "Tab1");

			form.Text = "SWF Taborder Test App Form";
			Assert.AreEqual("SWF Taborder Test App Form", form.Text, "Tab2");

			label1.Location = new Point(10, 10);
			Assert.AreEqual(new Point(10, 10), label1.Location, "Tab3");
			label1.Text = "Label1";
			form.Controls.Add(label1);

			label2.Location = new Point(200, 10);
			label2.Text = "Label2";
			form.Controls.Add(label2);

			group1.Text = "Group1";
			group2.Text = "Group2";
			group3.Text = "Group3";

			group1.Size = new Size(200, 400);
			group2.Size = new Size(200, 400);
			group3.Size = new Size(180, 180);
			Assert.AreEqual(new Size(180, 180), group3.Size, "Tab4");

			group1.Location = new Point(10, 40);
			group2.Location = new Point(220, 40);
			group3.Location = new Point(10, 210);

			group1.TabIndex = 30;
			Assert.AreEqual(30, group1.TabIndex, "Tab5");
			group1.TabStop = true;

			// Don't assign, test automatic assignment
			//group2.TabIndex = 0;
			group2.TabStop = true;
			Assert.AreEqual(0, group2.TabIndex, "Tab6");

			group3.TabIndex = 35;
			group3.TabStop = true;

			// Test default tab index
			Assert.AreEqual(0, radio11.TabIndex, "Tab7");

			text1.Text = "Edit Control";

			radio11.Text = "Radio 1-1 [Tab1]";
			radio12.Text = "Radio 1-2 [Tab2]";
			radio13.Text = "Radio 1-3 [Tab3]";
			radio14.Text = "Radio 1-4 [Tab4]";

			radio21.Text = "Radio 2-1 [Tab4]";
			radio22.Text = "Radio 2-2 [Tab3]";
			radio23.Text = "Radio 2-3 [Tab2]";
			radio24.Text = "Radio 2-4 [Tab1]";

			radio31.Text = "Radio 3-1 [Tab1]";
			radio32.Text = "Radio 3-2 [Tab3]";
			radio33.Text = "Radio 3-3 [Tab2]";
			radio34.Text = "Radio 3-4 [Tab4]";

			// We don't assign TabIndex for radio1X; test automatic assignment
			text1.TabStop = true;
			radio11.TabStop = true;

			radio21.TabIndex = 4;
			radio22.TabIndex = 3;
			radio23.TabIndex = 2;
			radio24.TabIndex = 1;
			radio24.TabStop = true;

			radio31.TabIndex = 11;
			radio31.TabStop = true;
			radio32.TabIndex = 13;
			radio33.TabIndex = 12;
			radio34.TabIndex = 14;

			text1.Location = new Point(10, 100);

			radio11.Location = new Point(10, 20);
			radio12.Location = new Point(10, 40);
			radio13.Location = new Point(10, 60);
			radio14.Location = new Point(10, 80);

			radio21.Location = new Point(10, 20);
			radio22.Location = new Point(10, 40);
			radio23.Location = new Point(10, 60);
			radio24.Location = new Point(10, 80);

			radio31.Location = new Point(10, 20);
			radio32.Location = new Point(10, 40);
			radio33.Location = new Point(10, 60);
			radio34.Location = new Point(10, 80);

			text1.Size = new Size(150, text1.PreferredHeight);

			radio11.Size = new Size(150, 20);
			radio12.Size = new Size(150, 20);
			radio13.Size = new Size(150, 20);
			radio14.Size = new Size(150, 20);

			radio21.Size = new Size(150, 20);
			radio22.Size = new Size(150, 20);
			radio23.Size = new Size(150, 20);
			radio24.Size = new Size(150, 20);

			radio31.Size = new Size(150, 20);
			radio32.Size = new Size(150, 20);
			radio33.Size = new Size(150, 20);
			radio34.Size = new Size(150, 20);

			group1.Controls.Add(text1);

			group1.Controls.Add(radio11);
			group1.Controls.Add(radio12);
			group1.Controls.Add(radio13);
			group1.Controls.Add(radio14);

			group2.Controls.Add(radio21);
			group2.Controls.Add(radio22);
			group2.Controls.Add(radio23);
			group2.Controls.Add(radio24);

			group3.Controls.Add(radio31);
			group3.Controls.Add(radio32);
			group3.Controls.Add(radio33);
			group3.Controls.Add(radio34);

			form.Controls.Add(group1);
			form.Controls.Add(group2);
			group2.Controls.Add(group3);

			// Perform some tests, the TabIndex stuff below will alter the outcome
			Assert.AreEqual(null, TestControl(group2, radio34, true), "Tab8");
			Assert.AreEqual(31, group2.TabIndex, "Tab9");

			// Does the taborder of containers and non-selectable things change behaviour?
			label1.TabIndex = 5;
			label2.TabIndex = 4;
			group1.TabIndex = 3;
			group2.TabIndex = 1;

			// Start verification
			Assert.AreEqual(null, TestControl(group2, radio34, true), "Tab10");
			Assert.AreEqual(radio24.Text, TestControl(group2, group2, true), "Tab11");
			Assert.AreEqual(radio31.Text, TestControl(group2, group3, true), "Tab12");
			Assert.AreEqual(null, TestControl(group1, radio14, true), "Tab13");
			Assert.AreEqual(radio23.Text, TestControl(group2, radio24, true), "Tab14");
			Assert.AreEqual(group3.Text, TestControl(group2, radio21, true), "Tab15");
			Assert.AreEqual(radio13.Text, TestControl(form, radio12, true), "Tab16");
			Assert.AreEqual(label2.Text, TestControl(form, radio14, true), "Tab17");
			Assert.AreEqual(group1.Text, TestControl(form, radio34, true), "Tab18");
			Assert.AreEqual(radio23.Text, TestControl(group2, radio24, true), "Tab19");

			// Sanity checks
			Assert.AreEqual(null, TestControl(radio11, radio21, true), "Tab20");
			Assert.AreEqual(text1.Text, TestControl(group1, radio21, true), "Tab21");

			Assert.AreEqual(radio14.Text, TestControl(form, label2, false), "Tab22");
			Assert.AreEqual(radio21.Text, TestControl(group2, group3, false), "Tab23");

			Assert.AreEqual(4, radio21.TabIndex, "Tab24");
			Assert.AreEqual(1, radio11.TabIndex, "Tab25");
			Assert.AreEqual(3, radio13.TabIndex, "Tab26");
			Assert.AreEqual(35, group3.TabIndex, "Tab27");
			Assert.AreEqual(1, group2.TabIndex, "Tab28");

			Assert.AreEqual(label1.Text, TestControl(form, form, false), "Tab29");
			Assert.AreEqual(radio14.Text, TestControl(group1, group1, false), "Tab30");
			Assert.AreEqual(radio34.Text, TestControl(group3, group3, false), "Tab31");

			Assert.AreEqual(null, TestControl(label1, label1, false), "Tab31");
			Assert.AreEqual(null, TestControl(radio11, radio21, false), "Tab32");
		}

		[Test]
		public void ScaleTest()
		{
			Control r1 = new Control();

			r1.Width = 40;
			r1.Height = 20;
			r1.Scale(2);
			Assert.AreEqual(80, r1.Width, "Scale1");
			Assert.AreEqual(40, r1.Height, "Scale2");
		}

		[Test]
		public void TextTest()
		{
			Control r1 = new Control();
			r1.Text = "Hi" ;
			Assert.AreEqual("Hi" , r1.Text , "Text1");

			r1.ResetText();
			Assert.AreEqual("" , r1.Text , "Text2");
		}

		[Test]
		public void PubMethodTest7()
		{
			Control r1 = new Control();
			r1.RightToLeft = RightToLeft.Yes ;
			r1.ResetRightToLeft() ;
			Assert.AreEqual(RightToLeft.No , r1.RightToLeft , "#81");
			r1.ImeMode = ImeMode.Off ;
			r1.ResetImeMode () ;
			Assert.AreEqual(ImeMode.NoControl , r1.ImeMode , "#82");
			r1.ForeColor= SystemColors.GrayText ;
			r1.ResetForeColor() ;
			Assert.AreEqual(SystemColors.ControlText , r1.ForeColor , "#83");
			//r1.Font = Font.FromHdc();
			r1.ResetFont () ;
			//Assert.AreEqual(FontFamily.GenericSansSerif , r1.Font , "#83");
			r1.Cursor = Cursors.Hand ;
			r1.ResetCursor () ;
			Assert.AreEqual(Cursors.Default , r1.Cursor , "#83");
			//r1.DataBindings = System.Windows.Forms.Binding ;
			//r1.ResetBindings() ;
			//Assert.AreEqual(ControlBindingsCollection , r1.DataBindings  , "#83");
			r1.BackColor = System.Drawing.Color.Black ;
			r1.ResetBackColor() ;
			Assert.AreEqual( SystemColors.Control , r1.BackColor  , "#84");
			r1.BackColor = System.Drawing.Color.Black ;
			r1.Refresh() ;
			Assert.AreEqual( null , r1.Region , "#85");
			Rectangle M = new Rectangle(10, 20, 30 ,40);
			r1.RectangleToScreen(M) ;
			Assert.AreEqual( null , r1.Region , "#86");

		}

		[Test]
		public void ScreenClientCoords()
		{
			Label l;
			Point p1;
			Point p2;
			Point p3;

			l = new Label();
			l.Left = 10;
			l.Top  = 12;
			l.Visible = true;
			p1 = new Point (10,10);
			p2 = l.PointToScreen(p1);
			p3 = l.PointToClient(p2);

			Assert.AreEqual (p1, p3, "SC1");
		}

		[Test]
		public void ContainsTest ()
		{
			Control t = new Control ();
			Control s = new Control ();

			t.Controls.Add (s);

			Assert.AreEqual (true, t.Contains (s), "Con1");
			Assert.AreEqual (false, s.Contains (t), "Con2");
			Assert.AreEqual (false, s.Contains (null), "Con3");
			Assert.AreEqual (false, t.Contains (new Control ()), "Con4");
		}

		[Test]
		public void CreateHandleTest ()
		{
			Control parent;
			Control child;

			parent = null;
			child = null;

			try {
				parent = new Control ();
				child = new Control ();

				parent.Visible = true;
				parent.Controls.Add (child);

				Assert.IsFalse (parent.IsHandleCreated, "CH1");
				Assert.IsFalse (child.IsHandleCreated, "CH2");

				parent.CreateControl ();
				Assert.IsNotNull (parent.Handle, "CH3");
				Assert.IsNotNull (child.Handle, "CH4");
				Assert.IsTrue (parent.IsHandleCreated, "CH5");
				Assert.IsTrue (child.IsHandleCreated, "CH6");
			} finally {
				if (parent != null)
					parent.Dispose ();
				if (child != null)
					child.Dispose ();
			}

			// Accessing Handle Property creates the handle
			try {
				parent = new Control ();
				parent.Visible = true;
				child = new Control ();
				parent.Controls.Add (child);
				Assert.IsFalse (parent.IsHandleCreated, "CH7");
				Assert.IsFalse (child.IsHandleCreated, "CH8");
				Assert.IsNotNull (parent.Handle, "CH9");
				Assert.IsTrue (parent.IsHandleCreated, "CH10");
				Assert.IsTrue (child.IsHandleCreated, "CH11");
			} finally {
				if (parent != null)
					parent.Dispose ();
				if (child != null)
					child.Dispose ();
			}
		}

		[Test]
		public void CreateGraphicsTest ()
		{
			Graphics g = null;
			Pen p = null;

			try {
				Control c = new Control ();
				c.SetBounds (0,0, 20, 20);
				g = c.CreateGraphics ();
				Assert.IsNotNull (g, "Graph1");
			} finally {
				if (p != null)
					p.Dispose ();
				if (g != null)
					g.Dispose ();
			}
		}

		bool delegateCalled = false;
		public delegate void TestDelegate ();

		[Test]
		[ExpectedException(typeof(System.InvalidOperationException))]
		public void InvokeException1 () {
			Control c = new Control ();
			IAsyncResult result;

			result = c.BeginInvoke (new TestDelegate (delegate_call));
			c.EndInvoke (result);
		}

		[Test]
		public void InvokeTest () {
			Control c = null;

			try {
				c = new Control ();
				IAsyncResult result;

				c.CreateControl ();
				result = c.BeginInvoke (new TestDelegate (delegate_call));
				c.EndInvoke (result);
				Assert.AreEqual (true, delegateCalled, "Invoke1");
			} finally {
				if (c != null)
					c.Dispose ();
			}
		}

		public void delegate_call () {
			delegateCalled = true;
		}

		[Test]
		public void FindFormTest () {
			Form f = new Form ();

			f.Name = "form";
			Control c = null;

			try {
				f.Controls.Add (c = new Control ());
				Assert.AreEqual (f.Name, c.FindForm ().Name, "Find1");

				f.Controls.Remove (c);

				GroupBox g = new GroupBox ();
				g.Name = "box";
				f.Controls.Add (g);
				g.Controls.Add (c);

				Assert.AreEqual (f.Name, f.FindForm ().Name, "Find2");

				g.Controls.Remove (c);
				Assert.IsNull(c.FindForm (), "Find3");

			} finally {
				if (c != null)
					c.Dispose ();
				if (f != null)
					f.Dispose ();
			}
		}

		[Test]
		public void FocusTest ()
		{
			Form f = null;
			Button c = null, d = null;

			try {
				f = new Form ();
				f.Visible = true;
				c = new Button ();
				c.Visible = true;
				f.Controls.Add (c);

				d = new Button ();
				d.Visible = false;
				f.Controls.Add (d);

				Assert.IsTrue (c.CanFocus, "Focus1");
				Assert.IsFalse (c.Focused, "Focus2");
				c.Focus ();
				Assert.IsTrue (c.Focused, "Focus3");
				d.Focus ();
				Assert.IsFalse (d.Focused, "Focus4");

				d.Visible = true;
				d.Focus ();
				Assert.IsTrue (d.Focused, "Focus5");
				Assert.IsFalse (c.Focused, "Focus6");

				c.Enabled = false;
				Assert.IsFalse (c.Focused, "Focus7");
			} finally {
				if (f != null)
					f.Dispose ();
				if (c != null)
					c.Dispose ();
				if (d != null)
					d.Dispose ();
			}
		}

		[Test]
		public void FromHandleTest ()
		{
			Control c1 = null;
			Control c2 = null;

			try {
				c1 = new Control ();
				c2 = new Control ();

				c1.Name = "parent";
				c2.Name = "child";
				c1.Controls.Add(c2);

				// Handle
				Assert.AreEqual (c1.Name, Control.FromHandle (c1.Handle).Name, "Handle1");
				Assert.IsNull (Control.FromHandle (IntPtr.Zero), "Handle2");

				// ChildHandle
				Assert.AreEqual (c1.Name, Control.FromChildHandle (c1.Handle).Name, "Handle3");
				Assert.IsNull (Control.FromChildHandle (IntPtr.Zero), "Handle4");


			} finally {
				if (c1 != null)
					c1.Dispose ();

				if (c2 != null)
					c2.Dispose ();
			}
		}

		[Test]
		public void GetChildAtPointTest ()
		{
			Control c = null, d = null, e = null;

			try {
				c = new Control ();
				c.Name = "c1";
				c.SetBounds (0, 0, 100, 100);

				d = new Control ();
				d.Name = "d1";
				d.SetBounds (10, 10, 40, 40);
				c.Controls.Add (d);

				e = new Control ();
				e.Name = "e1";
				e.SetBounds (55, 55, 10, 10);

				Control l = c.GetChildAtPoint (new Point (15, 15));
				Assert.AreEqual (d.Name, l.Name, "Child1");
				Assert.IsFalse (e.Name == l.Name, "Child2");

				l = c.GetChildAtPoint (new Point (57, 57));
				Assert.IsNull (l, "Child3");

				l = c.GetChildAtPoint (new Point (10, 10));
				Assert.AreEqual (d.Name, l.Name, "Child4");

				// GetChildAtPointSkip is not implemented and the following test is breaking for Net_2_0 profile
//				#if NET_2_0
//					c.Controls.Add (e);
//					e.Visible = false;
//					l = c.GetChildAtPoint (new Point (57, 57), GetChildAtPointSkip.Invisible);
//					Assert.IsNull (l, "Child5");

//					e.Visible = true;
//					l = c.GetChildAtPoint (new Point (57, 57), GetChildAtPointSkip.Invisible);
//					Assert.AreSame (e.Name, l.Name, "Child6");
//				#endif // NET_2_0                 
			} finally {
				if (c != null)
					c.Dispose ();
				if (d != null)
					d.Dispose ();
			}
		}

		
		public class LayoutTestControl : Control {
			public int LayoutCount;

			public LayoutTestControl () : base() {
				LayoutCount = 0;
			}

			protected override void OnLayout(LayoutEventArgs levent) {
				LayoutCount++;
				base.OnLayout (levent);
			}
		}

		[Test]
		public void LayoutTest() {
			LayoutTestControl c;

			c = new LayoutTestControl();

			c.SuspendLayout();
			c.SuspendLayout();
			c.SuspendLayout();
			c.SuspendLayout();

			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.SuspendLayout();
			c.PerformLayout();

			Assert.AreEqual(5, c.LayoutCount, "Layout Suspend/Resume locking does not bottom out at 0");
		}
	}
}
