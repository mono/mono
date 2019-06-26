//
// StatusBarTest.cs: Test cases for StatusBar.
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
using System.Collections;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class StatusBarTest : TestHelper 
	{
		[Test]
		public void StatusBarTextTest ()
		{
			string a = new string ('a', 127);
			string ab = a + "b";
			StatusBar sb = new StatusBar();
			sb.Text = ab;
			Assert.AreEqual (ab, sb.Text, "#01");
		}
		
		[Test]
		public void StatusBarShowPanelsTest ()
		{
			StatusBar sb = new StatusBar ();
			sb.ShowPanels = true;
			sb.Text = "Test";
			Assert.AreEqual ("Test", sb.Text, "#01");
		}	

		[Test]
		public void StatusBarPropertyTest ()
		{	
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			StatusBar mysbar = new StatusBar ();
			StatusBarPanel sbarpanel1 = new StatusBarPanel ();
			StatusBarPanel sbarpanel2 = new StatusBarPanel ();
			sbarpanel1.Text = "Status Quo";
			sbarpanel2.Text = "State 2";
			mysbar.Panels.Add (sbarpanel1);
			mysbar.Panels.Add (sbarpanel2);
			myform.Controls.Add (mysbar);
			
			// B
			Assert.AreEqual ("Control", mysbar.BackColor.Name, "#B1");

			// D
			Assert.AreEqual (DockStyle.Bottom, mysbar.Dock, "#D1");

			// F
			Assert.AreEqual ("ControlText", mysbar.ForeColor.Name, "#F2");
	
			// P
			Assert.AreEqual (sbarpanel1.Text, mysbar.Panels [0].Text , "#P1");
			
			// S
			Assert.AreEqual (false, mysbar.ShowPanels, "#S1");
			Assert.AreEqual (true, mysbar.SizingGrip, "#S2");
			Assert.AreEqual (null, mysbar.Site, "#S3");

			// T
			Assert.AreEqual ("", mysbar.Text, "#T1");
			mysbar.Text = "MONO STATUSBAR";
			Assert.AreEqual ("MONO STATUSBAR", mysbar.Text, "#T2");

			myform.Dispose ();
		}
		
		[Test]
		public void ToStringMethodTest () 
		{
			StatusBar mysbar = new StatusBar ();
			mysbar.Text = "New StatusBar";
			Assert.AreEqual ("System.Windows.Forms.StatusBar, Panels.Count: 0", mysbar.ToString (), "#T3");
		}
		//[MonoTODO ("Test case for DrawItem")]	
		//[MonoTODO ("Test case for PanelClick")]	

		private class MyPanel: StatusBarPanel
		{
			public bool IsDisposed { get; private set; }

			protected override void Dispose (bool disposing)
			{
				base.Dispose (disposing);
				IsDisposed = true;
			}
		}

		[Test]
		public void DisposeTest ()
		{
			StatusBar sut = new StatusBar ();
			MyPanel panel = new MyPanel ();
			sut.Panels.Add (panel);

			sut.Dispose ();

			Assert.That (panel.IsDisposed, Is.True);
		}
	}
	
	[TestFixture]
	public class StatusBarPanelCollectionTest : TestHelper
	{
		[Test]
		public void Test ()
		{

		}
		[Test]
		public void DefaultPropertiesTest ()
		{
			StatusBar bar = new StatusBar ();
			StatusBar.StatusBarPanelCollection collection = new StatusBar.StatusBarPanelCollection (bar);
			
			Assert.AreEqual (0, collection.Count, "C1");
			Assert.AreEqual (false, collection.IsReadOnly, "I1");
		}
		
		
		[Test]
		public void AddRemoveTest ()
		{
			StatusBar bar = new StatusBar ();
			StatusBar.StatusBarPanelCollection collection = new StatusBar.StatusBarPanelCollection (bar);

			StatusBarPanel panel = new StatusBarPanel ();
			StatusBarPanel panel2 = new StatusBarPanel ();
			
			collection.Add (panel);
			Assert.AreEqual (1, collection.Count, "#1");

			collection.Remove (panel);
			Assert.AreEqual (0, collection.Count, "#2");

			collection.Add (panel);
			collection.RemoveAt (0);
			Assert.AreEqual (0, collection.Count, "#3");

			collection.Add (panel);
			Assert.AreEqual (0, collection.IndexOf (panel), "#4");
			Assert.AreEqual (-1, collection.IndexOf (panel2), "#5");

			collection.Add (panel2);
			Assert.AreEqual (1, collection.IndexOf (panel2), "#6");

			collection.RemoveAt (0);
			Assert.AreEqual (0, collection.IndexOf (panel2), "#7");
			Assert.AreEqual (1, collection.Count, "#8");
			
			Assert.AreEqual (false, collection.Contains (panel), "#9");
			Assert.AreEqual (true, collection.Contains (panel2), "#10");
			
		}

		[Test]
		public void ItemByKeyTest ()
		{
			StatusBar bar = new StatusBar ();
			StatusBar.StatusBarPanelCollection c = new StatusBar.StatusBarPanelCollection (bar);

			StatusBarPanel panel1 = new StatusBarPanel ();
			StatusBarPanel panel2 = new StatusBarPanel ();
			StatusBarPanel panel3 = new StatusBarPanel ();
			StatusBarPanel panel4 = new StatusBarPanel ();
			StatusBarPanel panel5 = new StatusBarPanel ();


			panel1.Name = "p1";
			panel2.Name = "p2";
			panel3.Name = "P2";
			panel4.Name = "";
			panel5.Name = null;

			c.AddRange (new StatusBarPanel [] { panel1, panel2, panel3, panel4, panel5 });

			Assert.AreEqual (null, c [""], "#1");
			Assert.AreEqual (null, c [null], "#2");
			Assert.AreEqual (panel1, c ["p1"], "#3");
			Assert.AreEqual (panel1, c ["P1"], "#4");
			Assert.AreEqual (panel2, c ["p2"], "#5");
			Assert.AreEqual (panel2, c ["P2"], "#6");
			Assert.AreEqual (null, c ["p3"], "#7");
			Assert.AreEqual (null, c ["p"], "#8");	
		}

		[Test]
		public void RemoveByKeyTest ()
		{
			StatusBar bar = new StatusBar ();
			StatusBar.StatusBarPanelCollection c = new StatusBar.StatusBarPanelCollection (bar);

			StatusBarPanel panel1 = new StatusBarPanel ();
			StatusBarPanel panel2 = new StatusBarPanel ();
			StatusBarPanel panel3 = new StatusBarPanel ();
			StatusBarPanel panel4 = new StatusBarPanel ();
			StatusBarPanel panel5 = new StatusBarPanel ();


			panel1.Name = "p1";
			panel2.Name = "p2";
			panel3.Name = "P2";
			panel4.Name = "";
			panel5.Name = null;

			c.AddRange (new StatusBarPanel [] { panel1, panel2, panel3, panel4, panel5 });

			Assert.AreEqual (true, c.ContainsKey ("p1"), "#A1");
			Assert.AreEqual (true, c.ContainsKey ("P1"), "#A2");
			Assert.AreEqual (true, c.ContainsKey ("P2"), "#A3");
			Assert.AreEqual (false, c.ContainsKey (""), "#A4");
			Assert.AreEqual (false, c.ContainsKey (null), "#A5");
			Assert.AreEqual (false, c.ContainsKey ("p3"), "#A6");
			Assert.AreEqual (false, c.ContainsKey ("p"), "#A7");
			Assert.AreEqual (null, c [""], "#A8");
			Assert.AreEqual (null, c [null], "#A9");
			Assert.AreEqual (panel1, c ["p1"], "#A10");
			Assert.AreEqual (panel1, c ["P1"], "#A11");
			Assert.AreEqual (panel2, c ["p2"], "#A12");
			Assert.AreEqual (panel2, c ["P2"], "#A13");
			Assert.AreEqual (null, c ["p3"], "#A14");
			Assert.AreEqual (null, c ["p"], "#A15");	
			
			c.RemoveByKey ("P2");

			Assert.AreEqual (true, c.ContainsKey ("p1"), "#B1");
			Assert.AreEqual (true, c.ContainsKey ("P1"), "#B2");
			Assert.AreEqual (true, c.ContainsKey ("P2"), "#B3");
			Assert.AreEqual (false, c.ContainsKey (""), "#B4");
			Assert.AreEqual (false, c.ContainsKey (null), "#B5");
			Assert.AreEqual (false, c.ContainsKey ("p3"), "#B6");
			Assert.AreEqual (false, c.ContainsKey ("p"), "#B7");
			Assert.AreEqual (null, c [""], "#B8");
			Assert.AreEqual (null, c [null], "#B9");
			Assert.AreEqual (panel1, c ["p1"], "#B10");
			Assert.AreEqual (panel1, c ["P1"], "#B11");
			Assert.AreEqual (panel3, c ["p2"], "#B12");
			Assert.AreEqual (panel3, c ["P2"], "#B13");
			Assert.AreEqual (null, c ["p3"], "#B14");
			Assert.AreEqual (null, c ["p"], "#B15");

			c.RemoveByKey ("p2");

			Assert.AreEqual (true, c.ContainsKey ("p1"), "#C1");
			Assert.AreEqual (true, c.ContainsKey ("P1"), "#C2");
			Assert.AreEqual (false, c.ContainsKey ("P2"), "#C3");
			Assert.AreEqual (false, c.ContainsKey (""), "#C4");
			Assert.AreEqual (false, c.ContainsKey (null), "#C5");
			Assert.AreEqual (false, c.ContainsKey ("p3"), "#C6");
			Assert.AreEqual (false, c.ContainsKey ("p"), "#C7");
			Assert.AreEqual (null, c [""], "#C8");
			Assert.AreEqual (null, c [null], "#C9");
			Assert.AreEqual (panel1, c ["p1"], "#C10");
			Assert.AreEqual (panel1, c ["P1"], "#C11");
			Assert.AreEqual (null, c ["p2"], "#C12");
			Assert.AreEqual (null, c ["P2"], "#C13");
			Assert.AreEqual (null, c ["p3"], "#C14");
			Assert.AreEqual (null, c ["p"], "#C15");

			c.RemoveByKey ("p2");

			Assert.AreEqual (true, c.ContainsKey ("p1"), "#D1");
			Assert.AreEqual (true, c.ContainsKey ("P1"), "#D2");
			Assert.AreEqual (false, c.ContainsKey ("P2"), "#D3");
			Assert.AreEqual (false, c.ContainsKey (""), "#D4");
			Assert.AreEqual (false, c.ContainsKey (null), "#D5");
			Assert.AreEqual (false, c.ContainsKey ("p3"), "#D6");
			Assert.AreEqual (false, c.ContainsKey ("p"), "#D7");
			Assert.AreEqual (null, c [""], "#D8");
			Assert.AreEqual (null, c [null], "#D9");
			Assert.AreEqual (panel1, c ["p1"], "#D10");
			Assert.AreEqual (panel1, c ["P1"], "#D11");
			Assert.AreEqual (null, c ["p2"], "#D12");
			Assert.AreEqual (null, c ["P2"], "#D13");
			Assert.AreEqual (null, c ["p3"], "#D14");
			Assert.AreEqual (null, c ["p"], "#D15");
			
			c.RemoveByKey ("P1");

			Assert.AreEqual (false, c.ContainsKey ("p1"), "#E1");
			Assert.AreEqual (false, c.ContainsKey ("P1"), "#E2");
			Assert.AreEqual (false, c.ContainsKey ("P2"), "#E3");
			Assert.AreEqual (false, c.ContainsKey (""), "#E4");
			Assert.AreEqual (false, c.ContainsKey (null), "#E5");
			Assert.AreEqual (false, c.ContainsKey ("p3"), "#E6");
			Assert.AreEqual (false, c.ContainsKey ("p"), "#E7");
			Assert.AreEqual (null, c [""], "#E8");
			Assert.AreEqual (null, c [null], "#E9");
			Assert.AreEqual (null, c ["p1"], "#E10");
			Assert.AreEqual (null, c ["P1"], "#E11");
			Assert.AreEqual (null, c ["p2"], "#E12");
			Assert.AreEqual (null, c ["P2"], "#E13");
			Assert.AreEqual (null, c ["p3"], "#E14");
			Assert.AreEqual (null, c ["p"], "#E15");

			c.RemoveByKey ("");

			Assert.AreEqual (false, c.ContainsKey ("p1"), "#F1");
			Assert.AreEqual (false, c.ContainsKey ("P1"), "#F2");
			Assert.AreEqual (false, c.ContainsKey ("P2"), "#F3");
			Assert.AreEqual (false, c.ContainsKey (""), "#F4");
			Assert.AreEqual (false, c.ContainsKey (null), "#F5");
			Assert.AreEqual (false, c.ContainsKey ("p3"), "#F6");
			Assert.AreEqual (false, c.ContainsKey ("p"), "#F7");
			Assert.AreEqual (null, c [""], "#F8");
			Assert.AreEqual (null, c [null], "#F9");
			Assert.AreEqual (null, c ["p1"], "#F10");
			Assert.AreEqual (null, c ["P1"], "#F11");
			Assert.AreEqual (null, c ["p2"], "#F12");
			Assert.AreEqual (null, c ["P2"], "#F13");
			Assert.AreEqual (null, c ["p3"], "#F14");
			Assert.AreEqual (null, c ["p"], "#F15");


			c.RemoveByKey (null);

			Assert.AreEqual (false, c.ContainsKey ("p1"), "#G1");
			Assert.AreEqual (false, c.ContainsKey ("P1"), "#G2");
			Assert.AreEqual (false, c.ContainsKey ("P2"), "#G3");
			Assert.AreEqual (false, c.ContainsKey (""), "#G4");
			Assert.AreEqual (false, c.ContainsKey (null), "#G5");
			Assert.AreEqual (false, c.ContainsKey ("p3"), "#G6");
			Assert.AreEqual (false, c.ContainsKey ("p"), "#G7");
			Assert.AreEqual (null, c [""], "#G8");
			Assert.AreEqual (null, c [null], "#G9");
			Assert.AreEqual (null, c ["p1"], "#G10");
			Assert.AreEqual (null, c ["P1"], "#G11");
			Assert.AreEqual (null, c ["p2"], "#G12");
			Assert.AreEqual (null, c ["P2"], "#G13");
			Assert.AreEqual (null, c ["p3"], "#G14");
			Assert.AreEqual (null, c ["p"], "#G15");
			
		}
		
		public void ContainsKeyTest ()
		{

			StatusBar bar = new StatusBar ();
			StatusBar.StatusBarPanelCollection c = new StatusBar.StatusBarPanelCollection (bar);

			StatusBarPanel panel1 = new StatusBarPanel ();
			StatusBarPanel panel2 = new StatusBarPanel ();
			StatusBarPanel panel3 = new StatusBarPanel ();
			StatusBarPanel panel4 = new StatusBarPanel ();
			StatusBarPanel panel5 = new StatusBarPanel ();


			panel1.Name = "p1";
			panel2.Name = "p2";
			panel3.Name = "P2";
			panel4.Name = "";
			panel5.Name = null;
			
			c.AddRange (new StatusBarPanel [] {panel1, panel2, panel3, panel4, panel5});
			
			Assert.AreEqual (true, c.ContainsKey ("p1"), "#1");
			Assert.AreEqual (true, c.ContainsKey ("P1"), "#2");
			Assert.AreEqual (true, c.ContainsKey ("P2"), "#3");
			Assert.AreEqual (false, c.ContainsKey (""), "#4");
			Assert.AreEqual (false, c.ContainsKey (null), "#5");
			Assert.AreEqual (false, c.ContainsKey ("p3"), "#6");
			Assert.AreEqual (false, c.ContainsKey ("p"), "#7");

		}
		
		public void IndexByKeyTest ()
		{
			StatusBar bar = new StatusBar ();
			StatusBar.StatusBarPanelCollection c = new StatusBar.StatusBarPanelCollection (bar);

			StatusBarPanel panel1 = new StatusBarPanel ();
			StatusBarPanel panel2 = new StatusBarPanel ();
			StatusBarPanel panel3 = new StatusBarPanel ();
			StatusBarPanel panel4 = new StatusBarPanel ();
			StatusBarPanel panel5 = new StatusBarPanel ();


			panel1.Name = "p1";
			panel2.Name = "p2";
			panel3.Name = "P2";
			panel4.Name = "";
			panel5.Name = null;

			c.AddRange (new StatusBarPanel [] { panel1, panel2, panel3, panel4, panel5 });
			
			Assert.AreEqual (-1, c.IndexOfKey (""), "#1");
			Assert.AreEqual (-1, c.IndexOfKey (null), "#2");
			Assert.AreEqual (0, c.IndexOfKey ("p1"), "#3");
			Assert.AreEqual (0, c.IndexOfKey ("P1"), "#4");
			Assert.AreEqual (1, c.IndexOfKey ("p2"), "#5");
			Assert.AreEqual (1, c.IndexOfKey ("P2"), "#6");
			Assert.AreEqual (-1, c.IndexOfKey ("p3"), "#7");
			Assert.AreEqual (-1, c.IndexOfKey ("p"), "#8");			
		}

		[Test]
		public void PanelParentAlwaysSet()
		{
			// Ensure that the panel is always correctly initialized (Parent set) 
			// even when added to the collection even by the IList interface methods.
			//
			StatusBar.StatusBarPanelCollection coll;
			coll = CreateStatusBarPanelCollection ();
			StatusBarPanel sbp;
			//
			coll.Add ("panel1");
			Assert.IsNotNull (coll [0].Parent, "Add(String) null!");
			//
			sbp = new StatusBarPanel ();
			Assert.IsNull (sbp.Parent, "checking test precondition that Parent is null initially");
			coll.Add (sbp);
			Assert.IsNotNull (sbp.Parent, "Add(StatusBarPanel) null!");
			//
			sbp = new StatusBarPanel ();
			coll.Insert (1, sbp);
			Assert.IsNotNull (sbp.Parent, "Insert(int, StatusBarPanel) null!");
			//
			sbp = new StatusBarPanel ();
			coll[0] = sbp;
			Assert.IsNotNull (sbp.Parent, "this[int] null!");
			//
			// IList interface
			sbp = new StatusBarPanel ();
			((IList)coll).Add (sbp);
			Assert.IsNotNull (sbp.Parent, "IList.Add(object) null!");
			//
			sbp = new StatusBarPanel ();
			((IList)coll).Insert (2, sbp);
			Assert.IsNotNull (sbp.Parent, "IList.Insert(int, object) null!");
			//
			sbp = new StatusBarPanel ();
			((IList)coll)[0] = sbp;
			Assert.IsNotNull (sbp.Parent, "IList.this[int] null!");
		}

		[Test]
		public void Interface_Misc()
		{
			// Test other interface methods.
			//
			StatusBar.StatusBarPanelCollection coll;
			// From bug XXXXXX -- Ensure that the internal list is initialized when 
			// calling an interface method first.
			coll = CreateStatusBarPanelCollection ();
			object[] arr = new object[coll.Count];
			((ICollection)coll).CopyTo (arr, 0);
			coll = CreateStatusBarPanelCollection ();
			((IList)coll).Add (new StatusBarPanel ());
			//
			// Check what happens when a non-StatusBarPanel type is passed into the 
			// interface methods.
			object tmp = new Version (1, 2, 3, 4);
			try {
				((IList)coll) [0] = tmp;
			} catch (ArgumentException) { }
			try {
				((IList)coll).Add (tmp);
			} catch (ArgumentException) { }
			Assert.IsFalse (((IList)coll).Contains (tmp), "Contains(tmp)");
			Assert.AreEqual (-1, ((IList)coll).IndexOf (tmp), "IndexOf (tmp)");
			try {
				((IList)coll).Insert (0, tmp);
			} catch (ArgumentException) { }
			((IList)coll).Remove (tmp);
			//
			// Note that adding null fails, thus the try cast to 
			// StatusBarPanel means that the Contains/IndexOf/Remove 
			// methods will fail as shown above when passed another type.
			try {
				coll.Add ((StatusBarPanel)null);
			} catch (ArgumentNullException) { }
		}

		StatusBar.StatusBarPanelCollection CreateStatusBarPanelCollection()
		{
			return new StatusBar.StatusBarPanelCollection (new StatusBar ());
		}

		[Test]
		public void Interface_AddRemoveTest()
		{
			// Copy of test, for the equivalent ICollection/IList members.
			//
			StatusBar bar = new StatusBar ();
			StatusBar.StatusBarPanelCollection collectionX = new StatusBar.StatusBarPanelCollection (bar);
			IList collection = collectionX;

			StatusBarPanel panel = new StatusBarPanel ();
			StatusBarPanel panel2 = new StatusBarPanel ();

			collection.Add (panel);
			Assert.AreEqual (1, collection.Count, "#1");

			collection.Remove (panel);
			Assert.AreEqual (0, collection.Count, "#2");

			collection.Add (panel);
			collection.RemoveAt (0);
			Assert.AreEqual (0, collection.Count, "#3");

			collection.Add (panel);
			Assert.AreEqual (0, collection.IndexOf (panel), "#4");
			Assert.AreEqual (-1, collection.IndexOf (panel2), "#5");

			collection.Add (panel2);
			Assert.AreEqual (1, collection.IndexOf (panel2), "#6");

			collection.RemoveAt (0);
			Assert.AreEqual (0, collection.IndexOf (panel2), "#7");
			Assert.AreEqual (1, collection.Count, "#8");

			Assert.AreEqual (false, collection.Contains (panel), "#9");
			Assert.AreEqual (true, collection.Contains (panel2), "#10");

		}

		[Test]
		public void Insert()
		{
			StatusBar bar = new StatusBar ();
			StatusBar.StatusBarPanelCollection collection = new StatusBar.StatusBarPanelCollection (bar);

			StatusBarPanel panel = new StatusBarPanel ();
			StatusBarPanel panel2 = new StatusBarPanel ();

			collection.Add (panel);
			Assert.AreEqual (1, collection.Count, "#1");

			collection.Insert (0, panel);
			Assert.AreEqual (2, collection.Count, "#2");
		}

		[Test]
		public void Interface_Insert()
		{
			// Copy of test, for the equivalent ICollection/IList members.
			//
			StatusBar bar = new StatusBar ();
			StatusBar.StatusBarPanelCollection collectionX = new StatusBar.StatusBarPanelCollection (bar);
			IList collection = collectionX;

			StatusBarPanel panel = new StatusBarPanel ();
			StatusBarPanel panel2 = new StatusBarPanel ();

			collection.Add (panel);
			Assert.AreEqual (1, collection.Count, "#1");

			collection.Insert (0, panel);
			Assert.AreEqual (2, collection.Count, "#2");
		}

	}
}
