//
// ToolStripMenuItemTest.cs
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
#if NET_2_0
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolStripMenuItemTest : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripMenuItem tsi = new ToolStripMenuItem ();

			Assert.AreEqual (false, tsi.Checked, "A1");
			Assert.AreEqual (false, tsi.CheckOnClick, "A2");
			Assert.AreEqual (CheckState.Unchecked, tsi.CheckState, "A3");
			Assert.AreEqual (true, tsi.Enabled, "A4");
			Assert.AreEqual (false, tsi.IsMdiWindowListEntry, "A5");
			Assert.AreEqual (ToolStripItemOverflow.Never, tsi.Overflow, "A6");
			Assert.AreEqual (null, tsi.ShortcutKeyDisplayString, "A7");
			Assert.AreEqual (Keys.None, tsi.ShortcutKeys, "A8");
			Assert.AreEqual (true, tsi.ShowShortcutKeys, "A9");
			Assert.AreEqual (SystemColors.ControlText, tsi.ForeColor, "A9-1");
			
			Assert.AreEqual ("System.Windows.Forms.ToolStripMenuItem+ToolStripMenuItemAccessibleObject", tsi.AccessibilityObject.GetType ().ToString (), "A10");
			int count = 0;
			EventHandler oc = new EventHandler (delegate (object sender, EventArgs e) { count++; });
			Image i = new Bitmap (1, 1);

			tsi = new ToolStripMenuItem (i);
			tsi.PerformClick ();
			Assert.AreEqual (null, tsi.Text, "A10-1");
			Assert.AreSame (i, tsi.Image, "A10-2");
			Assert.AreEqual (0, count, "A10-3");
			Assert.AreEqual (string.Empty, tsi.Name, "A10-4");

			tsi = new ToolStripMenuItem ("Text");
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A10-5");
			Assert.AreSame (null, tsi.Image, "A11");
			Assert.AreEqual (0, count, "A12");
			Assert.AreEqual (string.Empty, tsi.Name, "A13");

			tsi = new ToolStripMenuItem ("Text", i);
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A14");
			Assert.AreSame (i, tsi.Image, "A15");
			Assert.AreEqual (0, count, "A16");
			Assert.AreEqual (string.Empty, tsi.Name, "A17");

			tsi = new ToolStripMenuItem ("Text", i, oc);
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A18");
			Assert.AreSame (i, tsi.Image, "A19");
			Assert.AreEqual (1, count, "A20");
			Assert.AreEqual (string.Empty, tsi.Name, "A21");

			tsi = new ToolStripMenuItem ("Text", i, oc, "Name");
			tsi.PerformClick ();
			Assert.AreEqual ("Text", tsi.Text, "A22");
			Assert.AreSame (i, tsi.Image, "A23");
			Assert.AreEqual (2, count, "A24");
			Assert.AreEqual ("Name", tsi.Name, "A25");
		}

		[Test]
		public void BehaviorKeyboardShortcuts ()
		{
			ExposeProtectedMethods tsmi = new ExposeProtectedMethods ();
			tsmi.ShortcutKeys = Keys.Control | Keys.D;

			Message m = new Message ();
			Assert.AreEqual (false, tsmi.PublicProcessCmdKey (ref m, Keys.D), "A1");
			Assert.AreEqual (false, tsmi.PublicProcessCmdKey (ref m, Keys.Control), "A2");
			Assert.AreEqual (true, tsmi.PublicProcessCmdKey (ref m, Keys.Control | Keys.D), "A3");
			Assert.AreEqual (false, tsmi.PublicProcessCmdKey (ref m, Keys.A), "A4");
			Assert.AreEqual (false, tsmi.PublicProcessCmdKey (ref m, Keys.Control | Keys.A), "A5");
			
			tsmi.ShowShortcutKeys = false;
			Assert.AreEqual (true, tsmi.PublicProcessCmdKey (ref m, Keys.Control | Keys.D), "A6");
			
			tsmi.ShortcutKeyDisplayString = "Moose";
			Assert.AreEqual (true, tsmi.PublicProcessCmdKey (ref m, Keys.Control | Keys.D), "A7");
		}
		
		[Test]
		public void BehaviorMdiWindowMenuItem ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.IsMdiContainer = true;
			Form c1 = new Form ();
			c1.MdiParent = f;
			Form c2 = new Form ();
			c2.MdiParent = f;				
		
			MenuStrip ms = new MenuStrip ();
			ToolStripMenuItem tsmi = (ToolStripMenuItem)ms.Items.Add ("Window");
			f.Controls.Add (ms);
			ms.MdiWindowListItem = tsmi;
			
			f.MainMenuStrip = ms;

			c1.Show ();
			f.Show ();
			Assert.AreEqual (true, (tsmi.DropDownItems[0] as ToolStripMenuItem).IsMdiWindowListEntry, "R1");
			
			f.Close ();
		}
		
		[Test]
		public void BehaviorShortcutText ()
		{
			ToolStripMenuItem tsmi = new ToolStripMenuItem ();
			
			tsmi.ShortcutKeys = Keys.Control | Keys.O;
			
			Assert.AreEqual (null, tsmi.ShortcutKeyDisplayString, "A1");
			
			tsmi.ShortcutKeyDisplayString = "Test String";
			Assert.AreEqual ("Test String", tsmi.ShortcutKeyDisplayString, "A2");

			tsmi.ShortcutKeys = Keys.Control | Keys.P;
			Assert.AreEqual ("Test String", tsmi.ShortcutKeyDisplayString, "A3");

			tsmi.ShortcutKeyDisplayString = string.Empty;
			Assert.AreEqual (string.Empty, tsmi.ShortcutKeyDisplayString, "A4");

			tsmi.ShortcutKeyDisplayString = null;
			Assert.AreEqual (null, tsmi.ShortcutKeyDisplayString, "A5");
		}
		
		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void GetCurrentParent ()
		{
			ToolStripMenuItem tsmiFile = new ToolStripMenuItem ("File");
			ToolStripMenuItem tsmiHelp = new ToolStripMenuItem ("Help");
			ToolStripMenuItem tsmiQuit = new ToolStripMenuItem ("Quit");
			ToolStripMenuItem tsmiAbout = new ToolStripMenuItem ("About");
			tsmiFile.DropDownItems.Add (tsmiQuit);
			tsmiHelp.DropDownItems.Add (tsmiAbout);
			MenuStrip menu = new MenuStrip ();
			menu.Items.Add (tsmiFile);
			menu.Items.Add (tsmiHelp);
			
			ToolStrip parent = tsmiFile.GetCurrentParent ();
			Assert.IsNotNull (parent, "A1");
			Assert.AreEqual (parent.GetType ().Name, typeof (MenuStrip).Name, "A2");
			Assert.AreEqual (parent, menu, "A3");
			
			//because it's not visible?:
			Assert.IsNull (tsmiQuit.GetCurrentParent (), "A4");
		}
		
		[Test]
		public void Owner ()
		{
			ToolStripMenuItem tsmiFile = new ToolStripMenuItem ("File");
			ToolStripMenuItem tsmiHelp = new ToolStripMenuItem ("Help");
			ToolStripMenuItem tsmiQuit = new ToolStripMenuItem ("Quit");
			ToolStripMenuItem tsmiNew = new ToolStripMenuItem ("New");
			ToolStripMenuItem tsmiAbout = new ToolStripMenuItem ("About");
			tsmiQuit.DropDownItems.Add (tsmiNew);
			tsmiFile.DropDownItems.Add (tsmiQuit);
			tsmiHelp.DropDownItems.Add (tsmiAbout);
			MenuStrip menu = new MenuStrip ();
			menu.Items.Add (tsmiFile);
			menu.Items.Add (tsmiHelp);
			
			Assert.IsNotNull (tsmiFile.Owner);
			Assert.AreEqual (tsmiFile.Owner.GetType().Name, typeof (MenuStrip).Name);
			Assert.AreEqual (tsmiFile.Owner, menu);

			Assert.IsNotNull (tsmiQuit.Owner);
			Assert.AreEqual (tsmiQuit.Owner.GetType().Name, typeof (ToolStripDropDownMenu).Name);
			Assert.AreEqual (tsmiQuit.Owner, tsmiFile.DropDown);
		}
		
		[Test]
		public void OwnerItem ()
		{
			ToolStripMenuItem tsmiFile = new ToolStripMenuItem ("File");
			ToolStripMenuItem tsmiHelp = new ToolStripMenuItem ("Help");
			ToolStripMenuItem tsmiQuit = new ToolStripMenuItem ("Quit");
			ToolStripMenuItem tsmiNew = new ToolStripMenuItem ("New");
			ToolStripMenuItem tsmiAbout = new ToolStripMenuItem ("About");
			tsmiQuit.DropDownItems.Add (tsmiNew);
			tsmiFile.DropDownItems.Add (tsmiQuit);
			tsmiHelp.DropDownItems.Add (tsmiAbout);
			MenuStrip menu = new MenuStrip ();
			menu.Items.Add (tsmiFile);
			menu.Items.Add (tsmiHelp);

			Assert.IsNull (tsmiFile.OwnerItem);

			Assert.IsNotNull (tsmiQuit.OwnerItem);
			Assert.AreEqual (tsmiQuit.OwnerItem.GetType ().Name, typeof (ToolStripMenuItem).Name);
			Assert.AreEqual (tsmiQuit.OwnerItem, tsmiFile);
		}
		
		[Test]
		public void ToolStripDropDownButton_SelectChild ()
		{
			ToolStripDropDownButton tsddb = new ToolStripDropDownButton ();
			tsddb.DropDownClosed += Helper.FireEvent1;
			tsddb.DropDownItemClicked += Helper.FireEvent2;
			tsddb.DropDownOpened += Helper.FireEvent1;
			tsddb.DropDownOpening += Helper.FireEvent1;
			tsddb.Click += Helper.FireEvent1;

			Helper item1 = new Helper ();
			Helper item2 = new Helper ();

			tsddb.DropDownItems.Add (item1);
			tsddb.DropDownItems.Add (item2);
			ToolStripDropDownButton_SelectChildVerify (item1);
			ToolStrip ts = new ToolStrip ();
			ts.Items.Add (tsddb);
			ToolStripDropDownButton_SelectChildVerify (item2);
		}
		
		private static void ToolStripDropDownButton_SelectChildVerify (Helper item)
		{
			Assert.IsNull (item.MyParent);
			Assert.IsTrue (item.CanSelect);
			Assert.IsFalse (item.Selected);
			item.Select ();
			Assert.IsTrue (item.Selected);
			Assert.IsFalse (Helper.eventFired);
		}
		
		private class Helper : ToolStripMenuItem
		{
			internal Helper () {
				this.DropDownClosed += Helper.FireEvent1;
				this.DropDownItemClicked += Helper.FireEvent2;
				this.DropDownOpened += Helper.FireEvent1;
				this.DropDownOpening += Helper.FireEvent1;
				this.Click += Helper.FireEvent1;
			}
			
			internal ToolStrip MyParent { get { return this.Parent; } }

			internal static bool eventFired = false;
			internal static void FireEvent1 (object o, EventArgs args) { eventFired = true; }
			internal static void FireEvent2 (object o, ToolStripItemClickedEventArgs args) { FireEvent1 (null, null); }
		}
		
		private class ExposeProtectedMethods : ToolStripMenuItem
		{
			public bool PublicProcessCmdKey (ref Message m, Keys keys)
			{
				return this.ProcessCmdKey (ref m, keys);
			}
		}

		[Test]
		public void EventsTest ()
		{
			ToolStripMenuItem tsmi = new ToolStripMenuItem ("Sample");
			tsmi.CheckStateChanged += new EventHandler (tsmi_CheckStateChanged);
			tsmi.CheckedChanged += new EventHandler (tsmi_CheckedChanged);
			event_log = String.Empty;

			Assert.AreEqual (false, tsmi.Checked, "#A1");
			Assert.AreEqual (CheckState.Unchecked, tsmi.CheckState, "#A2");

			tsmi.Checked = true;
			Assert.AreEqual (true, tsmi.Checked, "#B1");
			Assert.AreEqual (CheckState.Checked, tsmi.CheckState, "#B2");
			Assert.AreEqual ("CheckedChanged=True;CheckStateChanged=Checked;", event_log, "#B3");

			event_log = String.Empty;

			tsmi.CheckState = CheckState.Unchecked;
			Assert.AreEqual (false, tsmi.Checked, "#C1");
			Assert.AreEqual (CheckState.Unchecked, tsmi.CheckState, "#C2");
			Assert.AreEqual ("CheckedChanged=False;CheckStateChanged=Unchecked;", event_log, "#C3");
		}

		string event_log;

		void tsmi_CheckedChanged (object sender, EventArgs e)
		{
			event_log += "CheckedChanged=" + ((ToolStripMenuItem)sender).Checked + ";";
		}

		void tsmi_CheckStateChanged (object sender, EventArgs e)
		{
			event_log += "CheckStateChanged=" + ((ToolStripMenuItem)sender).CheckState + ";";
		}
	}
}
#endif
