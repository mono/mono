//
// ProgressBarTest.cs: Test cases for ProgressBar.
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
	public class ProgressBarTest
	{
		[Test]
		public void ProgressBarPropertyTest ()
		{
			ProgressBar myProgressBar = new ProgressBar ();
			
			// A
			Assert.AreEqual (false, myProgressBar.AllowDrop, "#A1");
			
			// B
			Assert.AreEqual ("Control", myProgressBar.BackColor.Name, "#B1");
			Assert.AreEqual (null, myProgressBar.BackgroundImage, "#B3");
			string gif = "M.gif";
			myProgressBar.BackgroundImage = Image.FromFile (gif);
			// comparing image objects fails on MS .Net so using Size property
			Assert.AreEqual (Image.FromFile(gif, true).Size, myProgressBar.BackgroundImage.Size, "#B4");
			
			// F 
			Assert.AreEqual (FontStyle.Regular, myProgressBar.Font.Style, "#F2");
			
			// M
			Assert.AreEqual (100, myProgressBar.Maximum, "#M1");
			Assert.AreEqual (0, myProgressBar.Minimum, "#M2");
			
			// R
			Assert.AreEqual (RightToLeft.No, myProgressBar.RightToLeft, "#R1");
						
			// S
			Assert.AreEqual (10, myProgressBar.Step, "#S1");

			// T
			Assert.AreEqual ("", myProgressBar.Text, "#T1");
			myProgressBar.Text = "New ProgressBar";
			Assert.AreEqual ("New ProgressBar", myProgressBar.Text, "#T2");

			// V
			Assert.AreEqual (0, myProgressBar.Value, "#V1");
		}

		[Test]
		public void ForeColorTest ()
		{
			ProgressBar progressBar = new ProgressBar ();
#if NET_2_0
			Assert.AreEqual (SystemColors.Highlight, progressBar.ForeColor, "#A1");
#else
			Assert.AreEqual (SystemColors.ControlText, progressBar.ForeColor, "#A1");
#endif
			progressBar.ForeColor = Color.Red;
			Assert.AreEqual (Color.Red, progressBar.ForeColor, "#A2");
			progressBar.ForeColor = Color.White;
			Assert.AreEqual (Color.White, progressBar.ForeColor, "#A3");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (progressBar);
			form.Show ();

			Assert.AreEqual (Color.White, progressBar.ForeColor, "#B1");
			progressBar.ForeColor = Color.Red;
			Assert.AreEqual (Color.Red, progressBar.ForeColor, "#B2");
			progressBar.ForeColor = Color.Red;
			Assert.AreEqual (Color.Red, progressBar.ForeColor, "#B3");
			progressBar.ForeColor = Color.Blue;
			Assert.AreEqual (Color.Blue, progressBar.ForeColor, "#B4");
			
			form.Close ();
		}

		[Test]
		public void ResetForeColor ()
		{
			ProgressBar progressBar = new ProgressBar ();
			progressBar.ForeColor = Color.Red;
			progressBar.ResetForeColor ();
#if NET_2_0
			Assert.AreEqual (SystemColors.Highlight, progressBar.ForeColor);
#else
			Assert.AreEqual (SystemColors.ControlText, progressBar.ForeColor);
#endif
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void ValueTest ()
		{
			ProgressBar myProgressBar = new ProgressBar ();
			myProgressBar.Value = -1;
			myProgressBar.Value = 100;
		}
	
		[Test]
		public void ToStringMethodTest () 
		{
			ProgressBar myProgressBar = new ProgressBar ();
			myProgressBar.Text = "New ProgressBar";
			Assert.AreEqual ("System.Windows.Forms.ProgressBar, Minimum: 0, Maximum: 100, Value: 0", myProgressBar.ToString (), "#T3");
		}
		// [MonoTODO("Add test for method Increment (Visual Test)")]
		// [MonoTODO("Add test for method PerformStep (Visual Test)")]
	}
}

