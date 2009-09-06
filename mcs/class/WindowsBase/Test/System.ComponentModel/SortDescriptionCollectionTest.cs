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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Brian O'Keefe (zer0keefie@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Specialized;

namespace MonoTests.System.ComponentModel {
	[TestFixture]
	public class SortDescriptionCollectionTest 
	{
		public SortDescriptionCollectionTest()
		{
		}

		[Test]
		public void SortDescriptionCollectionAddTest()
		{
			SortDescriptionCollection sdc = new SortDescriptionCollection ();
			SortDescription addedItem = new SortDescription ("NONE", ListSortDirection.Ascending);

			((INotifyCollectionChanged)sdc).CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				Assert.AreEqual (NotifyCollectionChangedAction.Add, e.Action, "ADD_#0");
				addedItem = (SortDescription)e.NewItems [0];
				Assert.AreEqual (true, addedItem.IsSealed, "ADD_#0b");
			};

			sdc.Add (new SortDescription ("A", ListSortDirection.Ascending));

			Assert.AreEqual ("A", addedItem.PropertyName, "ADD_#1");
			Assert.AreEqual (ListSortDirection.Ascending, addedItem.Direction, "ADD_#2");
			Assert.AreEqual (true, addedItem.IsSealed, "ADD_#3");
		}

		[Test]
		public void SortDescriptionCollectionAddNoHandlerTest()
		{
			SortDescriptionCollection sdc = new SortDescriptionCollection ();
			SortDescription addedItem = new SortDescription ("A", ListSortDirection.Ascending);

			sdc.Add (addedItem);

			addedItem = sdc[0];

			Assert.AreEqual ("A", addedItem.PropertyName, "ADDN_#1");
			Assert.AreEqual (ListSortDirection.Ascending, addedItem.Direction, "ADDN_#2");
			Assert.AreEqual (true, addedItem.IsSealed, "ADDN_#3");
		}

		[Test]
		public void SortDescriptionCollectionRemoveTest()
		{
			SortDescriptionCollection sdc = new SortDescriptionCollection ();
			SortDescription removedItem = new SortDescription ("NONE", ListSortDirection.Ascending);

			sdc.Add (new SortDescription ("A", ListSortDirection.Ascending));

			((INotifyCollectionChanged)sdc).CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				Assert.AreEqual (NotifyCollectionChangedAction.Remove, e.Action, "REM_#0");
				removedItem = (SortDescription)e.OldItems [0];
				Assert.AreEqual (true, removedItem.IsSealed, "REM_#0b");
			};

			sdc.RemoveAt (0);

			Assert.AreEqual ("A", removedItem.PropertyName, "REM_#1");
			Assert.AreEqual (ListSortDirection.Ascending, removedItem.Direction, "REM_#2");
			Assert.AreEqual (true, removedItem.IsSealed, "REM_#3");
		}

		[Test]
		public void SortDescriptionCollectionClearTest()
		{
			SortDescriptionCollection sdc = new SortDescriptionCollection ();
			bool eventFired = false;

			sdc.Add (new SortDescription ("A", ListSortDirection.Ascending));

			((INotifyCollectionChanged)sdc).CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				Assert.AreEqual (NotifyCollectionChangedAction.Reset, e.Action, "CLR_#0");
				eventFired = true;
			};

			sdc.Clear ();

			Assert.IsTrue (eventFired, "CLR_#1");
		}

		[Test]
		public void SortDescriptionCollectionSetTest()
		{
			SortDescriptionCollection sdc = new SortDescriptionCollection ();
			int addEvent = 0, removeEvent = 0;

			SortDescription addedItem = new SortDescription ("NONE", ListSortDirection.Ascending);
			SortDescription removedItem = new SortDescription ("NONE", ListSortDirection.Ascending);

			sdc.Add (new SortDescription ("A", ListSortDirection.Ascending));

			((INotifyCollectionChanged)sdc).CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				switch (e.Action) {
					case NotifyCollectionChangedAction.Add:
						addEvent++;
						addedItem = (SortDescription)e.NewItems [0];
						break;
					case NotifyCollectionChangedAction.Remove:
						removeEvent++;
						removedItem = (SortDescription)e.OldItems [0];
						break;
					default:
						Assert.Fail ("SET_#0");
						break;
				}
			};

			sdc [0] = new SortDescription ("B", ListSortDirection.Descending);

			Assert.AreEqual (1, addEvent, "SET_#1");
			Assert.AreEqual (1, removeEvent, "SET_#2");

			Assert.AreEqual ("A", removedItem.PropertyName, "REM_#1");
			Assert.AreEqual (ListSortDirection.Ascending, removedItem.Direction, "REM_#2");
			Assert.AreEqual (true, removedItem.IsSealed, "REM_#3");

			Assert.AreEqual ("B", addedItem.PropertyName, "ADD_#1");
			Assert.AreEqual (ListSortDirection.Descending, addedItem.Direction, "ADD_#2");
			Assert.AreEqual (true, addedItem.IsSealed, "ADD_#3");
		}
	}
}
