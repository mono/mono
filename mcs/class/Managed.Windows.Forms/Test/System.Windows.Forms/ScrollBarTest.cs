//
// ScrollBarTest.cs: Test cases for ScrollBar.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//


using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Remoting;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ScrollBarTest 
	{

		[Test]
		public void ScrollBarPropertyTest ()
		{	
			Form myform = new Form ();
			ScrollBar myscrlbar = new HScrollBar (); // Could be checked for VScrollBar as well 
				
			// B
			Assert.AreEqual ("Control", myscrlbar.BackColor.Name, "#B1");
			myscrlbar.BackColor = Color.Red;
			Assert.AreEqual (255, myscrlbar.BackColor.R, "#B2");
			myscrlbar.BackgroundImage = Image.FromFile ("M.gif");
			Assert.AreEqual (60, myscrlbar.BackgroundImage.Height, "#B3");

			// F
			Assert.AreEqual ("ControlText", myscrlbar.ForeColor.Name, "#F1");
	
			// I 
			Assert.AreEqual (ImeMode.Disable, myscrlbar.ImeMode, "#I1");
			
			// L
			Assert.AreEqual (10, myscrlbar.LargeChange, "#L1");
			
			// M
			Assert.AreEqual (100, myscrlbar.Maximum, "#M1");
			Assert.AreEqual (0, myscrlbar.Minimum, "#M2");

			// S
			Assert.AreEqual (null, myscrlbar.Site, "#S1");
			Assert.AreEqual (1, myscrlbar.SmallChange, "#S2");
			
			// T
			Assert.AreEqual ("", myscrlbar.Text, "#T1");
			myscrlbar.Text = "MONO SCROLLBAR";
			Assert.AreEqual ("MONO SCROLLBAR", myscrlbar.Text, "#T2");

			// V
			Assert.AreEqual (0, myscrlbar.Value, "#V1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ExceptionChangeTest ()
		{
			ScrollBar myHscrlbar = new HScrollBar ();
			myHscrlbar.LargeChange = -1; // LargeChange must be greater than 0
			myHscrlbar.SmallChange = -1; // SmallChange must be greater than 0	
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ExceptionValTest ()
		{
			ScrollBar myHscrlbar1 = new HScrollBar ();
			ScrollBar myHscrlbar2 = new HScrollBar ();
			myHscrlbar1.Value = -1 ;
			myHscrlbar2.Value = 101 ;
		}
		
		[Test]
		public void ToStringMethodTest () 
		{
			ScrollBar myHscrlbar = new HScrollBar ();
			myHscrlbar.Text = "New HScrollBar";
			Assert.AreEqual ("System.Windows.Forms.HScrollBar, Minimum: 0, Maximum: 100, Value: 0",
				         myHscrlbar.ToString (), "#T3");
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
