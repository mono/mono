//
// GroupBoxTest.cs: Test cases for GroupBox.
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
	public class GroupBoxTest
	{
		[Test]
		public void Constructor ()
		{
			GroupBox gb = new GroupBox ();

			Assert.AreEqual (false, gb.AllowDrop, "A1");
			// Top/Height are dependent on font height
			// Assert.AreEqual (new Rectangle (3, 16, 194, 81), gb.DisplayRectangle, "A2");
			Assert.AreEqual (FlatStyle.Standard, gb.FlatStyle, "A3");
			Assert.AreEqual (false, gb.TabStop, "A4");
			Assert.AreEqual (string.Empty, gb.Text, "A5");
			
#if NET_2_0
			Assert.AreEqual (false, gb.AutoSize, "A6");
			//Assert.AreEqual (AutoSizeMode.GrowOnly, gb.AutoSizeMode, "A7");
			Assert.AreEqual (true, gb.UseCompatibleTextRendering, "A8");
#endif
		}
		
		[Test]
		public void GroupBoxPropertyTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			GroupBox mygrpbox = new GroupBox ();
			RadioButton myradiobutton1 = new RadioButton ();
			RadioButton myradiobutton2 = new RadioButton ();
			mygrpbox.Controls.Add (myradiobutton1);
			mygrpbox.Controls.Add (myradiobutton2);
			myform.Show ();
			Assert.AreEqual (FlatStyle.Standard, mygrpbox.FlatStyle, "#1");
			mygrpbox.FlatStyle = FlatStyle.Popup;
			Assert.AreEqual (FlatStyle.Popup, mygrpbox.FlatStyle, "#2");
			mygrpbox.FlatStyle = FlatStyle.Flat;
			Assert.AreEqual (FlatStyle.Flat, mygrpbox.FlatStyle, "#3");
			mygrpbox.FlatStyle = FlatStyle.System;
			Assert.AreEqual (FlatStyle.System, mygrpbox.FlatStyle, "#4");
			myform.Dispose ();
		}

#if NET_2_0
		[Test]
		public void PropertyDisplayRectangle ()
		{
			GroupBox gb = new GroupBox ();
			gb.Size = new Size (200, 200);
			
			Assert.AreEqual (new Padding (3), gb.Padding, "A0");
			gb.Padding = new Padding (25, 25, 25, 25);

			Assert.AreEqual (new Rectangle (0, 0, 200, 200), gb.ClientRectangle, "A1");

			// Basically, we are testing that the DisplayRectangle includes
			// Padding.  Top/Height are affected by font height, so we aren't
			// using exact numbers.
			Assert.AreEqual (25, gb.DisplayRectangle.Left, "A2");
			Assert.AreEqual (150, gb.DisplayRectangle.Width, "A3");
			Assert.IsTrue (gb.DisplayRectangle.Top > gb.Padding.Top, "A4");
			Assert.IsTrue (gb.DisplayRectangle.Height < (gb.Height - gb.Padding.Vertical), "A5");
		}
#endif
	}
}
