//
// ToolBarTest.cs: Test cases for ToolBar.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolBarTest : TestHelper 
	{
		[Test] // bug #79863
		public void TabStop ()
		{
			ToolBar tb = new ToolBar ();
			Assert.IsFalse (tb.TabStop);
		}

		[Test]
		public void ToolBarPropertyTest ()
		{	
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			ToolBar myToolBar = new ToolBar ();
			ToolBarButton myToolBarButton1 = new ToolBarButton ();
			ToolBarButton myToolBarButton2 = new ToolBarButton ();
			myToolBarButton1.Text = "A";
			myToolBarButton2.Text = "B";
			myToolBar.Buttons.Add (myToolBarButton1);
			myToolBar.Buttons.Add (myToolBarButton2);
			myform.Controls.Add (myToolBar);
			
			// A
			Assert.AreEqual (ToolBarAppearance.Normal, myToolBar.Appearance, "#A1");
			Assert.AreEqual (true, myToolBar.AutoSize, "#A2");
			
			// B
			Assert.AreEqual ("Control", myToolBar.BackColor.Name, "#B1");
			myToolBar.BackgroundImage = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"));
			Assert.AreEqual (60, myToolBar.BackgroundImage.Height, "#B3");
			Assert.AreEqual (BorderStyle.None, myToolBar.BorderStyle, "#B4");
			myToolBar.BorderStyle = BorderStyle.Fixed3D;
			Assert.AreEqual (BorderStyle.Fixed3D, myToolBar.BorderStyle, "#B5"); 
			Assert.AreEqual (2, myToolBar.Buttons.Count, "#B6");
			Assert.AreEqual ("B", myToolBar.Buttons [1].Text, "#B7");
			
			// D
			Assert.AreEqual (DockStyle.Top, myToolBar.Dock, "#D1");
			Assert.AreEqual (true, myToolBar.Divider, "#D2");	
			Assert.AreEqual (true, myToolBar.DropDownArrows, "#D3");	

			// F
			Assert.AreEqual ("ControlText", myToolBar.ForeColor.Name, "#F2");
	
			// I
			ImageList myImageList = new ImageList ();
			myImageList.Images.Add (Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif")));
			myToolBar.ImageList = myImageList;
			Assert.AreEqual (1, myToolBar.ImageList.Images.Count, "#I1");
			Assert.AreEqual (16, myToolBar.ImageSize.Height, "#I2");
			Assert.AreEqual (16, myToolBar.ImageSize.Width, "#I3");
			Assert.AreEqual (ImeMode.Disable, myToolBar.ImeMode, "#I4");
			
			// R
			Assert.AreEqual (RightToLeft.No, myToolBar.RightToLeft, "#R1");

			// S
			Assert.AreEqual (true, myToolBar.ShowToolTips, "#S1");
			
			// T
			Assert.AreEqual ("", myToolBar.Text, "#T1");
			myToolBar.Text = "MONO TOOLBAR";
			Assert.AreEqual ("MONO TOOLBAR", myToolBar.Text, "#T2");
			Assert.AreEqual (ToolBarTextAlign.Underneath, myToolBar.TextAlign, "#T3");

			// WXYZ
			Assert.AreEqual (true, myToolBar.Wrappable, "#W1");

			myform.Dispose ();
		}
		
		[Test]
		public void ToStringMethodTest () 
		{
			ToolBar tb = new ToolBar ();
			tb.Text = "New ToolBar";
			Assert.AreEqual ("System.Windows.Forms.ToolBar, Buttons.Count: 0",
				tb.ToString (), "#1");

			ToolBarButton buttonA = new ToolBarButton ("A");
			ToolBarButton buttonB = new ToolBarButton ("B");
			tb.Buttons.Add (buttonA);
			tb.Buttons.Add (buttonB);
			Assert.AreEqual ("System.Windows.Forms.ToolBar, Buttons.Count: 2, " +
				"Buttons[0]: ToolBarButton: A, Style: PushButton", 
				tb.ToString (), "#2");
		}

		[Test]
		public void ToolbarButtonRectangleTest ()
		{
			ToolBar myToolBar = new ToolBar ();
			ToolBarButton tbb = new ToolBarButton ("hi");

			Assert.IsTrue (tbb.Rectangle.IsEmpty, "#T0");

			myToolBar.Visible = false;
			myToolBar.Buttons.Add (tbb);

			Assert.IsFalse (tbb.Rectangle.IsEmpty, "#T1");

			myToolBar.Visible = true;

			Assert.IsFalse (tbb.Rectangle.IsEmpty, "#T2");

			tbb.Visible = false;

			Assert.IsTrue (tbb.Rectangle.IsEmpty, "#T3");
		}

		[Test] // bug #80416
		public void DockDefault ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;

			ToolBar toolBar = new ToolBar ();
			form.Controls.Add (toolBar);
			form.Show ();
			Assert.AreEqual (DockStyle.Top, toolBar.Dock, "#1");
			Assert.AreEqual (form.ClientSize.Width, toolBar.Width, "#2");
			
			form.Close ();
		}
		
		[Test]
		public void ButtonSizeTest ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;

			ToolBar toolBar = new ToolBar ();
			
			// Size is fixed when dont have buttons
			Assert.AreEqual (39, toolBar.ButtonSize.Width, "#1");
			Assert.AreEqual (36, toolBar.ButtonSize.Height, "#2");

			toolBar.Buttons.Add (new ToolBarButton (""));
			form.Controls.Add (toolBar);
			form.Show ();

			// We cannot determine exact size as it depends of font size
			//Assert.IsTrue (toolBar.ButtonSize.Width < 39, "#3");
			//Assert.IsTrue (toolBar.ButtonSize.Height < 36, "#4");
			
			toolBar.Buttons.Add (new ToolBarButton ("Teste"));
			
			// We cannot determine exact size as it depends of font size
			//Assert.IsTrue (toolBar.ButtonSize.Width >= 39, "#5");
			//Assert.IsTrue (toolBar.ButtonSize.Height >= 36, "#6");
			
			form.Close ();
		}

		[Test]
		public void CreateHandleTest ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Show ();

			ToolBar toolbar = new ToolBar ();
			Assert.IsFalse (toolbar.IsHandleCreated, "#1");
			
			toolbar.Buttons.Add (new ToolBarButton (""));
			Assert.IsFalse (toolbar.IsHandleCreated, "#2");
			
			form.Controls.Add (toolbar);
			Assert.IsTrue (toolbar.IsHandleCreated, "#3");
			
			form.Close ();
		}

		[Test]
		public void HorizontalSizeTest ()
		{
			// Test aproximated sizes (> 30 and < 30) because it depends of font size. 
			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Show ();
			
			ToolBar toolbar = new ToolBar ();
			Assert.IsTrue (toolbar.Size.Height > 30, "#1");

			toolbar.Buttons.Add (new ToolBarButton (""));
			Assert.IsTrue (toolbar.Size.Height > 30, "#2");

			form.Controls.Add (toolbar);
			Assert.IsTrue (toolbar.Size.Height < 30, "#3");

			// TODO: Notworking at moment.
			//toolbar.Buttons.Add (new ToolBarButton ("Test"));
			//Assert.IsTrue (toolbar.Size.Height > 30, "#4");
			
			form.Close ();
		}

		[Test]
		public void VerticalSizeTest ()
		{
			// Test aproximated sizes (> 30 and < 30) because it depends of font size. 
			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Show ();
			
			ToolBar toolbar = new ToolBar ();
			toolbar.Dock = DockStyle.Left;
			Assert.IsTrue (toolbar.Size.Width > 30, "#1");

			toolbar.Buttons.Add (new ToolBarButton (""));
			Assert.IsTrue (toolbar.Size.Width > 30, "#2");

			form.Controls.Add (toolbar);
			Assert.IsTrue (toolbar.Size.Width < 30, "#3");

			// TODO: Notworking at moment.
			//toolbar.Buttons.Add (new ToolBarButton ("Test"));
			//Assert.IsTrue (toolbar.Size.Width > 30, "#4");
			
			form.Close ();
		}

	}

	// [MonoTODO ("Add test for ButtonClickEvent (Visual Test)"]
	// [MonoTODO ("Add test for ButtonDropDownEvent (Visual Test)"]
}
