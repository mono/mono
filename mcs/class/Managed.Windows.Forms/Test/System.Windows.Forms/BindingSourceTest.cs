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
// Copyright (c) 2007 Novell, Inc.
//

#if NET_2_0

using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms.DataBinding {

	[TestFixture]
	public class BindingSourceTest : TestHelper
	{
		[Test]
		public void DefaultDataSource ()
		{
			BindingSource source = new BindingSource ();
			Assert.IsTrue (source.List is BindingList<object>, "1");
			Assert.AreEqual (0, source.List.Count, "2");
		}

		[Test]
		public void DataSource_InitialAddChangingType ()
		{
			BindingSource source = new BindingSource ();

			source.Add ((int)32);
			Assert.IsTrue (source.List is BindingList<int>, "1");

			source = new BindingSource ();
			source.DataSource = new ArrayList ();
			source.Add ((int)32);
			Assert.IsFalse (source.List is BindingList<int>, "2");
		}

		class EmptyEnumerable : IEnumerable {
			class EmptyEnumerator : IEnumerator {
				public object Current {
					get { throw new InvalidOperationException (); }
				}

				public void Reset () {
					// nada
				}

				public bool MoveNext () {
					return false;
				}

			}

			public IEnumerator GetEnumerator () {
				return new EmptyEnumerator ();
			}
		}

		class GenericEnumerable : IEnumerable<int> {
			int length;

			public GenericEnumerable (int length) {
				this.length = length;
			}

			class MyEnumerator : IEnumerator<int> {
				public int count;
				public int index;

				public int Current {
					get { return index; }
				}

				object IEnumerator.Current {
					get { return Current; }
				}

				public void Reset () {
					index = 0;
				}

				public bool MoveNext () {
					if (index++ == count)
						return false;
					else
						return true;
				}

				void IDisposable.Dispose () {
				}
			}

			public IEnumerator<int> GetEnumerator () {
				MyEnumerator e = new MyEnumerator ();
				e.count = length;

				return e;
			}

			IEnumerator IEnumerable.GetEnumerator () {
				return GetEnumerator ();
			}
		}

		class WorkingEnumerable : IEnumerable {
			int length;

			public WorkingEnumerable (int length) {
				this.length = length;
			}

			class MyEnumerator : IEnumerator {
				public int count;
				public int index;

				public object Current {
					get { return index; }
				}

				public void Reset () {
					index = 0;
				}

				public bool MoveNext () {
					if (index++ == count)
						return false;
					else
						return true;
				}
			}

			public IEnumerator GetEnumerator () {
				MyEnumerator e = new MyEnumerator ();
				e.count = length;

				return e;
			}
		}

		[Test]
		public void DataSource_ListRelationship ()
		{
			BindingSource source = new BindingSource ();

			// null
			source.DataSource = null;
			Assert.IsTrue (source.List is BindingList<object>, "1");

			// a non-list object
			source.DataSource = new object ();
			Assert.IsTrue (source.List is BindingList<object>, "2");

			// array instance (value type)
			source.DataSource = new int[32];
			Assert.IsTrue (source.List is int[], "3");

			// an instance array with 0 elements
			source.DataSource = new int[0];
			Assert.IsTrue (source.List is int[], "4");

			// array instance (object type)
			source.DataSource = new string[32];
			Assert.IsTrue (source.List is string[], "5");

			// list type
			source.DataSource = new List<bool>();
			Assert.IsTrue (source.List is List<bool>, "6");

			// an IEnumerable type
			source.DataSource = "hi";
			Assert.IsTrue (source.List is BindingList<char>, "7");

			// an IEnumerable type with 0 items
			source.DataSource = "";
			Assert.IsTrue (source.List is BindingList<char>, "8");
			Assert.AreEqual (0, source.List.Count, "9");

			// a generic enumerable with no elements.
			// even though we can figure out the type
			// through reflection, we shouldn't..
			source.DataSource = new GenericEnumerable (0);
			Console.WriteLine (source.List.GetType());
			Assert.IsTrue (source.List is BindingList<char>, "10");
			Assert.AreEqual (0, source.List.Count, "11");

			// a non-generic IEnumerable type with 0 items
			// this doesn't seem to change the type of the
			// binding source's list, probably because it
			// can't determine the type of the
			// enumerable's elements.
			source.DataSource = new EmptyEnumerable ();
			Assert.IsTrue (source.List is BindingList<char>, "12");

			// an enumerable with some elements
			source.DataSource = new WorkingEnumerable (5);
			Assert.IsTrue (source.List is BindingList<int>, "13");
			Assert.AreEqual (5, source.List.Count, "14");

			// IListSource - returns an array
			source.DataSource = new ListBindingHelperTest.ListSource (true);
			Assert.IsTrue (source.List is Array, "#15");
			Assert.AreEqual (1, source.List.Count, "#16");
		}

		[Test]
		public void Filter ()
		{
			BindingSource source = new BindingSource ();
			DataTable table = new DataTable ();
			string filter = "Name = 'Mono'";
			IBindingListView view;

			table.Columns.Add ("Id", typeof (int));
			table.Columns.Add ("Name", typeof (string));

			table.Rows.Add (0, "Mono");
			table.Rows.Add (1, "Miguel");
			table.Rows.Add (2, "Paolo");
			table.Rows.Add (3, "Mono");

			source.DataSource = table;
			Assert.AreEqual (null, source.Filter, "A1");

			source.Filter = filter;
			view = (IBindingListView)((IListSource)table).GetList ();
			Assert.AreEqual (filter, source.Filter, "B1");
			Assert.AreEqual (filter, view.Filter, "B2");
			Assert.AreEqual (2, view.Count, "B3");
			Assert.AreEqual (2, source.List.Count, "B4");

			source.Filter = String.Empty;
			Assert.AreEqual (String.Empty, source.Filter, "C1");
			Assert.AreEqual (String.Empty, view.Filter, "C2");
			Assert.AreEqual (4, view.Count, "C3");
			Assert.AreEqual (4, source.List.Count, "C4");

			source.DataSource = null;
			Assert.AreEqual (String.Empty, source.Filter, "D1"); // Keep previous value

			filter = "Name = 'Miguel'"; // Apply filter before assigning data source
			source.Filter = filter;
			source.DataSource = table;

			view = (IBindingListView)((IListSource)table).GetList ();
			Assert.AreEqual (filter, source.Filter, "E1");
			Assert.AreEqual (filter, view.Filter, "E2");
			Assert.AreEqual (1, view.Count, "E3");
			Assert.AreEqual (1, source.List.Count, "E4");
		}

		[Test]
		public void Filter_NonBindingListView ()
		{
			BindingSource source = new BindingSource ();
			List<int> list = new List<int> ();
			list.AddRange (new int [] { 0, 1, 2 });
			string filter = "NonExistentColumn = 'A'"; ;

			source.DataSource = list;
			Assert.AreEqual (null, source.Filter, "A1");

			// List<> doesn't implement IBindingListView, but
			// the filter string is saved
			source.Filter = filter;
			Assert.AreEqual (filter, source.Filter, "B1");

			source.Filter = null;
			Assert.AreEqual (null, source.Filter, "C1");
		}

		[Test]
		public void RemoveFilter ()
		{
			BindingSource source = new BindingSource ();
			source.Filter = "Name = 'Something'";
			source.RemoveFilter ();

			Assert.AreEqual (null, source.Filter, "A1");
		}

		[Test]
		public void RemoveSort ()
		{
			BindingSource source = new BindingSource ();
			DataTable table = CreateTable ();
			source.DataSource = table;

			source.Sort = "Name";
			IBindingListView view = (IBindingListView)source.List;
			Assert.AreEqual ("Name", source.Sort, "A1");
			Assert.AreEqual ("Name", view.SortProperty.Name, "A2");

			source.RemoveSort ();
			Assert.AreEqual (null, source.Sort, "B1");
			Assert.AreEqual (null, view.SortProperty, "B2");

			// Non IBindingListView source - No exception, as opposed to what
			// the documentation says
			source.Sort = null;
			source.DataSource = new List<string> ();

			source.RemoveSort ();
		}

		[Test]
		public void ResetItem ()
		{
			BindingSource source = new BindingSource ();
			bool delegate_reached = false;
			int old_index = 5;
			int new_index = 5;
			ListChangedType type = ListChangedType.Reset;

			source.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				delegate_reached = true;
				type = e.ListChangedType;
				old_index = e.OldIndex;
				new_index = e.NewIndex;
			};

			source.ResetItem (0);

			Assert.IsTrue (delegate_reached, "1");
			Assert.AreEqual (-1, old_index, "2");
			Assert.AreEqual (0, new_index, "3");
			Assert.AreEqual (ListChangedType.ItemChanged, type, "3");
		}

		DataTable CreateTable ()
		{
			DataTable table = new DataTable ();

			table.Columns.Add ("Id", typeof (int));
			table.Columns.Add ("Name", typeof (string));

			table.Rows.Add (0, "Mono");
			table.Rows.Add (2, "JPobst");
			table.Rows.Add (1, "Miguel");

			return table;
		}

		[Test]
		public void Sort_IBindingList ()
		{
			BindingSource source = new BindingSource ();
			BindingList<string> list = new BindingList<string> ();

			source.DataSource = list;

			// Implements IBindingList but SupportsSorting is false
			try {
				source.Sort = "Name";
				Assert.Fail ("A1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Sort_IBindingListView ()
		{
			BindingSource source = new BindingSource ();
			DataTable table = CreateTable ();

			// 
			// Simple
			//
			source.DataSource = table;
			source.Sort = "Name";

			DataView view = (DataView)((IListSource)table).GetList ();
			Assert.AreEqual ("Name", source.Sort, "A1");
			Assert.AreEqual (ListSortDirection.Ascending, ((IBindingListView) source).SortDirection, "A2");
			Assert.AreEqual (ListSortDirection.Ascending, ((IBindingListView)view).SortDirection, "A3");
			Assert.AreEqual ("Name", ((IBindingListView)source).SortProperty.Name, "A4");
			Assert.AreEqual ("Name", ((IBindingListView)view).SortProperty.Name, "A5");
			Assert.AreEqual (1, ((IBindingListView)view).SortDescriptions.Count, "A6");
			Assert.AreEqual ("Name", ((IBindingListView)view).SortDescriptions [0].PropertyDescriptor.Name, "A7");
			Assert.AreEqual ("JPobst", view [0]["Name"], "A8");
			Assert.AreEqual ("Miguel", view [1]["Name"], "A9");
			Assert.AreEqual ("Mono", view [2]["Name"], "A10");

			//
			// Simple with direction (extra white spaces)
			//
			source.Sort = "   Name  DESC   ";

			//Assert.AreEqual ("Name DESC", source.Sort, "B1");
			Assert.AreEqual (ListSortDirection.Descending, ((IBindingListView)view).SortDirection, "B2");
			Assert.AreEqual ("Name", ((IBindingListView)view).SortProperty.Name, "B3");
			Assert.AreEqual ("Mono", view [0]["Name"], "B4");
			Assert.AreEqual ("Miguel", view [1]["Name"], "B5");
			Assert.AreEqual ("JPobst", view [2]["Name"], "B6");

			// 
			// Multiple
			//
			source.Sort = "Name DESC, Id asc";

			ListSortDescriptionCollection desc_coll = ((IBindingListView)view).SortDescriptions;
			Assert.AreEqual ("Name DESC, Id asc", source.Sort, "C1");
			Assert.AreEqual (2, desc_coll.Count, "C2");
			Assert.AreEqual (ListSortDirection.Descending, desc_coll [0].SortDirection, "C3");
			Assert.AreEqual ("Name", desc_coll [0].PropertyDescriptor.Name, "C4");
			Assert.AreEqual (ListSortDirection.Ascending, desc_coll [1].SortDirection, "C5");
			Assert.AreEqual ("Id", desc_coll [1].PropertyDescriptor.Name, "C6");
		}

		[Test]
		public void Sort_NonBindingList ()
		{
			BindingSource source = new BindingSource ();
			List<int> list = new List<int> (new int [] { 0, 1, 2, 3 });

			source.DataSource = list;
			Assert.AreEqual (null, source.Sort, "A1");

			try {
				source.Sort = "Name";
				Assert.Fail ("B1");
			} catch (ArgumentException) {
			}

			source.Sort = String.Empty;
			Assert.AreEqual (String.Empty, source.Sort, "C1");
		}

		[Test]
		public void Sort_Exceptions ()
		{
			BindingSource source = new BindingSource ();
			DataTable table = CreateTable ();

			source.DataSource = table;

			// Non-existant property
			try {
				source.Sort = "Name, DontExist DESC";
				Assert.Fail ("exc1");
			} catch (ArgumentException) {
			}

			// Wrong direction
			try {
				source.Sort = "Name WRONGDIR";
				Assert.Fail ("exc2");
			} catch (ArgumentException) {
			}

			// Wrong format
			try {
				source.Sort = "Name, , Id";
				Assert.Fail ("exc3");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Movement ()
		{
			BindingSource source = new BindingSource ();
			source.DataSource = new WorkingEnumerable (5);

			int raised = 0;
			source.PositionChanged += delegate (object sender, EventArgs e) { raised ++; };

			Console.WriteLine ("count = {0}", source.Count);
			source.Position = 3;
			Assert.AreEqual (3, source.Position, "1");

			source.MoveFirst ();
			Assert.AreEqual (0, source.Position, "2");

			source.MoveNext ();
			Assert.AreEqual (1, source.Position, "3");

			source.MovePrevious ();
			Assert.AreEqual (0, source.Position, "4");

			source.MoveLast ();
			Assert.AreEqual (4, source.Position, "5");

			Assert.AreEqual (5, raised, "6");
		}

		[Test]
		public void Position ()
		{
			BindingSource source = new BindingSource ();
			CurrencyManager currency_manager = source.CurrencyManager;

			Assert.AreEqual (-1, source.Position, "A1");
			Assert.AreEqual (-1, currency_manager.Position, "A2");

			source.DataSource = new WorkingEnumerable (5);

			int raised = 0;
			int currency_raised = 0;
			source.PositionChanged += delegate (object sender, EventArgs e) { raised ++; };
			currency_manager.PositionChanged += delegate (object sender, EventArgs e) { currency_raised++; };


			Assert.AreEqual (0, source.Position, "B1");
			Assert.AreEqual (0, currency_manager.Position, "B2");

			source.Position = -1;
			Assert.AreEqual (0, source.Position, "C1");
			Assert.AreEqual (0, currency_manager.Position, "C2");
			Assert.AreEqual (0, raised, "C3");
			Assert.AreEqual (0, currency_raised, "C4");

			source.Position = 10;
			Assert.AreEqual (4, source.Position, "D1");
			Assert.AreEqual (4, currency_manager.Position, "D2");
			Assert.AreEqual (1, raised, "D3");
			Assert.AreEqual (1, currency_raised, "D4");

			source.Position = 10;
			Assert.AreEqual (4, source.Position, "E1");
			Assert.AreEqual (1, raised, "E2");

			// Now make some changes in CurrencyManager.Position, which should be visible
			// in BindingSource.Position

			currency_manager.Position = 0;
			Assert.AreEqual (0, source.Position, "F1");
			Assert.AreEqual (0, currency_manager.Position, "F2");
			Assert.AreEqual (2, raised, "F3");
			Assert.AreEqual (2, currency_raised, "F4");

			// Finally an etmpy collection
			source.DataSource = new List<int> ();

			Assert.AreEqual (-1, source.Position, "G1");
			Assert.AreEqual (-1, currency_manager.Position, "G2");
		}

		[Test]
		public void ResetCurrentItem ()
		{
			BindingSource source = new BindingSource ();
			bool delegate_reached = false;
			int old_index = 5;
			int new_index = 5;
			ListChangedType type = ListChangedType.Reset;

			source.DataSource = new WorkingEnumerable (5);
			source.Position = 2;

			source.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				delegate_reached = true;
				type = e.ListChangedType;
				old_index = e.OldIndex;
				new_index = e.NewIndex;
			};

			source.ResetCurrentItem ();

			Assert.IsTrue (delegate_reached, "1");
			Assert.AreEqual (-1, old_index, "2");
			Assert.AreEqual (2, new_index, "3");
			Assert.AreEqual (ListChangedType.ItemChanged, type, "3");
		}

		[Test]
		public void Remove ()
		{
			BindingSource source = new BindingSource ();

			List<string> list = new List<string> ();
			list.Add ("A");
			source.DataSource = list;
			Assert.AreEqual (1, source.List.Count, "1");

			source.Remove ("A");
			Assert.AreEqual (0, list.Count, "2");

			// Different type, - no exception
			source.Remove (7);

			// Fixed size
			try {
				source.DataSource = new int [0];
				source.Remove (7);
				Assert.Fail ("exc1");
			} catch (NotSupportedException) {
			}

			// Read only
			try {
				source.DataSource = Array.AsReadOnly (new int [0]);
				source.Remove (7);
				Assert.Fail ("exc2");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		public void RemoveCurrent ()
		{
			BindingSource source = new BindingSource ();
			List<string> list = new List<string> ();
			list.Add ("A");
			list.Add ("B");
			list.Add ("C");
			source.DataSource = list;

			source.Position = 1;
			Assert.AreEqual (1, source.Position, "A1");
			Assert.AreEqual ("B", source.Current, "A2");

			source.RemoveCurrent ();
			Assert.AreEqual (1, source.Position, "B1");
			Assert.AreEqual ("C", source.Current, "B2");
			Assert.AreEqual (2, source.Count, "B3");
			Assert.AreEqual ("A", source [0], "B4");
			Assert.AreEqual ("C", source [1], "B5");

			// Position is -1, since there are no items
			source.Clear ();
			try {
				source.RemoveCurrent ();
				Assert.Fail ("exc1");
			} catch (InvalidOperationException) {
			}

			source.DataSource = new int [1];
			try {
				source.RemoveCurrent ();
				Assert.Fail ("exc2");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void ResetBindings ()
		{
			BindingSource source;
			int event_count = 0;

			ListChangedType[] types = new ListChangedType[2];
			int[] old_index = new int[2];
			int[] new_index = new int[2];

			source = new BindingSource ();
			source.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				types[event_count] = e.ListChangedType;
				old_index[event_count] = e.OldIndex;
				new_index[event_count] = e.NewIndex;
				event_count ++;
			};

			event_count = 0;
			source.ResetBindings (false);

			Assert.AreEqual (1, event_count, "1");
			Assert.AreEqual (ListChangedType.Reset, types[0], "2");
			Assert.AreEqual (-1, old_index[0], "3");
			Assert.AreEqual (-1, new_index[0], "4");

			event_count = 0;
			source.ResetBindings (true);
			Assert.AreEqual (2, event_count, "5");
			Assert.AreEqual (ListChangedType.PropertyDescriptorChanged, types[0], "6");
			Assert.AreEqual (0, old_index[0], "7");
			Assert.AreEqual (0, new_index[0], "8");

			Assert.AreEqual (ListChangedType.Reset, types[1], "9");
			Assert.AreEqual (-1, old_index[1], "10");
			Assert.AreEqual (-1, new_index[1], "11");
		}

		[Test]
		public void AllowEdit ()
		{
			BindingSource source = new BindingSource ();

			Assert.IsTrue (source.AllowEdit, "1");

			source.DataSource = "";
			Assert.IsTrue (source.AllowEdit, "2");

			source.DataSource = new int[10];
			Assert.IsTrue (source.AllowEdit, "3");

			source.DataSource = new WorkingEnumerable (5);
			Assert.IsTrue (source.AllowEdit, "4");

			ArrayList al = new ArrayList ();
			al.Add (5);

			source.DataSource = al;
			Assert.IsTrue (source.AllowEdit, "5");

			source.DataSource = ArrayList.ReadOnly (al);
			Assert.IsFalse (source.AllowEdit, "6");
		}

		[Test]
		public void AllowRemove ()
		{
			BindingSource source = new BindingSource ();

			Assert.IsTrue (source.AllowRemove, "1");

			source.DataSource = "";
			Assert.IsTrue (source.AllowRemove, "2");

			source.DataSource = new ArrayList ();
			Assert.IsTrue (source.AllowRemove, "3");

			source.DataSource = new int[10];
			Assert.IsFalse (source.AllowRemove, "4");

			source.DataSource = new WorkingEnumerable (5);
			Assert.IsTrue (source.AllowRemove, "5");
		}

		[Test]
		public void DataMember_ListRelationship ()
		{
			ListView lv = new ListView ();
			BindingSource source = new BindingSource ();

			// Empty IEnumerable, that also implements IList
			source.DataSource = lv.Items;
			source.DataMember = "Text";
			Assert.IsTrue (source.List is BindingList<string>, "1");
			Assert.AreEqual (0, source.List.Count, "2");
		}

		[Test]
		public void DataMemberNull ()
		{
			BindingSource source = new BindingSource ();

			Assert.AreEqual ("", source.DataMember, "1");
			source.DataMember = null;
			Assert.AreEqual ("", source.DataMember, "2");
		}

		[Test]
		public void DataSourceChanged ()
		{
			ArrayList list = new ArrayList ();
			BindingSource source = new BindingSource ();

			bool event_raised = false;

			source.DataSourceChanged += delegate (object sender, EventArgs e) {
				event_raised = true;
			};

			source.DataSource = list;
			Assert.IsTrue (event_raised, "1");
			event_raised = false;
			source.DataSource = list;
			Assert.IsFalse (event_raised, "2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // DataMember property 'hi' cannot be found on the DataSource.
		public void DataMemberArgumentException ()
		{
			ArrayList list = new ArrayList ();
			BindingSource source = new BindingSource ();
			source.DataSource = list;
			source.DataMember = "hi";
		}

		[Test]
		public void DataMemberBeforeDataSource ()
		{
			ArrayList list = new ArrayList ();
			BindingSource source = new BindingSource ();
			source.DataMember = "hi";
			Assert.AreEqual ("hi", source.DataMember, "1");
			source.DataSource = list;
			Assert.AreEqual ("", source.DataMember, "2");
		}

		[Test]
		public void DataSourceAssignToDefaultType()
		{
			BindingSource source = new BindingSource ();
			source.DataMember = "hi";
			Assert.AreEqual ("hi", source.DataMember, "1");
			Assert.IsTrue (source.List is BindingList<object>, "2");
			source.DataSource = new BindingList<object>();
			Assert.AreEqual ("", source.DataMember, "3");
		}

		[Test]
		public void DataSourceSetType ()
		{
			BindingSource source = new BindingSource ();
			source.DataSource = typeof (DateTime);

			Assert.IsTrue (source.List is BindingList<DateTime>, "A1");
			Assert.AreEqual (0, source.List.Count, "A2");
			Assert.AreEqual (typeof (DateTime), source.DataSource);
		}

		[Test]
		public void DataMemberChanged ()
		{
			ArrayList list = new ArrayList ();
			BindingSource source = new BindingSource ();

			bool event_raised = false;

			list.Add ("hi"); // make the type System.String

			source.DataMemberChanged += delegate (object sender, EventArgs e) {
				event_raised = true;
			};

			source.DataSource = list;
			source.DataMember = "Length";
			Assert.IsTrue (event_raised, "1");
			event_raised = false;
			source.DataMember = "Length";
			Assert.IsFalse (event_raised, "2");
		}

		[Test]
		public void DataMemberNullDataSource ()
		{
			BindingSource source = new BindingSource ();

			source.Add ("hellou");
			source.DataMember = "SomeProperty"; // Should reset the list, even if data source is null

			Assert.IsTrue (source.List is BindingList<object>, "A1");
			Assert.AreEqual (0, source.List.Count, "A2");
		}

		[Test]
		public void SuppliedDataSource ()
		{
			List<string> list = new List<string>();

			BindingSource source;

			source = new BindingSource (list, "");
			Assert.IsTrue (source.List is List<string>, "1");

			source.DataMember = "Length";
			Assert.IsTrue (source.List is BindingList<int>, "2");

			source = new BindingSource (list, "Length");
			Assert.IsTrue (source.List is BindingList<int>, "3");
		}

		[Test]
		public void DataSourceMember_set ()
		{
			BindingSource source = new BindingSource ();

			source.DataSource = new List<string>();
			source.DataMember = "Length";
			Assert.IsNotNull (source.CurrencyManager, "1");

			source.DataSource = new List<string>();
			Assert.AreEqual ("Length", source.DataMember, "2");
			Assert.IsNotNull (source.CurrencyManager, "3");

			source.DataSource = new List<string[]>();
			Assert.AreEqual ("Length", source.DataMember, "4");
			Assert.IsNotNull (source.CurrencyManager, "5");
		}

		[Test]
		public void DataSourceMemberChangedEvents ()
		{
			BindingSource source = new BindingSource ();

			bool data_source_changed = false;
			bool data_member_changed = false;

			source.DataSourceChanged += delegate (object sender, EventArgs e) {
				data_source_changed = true;
			};
			source.DataMemberChanged += delegate (object sender, EventArgs e) {
				data_member_changed = true;
			};

			data_source_changed = false;
			data_member_changed = false;
			source.DataSource = new List<string>();
			Assert.IsTrue (data_source_changed, "1");
			Assert.IsFalse (data_member_changed, "2");

			data_source_changed = false;
			data_member_changed = false;
			source.DataMember = "Length";
			Assert.IsFalse (data_source_changed, "3");
			Assert.IsTrue (data_member_changed, "4");
		}

		[Test]
		public void IsBindingSuspended ()
		{
			BindingSource source = new BindingSource ();
			CurrencyManager currency_manager = source.CurrencyManager;
			source.DataSource = new object [1];

			source.SuspendBinding ();
			Assert.AreEqual (true, source.IsBindingSuspended, "A1");
			Assert.AreEqual (true, currency_manager.IsBindingSuspended, "A2");

			source.ResumeBinding ();
			Assert.AreEqual (false, source.IsBindingSuspended, "B1");
			Assert.AreEqual (false, currency_manager.IsBindingSuspended, "B2");

			// Changes made to CurrencyManager should be visible in BindingSource
			currency_manager.SuspendBinding ();
			Assert.AreEqual (true, source.IsBindingSuspended, "C1");
			Assert.AreEqual (true, currency_manager.IsBindingSuspended, "C2");

			currency_manager.ResumeBinding ();
			Assert.AreEqual (false, source.IsBindingSuspended, "D1");
			Assert.AreEqual (false, currency_manager.IsBindingSuspended, "D2");
		}

		[Test]
		public void Add ()
		{
			BindingSource source = new BindingSource ();

			source.DataSource = new List<string> ();
			source.Add ("A");
			Assert.AreEqual (1, source.List.Count, "2");

			// Different item type
			try {
				source.Add (4);
				Assert.Fail ("exc1");
			} catch (InvalidOperationException) {
			}

			// FixedSize
			try {
				source.DataSource = new int [0];
				source.Add (7);
				Assert.Fail ("exc2");
			} catch (NotSupportedException) {
			}

			// ReadOnly
			try {
				source.DataSource = Array.AsReadOnly (new int [0]);
				source.Add (7);
				Assert.Fail ("exc3");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		public void Add_NullDataSource ()
		{
			BindingSource source = new BindingSource ();

			source.Add ("A");
			Assert.AreEqual (1, source.List.Count, "1");
			Assert.IsTrue (source.List is BindingList<string>, "2");
			Assert.IsNull (source.DataSource, "3");

			source = new BindingSource ();
			source.Add (null);
			Assert.IsTrue (source.List is BindingList<object>, "4");
			Assert.AreEqual (1, source.List.Count, "5");
		}

		[Test]
		public void AddNew ()
		{
			BindingSource source = new BindingSource ();
			source.AddNew ();
			Assert.AreEqual (1, source.Count, "1");
		}

		[Test]
		public void AddNew_NonBindingList ()
		{
			IList list = new List<object> ();
			BindingSource source = new BindingSource ();
			source.DataSource = list;
			Assert.IsTrue (source.List is List<object>, "1");
			source.AddNew ();
			Assert.AreEqual (1, source.Count, "2");
		}

		[Test]
		public void ApplySort ()
		{
			BindingSource source = new BindingSource ();
			DataTable table = CreateTable ();

			source.DataSource = table;
			IBindingListView source_view = ((IBindingListView)source);
			IBindingListView view = ((IBindingListView)source.List);
			PropertyDescriptor property = source.GetItemProperties (null) ["Name"];

			source_view.ApplySort (property, ListSortDirection.Ascending);

			Assert.AreEqual (property, view.SortProperty, "A1");

			// Non IBindingList source - Passing an invalid property
			// but the method is not called since source is not of the required type

			source.DataSource = new List<string> ();
			try {
				source_view.ApplySort (property, ListSortDirection.Ascending);
				Assert.Fail ("B1");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		public void AllowNew_getter ()
		{
			BindingSource source = new BindingSource ();

			// true because the default datasource is BindingList<object>
			Assert.IsTrue (source.AllowNew, "1");

			source.DataSource = new object[10];

			// fixed size
			Assert.IsFalse (source.AllowNew, "2");

			source.DataSource = new BindingList<string>();

			// no default ctor
			Assert.IsFalse (source.AllowNew, "3");
		}

		[Test]
		public void AllowNew ()
		{
			BindingSource source = new BindingSource ();
			source.AllowNew = false;

			Assert.IsFalse (source.AllowNew, "1");

			source.ResetAllowNew ();

			Assert.IsTrue (source.AllowNew, "2");
		}


		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// "AllowNew can only be set to true on an
		// IBindingList or on a read-write list with a default
		// public constructor."
		public void AllowNew_FixedSize ()
		{
			BindingSource source = new BindingSource ();
			source.DataSource = new object[10];

			source.AllowNew = true;
		}

#if false
		// According to the MS docs, this should fail with a MissingMethodException

		[Test]
		public void AllowNew_NoDefaultCtor ()
		{
			List<string> s = new List<string>();
			s.Add ("hi");

			BindingSource source = new BindingSource ();
			source.DataSource = s;

			source.AllowNew = true;

			Assert.Fail ();
		}
#endif

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// "Item cannot be added to a read-only or fixed-size list."
		public void AddNew_BindingListWithoutAllowNew ()
		{
			BindingList<int> l = new BindingList<int>();
			l.AllowNew = false;

			BindingSource source = new BindingSource ();
			source.DataSource = l;
			source.AddNew ();
			Assert.AreEqual (1, source.Count, "1");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// "Item cannot be added to a read-only or fixed-size list."
		public void AddNew_FixedSize ()
		{
			BindingSource source = new BindingSource ();
			source.DataSource = new int[5];
			object o = source.AddNew ();
			Assert.IsTrue (o is int, "1");
			Assert.AreEqual (6, source.Count, "2");
		}

		class ReadOnlyList : List<int>, IList {
			public int Add (object value) {
				throw new Exception ();
			}

			public bool Contains (object value) {
				throw new Exception ();
			}
			public int IndexOf (object value) {
				throw new Exception ();
			}

			public void Insert (int index, object value) {
				throw new Exception ();
			}
			public void Remove (object value) {
				throw new Exception ();
			}

			public bool IsFixedSize {
				get { return false; }
			}
			public bool IsReadOnly {
				get { return true; }
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// "Item cannot be added to a read-only or fixed-size list."
		public void AddNew_ReadOnly ()
		{
			BindingSource source = new BindingSource ();
			source.DataSource = new ReadOnlyList ();
			object o = source.AddNew ();
			
			TestHelper.RemoveWarning (o);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// "AddNew cannot be called on the 'System.String' type.  This type does not have a public default constructor.  You can call AddNew on the 'System.String' type if you set AllowNew=true and handle the AddingNew event."
		public void AddNew_Invalid ()
		{
			BindingSource source = new BindingSource ();
			source.DataSource = new List<string>();
			object o = source.AddNew ();
			
			TestHelper.RemoveWarning (o);
		}

		[Test]
		public void AddingNew ()
		{
			// Need to use a class missing a default .ctor
			BindingSource source = new BindingSource ();
			List<DateTime> list = new List<DateTime> ();
			source.DataSource = list;

			Assert.AreEqual (false, source.AllowNew, "A1");

			source.AllowNew = true;
			source.AddingNew += delegate (object o, AddingNewEventArgs args)
			{
				args.NewObject = DateTime.Now;
			};

			source.AddNew ();
			Assert.AreEqual (1, source.Count, "A1");
		}

		[Test]
		public void AddingNew_Exceptions ()
		{
			BindingSource source = new BindingSource ();
			List<DateTime> list = new List<DateTime> ();
			source.DataSource = list;

			Assert.AreEqual (false, source.AllowNew, "A1");

			source.AllowNew = true;

			// No handler for AddingNew
			try {
				source.AddNew ();
				Assert.Fail ("exc1");
			} catch (InvalidOperationException) {
			}

			// Adding new handled, but AddingNew is false
			source.AllowNew = false;
			source.AddingNew += delegate (object o, AddingNewEventArgs args)
			{
				args.NewObject = DateTime.Now;
			};

			try {
				source.AddNew ();
				Assert.Fail ("exc2");
			} catch (InvalidOperationException) {
			}

			// Wrong type
			source = new BindingSource ();
			source.DataSource = new List<string> ();
			source.AllowNew = true;
			source.AddingNew += delegate (object o, AddingNewEventArgs args)
			{
				args.NewObject = 3.1416;
			};

			try {
				source.AddNew ();
				Assert.Fail ("exc3");
			} catch (InvalidOperationException) {
			}

			// Null value
			source = new BindingSource ();
			source.DataSource = new List<string> ();
			source.AllowNew = true;
			source.AddingNew += delegate (object o, AddingNewEventArgs args)
			{
				args.NewObject = null;
			};

			try {
				source.AddNew ();
				Assert.Fail ("exc4");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void BindingSuspended1 ()
		{
			// Empty collection as datasource means CurrencyManager will remain
			// as suspended
			BindingSource source = new BindingSource ();

			source.DataSource = new List<string>();

			Assert.IsTrue (source.IsBindingSuspended, "1");
			source.SuspendBinding ();
			Assert.IsTrue (source.IsBindingSuspended, "2");
			source.ResumeBinding ();
			Assert.IsTrue (source.IsBindingSuspended, "3");
			source.ResumeBinding ();
			Assert.IsTrue (source.IsBindingSuspended, "4");
		}

		[Test]
		public void ICancelAddNew ()
		{
			BindingSource source = new BindingSource ();
			source.DataSource = new List<string> ();
			source.AddingNew += delegate (object o, AddingNewEventArgs args) { args.NewObject = "A"; };
			source.AllowNew = true;

			// CancelNew
			source.AddNew ();
			Assert.AreEqual (1, source.Count, "A1");
			Assert.AreEqual ("A", source [0], "A2");

			((ICancelAddNew)source).CancelNew (0);
			Assert.AreEqual (0, source.Count, "A3");

			// EndNew
			source.AddNew ();
			((ICancelAddNew)source).EndNew (0);
			((ICancelAddNew)source).CancelNew (0);
			Assert.AreEqual (1, source.Count, "B1");
			Assert.AreEqual ("A", source [0], "B2");

			//
			// Access operations are suppoused to automatically
			// call EndNew, but that only happens with AddNew
			//

			// AddNew
			source.Clear ();

			source.AddNew ();
			source.AddNew ();
			((ICancelAddNew)source).CancelNew (0);
			Assert.AreEqual (2, source.Count, "C1");
			Assert.AreEqual ("A", source [0], "C2");
			Assert.AreEqual ("A", source [1], "C3");

			// Add - Does not call EndNew
			source.Clear ();
			source.AddNew ();
			source.Add ("B");

			((ICancelAddNew)source).CancelNew (0);
			Assert.AreEqual (1, source.Count, "D1");
			Assert.AreEqual ("B", source [0], "D2");

			// Remove - Does not neither
			source.Clear ();
			source.AddNew ();
			source.Add ("B");
			source.Remove ("B");

			((ICancelAddNew)source).CancelNew (0);
			Assert.AreEqual (0, source.Count, "E1");

			// Wrong index param passed to CancelNew
			source.Clear ();
			source.AddNew ();
			source.Add ("B");

			((ICancelAddNew)source).CancelNew (1); // Should pass 0
			Assert.AreEqual (2, source.Count, "F1");

			// Multiple pending items - Only takes into account the last one
			source.Clear ();
			source.AddNew ();
			source.AddNew ();

			((ICancelAddNew)source).CancelNew (1);
			((ICancelAddNew)source).CancelNew (0);
			Assert.AreEqual (1, source.Count, "G1");

			// Bug?
			source.Clear ();
			source.AddNew ();
			source.Add ("B");

			source.RemoveAt (0); // Added with AddNew
			((ICancelAddNew)source).CancelNew (0); // Removed item that wasn't added with AddNew
			Assert.AreEqual (0, source.Count, "H1");
		}

		[Test]
		public void Clear ()
		{
			BindingSource source = new BindingSource ();
			List<string> list = new List<string> ();
			list.Add ("A");
			list.Add ("B");

			source.DataSource = list;
			source.Clear ();
			Assert.AreEqual (0, source.List.Count, "1");
			Assert.IsTrue (source.List is List<string>, "2");

			// Exception only for ReadOnly, not for fixed size
			source.DataSource = new int [0];
			source.Clear ();

			try {
				source.DataSource = Array.AsReadOnly (new int [0]);
				source.Clear ();
				Assert.Fail ("exc1");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		public void Clear_NullDataSource ()
		{
			BindingSource source = new BindingSource ();

			source.Add ("A");
			Assert.AreEqual (1, source.List.Count, "1");
			Assert.IsTrue (source.List is BindingList<string>, "2");
			Assert.IsNull (source.DataSource, "3");

			source.Clear ();
			Assert.AreEqual (0, source.List.Count, "4");
			Assert.IsTrue (source.List is BindingList<string>, "5");

			// Change list item type after clearing
			source.Add (7);
			Assert.AreEqual (1, source.List.Count, "6");
			Assert.IsTrue (source.List is BindingList<int>, "7");
		}

		[Test]
		public void CurrencyManager ()
		{
			BindingSource source = new BindingSource ();
			CurrencyManager curr_manager;

			// 
			// Null data source
			//
			curr_manager = source.CurrencyManager;
			Assert.IsTrue (curr_manager != null, "A1");
			Assert.IsTrue (curr_manager.List != null, "A2");
			Assert.IsTrue (curr_manager.List is BindingSource, "A3");
			Assert.AreEqual (0, curr_manager.List.Count, "A4");
			Assert.AreEqual (0, curr_manager.Count, "A5");
			Assert.AreEqual (-1, curr_manager.Position, "A6");
			Assert.IsTrue (curr_manager.Bindings != null, "A7");
			Assert.AreEqual (0, curr_manager.Bindings.Count, "A8");
			Assert.AreEqual (source, curr_manager.List, "A9");

			// 
			// Non null data source
			//
			List<string> list = new List<string> ();
			list.Add ("A");
			list.Add ("B");
			source.DataSource = list;
			curr_manager = source.CurrencyManager;
			Assert.IsTrue (curr_manager != null, "B1");
			Assert.IsTrue (curr_manager.List != null, "B2");
			Assert.IsTrue (curr_manager.List is BindingSource, "B3");
			Assert.AreEqual (2, curr_manager.List.Count, "B4");
			Assert.AreEqual (2, curr_manager.Count, "B5");
			Assert.AreEqual (0, curr_manager.Position, "B6");
			Assert.IsTrue (curr_manager.Bindings != null, "B7");
			Assert.AreEqual (0, curr_manager.Bindings.Count, "B8");
			Assert.AreEqual (source, curr_manager.List, "B9");

			curr_manager.Position = source.Count - 1;
			Assert.AreEqual (1, curr_manager.Position, "C1");
			Assert.AreEqual ("B", curr_manager.Current, "C2");
			Assert.AreEqual (1, source.Position, "C3");
			Assert.AreEqual ("B", source.Current, "C4");
		}

		[Test]
		public void GetRelatedCurrencyManagerList ()
		{
			ListView lv = new ListView ();
			lv.Columns.Add ("A");
			BindingSource source = new BindingSource ();
			source.DataSource = lv;

			CurrencyManager cm = source.GetRelatedCurrencyManager ("Columns");
			BindingSource related_source = (BindingSource)cm.List;
			Assert.AreEqual (1, cm.Count, "A1");
			Assert.AreEqual (1, related_source.Count, "A2");
			Assert.AreEqual ("Columns", related_source.DataMember, "A3");
			Assert.AreSame (source, related_source.DataSource, "A4");
			Assert.IsTrue (related_source.List is ListView.ColumnHeaderCollection, "A5");
			Assert.AreSame (cm, source.GetRelatedCurrencyManager ("Columns"), "A6");

			// A path string returns null
			cm = source.GetRelatedCurrencyManager ("Columns.Count");
			Assert.IsNull (cm, "B1");

			// String.Empty and null
			Assert.AreSame (source.CurrencyManager, source.GetRelatedCurrencyManager (String.Empty), "C1");
			Assert.AreSame (source.CurrencyManager, source.GetRelatedCurrencyManager (null), "C2");
		}

		[Test]
		public void GetRelatedCurrencyManagerObject ()
		{
			BindingSource source = new BindingSource ();
			ListViewItem item = new ListViewItem ();

			source.DataSource = item;
			CurrencyManager font_cm = source.GetRelatedCurrencyManager ("Font");
			CurrencyManager name_cm = source.GetRelatedCurrencyManager ("Font.Name");
			Assert.IsNull (name_cm, "A1");
		}

		class BindingListViewPoker : BindingList<string>, IBindingListView
		{
			public bool supports_filter;
			public bool supports_advanced_sorting;

			public string Filter {
				get { return ""; }
				set { }
			}

			public ListSortDescriptionCollection SortDescriptions {
				get { return null; }
			}

			public bool SupportsAdvancedSorting {
				get { return supports_advanced_sorting; }
			}

			public bool SupportsFiltering {
				get { return supports_filter; }
			}

			public void ApplySort (ListSortDescriptionCollection sorts)
			{
			}

			public void RemoveFilter ()
			{
			}
		}

		[Test]
		public void SupportsFilter ()
		{
			BindingListViewPoker c = new BindingListViewPoker ();
			BindingSource source = new BindingSource ();

			// because the default list is a BindingList<object>
			Assert.IsFalse (source.SupportsFiltering, "1");

			source.DataSource = c;

			// the DataSource is IBindingListView, but
			// SupportsFilter is false.
			Assert.IsFalse (source.SupportsFiltering, "2");

			c.supports_filter = true;

			Assert.IsTrue (source.SupportsFiltering, "3");
		}

		[Test]
		public void SupportsAdvancedSorting ()
		{
			BindingListViewPoker c = new BindingListViewPoker ();
			BindingSource source = new BindingSource ();

			// because the default list is a BindingList<object>
			Assert.IsFalse (source.SupportsAdvancedSorting, "1");

			source.DataSource = c;

			// the DataSource is IBindingListView, but
			// SupportsAdvancedSorting is false.
			Assert.IsFalse (source.SupportsAdvancedSorting, "2");

			c.supports_advanced_sorting = true;

			Assert.IsTrue (source.SupportsAdvancedSorting, "3");
		}

		class IBindingListPoker : BindingList<string>, IBindingList {
			public void AddIndex (PropertyDescriptor property)
			{
			}

			public void ApplySort (PropertyDescriptor property, ListSortDirection direction)
			{
			}

			public int Find (PropertyDescriptor property, object key)
			{
				throw new NotImplementedException ();
			}

			public void RemoveIndex (PropertyDescriptor property)
			{
			}

			public void RemoveSort ()
			{
			}

			public bool IsSorted {
				get { throw new NotImplementedException (); }
			}

			public ListSortDirection SortDirection {
				get { throw new NotImplementedException (); }
			}

			public PropertyDescriptor SortProperty {
				get { throw new NotImplementedException (); }
			}

			public bool SupportsChangeNotification {
				get { return supports_change_notification; }
			}

			public bool SupportsSearching {
				get { return supports_searching; }
			}

			public bool SupportsSorting {
				get { return supports_sorting; }
			}

			public bool supports_change_notification;
			public bool supports_searching;
			public bool supports_sorting;
		}

		[Test]
		public void SupportsSearching ()
		{
			IBindingListPoker c = new IBindingListPoker ();
			BindingSource source = new BindingSource ();

			// because the default list is a BindingList<object>
			Assert.IsFalse (source.SupportsSearching, "1");

			source.DataSource = c;

			// the DataSource is IBindingList, but
			// SupportsSearching is false.
			Assert.IsFalse (source.SupportsSearching, "2");

			c.supports_searching = true;

			Console.WriteLine ("set c.supports_searching to {0}, so c.SupportsSearching = {1}",
					   c.supports_searching, c.SupportsSearching);

			Assert.IsTrue (source.SupportsSearching, "3");
		}

		[Test]
		public void SupportsSorting ()
		{
			IBindingListPoker c = new IBindingListPoker ();
			BindingSource source = new BindingSource ();

			// because the default list is a BindingList<object>
			Assert.IsFalse (source.SupportsSorting, "1");

			source.DataSource = c;

			// the DataSource is IBindingList, but
			// SupportsSorting is false.
			Assert.IsFalse (source.SupportsSorting, "2");

			c.supports_sorting = true;

			Assert.IsTrue (source.SupportsSorting, "3");
		}

		[Test]
		public void SupportsChangeNotification ()
		{
			IBindingListPoker c = new IBindingListPoker ();
			BindingSource source = new BindingSource ();

			// because the default list is a BindingList<object>
			Assert.IsTrue (source.SupportsChangeNotification, "1");

			source.DataSource = c;

			// the DataSource is IBindingList, but
			// SupportsChangeNotification is false.
			Assert.IsTrue (source.SupportsChangeNotification, "2");

			c.supports_change_notification = true;

			Assert.IsTrue (source.SupportsChangeNotification, "3");
		}

		[Test]
		public void ISupportInitializeNotification ()
		{
			BindingSource source = new BindingSource ();
			List<string> list = new List<string> ();

			bool initialized_handled = false;
			ISupportInitializeNotification inotification = (ISupportInitializeNotification)source;
			inotification.Initialized += delegate { initialized_handled = true; };
			Assert.AreEqual (true, inotification.IsInitialized, "A1");
			Assert.AreEqual (false, initialized_handled, "A2");

			inotification.BeginInit ();
			Assert.AreEqual (false, inotification.IsInitialized, "B1");
			Assert.AreEqual (false, initialized_handled, "B2");

			source.DataSource = list;
			Assert.AreEqual (list, source.DataSource, "C1");
			Assert.AreEqual (false, initialized_handled, "C2");

			inotification.EndInit ();
			Assert.AreEqual (true, inotification.IsInitialized, "D1");
			Assert.AreEqual (true, initialized_handled, "D2");

			// Reset event info
			initialized_handled = false;
			inotification.EndInit ();

			Assert.AreEqual (true, initialized_handled, "E1");

			// 
			// Case 2: use a data source that implements ISupportsInitializeNotification
			//
			InitializableObject init_obj = new InitializableObject ();
			init_obj.BeginInit ();
			source.DataSource = null;

			inotification.BeginInit ();
			initialized_handled = false;
			source.DataSource = init_obj;

			Assert.AreEqual (false, inotification.IsInitialized, "G1");
			Assert.AreEqual (false, initialized_handled, "G2");
			Assert.AreEqual (false, init_obj.IsInitialized, "G3");
			Assert.AreEqual (init_obj, source.DataSource, "G4");
			Assert.IsTrue (source.List is BindingList<object>, "G5"); // Default list

			inotification.EndInit ();
			Assert.AreEqual (false, inotification.IsInitialized, "H1");
			Assert.AreEqual (false, initialized_handled, "H2");
			Assert.AreEqual (false, init_obj.IsInitialized, "H3");

			init_obj.EndInit ();
			Assert.AreEqual (true, inotification.IsInitialized, "J1");
			Assert.AreEqual (true, initialized_handled, "J2");
			Assert.AreEqual (true, init_obj.IsInitialized, "J3");

			Assert.IsTrue (source.List is BindingList<InitializableObject>, "K");

			// Call again EndInit on datasource, which should *not* cause a
			// EndInit call in BindingSource, since it is already initialized
			initialized_handled = false;
			init_obj.EndInit ();
			Assert.AreEqual (false, initialized_handled, "L");
		}

		class InitializableObject : ISupportInitializeNotification
		{
			bool is_initialized = true;

			public void BeginInit ()
			{
				is_initialized = false;
			}

			public void EndInit ()
			{
				is_initialized = true;

				if (Initialized != null)
					Initialized (this, EventArgs.Empty);
			}

			public bool IsInitialized {
				get {
					return is_initialized;
				}
			}

			public event EventHandler Initialized;
		}

		//
		// Events section
		//
		int iblist_raised;
		int ilist_raised;
		ListChangedEventArgs iblist_changed_args;
		ListChangedEventArgs ilist_changed_args;
		BindingSource iblist_source;
		BindingSource ilist_source;

		void ResetEventsInfo ()
		{
			iblist_raised = ilist_raised = 0;
			iblist_source = new BindingSource ();
			ilist_source = new BindingSource ();

			iblist_source.ListChanged += delegate (object o, ListChangedEventArgs e)
			{
				iblist_raised++;
				iblist_changed_args = e;
			};
			ilist_source.ListChanged += delegate (object o, ListChangedEventArgs e)
			{
				ilist_raised++;
				ilist_changed_args = e;
			};
		}

		[Test]
		public void ListChanged_DataSourceSet ()
		{
			IBindingList bindinglist = new BindingList<string> ();
			bindinglist.Add ("A");
			IList arraylist = new ArrayList (bindinglist);

			ResetEventsInfo ();

			iblist_source.DataSource = bindinglist;
			ilist_source.DataSource = arraylist;

			Assert.AreEqual (2, iblist_raised, "A1");
			Assert.AreEqual (2, ilist_raised, "A2");
			Assert.AreEqual (ListChangedType.Reset, iblist_changed_args.ListChangedType, "A3");
			Assert.AreEqual (ListChangedType.Reset, ilist_changed_args.ListChangedType, "A4");
			Assert.AreEqual (-1, iblist_changed_args.NewIndex, "A5");
			Assert.AreEqual (-1, ilist_changed_args.NewIndex, "A6");
		}

		[Test]
		public void ListChanged_ItemAdded ()
		{
			IBindingList bindinglist = new BindingList<string> ();
			bindinglist.Add ("A");
			IList arraylist = new ArrayList (bindinglist);

			ResetEventsInfo ();

			iblist_source.DataSource = bindinglist;
			ilist_source.DataSource = arraylist;

			// Clear after setting DataSource generated some info
			iblist_raised = ilist_raised = 0;
			iblist_changed_args = ilist_changed_args = null;

			iblist_source.Add ("B");
			ilist_source.Add ("B");
			Assert.AreEqual (1, iblist_raised, "A1");
			Assert.AreEqual (1, ilist_raised, "A2");
			Assert.AreEqual (ListChangedType.ItemAdded, iblist_changed_args.ListChangedType, "A3");
			Assert.AreEqual (ListChangedType.ItemAdded, ilist_changed_args.ListChangedType, "A4");
			Assert.AreEqual (1, iblist_changed_args.NewIndex, "A5");
			Assert.AreEqual (1, ilist_changed_args.NewIndex, "A6");

			iblist_raised = ilist_raised = 0;
			iblist_changed_args = ilist_changed_args = null;

			iblist_source.Insert (0, "C");
			ilist_source.Insert (0, "C");

			Assert.AreEqual (1, iblist_raised, "B1");
			Assert.AreEqual (1, ilist_raised, "B2");
			Assert.AreEqual (ListChangedType.ItemAdded, iblist_changed_args.ListChangedType, "B3");
			Assert.AreEqual (ListChangedType.ItemAdded, ilist_changed_args.ListChangedType, "B4");
			Assert.AreEqual (0, iblist_changed_args.NewIndex, "B5");
			Assert.AreEqual (0, ilist_changed_args.NewIndex, "B6");

			// AddNew
			iblist_source.AddingNew += delegate (object o, AddingNewEventArgs e) { e.NewObject = "Z"; };
			ilist_source.AddingNew += delegate (object o, AddingNewEventArgs e) { e.NewObject = "Z"; };

			iblist_source.AllowNew = true;
			ilist_source.AllowNew = true;

			iblist_raised = ilist_raised = 0;
			iblist_changed_args = ilist_changed_args = null;

			iblist_source.AddNew ();
			ilist_source.AddNew ();

			Assert.AreEqual (1, iblist_raised, "C1");
			Assert.AreEqual (1, ilist_raised, "C2");
			Assert.AreEqual (ListChangedType.ItemAdded, iblist_changed_args.ListChangedType, "C3");
			Assert.AreEqual (ListChangedType.ItemAdded, ilist_changed_args.ListChangedType, "C4");
			Assert.AreEqual (3, iblist_changed_args.NewIndex, "C5");
			Assert.AreEqual (3, ilist_changed_args.NewIndex, "C6");

			iblist_raised = ilist_raised = 0;
			iblist_changed_args = ilist_changed_args = null;
			// This only applies to IBindingList - Direct access, not through BindingSource
			bindinglist.Add ("D");

			Assert.AreEqual (1, iblist_raised, "D1");
			Assert.AreEqual (ListChangedType.ItemAdded, iblist_changed_args.ListChangedType, "D2");
			Assert.AreEqual (4, iblist_changed_args.NewIndex, "D3");
		}

		[Test]
		public void ListChanged_ItemDeleted ()
		{
			IBindingList bindinglist = new BindingList<string> ();
			bindinglist.Add ("A");
			bindinglist.Add ("B");
			bindinglist.Add ("C");
			IList arraylist = new ArrayList (bindinglist);

			ResetEventsInfo ();

			iblist_source.DataSource = bindinglist;
			ilist_source.DataSource = arraylist;

			// Clear after setting DataSource generated some info
			iblist_raised = ilist_raised = 0;
			iblist_changed_args = ilist_changed_args = null;

			iblist_source.RemoveAt (2);
			ilist_source.RemoveAt (2);

			Assert.AreEqual (1, iblist_raised, "A1");
			Assert.AreEqual (1, ilist_raised, "A2");
			Assert.AreEqual (ListChangedType.ItemDeleted, iblist_changed_args.ListChangedType, "A3");
			Assert.AreEqual (ListChangedType.ItemDeleted, ilist_changed_args.ListChangedType, "A4");
			Assert.AreEqual (2, iblist_changed_args.NewIndex, "A5");
			Assert.AreEqual (2, ilist_changed_args.NewIndex, "A6");

			iblist_raised = ilist_raised = 0;
			iblist_changed_args = ilist_changed_args = null;

			iblist_source.Remove ("A");
			ilist_source.Remove ("A");

			Assert.AreEqual (1, iblist_raised, "B1");
			Assert.AreEqual (1, ilist_raised, "B2");
			Assert.AreEqual (ListChangedType.ItemDeleted, iblist_changed_args.ListChangedType, "B3");
			Assert.AreEqual (ListChangedType.ItemDeleted, ilist_changed_args.ListChangedType, "B4");
			Assert.AreEqual (0, iblist_changed_args.NewIndex, "B5");
			Assert.AreEqual (0, ilist_changed_args.NewIndex, "B6");

			iblist_raised = ilist_raised = 0;
			iblist_changed_args = ilist_changed_args = null;

			// This only applies to IBindingList - Direct access, not through BindingSource
			bindinglist.Remove ("B");

			Assert.AreEqual (1, iblist_raised, "C1");
			Assert.AreEqual (ListChangedType.ItemDeleted, iblist_changed_args.ListChangedType, "C2");
			Assert.AreEqual (0, iblist_changed_args.NewIndex, "C3");
		}

		[Test]
		public void ListChanged_Reset ()
		{
			IBindingList bindinglist = new BindingList<string> ();
			bindinglist.Add ("A");
			bindinglist.Add ("B");
			bindinglist.Add ("C");
			IList arraylist = new ArrayList (bindinglist);

			ResetEventsInfo ();

			iblist_source.DataSource = bindinglist;
			ilist_source.DataSource = arraylist;

			// Clear after setting DataSource generated some info
			iblist_raised = ilist_raised = 0;
			iblist_changed_args = ilist_changed_args = null;

			iblist_source.Clear ();
			ilist_source.Clear ();

			Assert.AreEqual (1, iblist_raised, "A1");
			Assert.AreEqual (1, ilist_raised, "A2");
			Assert.AreEqual (ListChangedType.Reset, iblist_changed_args.ListChangedType, "A3");
			Assert.AreEqual (ListChangedType.Reset, ilist_changed_args.ListChangedType, "A4");
			Assert.AreEqual (-1, iblist_changed_args.NewIndex, "A5");
			Assert.AreEqual (-1, ilist_changed_args.NewIndex, "A6");

			// This is only for BindingList - Direct access to Clear
			// First add some items
			bindinglist.Add ("D");
			bindinglist.Add ("E");

			iblist_raised = ilist_raised = 0;
			iblist_changed_args = ilist_changed_args = null;

			bindinglist.Clear ();
			Assert.AreEqual (1, iblist_raised, "B1");
			Assert.AreEqual (ListChangedType.Reset, iblist_changed_args.ListChangedType, "B2");
			Assert.AreEqual (-1, iblist_changed_args.NewIndex, "B3");
		}

		[Test] // bug 664833
		public void TestDataMemberValue ()
		{
			BindingSource bs = new BindingSource ();
			bs.DataMember = "TestField";
			DataTable table = new DataTable ();
			table.Columns.Add ("TestField");
			bs.DataSource = table;

			Assert.AreEqual ("TestField", bs.DataMember, "#1");
		}
	}
}

#endif
