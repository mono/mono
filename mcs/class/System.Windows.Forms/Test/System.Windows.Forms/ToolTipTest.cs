//
// ToolTipTest.cs: Test cases for ToolTip.
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
	public class ToolTipTest : TestHelper 
	{

		[Test]
		public void ToolTipPropertyTest ()
		{	
			ToolTip myToolTip = new ToolTip ();
			
			// A
			Assert.AreEqual (true, myToolTip.Active, "#A1");
			Assert.AreEqual (5000, myToolTip.AutoPopDelay, "#A2");
			Assert.AreEqual (5000, myToolTip.AutoPopDelay, "#A3");

			// I 
			Assert.AreEqual (500, myToolTip.InitialDelay, "#I1");
			
			// R
			Assert.AreEqual (100, myToolTip.ReshowDelay, "#R1");

			// S
			Assert.AreEqual (false, myToolTip.ShowAlways, "#S1");
		}

		[Test]
		public void GetAndSetToolTipTest ()
		{
			ToolTip myToolTip = new ToolTip ();
			Button myButton = new Button ();
			myToolTip.ShowAlways = true;
			myToolTip.SetToolTip (myButton, "My Button");
			string myString = myToolTip.GetToolTip (myButton);
			Assert.AreEqual ("My Button", myString, "#Mtd1");
		}
		
		[Test]
		public void RemoveToolTipTest ()
		{
			ToolTip myToolTip = new ToolTip ();
			Button myButton = new Button ();
			myToolTip.ShowAlways = true;
			myToolTip.SetToolTip (myButton, "My Button");
			myToolTip.RemoveAll ();
			Assert.AreEqual ("", myToolTip.GetToolTip (myButton), "#Mtd2");
		}

		[Test]
		public void ToStringTest ()
		{
			ToolTip myToolTip = new ToolTip ();
			Assert.AreEqual ("System.Windows.Forms.ToolTip InitialDelay: 500, ShowAlways: False", myToolTip.ToString (), "#Mtd3");
		}
		
		[Test] // bug 82399
		public void DontCreateHandle ()
		{
			Form f = new Form ();
			Button b = new Button ();
			
			f.Controls.Add (b);
			
			ToolTip t = new ToolTip ();
			
			Assert.AreEqual (false, f.IsHandleCreated, "A1");
			t.SetToolTip (b, string.Empty);
			Assert.AreEqual (false, f.IsHandleCreated, "A2");
			
			f.Dispose ();
		}
	}
}
