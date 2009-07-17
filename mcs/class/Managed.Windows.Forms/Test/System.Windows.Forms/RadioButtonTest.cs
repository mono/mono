//
// RadioRadioButtonTest.cs: Test cases for RadioRadioButton.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class RadioButtonTest : TestHelper
	{
		[Test]
		public void RadioButtonPropertyTest ()
		{
			RadioButton rButton1 = new RadioButton ();
			
			// A
			Assert.AreEqual (Appearance.Normal, rButton1.Appearance, "#A1");
			Assert.AreEqual (true, rButton1.AutoCheck, "#A2");

			// C
			Assert.AreEqual (false, rButton1.Checked, "#C1");
			Assert.AreEqual (ContentAlignment.MiddleLeft, rButton1.CheckAlign, "#C2");
					
			// S
			Assert.AreEqual (null, rButton1.Site, "#S1");	

			// T
			rButton1.Text = "New RadioButton";
			Assert.AreEqual ("New RadioButton", rButton1.Text, "#T1");
			Assert.AreEqual (ContentAlignment.MiddleLeft, rButton1.TextAlign, "#T2");
			Assert.IsFalse (rButton1.TabStop, "#T3");
		}

		[Test]
		public void CheckedTest ()
		{
			RadioButton rb = new RadioButton ();

			Assert.AreEqual (false, rb.TabStop, "#A1");
			Assert.AreEqual (false, rb.Checked, "#A2");

			rb.Checked = true;

			Assert.AreEqual (true, rb.TabStop, "#B1");
			Assert.AreEqual (true, rb.Checked, "#B2");

			rb.Checked = false;

			Assert.AreEqual (false, rb.TabStop, "#C1");
			Assert.AreEqual (false, rb.Checked, "#C2");

			// RadioButton is NOT checked, but since it is the only
			// RadioButton instance in Form, when it gets selected (Form.Show)
			// it should acquire the focus
			Form f = new Form ();
			f.Controls.Add (rb);
			rb.CheckedChanged += new EventHandler (rb_checked_changed);
			event_received = false;

			f.ActiveControl = rb;

			Assert.AreEqual (true, event_received, "#D1");
			Assert.AreEqual (true, rb.Checked, "#D2");
			Assert.AreEqual (true, rb.TabStop, "#D3");

			f.Dispose ();
		}

		bool event_received = false;
		void rb_tabstop_changed (object sender, EventArgs e)
		{
			event_received = true;
		}

		void rb_checked_changed (object sender, EventArgs e)
		{
			event_received = true;
		}

		[Test]
		public void TabStopEventTest ()
		{
			RadioButton rb = new RadioButton ();

			rb.TabStopChanged += new EventHandler (rb_tabstop_changed);
			event_received = false;

			rb.TabStop = true;

			Assert.IsTrue (event_received);
		}

		[Test]
		public void ToStringTest ()
		{
			RadioButton rButton1 = new RadioButton ();
			Assert.AreEqual ("System.Windows.Forms.RadioButton, Checked: False" , rButton1.ToString (), "#9");
		}

#if NET_2_0
		[Test]
		public void AutoSizeText ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			RadioButton rb = new RadioButton ();
			rb.AutoSize = true;
			rb.Width = 14;
			f.Controls.Add (rb);
			
			int width = rb.Width;
			
			rb.Text = "Some text that is surely longer than 100 pixels.";

			if (rb.Width == width)
				Assert.Fail ("RadioButton did not autosize, actual: {0}", rb.Width);
		}
#endif
	}
	
	[TestFixture]
	public class RadioButtonEventTestClass : TestHelper
	{
		static bool eventhandled = false;
		public static void RadioButton_EventHandler (object sender, EventArgs e)
		{
			eventhandled = true;
		}

		[Test]
		public void PanelClickTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			RadioButton rButton1 = new RadioButton ();
			rButton1.Select ();
			rButton1.Visible = true;
			myForm.Controls.Add (rButton1);
			eventhandled = false;
			rButton1.Click += new EventHandler (RadioButton_EventHandler);
			myForm.Show ();
			rButton1.PerformClick ();
			Assert.AreEqual (true, eventhandled, "#2");
			myForm.Dispose ();
		}

		[Test]
		public void ApperanceChangedTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			RadioButton rButton1 = new RadioButton ();
			rButton1.Select ();
			rButton1.Visible = true;
			myForm.Controls.Add (rButton1);
			rButton1.Appearance = Appearance.Normal;
			eventhandled = false;
			rButton1.AppearanceChanged += new EventHandler (RadioButton_EventHandler);
			rButton1.Appearance = Appearance.Button;
			Assert.AreEqual (true, eventhandled, "#2");
			myForm.Dispose ();
		}
	
		[Test]
		public void CheckedChangedTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			RadioButton rButton1 = new RadioButton ();
			rButton1.Select ();
			rButton1.Visible = true;
			myForm.Controls.Add (rButton1);
			rButton1.Checked = false;
			eventhandled = false;
			rButton1.CheckedChanged += new EventHandler (RadioButton_EventHandler);
			rButton1.Checked = true;
			Assert.AreEqual (true, eventhandled, "#3");
			myForm.Dispose ();
		}
	}
}
