// DataViewTest_IBindingList.cs - Nunit Test Cases for for testing the DataView
// class for IBindingList Implementation
//
// Authors:
//      Sureshkumar T <tsureshkumar@novell.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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


using System;
using System.IO;
using System.Data;
using System.ComponentModel;

namespace MonoTests.System.Data
{
	using NUnit.Framework;

	[TestFixture]
	public class DataViewTest_IBindingList
	{
		DataTable dt = new DataTable ();

		public DataViewTest_IBindingList ()
		{
			dt.Columns.Add ("id", typeof (int));
			dt.Columns [0].AutoIncrement = true;
			dt.Columns [0].AutoIncrementSeed = 5;
			dt.Columns [0].AutoIncrementStep = 5;
			dt.Columns.Add ("name", typeof (string));

			dt.Rows.Add (new object [] { null, "mono test 1" });
			dt.Rows.Add (new object [] { null, "mono test 3" });
			dt.Rows.Add (new object [] { null, "mono test 2" });
			dt.Rows.Add (new object [] { null, "mono test 4" });
		}

		[Test]
		public void PropertyTest ()
		{
			DataView dv = new DataView (dt);
			IBindingList ib = (IBindingList) dv;
			Assert.IsTrue (ib.AllowEdit, "#1");
			Assert.IsTrue (ib.AllowNew, "#2");
			Assert.IsTrue (ib.AllowRemove, "#3");
			Assert.IsFalse (ib.IsSorted, "#4");
			Assert.AreEqual (ListSortDirection.Ascending, ib.SortDirection, "#5");
			Assert.IsTrue (ib.SupportsChangeNotification, "#6");
			Assert.IsTrue (ib.SupportsSearching, "#7");
			Assert.IsTrue (ib.SupportsSorting, "#8");
			Assert.IsNull (ib.SortProperty, "#9");
		}

		[Test]
		public void AddNewTest ()
		{
			DataView dv = new DataView (dt);
			IBindingList ib = (IBindingList) dv;
			ib.ListChanged += new ListChangedEventHandler (OnListChanged);

			try {
				args = null;
				object o = ib.AddNew ();
				Assert.AreEqual (typeof (DataRowView), o.GetType (), "#1");
				Assert.AreEqual (ListChangedType.ItemAdded, args.ListChangedType, "#1.1");
				Assert.AreEqual (4, args.NewIndex, "#1.2");
				Assert.AreEqual (-1, args.OldIndex, "#1.3");

				DataRowView r = (DataRowView) o;
				Assert.AreEqual (25, r ["id"], "#2"); 
				Assert.AreEqual (DBNull.Value, r ["name"], "#3"); 
				Assert.AreEqual (5, dv.Count, "#4");

				args = null;
				r.CancelEdit ();
				Assert.AreEqual (ListChangedType.ItemDeleted, args.ListChangedType, "#4.1");
				Assert.AreEqual (4, args.NewIndex, "#4.2");
				Assert.AreEqual (-1, args.OldIndex, "#4.3");
				Assert.AreEqual (4, dv.Count, "#5");
			} finally {
				ib.ListChanged -= new ListChangedEventHandler (OnListChanged);
			}
		}

		ListChangedEventArgs args = null;

		public void OnListChanged (object sender, ListChangedEventArgs args)
		{
			this.args = args;
		}

		[Test]
		public void SortTest ()
		{
			DataView dv = new DataView (dt);
			IBindingList ib = (IBindingList) dv;
			ib.ListChanged += new ListChangedEventHandler (OnListChanged);
			try {
				args = null;
				dv.Sort = "[id] DESC";
				Assert.AreEqual (ListChangedType.Reset, args.ListChangedType, "#0.1");
				Assert.AreEqual (-1, args.NewIndex, "#0.2");
				Assert.AreEqual (-1, args.OldIndex, "#0.3");
				Assert.IsTrue (ib.IsSorted, "#1");
				Assert.IsNotNull (ib.SortProperty, "#2");
				Assert.AreEqual (ListSortDirection.Descending, ib.SortDirection, "2.0");


				args = null;
				dv.Sort = null;
				Assert.AreEqual (ListChangedType.Reset, args.ListChangedType, "#2.1");
				Assert.AreEqual (-1, args.NewIndex, "#2.2");
				Assert.AreEqual (-1, args.OldIndex, "#2.3");
				Assert.IsFalse (ib.IsSorted, "#3");
				Assert.IsNull (ib.SortProperty, "#4");

				PropertyDescriptorCollection pds = ( (ITypedList) dv).GetItemProperties (null);
				PropertyDescriptor pd = pds.Find ("id", false);
				args = null;
				ib.ApplySort (pd, ListSortDirection.Ascending);
				Assert.AreEqual (ListChangedType.Reset, args.ListChangedType, "#4.1");
				Assert.AreEqual (-1, args.NewIndex, "#4.2");
				Assert.AreEqual (-1, args.OldIndex, "#4.3");
				Assert.IsTrue (ib.IsSorted, "#5");
				Assert.IsNotNull (ib.SortProperty, "#6");
				Assert.AreEqual ("[id]", dv.Sort, "#6.1");

				args = null;
				ib.RemoveSort ();
				Assert.AreEqual (ListChangedType.Reset, args.ListChangedType, "#6.1");
				Assert.AreEqual (-1, args.NewIndex, "#6.2");
				Assert.AreEqual (-1, args.OldIndex, "#6.3");
				Assert.IsFalse (ib.IsSorted, "#7");
				Assert.IsNull (ib.SortProperty, "#8");
				Assert.AreEqual (String.Empty, dv.Sort, "#8.1");
				args = null;

				// descending
				args = null;
				ib.ApplySort (pd, ListSortDirection.Descending);
				Assert.AreEqual (20, dv [0] [0], "#9");
				Assert.AreEqual ("[id] DESC", dv.Sort, "#10");
				args = null;
			} finally {
				ib.ListChanged -= new ListChangedEventHandler (OnListChanged);
			}
		}

		[Test]
		public void FindTest ()
		{
			DataView dv = new DataView (dt);

			IBindingList ib = (IBindingList) dv;
			ib.ListChanged += new ListChangedEventHandler (OnListChanged);
			try {
				args = null;
				dv.Sort = "id DESC";
				PropertyDescriptorCollection pds = ( (ITypedList) dv).GetItemProperties (null);
				PropertyDescriptor pd = pds.Find ("id", false);
				int index = ib.Find (pd, 15);
				Assert.AreEqual (1, index, "#1");

				// negative search
				index = ib.Find (pd, 44);
				Assert.AreEqual (-1, index, "#2");
			} finally {
				ib.ListChanged -= new ListChangedEventHandler (OnListChanged);
			}
		}

		[Test]
		public void TestIfCorrectIndexIsUsed ()
		{
			DataTable table= new DataTable ();
			table.Columns.Add ("id", typeof (int));

			table.Rows.Add (new object[] {1});
			table.Rows.Add (new object[] {2});
			table.Rows.Add (new object[] {3});
			table.Rows.Add (new object[] {4});

			DataView dv = new DataView(table);

			dv.Sort = "[id] DESC";

			// for the new view, the index thats chosen, shud be different from the the one
			// created for the older view.
			dv = new DataView (table);
			IBindingList ib = (IBindingList) dv;
			PropertyDescriptorCollection pds = ((ITypedList)dv).GetItemProperties (null);
			PropertyDescriptor pd = pds.Find ("id", false);
			int index = ib.Find (pd, 4);
			Assert.AreEqual (3, index, "#1");
		}
	}
}
