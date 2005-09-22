//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//      Hisham Mardam Bey (hisham.mardambey@gmail.com)
//
//
                                                

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Remoting;

namespace MonoTests.System.Windows.Forms
{
        [TestFixture]
	[Ignore ("This is a work in progress.")]
        public class ScrollbarTest
        {		
		[Test]
		public void PubPropTest ()
		{
			ScrollBar myscrlbar = new HScrollBar ();
			
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
			ScrollBar myscrlbar = new HScrollBar ();
			myscrlbar.Minimum = 10;
			myscrlbar.Maximum = 20;			
			myscrlbar.Value = 9;
			myscrlbar.Value = 21;
		}
		
		[Test]
		public void PubMethodTest ()
		{
			ScrollBar myscrlbar = new HScrollBar ();
			myscrlbar.Text = "New HScrollBar";
			Assert.AreEqual ("System.Windows.Forms.HScrollBar, Minimum: 0, Maximum: 100, Value: 0",
					 myscrlbar.ToString (), "T5");
		}				
	}

        [TestFixture]
	[Ignore ("This is a work in progress.")]     
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
		
		[Test, Ignore ("Incomplete.")]
	        public void MouseDownTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.MouseDown += new MouseEventHandler (ScrollBar_EventHandler);

			Assert.AreEqual (true, eventhandled, "M5");
			eventhandled = false;
		}
		
		[Test, Ignore ("Incomplete.")]
	        public void MouseMoveTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.MouseMove += new MouseEventHandler (ScrollBar_EventHandler);

			Assert.AreEqual (true, eventhandled, "M6");
			eventhandled = false;
		}
		
		[Test, Ignore ("Incomplete.")]
	        public void MouseUpTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.MouseUp += new MouseEventHandler (ScrollBar_EventHandler);

			Assert.AreEqual (true, eventhandled, "M7");
			eventhandled = false;
		}
		
		[Test, Ignore ("Incomplete.")]
	        public void PaintTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.Paint += new PaintEventHandler (ScrollBar_EventHandler);

			Assert.AreEqual (true, eventhandled, "P1");
			eventhandled = false;
		}
		
		[Test, Ignore ("Incomplete.")]
	        public void ScrollTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
			myform.Controls.Add (myHscrlbar);
			myHscrlbar.Scroll += new ScrollEventHandler (ScrollBar_EventHandler);
			
			Assert.AreEqual (true, eventhandled, "S4");
			eventhandled = false;
		}	
		
		[Test, Ignore ("Is this raised? Check on MS.")]
	        public void TextChangedTest ()
	        {
			Form myform = new Form ();
			myform.Visible = true;			
			ScrollBar myHscrlbar = new HScrollBar ();
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
			ScrollBar myHscrlbar = new HScrollBar ();
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
	[Ignore ("This is a work in progress.")]     
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
	[Ignore ("This is a work in progress.")]     
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
}
	   
