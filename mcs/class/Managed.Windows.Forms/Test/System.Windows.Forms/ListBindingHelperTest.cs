//
// ListBindingHelperTest.cs: Test cases for ListBindingHelper class.
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

// Author:
// 	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
//

#if NET_2_0

using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListBindingHelperTest : TestHelper
	{
		[Test]
		public void GetListTest ()
		{
			ListSource lsource = new ListSource (true);
			Stack stack = new Stack ();
			stack.Push (3);

			Assert.IsTrue (ListBindingHelper.GetList (lsource) is SimpleItem [], "#A1");
			Assert.AreEqual ("NonList", ListBindingHelper.GetList ("NonList"), "#A2");
			Assert.AreEqual (null, ListBindingHelper.GetList (null), "#A3");
			Assert.AreEqual (stack, ListBindingHelper.GetList (stack), "#A4"); // IEnumerable

			Assert.IsTrue (ListBindingHelper.GetList (lsource, String.Empty) is SimpleItem [], "#B1");
			Assert.AreEqual ("NonList", ListBindingHelper.GetList ("NonList", String.Empty), "#B2");
			Assert.AreEqual (null, ListBindingHelper.GetList (null, "DontExist"), "#B3");
			Assert.IsTrue (ListBindingHelper.GetList (lsource, null) is SimpleItem [], "#B4");

			ListContainer list_container = new ListContainer ();
			Assert.AreEqual (new object [0], ListBindingHelper.GetList (list_container, "List"), "#C1");

			// Even if IListSource.ContainsListCollection is false, we return the result of GetList ()
			lsource = new ListSource (false);
			Assert.IsTrue (ListBindingHelper.GetList (lsource) is SimpleItem [], "#D1");

			// DataMember is not if IList type
			Assert.AreEqual (new SimpleItem (), ListBindingHelper.GetList (list_container, "NonList"), "#E1");

			// List (IEnumerable)
			stack.Clear ();
			stack.Push (new SimpleItem (3));
			stack.Push (new SimpleItem (7));
			object obj = ListBindingHelper.GetList (stack, "Value");
			Assert.IsTrue (obj != null, "#F1");
			Assert.IsTrue (obj is int, "#F2");
			Assert.AreEqual (7, (int) obj, "#F3");

			// ListSource returning an IEnumerable,
			// which in turn retrieves dataMember
			obj = ListBindingHelper.GetList (lsource, "Value");
			Assert.IsTrue (obj != null, "#G1");
			Assert.IsTrue (obj is int, "#G2");
			Assert.AreEqual (0, (int)obj, "#G3");

			// Empty IEnumerable - valid property for list item type
			// Since it's empty, it needs to check whether the datamember is
			// a valid value, and thus we need the datasource to also be IList
			// Then we need a parameterized IEnumerable, which returns null.
			// *Observation: if it is empty and it doesn't implement IList,
			// it doesn't have a way to get the properties, and will throw an exc
			StringCollection str_coll = new StringCollection ();
			obj = ListBindingHelper.GetList (str_coll, "Length");
			Assert.IsNull (obj, "#H1");

			// IEnumerable that returns instances of ICustomTypeDescriptor
			// Use DataTable as source, which returns, when enumerating,
			// instances of DataRowView, which in turn implement ICustomTypeDescriptor
			DataTable table = new DataTable ();
			table.Columns.Add ("Id", typeof (int));
			table.Rows.Add (666);
			object l = ListBindingHelper.GetList (table, "Id");
			Assert.AreEqual (666, l, "#J1");

			try {
				ListBindingHelper.GetList (list_container, "DontExist");
				Assert.Fail ("#EXC1");
			} catch (ArgumentException) {
			}

			// Empty IEnumerable not implementing IList
			// Don't have a way to know whether at least datamember is valid or not.
			try {
				stack.Clear ();
				obj = ListBindingHelper.GetList (stack, "Value");
				Assert.Fail ("#EXC3");
			} catch (ArgumentException) {
			}

		}

		internal class ListSource : IListSource
		{
			bool contains_collection;

			public ListSource (bool containsCollection)
			{
				contains_collection = containsCollection;
			}

			public bool ContainsListCollection {
				get {
					return contains_collection;
				}
			}

			public IList GetList ()
			{
				return new SimpleItem [] { new SimpleItem () };
			}
		}

		class SuperContainer
		{
			public ListContainer ListContainer
			{
				get
				{
					return new ListContainer ();
				}
			}
		}

		class ListContainer
		{
			public IList List {
				get {
					return new SimpleItem [0];
				}
			}

			public SimpleItem NonList {
				get {
					return new SimpleItem ();
				}
			}
		}

		class SimpleItem
		{
			int value;

			public SimpleItem ()
			{
			}

			public SimpleItem (int value)
			{
				this.value = value;
			}

			public int Value
			{
				get
				{
					return value;
				}
				set
				{
					this.value = value;
				}
			}

			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}

			public override bool Equals (object obj)
			{
				return value == ((SimpleItem)obj).value;
			}
		}

		[Test]
		public void GetListItemPropertiesTest ()
		{
			SimpleItem [] items = new SimpleItem [0];
			PropertyDescriptorCollection properties = ListBindingHelper.GetListItemProperties (items);

			Assert.AreEqual (1, properties.Count, "#A1");
			Assert.AreEqual ("Value", properties [0].Name, "#A2");

			List<SimpleItem> items_list = new List<SimpleItem> ();
			properties = ListBindingHelper.GetListItemProperties (items_list);

			Assert.AreEqual (1, properties.Count, "#B1");
			Assert.AreEqual ("Value", properties [0].Name, "#B2");

			// Empty arraylist
			ArrayList items_arraylist = new ArrayList ();
			properties = ListBindingHelper.GetListItemProperties (items_arraylist);

			Assert.AreEqual (0, properties.Count, "#C1");

			// Non empty arraylist
			items_arraylist.Add (new SimpleItem ());
			properties = ListBindingHelper.GetListItemProperties (items_arraylist);

			Assert.AreEqual (1, properties.Count, "#D1");
			Assert.AreEqual ("Value", properties [0].Name, "#D2");

			// non list object
			properties = ListBindingHelper.GetListItemProperties (new SimpleItem ());

			Assert.AreEqual (1, properties.Count, "#E1");
			Assert.AreEqual ("Value", properties [0].Name, "#E2");

			// null value
			properties = ListBindingHelper.GetListItemProperties (null);

			Assert.AreEqual (0, properties.Count, "#F1");

			// ListSource
			properties = ListBindingHelper.GetListItemProperties (new ListSource (true));

			Assert.AreEqual (1, properties.Count, "#G1");
			Assert.AreEqual ("Value", properties [0].Name, "#G2");

			// ITypedList
			DataTable table = new DataTable ();
			table.Columns.Add ("Name", typeof (string));

			properties = ListBindingHelper.GetListItemProperties (table);
			Assert.AreEqual (1, properties.Count, "#H1");
			Assert.AreEqual ("Name", properties [0].Name, "#H2");
		}

		// tests for the overloads of the method
		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void GetListItemPropertiesTest2 ()
		{
			ListContainer list_container = new ListContainer ();
			PropertyDescriptorCollection list_properties = TypeDescriptor.GetProperties (list_container);
			PropertyDescriptor property = list_properties ["List"];

			PropertyDescriptorCollection property_coll = ListBindingHelper.GetListItemProperties (list_container,
				new PropertyDescriptor [] { property });
			Assert.AreEqual (1, property_coll.Count, "#A1");
			Assert.AreEqual ("Value", property_coll [0].Name, "#A2");

			// Empty property descriptor array 
			// Returns list_container properties, since it's not a list
			property_coll = ListBindingHelper.GetListItemProperties (list_container, new PropertyDescriptor [0]);
			Assert.AreEqual (2, property_coll.Count, "#B1");

			// Non list property
			// Returns the propeties of the type of that property
			property = list_properties ["NonList"];
			property_coll = ListBindingHelper.GetListItemProperties (list_container,
				new PropertyDescriptor [] { property });
			Assert.AreEqual (1, property_coll.Count, "#C1");
			Assert.AreEqual ("Value", property_coll [0].Name, "#C2");

			// Pass two properties
			property = list_properties ["List"];
			PropertyDescriptor property2 = list_properties ["NonList"];
			property_coll = ListBindingHelper.GetListItemProperties (list_container,
				new PropertyDescriptor [] { property2, property });
			Assert.AreEqual (0, property_coll.Count, "#D1");

			//
			// Third overload - doble re-direction
			//
			SuperContainer super_container = new SuperContainer ();
			property = list_properties ["List"];

			property_coll = ListBindingHelper.GetListItemProperties (super_container, "ListContainer",
				new PropertyDescriptor [] { property });
			Assert.AreEqual (1, property_coll.Count, "#E1");
		}

		[Test]
		public void GetListItemTypeTest ()
		{
			List<int> list = new List<int> ();
			DateTime [] date_list = new DateTime [0];
			StringCollection string_coll = new StringCollection ();

			Assert.AreEqual (typeof (int), ListBindingHelper.GetListItemType (list), "#A1");
			Assert.AreEqual (typeof (DateTime), ListBindingHelper.GetListItemType (date_list), "#A2");
			Assert.AreEqual (typeof (string), ListBindingHelper.GetListItemType (string_coll), "#A4");

			// Returns the type of the first item if could enumerate
			ArrayList arraylist = new ArrayList ();
			arraylist.Add ("hellou");
			arraylist.Add (3.1416);
			Assert.AreEqual (typeof (string), ListBindingHelper.GetListItemType (arraylist), "#B1");

			// Returns the type of the public Item property, not the explicit one
			ListView.ColumnHeaderCollection col_collection = new ListView.ColumnHeaderCollection (null);
			Assert.AreEqual (typeof (ColumnHeader), ListBindingHelper.GetListItemType (col_collection), "#C1");

			ListContainer list_container = new ListContainer ();
			String str = "A";
			Assert.AreEqual (typeof (IList), ListBindingHelper.GetListItemType (list_container, "List"), "#D1");
			Assert.AreEqual (typeof (int), ListBindingHelper.GetListItemType (str, "Length"), "#D2");
			// Property doesn't exist - fallback to object type
			Assert.AreEqual (typeof (object), ListBindingHelper.GetListItemType (str, "DoesntExist"), "#D3");

			// finally, objects that are not array nor list
			Assert.AreEqual (typeof (double), ListBindingHelper.GetListItemType (3.1416), "#E1");
			Assert.AreEqual (null, ListBindingHelper.GetListItemType (null), "#E2");

			// bug #507120 - an IEnumerator instance with a Current value returning null,
			// falling back to IList.this detection, if possible
			Assert.AreEqual (typeof (string), ListBindingHelper.GetListItemType (new NullEnumerable (), null), "#F1");
		}

		// useless class that help us with a simple enumerator with a null Current property
		// and implementing IList to let the ListBindingHelper get info from the this [] property
		class NullEnumerable : IList, ICollection, IEnumerable
		{
			public IEnumerator GetEnumerator ()
			{
				return new NullEnumerator ();
			}

			class NullEnumerator : IEnumerator
			{
				int pos = -1;

				// the idea is that we just move one time - the first time
				public bool MoveNext ()
				{
					if (pos > -1)
						return false;

					pos = 0;
					return true;
				}

				public void Reset ()
				{
					pos = -1;
				}

				public object Current {
					get {
						return null;
					}
				}
			}

			// make this return a string, and hide the interface impl,
			// so we are sure ListBindingHelper is actually accessing this property
			public string this [int index] {
				get {
					if (index != 0)
						throw new ArgumentOutOfRangeException ("index");

					return null;
				}
				set {
				}
			}

			object IList.this [int index] {
				get {
					return this [index];
				}
				set {
				}
			}

			public int Add (object o)
			{
				return 0;
			}

			public void Clear ()
			{
			}

			public bool Contains (object o)
			{
				return false;
			}

			public int IndexOf (object o)
			{
				return -1;
			}

			public void Insert (int index, object o)
			{
			}

			public void Remove (object o)
			{
			}

			public void RemoveAt (int index)
			{
			}

			public bool IsFixedSize {
				get {
					return true;
				}
			}

			public bool IsReadOnly {
				get {
					return true;
				}
			}

			public void CopyTo (Array array, int offset)
			{
			}

			public int Count {
				get {
					return 1;
				}
			}

			public bool IsSynchronized {
				get {
					return false;
				}
			}

			public object SyncRoot {
				get {
					return this;
				}
			}
		}

		[Test]
		public void GetListNameTest ()
		{
			List<int> list = new List<int> ();
			int [] arr = new int [0];
			SimpleItem item = new SimpleItem ();

			Assert.AreEqual (typeof (int).Name, ListBindingHelper.GetListName (list, null), "1");
			Assert.AreEqual (typeof (int).Name, ListBindingHelper.GetListName (arr, null), "2");
			Assert.AreEqual (typeof (SimpleItem).Name, ListBindingHelper.GetListName (item, null), "3");
			Assert.AreEqual (String.Empty, ListBindingHelper.GetListName (null, null), "4");
		}
	}
}

#endif
