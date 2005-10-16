//
// TrackBarTest.cs: Test cases for TrackBar.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TrackBarBaseTest
	{
		[Test]
		public void TrackBarPropertyTest ()
		{
			TrackBar myTrackBar = new TrackBar ();
			
			// A
			Assert.AreEqual (true, myTrackBar.AutoSize, "#A1");

			// L
			Assert.AreEqual (5, myTrackBar.LargeChange, "#L1");
                	
			// M
			Assert.AreEqual (10, myTrackBar.Maximum, "#M1");
			Assert.AreEqual (0, myTrackBar.Minimum, "#M2");
			
			// O
			Assert.AreEqual (Orientation.Horizontal, myTrackBar.Orientation, "#O1");
				
			// S
			Assert.AreEqual (1, myTrackBar.SmallChange, "#S1");

			// T
			Assert.AreEqual (1, myTrackBar.TickFrequency, "#T1");
			Assert.AreEqual (TickStyle.BottomRight, myTrackBar.TickStyle, "#T2");
			Assert.AreEqual ("", myTrackBar.Text, "#T3");
			myTrackBar.Text = "New TrackBar";
			Assert.AreEqual ("New TrackBar", myTrackBar.Text, "#T4");

			// V
			Assert.AreEqual (0, myTrackBar.Value, "#V1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]		
		public void LargeChangeTest ()
		{
			TrackBar myTrackBar = new TrackBar ();
			myTrackBar.LargeChange = -1;
		}
	
		[Test]
		public void SetRangeTest () 
		{
			TrackBar myTrackBar = new TrackBar ();
			myTrackBar.SetRange (2,9);
			Assert.AreEqual (9, myTrackBar.Maximum, "#setM1");
			Assert.AreEqual (2, myTrackBar.Minimum, "#setM2");
		}

		[Test]
		public void ToStringMethodTest () 
		{
			TrackBar myTrackBar = new TrackBar ();
			myTrackBar.Text = "New TrackBar";
			Assert.AreEqual ("System.Windows.Forms.TrackBar, Minimum: 0, Maximum: 10, Value: 0", myTrackBar.ToString (), "#T3");
		}
	}
}

