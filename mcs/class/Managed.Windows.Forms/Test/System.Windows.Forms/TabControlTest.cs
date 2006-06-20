//
// TabControlTest.cs: Test cases for TabControl.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;




namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class TabControlTest
	{
		private class TabControlPoker : TabControl {

			public bool CheckIsInputKey (Keys key)
			{
				return IsInputKey (key);
			}

			protected override void WndProc (ref Message m)
			{
				base.WndProc (ref m);
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void TabControlPropertyTest ()
		{
			Form myForm = new Form ();
			TabControl myTabControl = new TabControl ();
			myTabControl.Visible = true;
			myTabControl.Name = "Mono TabControl";
		
			// A 
			Assert.AreEqual (TabAlignment.Top, myTabControl.Alignment, "A1");
			Assert.AreEqual (TabAppearance.Normal, myTabControl.Appearance, "#A2");
		
			// D 
			Assert.AreEqual (4, myTabControl.DisplayRectangle.X, "#D1");
			Assert.AreEqual (4, myTabControl.DisplayRectangle.Y, "#D2");
			Assert.AreEqual (192, myTabControl.DisplayRectangle.Width, "#D3");
			Assert.AreEqual (92, myTabControl.DisplayRectangle.Height, "#D4");
			Assert.AreEqual (TabDrawMode.Normal, myTabControl.DrawMode, "#D5");
		
			// H
			Assert.AreEqual (false, myTabControl.HotTrack, "#H1");
		
			// I 
			Assert.AreEqual (null, myTabControl.ImageList, "#I1");
			// It is environment dependent
			//Assert.AreEqual (18, myTabControl.ItemSize.Height, "#I2");
			Assert.AreEqual (0, myTabControl.ItemSize.Width, "#I3");

			// M 
			Assert.AreEqual (false, myTabControl.Multiline, "#M1");
		
			// P
			Assert.AreEqual (6, myTabControl.Padding.X, "#P1");
			Assert.AreEqual (3, myTabControl.Padding.Y, "#P1");

			// R
			Assert.AreEqual (0, myTabControl.RowCount, "#R1");

			// S
			Assert.AreEqual (-1, myTabControl.SelectedIndex, "#S1");
			Assert.AreEqual (null, myTabControl.SelectedTab, "#S2");
			Assert.AreEqual (false, myTabControl.ShowToolTips, "#S3");
			Assert.AreEqual (TabSizeMode.Normal, myTabControl.SizeMode, "#S4");

			// T
			Assert.AreEqual (0, myTabControl.TabCount, "#T1");
			Assert.AreEqual (0, myTabControl.TabPages.Count, "#T2");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetTabRectTest ()
		{
			TabControl myTabControl = new TabControl ();
			TabPage myTabPage = new TabPage();
			myTabControl.Controls.Add(myTabPage);
			myTabPage.TabIndex = 0;
			Rectangle myTabRect = myTabControl.GetTabRect (0);
			Assert.AreEqual (2, myTabRect.X, "#GetT1");
			Assert.AreEqual (2, myTabRect.Y, "#GetT2");
			Assert.AreEqual (42, myTabRect.Width, "#GetT3");
			// It is environment dependent
			//Assert.AreEqual (18, myTabRect.Height, "#GetT4");
		}

		[Test]
		public void ToStringTest ()
		{
			TabControl myTabControl = new TabControl ();
			Assert.AreEqual ("System.Windows.Forms.TabControl, TabPages.Count: 0", myTabControl.ToString(), "#1");
		}

		[Test]
		public void ClearTabPagesTest ()
		{
			// no tab pages
			TabControl tab = new TabControl ();
			tab.TabPages.Clear ();
			Assert.AreEqual (-1, tab.SelectedIndex, "#A1");
			Assert.AreEqual (0, tab.TabPages.Count, "#A2");

			// single tab page
			tab.Controls.Add (new TabPage ());
			Assert.AreEqual (0, tab.SelectedIndex, "#B1");
			Assert.AreEqual (1, tab.TabPages.Count, "#B2");
			tab.TabPages.Clear();
			Assert.AreEqual (-1, tab.SelectedIndex, "#B3");
			Assert.AreEqual (0, tab.TabPages.Count, "#B4");

			// multiple tab pages
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			Assert.AreEqual (0, tab.SelectedIndex, "#C1");
			Assert.AreEqual (3, tab.TabPages.Count, "#C2");
			tab.SelectedIndex = 1;
			tab.TabPages.Clear ();
			Assert.AreEqual (-1, tab.SelectedIndex, "#C3");
			Assert.AreEqual (0, tab.TabPages.Count, "#C4");
		}

		[Test]
		public void SetSelectedIndex ()
		{
			// bug #78395
			TabControl c = new TabControl ();
			c.SelectedIndex = 0;

			c.TabPages.Add (new TabPage ());
			c.TabPages.Add (new TabPage ());
			Assert.AreEqual (0, c.SelectedIndex, "#1");
			Form f = new Form ();
			f.Controls.Add (c);
			f.Show ();
			c.SelectedIndex = 2; // beyond the pages - ignored
			Assert.AreEqual (0, c.SelectedIndex, "#2");
		}

		[Test]
		public void InputKeyTest ()
		{
			TabControlPoker p = new TabControlPoker ();

			foreach (Keys key in Enum.GetValues (typeof (Keys))) {
				switch (key) {
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.End:
				case Keys.Home:
					continue;
				}
				Assert.IsFalse (p.CheckIsInputKey (key), "FALSE- " + key);
			}

			Assert.IsTrue (p.CheckIsInputKey (Keys.PageUp), "TRUE-pageup");
			Assert.IsTrue (p.CheckIsInputKey (Keys.PageDown), "TRUE-pagedown");
			Assert.IsTrue (p.CheckIsInputKey (Keys.End), "TRUE-end");
			Assert.IsTrue (p.CheckIsInputKey (Keys.Home), "TRUE-home");

			// Create the handle, things are a little different with
			// the handle created
			IntPtr dummy = p.Handle;

			foreach (Keys key in Enum.GetValues (typeof (Keys))) {
				switch (key) {
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.End:
				case Keys.Home:
					continue;
				}
				Assert.IsFalse (p.CheckIsInputKey (key), "PH-FALSE- " + key);
			}

			Assert.IsTrue (p.CheckIsInputKey (Keys.Left), "PH-TRUE-left");
			Assert.IsTrue (p.CheckIsInputKey (Keys.Right), "PH-TRUE-right");
			Assert.IsTrue (p.CheckIsInputKey (Keys.Up), "PH-TRUE-up");
			Assert.IsTrue (p.CheckIsInputKey (Keys.Down), "PH-TRUE-down");
			Assert.IsTrue (p.CheckIsInputKey (Keys.PageUp), "PH-TRUE-pageup");
			Assert.IsTrue (p.CheckIsInputKey (Keys.PageDown), "PH-TRUE-pagedown");
			Assert.IsTrue (p.CheckIsInputKey (Keys.End), "PH-TRUE-end");
			Assert.IsTrue (p.CheckIsInputKey (Keys.Home), "PH-TRUE-home");
		}
	}

}
