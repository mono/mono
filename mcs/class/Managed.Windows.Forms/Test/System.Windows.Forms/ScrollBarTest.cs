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

namespace MonoTests.System.Windows.Forms
{
public class MyScrollBar : HScrollBar
        {
		private ArrayList results = new ArrayList ();
	        public MyScrollBar () : base ()
		{
			// TODO: add event handlers (+=)
		}
		
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
	   
	        public void MoveMouse ()
	        {
		   Message m;
		   
		   m = new Message ();
		   
		   m.Msg = (int)WndMsg.WM_NCHITTEST;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x0;
		   m.LParam = (IntPtr)0x1c604ea;
		   this.WndProc(ref m);
		   
		   m.Msg = (int)WndMsg.WM_SETCURSOR;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x100448;
		   m.LParam = (IntPtr)0x2000001;
		   this.WndProc(ref m);
		   
		   m.Msg = (int)WndMsg.WM_MOUSEFIRST;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x0;
		   m.LParam = (IntPtr)0x14000b;
		   this.WndProc(ref m);
		   
		   m.Msg = (int)WndMsg.WM_MOUSEHOVER;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x0;
		   m.LParam = (IntPtr)0x14000b;
		   this.WndProc(ref m);
		}		
	   
	        public void MouseRightDown() 
	        {
		   Message m;
		   
		   m = new Message();
		   
		   m.Msg = (int)WndMsg.WM_RBUTTONDOWN;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x01;
		   m.LParam = (IntPtr)0x9004f;
		   this.WndProc(ref m);
		}
	   
	        public void MouseRightUp() 
	        {
		   Message m;
		   
		   m = new Message();
		   
		   m.Msg = (int)WndMsg.WM_RBUTTONUP;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x01;
		   m.LParam = (IntPtr)0x9004f;
		   this.WndProc(ref m);
		}
	   
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
        public class ScrollbarTest
        {		
		[Test]
		public void PubPropTest ()
		{
			MyScrollBar myscrlbar = new MyScrollBar ();
			
			// B
			myscrlbar.BackColor = Color.Red;
			Assert.AreEqual (255, myscrlbar.BackColor.R, "B2");
			myscrlbar.BackgroundImage = Image.FromFile ("Test/System.Windows.Forms/bitmaps/a.png");
			Assert.AreEqual (16, myscrlbar.BackgroundImage.Height, "B3");
			
			// F
			Assert.AreEqual ("ff000000", myscrlbar.ForeColor.Name, "F1");
			
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
		[ExpectedException (typeof (ArgumentException))]
		public void ExceptionValueTest ()
		{
			MyScrollBar myscrlbar = new MyScrollBar ();
			myscrlbar.Minimum = 10;
			myscrlbar.Maximum = 20;			
			myscrlbar.Value = 9;
			myscrlbar.Value = 21;
		}
		
		[Test]
		public void PubMethodTest ()
		{
			MyScrollBar myscrlbar = new MyScrollBar ();
			myscrlbar.Text = "New HScrollBar";
			Assert.AreEqual ("System.Windows.Forms.HScrollBar, Minimum: 0, Maximum: 100, Value: 0",
					 myscrlbar.ToString (), "T5");
		}				
	}

        [TestFixture]	
        public class ScrollBarEventTest
        {
		static bool eventhandled = false;
		public void ScrollBar_EventHandler (object sender,EventArgs e)
		{
			eventhandled = true;
		}
				
		[Test]
	        public void BackColorChangedTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.BackColorChanged += new EventHandler (ScrollBar_EventHandler);
			myHscrlbar.BackColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "B4");
			eventhandled = false;			
		}
		
		[Test]
	        public void BackgroundImageChangedTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.BackgroundImageChanged += new EventHandler (ScrollBar_EventHandler);
			myHscrlbar.BackgroundImage = Image.FromFile ("Test/System.Windows.Forms/bitmaps/a.png");
			Assert.AreEqual (true, eventhandled, "B5");
			eventhandled = false;
		}
		
		[Test, Ignore ("Incomplete.")]
	        public void ClickTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.Click += new EventHandler (ScrollBar_EventHandler);

			Assert.AreEqual (true, eventhandled, "C1");
		}
		
		[Test, Ignore ("Incomplete.")]
	        public void DoubleClickTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.DoubleClick += new EventHandler (ScrollBar_EventHandler);

			Assert.AreEqual (true, eventhandled, "D1");
		}		
		
		[Test]
	        public void FontChangedTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.FontChanged += new EventHandler (ScrollBar_EventHandler);
			FontDialog myFontDialog = new FontDialog();
			myHscrlbar.Font = myFontDialog.Font;
			Assert.AreEqual (true, eventhandled, "F2");
			eventhandled = false;
		}

		[Test]
	        public void ForeColorChangedTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.ForeColorChanged += new EventHandler (ScrollBar_EventHandler);
			myHscrlbar.ForeColor = Color.Azure;
			Assert.AreEqual (true, eventhandled, "F3");
			eventhandled = false;
		}
				
		[Test]
	        public void ImeModehangedTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.ImeModeChanged += new EventHandler (ScrollBar_EventHandler);
			myHscrlbar.ImeMode = ImeMode.Katakana;
			Assert.AreEqual (true, eventhandled, "I2");
			eventhandled = false;
		}
		
		[Test]
	        public void MouseDownTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			MyScrollBar myHscrlbar = new MyScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.MouseDown += new MouseEventHandler (ScrollBar_EventHandler);
		        myHscrlbar.MouseRightDown ();
		   
			Assert.AreEqual (true, eventhandled, "M5");
			eventhandled = false;
		}
		
		[Test]
	        public void MouseMoveTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			MyScrollBar myHscrlbar = new MyScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.MouseMove += new MouseEventHandler (ScrollBar_EventHandler);
		        myHscrlbar.MoveMouse ();
		   
			Assert.AreEqual (true, eventhandled, "M6");
			eventhandled = false;
		}
		
		[Test]
	        public void MouseUpTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			MyScrollBar myHscrlbar = new MyScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.MouseUp += new MouseEventHandler (ScrollBar_EventHandler);
		        myHscrlbar.MouseRightUp ();
		   
			Assert.AreEqual (true, eventhandled, "M7");
			eventhandled = false;
		}
		
		[Test, Ignore ("Incomplete.")]
	        public void PaintTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			MyScrollBar myHscrlbar = new MyScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.Paint += new PaintEventHandler (ScrollBar_EventHandler);

			Assert.AreEqual (true, eventhandled, "P1");
			eventhandled = false;
		}
		
		[Test]
	        public void ScrollTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			MyScrollBar myHscrlbar = new MyScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.Scroll += new ScrollEventHandler (ScrollBar_EventHandler);
		        myHscrlbar.ScrollNow ();
		   
			Assert.AreEqual (true, eventhandled, "S4");
			eventhandled = false;
		}	
		
		[Test, Ignore ("Is this raised? Check on MS.")]
	        public void TextChangedTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			MyScrollBar myHscrlbar = new MyScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.TextChanged += new EventHandler (ScrollBar_EventHandler);

			Assert.AreEqual (true, eventhandled, "T6");
			eventhandled = false;
		}		
		 
		[Test]
	        public void ValueChangeTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			MyScrollBar myHscrlbar = new MyScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.Value = 40 ;
			myHscrlbar.ValueChanged += new EventHandler (ScrollBar_EventHandler);
			myHscrlbar.Value = 50 ;
			Assert.AreEqual (true, eventhandled, "V3");
			eventhandled = false;			
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
        public class MyHScrollBarTest
        {
		[Test]
		public void ProtectedTest ()
		{
			MyHScrollBar msbar = new MyHScrollBar ();
			
			Assert.AreEqual (80, msbar.MyDefaultSize.Width, "D1");
			Assert.AreEqual (16, msbar.MyDefaultSize.Height, "D2");
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
        public class MyVScrollBarTest
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
			
			Assert.AreEqual (16, msbar.MyDefaultSize.Width, "D3");
			Assert.AreEqual (80, msbar.MyDefaultSize.Height, "D4");
		}		
	}
                  
        [TestFixture]
        public class HScrollBarTestEventsOrder
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
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
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
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
			s.BackColor = Color.Aqua;
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
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
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
			s.BackgroundImage = Image.FromFile ("Test/System.Windows.Forms/bitmaps/a.png");
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		}
		
		[Test, Ignore ("Not implemented yet, needs msg")]
		public void ClickEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged",
				  "OnClick"
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);			
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		}		
		
		[Test, Ignore ("Not implemented yet, needs msg")]
		public void DoubleClickEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged",
				  "OnDoubleClick"
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);			
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		}
		
		[Test]
		public void FontChangedEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged"
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
			FontDialog myFontDialog = new FontDialog();
			s.Font = myFontDialog.Font;
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
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
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
			s.ForeColor = Color.Aqua;
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
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
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
			s.ImeMode = ImeMode.Katakana;
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		}
		
		[Test]
		public void MouseDownEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged"
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
			s.MouseRightDown ();
		   
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		}
		
		[Test]
		public void MouseMoveEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged"
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
		        s.MoveMouse ();
		   
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		}
		
		[Test]
		public void MouseUpEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged"
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
		        s.MouseRightUp ();
		   
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		}
		
		[Test]
		public void PaintEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged"
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
			s.Visible = true;
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		}
		
		[Test, Ignore ("Not implemented yet, needs msg.")]
		public void ScrollEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged"
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);			
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
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
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
			s.Text = "foobar";
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
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
			myform.Visible = true;
			MyScrollBar s = new MyScrollBar ();
			myform.Controls.Add (s);
			s.Value = 10;
			
			Assert.AreEqual (EventsWanted, ArrayListToString (s.Results));
		}
	}         
}
