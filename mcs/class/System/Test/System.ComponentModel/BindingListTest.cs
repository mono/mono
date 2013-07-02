#if NET_2_0

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class BindingListTest
	{
		[Test]
		public void BindingListDefaults ()
		{
			BindingList<string> l = new BindingList<string> ();
			IBindingList ibl = (IBindingList)l;
				
			Assert.IsTrue (l.AllowEdit, "1");
			Assert.IsFalse (l.AllowNew, "2");
			Assert.IsTrue (l.AllowRemove, "3");
			Assert.IsTrue (l.RaiseListChangedEvents, "4");

			Assert.IsFalse (ibl.IsSorted, "5");
			Assert.AreEqual (ibl.SortDirection, ListSortDirection.Ascending, "6");
			Assert.IsTrue (ibl.SupportsChangeNotification, "7");
			Assert.IsFalse (ibl.SupportsSearching, "8");
			Assert.IsFalse (ibl.SupportsSorting, "9");
			Assert.IsFalse (((IRaiseItemChangedEvents)l).RaisesItemChangedEvents, "10");
		}

		[Test]
		public void BindingListDefaults_FixedSizeList ()
		{
			string[] arr = new string[10];
			BindingList<string> l = new BindingList<string> (arr);
			IBindingList ibl = (IBindingList)l;

			Assert.IsTrue (l.AllowEdit, "1");
			Assert.IsFalse (l.AllowNew, "2");
			Assert.IsTrue (l.AllowRemove, "3");
			Assert.IsTrue (l.RaiseListChangedEvents, "4");

			Assert.IsFalse (ibl.IsSorted, "5");
			Assert.AreEqual (ibl.SortDirection, ListSortDirection.Ascending, "6");
			Assert.IsTrue (ibl.SupportsChangeNotification, "7");
			Assert.IsFalse (ibl.SupportsSearching, "8");
			Assert.IsFalse (ibl.SupportsSorting, "9");
			Assert.IsFalse (((IRaiseItemChangedEvents)l).RaisesItemChangedEvents, "10");
		}

		[Test]
		public void BindingListDefaults_NonFixedSizeList ()
		{
			List<string> list = new List<string> ();
			BindingList<string> l = new BindingList<string> (list);
			IBindingList ibl = (IBindingList)l;

			Assert.IsTrue (l.AllowEdit, "1");
			Assert.IsFalse (l.AllowNew, "2");
			Assert.IsTrue (l.AllowRemove, "3");
			Assert.IsTrue (l.RaiseListChangedEvents, "4");

			Assert.IsFalse (ibl.IsSorted, "5");
			Assert.AreEqual (ibl.SortDirection, ListSortDirection.Ascending, "6");
			Assert.IsTrue (ibl.SupportsChangeNotification, "7");
			Assert.IsFalse (ibl.SupportsSearching, "8");
			Assert.IsFalse (ibl.SupportsSorting, "9");
			Assert.IsFalse (((IRaiseItemChangedEvents)l).RaisesItemChangedEvents, "10");
		}

		[Test]
		public void BindingListDefaults_ReadOnlyList ()
		{
			List<string> list = new List<string> ();
			BindingList<string> l = new BindingList<string> (list);
			IBindingList ibl = (IBindingList)l;

			Assert.IsTrue (l.AllowEdit, "1");
			Assert.IsFalse (l.AllowNew, "2");
			Assert.IsTrue (l.AllowRemove, "3");
			Assert.IsTrue (l.RaiseListChangedEvents, "4");

			Assert.IsFalse (ibl.IsSorted, "5");
			Assert.AreEqual (ibl.SortDirection, ListSortDirection.Ascending, "6");
			Assert.IsTrue (ibl.SupportsChangeNotification, "7");
			Assert.IsFalse (ibl.SupportsSearching, "8");
			Assert.IsFalse (ibl.SupportsSorting, "9");
			Assert.IsFalse (((IRaiseItemChangedEvents)l).RaisesItemChangedEvents, "10");
		}

		[Test]
		public void TestAllowNew ()
		{
			/* Object has a default ctor */
			BindingList<object> l1 = new BindingList<object> ();
			Assert.IsTrue (l1.AllowNew, "1");

			/* string has no default ctor */
			BindingList<string> l2 = new BindingList<string> ();
			Assert.IsFalse (l2.AllowNew, "2");

			/* adding a delegate to AddingNew fixes that */
			l2.AddingNew += delegate (object sender, AddingNewEventArgs e) { };
			Assert.IsTrue (l2.AllowNew, "3");

			l1 = new BindingList<object> ();

			bool list_changed = false;
			bool expected = false;

			l1.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				list_changed = true;
				Assert.AreEqual (-1, e.NewIndex, "4");
				Assert.AreEqual (ListChangedType.Reset, e.ListChangedType, "5");
				Assert.AreEqual (expected, l1.AllowNew, "6");
			};

			expected = false;
			l1.AllowNew = false;

			Assert.IsTrue (list_changed, "7");

			/* the default for T=object is true, so check
			   if we enter the block for raising the event
			   if we explicitly set it to the value it
			   currently has. */
			l1 = new BindingList<object> ();

			list_changed = false;
			
			l1.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				list_changed = true;
				Assert.AreEqual (-1, e.NewIndex, "8");
				Assert.AreEqual (ListChangedType.Reset, e.ListChangedType, "9");
				Assert.AreEqual (expected, l1.AllowNew, "10");
			};

			expected = true;
			l1.AllowNew = true;

			/* turns out it doesn't raise the event, so the check must only be for "allow_new == value" */
			Assert.IsFalse (list_changed, "11");
		}

		[Test]
		public void TestResetBindings ()
		{
			BindingList<object> l = new BindingList<object> ();

			bool list_changed = false;

			l.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				list_changed = true;
				Assert.AreEqual (-1, e.NewIndex, "1");
				Assert.AreEqual (ListChangedType.Reset, e.ListChangedType, "2");
			};

			l.ResetBindings ();

			Assert.IsTrue (list_changed, "3");
		}

		[Test]
		public void TestResetItem ()
		{
			List<object> list = new List<object>();
			list.Add (new object());

			BindingList<object> l = new BindingList<object> (list);

			bool item_changed = false;

			l.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				item_changed = true;
				Assert.AreEqual (0, e.NewIndex, "1");
				Assert.AreEqual (ListChangedType.ItemChanged, e.ListChangedType, "2");
			};

			l.ResetItem (0);

			Assert.IsTrue (item_changed, "3");
		}

		[Test]
		public void TestRemoveItem ()
		{
			List<object> list = new List<object>();
			list.Add (new object());

			BindingList<object> l = new BindingList<object> (list);

			bool item_deleted = false;

			l.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				item_deleted = true;
				Assert.AreEqual (0, e.NewIndex, "1");
				Assert.AreEqual (ListChangedType.ItemDeleted, e.ListChangedType, "2");
				Assert.AreEqual (0, l.Count, "3"); // to show the event is raised after the removal
			};

			l.RemoveAt (0);

			Assert.IsTrue (item_deleted, "4");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestRemoveItem_AllowRemoveFalse ()
		{
			List<object> list = new List<object>();
			list.Add (new object());

			BindingList<object> l = new BindingList<object> (list);

			l.AllowRemove = false;

			l.RemoveAt (0);
		}

		[Test]
		public void TestAllowEditEvent ()
		{
			BindingList<object> l = new BindingList<object>();

			bool event_raised = false;
			bool expected = false;

			l.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				event_raised = true;
				Assert.AreEqual (-1, e.NewIndex, "1");
				Assert.AreEqual (ListChangedType.Reset, e.ListChangedType, "2");
				Assert.AreEqual (expected, l.AllowEdit, "3");
			};

			expected = false;
			l.AllowEdit = false;

			Assert.IsTrue (event_raised, "4");

			// check to see if RaiseListChangedEvents affects AllowEdit's event.
			l.RaiseListChangedEvents = false;

			event_raised = false;
			expected = true;
			l.AllowEdit = true;

			Assert.IsFalse (event_raised, "5");
		}

		[Test]
		public void TestAllowRemove ()
		{
			BindingList<object> l = new BindingList<object>();

			bool event_raised = false;
			bool expected = false;

			l.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				event_raised = true;
				Assert.AreEqual (-1, e.NewIndex, "1");
				Assert.AreEqual (ListChangedType.Reset, e.ListChangedType, "2");
				Assert.AreEqual (expected, l.AllowRemove, "3");
			};

			expected = false;
			l.AllowRemove = false;

			Assert.IsTrue (event_raised, "4");

			// check to see if RaiseListChangedEvents affects AllowRemove's event.
			l.RaiseListChangedEvents = false;

			event_raised = false;
			expected = true;
			l.AllowRemove = true;

			Assert.IsFalse (event_raised, "5");
		}

		[Test]
		public void TestAddNew_SettingArgsNewObject ()
		{
			BindingList<object> l = new BindingList<object>();

			bool adding_event_raised = false;
			object o = new object ();

			l.AddingNew += delegate (object sender, AddingNewEventArgs e) {
				adding_event_raised = true;
				Assert.IsNull (e.NewObject, "1");
				e.NewObject = o;
			};

			object rv = l.AddNew ();
			Assert.IsTrue (adding_event_raised, "2");
			Assert.AreSame (o, rv, "3");
		}

		[Test]
		public void TestAddNew ()
		{
			BindingList<object> l = new BindingList<object>();

			bool adding_event_raised = false;
			object o = new object ();

			l.AddingNew += delegate (object sender, AddingNewEventArgs e) {
				adding_event_raised = true;
				Assert.IsNull (e.NewObject, "1");
			};

			object rv = l.AddNew ();
			Assert.IsTrue (adding_event_raised, "2");
			Assert.IsNotNull (rv, "3");
		}

		[Test]
		public void TestAddNew_Cancel ()
		{
			BindingList<object> l = new BindingList<object>();

			bool adding_event_raised = false;
			object o = new object ();

			bool list_changed = false;
			ListChangedType change_type = ListChangedType.Reset;
			int list_changed_index = -1;

			l.AddingNew += delegate (object sender, AddingNewEventArgs e) {
				adding_event_raised = true;
				Assert.IsNull (e.NewObject, "1");
			};

			l.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				list_changed = true;
				change_type = e.ListChangedType;
				list_changed_index = e.NewIndex;
			};

			object rv = l.AddNew ();
			Assert.IsTrue (adding_event_raised, "2");
			Assert.IsNotNull (rv, "3");

			Assert.AreEqual (1, l.Count, "4");
			Assert.AreEqual (0, l.IndexOf (rv), "5");
			Assert.IsTrue (list_changed, "6");
			Assert.AreEqual (ListChangedType.ItemAdded, change_type, "7");
			Assert.AreEqual (0, list_changed_index, "8");

			list_changed = false;

			l.CancelNew (0);

			Assert.AreEqual (0, l.Count, "9");
			Assert.IsTrue (list_changed, "10");
			Assert.AreEqual (ListChangedType.ItemDeleted, change_type, "11");
			Assert.AreEqual (0, list_changed_index, "12");
		}

		[Test]
		public void TestAddNew_CancelDifferentIndex ()
		{
			List<object> list = new List<object>();

			list.Add (new object ());
			list.Add (new object ());

			BindingList<object> l = new BindingList<object>(list);

			bool adding_event_raised = false;
			object o = new object ();

			bool list_changed = false;
			ListChangedType change_type = ListChangedType.Reset;
			int list_changed_index = -1;

			l.AddingNew += delegate (object sender, AddingNewEventArgs e) {
				adding_event_raised = true;
				Assert.IsNull (e.NewObject, "1");
			};

			l.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				list_changed = true;
				change_type = e.ListChangedType;
				list_changed_index = e.NewIndex;
			};

			object rv = l.AddNew ();
			Assert.IsTrue (adding_event_raised, "2");
			Assert.IsNotNull (rv, "3");

			Assert.AreEqual (3, l.Count, "4");
			Assert.AreEqual (2, l.IndexOf (rv), "5");
			Assert.IsTrue (list_changed, "6");
			Assert.AreEqual (ListChangedType.ItemAdded, change_type, "7");
			Assert.AreEqual (2, list_changed_index, "8");

			list_changed = false;

			l.CancelNew (0);

			Assert.IsFalse (list_changed, "9");
			Assert.AreEqual (3, l.Count, "10");

			l.CancelNew (2);

			Assert.IsTrue (list_changed, "11");
			Assert.AreEqual (ListChangedType.ItemDeleted, change_type, "12");
			Assert.AreEqual (2, list_changed_index, "13");
			Assert.AreEqual (2, l.Count, "14");
		}

		[Test]
		public void TestAddNew_End ()
		{
			BindingList<object> l = new BindingList<object>();

			bool adding_event_raised = false;
			object o = new object ();

			bool list_changed = false;
			ListChangedType change_type = ListChangedType.Reset;
			int list_changed_index = -1;

			l.AddingNew += delegate (object sender, AddingNewEventArgs e) {
				adding_event_raised = true;
				Assert.IsNull (e.NewObject, "1");
			};

			l.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				list_changed = true;
				change_type = e.ListChangedType;
				list_changed_index = e.NewIndex;
			};

			object rv = l.AddNew ();
			Assert.IsTrue (adding_event_raised, "2");
			Assert.IsNotNull (rv, "3");

			Assert.AreEqual (1, l.Count, "4");
			Assert.AreEqual (0, l.IndexOf (rv), "5");
			Assert.IsTrue (list_changed, "6");
			Assert.AreEqual (ListChangedType.ItemAdded, change_type, "7");
			Assert.AreEqual (0, list_changed_index, "8");

			list_changed = false;

			l.EndNew (0);

			Assert.AreEqual (1, l.Count, "9");
			Assert.IsFalse (list_changed, "10");
		}

		[Test]
		public void TestAddNew_CancelDifferentIndexThenEnd ()
		{
			BindingList<object> l = new BindingList<object>();

			bool adding_event_raised = false;
			object o = new object ();

			bool list_changed = false;
			ListChangedType change_type = ListChangedType.Reset;
			int list_changed_index = -1;

			l.AddingNew += delegate (object sender, AddingNewEventArgs e) {
				adding_event_raised = true;
				Assert.IsNull (e.NewObject, "1");
			};

			l.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				list_changed = true;
				change_type = e.ListChangedType;
				list_changed_index = e.NewIndex;
			};

			object rv = l.AddNew ();
			Assert.IsTrue (adding_event_raised, "2");
			Assert.IsNotNull (rv, "3");

			Assert.AreEqual (1, l.Count, "4");
			Assert.AreEqual (0, l.IndexOf (rv), "5");
			Assert.IsTrue (list_changed, "6");
			Assert.AreEqual (ListChangedType.ItemAdded, change_type, "7");
			Assert.AreEqual (0, list_changed_index, "8");

			list_changed = false;

			l.CancelNew (2);

			Assert.AreEqual (1, l.Count, "9");
			Assert.IsFalse (list_changed, "10");

			l.EndNew (0);

			Assert.AreEqual (1, l.Count, "11");
			Assert.IsFalse (list_changed, "12");
		}

		[Test]
		public void TestAddNew_EndDifferentIndexThenCancel ()
		{
			BindingList<object> l = new BindingList<object>();

			bool adding_event_raised = false;
			object o = new object ();

			bool list_changed = false;
			ListChangedType change_type = ListChangedType.Reset;
			int list_changed_index = -1;

			l.AddingNew += delegate (object sender, AddingNewEventArgs e) {
				adding_event_raised = true;
				Assert.IsNull (e.NewObject, "1");
			};

			l.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				list_changed = true;
				change_type = e.ListChangedType;
				list_changed_index = e.NewIndex;
			};

			object rv = l.AddNew ();
			Assert.IsTrue (adding_event_raised, "2");
			Assert.IsNotNull (rv, "3");

			Assert.AreEqual (1, l.Count, "4");
			Assert.AreEqual (0, l.IndexOf (rv), "5");
			Assert.IsTrue (list_changed, "6");
			Assert.AreEqual (ListChangedType.ItemAdded, change_type, "7");
			Assert.AreEqual (0, list_changed_index, "8");

			list_changed = false;

			l.EndNew (2);

			Assert.AreEqual (1, l.Count, "9");
			Assert.IsFalse (list_changed, "10");

			l.CancelNew (0);

			Assert.IsTrue (list_changed, "11");
			Assert.AreEqual (ListChangedType.ItemDeleted, change_type, "12");
			Assert.AreEqual (0, list_changed_index, "13");
		}

		class BindingListPoker : BindingList<object>
		{
			public object DoAddNewCore()
			{
				return base.AddNewCore ();
			}
		}

		// test to make sure that the events are raised in AddNewCore and not in AddNew
		[Test]
		public void TestAddNewCore_Insert ()
		{
			BindingListPoker poker = new BindingListPoker ();

			bool adding_event_raised = false;

			bool list_changed = false;
			ListChangedType change_type = ListChangedType.Reset;
			int list_changed_index = -1;

			poker.AddingNew += delegate (object sender, AddingNewEventArgs e) {
				adding_event_raised = true;
			};

			poker.ListChanged += delegate (object sender, ListChangedEventArgs e) {
				list_changed = true;
				change_type = e.ListChangedType;
				list_changed_index = e.NewIndex;
			};

			object o = poker.DoAddNewCore ();

			Assert.IsTrue (adding_event_raised, "1");
			Assert.IsTrue (list_changed, "2");
			Assert.AreEqual (ListChangedType.ItemAdded, change_type, "3");
			Assert.AreEqual (0, list_changed_index, "4");
			Assert.AreEqual (1, poker.Count, "5");
		}

		private class Item : INotifyPropertyChanged {

			public event PropertyChangedEventHandler PropertyChanged;

			string _name;

			public string Name {
				get { return _name; }
				set {
					if (_name != value) {
						_name = value;
						OnPropertyChanged ("Name");
					}
				}
			}

			void OnPropertyChanged (string name)
			{
				var fn = PropertyChanged;
				if (fn != null)
					fn (this, new PropertyChangedEventArgs (name));
			}
		}

		[Test] // https://bugzilla.xamarin.com/show_bug.cgi?id=8366
		public void Bug8366 ()
		{
			bool added = false;
			bool changed = false;
			var list = new BindingList<Item> ();
			list.ListChanged += (object sender, ListChangedEventArgs e) => {
				added |= e.ListChangedType == ListChangedType.ItemAdded;
				changed |= e.ListChangedType == ListChangedType.ItemChanged;
			};

			var item = new Item () { Name = "1" };
			list.Add (item);

			item.Name = "2";

			Assert.IsTrue (added, "ItemAdded");
			Assert.IsTrue (changed, "ItemChanged");
		}
	}
}

#endif
