//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class CheckBoxEventTest
	{
		static bool eventhandled = false;
		public void CheckBox_EventHandler (object sender,EventArgs e)
		{
			eventhandled = true;
		}		

		[Test]
		public void ApperanceEventTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			CheckBox chkbox = new CheckBox ();
			chkbox.Visible = true;
			myform.Controls.Add (chkbox);
			chkbox.AppearanceChanged += new EventHandler (CheckBox_EventHandler);
			chkbox.Appearance = Appearance.Button;
			Assert.AreEqual (true, eventhandled, "#A1");
		}

		[Test]
		public void CheckedChangedEventTest ()
		{
			eventhandled = false;
			Form myform = new Form ();
			myform.Visible = true;
			CheckBox chkbox = new CheckBox ();
			chkbox.Visible = true;
			myform.Controls.Add (chkbox);
			chkbox.CheckedChanged += new EventHandler (CheckBox_EventHandler);
			chkbox.CheckState = CheckState.Indeterminate;
			Assert.AreEqual (true, eventhandled, "#A2");
		}

		[Test]
		public void CheckStateChangedEventTest ()
		{
			eventhandled = false;
			Form myform = new Form ();
			myform.Visible = true;
			CheckBox chkbox = new CheckBox ();
			chkbox.Visible = true;
			myform.Controls.Add (chkbox);
			chkbox.CheckStateChanged += new EventHandler (CheckBox_EventHandler);
			chkbox.CheckState = CheckState.Checked;
			Assert.AreEqual (true, eventhandled, "#A3");
		}
	}
}
