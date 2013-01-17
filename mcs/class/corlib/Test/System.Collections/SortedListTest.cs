// SortedListTest.cs - NUnit Test Cases for the System.Collections.SortedList class
//
// Authors:
//      Jaak Simm
//      Duncan Mak (duncan@ximian.com)
//
// Thanks go to David Brandt (bucky@keystreams.com),
// because this file is based on his ArrayListTest.cs
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// 
// main TODO: additional tests for functions affected by
//            fixedsize and read-only properties 


using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.Collections
{
	public class SortedListComparer : IComparer
	{
		public int Compare (object x, object y)
		{
			return x.GetHashCode () - y.GetHashCode ();
		}
	}

	[TestFixture]
	public class SortedListTest
	{
		protected SortedList sl1;
		protected SortedList sl2;
		protected SortedList emptysl;
		protected const int icap = 16;

		public void TestConstructor1 ()
		{
			SortedList temp1 = new SortedList ();
			Assert.IsNotNull (temp1, "#1");
#if !NET_2_0 // no such expectation as it is broken in .NET 2.0
			Assert.AreEqual (icap, temp1.Capacity, "#2");
#endif
		}

		[Test]
		public void TestConstructor2 ()
		{
			Comparer c = Comparer.Default;
			SortedList temp1 = new SortedList (c);
			Assert.IsNotNull (temp1, "#1");
#if !NET_2_0 // no such expectation as it is broken in .NET 2.0
			Assert.AreEqual (icap, temp1.Capacity, "#2");
#endif
		}

		[Test]
		public void TestConstructor3 ()
		{
			Hashtable d = new Hashtable ();
			d.Add ("one", "Mircosoft");
			d.Add ("two", "will");
			d.Add ("three", "rule");
			d.Add ("four", "the world");

			SortedList temp1 = new SortedList (d);
			Assert.IsNotNull (temp1, "#A1");
			Assert.AreEqual (4, temp1.Capacity, "#A2");
			Assert.AreEqual (4, temp1.Count, "#A3");

			try {
				new SortedList ((Hashtable) null);
				Assert.Fail ("#B");
			} catch (ArgumentNullException) {
			}

			try {
				d = new Hashtable ();
				d.Add ("one", "Mircosoft");
				d.Add ("two", "will");
				d.Add ("three", "rule");
				d.Add ("four", "the world");
				d.Add (7987, "lkj");
				new SortedList (d);
				Assert.Fail ("#C");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void TestConstructor4 ()
		{
			SortedList temp1 = new SortedList (17);
			Assert.IsNotNull (temp1, "#A1");
			Assert.AreEqual (17, temp1.Capacity, "#A2");

			try {
				new SortedList (-6);
				Assert.Fail ("#B");
			} catch (ArgumentOutOfRangeException) {
			}

			temp1 = new SortedList (0);
			Assert.IsNotNull (temp1, "#C");
		}

		[Test]
		public void TestConstructor5 ()
		{
			Comparer c = Comparer.Default;
			SortedList temp1 = new SortedList (c, 27);
			Assert.IsNotNull (temp1, "#A1");
			Assert.AreEqual (27, temp1.Capacity, "#A2");

			try {
				new SortedList (-12);
				Assert.Fail ("#B");
			} catch (ArgumentOutOfRangeException) {
			}
		}

#if !NET_2_0 // no such expectation as it is broken in .NET 2.0
		[Test]
		public void Constructor_Capacity ()
		{
			SortedList sl = new SortedList (0);
			Assert.AreEqual (0, sl.Capacity, "#A1");
			sl.Capacity = 0;
			// doesn't reset to class default (16)
			Assert.AreEqual (0, sl.Capacity, "#A2");

			for (int i = 1; i <= 16; i++) {
				sl = new SortedList (i);
				Assert.AreEqual (i, sl.Capacity, "#B1:" + i);
				sl.Capacity = 0;
				// reset to class default (16)
				Assert.AreEqual (16, sl.Capacity, "#B2:" + i);
			}
		}
#endif

		[Test]
		public void TestIsSynchronized ()
		{
			SortedList sl1 = new SortedList ();
			Assert.IsFalse (sl1.IsSynchronized, "#1");
			SortedList sl2 = SortedList.Synchronized (sl1);
			Assert.IsTrue (sl2.IsSynchronized, "#2");
		}

		[Test]
		public void TestCapacity ()
		{
			for (int i = 0; i < 100; i++) {
				SortedList sl1 = new SortedList (i);
				Assert.AreEqual (i, sl1.Capacity, i.ToString ());
			}
		}

		[Test]
		public void TestCapacity2 ()
		{
			SortedList list = new SortedList ();

			list.Capacity = 5;
			Assert.AreEqual (5, list.Capacity, "#1");

			SortedList sync = SortedList.Synchronized (list);
			Assert.AreEqual (5, sync.Capacity, "#2");

			list.Capacity = 20;
			Assert.AreEqual (20, list.Capacity, "#3");
			Assert.AreEqual (20, sync.Capacity, "#4");
		}

		[Test]
		public void TestCapacity3 ()
		{
			int new_capacity = 5;
			SortedList list = new SortedList (1000);
			list.Capacity = new_capacity;

			Assert.AreEqual (new_capacity, list.Capacity);
		}

		[Test]
		public void Capacity_BackTo0 ()
		{
			SortedList list = new SortedList (42);
			Assert.AreEqual (42, list.Capacity, "#1");
			list.Capacity = 0;
		}

		[Test]
		[ExpectedException (typeof (OutOfMemoryException))]
		public void TestCapacity4 ()
		{
			SortedList list = new SortedList ();
			list.Capacity = Int32.MaxValue;
		}

		[Test]
		public void TestCount ()
		{
			SortedList sl1 = new SortedList ();
			Assert.AreEqual (0, sl1.Count, "#1");
			for (int i = 1; i <= 100; i++) {
				sl1.Add ("" + i, "" + i);
				Assert.AreEqual (i, sl1.Count, "#2:" + i);
			}
		}

		[Test]
		public void TestIsFixed ()
		{
			SortedList sl1 = new SortedList ();
			Assert.IsFalse (sl1.IsFixedSize);
		}

		[Test]
		public void TestIsReadOnly ()
		{
			SortedList sl1 = new SortedList ();
			Assert.IsFalse (sl1.IsReadOnly);
		}

		[Test]
		public void TestItem ()
		{
			SortedList sl1 = new SortedList ();

			object o = sl1 [-1];
			Assert.IsNull (o, "#A");

			try {
				o = sl1 [(string) null];
				Assert.Fail ("#B");
			} catch (ArgumentNullException) {
			}

			for (int i = 0; i <= 100; i++)
				sl1.Add ("kala " + i, i);
			for (int i = 0; i <= 100; i++)
				Assert.AreEqual (i, sl1 ["kala " + i], "#C:" + i);
		}

		[Test]
		public void TestSyncRoot ()
		{
			SortedList sl1 = new SortedList ();
			Assert.IsNotNull (sl1.SyncRoot);
			/*
			lock( sl1.SyncRoot ) {
				foreach ( Object item in sl1 ) {
					item="asdf";
					Assert ("sl.SyncRoot: item not read-only",item.IsReadOnly);
				}
			}
			*/
		}

		[Test]
		public void TestValues ()
		{
			SortedList sl1 = new SortedList ();
			ICollection ic1 = sl1.Values;
			for (int i = 0; i <= 100; i++) {
				sl1.Add ("kala " + i, i);
				Assert.AreEqual (ic1.Count, sl1.Count);
			}
		}


		// TODO: Add with IComparer
		[Test]
		public void TestAdd ()
		{
			// seems SortedList cannot be set fixedsize or readonly
			SortedList sl1 = new SortedList ();

			try {
				sl1.Add ((string) null, "kala");
				Assert.Fail ("#A");
			} catch (ArgumentNullException) {
			}

			for (int i = 1; i <= 100; i++) {
				sl1.Add ("kala " + i, i);
				Assert.AreEqual (i, sl1.Count, "#B1:" + i);
				Assert.AreEqual (i, sl1 ["kala " + i], "#B2:" + i);
			}

			try {
				sl1.Add ("kala", 10);
				sl1.Add ("kala", 11);
				Assert.Fail ("#C");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void TestClear ()
		{
			SortedList sl1 = new SortedList (10);
			sl1.Add ("kala", 'c');
			sl1.Add ("kala2", 'd');
			Assert.AreEqual (10, sl1.Capacity, "#A1");
			Assert.AreEqual (2, sl1.Count, "#A2");
			sl1.Clear ();
			Assert.AreEqual (0, sl1.Count, "#B1");
#if !NET_2_0 // no such expectation as it is broken in .NET 2.0
			Assert.AreEqual (16, sl1.Capacity, "#B2");
#endif
		}

#if !NET_2_0 // no such expectation as it is broken in .NET 2.0
		[Test]
		public void Clear_Capacity ()
		{
			// strangely Clear change the default capacity (while Capacity doesn't)
			for (int i = 0; i <= 16; i++) {
				SortedList sl = new SortedList (i);
				Assert.AreEqual (i, sl.Capacity, "#1:"+ i);
				sl.Clear ();
				// reset to class default (16)
				Assert.AreEqual (16, sl.Capacity, "#2:" + i);
			}
		}

		[Test]
		public void Clear_Capacity_Reset ()
		{
			SortedList sl = new SortedList (0);
			Assert.AreEqual (0, sl.Capacity, "#1");
			sl.Clear ();
			// reset to class default (16)
			Assert.AreEqual (16, sl.Capacity, "#2");
			sl.Capacity = 0;
			Assert.AreEqual (16, sl.Capacity, "#3");
			// note: we didn't return to 0 - so Clear cahnge the default capacity
		}
#endif

		[Test]
		public void ClearDoesNotTouchCapacity ()
		{
			SortedList sl = new SortedList ();
			// according to MSDN docs Clear () does not change capacity
			for (int i = 0; i < 18; i++) {
				sl.Add (i, i);
			}
			int capacityBeforeClear = sl.Capacity;
			sl.Clear ();
			int capacityAfterClear = sl.Capacity;
			Assert.AreEqual (capacityBeforeClear, capacityAfterClear);
		}

		[Test]
		public void TestClone ()
		{
			{
				SortedList sl1 = new SortedList (10);
				for (int i = 0; i <= 50; i++)
					sl1.Add ("kala " + i, i);
				SortedList sl2 = (SortedList) sl1.Clone ();
				for (int i = 0; i <= 50; i++)
					Assert.AreEqual (sl1 ["kala " + i], sl2 ["kala " + i], "#A:" + i);
			}
			{
				char [] d10 = { 'a', 'b' };
				char [] d11 = { 'a', 'c' };
				char [] d12 = { 'b', 'c' };
				//char[][] d1 = {d10, d11, d12};
				SortedList sl1 = new SortedList ();
				sl1.Add ("d1", d10);
				sl1.Add ("d2", d11);
				sl1.Add ("d3", d12);
				SortedList sl2 = (SortedList) sl1.Clone ();
				Assert.AreEqual (sl1 ["d1"], sl2 ["d1"], "#B1");
				Assert.AreEqual (sl1 ["d2"], sl2 ["d2"], "#B2");
				Assert.AreEqual (sl1 ["d3"], sl2 ["d3"], "#B3");
				((char []) sl1 ["d1"]) [0] = 'z';
				Assert.AreEqual (sl1 ["d1"], sl2 ["d1"], "#B4");
			}
		}

		[Test]
		public void TestContains ()
		{
			SortedList sl1 = new SortedList (55);
			for (int i = 0; i <= 50; i++) { sl1.Add ("kala " + i, i); }

			try {
				if (sl1.Contains (null)) {
				}
				Assert.Fail ("#A");
			} catch (ArgumentNullException) {
			}

			Assert.IsTrue (sl1.Contains ("kala 17"), "#B1");
			Assert.IsFalse (sl1.Contains ("ohoo"), "#B2");
		}

		[Test]
		public void TestContainsKey ()
		{
			SortedList sl1 = new SortedList (55);
			for (int i = 0; i <= 50; i++) { sl1.Add ("kala " + i, i); }

			try {
				if (sl1.ContainsKey (null)) {
				}
				Assert.Fail ("#A");
			} catch (ArgumentNullException) {
			}

			Assert.IsTrue (sl1.ContainsKey ("kala 17"), "#B1");
			Assert.IsFalse (sl1.ContainsKey ("ohoo"), "#B2");
		}

		[Test]
		public void TestContainsValue ()
		{
			SortedList sl1 = new SortedList (55);
			sl1.Add (0, "zero");
			sl1.Add (1, "one");
			sl1.Add (2, "two");
			sl1.Add (3, "three");
			sl1.Add (4, "four");

			Assert.IsTrue (sl1.ContainsValue ("zero"), "#1");
			Assert.IsFalse (sl1.ContainsValue ("ohoo"), "#2");
			Assert.IsFalse (sl1.ContainsValue (null), "#3");
		}

		[Test]
		public void TestCopyTo ()
		{
			SortedList sl1 = new SortedList ();
			for (int i = 0; i <= 10; i++) { sl1.Add ("kala " + i, i); }
			{
				try {
					sl1.CopyTo (null, 2);
					Assert.Fail ("sl.CopyTo: does not throw ArgumentNullException when target null");
				} catch (ArgumentNullException) {
				}
			}
			{
				try {
					Char [,] c2 = new Char [2, 2];
					sl1.CopyTo (c2, 2);
					Assert.Fail ("sl.CopyTo: does not throw ArgumentException when target is multiarray");
				} catch (ArgumentException) {
				}
			}
			{
				try {
					Char [] c1 = new Char [2];
					sl1.CopyTo (c1, -2);
					Assert.Fail ("sl.CopyTo: does not throw ArgumentOutOfRangeException when index is negative");
				} catch (ArgumentOutOfRangeException) {
				}
			}
			{
				try {
					Char [] c1 = new Char [2];
					sl1.CopyTo (c1, 3);
					Assert.Fail ("sl.CopyTo: does not throw ArgumentException when index is too large");
				} catch (ArgumentException) {
				}
			}
			{
				try {
					Char [] c1 = new Char [2];
					sl1.CopyTo (c1, 1);
					Assert.Fail ("sl.CopyTo: does not throw ArgumentException when SortedList too big for the array");
				} catch (ArgumentException) {
				}
			}
			{
				try {
					Char [] c2 = new Char [15];
					sl1.CopyTo (c2, 0);
					Assert.Fail ("sl.CopyTo: does not throw InvalidCastException when incompatible data types");
				} catch (InvalidCastException) {
				}
			}

			// CopyTo function does not work well with SortedList
			// even example at MSDN gave InvalidCastException
			// thus, it is NOT tested here
			/*
					sl1.Clear();
					for (int i = 0; i <= 5; i++) {sl1.Add(i,""+i);}
			    Char[] copy = new Char[15];
			    Array.Clear(copy,0,copy.Length);
			    copy.SetValue( "The", 0 );
			    copy.SetValue( "quick", 1 );
			    copy.SetValue( "brown", 2 );
			    copy.SetValue( "fox", 3 );
			    copy.SetValue( "jumped", 4 );
			    copy.SetValue( "over", 5 );
			    copy.SetValue( "the", 6 );
			    copy.SetValue( "lazy", 7 );
			    copy.SetValue( "dog", 8 );
					sl1.CopyTo(copy,1);
					AssertEquals("sl.CopyTo: incorrect copy(1).","The", copy.GetValue(0));
					AssertEquals("sl.CopyTo: incorrect copy(1).","quick", copy.GetValue(1));
					for (int i=2; i<8; i++) AssertEquals("sl.CopyTo: incorrect copy(2).",sl1["kala "+(i-2)], copy.GetValue(i));
					AssertEquals("sl.CopyTo: incorrect copy(3).","dog", copy.GetValue(8));
			*/
		}

		public SortedList DefaultSL ()
		{
			SortedList sl1 = new SortedList ();
			sl1.Add (1.0, "The");
			sl1.Add (1.1, "quick");
			sl1.Add (34.0, "brown");
			sl1.Add (-100.75, "fox");
			sl1.Add (1.4, "jumped");
			sl1.Add (1.5, "over");
			sl1.Add (1.6, "the");
			sl1.Add (1.7, "lazy");
			sl1.Add (1.8, "dog");
			return sl1;
		}

		public IList DefaultValues ()
		{
			IList il = new ArrayList ();
			il.Add ("fox");
			il.Add ("The");
			il.Add ("quick");
			il.Add ("jumped");
			il.Add ("over");
			il.Add ("the");
			il.Add ("lazy");
			il.Add ("dog");
			il.Add ("brown");
			return il;
		}

		[Test]
		public void TestGetByIndex ()
		{
			SortedList sl1 = DefaultSL ();
			Assert.AreEqual ("over", sl1.GetByIndex (4), "#A1");
			Assert.AreEqual ("brown", sl1.GetByIndex (8), "#A2");

			try {
				sl1.GetByIndex (-1);
				Assert.Fail ("#B");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				sl1.GetByIndex (100);
				Assert.Fail ("#C");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void GetEnumerator ()
		{
			SortedList sl1 = DefaultSL ();
			IDictionaryEnumerator e = sl1.GetEnumerator ();
			Assert.IsNotNull (e, "#1");
			Assert.IsTrue (e.MoveNext (), "#2");
			Assert.IsNotNull (e.Current, "#3");
			Assert.IsTrue ((e is ICloneable), "#4");
			Assert.IsTrue ((e is IDictionaryEnumerator), "#5");
			Assert.IsTrue ((e is IEnumerator), "#6");
		}

		[Test]
		public void TestGetKey ()
		{
			SortedList sl1 = DefaultSL ();
			Assert.AreEqual (1.5, sl1.GetKey (4), "#A1");
			Assert.AreEqual (34.0, sl1.GetKey (8), "#A2");

			try {
				sl1.GetKey (-1);
				Assert.Fail ("#B");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				sl1.GetKey (100);
				Assert.Fail ("#C");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void TestGetKeyList ()
		{
			SortedList sl1 = DefaultSL ();
			IList keys = sl1.GetKeyList ();
			Assert.IsNotNull (keys, "#A1");
			Assert.IsTrue (keys.IsReadOnly, "#A2");
			Assert.AreEqual (9, keys.Count, "#A3");
			Assert.AreEqual (1.4, keys [3], "#A4");
			sl1.Add (33.9, "ehhe");
			Assert.AreEqual (10, keys.Count, "#B1");
			Assert.AreEqual (33.9, keys [8], "#B2");
		}

		[Test]
		public void TestGetValueList ()
		{
			SortedList sl1 = DefaultSL ();
			IList originalvals = DefaultValues ();
			IList vals = sl1.GetValueList ();
			Assert.IsNotNull (vals, "#A1");
			Assert.IsTrue (vals.IsReadOnly, "#A2");
			Assert.AreEqual (vals.Count, sl1.Count, "#A3");

			for (int i = 0; i < sl1.Count; i++) {
				Assert.AreEqual (vals [i], originalvals [i], "#A4:" + i);
			}

			sl1.Add (0.01, "ehhe");
			Assert.AreEqual (10, vals.Count, "#B1");
			Assert.AreEqual ("dog", vals [8], "#B2");
		}

		// TODO: IEnumerable.GetEnumerator [Explicit Interface Implementation]
		/*
		public void TestIEnumerable_GetEnumerator() {
			SortedList sl1 = DefaultSL();
			IEnumerator e = sl1.IEnumerable.GetEnumerator();
			AssertNotNull("sl.GetEnumerator: does not return enumerator", e);
			AssertEquals("sl.GetEnumerator: enumerator not working(1)",e.MoveNext(),true);
			AssertNotNull("sl.GetEnumerator: enumerator not working(2)",e.Current);
		}
		*/

		[Test]
		public void TestIndexOfKey ()
		{
			SortedList sl1 = new SortedList (24);

			for (int i = 0; i <= 50; i++) {
				string s = string.Format ("{0:D2}", i);
				sl1.Add ("kala " + s, i);
			}
			Assert.AreEqual (-1, sl1.IndexOfKey ("kala "), "#A");

			try {
				sl1.IndexOfKey ((string) null);
				Assert.Fail ("#B");
			} catch (ArgumentNullException) {
			}

			try {
				sl1.IndexOfKey (10);
				Assert.Fail ("#C");
			} catch (InvalidOperationException) {
			}

			for (int i = 0; i <= 50; i++) {
				string s = string.Format ("{0:D2}", i);
				Assert.AreEqual (i, sl1.IndexOfKey ("kala " + s), "#D:" + i);
			}
		}

		[Test]
		public void TestIndexOfValue ()
		{
			SortedList sl1 = new SortedList (24);
			string s = null;
			for (int i = 0; i < 50; i++) {
				s = string.Format ("{0:D2}", i);
				sl1.Add ("kala " + s, 100 + i * i);
			}
			for (int i = 0; i < 50; i++) {
				s = string.Format ("{0:D2}", i + 50);
				sl1.Add ("kala " + s, 100 + i * i);
			}
			Assert.AreEqual (-1, sl1.IndexOfValue (102), "#1");
			Assert.AreEqual (-1, sl1.IndexOfValue (null), "#2");
			for (int i = 0; i < 50; i++)
				Assert.AreEqual (i, sl1.IndexOfValue (100 + i * i), "#3:" + i);
		}

		[Test]
		public void TestIndexOfValue2 ()
		{
			SortedList list = new SortedList ();
			list.Add ("key0", "la la");
			list.Add ("key1", "value");
			list.Add ("key2", "value");

			int i = list.IndexOfValue ("value");
			Assert.AreEqual (1, i);
		}

		[Test]
		public void TestIndexOfValue3 ()
		{
			SortedList list = new SortedList ();
			int i = list.IndexOfValue ((string) null);
			Assert.AreEqual (1, -i);
		}

		[Test]
		public void TestIndexer ()
		{
			SortedList list = new SortedList ();

			list.Add (1, new Queue ());
			list.Add (2, new Hashtable ());
			list.Add (3, new Stack ());

			Assert.AreEqual (typeof (Queue), list [1].GetType (), "#1");
			Assert.AreEqual (typeof (Hashtable), list [2].GetType (), "#2");
			Assert.AreEqual (typeof (Stack), list [3].GetType (), "#3");
		}

		[Test]
		public void TestEnumerator ()
		{
			SortedList list = new SortedList ();

			list.Add (1, new Queue ());
			list.Add (2, new Hashtable ());
			list.Add (3, new Stack ());

			foreach (DictionaryEntry d in list) {

				int key = (int) d.Key;
				Type value = d.Value.GetType ();

				switch (key) {
				case 1:
					Assert.AreEqual (typeof (Queue), value, "#1");
					break;

				case 2:
					Assert.AreEqual (typeof (Hashtable), value, "#2");
					break;

				case 3:
					Assert.AreEqual (typeof (Stack), value, "#3");
					break;

				default:
					Assert.Fail ("#4:" + value.FullName);
					break;
				}
			}
		}

		[Test]
		public void TestRemove ()
		{
			SortedList sl1 = new SortedList (24);
			string s = null;
			int k;
			for (int i = 0; i < 50; i++) sl1.Add ("kala " + i, i);

			try {
				sl1.Remove (s);
				Assert.Fail ("#A");
			} catch (ArgumentNullException) {
			}

			k = sl1.Count;
			sl1.Remove ("kala ");
			Assert.AreEqual (k, sl1.Count, "#B");

			try {
				sl1.Remove (15);
				Assert.Fail ("#C");
			} catch (InvalidOperationException) {
			}

			for (int i = 15; i < 20; i++)
				sl1.Remove ("kala " + i);
			for (int i = 45; i < 55; i++)
				sl1.Remove ("kala " + i);

			Assert.AreEqual (40, sl1.Count, "#D1");
			for (int i = 45; i < 55; i++)
				Assert.IsNull (sl1 ["kala " + i], "#D2:" + i);
		}

		[Test]
		public void TestRemoveAt ()
		{
			SortedList sl1 = new SortedList (24);
			int k;

			for (int i = 0; i < 50; i++) {
				string s = string.Format ("{0:D2}", i);
				sl1.Add ("kala " + s, i);
			}

			try {
				sl1.RemoveAt (-1);
				Assert.Fail ("#A");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				sl1.RemoveAt (100);
				Assert.Fail ("#B");
			} catch (ArgumentOutOfRangeException) {
			}

			k = sl1.Count;

			for (int i = 0; i < 20; i++)
				sl1.RemoveAt (9);

			Assert.AreEqual (30, sl1.Count, 30, "#C1");
			for (int i = 0; i < 9; i++)
				Assert.AreEqual (i, sl1 ["kala " + string.Format ("{0:D2}", i)], "#C2:" + i);
			for (int i = 9; i < 29; i++)
				Assert.IsNull (sl1 ["kala " + string.Format ("{0:D2}", i)], "#C3:" + i);
			for (int i = 29; i < 50; i++)
				Assert.AreEqual (i, sl1 ["kala " + string.Format ("{0:D2}", i)], "#C4:" + i);
		}

		[Test]
		public void TestSetByIndex ()
		{
			SortedList sl1 = new SortedList (24);
			for (int i = 49; i >= 0; i--) sl1.Add (100 + i, i);

			try {
				sl1.SetByIndex (-1, 77);
				Assert.Fail ("#A");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				sl1.SetByIndex (100, 88);
				Assert.Fail ("#B");
			} catch (ArgumentOutOfRangeException) {
			}

			for (int i = 5; i < 25; i++)
				sl1.SetByIndex (i, -1);
			for (int i = 0; i < 5; i++)
				Assert.AreEqual (i, sl1 [100 + i], "#C1");
			for (int i = 5; i < 25; i++)
				Assert.AreEqual (-1, sl1 [100 + i], "#C2");
			for (int i = 25; i < 50; i++)
				Assert.AreEqual (i, sl1 [100 + i], "#C3");
		}

		[Test]
		public void TestTrimToSize ()
		{
			SortedList sl1 = new SortedList (24);

			sl1.TrimToSize ();
#if !NET_2_0 // no such expectation as it is broken in .NET 2.0
			Assert.AreEqual (icap, sl1.Capacity, "#1");
#endif

			for (int i = 72; i >= 0; i--)
				sl1.Add (100 + i, i);
			sl1.TrimToSize ();
#if !NET_2_0 // no such expectation as it is broken in .NET 2.0
			Assert.AreEqual (73, sl1.Capacity, "#2");
#endif
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void SerializeTest ()
		{
			SortedList sl1 = new SortedList ();
			sl1.Add (5, "A");
			sl1.Add (0, "B");
			sl1.Add (7, "C");

#if TARGET_JVM
			BinaryFormatter bf = (BinaryFormatter)vmw.@internal.remoting.BinaryFormatterUtils.CreateBinaryFormatter (false);
#else
			BinaryFormatter bf = new BinaryFormatter ();
#endif // TARGET_JVM
			bf.AssemblyFormat = FormatterAssemblyStyle.Full;
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, sl1);
			ms.Position = 0;

			SortedList sl2 = (SortedList) bf.Deserialize (ms);
			Assert.IsNotNull (sl2, "#1");
			Assert.AreEqual (3, sl2.Count, "#2");
			Assert.AreEqual (sl1 [0], sl2 [0], "#3");
			Assert.AreEqual (sl1 [1], sl2 [1], "#4");
			Assert.AreEqual (sl1 [2], sl2 [2], "#5");
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void Keys_Serialize ()
		{
			SortedList sl = new SortedList ();
			sl.Add (5, "A");
			sl.Add (0, "B");
			sl.Add (7, "C");

			IList keys1 = (IList) sl.Keys;
#if TARGET_JVM
			BinaryFormatter bf = (BinaryFormatter)vmw.@internal.remoting.BinaryFormatterUtils.CreateBinaryFormatter (false);
#else
			BinaryFormatter bf = new BinaryFormatter ();
#endif // TARGET_JVM
			bf.AssemblyFormat = FormatterAssemblyStyle.Full;
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, keys1);
			ms.Position = 0;

			IList keys2 = (IList) bf.Deserialize (ms);
			Assert.IsNotNull (keys2, "#1");
			Assert.AreEqual (3, keys2.Count, "#2");
			Assert.AreEqual (keys1 [0], keys2 [0], "#3");
			Assert.AreEqual (keys1 [1], keys2 [1], "#4");
			Assert.AreEqual (keys1 [2], keys2 [2], "#5");
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void Values_Serialize ()
		{
			SortedList sl = new SortedList ();
			sl.Add (5, "A");
			sl.Add (0, "B");
			sl.Add (7, "C");

			IList values1 = (IList) sl.Values;
#if TARGET_JVM
			BinaryFormatter bf = (BinaryFormatter)vmw.@internal.remoting.BinaryFormatterUtils.CreateBinaryFormatter (false);
#else
			BinaryFormatter bf = new BinaryFormatter ();
#endif // TARGET_JVM
			bf.AssemblyFormat = FormatterAssemblyStyle.Full;
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, values1);
			ms.Position = 0;

			IList values2 = (IList) bf.Deserialize (ms);
			Assert.IsNotNull (values2, "#1");
			Assert.AreEqual (3, values2.Count, "#2");
			Assert.AreEqual (values1 [0], values2 [0], "#3");
			Assert.AreEqual (values1 [1], values2 [1], "#4");
			Assert.AreEqual (values1 [2], values2 [2], "#5");
		}

		[Test]
		[Category ("NotWorking")]
		public void Values_Deserialize ()
		{
#if TARGET_JVM
			BinaryFormatter bf = (BinaryFormatter)vmw.@internal.remoting.BinaryFormatterUtils.CreateBinaryFormatter (false);
#else
			BinaryFormatter bf = new BinaryFormatter ();
#endif // TARGET_JVM

			MemoryStream ms = new MemoryStream ();
			ms.Write (_serializedValues, 0, _serializedValues.Length);
			ms.Position = 0;

			IList values = (IList) bf.Deserialize (ms);
			Assert.AreEqual (3, values.Count, "#1");
			Assert.AreEqual ("B", values [0], "#2");
			Assert.AreEqual ("A", values [1], "#3");
			Assert.AreEqual ("C", values [2], "#4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SetIdenticalObjectException ()
		{
			// Even though the key/value pair being set are identical to
			// the existing one, it causes snapshot out of sync.
			SortedList sl = new SortedList ();
			sl ["foo"] = "bar";
			foreach (string s in sl.Keys)
				sl ["foo"] = "bar";
		}

		[Test]
		public void Ctor_IComparer ()
		{
			SortedList sl = new SortedList (new SortedListComparer ());
			sl.Add (new object (), new object ());
		}

		[Test]
		public void Ctor_IComparer_Null ()
		{
			SortedList sl = new SortedList ((IComparer) null);
			sl.Add (new object (), new object ());
		}

		[Test]
		public void Ctor_IDictionary_IComparer_Before ()
		{
			Hashtable ht = new Hashtable ();
			ht.Add (2, "a");
			ht.Add (1, "b");
			// adding a non-IComparable in Hashtable
			ht.Add (new object (), "c");
			SortedList sl = new SortedList (ht, new SortedListComparer ());
			Assert.AreEqual (3, sl.Count);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Ctor_IDictionary_DefaultInvariant_Before ()
		{
			Hashtable ht = new Hashtable ();
			ht.Add (2, "a");
			ht.Add (1, "b");
			// adding a non-IComparable in Hashtable
			ht.Add (new object (), "c");
			SortedList sl = new SortedList (ht, Comparer.DefaultInvariant);
			Assert.AreEqual (3, sl.Count);
		}

		[Test]
		public void Ctor_IDictionary_IComparer_Null_Before_1item ()
		{
			Hashtable ht = new Hashtable ();
			// adding a non-IComparable in Hashtable
			ht.Add (new object (), "c");
			SortedList sl = new SortedList (ht, null);
			Assert.AreEqual (1, sl.Count);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Ctor_IDictionary_IComparer_Null_Before_2items ()
		{
			Hashtable ht = new Hashtable ();
			ht.Add (2, "a");
			// adding a non-IComparable in Hashtable
			ht.Add (new object (), "c");
			SortedList sl = new SortedList (ht, null);
			Assert.AreEqual (2, sl.Count);
		}

		[Test]
		public void Ctor_IDictionary_IComparer_After ()
		{
			Hashtable ht = new Hashtable ();
			ht.Add (2, "a");
			ht.Add (1, "b");
			SortedList sl = new SortedList (ht, new SortedListComparer ());
			Assert.AreEqual (2, sl.Count);
			// adding a non-IComparable in SortedList
			sl.Add (new object (), "c");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Ctor_IDictionary_DefaultInvariant_After ()
		{
			Hashtable ht = new Hashtable ();
			ht.Add (2, "a");
			ht.Add (1, "b");
			SortedList sl = new SortedList (ht, Comparer.DefaultInvariant);
			Assert.AreEqual (2, sl.Count);
			// adding a non-IComparable in SortedList
			sl.Add (new object (), "c");
		}

		[Test]
		public void Ctor_IDictionary_IComparer_Null_After_1item ()
		{
			SortedList sl = new SortedList (new Hashtable (), null);
			sl.Add (new object (), "b");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Ctor_IDictionary_IComparer_Null_After_2items ()
		{
			SortedList sl = new SortedList (new Hashtable (), null);
			sl.Add (2, "a");
			sl.Add (new object (), "b");
		}

		[Test]
		public void IComparer_Clone ()
		{
			SortedList sl = new SortedList (new SortedListComparer ());
			sl.Add (new object (), new object ());
			SortedList clone = (SortedList) sl.Clone ();
		}

		[Test]
		public void IComparer_Null_Clone ()
		{
			SortedList sl = new SortedList ((IComparer) null);
			sl.Add (new object (), new object ());
			SortedList clone = (SortedList) sl.Clone ();
		}

		sealed class StartsWithComparator : IComparer {
			public static readonly StartsWithComparator Instance = new StartsWithComparator();

			public int Compare(object p, object w)
			{
				string part = (string) p;
				string whole = (string) w;
				// let the default string comparer deal with null or when part is not smaller then whole
				if (part == null || whole == null || part.Length >= whole.Length)
					return String.Compare (part, whole);

				// loop through all characters that part and whole have in common
				int pos = 0;
				bool match;
				do {
					match = (part[pos] == whole[pos]);
				} while (match && ++pos < part.Length);

				// return result of last comparison
				return match ? 0 : (part[pos] < whole[pos] ? -1 : 1);
			}
		}

		sealed class StartsWithComparatorPartWholeCheck : IComparer
		{
			public static readonly StartsWithComparator Instance = new StartsWithComparator();

			public int Compare(object p, object w)
			{
				string part = (string) p;
				string whole = (string) w;
				Assert.IsTrue(part == "Part", "#PWC0");
				Assert.IsTrue(whole == "Whole", "#PWC1");

				// let the default string comparer deal with null or when part is not smaller then whole
				if (part == null || whole == null || part.Length >= whole.Length)
					return String.Compare(part, whole);

				// loop through all characters that part and whole have in common
				int pos = 0;
				bool match;
				do {
					match = (part[pos] == whole[pos]);
				} while (match && ++pos < part.Length);

				// return result of last comparison
				return match ? 0 : (part[pos] < whole[pos] ? -1 : 1);
			}
		}

		[Test]
		public void ComparatorUsageTest()
		{
			SortedList sl = new SortedList(StartsWithComparator.Instance);

			sl.Add("Apples", "Value-Apples");
			sl.Add("Bananas", "Value-Bananas");
			sl.Add("Oranges", "Value-Oranges");

			// Ensure 3 objects exist in the collection
			Assert.IsTrue(sl.Count == 3, "Count");

			// Complete Match Test Set
			Assert.IsTrue(sl.ContainsKey("Apples"), "#A0");
			Assert.IsTrue(sl.ContainsKey("Bananas"), "#A1");
			Assert.IsTrue(sl.ContainsKey("Oranges"), "#A2");

			// Partial Match Test Set
			Assert.IsTrue(sl.ContainsKey("Apples are great fruit!"), "#B0");
			Assert.IsTrue(sl.ContainsKey("Bananas are better fruit."), "#B1");
			Assert.IsTrue(sl.ContainsKey("Oranges are fun to peel."), "#B2");

			// Reversed Match Test Set
			Assert.IsFalse(sl.ContainsKey("Value"), "#C0");

			// No match tests
			Assert.IsFalse(sl.ContainsKey("I forgot to bring my bananas."), "#D0");
			Assert.IsFalse(sl.ContainsKey("My apples are on vacation."), "#D0");
			Assert.IsFalse(sl.ContainsKey("The oranges are not ripe yet."), "#D0");

		}

		[Test]
		public void ComparatorPartWholeCheck()
		{
			SortedList sl = new SortedList (StartsWithComparatorPartWholeCheck.Instance);
			sl.Add("Part", "Value-Part");
			Assert.IsFalse(sl.ContainsKey("Whole"), "#PWC2");
		}

		[Test]
		public void NonComparatorStringCheck()
		{
			SortedList sl = new SortedList ();

			sl.Add("Oranges", "Value-Oranges");
			sl.Add("Apples", "Value-Apples");
			sl.Add("Bananas", "Value-Bananas");

			int i = 0;
			Assert.IsTrue(sl.Count == 3, "NCSC #A0");

			Assert.IsTrue(sl.ContainsKey("Apples"), "NCSC #B1");
			Assert.IsTrue(sl.ContainsKey("Bananas"), "NCSC #B2");
			Assert.IsTrue(sl.ContainsKey("Oranges"), "NCSC #B3");

			Assert.IsFalse(sl.ContainsKey("XApples"), "NCSC #C1");
			Assert.IsFalse(sl.ContainsKey("XBananas"), "NCSC #C2");
			Assert.IsFalse(sl.ContainsKey("XOranges"), "NCSC #C3");

			string [] keys = new string [sl.Keys.Count];
			sl.Keys.CopyTo (keys, 0);
			Assert.IsTrue(keys [0] == "Apples", "NCSC #D1");
			Assert.IsTrue(keys [1] == "Bananas", "NCSC #D2");
			Assert.IsTrue(keys [2] == "Oranges", "NCSC #D3");
		}

		private static byte [] _serializedValues = new byte [] {
			0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
			0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04,
			0x01, 0x00, 0x00, 0x00, 0x27, 0x53, 0x79, 0x73, 0x74,
			0x65, 0x6d, 0x2e, 0x43, 0x6f, 0x6c, 0x6c, 0x65, 0x63,
			0x74, 0x69, 0x6f, 0x6e, 0x73, 0x2e, 0x53, 0x6f, 0x72,
			0x74, 0x65, 0x64, 0x4c, 0x69, 0x73, 0x74, 0x2b, 0x56,
			0x61, 0x6c, 0x75, 0x65, 0x4c, 0x69, 0x73, 0x74, 0x01,
			0x00, 0x00, 0x00, 0x0a, 0x73, 0x6f, 0x72, 0x74, 0x65,
			0x64, 0x4c, 0x69, 0x73, 0x74, 0x03, 0x1d, 0x53, 0x79,
			0x73, 0x74, 0x65, 0x6d, 0x2e, 0x43, 0x6f, 0x6c, 0x6c,
			0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x73, 0x2e, 0x53,
			0x6f, 0x72, 0x74, 0x65, 0x64, 0x4c, 0x69, 0x73, 0x74,
			0x09, 0x02, 0x00, 0x00, 0x00, 0x04, 0x02, 0x00, 0x00,
			0x00, 0x1d, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e,
			0x43, 0x6f, 0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f,
			0x6e, 0x73, 0x2e, 0x53, 0x6f, 0x72, 0x74, 0x65, 0x64,
			0x4c, 0x69, 0x73, 0x74, 0x07, 0x00, 0x00, 0x00, 0x04,
			0x6b, 0x65, 0x79, 0x73, 0x06, 0x76, 0x61, 0x6c, 0x75,
			0x65, 0x73, 0x05, 0x5f, 0x73, 0x69, 0x7a, 0x65, 0x07,
			0x76, 0x65, 0x72, 0x73, 0x69, 0x6f, 0x6e, 0x08, 0x63,
			0x6f, 0x6d, 0x70, 0x61, 0x72, 0x65, 0x72, 0x07, 0x6b,
			0x65, 0x79, 0x4c, 0x69, 0x73, 0x74, 0x09, 0x76, 0x61,
			0x6c, 0x75, 0x65, 0x4c, 0x69, 0x73, 0x74, 0x05, 0x05,
			0x00, 0x00, 0x03, 0x03, 0x03, 0x08, 0x08, 0x1b, 0x53,
			0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x43, 0x6f, 0x6c,
			0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x73, 0x2e,
			0x43, 0x6f, 0x6d, 0x70, 0x61, 0x72, 0x65, 0x72, 0x25,
			0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x43, 0x6f,
			0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x73,
			0x2e, 0x53, 0x6f, 0x72, 0x74, 0x65, 0x64, 0x4c, 0x69,
			0x73, 0x74, 0x2b, 0x4b, 0x65, 0x79, 0x4c, 0x69, 0x73,
			0x74, 0x27, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e,
			0x43, 0x6f, 0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f,
			0x6e, 0x73, 0x2e, 0x53, 0x6f, 0x72, 0x74, 0x65, 0x64,
			0x4c, 0x69, 0x73, 0x74, 0x2b, 0x56, 0x61, 0x6c, 0x75,
			0x65, 0x4c, 0x69, 0x73, 0x74, 0x09, 0x03, 0x00, 0x00,
			0x00, 0x09, 0x04, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00,
			0x00, 0x03, 0x00, 0x00, 0x00, 0x09, 0x05, 0x00, 0x00,
			0x00, 0x0a, 0x09, 0x01, 0x00, 0x00, 0x00, 0x10, 0x03,
			0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x08, 0x08,
			0x00, 0x00, 0x00, 0x00, 0x08, 0x08, 0x05, 0x00, 0x00,
			0x00, 0x08, 0x08, 0x07, 0x00, 0x00, 0x00, 0x0d, 0x0d,
			0x10, 0x04, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00,
			0x06, 0x07, 0x00, 0x00, 0x00, 0x01, 0x42, 0x06, 0x08,
			0x00, 0x00, 0x00, 0x01, 0x41, 0x06, 0x09, 0x00, 0x00,
			0x00, 0x01, 0x43, 0x0d, 0x0d, 0x04, 0x05, 0x00, 0x00,
			0x00, 0x1b, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e,
			0x43, 0x6f, 0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f,
			0x6e, 0x73, 0x2e, 0x43, 0x6f, 0x6d, 0x70, 0x61, 0x72,
			0x65, 0x72, 0x00, 0x00, 0x00, 0x00, 0x0b };
	}
}
