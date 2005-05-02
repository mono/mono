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

[TestFixture]
public class ControlTest
{
	[Test]
	public void PubPropTest()
    	{
            Control c = new Control();
            Assert.AreEqual(false, c.AccessibilityObject == null, "#1");
            Assert.AreEqual(false, c.AllowDrop , "#2");
            Assert.AreEqual(AnchorStyles.Top | AnchorStyles.Left, c.Anchor, "#4");
            Assert.AreEqual("Control", c.BackColor.Name , "#6");
            Assert.AreEqual(null, c.BindingContext, "#7");
            Assert.AreEqual(null, c.BackgroundImage, "#8");
            Assert.AreEqual(0, c.Bottom, "#10");
            //Assert.AreEqual (Rectangle (0,0, 100, 23) , c.Bounds , "#11");
            //Assert.AreEqual ( BoundsSpecified.X  , c.Bounds , "#11");
            Assert.AreEqual(false, c.CanFocus, "#12");
            Assert.AreEqual(true, c.CanSelect, "#13");
            Assert.AreEqual(false, c.Capture, "#14");
            Assert.AreEqual(true, c.CausesValidation, "#15");
            //Assert.AreEqual (false , c.ClientRectangle , "#16");
            //Assert.AreEqual (false , c.ClientSize , "#17");
            // Assert.AreEqual("Mono Project, Novell, Inc.", c.CompanyName, "#18");
            Assert.AreEqual(null, c.Container, "#19");
            Assert.AreEqual(false, c.ContainsFocus, "#20");
            Assert.AreEqual(null, c.ContextMenu, "#21");
            //Assert.AreEqual (Control+ControlCollection , c.Controls, "#22");
            Assert.AreEqual(true, c.Created, "#23");
            Assert.AreEqual(Cursors.Default, c.Cursor, "#24");
            Assert.AreEqual(false, c.DataBindings == null, "#25");
            //Assert.AreEqual(false, c.DefaultBackColor, "#25a");
            //Assert.AreEqual(false, c.DefaultForeColor == null, "#25b");
            //Assert.AreEqual(false, c.DefaultFont == null, "#25c");
            //Assert.AreEqual (false , c.DisplayRectangle , "#26");
            //<{X=0,Y=0,Width=100,Height=23}>
            Assert.AreEqual(false, c.Disposing, "#27");
            Assert.AreEqual(DockStyle.None, c.Dock, "#28");
            Assert.AreEqual(true, c.Enabled, "#29");
            Assert.AreEqual(false, c.Focused, "#31");
            //Assert.AreEqual(FontFamily.GenericSansSerif, c.Font, "#32");
            Assert.AreEqual(SystemColors.ControlText, c.ForeColor, "#33");
            //Assert.AreEqual (IWin32Window.Handle , c.Handle, "#34");
            Assert.AreEqual(false, c.HasChildren, "#35");
            Assert.AreEqual(0, c.Height, "#36");
            //Assert.AreEqual (false , c.ImeMode, "#41");
            Assert.AreEqual(false, c.InvokeRequired, "#42");
            Assert.AreEqual(false, c.IsAccessible, "#43");
            Assert.AreEqual(false, c.IsDisposed, "#44");
            Assert.AreEqual(true, c.IsHandleCreated, "#45");
            Assert.AreEqual(0, c.Left, "#46");
            Assert.AreEqual(Point.Empty, c.Location, "#47");
            //Assert.AreEqual(Point.Empty, c.ModifierKeys, "#47a");
            //Assert.AreEqual(Point.Empty, c.MousePosition, "#47b");
            //Assert.AreEqual(Point.Empty, c.MouseButtons, "#47c");
            Assert.AreEqual("", c.Name, "#48");
            Assert.AreEqual(null, c.Parent, "#49");
            //Assert.AreEqual("Novell Mono MWF", c.ProductName, "#52");
            Assert.AreEqual("1.1.4322.573", c.ProductVersion, "#53");
            Assert.AreEqual(false, c.RecreatingHandle, "#54");
            Assert.AreEqual(null, c.Region, "#55");
            Assert.AreEqual(0, c.Right, "#56");
            Assert.AreEqual(RightToLeft.No, c.RightToLeft, "#57");
            Assert.AreEqual(null, c.Site, "#58");
            //Assert.AreEqual (false , c.Size, "#59");
            //Assert.AreEqual(true , c.TabIndex , "#60");
            //true , 0
            Assert.AreEqual(true, c.TabStop, "#60a");
            Assert.AreEqual(null, c.Tag, "#61");
            Assert.AreEqual("", c.Text, "#62");
            Assert.AreEqual(0, c.Top, "#64");
            Assert.AreEqual(null, c.TopLevelControl, "#65");
            Assert.AreEqual(true, c.Visible, "#67");
            Assert.AreEqual(0, c.Width, "#68");
		
    	}
    
	[Test]
	public void PubMethodTest2()
    	{
     	   Control C1 = new Control();
     	   Control ctl = new Control();
     	   C1.Show() ;
           Assert.AreEqual(true , C1.Visible , "#69");
     	   Assert.AreEqual(false, C1.Contains(ctl) , "#70");
     	   Assert.AreEqual("System.Windows.Forms.Control", C1.ToString() , "#71");
     	
     	  //C1.Update ();
     	  //Assert.AreEqual(false, C1 , "#70");
     	  //Causes the control to redraw the invalidated regions within its client area.
     	
     	  C1.Anchor = AnchorStyles.Top;
     	  C1.SuspendLayout ();
     	  C1.Anchor = AnchorStyles.Left ;
     	  C1.ResumeLayout ();
     	  Assert.AreEqual(AnchorStyles.Left , C1.Anchor, "#73");
     	
     	  C1.SetBounds(10, 20, 30, 40) ;
     	  Assert.AreEqual( 20 ,C1.Bounds.Top , "#74a");
     	  Assert.AreEqual( 10 ,C1.Bounds.Left , "#74b");
     	  Assert.AreEqual( 30 ,C1.Bounds.Width , "#74c");
     	  Assert.AreEqual( 40 ,C1.Bounds.Hight , "#74d");
     	
     	  //C1.SendToBack() ;
     	  //Assert.AreEqual( false , C1.Bounds , "#75");
     	
	}

	[Test]
	public void PubMethodTest3()
    	{
     	   Control B = new Control();
     	   //Form frm = new Form.ControlNativeWindow() ;
     	   //B.SelectNextControl(frm)  ;
     	   //Assert.AreEqual(true, B.TabStop , "#75");
     	   //LayoutEventArgs l = LayoutEventArgs ();
     	   //Layout L1 = B.PerformLayout(l) ;
     	   //Anchor = AnchorStyles.Bottom
     	   B.SuspendLayout() ;
     	   B.Size = Size.Empty ;
     	   B.Location = Point.Empty;
     	   B.Anchor = AnchorStyles.Bottom;
     	   B.Dock = DockStyle.Left;
     	   B.ResumeLayout() ;
     	   //Assert.AreEqual(false, B.Size , "#75a");
     	   //Assert.AreEqual(false, B.Location , "#75b");
     	   Assert.AreEqual(AnchorStyles.Top | AnchorStyles.Left , B.Anchor , "#75c");
     	   Assert.AreEqual(DockStyle.None , B.Dock , "#75d");
     	
	}
    
	[Test]
	public void PubMethodTest4()
	{
           Control s1 = new Control();
           Control s2 = new Control();
           s1.Text = "abc";
           s2.Text = "abc";
           Assert.AreEqual(false, s1.Equals(s2), "#76");
           Assert.AreEqual(true, s1.Equals(s1), "#77");
	}
	[Test]
	public void PubMethodTest5()
    	{
           Control r1 = new Control();
           r1.Width = 40;
           r1.Height = 20;
           r1.Scale(2);
           Assert.AreEqual(80, r1.Width, "#78");
           Assert.AreEqual(40, r1.Height, "#79");
    	}
    
	[Test]
    	public void PubMethodTest6()
    	{
           Control r1 = new Control();
           r1.Text = "Hi" ;
           r1.ResetText();
           Assert.AreEqual("" , r1.Text , "#80");
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
    	public void PubMethodTest8()
    	{
    	   Label N = new Label();
	   N.Left = 10;
       	   N.Top  = 12;
	   N.Visible = true;
           Point p = new Point (10,10);
           Point p1 = N.PointToScreen(p) ;
           Point p2 = N.PointToClient (p1);
           Assert.AreEqual (p, p2, "#1 converting client->screen->client should not loose data");

    	}
    
     	[Test]
	public void ContainsTest ()
        {
            Control t = new Control ();
            Control s = new Control ();
            t.Controls.Add (s);

            Assert.AreEqual (true, t.Contains (s), "#1 should contain");
            Assert.AreEqual (false, s.Contains (t), "#2 should not contain");
            Assert.AreEqual (false, s.Contains (null), "#3 should not contain");
            Assert.AreEqual (false, t.Contains (new Control ()), "#4 should not contain");
        }

	[Test]
        public void CreateHandleTest ()
        {
            Control parent = null, child = null;
            try {
                parent = new Control ();
                parent.Visible = true;
                child = new Control ();
                parent.Controls.Add (child);

                Assert.IsFalse (parent.IsHandleCreated, "#1 handle should not be created while ctor");
                Assert.IsFalse (child.IsHandleCreated, "#2 handle should not be created while ctor");

                parent.CreateControl ();
                Assert.IsNotNull (parent.Handle, "#3 should create a handle");
                Assert.IsNotNull (child.Handle, "#4 should create a handle");
                Assert.IsTrue (parent.IsHandleCreated, "#5 should have created handle");
                Assert.IsTrue (child.IsHandleCreated, "#6 should have created handle");
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
                Assert.IsFalse (parent.IsHandleCreated, "#7 handle is not created while ctor");
                Assert.IsFalse (child.IsHandleCreated, "#8 handle is not created while ctor");
                Assert.IsNotNull (parent.Handle, "#9 should create a handle");
                Assert.IsTrue (parent.IsHandleCreated, "#10 should have created handle");
                Assert.IsTrue (child.IsHandleCreated, "#11 should have created handle");
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
                Assert.IsNotNull (g, "#1 should create a graphics object");
                g.DrawLine (p = new Pen (Color.Red), 10, 10, 20, 20);
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
        public void InvokeTest ()
        {
            Control c = null;
            try {
                c = new Control ();
                IAsyncResult result;
                try {
                    result = c.BeginInvoke (new TestDelegate (delegate_call));
                    c.EndInvoke (result);
                    Assert.Fail ("#1 should not invoke without window handle");
                } catch (InvalidOperationException) { }
                c.CreateControl ();
                result = c.BeginInvoke (new TestDelegate (delegate_call));
                c.EndInvoke (result);

                Assert.IsTrue (delegateCalled, "#1 value should have been set to true");
            } finally {
                if (c != null)
                    c.Dispose ();
            }
        }

        public void delegate_call ()
        {
            delegateCalled = true;
        }

        [Test]
        public void FindFormTest ()
        {
            Form f = new Form ();
            f.Name = "form";
            Control c = null;
            try {
                f.Controls.Add (c = new Control ());
                Assert.AreEqual (f.Name, c.FindForm ().Name, "#1 should find the parent form");

                f.Controls.Remove (c);

                GroupBox g = new GroupBox ();
                g.Name = "box";
                f.Controls.Add (g);
                g.Controls.Add (c);

                Assert.AreEqual (f.Name, f.FindForm ().Name, "#2 still should find the form");

                g.Controls.Remove (c);
                Assert.IsNull(c.FindForm (), "#3 should be null");

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
                
                Assert.IsTrue (c.CanFocus, "#1 button should be able to be focused");
                Assert.IsFalse (c.Focused, "#2 button should not be focussed initially");
                c.Focus ();
                Assert.IsTrue (c.Focused, "#3 button should be focussed after Focus ()");
                d.Focus ();
                Assert.IsFalse (d.Focused, "#4 invisible button should not be focussed");

                d.Visible = true;
                d.Focus ();
                Assert.IsTrue (d.Focused, "#5 button should be focussed after Focus () & visible");
                Assert.IsFalse (c.Focused, "#6 button should lost focus");

                c.Enabled = false;
                Assert.IsFalse (c.Focused, "#7 disabled button should not be focused");
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
        public void FromChildHandleTest ()
        {
            // FIXME : how to make a control to have more than one handle?
            Control c = null;
            try {
                c = new Control ();
                c.Name = "hello";
                IntPtr handle = c.Handle;
                Assert.AreEqual (c.Name, Control.FromChildHandle (handle).Name, "#1 handle should be able to relate to control");

                handle = IntPtr.Zero;
                Assert.IsNull (Control.FromChildHandle (handle), "#2 should return null");
            } finally {
                if (c != null)
                    c.Dispose ();
            }
        }

        [Test]
        public void FromHandleTest ()
        {
            Control c = null;
            try {
                c = new Control ();
                c.Name = "hello";
                IntPtr handle = c.Handle;
                Assert.AreEqual (c.Name, Control.FromHandle (handle).Name, "#1 handle should be able to relate to control");

                handle = IntPtr.Zero;
                Assert.IsNull (Control.FromHandle (handle), "#2 should return null");
            } finally {
                if (c != null)
                    c.Dispose ();
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
                Assert.AreEqual (d.Name, l.Name, "#1 should return the cild");
                Assert.IsFalse (e.Name == l.Name, "#2 e is not child of c");

                l = c.GetChildAtPoint (new Point (57, 57));
                Assert.IsNull (l, "#3 no control at 55");

                l = c.GetChildAtPoint (new Point (10, 10));
                Assert.AreEqual (d.Name, l.Name, "#4 should return the child even if it falls on border");

                /* net 2.0 overload
#if NET_2_0
                c.Controls.Add (e);
                e.Visible = false;
                l = c.GetChildAtPoint (new Point (57, 57), GetChildAtPointSkip.Invisible);
                Assert.IsNull (l, "#4 should ignore of type invisible");

                e.Visible = true;
                l = c.GetChildAtPoint (new Point (57, 57), GetChildAtPointSkip.Invisible);
                Assert.AreSame (e.Name, l.Name, "#4 should show visible controls");
#endif // NET_2_0                 
                 */
            } finally {
                if (c != null)
                    c.Dispose ();
                if (d != null)
                    d.Dispose ();
            }
        }

   }
