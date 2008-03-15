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
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListBindingHelperTest
	{
		[Test]
		public void GetListTest ()
		{
			ListSource lsource = new ListSource (true);

			Assert.AreEqual (new object [0], ListBindingHelper.GetList (lsource), "#A1");
			Assert.AreEqual ("NonList", ListBindingHelper.GetList ("NonList"), "#A2");
			Assert.AreEqual (null, ListBindingHelper.GetList (null), "#A3");

			Assert.AreEqual (new object [0], ListBindingHelper.GetList (lsource, String.Empty), "#B1");
			Assert.AreEqual ("NonList", ListBindingHelper.GetList ("NonList", String.Empty), "#B2");
			Assert.AreEqual (null, ListBindingHelper.GetList (null, null), "#B3");
			Assert.AreEqual (new object [0], ListBindingHelper.GetList (lsource, null), "#B4");

			ListContainer list_container = new ListContainer ();
			Assert.AreEqual (new object [0], ListBindingHelper.GetList (list_container, "List"), "#C1");

			// Even if IListSource.ContainsListCollection is false, we return the result of GetList ()
			lsource = new ListSource (false);
			Assert.AreEqual (new object [0], ListBindingHelper.GetList (lsource), "#D1");

			try {
				ListBindingHelper.GetList (list_container, "DontExist");
				Assert.Fail ("#EXC1");
			} catch (ArgumentException) {
			}

			try {
				ListBindingHelper.GetList (lsource, "DontExist");
				Assert.Fail ("#EXC2");
			} catch (ArgumentException) {
			}

			// dataMember exists, but is not of IList type
			try {
				ListBindingHelper.GetList (lsource, "NonList");
				Assert.Fail ("#EXC3");
			} catch (ArgumentException) {
			}
		}

		class ListSource : IListSource
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
				return new object [0];
			}
		}

		class ListContainer
		{
			public IList List {
				get {
					return new object [0];
				}
			}

			public object NonList {
				get {
					return new object ();
				}
			}
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
		}

	}
}

#endif
