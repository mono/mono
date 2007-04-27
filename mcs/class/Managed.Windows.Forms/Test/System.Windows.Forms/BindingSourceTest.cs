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
	public class BindingSourceTest
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
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

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

			Assert.AreEqual (-1, source.Position, "0");

			source.DataSource = new WorkingEnumerable (5);

			int raised = 0;
			source.PositionChanged += delegate (object sender, EventArgs e) { raised ++; };


			Assert.AreEqual (0, source.Position, "1");

			source.Position = -1;
			Assert.AreEqual (0, source.Position, "2");
			Assert.AreEqual (0, raised, "3");

			source.Position = 10;
			Assert.AreEqual (4, source.Position, "4");
			Assert.AreEqual (1, raised, "5");

			source.Position = 10;
			Assert.AreEqual (4, source.Position, "6");
			Assert.AreEqual (1, raised, "7");
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
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

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
		public void SuppliedDataSource ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

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
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

			BindingSource source = new BindingSource ();

			source.DataSource = new List<string>();
			source.DataMember = "Length";
			Assert.IsNotNull (source.CurrencyManager, "1");

			source.DataSource = new List<string>();
			Assert.AreEqual ("Length", source.DataMember, "2");
			Assert.IsNotNull (source.CurrencyManager, "3");

			source.DataSource = new List<string>();
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
		public void AddNew ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

			BindingSource source = new BindingSource ();
			source.AddNew ();
			Assert.AreEqual (1, source.Count, "1");
		}

		[Test]
		public void AddNew_NonBindingList ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

			IList list = new List<object> ();
			BindingSource source = new BindingSource ();
			source.DataSource = list;
			Assert.IsTrue (source.List is List<object>, "1");
			source.AddNew ();
			Assert.AreEqual (1, source.Count, "2");
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
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

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
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

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
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

			BindingSource source = new BindingSource ();
			source.DataSource = new List<string>();
			object o = source.AddNew ();
			
			TestHelper.RemoveWarning (o);
		}

		[Test]
		public void BindingSuspended1 ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

			/* how does this property work? */
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
		public void InitializedEvent ()
		{
			// XXX this test is officially useless.  does
			// BindingSource even raise the event?  it
			// seems to always be initialized.
			BindingSource source = new BindingSource ();
			ISupportInitializeNotification n = (ISupportInitializeNotification)source;

			bool event_handled = false;
			n.Initialized += delegate (object sender, EventArgs e) {
				event_handled = true;
			};

			Assert.IsTrue (n.IsInitialized, "1");

			source.DataSource = "hi";

			Assert.IsTrue (n.IsInitialized, "2");
			Assert.IsFalse (event_handled, "3");
		}
	}
}

#endif
