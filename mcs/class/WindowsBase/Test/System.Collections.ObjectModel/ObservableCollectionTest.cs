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
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Collections.ObjectModel {

	[TestFixture]
	[Category ("NotWorking")]
	public class ObservableCollectionTest {
		[Test]
		public void InsertItem ()
		{
			bool reached = false;
			ObservableCollection<int> col = new ObservableCollection<int>();
			col.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				reached = true;
				Assert.AreEqual (NotifyCollectionChangedAction.Add, e.Action);
				Assert.AreEqual (0, e.NewStartingIndex);
				Assert.AreEqual (-1, e.OldStartingIndex);
				Assert.AreEqual (1, e.NewItems.Count);
				Assert.AreEqual (5, (int)e.NewItems[0]);
				Assert.AreEqual (null, e.OldItems);
			};
			col.Insert (0, 5);
			Assert.IsTrue (reached);
		}

		[Test]
		public void Remove ()
		{
			bool reached = false;
			ObservableCollection<int> col = new ObservableCollection<int>();
			col.Insert (0, 5);
			col.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				reached = true;
				Assert.AreEqual (NotifyCollectionChangedAction.Remove, e.Action);
				Assert.AreEqual (-1, e.NewStartingIndex);
				Assert.AreEqual (0, e.OldStartingIndex);
				Assert.AreEqual (null, e.NewItems);
				Assert.AreEqual (1, e.OldItems.Count);
				Assert.AreEqual (5, (int)e.OldItems[0]);
			};
			col.RemoveAt (0);
			Assert.IsTrue (reached, "reached");
		}

		[Test]
		public void Move ()
		{
			bool reached = false;
			ObservableCollection<int> col = new ObservableCollection<int>();
			col.Insert (0, 0);
			col.Insert (1, 1);
			col.Insert (2, 2);
			col.Insert (3, 3);
			col.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				reached = true;
				Assert.AreEqual (NotifyCollectionChangedAction.Move, e.Action, "1");
				Assert.AreEqual (3, e.NewStartingIndex, "2");
				Assert.AreEqual (1, e.OldStartingIndex, "3");
				Assert.AreEqual (1, e.NewItems.Count, "4");
				Assert.AreEqual (1, e.NewItems[0], "5");
				Assert.AreEqual (1, e.OldItems.Count, "6");
				Assert.AreEqual (1, e.OldItems[0], "7");
			};
			col.Move (1, 3);
			Assert.IsTrue (reached, "8");
		}
	}

}
