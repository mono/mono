//
// ToolStripItemCollectionTest.cs
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
// Copyright (c) 2007 Gert Driesen
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//

#if NET_2_0
using System;
using System.Collections.Generic;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolStripItemCollectionTests : TestHelper
	{
		private List<ToolStripItem> itemsAdded;
		private List<ToolStripItem> itemsRemoved;

		[SetUp]
		protected override void SetUp () {
			itemsAdded = new List<ToolStripItem> ();
			itemsRemoved = new List<ToolStripItem> ();
			base.SetUp ();
		}

		[Test]
		public void Constructor ()
		{
			ToolStrip toolStrip = CreateToolStrip ();
			ToolStripItemCollection items = null;

			items = new ToolStripItemCollection (toolStrip, new ToolStripItem [0]);
			Assert.AreEqual (0, items.Count, "#A1");
			Assert.IsFalse (items.IsReadOnly, "#A2");
			Assert.AreEqual (0, itemsAdded.Count, "#A3");

			MockToolStripButton buttonA = new MockToolStripButton ("A");
			MockToolStripButton buttonB = new MockToolStripButton ("B");
			items = new ToolStripItemCollection (toolStrip, new ToolStripItem [] {
				buttonA, buttonB });
			Assert.AreEqual (2, items.Count, "#B1");
			Assert.IsFalse (items.IsReadOnly, "#B2");
			Assert.AreEqual (0, itemsAdded.Count, "#B3");
			Assert.AreSame (buttonA, items [0], "#B4");
			Assert.AreSame (buttonB, items [1], "#B5");
			Assert.IsNull (buttonA.Owner, "#B6");
			Assert.IsNull (buttonA.ParentToolStrip, "#B7");
			Assert.IsNull (buttonB.Owner, "#B8");
			Assert.IsNull (buttonB.ParentToolStrip, "#B9");

			// null item
			try {
				new ToolStripItemCollection (toolStrip, new ToolStripItem [] {
					buttonA, null, buttonB });
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("value", ex.ParamName, "#C5");
			}
		}

		[Test]
		public void Constructor_Owner_Null ()
		{
			try {
				new ToolStripItemCollection ((ToolStrip) null, new ToolStripItem [0]);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("owner", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Constructor_Items_Null ()
		{
			try {
				new ToolStripItemCollection (new ToolStrip (), (ToolStripItem []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("toolStripItems", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Clear ()
		{
			ToolStrip ts = new ToolStrip ();
			ToolStripItemCollection coll = ts.Items;
			ToolStripItem item1 = new ToolStripLabel ("a");
			ToolStripItem item2 = new ToolStripLabel ("b");
			ToolStripItem item3 = new ToolStripLabel ("c");

			coll.Add (item1);
			coll.Add (item2);
			coll.Add (item3);

			Assert.AreEqual (3, coll.Count, "#A0");
			Assert.AreEqual (ts, item1.Owner, "#A1");
			Assert.AreEqual (ts, item2.Owner, "#A2");
			Assert.AreEqual (ts, item3.Owner, "#A3");

			coll.Clear ();
			Assert.AreEqual (0, coll.Count, "#B0");
			Assert.AreEqual (null, item1.Owner, "#B1");
			Assert.AreEqual (null, item2.Owner, "#B2");
			Assert.AreEqual (null, item3.Owner, "#B3");
		}

		[Test]
		public void Find ()
		{
			ToolStripItemCollection coll = new ToolStrip ().Items;

			ToolStripItem item1 = new ToolStripLabel ("alpha");
			item1.Name = "alpha";
			ToolStripItem item2 = new ToolStripLabel ("beta");
			item2.Name = "beta";
			ToolStripItem item3 = new ToolStripLabel ("Alpha");
			item3.Name = "Alpha";

			coll.Add (item1);
			coll.Add (item2);
			coll.Add (item3);

			ToolStripItem [] res = coll.Find ("alpha", true);
			Assert.AreEqual (2, res.Length, "#A1");

			res = coll.Find ("Beta", true);
			Assert.AreEqual (1, res.Length, "#B1");

			res = coll.Find ("DoesntExist", true);
			Assert.AreEqual (0, res.Length, "#C1");

			try {
				coll.Find (null, true);
				Assert.Fail ("#D1");
			} catch (ArgumentNullException) {
			}

			try {
				coll.Find (String.Empty, true);
				Assert.Fail ("#E1");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void Insert_Owned ()
		{
			ToolStrip toolStrip = CreateToolStrip ();
			ToolStripItemCollection items = toolStrip.Items;

			MockToolStripButton buttonA = new MockToolStripButton ("A");
			items.Insert (0, buttonA);
			Assert.AreEqual (1, items.Count, "#A1");
			Assert.AreEqual (1, itemsAdded.Count, "#A2");
			Assert.AreSame (buttonA, items [0], "#A3");
			Assert.AreSame (toolStrip, buttonA.Owner, "#A4");
			Assert.IsNull (buttonA.ParentToolStrip, "#A5");

			MockToolStripButton buttonB = new MockToolStripButton ("B");
			items.Insert (0, buttonB);
			Assert.AreEqual (2, items.Count, "#B1");
			Assert.AreEqual (2, itemsAdded.Count, "#B2");
			Assert.AreSame (buttonB, items [0], "#B3");
			Assert.AreSame (buttonA, items [1], "#B4");
			Assert.AreSame (toolStrip, buttonB.Owner, "#B5");
			Assert.IsNull (buttonB.ParentToolStrip, "#B6");

			MockToolStripButton buttonC = new MockToolStripButton ("C");
			items.Insert (1, buttonC);
			Assert.AreEqual (3, items.Count, "#C1");
			Assert.AreEqual (3, itemsAdded.Count, "#C2");
			Assert.AreSame (buttonB, items [0], "#C3");
			Assert.AreSame (buttonC, items [1], "#C4");
			Assert.AreSame (buttonA, items [2], "#C5");
			Assert.AreSame (toolStrip, buttonC.Owner, "#C6");
			Assert.IsNull (buttonC.ParentToolStrip, "#C7");
		}

		[Test]
		public void Insert_Owned_CreateControl ()
		{
			ToolStrip toolStrip = CreateToolStrip ();
			toolStrip.CreateControl ();
			ToolStripItemCollection items = toolStrip.Items;

			MockToolStripButton buttonA = new MockToolStripButton ("A");
			items.Insert (0, buttonA);
			Assert.AreEqual (1, items.Count, "#A1");
			Assert.AreEqual (1, itemsAdded.Count, "#A2");
			Assert.AreSame (buttonA, items[0], "#A3");
			Assert.AreSame (toolStrip, buttonA.Owner, "#A4");
			Assert.IsNotNull (buttonA.ParentToolStrip, "#A5");

			MockToolStripButton buttonB = new MockToolStripButton ("B");
			items.Insert (0, buttonB);
			Assert.AreEqual (2, items.Count, "#B1");
			Assert.AreEqual (2, itemsAdded.Count, "#B2");
			Assert.AreSame (buttonB, items[0], "#B3");
			Assert.AreSame (buttonA, items[1], "#B4");
			Assert.AreSame (toolStrip, buttonB.Owner, "#B5");
			Assert.IsNotNull (buttonB.ParentToolStrip, "#B6");

			MockToolStripButton buttonC = new MockToolStripButton ("C");
			items.Insert (1, buttonC);
			Assert.AreEqual (3, items.Count, "#C1");
			Assert.AreEqual (3, itemsAdded.Count, "#C2");
			Assert.AreSame (buttonB, items[0], "#C3");
			Assert.AreSame (buttonC, items[1], "#C4");
			Assert.AreSame (buttonA, items[2], "#C5");
			Assert.AreSame (toolStrip, buttonC.Owner, "#C6");
			Assert.IsNotNull (buttonC.ParentToolStrip, "#C7");
		}

		[Test]
		public void Insert_StandAlone ()
		{
			ToolStrip toolStrip = CreateToolStrip ();
			ToolStripItemCollection items = new ToolStripItemCollection (
				toolStrip, new ToolStripItem [0]);

			MockToolStripButton buttonA = new MockToolStripButton ("A");
			items.Insert (0, buttonA);
			Assert.AreEqual (1, items.Count, "#A1");
			Assert.AreEqual (0, itemsAdded.Count, "#A2");
			Assert.AreSame (buttonA, items [0], "#A3");
			Assert.IsNull (buttonA.Owner, "#A4");
			Assert.IsNull (buttonA.ParentToolStrip, "#A5");

			MockToolStripButton buttonB = new MockToolStripButton ("B");
			items.Insert (0, buttonB);
			Assert.AreEqual (2, items.Count, "#B1");
			Assert.AreEqual (0, itemsAdded.Count, "#B2");
			Assert.AreSame (buttonB, items [0], "#B3");
			Assert.AreSame (buttonA, items [1], "#B4");
			Assert.IsNull (buttonB.Owner, "#B5");
			Assert.IsNull (buttonB.ParentToolStrip, "#B6");

			MockToolStripButton buttonC = new MockToolStripButton ("C");
			items.Insert (1, buttonC);
			Assert.AreEqual (3, items.Count, "#C1");
			Assert.AreEqual (0, itemsAdded.Count, "#C2");
			Assert.AreSame (buttonB, items [0], "#C3");
			Assert.AreSame (buttonC, items [1], "#C4");
			Assert.AreSame (buttonA, items [2], "#C5");
			Assert.IsNull (buttonC.Owner, "#C6");
			Assert.IsNull (buttonC.ParentToolStrip, "#C7");
		}

		[Test]
		public void Insert_Index_OutOfRange ()
		{
			ToolStrip toolStrip = CreateToolStrip ();
			ToolStripItemCollection items = new ToolStripItemCollection (
				toolStrip, new ToolStripItem [0]);

			try {
				items.Insert (-1, new ToolStripButton ());
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("index", ex.ParamName, "#A5");
			}

			try {
				items.Insert (1, new ToolStripButton ());
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("index", ex.ParamName, "#B5");
			}
		}

		[Test]
		public void Insert_Item_Null ()
		{
			ToolStrip toolStrip = CreateToolStrip ();
			ToolStripItemCollection items = new ToolStripItemCollection (
				toolStrip, new ToolStripItem [0]);
			try {
				items.Insert (0, (ToolStripItem) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("value", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Remove_Owned ()
		{
			ToolStrip toolStrip = CreateToolStrip ();
			ToolStripItemCollection items = toolStrip.Items;

			MockToolStripButton buttonA = new MockToolStripButton ("A");
			MockToolStripButton buttonB = new MockToolStripButton ("B");
			MockToolStripButton buttonC = new MockToolStripButton ("B");
			items.Insert (0, buttonA);
			items.Insert (0, buttonB);

			items.Remove (buttonB);
			Assert.AreEqual (1, items.Count, "#A1");
			Assert.AreEqual (1, itemsRemoved.Count, "#A2");
			Assert.AreSame (buttonA, items [0], "#A3");
			Assert.AreSame (buttonB, itemsRemoved [0], "#A4");
			Assert.IsNull (buttonB.Owner, "#A5");
			Assert.IsNull (buttonB.ParentToolStrip, "#A6");

			// remove null item
			items.Remove ((ToolStripItem) null);
			Assert.AreEqual (1, items.Count, "#B1");
			Assert.AreEqual (2, itemsRemoved.Count, "#B2");
			Assert.AreSame (buttonA, items [0], "#B3");
			Assert.IsNull (itemsRemoved [1], "#B4");

			// remove item not owner by toolstrip
			items.Remove (buttonC);
			Assert.AreEqual (1, items.Count, "#C1");
			Assert.AreEqual (3, itemsRemoved.Count, "#C2");
			Assert.AreSame (buttonA, items [0], "#C3");
			Assert.AreSame(buttonC, itemsRemoved [2], "#C4");
			Assert.IsNull (buttonC.Owner, "#C5");
			Assert.IsNull (buttonC.ParentToolStrip, "#C6");

			items.Remove (buttonA);
			Assert.AreEqual (0, items.Count, "#D1");
			Assert.AreEqual (4, itemsRemoved.Count, "#D2");
			Assert.AreSame(buttonA, itemsRemoved [3], "#D3");
			Assert.IsNull (buttonC.Owner, "#D4");
			Assert.IsNull (buttonC.ParentToolStrip, "#D5");

			// remove item which is no longer in the collection
			items.Remove (buttonA);
			Assert.AreEqual (0, items.Count, "#E1");
			Assert.AreEqual (5, itemsRemoved.Count, "#E2");
			Assert.AreSame(buttonA, itemsRemoved [4], "#E3");

			// remove item owned by other toolstrip
			ToolStrip otherToolStrip = new ToolStrip ();
			MockToolStripButton buttonD = new MockToolStripButton ("B");
			otherToolStrip.Items.Add (buttonD);
			Assert.AreSame (otherToolStrip, buttonD.Owner, "#F1");
			Assert.IsNull (buttonD.ParentToolStrip, "#F2");
			items.Remove (buttonD);
			Assert.AreEqual (0, items.Count, "#F3");
			Assert.AreEqual (6, itemsRemoved.Count, "#F4");
			Assert.IsNull (buttonD.Owner, "#F5");
			Assert.IsNull (buttonD.ParentToolStrip, "#F6");
		}

		[Test]
		public void Remove_StandAlone ()
		{
			ToolStrip toolStrip = CreateToolStrip ();
			ToolStripItemCollection items = new ToolStripItemCollection (
				toolStrip, new ToolStripItem [0]);

			MockToolStripButton buttonA = new MockToolStripButton ("A");
			MockToolStripButton buttonB = new MockToolStripButton ("B");
			MockToolStripButton buttonC = new MockToolStripButton ("B");
			items.Insert (0, buttonA);
			items.Insert (0, buttonB);

			items.Remove (buttonB);
			Assert.AreEqual (1, items.Count, "#A1");
			Assert.AreEqual (0, itemsRemoved.Count, "#A2");
			Assert.AreSame (buttonA, items [0], "#A3");

			items.Remove ((ToolStripItem) null);
			Assert.AreEqual (1, items.Count, "#B1");
			Assert.AreEqual (0, itemsRemoved.Count, "#B2");
			Assert.AreSame (buttonA, items [0], "#B3");

			items.Remove (buttonC);
			Assert.AreEqual (1, items.Count, "#C1");
			Assert.AreEqual (0, itemsRemoved.Count, "#C2");
			Assert.AreSame (buttonA, items [0], "#C3");

			items.Remove (buttonA);
			Assert.AreEqual (0, items.Count, "#D1");
			Assert.AreEqual (0, itemsRemoved.Count, "#D2");

			items.Remove (buttonA);
			Assert.AreEqual (0, items.Count, "#E1");
			Assert.AreEqual (0, itemsRemoved.Count, "#E2");

			// remove item owned by other toolstrip
			ToolStrip otherToolStrip = new ToolStrip ();
			MockToolStripButton buttonD = new MockToolStripButton ("B");
			otherToolStrip.Items.Add (buttonD);
			Assert.AreSame (otherToolStrip, buttonD.Owner, "#F1");
			Assert.IsNull (buttonD.ParentToolStrip, "#F2");
			items.Remove (buttonD);
			Assert.AreEqual (0, items.Count, "#F3");
			Assert.AreEqual (0, itemsRemoved.Count, "#F4");
			Assert.AreSame (otherToolStrip, buttonD.Owner, "#F5");
			Assert.IsNull (buttonD.ParentToolStrip, "#F6");
		}
		
		[Test]
		public void AddToolStripInstanceMultipleTimes ()
		{
			ToolStrip toolStrip = CreateToolStrip ();
			ToolStripItemCollection items = null;
					
			items = new ToolStripItemCollection (toolStrip, new ToolStripItem [0]);
			
			var item = new ToolStripButton ("test");
			toolStrip.Items.Add (item);
			Assert.AreEqual(1, toolStrip.Items.Count, "A1");
			
			toolStrip.Items.Add (item);
			Assert.AreEqual(1, toolStrip.Items.Count, "A2");
		}

		void ToolStrip_ItemAdded (object sender, ToolStripItemEventArgs e)
		{
			itemsAdded.Add (e.Item);
		}

		void ToolStrip_ItemRemoved (object sender, ToolStripItemEventArgs e)
		{
			itemsRemoved.Add (e.Item);
		}

		ToolStrip CreateToolStrip ()
		{
			ToolStrip toolStrip = new ToolStrip ();
			toolStrip.ItemAdded += ToolStrip_ItemAdded;
			toolStrip.ItemRemoved += ToolStrip_ItemRemoved;
			return toolStrip;
		}

		class MockToolStripButton : ToolStripButton
		{
			public MockToolStripButton (string text) : base (text)
			{
			}

			public ToolStrip ParentToolStrip {
				get { return base.Parent; }
				set { base.Parent = value; }
			}
		}
	}
}
#endif
