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
// Copyright 2011 Xamarin Inc.
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Brian O'Keefe (zer0keefie@gmail.com)
//	Marek Safar (marek.safar@gmail.com)
//

#if NET_4_0

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Collections;
using MonoTests.System.Collections.Specialized;

namespace MonoTests.System.Collections.ObjectModel {

	[TestFixture]
	public class ObservableCollectionTest
	{
		[Test]
		public void Constructor ()
		{
			var list = new List<int> { 3 };
			var col = new ObservableCollection<int> (list);
			col.Add (5);
			Assert.AreEqual (1, list.Count, "#1");

			col = new ObservableCollection<int> ((IEnumerable<int>) list);
			col.Add (5);
			Assert.AreEqual (1, list.Count, "#2");
		}

		[Test]
		public void Constructor_Invalid ()
		{
			try {
				new ObservableCollection<int> ((List<int>) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				new ObservableCollection<int> ((IEnumerable<int>) null);
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void Insert()
		{
			bool reached = false;
			ObservableCollection<int> col = new ObservableCollection<int> ();
			col.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				reached = true;
				Assert.AreEqual (NotifyCollectionChangedAction.Add, e.Action, "INS_1");
				Assert.AreEqual (0, e.NewStartingIndex, "INS_2");
				Assert.AreEqual (-1, e.OldStartingIndex, "INS_3");
				Assert.AreEqual (1, e.NewItems.Count, "INS_4");
				Assert.AreEqual (5, (int)e.NewItems [0], "INS_5");
				Assert.AreEqual (null, e.OldItems, "INS_6");
			};
			col.Insert (0, 5);
			Assert.IsTrue (reached, "INS_5");
		}

		[Test]
		public void RemoveAt()
		{
			bool reached = false;
			ObservableCollection<int> col = new ObservableCollection<int> ();
			col.Insert (0, 5);
			col.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				reached = true;
				Assert.AreEqual (NotifyCollectionChangedAction.Remove, e.Action, "REMAT_1");
				Assert.AreEqual (-1, e.NewStartingIndex, "REMAT_2");
				Assert.AreEqual (0, e.OldStartingIndex, "REMAT_3");
				Assert.AreEqual (null, e.NewItems, "REMAT_4");
				Assert.AreEqual (1, e.OldItems.Count, "REMAT_5");
				Assert.AreEqual (5, (int)e.OldItems [0], "REMAT_6");
			};
			col.RemoveAt (0);
			Assert.IsTrue (reached, "REMAT_7");
		}

		[Test]
		public void Move()
		{
			bool reached = false;
			ObservableCollection<int> col = new ObservableCollection<int> ();
			col.Insert (0, 0);
			col.Insert (1, 1);
			col.Insert (2, 2);
			col.Insert (3, 3);
			col.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				reached = true;
				Assert.AreEqual (NotifyCollectionChangedAction.Move, e.Action, "MOVE_1");
				Assert.AreEqual (3, e.NewStartingIndex, "MOVE_2");
				Assert.AreEqual (1, e.OldStartingIndex, "MOVE_3");
				Assert.AreEqual (1, e.NewItems.Count, "MOVE_4");
				Assert.AreEqual (1, e.NewItems [0], "MOVE_5");
				Assert.AreEqual (1, e.OldItems.Count, "MOVE_6");
				Assert.AreEqual (1, e.OldItems [0], "MOVE_7");
			};
			col.Move (1, 3);
			Assert.IsTrue (reached, "MOVE_8");
		}

		[Test]
		public void Add()
		{
			ObservableCollection<char> collection = new ObservableCollection<char> ();
			bool propertyChanged = false;
			List<string> changedProps = new List<string> ();
			NotifyCollectionChangedEventArgs args = null;

			((INotifyPropertyChanged)collection).PropertyChanged += delegate (object sender, PropertyChangedEventArgs e) {
				propertyChanged = true;
				changedProps.Add (e.PropertyName);
			};

			collection.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				args = e;
			};

			collection.Add ('A');

			Assert.IsTrue (propertyChanged, "ADD_1");
			Assert.IsTrue (changedProps.Contains ("Count"), "ADD_2");
			Assert.IsTrue (changedProps.Contains ("Item[]"), "ADD_3");

			CollectionChangedEventValidators.ValidateAddOperation (args, new char [] { 'A' }, 0, "ADD_4");
		}

		[Test]
		public void Remove()
		{
			ObservableCollection<char> collection = new ObservableCollection<char> ();
			bool propertyChanged = false;
			List<string> changedProps = new List<string> ();
			NotifyCollectionChangedEventArgs args = null;

			collection.Add ('A');
			collection.Add ('B');
			collection.Add ('C');

			((INotifyPropertyChanged)collection).PropertyChanged += delegate (object sender, PropertyChangedEventArgs e) {
				propertyChanged = true;
				changedProps.Add (e.PropertyName);
			};

			collection.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				args = e;
			};

			collection.Remove ('B');

			Assert.IsTrue (propertyChanged, "REM_1");
			Assert.IsTrue (changedProps.Contains ("Count"), "REM_2");
			Assert.IsTrue (changedProps.Contains ("Item[]"), "REM_3");

			CollectionChangedEventValidators.ValidateRemoveOperation (args, new char [] { 'B' }, 1, "REM_4");
		}

		[Test]
		public void Set()
		{
			ObservableCollection<char> collection = new ObservableCollection<char> ();
			bool propertyChanged = false;
			List<string> changedProps = new List<string> ();
			NotifyCollectionChangedEventArgs args = null;

			collection.Add ('A');
			collection.Add ('B');
			collection.Add ('C');

			((INotifyPropertyChanged)collection).PropertyChanged += delegate (object sender, PropertyChangedEventArgs e) {
				propertyChanged = true;
				changedProps.Add (e.PropertyName);
			};

			collection.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				args = e;
			};

			collection [2] = 'I';

			Assert.IsTrue (propertyChanged, "SET_1");
			Assert.IsTrue (changedProps.Contains ("Item[]"), "SET_2");

			CollectionChangedEventValidators.ValidateReplaceOperation (args, new char [] { 'C' }, new char [] { 'I' }, 2, "SET_3");
		}

		[Test]
		public void Reentrant()
		{
			ObservableCollection<char> collection = new ObservableCollection<char> ();
			bool propertyChanged = false;
			List<string> changedProps = new List<string> ();
			NotifyCollectionChangedEventArgs args = null;

			collection.Add ('A');
			collection.Add ('B');
			collection.Add ('C');

			PropertyChangedEventHandler pceh = delegate (object sender, PropertyChangedEventArgs e) {
				propertyChanged = true;
				changedProps.Add (e.PropertyName);
			};

			// Adding a PropertyChanged event handler
			((INotifyPropertyChanged)collection).PropertyChanged += pceh;

			collection.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				args = e;
			};

			collection.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				// This one will attempt to break reentrancy
				try {
					collection.Add ('X');
					Assert.Fail ("Reentrancy should not be allowed.");
				} catch (InvalidOperationException) {
				}
			};

			collection [2] = 'I';

			Assert.IsTrue (propertyChanged, "REENT_1");
			Assert.IsTrue (changedProps.Contains ("Item[]"), "REENT_2");

			CollectionChangedEventValidators.ValidateReplaceOperation (args, new char [] { 'C' }, new char [] { 'I' }, 2, "REENT_3");

			// Removing the PropertyChanged event handler should work as well:
			((INotifyPropertyChanged)collection).PropertyChanged -= pceh;
		}

		//Private test class for protected members of ObservableCollection
		private class ObservableCollectionTestHelper : ObservableCollection<char> {
			internal void DoubleEnterReentrant()
			{
				IDisposable object1 = BlockReentrancy ();
				IDisposable object2 = BlockReentrancy ();

				Assert.AreSame (object1, object2);

				//With double block, try the reentrant:
				NotifyCollectionChangedEventArgs args = null;

				CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
					args = e;
				};

				// We need a second callback for reentrancy to matter
				CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
					// Doesn't need to do anything; just needs more than one callback registered.
				};

				// Try adding - this should cause reentrancy, and fail
				try {
					Add ('I');
					Assert.Fail ("Reentrancy should not be allowed. -- #2");
				} catch (InvalidOperationException) {
				}
				
				// Release the first reentrant
				object1.Dispose ();

				// Try adding again - this should cause reentrancy, and fail again
				try {
					Add ('J');
					Assert.Fail ("Reentrancy should not be allowed. -- #3");
				} catch (InvalidOperationException) {
				}

				// Release the reentrant a second time
				object1.Dispose ();

				// This last add should work fine.
				Add ('K');
				CollectionChangedEventValidators.ValidateAddOperation (args, new char [] { 'K' }, 0, "REENTHELP_1");
			}
		}

		[Test]
		public void ReentrantReuseObject()
		{
			ObservableCollectionTestHelper helper = new ObservableCollectionTestHelper ();

			helper.DoubleEnterReentrant ();
		}

		[Test]
		public void Clear()
		{
			List<char> initial = new List<char> ();

			initial.Add ('A');
			initial.Add ('B');
			initial.Add ('C');

			ObservableCollection<char> collection = new ObservableCollection<char> (initial);
			bool propertyChanged = false;
			List<string> changedProps = new List<string> ();
			NotifyCollectionChangedEventArgs args = null;

			((INotifyPropertyChanged)collection).PropertyChanged += delegate (object sender, PropertyChangedEventArgs e) {
				propertyChanged = true;
				changedProps.Add (e.PropertyName);
			};

			collection.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
				args = e;
			};

			collection.Clear ();

			Assert.IsTrue (propertyChanged, "CLEAR_1");
			Assert.IsTrue (changedProps.Contains ("Count"), "CLEAR_2");
			Assert.IsTrue (changedProps.Contains ("Item[]"), "CLEAR_3");

			CollectionChangedEventValidators.ValidateResetOperation (args, "CLEAR_4");
		}
	}
}

#endif