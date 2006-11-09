//
// StatusBarTest.cs: Test cases for StatusBar.
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
	public class StatusBarTest 
	{

		[Test]
		public void StatusBarPropertyTest ()
		{	
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			StatusBar mysbar = new StatusBar ();
			StatusBarPanel sbarpanel1 = new StatusBarPanel ();
			StatusBarPanel sbarpanel2 = new StatusBarPanel ();
			sbarpanel1.Text = "Status Quo";
			sbarpanel2.Text = "State 2";
			mysbar.Panels.Add (sbarpanel1);
			mysbar.Panels.Add (sbarpanel2);
			myform.Controls.Add (mysbar);
			
			// B
			Assert.AreEqual ("Control", mysbar.BackColor.Name, "#B1");

			// D
			Assert.AreEqual (DockStyle.Bottom, mysbar.Dock, "#D1");

			// F
			Assert.AreEqual ("ControlText", mysbar.ForeColor.Name, "#F2");
	
			// P
			Assert.AreEqual (sbarpanel1.Text, mysbar.Panels [0].Text , "#P1");
			
			// S
			Assert.AreEqual (false, mysbar.ShowPanels, "#S1");
			Assert.AreEqual (true, mysbar.SizingGrip, "#S2");
			Assert.AreEqual (null, mysbar.Site, "#S3");

			// T
			Assert.AreEqual ("", mysbar.Text, "#T1");
			mysbar.Text = "MONO STATUSBAR";
			Assert.AreEqual ("MONO STATUSBAR", mysbar.Text, "#T2");

			myform.Dispose ();
		}
		
		[Test]
		public void ToStringMethodTest () 
		{
			StatusBar mysbar = new StatusBar ();
			mysbar.Text = "New StatusBar";
			Assert.AreEqual ("System.Windows.Forms.StatusBar, Panels.Count: 0", mysbar.ToString (), "#T3");
		}
		//[MonoTODO ("Test case for DrawItem")]	
		//[MonoTODO ("Test case for PanelClick")]	
	}
}
