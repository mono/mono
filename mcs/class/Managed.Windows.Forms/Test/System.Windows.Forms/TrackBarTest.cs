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
	public class TrackBarBaseTest : TestHelper
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
#if NET_2_0
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void LargeChangeTest ()
		{
			TrackBar myTrackBar = new TrackBar ();
			myTrackBar.LargeChange = -1;
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void SmallChangeTest ()
		{
			TrackBar myTrackBar = new TrackBar ();
			myTrackBar.SmallChange = -1;
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

		[Test]
		public void OrientationSizeTest ()
		{	
			IntPtr handle;
			int width;
			int height ;
			int default_height = 45;
			int default_height2 = 42;

			using (TrackBar myTrackBar = new TrackBar()) {
				width = myTrackBar.Width;
				height = myTrackBar.Height;
				myTrackBar.Orientation = Orientation.Vertical;
				Assert.AreEqual(width, myTrackBar.Width, "#OS1");
				Assert.AreEqual(height, myTrackBar.Height, "#OS2");
			}
			
			using (Form myForm = new Form()) {
				using ( TrackBar myTrackBar = new TrackBar()) {
					width = myTrackBar.Width;
					height = myTrackBar.Height;
					myForm.Controls.Add(myTrackBar);
					handle = myTrackBar.Handle; // causes the handle to be created.
					myTrackBar.Orientation = Orientation.Vertical;
					AreEqual(default_height, default_height2, myTrackBar.Width,  "#OS3");
					Assert.AreEqual(width, myTrackBar.Height, "#OS4");
				}
			}

			using (Form myForm = new Form()) {
				using ( TrackBar myTrackBar = new TrackBar()) {
					myForm.Controls.Add(myTrackBar);
					handle = myTrackBar.Handle; // causes the handle to be created.
					myTrackBar.Width = 200;
					myTrackBar.Orientation = Orientation.Vertical;
					Assert.AreEqual(200, myTrackBar.Height, "#OS5");
				}
			}
			Assert.AreEqual(handle, handle, "Removes warning");
		}
	
		private void AreEqual(int expected1, int expected2, int real, string name)
		{
			// This is needed since the default size vary between XP theme and W2K theme.
			if (real != expected1 && real != expected2) {
				Assert.Fail("{3}: Expected <{0}> or <{1}>, but was <{2}>", expected1, expected2, real, name);
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void SizeTestSettingOrientation ()
		{
			IntPtr handle;
			int default_height = 45;
			int default_height2 = 42;

			using (TrackBar myTrackBar = new TrackBar()) {
				myTrackBar.Width = 200;
				myTrackBar.Height = 250;
				myTrackBar.Orientation = Orientation.Vertical;
				Assert.AreEqual(200, myTrackBar.Width, "#SIZE03");
				AreEqual(default_height, default_height2, myTrackBar.Height, "#SIZE04");
			}

			using (TrackBar myTrackBar = new TrackBar()) {
				myTrackBar.AutoSize = false;
				myTrackBar.Width = 200;
				myTrackBar.Height = 250;
				myTrackBar.Orientation = Orientation.Vertical;
				Assert.AreEqual(200, myTrackBar.Width, "#SIZE07");
				Assert.AreEqual(250, myTrackBar.Height, "#SIZE08");
			}

			using (TrackBar myTrackBar = new TrackBar()) {
				myTrackBar.Width = 200;
				myTrackBar.Height = 250;
				myTrackBar.AutoSize = false;
				myTrackBar.Orientation = Orientation.Vertical;
				Assert.AreEqual(200, myTrackBar.Width, "#SIZE11");
				AreEqual(default_height, default_height2, myTrackBar.Height, "#SIZE12");
			}

			using (TrackBar myTrackBar = new TrackBar()) {
				using (Form myForm = new Form()) {
					myForm.Controls.Add(myTrackBar);
					myTrackBar.Width = 200;
					myTrackBar.Height = 250;
					myTrackBar.Orientation = Orientation.Vertical;
					handle = myTrackBar.Handle;
					
					AreEqual(default_height, default_height2, myTrackBar.Width, "#SIZE17");
					AreEqual(default_height, default_height2, myTrackBar.Height, "#SIZE18");
				}
			}

			using (TrackBar myTrackBar = new TrackBar()) {
				using (Form myForm = new Form()) {
					myForm.Controls.Add(myTrackBar);
					myTrackBar.Width = 200;
					myTrackBar.Height = 250;
					myTrackBar.Orientation = Orientation.Vertical;
					handle = myTrackBar.Handle;
					
					AreEqual(default_height, default_height2, myTrackBar.Width, "#SIZE19");
					AreEqual(default_height, default_height2, myTrackBar.Height, "#SIZE20");
				}
			}

			using (TrackBar myTrackBar = new TrackBar()) {
				using (Form myForm = new Form()) {
					myForm.Controls.Add(myTrackBar);
					myTrackBar.Width = 200;
					myTrackBar.Height = 250;
					myTrackBar.Orientation = Orientation.Vertical;
					handle = myTrackBar.Handle;
					
					myTrackBar.Orientation = Orientation.Horizontal;
					
					AreEqual(default_height, default_height2, myTrackBar.Width, "#SIZE23");
					AreEqual(default_height, default_height2, myTrackBar.Height, "#SIZE24");
				}
			}

			using (TrackBar myTrackBar = new TrackBar()) {
				myTrackBar.AutoSize = false;
				myTrackBar.Height = 50;
				myTrackBar.Width = 80;
				myTrackBar.Orientation = Orientation.Vertical;
				myTrackBar.Width = 100;
				
				Assert.AreEqual(50, myTrackBar.Height, "#SIZE2_1");
				Assert.AreEqual(100, myTrackBar.Width, "#SIZE2_2");
				
				using (Form myForm = new Form()){
					myForm.Controls.Add(myTrackBar);
					myForm.Show();
					
					Assert.AreEqual(50, myTrackBar.Height, "#SIZE2_3");
					Assert.AreEqual(100, myTrackBar.Width, "#SIZE2_4");
				}
			}

			Assert.AreEqual(handle, handle, "Removes warning");
		}

		[Test]
		public void SizeTest ()
		{
			IntPtr handle;
			int default_height = 45;
			int default_height2 = 42;
			
			using (TrackBar myTrackBar = new TrackBar()) {
				myTrackBar.Width = 200;
				myTrackBar.Height = 250;
				Assert.AreEqual(200, myTrackBar.Width, "#SIZE01");
				AreEqual(default_height, default_height2, myTrackBar.Height, "#SIZE02");
			}
			
			using (TrackBar myTrackBar = new TrackBar()) {
				myTrackBar.AutoSize = false;
				myTrackBar.Width = 200;
				myTrackBar.Height = 250;
				Assert.AreEqual(200, myTrackBar.Width, "#SIZE05");
				Assert.AreEqual(250, myTrackBar.Height, "#SIZE06");
			}

			using (TrackBar myTrackBar = new TrackBar()) {
				myTrackBar.Width = 200;
				myTrackBar.Height = 250;
				myTrackBar.AutoSize = false;
				Assert.AreEqual(200, myTrackBar.Width, "#SIZE09");
				AreEqual(default_height, default_height2, myTrackBar.Height, "#SIZE10");
			}

			using (TrackBar myTrackBar = new TrackBar()) {
				using (Form myForm = new Form()) {
					myForm.Controls.Add(myTrackBar);
					myTrackBar.Width = 200;
					myTrackBar.Height = 250;
					myTrackBar.Orientation = Orientation.Vertical;
					myTrackBar.Orientation = Orientation.Horizontal;
					handle = myTrackBar.Handle;
					
					Assert.AreEqual(200, myTrackBar.Width, "#SIZE21");
					AreEqual(default_height, default_height2, myTrackBar.Height, "#SIZE22");
				}
			}

			Assert.AreEqual(handle, handle, "Removes warning");
		}

		[Test]
		public void EventsTest ()
		{
			TrackBar trackbar = new TrackBar ();
			trackbar.Maximum = 10;
			trackbar.Minimum = 1;
			trackbar.LargeChange = 2;

			trackbar.ValueChanged += new EventHandler (trackbar_ValueChanged);
			trackbar.Scroll += new EventHandler (trackbar_Scroll);

			trackbar.Value = 6;
			Assert.AreEqual ("ValueChanged=6;", event_log, "#A1");
		}

		string event_log;

		void trackbar_Scroll (object sender, EventArgs e)
		{
			event_log += "Scroll=" + ((TrackBar)sender).Value + ";";
		}

		void trackbar_ValueChanged (object sender, EventArgs e)
		{
			event_log += "ValueChanged=" + ((TrackBar)sender).Value + ";";
		}
	}
}

