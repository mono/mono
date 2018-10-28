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

using MonoTests.Helpers;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ProgressBarTest : TestHelper
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
			string gif = TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif");
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
			Assert.AreEqual (SystemColors.Highlight, progressBar.ForeColor, "#A1");
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
			Assert.AreEqual (SystemColors.Highlight, progressBar.ForeColor);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ValueTest ()
		{
			ProgressBar myProgressBar = new ProgressBar ();
			myProgressBar.Value = -1;
			myProgressBar.Value = 100;
		}

		[Test]
		public void MinMax()
		{
			Type expectedArgExType;
			expectedArgExType = typeof (ArgumentOutOfRangeException);
			//
			ProgressBar c = new ProgressBar ();
			Assert.AreEqual (0, c.Minimum, "default_min");
			Assert.AreEqual (100, c.Maximum, "default_max");
			Assert.AreEqual (0, c.Value, "default_value");
			//----
			try {
				c.Minimum = -1;
				Assert.Fail ("should have thrown -- Min-1");
			} catch (ArgumentException ex) {
				// MSDN says ArgumentException, but really its *subtype* ArgumentOutOfRangeException.
				// Actually it changed in FX2.
				Assert.AreEqual (expectedArgExType, ex.GetType (), "Typeof Min-1");
				Assert.AreEqual ("Minimum", ex.ParamName, "ParamName Min-1"); // (culture insensitive).
			}
			try {
				c.Maximum = -1;
				Assert.Fail ("should have thrown -- Max-1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (expectedArgExType, ex.GetType (), "Typeof Max-1");
				Assert.AreEqual ("Maximum", ex.ParamName, "ParamName Max-1"); // (culture insensitive).
			}
			Assert.AreEqual (0, c.Minimum, "after Min/Max-1_min");
			Assert.AreEqual (100, c.Maximum, "after Min/Max-1_max");
			Assert.AreEqual (0, c.Value, "after Min/Max-1_value");
			//
			// What happens when Min/Max is set respectively above/below the current Value
			// and Max/Min values.
			c.Minimum = 200;
			Assert.AreEqual (200, c.Minimum, "200L_min");
			Assert.AreEqual (200, c.Maximum, "200L_max");
			Assert.AreEqual (200, c.Value, "200L_value");
			//
			c.Minimum = 50;
			Assert.AreEqual (50, c.Minimum, "50L_min");
			Assert.AreEqual (200, c.Maximum, "50L_max");
			Assert.AreEqual (200, c.Value, "50L_value");
			//
			c.Maximum = 30;
			Assert.AreEqual (30, c.Minimum, "30T_min");
			Assert.AreEqual (30, c.Maximum, "30T_max");
			Assert.AreEqual (30, c.Value, "30T_value");
			//
			// What happens when Value is set outside the Min/Max ranges.
			c.Maximum = 50;
			Assert.AreEqual (30, c.Minimum, "50T_min");
			Assert.AreEqual (50, c.Maximum, "50T_max");
			c.Value = 45;
			Assert.AreEqual (45, c.Value, "50T_value");
			try {
				c.Value = 29;
				Assert.Fail ("should have thrown -- 29");
			} catch (ArgumentException ex) {
				Assert.AreEqual (expectedArgExType, ex.GetType (), "Typeof 29");
				Assert.AreEqual ("Value", ex.ParamName, "ParamName 29");
			}
			Assert.AreEqual (45, c.Value, "after 29_value");
			try {
				c.Value = 51;
				Assert.Fail ("should have thrown -- 51");
			} catch (ArgumentException ex) {
				Assert.AreEqual (expectedArgExType, ex.GetType (), "Typeof 51");
				Assert.AreEqual ("Value", ex.ParamName, "ParamName 151");
			}
			Assert.AreEqual (45, c.Value, "after 51_value");
		}

		[Test]
		public void PerformStepAndIncrement ()
		{
			ProgressBar c = new ProgressBar ();
			//
			c.Value = 10;
			c.Step = 30;
			Assert.AreEqual (10, c.Value, "StepAt30_Init");
			c.PerformStep ();
			Assert.AreEqual (40, c.Value, "StepAt30_1");
			c.PerformStep ();
			Assert.AreEqual (70, c.Value, "StepAt30_2");
			//
			c.Value = 0;
			c.Step = 20;
			Assert.AreEqual (0, c.Value, "StepAt20_Init");
			//
			c.PerformStep ();
			Assert.AreEqual (20, c.Value, "StepAt20_1");
			c.PerformStep ();
			Assert.AreEqual (40, c.Value, "StepAt20_2");
			c.PerformStep ();
			Assert.AreEqual (60, c.Value, "StepAt20_3");
			c.PerformStep ();
			Assert.AreEqual (80, c.Value, "StepAt20_4");
			c.PerformStep ();
			Assert.AreEqual (100, c.Value, "StepAt20_5");
			c.PerformStep ();
			Assert.AreEqual (100, c.Value, "StepAt20_6x");
			c.PerformStep ();
			Assert.AreEqual (100, c.Value, "StepAt20_7x");
			//
			c.Step = -20;
			Assert.AreEqual (100, c.Value, "StepAt2Neg0_Init");
			c.PerformStep ();
			Assert.AreEqual (80, c.Value, "StepAtNeg20_1");
			c.PerformStep ();
			Assert.AreEqual (60, c.Value, "StepAtNeg20_2");
			//
			c.Step = -40;
			Assert.AreEqual (60, c.Value, "StepAt2Neg40_Init");
			c.PerformStep ();
			Assert.AreEqual (20, c.Value, "StepAtNeg40_1");
			c.PerformStep ();
			Assert.AreEqual (0, c.Value, "StepAtNeg40_2");
			c.PerformStep ();
			Assert.AreEqual (0, c.Value, "StepAtNeg40_2");
			//
			c.Increment (30);
			Assert.AreEqual (30, c.Value, "Increment30_1");
			c.Increment (30);
			Assert.AreEqual (60, c.Value, "Increment30_2");
			c.Increment (30);
			Assert.AreEqual (90, c.Value, "Increment30_3");
			c.Increment (30);
			Assert.AreEqual (100, c.Value, "Increment30_4x");
		}

		[Test]
		public void Styles ()
		{
			ProgressBar c = new ProgressBar ();
			//--
			Assert.AreEqual(ProgressBarStyle.Blocks, c.Style, "orig=blocks");
			//--
			c.Style = ProgressBarStyle.Continuous;
			//--
			c.Style = ProgressBarStyle.Marquee;
			// Increment and PerformStep are documented to fail in Marquee style.
			try {
				c.Increment (5);
				Assert.Fail ("should have thrown -- Increment");
			} catch (InvalidOperationException) {
			}
			try {
				c.PerformStep ();
				Assert.Fail ("should have thrown -- PerformStep ");
			} catch (InvalidOperationException) {
			}
			// What about the other value-related properties?  No fail apparently!
			c.Value = 20;
			c.Minimum = 5;
			c.Maximum = 95;
			//--
			// Now undefined style values...
			try {
				c.Style = (ProgressBarStyle)4;
				Assert.Fail("should have thrown -- bad style4");
			} catch (global::System.ComponentModel.InvalidEnumArgumentException ex) {
				//Console.WriteLine(ex.Message);
				Assert.AreEqual(typeof(global::System.ComponentModel.InvalidEnumArgumentException), ex.GetType (), "Typeof bad style4");
				Assert.AreEqual("value", ex.ParamName, "ParamName bad style 4");
			}
			try {
				c.Style = (ProgressBarStyle)99;
				Assert.Fail("should have thrown -- bad style99");
			} catch (global::System.ComponentModel.InvalidEnumArgumentException ex) {
				Assert.AreEqual (typeof(global::System.ComponentModel.InvalidEnumArgumentException), ex.GetType (), "Typeof bad style99");
				Assert.AreEqual ("value", ex.ParamName, "ParamName bad style 99");
			}
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

