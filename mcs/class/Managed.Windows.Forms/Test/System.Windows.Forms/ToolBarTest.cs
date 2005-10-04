//
// ToolBarTest.cs: Test cases for ToolBar.
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
	[Ignore ("This test has to be completly reviewed")]
	public class ToolBarTest 
	{

		[Test]
		public void ToolBarPropertyTest ()
		{	
			Form myform = new Form ();
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
			myToolBar.BackColor = Color.Red;
			Assert.AreEqual (255, myToolBar.BackColor.R, "#B2");
			myToolBar.BackgroundImage = Image.FromFile ("M.gif");
			Assert.AreEqual (60, myToolBar.BackgroundImage.Height, "#B3");
			Assert.AreEqual (BorderStyle.None, myToolBar.BorderStyle, "#B4");
			myToolBar.BorderStyle = BorderStyle.Fixed3D;
			Assert.AreEqual (BorderStyle.Fixed3D, myToolBar.BorderStyle, "#B5"); 
			Assert.AreEqual (2, myToolBar.Buttons.Count, "#B6");
			Assert.AreEqual ("B", myToolBar.Buttons [1].Text, "#B7");
			Assert.AreEqual (39, myToolBar.ButtonSize.Width, "#B8");
			Assert.AreEqual (36, myToolBar.ButtonSize.Height, "#B9");
			
			// D
			Assert.AreEqual (DockStyle.Top, myToolBar.Dock, "#D1");
			Assert.AreEqual (true, myToolBar.Divider, "#D2");	
			Assert.AreEqual (true, myToolBar.DropDownArrows, "#D3");	

			// F
			Assert.AreEqual ("ControlText", myToolBar.ForeColor.Name, "#F2");
	
			// I
			ImageList myImageList = new ImageList ();
			myImageList.Images.Add (Image.FromFile ("M.gif"));
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
		}
		
		[Test]
		public void ToStringMethodTest () 
		{
			ToolBar myToolBar = new ToolBar ();
			myToolBar.Text = "New ToolBar";
			Assert.AreEqual ("System.Windows.Forms.ToolBar, Buttons.Count: 0", myToolBar.ToString (), "#T3");
		}
      	}
	// [MonoTODO ("Add test for ButtonClickEvent (Visual Test)"]
	// [MonoTODO ("Add test for ButtonDropDownEvent (Visual Test)"]
}
