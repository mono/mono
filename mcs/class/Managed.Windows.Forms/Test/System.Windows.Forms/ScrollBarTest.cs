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
	[Ignore ("This test has to be completly reviewed")]
        public class ScrollbarTest
        {
		
		[Test]
		public void PubPropTest ()
		{
			ScrollBar myscrlbar = new HScrollBar ();
			
			// B
			myscrlbar.BackColor = Color.Red;
			Assert.AreEqual (255, myscrlbar.BackColor.R, "#B2");
			myscrlbar.BackgroundImage = Image.FromFile ("a.png");
			Assert.AreEqual (16, myscrlbar.BackgroundImage.Height, "#B3");
			
			// F
			Assert.AreEqual ("ff000000", myscrlbar.ForeColor.Name, "#F1");
			
			// I
			//Assert.AreEqual (ImeMode.Disable, myscrlbar.ImeMode, "#I1");
			
			// L
                        Assert.AreEqual (10, myscrlbar.LargeChange, "#L1");
			
			// M
			Assert.AreEqual (100, myscrlbar.Maximum, "#M1");
			Assert.AreEqual (0, myscrlbar.Minimum, "#M2");
			myscrlbar.Maximum = 300;
			myscrlbar.Minimum = 100;
			Assert.AreEqual (300, myscrlbar.Maximum, "#M3");
			Assert.AreEqual (100, myscrlbar.Minimum, "#M4");
			
			// S
			Assert.AreEqual (null, myscrlbar.Site, "#S1");
			Assert.AreEqual (1, myscrlbar.SmallChange, "#S2");
			myscrlbar.SmallChange = 10;
			Assert.AreEqual (10, myscrlbar.SmallChange, "#S3");
			
			// T
			Assert.AreEqual (false, myscrlbar.TabStop, "#T1");
			myscrlbar.TabStop = true;
			Assert.AreEqual (true, myscrlbar.TabStop, "#T2");
			Assert.AreEqual ("", myscrlbar.Text, "#T3");
			myscrlbar.Text = "MONO SCROLLBAR";
			Assert.AreEqual ("MONO SCROLLBAR", myscrlbar.Text, "#T4");
			
			// V
                        Assert.AreEqual (100, myscrlbar.Value, "#V1");
			myscrlbar.Value = 150;			
			Assert.AreEqual (150, myscrlbar.Value, "#V2");
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
					 myscrlbar.ToString (), "#T5");
		}				
	}
   
        [TestFixture]
        public class ScrollBarValueChangedEventClass
        {
		static bool eventhandled = false;
		public static void ValueChange_EventHandler (object sender, EventArgs e)
		{
			eventhandled = true;
		}
		
		[Test]
	        public void ValueChangeEventTest ()
	        {
			ScrollBar myHscrlbar = new HScrollBar ();
			myHscrlbar.Value = 40 ;
			myHscrlbar.ValueChanged += new EventHandler (ValueChange_EventHandler);
			myHscrlbar.Value = 50 ;
			Assert.AreEqual (true, eventhandled, "#1");
		}
	}   
}
	   
