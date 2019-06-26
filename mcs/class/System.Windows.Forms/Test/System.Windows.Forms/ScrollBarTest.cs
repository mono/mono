//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Hisham Mardam Bey (hisham.mardambey@gmail.com)
//      Ritvik Mayank (mritvik@novell.com)
//
//

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

using MonoTests.Helpers;

namespace MonoTests.System.Windows.Forms
{

public class MyScrollBar : HScrollBar
    {
	    private ArrayList results = new ArrayList ();
     public MyScrollBar () : base ()
	     {}
	     
		public Padding PublicDefaultMargin { get { return base.DefaultMargin; } }

	    protected override void OnBackColorChanged (EventArgs e)
	     {
		     results.Add ("OnBackColorChanged");
		     base.OnBackColorChanged (e);
	     }

	    protected override void OnBackgroundImageChanged (EventArgs e)
	     {
		     results.Add ("OnBackgroundImageChanged");
		     base.OnBackgroundImageChanged (e);
	     }

	    protected override void OnClick (EventArgs e)
	     {
		     results.Add ("OnClick");
		     base.OnClick (e);
	     }

	    protected override void OnDoubleClick (EventArgs e)
	     {
		     results.Add ("OnDoubleClick");
		     base.OnDoubleClick (e);
	     }

	    protected override void OnFontChanged (EventArgs e)
	     {
		     results.Add ("OnFontChanged");
		     base.OnFontChanged (e);
	     }

	    protected override void OnForeColorChanged (EventArgs e)
	     {
		     results.Add ("OnForeColorChanged");
		     base.OnForeColorChanged (e);
	     }

	    protected override void OnImeModeChanged (EventArgs e)
	     {
		     results.Add ("OnImeModeChanged");
		     base.OnImeModeChanged (e);
	     }

	    protected override void OnMouseDown (MouseEventArgs e)
	     {
		     results.Add ("OnMouseDown");
		     base.OnMouseDown (e);
	     }

	    protected override void OnMouseMove (MouseEventArgs e)
	     {
		     results.Add ("OnMouseMove");
		     base.OnMouseMove (e);
	     }

	    protected override void OnMouseEnter (EventArgs e)
	     {
		     results.Add ("OnMouseEnter");
		     base.OnMouseEnter (e);
	     }

	    protected override void OnMouseLeave (EventArgs e)
	     {
		     results.Add ("OnMouseLeave");
		     base.OnMouseLeave (e);
	     }

	    protected override void OnMouseHover (EventArgs e)
	     {
		     results.Add ("OnMouseHover");
		     base.OnMouseHover (e);
	     }

	    protected override void OnMouseUp (MouseEventArgs e)
	     {
		     results.Add ("OnMouseUp");
		     base.OnMouseUp (e);
	     }

	    protected override void OnHandleCreated (EventArgs e)
	     {
		     results.Add ("OnHandleCreated");
		     base.OnHandleCreated (e);
	     }

	    protected override void OnBindingContextChanged (EventArgs e)
	     {
		     results.Add ("OnBindingContextChanged");
		     base.OnBindingContextChanged (e);
	     }

	    protected override void OnInvalidated (InvalidateEventArgs e)
	     {
		     results.Add("OnInvalidated");
		     base.OnInvalidated (e);
	     }

	    protected override void OnResize (EventArgs e)
	     {
		     results.Add("OnResize");
		     base.OnResize (e);
	     }

	    protected override void OnSizeChanged (EventArgs e)
	     {
		     results.Add("OnSizeChanged");
		     base.OnSizeChanged (e);
	     }

	    protected override void OnLayout (LayoutEventArgs e)
	     {
		     results.Add("OnLayout");
		     base.OnLayout (e);
	     }

	    protected override void OnVisibleChanged (EventArgs e)
	     {
		     results.Add("OnVisibleChanged");
		     base.OnVisibleChanged (e);
	     }

	    protected override void OnScroll (ScrollEventArgs e)
	     {
		     results.Add("OnScroll");
		     base.OnScroll (e);
	     }

	    protected override void OnTextChanged (EventArgs e)
	     {
		     results.Add("OnTextChanged");
		     base.OnTextChanged (e);
	     }

	    protected override void OnValueChanged (EventArgs e)
	     {
		     results.Add("OnValueChanged");
		     base.OnValueChanged (e);
	     }

	    protected override void OnPaint (PaintEventArgs e)
	     {
		     results.Add("OnPaint");
		     base.OnPaint (e);
	     }

	    public ArrayList Results {
		    get {	return results; }
	    }

	    //public void MoveMouse ()
	    // {
	    //         Message m;

	    //         m = new Message ();

	    //         m.Msg = (int)WndMsg.WM_NCHITTEST;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x0;
	    //         m.LParam = (IntPtr)0x1c604ea;
	    //         this.WndProc(ref m);

	    //         m.Msg = (int)WndMsg.WM_SETCURSOR;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x100448;
	    //         m.LParam = (IntPtr)0x2000001;
	    //         this.WndProc(ref m);

	    //         m.Msg = (int)WndMsg.WM_MOUSEFIRST;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x0;
	    //         m.LParam = (IntPtr)0x14000b;
	    //         this.WndProc(ref m);

	    //         m.Msg = (int)WndMsg.WM_MOUSEHOVER;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x0;
	    //         m.LParam = (IntPtr)0x14000b;
	    //         this.WndProc(ref m);
	    // }

	    //public new void MouseClick()
	    // {

	    //         Message m;

	    //         m = new Message();

	    //         m.Msg = (int)WndMsg.WM_LBUTTONDOWN;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x01;
	    //         m.LParam = (IntPtr)0x9004f;
	    //         this.WndProc(ref m);

	    //         m = new Message();

	    //         m.Msg = (int)WndMsg.WM_LBUTTONUP;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x01;
	    //         m.LParam = (IntPtr)0x9004f;
	    //         this.WndProc(ref m);
	    // }

	    //public new void MouseDoubleClick ()
	    // {
	    //         this.MouseClick ();
	    //         this.MouseClick ();
	    // }
	    //public void MouseRightDown()
	    // {
	    //         Message m;

	    //         m = new Message();

	    //         m.Msg = (int)WndMsg.WM_RBUTTONDOWN;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x01;
	    //         m.LParam = (IntPtr)0x9004f;
	    //         this.WndProc(ref m);
	    // }

	    //public void MouseRightUp()
	    // {
	    //         Message m;

	    //         m = new Message();

	    //         m.Msg = (int)WndMsg.WM_RBUTTONUP;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x01;
	    //         m.LParam = (IntPtr)0x9004f;
	    //         this.WndProc(ref m);
	    // }

	    public void ScrollNow ()
	     {
		     Message m;

		     m = new Message ();

		     m.Msg = 8468;
		     m.HWnd = this.Handle;
		     m.WParam = (IntPtr)0x1;
		     m.LParam = (IntPtr)0x1a051a;
		     this.WndProc(ref m);

		     m.Msg = 233;
		     m.HWnd = this.Handle;
		     m.WParam = (IntPtr)0x1;
		     m.LParam = (IntPtr)0x12eb34;
		     this.WndProc(ref m);
	     }
    }
   [TestFixture]
   public class ScrollBarTest : TestHelper
    {
	    [Test]
	    public void PubPropTest ()
	       {
		       MyScrollBar myscrlbar = new MyScrollBar ();

		       // B
		       myscrlbar.BackColor = Color.Red;
		       Assert.AreEqual (255, myscrlbar.BackColor.R, "B2");
		       myscrlbar.BackgroundImage = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/System.Windows.Forms/bitmaps/a.png"));
		       Assert.AreEqual (16, myscrlbar.BackgroundImage.Height, "B3");

		       // F
		       Assert.AreEqual ("ControlText", myscrlbar.ForeColor.Name, "F1");

		       // I
		       //Assert.AreEqual (ImeMode.Disable, myscrlbar.ImeMode, "I1");

		       // L
		       Assert.AreEqual (10, myscrlbar.LargeChange, "L1");

		       // M
		       Assert.AreEqual (100, myscrlbar.Maximum, "M1");
		       Assert.AreEqual (0, myscrlbar.Minimum, "M2");
		       myscrlbar.Maximum = 300;
		       myscrlbar.Minimum = 100;
		       Assert.AreEqual (300, myscrlbar.Maximum, "M3");
		       Assert.AreEqual (100, myscrlbar.Minimum, "M4");

		       // S
		       Assert.AreEqual (null, myscrlbar.Site, "S1");
		       Assert.AreEqual (1, myscrlbar.SmallChange, "S2");
		       myscrlbar.SmallChange = 10;
		       Assert.AreEqual (10, myscrlbar.SmallChange, "S3");

		       // T
		       Assert.AreEqual (false, myscrlbar.TabStop, "T1");
		       myscrlbar.TabStop = true;
		       Assert.AreEqual (true, myscrlbar.TabStop, "T2");
		       Assert.AreEqual ("", myscrlbar.Text, "T3");
		       myscrlbar.Text = "MONO SCROLLBAR";
		       Assert.AreEqual ("MONO SCROLLBAR", myscrlbar.Text, "T4");

		       // V
		       Assert.AreEqual (100, myscrlbar.Value, "V1");
		       myscrlbar.Value = 150;
		       Assert.AreEqual (150, myscrlbar.Value, "V2");
	       }

	    [Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
	   public void ExceptionValueTest ()
	       {
		       MyScrollBar myscrlbar = new MyScrollBar ();
		       myscrlbar.Minimum = 10;
		       myscrlbar.Maximum = 20;
		       myscrlbar.Value = 9;
		       myscrlbar.Value = 21;
	       }

	    [Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
	   public void ExceptionSmallChangeTest ()
	       {
		       MyScrollBar myscrlbar = new MyScrollBar ();
		       myscrlbar.SmallChange = -1;
	       }

	    [Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
	   public void ExceptionLargeChangeTest ()
	       {
		       MyScrollBar myscrlbar = new MyScrollBar ();
		       myscrlbar.LargeChange = -1;
	       }

	    [Test]
	    public void PubMethodTest ()
	       {
		       MyScrollBar myscrlbar = new MyScrollBar ();
		       myscrlbar.Text = "New HScrollBar";
		       Assert.AreEqual ("MonoTests.System.Windows.Forms.MyScrollBar, Minimum: 0, Maximum: 100, Value: 0",
					myscrlbar.ToString (), "T5");
	       }

	   [Test]
	   public void DefaultMarginTest ()
	   {
		   MyScrollBar s = new MyScrollBar ();
		   Assert.AreEqual (new Padding (0), s.PublicDefaultMargin, "A1");
	   }
	   
		[Test]
		public void GetScaledBoundsTest ()
		{
			ScaleScrollBar c = new ScaleScrollBar ();
			
			Rectangle r = new Rectangle (10, 20, 30, 40);

			Assert.AreEqual (new Rectangle (20, 10, 60, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.All), "A1");
			Assert.AreEqual (new Rectangle (20, 10, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Location), "A2");
			Assert.AreEqual (new Rectangle (10, 20, 60, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Size), "A3");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Height), "A4");
			Assert.AreEqual (new Rectangle (20, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.X), "A5");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.None), "A6");
		}
		
		private class ScaleScrollBar : ScrollBar
		{
			public Rectangle PublicGetScaledBounds (Rectangle bounds, SizeF factor, BoundsSpecified specified)
			{
				return base.GetScaledBounds (bounds, factor, specified);
			}
		}

		[Test]
		public void HScrollGetScaledBoundsTest ()
		{
			HScaleScrollBar c = new HScaleScrollBar ();

			Rectangle r = new Rectangle (10, 20, 30, 40);

			Assert.AreEqual (new Rectangle (20, 10, 60, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.All), "A1");
			Assert.AreEqual (new Rectangle (20, 10, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Location), "A2");
			Assert.AreEqual (new Rectangle (10, 20, 60, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Size), "A3");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Height), "A4");
			Assert.AreEqual (new Rectangle (20, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.X), "A5");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.None), "A6");
		}

		private class HScaleScrollBar : HScrollBar
		{
			public Rectangle PublicGetScaledBounds (Rectangle bounds, SizeF factor, BoundsSpecified specified)
			{
				return base.GetScaledBounds (bounds, factor, specified);
			}
		}

		[Test]
		public void VScrollGetScaledBoundsTest ()
		{
			VScaleScrollBar c = new VScaleScrollBar ();

			Rectangle r = new Rectangle (10, 20, 30, 40);

			Assert.AreEqual (new Rectangle (20, 10, 30, 20), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.All), "A1");
			Assert.AreEqual (new Rectangle (20, 10, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Location), "A2");
			Assert.AreEqual (new Rectangle (10, 20, 30, 20), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Size), "A3");
			Assert.AreEqual (new Rectangle (10, 20, 30, 20), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Height), "A4");
			Assert.AreEqual (new Rectangle (20, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.X), "A5");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.None), "A6");
		}

		private class VScaleScrollBar : VScrollBar
		{
			public Rectangle PublicGetScaledBounds (Rectangle bounds, SizeF factor, BoundsSpecified specified)
			{
				return base.GetScaledBounds (bounds, factor, specified);
			}
		}
		
		[Test]
		public void MaximumValueTest ()
		{
			ScrollBar s = new VScrollBar ();

			s.LargeChange = 0;
			s.Maximum = 100;
			s.Value = 20;
			s.Maximum = 0;

			Assert.AreEqual (0, s.LargeChange, "A1");
			Assert.AreEqual (0, s.Maximum, "A2");
			Assert.AreEqual (0, s.Value, "A3");
		}

		[Test]
		public void LargeSmallerThanSmallChange ()
		{
			ScrollBar s = new VScrollBar ();

			s.LargeChange = 0;

			Assert.AreEqual (0, s.LargeChange, "A1");
			Assert.AreEqual (0, s.SmallChange, "A2");
			
			s.SmallChange = 10;

			Assert.AreEqual (0, s.LargeChange, "A3");
			Assert.AreEqual (0, s.SmallChange, "A4");
			
			s.LargeChange = 15;

			Assert.AreEqual (15, s.LargeChange, "A5");
			Assert.AreEqual (10, s.SmallChange, "A6");
			
			s.LargeChange = 5;

			Assert.AreEqual (5, s.LargeChange, "A7");
			Assert.AreEqual (5, s.SmallChange, "A8");
		}
		
		[Test]
		public void CalculateLargeChange ()
		{
			ScrollBar s = new HScrollBar ();

			s.Minimum = -50;
			s.Maximum = 50;
			s.LargeChange = 1000;

			Assert.AreEqual (101, s.LargeChange, "A1");

			s.Maximum = 200;
			s.Minimum = 199;
			s.LargeChange = 1000;

			Assert.AreEqual (2, s.LargeChange, "A2");

			s.Minimum = 200;
			s.LargeChange = 1000;

			Assert.AreEqual (1, s.LargeChange, "A3");
		}
    }

   [TestFixture]
   public class ScrollBarEventTest : TestHelper
    {
	    static bool eventhandled = false;
	    public void ScrollBar_EventHandler (object sender,EventArgs e)
	     {
		     eventhandled = true;
	     }

	    public void ScrollBarMouse_EventHandler (object sender,MouseEventArgs e)
	     {
		     eventhandled = true;
	     }

	    public void ScrollBarScroll_EventHandler (object sender,ScrollEventArgs e)
	     {
		     eventhandled = true;
	     }

	    public void ScrollBarPaint_EventHandler (object sender,PaintEventArgs e)
	     {
		     eventhandled = true;
	     }

	    [Test]
	    public void BackColorChangedTest ()
	       {
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       ScrollBar myHscrlbar = new HScrollBar ();
		       myform.Controls.Add (myHscrlbar);
		       myHscrlbar.BackColorChanged += new EventHandler (ScrollBar_EventHandler);
		       myHscrlbar.BackColor = Color.Red;
		       Assert.AreEqual (true, eventhandled, "B4");
		       eventhandled = false;
		       myform.Dispose ();
	       }

	    [Test]
	    public void BackgroundImageChangedTest ()
	       {
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       ScrollBar myHscrlbar = new HScrollBar ();
		       myform.Controls.Add (myHscrlbar);
		       myHscrlbar.BackgroundImageChanged += new EventHandler (ScrollBar_EventHandler);
		       myHscrlbar.BackgroundImage = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/System.Windows.Forms/bitmaps/a.png"));
		       Assert.AreEqual (true, eventhandled, "B5");
		       eventhandled = false;
		       myform.Dispose ();
	       }

	    [Test]
	    public void FontChangedTest ()
	       {
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       ScrollBar myHscrlbar = new HScrollBar ();
		       myform.Controls.Add (myHscrlbar);
		       myHscrlbar.Font = new Font (FontFamily.GenericMonospace, 10);
		       myHscrlbar.FontChanged += new EventHandler (ScrollBar_EventHandler);
		       FontDialog myFontDialog = new FontDialog();
		       myHscrlbar.Font = myFontDialog.Font;
		       Assert.AreEqual (true, eventhandled, "F2");
		       eventhandled = false;
		       myform.Dispose ();
	       }

	    [Test]
	    public void ForeColorChangedTest ()
	       {
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       ScrollBar myHscrlbar = new HScrollBar ();
		       myform.Controls.Add (myHscrlbar);
		       myHscrlbar.ForeColorChanged += new EventHandler (ScrollBar_EventHandler);
		       myHscrlbar.ForeColor = Color.Azure;
		       Assert.AreEqual (true, eventhandled, "F3");
		       eventhandled = false;
		       myform.Dispose ();
	       }

	    //[Test]
	    //public void MouseDownTest ()
	    //   {
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar myHscrlbar = new MyScrollBar ();
	    //           myform.Controls.Add (myHscrlbar);
	    //           myHscrlbar.MouseDown += new MouseEventHandler (ScrollBarMouse_EventHandler);
	    //           myHscrlbar.MouseRightDown ();

	    //           Assert.AreEqual (true, eventhandled, "M5");
	    //           eventhandled = false;
	    //           myform.Dispose ();
	    //   }

	    //[Test]
	    //public void MouseMoveTest ()
	    //   {
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar myHscrlbar = new MyScrollBar ();
	    //           myform.Controls.Add (myHscrlbar);
	    //           myHscrlbar.MouseMove += new MouseEventHandler (ScrollBarMouse_EventHandler);
	    //           myHscrlbar.MoveMouse ();

	    //           Assert.AreEqual (true, eventhandled, "M6");
	    //           eventhandled = false;
	    //           myform.Dispose ();
	    //   }

	    //[Test]
	    //public void MouseUpTest ()
	    //   {
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar myHscrlbar = new MyScrollBar ();
	    //           myform.Controls.Add (myHscrlbar);
	    //           myHscrlbar.MouseUp += new MouseEventHandler (ScrollBarMouse_EventHandler);
	    //           myHscrlbar.MouseRightUp ();

	    //           Assert.AreEqual (true, eventhandled, "M7");
	    //           eventhandled = false;
	    //           myform.Dispose ();
	    //   }

	    [Test]
	    [Category ("NotWorking")]
	    public void ScrollTest ()
	       {
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar myHscrlbar = new MyScrollBar ();
		       myform.Controls.Add (myHscrlbar);
		       myHscrlbar.Scroll += new ScrollEventHandler (ScrollBarScroll_EventHandler);
		       myHscrlbar.ScrollNow ();

		       Assert.AreEqual (true, eventhandled, "S4");
		       eventhandled = false;
		       myform.Dispose ();
	       }

	    [Test]
	    public void TextChangedTest ()
	       {
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar myHscrlbar = new MyScrollBar ();
		       myform.Controls.Add (myHscrlbar);
		       myHscrlbar.TextChanged += new EventHandler (ScrollBar_EventHandler);
		       myHscrlbar.Text = "foo";

		       Assert.AreEqual (true, eventhandled, "T6");
		       eventhandled = false;
		       myform.Dispose ();
	       }

	    [Test]
	    public void ValueChangeTest ()
	       {
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar myHscrlbar = new MyScrollBar ();
		       myform.Controls.Add (myHscrlbar);
		       myHscrlbar.Value = 40 ;
		       myHscrlbar.ValueChanged += new EventHandler (ScrollBar_EventHandler);
		       myHscrlbar.Value = 50 ;
		       Assert.AreEqual (true, eventhandled, "V3");
		       eventhandled = false;
		       myform.Dispose ();
	       }
    }

public class MyHScrollBar : HScrollBar
    {
     public MyHScrollBar () : base ()
	     {
	     }

	    public Size MyDefaultSize {
		    get { return DefaultSize; }
	    }

	    public CreateParams MyCreateParams {
		    get { return CreateParams; }
	    }
    }

   [TestFixture]
   public class MyHScrollBarTest : TestHelper
    {
	    [Test]
	    public void ProtectedTest ()
	       {
		       MyHScrollBar msbar = new MyHScrollBar ();

		       Assert.AreEqual (80, msbar.MyDefaultSize.Width, "D1");
			// this is environment dependent.
		       //Assert.AreEqual (21, msbar.MyDefaultSize.Height, "D2");
	       }
    }

public class MyVScrollBar : VScrollBar
    {
     public MyVScrollBar () : base ()
	     {
	     }

	    public Size MyDefaultSize {
		    get { return DefaultSize; }
	    }

	    public CreateParams MyCreateParams {
		    get { return CreateParams; }
	    }
    }

   [TestFixture]
   public class MyVScrollBarTest : TestHelper
    {
	    [Test]
	    public void PubMethodTest ()
	       {
		       MyVScrollBar msbar = new MyVScrollBar ();

		       Assert.AreEqual (RightToLeft.No, msbar.RightToLeft, "R1");

	       }

	    [Test]
	    public void ProtMethodTest ()
	       {
		       MyVScrollBar msbar = new MyVScrollBar ();

			// This is environment dependent.
		       //Assert.AreEqual (21, msbar.MyDefaultSize.Width, "D3");
		       Assert.AreEqual (80, msbar.MyDefaultSize.Height, "D4");
	       }
    }

   [TestFixture]
   [Ignore("Tests too strict")]
   public class HScrollBarTestEventsOrder : TestHelper
    {
	    public string [] ArrayListToString (ArrayList arrlist)
	     {
		     string [] retval = new string [arrlist.Count];
		     for (int i = 0; i < arrlist.Count; i++)
		       retval[i] = (string)arrlist[i];
		     return retval;
	     }

	    [Test]
	    public void CreateEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "OnHandleCreated",
				 "OnBindingContextChanged",
				 "OnBindingContextChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar s = new MyScrollBar ();
		       myform.Controls.Add (s);

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void BackColorChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "OnHandleCreated",
				 "OnBindingContextChanged",
				 "OnBindingContextChanged",
				 "OnBackColorChanged",
				 "OnInvalidated"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar s = new MyScrollBar ();
		       myform.Controls.Add (s);
		       s.BackColor = Color.Aqua;

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void BackgroundImageChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "OnHandleCreated",
				 "OnBindingContextChanged",
				 "OnBindingContextChanged",
				 "OnBackgroundImageChanged",
				 "OnInvalidated"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar s = new MyScrollBar ();
		       myform.Controls.Add (s);
		       s.BackgroundImage = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/System.Windows.Forms/bitmaps/a.png"));

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    //[Test, Ignore ("Need to send proper Click / DoubleClick")]
	    //public void ClickEventsOrder ()
	    //   {
	    //           string[] EventsWanted = {
	    //                   "OnHandleCreated",
	    //                     "OnBindingContextChanged",
	    //                     "OnBindingContextChanged"
	    //           };
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar s = new MyScrollBar ();
	    //           myform.Controls.Add (s);
	    //           s.MouseClick ();

	    //           Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
	    //           myform.Dispose ();
	    //   }

	    //[Test, Ignore ("Need to send proper Click / DoubleClick")]
	    //public void DoubleClickEventsOrder ()
	    //   {
	    //           string[] EventsWanted = {
	    //                   "OnHandleCreated",
	    //                     "OnBindingContextChanged",
	    //                     "OnBindingContextChanged"
	    //           };
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar s = new MyScrollBar ();
	    //           myform.Controls.Add (s);
	    //           s.MouseDoubleClick ();

	    //           Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
	    //           myform.Dispose ();
	    //   }

	    [Test]
	    public void FontChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "OnHandleCreated",
				 "OnBindingContextChanged",
				 "OnBindingContextChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar s = new MyScrollBar ();
		       myform.Controls.Add (s);
		       FontDialog myFontDialog = new FontDialog();
		       s.Font = myFontDialog.Font;

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void ForeColorChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "OnHandleCreated",
				 "OnBindingContextChanged",
				 "OnBindingContextChanged",
				 "OnForeColorChanged",
				 "OnInvalidated"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar s = new MyScrollBar ();
		       myform.Controls.Add (s);
		       s.ForeColor = Color.Aqua;

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void ImeModeChangedChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "OnHandleCreated",
				 "OnBindingContextChanged",
				 "OnBindingContextChanged",
				 "OnImeModeChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar s = new MyScrollBar ();
		       myform.Controls.Add (s);
		       s.ImeMode = ImeMode.Katakana;

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    //[Test]
	    //public void MouseDownEventsOrder ()
	    //   {
	    //           string[] EventsWanted = {
	    //                   "OnHandleCreated",
	    //                     "OnBindingContextChanged",
	    //                     "OnBindingContextChanged",
	    //                     "OnMouseDown"
	    //           };
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar s = new MyScrollBar ();
	    //           myform.Controls.Add (s);
	    //           s.MouseRightDown ();

	    //           Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
	    //           myform.Dispose ();
	    //   }

	    //[Test]
	    //public void MouseMoveEventsOrder ()
	    //   {
	    //           string[] EventsWanted = {
	    //                   "OnHandleCreated",
	    //                     "OnBindingContextChanged",
	    //                     "OnBindingContextChanged",
	    //                     "OnMouseMove",
	    //                     "OnMouseHover"
	    //           };
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar s = new MyScrollBar ();
	    //           myform.Controls.Add (s);
	    //           s.MoveMouse ();

	    //           Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
	    //           myform.Dispose ();
	    //   }

	    //[Test]
	    //public void MouseUpEventsOrder ()
	    //   {
	    //           string[] EventsWanted = {
	    //                   "OnHandleCreated",
	    //                     "OnBindingContextChanged",
	    //                     "OnBindingContextChanged",
	    //                     "OnMouseUp"
	    //           };
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar s = new MyScrollBar ();
	    //           myform.Controls.Add (s);
	    //           s.MouseRightUp ();

	    //           Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
	    //           myform.Dispose ();
	    //   }

	    [Test]
	    public void PaintEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "OnHandleCreated",
				 "OnBindingContextChanged",
				 "OnBindingContextChanged",
				 "OnInvalidated"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar s = new MyScrollBar ();
		       myform.Controls.Add (s);
		       s.Visible = true;
		       s.Refresh ();
		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void ScrollEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "OnHandleCreated",
				 "OnBindingContextChanged",
				 "OnBindingContextChanged",
				 "OnScroll",
				 "OnValueChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar s = new MyScrollBar ();
		       myform.Controls.Add (s);
		       s.ScrollNow ();

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void TextChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "OnHandleCreated",
				 "OnBindingContextChanged",
				 "OnBindingContextChanged",
				 "OnTextChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar s = new MyScrollBar ();
		       myform.Controls.Add (s);
		       s.Text = "foobar";

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void ValueChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "OnHandleCreated",
				 "OnBindingContextChanged",
				 "OnBindingContextChanged",
				 "OnValueChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar s = new MyScrollBar ();
		       myform.Controls.Add (s);
		       s.Value = 10;

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }
    }

public class MyScrollBar2 : HScrollBar
    {
	    protected ArrayList results = new ArrayList ();
     public MyScrollBar2 () : base ()
	     {
		     this.HandleCreated += new EventHandler (HandleCreated_Handler);
		     this.BackColorChanged += new EventHandler (BackColorChanged_Handler);
		     this.BackgroundImageChanged += new EventHandler (BackgroundImageChanged_Handler);
		     this.BindingContextChanged += new EventHandler (BindingContextChanged_Handler);
		     this.Click += new EventHandler (Click_Handler);
		     this.DoubleClick += new EventHandler (DoubleClick_Handler);
		     this.FontChanged += new EventHandler (FontChanged_Handler);
		     this.ForeColorChanged += new EventHandler (ForeColorChanged_Handler);
		     this.ImeModeChanged += new EventHandler (ImeModeChanged_Handler);
		     this.MouseDown += new MouseEventHandler (MouseDown_Handler);
		     this.MouseMove += new MouseEventHandler (MouseMove_Handler);
		     this.MouseUp += new MouseEventHandler (MouseUp_Handler);
		     this.Invalidated += new InvalidateEventHandler (Invalidated_Handler);
		     this.Resize += new EventHandler (Resize_Handler);
		     this.SizeChanged += new EventHandler (SizeChanged_Handler);
		     this.Layout += new LayoutEventHandler (Layout_Handler);
		     this.VisibleChanged += new EventHandler (VisibleChanged_Handler);
		     this.Paint += new PaintEventHandler (Paint_Handler);
		     this.Scroll += new ScrollEventHandler (Scroll_Handler);
		     this.TextChanged += new EventHandler (TextChanged_Handler);
		     this.ValueChanged += new EventHandler (ValueChanged_Handler);
	     }

	    protected void HandleCreated_Handler (object sender, EventArgs e)
	     {
		     results.Add ("HandleCreated");
	     }

	    protected void BackColorChanged_Handler (object sender, EventArgs e)
	     {
		     results.Add ("BackColorChanged");
	     }

	    protected void BackgroundImageChanged_Handler (object sender, EventArgs e)
	     {
		     results.Add ("BackgroundImageChanged");
	     }

	    protected void Click_Handler (object sender, EventArgs e)
	     {
		     results.Add ("Click");
	     }

	    protected void DoubleClick_Handler (object sender, EventArgs e)
	     {
		     results.Add ("DoubleClick");
	     }

	    protected void FontChanged_Handler (object sender, EventArgs e)
	     {
		     results.Add ("FontChanged");
	     }

	    protected void ForeColorChanged_Handler (object sender, EventArgs e)
	     {
		     results.Add ("ForeColorChanged");
	     }

	    protected void ImeModeChanged_Handler (object sender, EventArgs e)
	     {
		     results.Add ("ImeModeChanged");
	     }

	    protected void MouseDown_Handler (object sender, MouseEventArgs e)
	     {
		     results.Add ("MouseDown");
	     }

	    protected void MouseMove_Handler (object sender, MouseEventArgs e)
	     {
		     results.Add ("MouseMove");
	     }

	    protected void MouseUp_Handler (object sender, MouseEventArgs e)
	     {
		     results.Add ("MouseUp");
	     }

	    protected void BindingContextChanged_Handler (object sender, EventArgs e)
	     {
		     results.Add ("BindingContextChanged");
	     }

	    protected void Invalidated_Handler (object sender, InvalidateEventArgs e)
	     {
		     results.Add("Invalidated");
	     }

	    protected void Resize_Handler (object sender, EventArgs e)
	     {
		     results.Add("Resize");
	     }

	    protected void SizeChanged_Handler (object sender, EventArgs e)
	     {
		     results.Add("SizeChanged");
	     }

	    protected void Layout_Handler (object sender, LayoutEventArgs e)
	     {
		     results.Add("Layout");
	     }

	    protected void VisibleChanged_Handler (object sender, EventArgs e)
	     {
		     results.Add("VisibleChanged");
	     }

	    protected void Paint_Handler (object sender, PaintEventArgs e)
	     {
		     results.Add("Paint");
	     }

	    protected void Scroll_Handler (object sender, ScrollEventArgs e)
	     {
		     results.Add ("Scroll");
	     }

	    protected void TextChanged_Handler (object sender, EventArgs e)
	     {
		     results.Add ("TextChanged");
	     }

	    protected void ValueChanged_Handler (object sender, EventArgs e)
	     {
		     results.Add ("ValueChanged");
	     }

	    public ArrayList Results {
		    get {	return results; }
	    }

	    //public void MoveMouse ()
	    // {
	    //         Message m;

	    //         m = new Message ();

	    //         m.Msg = (int)WndMsg.WM_NCHITTEST;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x0;
	    //         m.LParam = (IntPtr)0x1c604ea;
	    //         this.WndProc(ref m);

	    //         m.Msg = (int)WndMsg.WM_SETCURSOR;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x100448;
	    //         m.LParam = (IntPtr)0x2000001;
	    //         this.WndProc(ref m);

	    //         m.Msg = (int)WndMsg.WM_MOUSEFIRST;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x0;
	    //         m.LParam = (IntPtr)0x14000b;
	    //         this.WndProc(ref m);

	    //         m.Msg = (int)WndMsg.WM_MOUSEHOVER;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x0;
	    //         m.LParam = (IntPtr)0x14000b;
	    //         this.WndProc(ref m);
	    // }

	    //public void MouseRightDown()
	    // {
	    //         Message m;

	    //         m = new Message();

	    //         m.Msg = (int)WndMsg.WM_RBUTTONDOWN;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x01;
	    //         m.LParam = (IntPtr)0x9004f;
	    //         this.WndProc(ref m);
	    // }

	    //public new void MouseClick()
	    // {
	    //         Message m;

	    //         m = new Message();

	    //         m.Msg = (int)WndMsg.WM_LBUTTONDOWN;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x01;
	    //         m.LParam = (IntPtr)0x9004f;
	    //         this.WndProc(ref m);

	    //         m = new Message();

	    //         m.Msg = (int)WndMsg.WM_LBUTTONUP;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x01;
	    //         m.LParam = (IntPtr)0x9004f;
	    //         this.WndProc(ref m);
	    // }

	    //public new void MouseDoubleClick ()
	    // {
	    //         MouseClick ();
	    //         MouseClick ();
	    // }

	    //public void MouseRightUp()
	    // {
	    //         Message m;

	    //         m = new Message();

	    //         m.Msg = (int)WndMsg.WM_RBUTTONUP;
	    //         m.HWnd = this.Handle;
	    //         m.WParam = (IntPtr)0x01;
	    //         m.LParam = (IntPtr)0x9004f;
	    //         this.WndProc(ref m);
	    // }

	public void ScrollNow ()
	{
		Message m;

		m = new Message ();

		m.Msg = 8468;
		m.HWnd = this.Handle;
		m.WParam = (IntPtr)0x1;
		m.LParam = (IntPtr)0x1a051a;
		this.WndProc (ref m);

		m.Msg = 233;
		m.HWnd = this.Handle;
		m.WParam = (IntPtr)0x1;
		m.LParam = (IntPtr)0x12eb34;
		this.WndProc (ref m);
	}
    }

   [TestFixture]
   [Ignore("Tests too strict")]
   public class HScrollBarTestEventsOrder2 : TestHelper
    {
	    public string [] ArrayListToString (ArrayList arrlist)
	     {
		     string [] retval = new string [arrlist.Count];
		     for (int i = 0; i < arrlist.Count; i++)
		       retval[i] = (string)arrlist[i];
		     return retval;
	     }

	    [Test]
	    public void CreateEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "HandleCreated",
				 "BindingContextChanged",
				 "BindingContextChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar2 s = new MyScrollBar2 ();
		       myform.Controls.Add (s);

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void BackColorChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "HandleCreated",
				 "BindingContextChanged",
				 "BindingContextChanged",
				 "Invalidated",
				 "BackColorChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar2 s = new MyScrollBar2 ();
		       myform.Controls.Add (s);
		       s.BackColor = Color.Aqua;

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void BackgroundImageChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "HandleCreated",
				 "BindingContextChanged",
				 "BindingContextChanged",
				 "Invalidated",
				 "BackgroundImageChanged"

		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar2 s = new MyScrollBar2 ();
		       myform.Controls.Add (s);
		       s.BackgroundImage = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/System.Windows.Forms/bitmaps/a.png"));

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    //[Test, Ignore ("Need to send proper Click / DoubleClick")]
	    //public void ClickEventsOrder ()
	    //   {
	    //           string[] EventsWanted = {
	    //                   "HandleCreated",
	    //                     "BindingContextChanged",
	    //                     "BindingContextChanged"
	    //           };
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar2 s = new MyScrollBar2 ();
	    //           myform.Controls.Add (s);
	    //           s.MouseClick ();

	    //           Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
	    //           myform.Dispose ();
	    //   }

	    //[Test, Ignore ("Need to send proper Click / DoubleClick")]
	    //public void DoubleClickEventsOrder ()
	    //   {
	    //           string[] EventsWanted = {
	    //                   "HandleCreated",
	    //                     "BindingContextChanged",
	    //                     "BindingContextChanged"
	    //           };
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar2 s = new MyScrollBar2 ();
	    //           myform.Controls.Add (s);
	    //           s.MouseDoubleClick ();

	    //           Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
	    //           myform.Dispose ();
	    //   }

	    [Test]
	    public void FontChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "HandleCreated",
				 "BindingContextChanged",
				 "BindingContextChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar2 s = new MyScrollBar2 ();
		       myform.Controls.Add (s);
		       FontDialog myFontDialog = new FontDialog();
		       s.Font = myFontDialog.Font;

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void ForeColorChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "HandleCreated",
				 "BindingContextChanged",
				 "BindingContextChanged",
				 "Invalidated",
				 "ForeColorChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar2 s = new MyScrollBar2 ();
		       myform.Controls.Add (s);
		       s.ForeColor = Color.Aqua;

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void ImeModeChangedChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "HandleCreated",
				 "BindingContextChanged",
				 "BindingContextChanged",
				 "ImeModeChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar2 s = new MyScrollBar2 ();
		       myform.Controls.Add (s);
		       s.ImeMode = ImeMode.Katakana;

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    //[Test]
	    //public void MouseDownEventsOrder ()
	    //   {
	    //           string[] EventsWanted = {
	    //                   "HandleCreated",
	    //                     "BindingContextChanged",
	    //                     "BindingContextChanged",
	    //                     "MouseDown"
	    //           };
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar2 s = new MyScrollBar2 ();
	    //           myform.Controls.Add (s);
	    //           s.MouseRightDown ();

	    //           Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
	    //           myform.Dispose ();
	    //   }

	    //[Test]
	    //public void MouseMoveEventsOrder ()
	    //   {
	    //           string[] EventsWanted = {
	    //                   "HandleCreated",
	    //                     "BindingContextChanged",
	    //                     "BindingContextChanged",
	    //                     "MouseMove"
	    //           };
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar2 s = new MyScrollBar2 ();
	    //           myform.Controls.Add (s);
	    //           s.MoveMouse ();

	    //           Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
	    //           myform.Dispose ();
	    //   }

	    //[Test]
	    //public void MouseUpEventsOrder ()
	    //   {
	    //           string[] EventsWanted = {
	    //                   "HandleCreated",
	    //                     "BindingContextChanged",
	    //                     "BindingContextChanged",
	    //                     "MouseUp"
	    //           };
	    //           Form myform = new Form ();
	    //           myform.ShowInTaskbar = false;
	    //           myform.Visible = true;
	    //           MyScrollBar2 s = new MyScrollBar2 ();
	    //           myform.Controls.Add (s);
	    //           s.MouseRightUp ();

	    //           Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
	    //           myform.Dispose ();
	    //   }

	    [Test]
	    public void PaintEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "HandleCreated",
				 "BindingContextChanged",
				 "BindingContextChanged",
				 "Invalidated"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar2 s = new MyScrollBar2 ();
		       myform.Controls.Add (s);
		       s.Visible = true;
		       s.Refresh ();

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void ScrollEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "HandleCreated",
				 "BindingContextChanged",
				 "BindingContextChanged",
				 "Scroll",
				 "ValueChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar2 s = new MyScrollBar2 ();
		       myform.Controls.Add (s);
		       s.ScrollNow ();

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void TextChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "HandleCreated",
				 "BindingContextChanged",
				 "BindingContextChanged",
				 "TextChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar2 s = new MyScrollBar2 ();
		       myform.Controls.Add (s);
		       s.Text = "foobar";

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }

	    [Test]
	    public void ValueChangedEventsOrder ()
	       {
		       string[] EventsWanted = {
			       "HandleCreated",
				 "BindingContextChanged",
				 "BindingContextChanged",
				 "ValueChanged"
		       };
		       Form myform = new Form ();
		       myform.ShowInTaskbar = false;
		       myform.Visible = true;
		       MyScrollBar2 s = new MyScrollBar2 ();
		       myform.Controls.Add (s);
		       s.Value = 10;

		       Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		       myform.Dispose ();
	       }
    }
    
    [TestFixture]
    public class ScrollEventArgsTest : TestHelper
    {
	[Test]
	public void Defaults ()
	{
		ScrollEventArgs e = new ScrollEventArgs (ScrollEventType.EndScroll, 5);

		Assert.AreEqual (5, e.NewValue, "A1");
		Assert.AreEqual (-1, e.OldValue, "A2");
		Assert.AreEqual (ScrollOrientation.HorizontalScroll, e.ScrollOrientation, "A3");
		Assert.AreEqual (ScrollEventType.EndScroll, e.Type, "A4");

		e = new ScrollEventArgs (ScrollEventType.EndScroll, 5, 10);

		Assert.AreEqual (10, e.NewValue, "A5");
		Assert.AreEqual (5, e.OldValue, "A6");
		Assert.AreEqual (ScrollOrientation.HorizontalScroll, e.ScrollOrientation, "A7");
		Assert.AreEqual (ScrollEventType.EndScroll, e.Type, "A8");

		e = new ScrollEventArgs (ScrollEventType.EndScroll, 5, ScrollOrientation.VerticalScroll);

		Assert.AreEqual (5, e.NewValue, "A9");
		Assert.AreEqual (-1, e.OldValue, "A10");
		Assert.AreEqual (ScrollOrientation.VerticalScroll, e.ScrollOrientation, "A11");
		Assert.AreEqual (ScrollEventType.EndScroll, e.Type, "A12");

		e = new ScrollEventArgs (ScrollEventType.EndScroll, 5, 10, ScrollOrientation.VerticalScroll);

		Assert.AreEqual (10, e.NewValue, "A13");
		Assert.AreEqual (5, e.OldValue, "A14");
		Assert.AreEqual (ScrollOrientation.VerticalScroll, e.ScrollOrientation, "A15");
		Assert.AreEqual (ScrollEventType.EndScroll, e.Type, "A16");
	}
    }
}
