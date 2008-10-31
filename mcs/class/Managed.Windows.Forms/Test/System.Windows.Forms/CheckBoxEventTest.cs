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
	public class CheckBoxEventTest : TestHelper
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
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			CheckBox chkbox = new CheckBox ();
			chkbox.Visible = true;
			myform.Controls.Add (chkbox);
			chkbox.AppearanceChanged += new EventHandler (CheckBox_EventHandler);
			chkbox.Appearance = Appearance.Button;
			Assert.AreEqual (true, eventhandled, "#A1");
			myform.Dispose ();
		}

		[Test]
		public void CheckedChangedEventTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			eventhandled = false;
			myform.Visible = true;
			CheckBox chkbox = new CheckBox ();
			chkbox.Visible = true;
			myform.Controls.Add (chkbox);
			chkbox.CheckedChanged += new EventHandler (CheckBox_EventHandler);
			chkbox.CheckState = CheckState.Indeterminate;
			Assert.AreEqual (true, eventhandled, "#A2");
			myform.Dispose ();
		}

		[Test]
		public void CheckStateChangedEventTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			eventhandled = false;
			myform.Visible = true;
			CheckBox chkbox = new CheckBox ();
			chkbox.Visible = true;
			myform.Controls.Add (chkbox);
			chkbox.CheckStateChanged += new EventHandler (CheckBox_EventHandler);
			chkbox.CheckState = CheckState.Checked;
			Assert.AreEqual (true, eventhandled, "#A3");
			myform.Dispose ();
		}
	}
}
