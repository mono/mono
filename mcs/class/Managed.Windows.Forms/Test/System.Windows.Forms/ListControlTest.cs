//
// ListControlTest.cs: Tests for ListControl abstract class.
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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//

using System;
using System.Collections;
#if NET_2_0
using System.ComponentModel;
#endif
using System.IO;
using System.Data;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListControlTest
	{
		private int dataSourceChanged;

		[SetUp]
		public void SetUp ()
		{
			dataSourceChanged = 0;
		}

		[Test]
		// Bug 80794
		public void DataBindingsTest ()
		{
			string table =
@"<?xml version=""1.0"" standalone=""yes""?>
<DOK>
<DOK>
<klient>287</klient>
</DOK>
</DOK>
";
			string lookup =
@"<?xml version=""1.0"" standalone=""yes""?>
<klient>
<klient>
<nimi>FAILED</nimi>
<kood>316</kood>
</klient>
<klient>
<nimi>SUCCESS</nimi>
<kood>287</kood>
</klient>
</klient>";

			using (Form frm = new Form ()) {
				frm.ShowInTaskbar = false;
				DataSet dsTable = new DataSet ();
				dsTable.ReadXml (new StringReader (table));
				DataSet dsLookup = new DataSet ();
				dsLookup.ReadXml (new StringReader (lookup));
				ComboBox cb = new ComboBox ();
				cb.DataSource = dsLookup.Tables [0];
				cb.DisplayMember = "nimi";
				cb.ValueMember = "kood";
				cb.DataBindings.Add ("SelectedValue", dsTable.Tables [0], "klient");
				frm.Controls.Add (cb);
				Assert.AreEqual ("", cb.Text, "#01");
				frm.Show ();
				Assert.AreEqual ("SUCCESS", cb.Text, "#02");
			}
		}

		[Test]
		public void GetItemText ()
		{
			MockItem itemA = new MockItem ("A", 1);
			MockItem itemB = new MockItem ("B", 2);
			object itemC = new object ();

			ListControlChild lc = new ListControlChild ();
			lc.DisplayMember = "Text";

			// No DataSource available
			Assert.AreEqual ("A", lc.GetItemText (itemA), "#A1");
			Assert.AreEqual ("B", lc.GetItemText (itemB), "#A2");
			Assert.AreEqual (itemC.GetType ().FullName, lc.GetItemText (itemC), "#A3");

			lc.DisplayMember = String.Empty;

			Assert.AreEqual (itemA.GetType ().FullName, lc.GetItemText (itemA), "#B1");
			Assert.AreEqual (itemB.GetType ().FullName, lc.GetItemText (itemB), "#B2");
			Assert.AreEqual (itemC.GetType ().FullName, lc.GetItemText (itemC), "#B3");

			// DataSource available
			object [] objects = new object [] {itemA, itemB, itemC};
			lc.DisplayMember = "Text";
			lc.DataSource = objects;

			Assert.AreEqual ("A", lc.GetItemText (itemA), "#C1");
			Assert.AreEqual ("B", lc.GetItemText (itemB), "#C2");
			Assert.AreEqual (itemC.GetType ().FullName, lc.GetItemText (itemC), "#C3");

			lc.DisplayMember = String.Empty;

			Assert.AreEqual (itemA.GetType ().FullName, lc.GetItemText (itemA), "#D1");
			Assert.AreEqual (itemB.GetType ().FullName, lc.GetItemText (itemB), "#D2");
			Assert.AreEqual (itemC.GetType ().FullName, lc.GetItemText (itemC), "#D3");
		}
		
		[Test]
		public void FilterItemOnProperty ()
		{
			MockItem itemA = new MockItem ("A", 1);
			MockItem itemB = new MockItem ("B", 2);
			object itemC = new object ();

			ListControlChild lc = new ListControlChild ();
			lc.DisplayMember = "Text";

			// No DataSource available
			Assert.AreEqual ("A", lc.FilterItem (itemA, lc.DisplayMember), "#A1");
			Assert.AreEqual ("B", lc.FilterItem (itemB, lc.DisplayMember), "#A2");
			Assert.AreEqual (itemC, lc.FilterItem (itemC, lc.DisplayMember), "#A3");

			lc.DisplayMember = String.Empty;

			Assert.AreEqual (itemA, lc.FilterItem (itemA, lc.DisplayMember), "#B1");
			Assert.AreEqual (itemB, lc.FilterItem (itemB, lc.DisplayMember), "#B2");
			Assert.AreEqual (itemC, lc.FilterItem (itemC, lc.DisplayMember), "#B3");

			// DataSource available
			object [] objects = new object [] {itemA, itemB, itemC};
			lc.DisplayMember = "Text";
			lc.DataSource = objects;

			Assert.AreEqual ("A", lc.FilterItem (itemA, lc.DisplayMember), "#C1");
			Assert.AreEqual ("B", lc.FilterItem (itemB, lc.DisplayMember), "#C2");
			Assert.AreEqual (itemC, lc.FilterItem (itemC, lc.DisplayMember), "#C3");

			lc.DisplayMember = String.Empty;

			Assert.AreEqual (itemA, lc.FilterItem (itemA, lc.DisplayMember), "#D1");
			Assert.AreEqual (itemB, lc.FilterItem (itemB, lc.DisplayMember), "#D2");
			Assert.AreEqual (itemC, lc.FilterItem (itemC, lc.DisplayMember), "#D3");
		}

		[Test]
		public void DisplayMemberNullTest ()
		{
			ListControlChild lc = new ListControlChild ();
			lc.DisplayMember = null;
			Assert.AreEqual (String.Empty, lc.DisplayMember, "#1");
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DataSourceWrongArgumentType ()
		{
			ListControlChild lc = new ListControlChild ();
			lc.DataSource = new object ();
		}

		[Test]
		public void DataSource1 ()
		{
			ArrayList list1 = new ArrayList ();
			list1.Add ("item 1");
			ArrayList list2 = new ArrayList ();

			ListControlChild lc = new ListControlChild ();
			lc.DataSourceChanged += new EventHandler (ListControl_DataSourceChanged);
			lc.DataSource = list1;
			Assert.AreEqual (1, dataSourceChanged, "#A1");
			Assert.AreSame (list1, lc.DataSource, "#A2");

			Form form = new Form ();
			form.Controls.Add (lc);

			Assert.AreEqual (1, dataSourceChanged, "#B1");
			Assert.AreSame (list1, lc.DataSource, "#B2");
			lc.DataSource = list1;
			Assert.AreEqual (1, dataSourceChanged, "#B3");
			Assert.AreSame (list1, lc.DataSource, "#B4");
			lc.DataSource = list2;
			Assert.AreEqual (2, dataSourceChanged, "#B5");
			Assert.AreSame (list2, lc.DataSource, "#B6");
			lc.DataSource = null;
			Assert.AreEqual (3, dataSourceChanged, "#B7");
			Assert.IsNull (lc.DataSource, "#B8");

			list1.Add ("whatever");
			list2.Add ("whatever");
			list1.Clear ();
			list2.Clear ();

			form.Dispose ();
		}

		[Test]
		public void DataSource2 ()
		{
			ArrayList list1 = new ArrayList ();
			list1.Add ("item 1");
			ArrayList list2 = new ArrayList ();

			ListControlChild lc = new ListControlChild ();
			lc.DataSourceChanged += new EventHandler (ListControl_DataSourceChanged);

			Form form = new Form ();
			form.Controls.Add (lc);

			Assert.AreEqual (0, dataSourceChanged, "#1");
			Assert.IsNull (lc.DataSource, "#2");
			lc.DataSource = list1;
			Assert.AreEqual (1, dataSourceChanged, "#3");
			Assert.AreSame (list1, lc.DataSource, "#4");
			lc.DataSource = list2;
			Assert.AreEqual (2, dataSourceChanged, "#5");
			Assert.AreSame (list2, lc.DataSource, "#6");
			lc.DataSource = null;
			Assert.AreEqual (3, dataSourceChanged, "#7");
			Assert.IsNull (lc.DataSource, "#8");

			list1.Add ("whatever");
			list2.Add ("whatever");
			list1.Clear ();
			list2.Clear ();

			form.Dispose ();
		}

#if NET_2_0
		[Test] // bug #81771
		public void DataSource_BindingList1 ()
		{
			BindingList<string> list1 = new BindingList<string> ();
			list1.Add ("item 1");
			BindingList<string> list2 = new BindingList<string> ();

			ListControlChild lc = new ListControlChild ();
			lc.DataSourceChanged += new EventHandler (ListControl_DataSourceChanged);
			lc.DataSource = list1;
			Assert.AreEqual (1, dataSourceChanged, "#A1");
			Assert.AreSame (list1, lc.DataSource, "#A2");

			Form form = new Form ();
			form.Controls.Add (lc);

			Assert.AreEqual (1, dataSourceChanged, "#B1");
			Assert.AreSame (list1, lc.DataSource, "#B2");
			lc.DataSource = list2;
			Assert.AreEqual (2, dataSourceChanged, "#B3");
			Assert.AreSame (list2, lc.DataSource, "#B4");
			lc.DataSource = null;
			Assert.AreEqual (3, dataSourceChanged, "#B5");
			Assert.IsNull (lc.DataSource, "#B6");

			list1.Add ("item");
			list1.Clear ();

			form.Dispose ();
		}

		[Test] // bug #81771
		public void DataSource_BindingList2 ()
		{
			BindingList<string> list1 = new BindingList<string> ();
			list1.Add ("item 1");
			BindingList<string> list2 = new BindingList<string> ();

			ListControlChild lc = new ListControlChild ();
			lc.DataSourceChanged += new EventHandler (ListControl_DataSourceChanged);

			Form form = new Form ();
			form.Controls.Add (lc);

			Assert.AreEqual (0, dataSourceChanged, "#1");
			Assert.IsNull (lc.DataSource, "#2");
			lc.DataSource = list1;
			Assert.AreEqual (1, dataSourceChanged, "#3");
			Assert.AreSame (list1, lc.DataSource, "#4");
			lc.DataSource = list2;
			Assert.AreEqual (2, dataSourceChanged, "#5");
			Assert.AreSame (list2, lc.DataSource, "#6");
			lc.DataSource = null;
			Assert.AreEqual (3, dataSourceChanged, "#7");
			Assert.IsNull (lc.DataSource, "#8");
			list1.Add ("item");
			list1.Clear ();

			form.Dispose ();
		}

		[Test]
		public void AllowSelection ()
		{
			ListControlChild lc = new ListControlChild ();
			Assert.IsTrue (lc.allow_selection);
		}
		
		[Test]
		public void BehaviorFormatting ()
		{
			ListControl lc = new ListControlChild ();
			DateTime dt = new DateTime (1, 2, 3, 4, 5, 6);

			Assert.AreEqual (false, lc.FormattingEnabled, "A1");
			Assert.AreEqual (null, lc.FormatInfo, "A2");
			Assert.AreEqual (string.Empty, lc.FormatString, "A3");
			
			Assert.AreEqual ("2/3/0001 4:05:06 AM", lc.GetItemText (dt), "A4");
			
			lc.FormattingEnabled = true;
			lc.FormatString = "MM/dd";

			Assert.AreEqual ("02/03", lc.GetItemText (dt), "A5");

			lc.Format += new ListControlConvertEventHandler (lc_Format);
			Assert.AreEqual ("Monkey!", lc.GetItemText (dt), "A6");
		}

		void lc_Format (object sender, ListControlConvertEventArgs e)
		{
			e.Value = "Monkey!";
		}
#endif

		void ListControl_DataSourceChanged (object sender, EventArgs e)
		{
			dataSourceChanged++;
		}

		public class ListControlChild : ListControl
		{
			public override int SelectedIndex {
				get {
					return -1;
				}
				set {
				}
			}

#if NET_2_0
			public bool allow_selection {
				get { return base.AllowSelection; }
			}
#endif

			public object FilterItem (object obj, string field)
			{
				return FilterItemOnProperty (obj, field);
			}

			protected override void RefreshItem (int index)
			{
			}

			protected override void SetItemsCore (IList items)
			{
			}
		}
	}

	public class MockItem
	{
		public MockItem (string text, int value)
		{
			_text = text;
			_value = value;
		}

		public string Text {
			get { return _text; }
		}

		public int Value {
			get { return _value; }
		}

		private readonly string _text;
		private readonly int _value;
	}
}
