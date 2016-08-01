// 
// System.Collections.SortedListTest.cs
// 
// Author:
//   Zoltan Varga (vargaz@gmail.com)
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright 2012 Xamarin Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace MonoTests.System.Collections.Generic
{
	[TestFixture]
	public class SortedListTest
	{
		SortedList<int, string> list;
		SortedList<string, int> list2;

		[SetUp]
		public void SetUp () {
			list = new SortedList <int, string> ();

			list [0] = "A";
			list [5] = "C";
			list [2] = "B";

			list2 = new SortedList<string, int> ();
		}

		[Test]
		public void Item () {
			Assert.AreEqual ("A", list [0]);
			Assert.AreEqual ("B", list [2]);
			Assert.AreEqual ("C", list [5]);

			list [2] = "D";

			Assert.AreEqual ("D", list [2]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ItemNullKey () {
			int i = list2 [null];
		}

		[Test]
		[ExpectedException (typeof (KeyNotFoundException))]
		public void ItemMissingKey () {
			string s = list [99];
		}

		[Test]
		public void Keys () {
			IList<int> keys = list.Keys;

			Assert.AreEqual (3, keys.Count);
			Assert.AreEqual (0, keys [0]);
			Assert.AreEqual (2, keys [1]);
			Assert.AreEqual (5, keys [2]);

			int[] arr = new int [4];
			keys.CopyTo (arr, 1);
			Assert.AreEqual (0, arr [1]);
			Assert.AreEqual (2, arr [2]);
			Assert.AreEqual (5, arr [3]);

			Assert.AreEqual (true, keys.Contains (2));
			Assert.AreEqual (false, keys.Contains (100));

			Assert.AreEqual (2, keys.IndexOf (5));
			Assert.AreEqual (-1, keys.IndexOf (100));

			int index = 0;
			arr [0] = 0;
			arr [1] = 0;
			arr [2] = 0;
			foreach (int i in keys)
				arr [index ++] = i;
			Assert.AreEqual (0, arr [0]);
			Assert.AreEqual (2, arr [1]);
			Assert.AreEqual (5, arr [2]);
		}

		[Test]
		public void KeysNonGeneric () {
			ICollection keys = ((IDictionary)list).Keys;

			Assert.AreEqual (3, keys.Count);

			int[] arr = new int [4];
			keys.CopyTo (arr, 1);
			Assert.AreEqual (0, arr [1]);
			Assert.AreEqual (2, arr [2]);
			Assert.AreEqual (5, arr [3]);

			int index = 0;
			arr [0] = 0;
			arr [1] = 0;
			arr [2] = 0;
			foreach (int i in keys)
				arr [index ++] = i;
			Assert.AreEqual (0, arr [0]);
			Assert.AreEqual (2, arr [1]);
			Assert.AreEqual (5, arr [2]);
		}

		[Test]
		public void Values () {
			IList<string> values = list.Values;

			Assert.AreEqual (3, values.Count);
			Assert.AreEqual ("A", values [0]);
			Assert.AreEqual ("B", values [1]);
			Assert.AreEqual ("C", values [2]);

			string[] arr = new string [4];
			values.CopyTo (arr, 1);
			Assert.AreEqual ("A", arr [1]);
			Assert.AreEqual ("B", arr [2]);
			Assert.AreEqual ("C", arr [3]);

			Assert.AreEqual (true, values.Contains ("B"));
			Assert.AreEqual (false, values.Contains ("X"));

			Assert.AreEqual (2, values.IndexOf ("C"));
			Assert.AreEqual (-1, values.IndexOf ("X"));

			int index = 0;
			arr [0] = null;
			arr [1] = null;
			arr [2] = null;
			foreach (string s in values)
				arr [index ++] = s;
			Assert.AreEqual ("A", arr [0]);
			Assert.AreEqual ("B", arr [1]);
			Assert.AreEqual ("C", arr [2]);
		}

		[Test]
		public void ValuesNonGeneric () {
			ICollection values = ((IDictionary)list).Values;

			Assert.AreEqual (3, values.Count);

			string[] arr = new string [4];
			values.CopyTo (arr, 1);
			Assert.AreEqual ("A", arr [1]);
			Assert.AreEqual ("B", arr [2]);
			Assert.AreEqual ("C", arr [3]);

			int index = 0;
			arr [0] = null;
			arr [1] = null;
			arr [2] = null;
			foreach (string s in values)
				arr [index ++] = s;
			Assert.AreEqual ("A", arr [0]);
			Assert.AreEqual ("B", arr [1]);
			Assert.AreEqual ("C", arr [2]);
		}

		[Test]
		public void KeysIDictionaryGeneric () {
			ICollection<int> keys = ((IDictionary<int,string>)list).Keys;

			Assert.AreEqual (3, keys.Count);
		}

		[Test]
		public void EmptyKeysCopyToZeroSizedArray ()
		{
			string [] ary = new string [0];
			list2.Keys.CopyTo (ary, 0);
		}

		[Test]
		public void EmptyValuesCopyToZeroSizedArray ()
		{
			int [] ary = new int [0];
			list2.Values.CopyTo (ary, 0);
		}

		[Test]
		public void ValuesIDictionaryGeneric () {
			ICollection<string> values = ((IDictionary<int,string>)list).Values;

			Assert.AreEqual (3, values.Count);
		}

		public void Add () {
			list.Add (10, "D");

			Assert.AreEqual ("D", list [10]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNullKey () {
			list2.Add (null, 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddKeyAlreadyExists () {
			list.Add (10, "B");
			list.Add (10, "C");
		}

		[Test]
		public void ContainsKey () {
			Assert.AreEqual (true, list.ContainsKey (5));
			Assert.AreEqual (false, list.ContainsKey (10));
		}

		[Test]
		public void Remove () {
			Assert.AreEqual (true, list.Remove (5));
			Assert.AreEqual (false, list.Remove (5));
			Assert.AreEqual (false, list.Remove (10));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveNullKey () {
			list2.Remove (null);
		}

		[Test]
		public void GetEnumerator () {
			int[] keys = new int [3];
			string[] values = new string [3];
			int i = 0;
			foreach (KeyValuePair <int, string> kvp in list) {
				keys [i] = kvp.Key;
				values [i] = kvp.Value;
				i ++;
			}

			Assert.AreEqual (0, keys [0]);
			Assert.AreEqual (2, keys [1]);
			Assert.AreEqual (5, keys [2]);
			Assert.AreEqual ("A", values [0]);
			Assert.AreEqual ("B", values [1]);
			Assert.AreEqual ("C", values [2]);
		}

		[Test]
		public void CopyTo ()
		{	
			ICollection<KeyValuePair<int, string>> col1 =
				list as ICollection<KeyValuePair<int, string>>;
			KeyValuePair <int, string> [] array1 =
				new KeyValuePair <int, string> [col1.Count];
			col1.CopyTo (array1, 0);
			Assert.AreEqual (3, array1.Length);
			
			ICollection col = list as ICollection;
			array1 = new KeyValuePair <int, string> [col.Count];
			col.CopyTo (array1, 0);			
			Assert.AreEqual (3, array1.Length);
			
			ICollection<KeyValuePair<string, int>> col2 =
				list2 as ICollection<KeyValuePair<string, int>>;
			KeyValuePair <string, int> [] array2 =
				new KeyValuePair <string, int> [col2.Count];
			col2.CopyTo (array2, 0);
			Assert.AreEqual (0, array2.Length);
			
			col = list2 as ICollection;
			array2 = new KeyValuePair <string, int> [col.Count];
			col.CopyTo (array2, 0);
			Assert.AreEqual (0, array2.Length);			
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void KeyEnumeratorVersionChecking () {
			var en = list.Keys.GetEnumerator();

			int i = 0;
			en.MoveNext ();
			list.Remove (en.Current);
			en.MoveNext ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ValueEnumeratorVersionChecking () {
            var en = list.Values.GetEnumerator();

            int i = 0;
			en.MoveNext ();
			list.RemoveAt (0);
			en.MoveNext ();
		}

		sealed class StartsWithComparator : IComparer<string> {
			private readonly static Comparer<string> _stringComparer = Comparer<string>.Default;
			public static readonly StartsWithComparator Instance = new StartsWithComparator();

			public int Compare(string part, string whole)
			{
				// let the default string comparer deal with null or when part is not smaller then whole
				if (part == null || whole == null || part.Length >= whole.Length)
					return _stringComparer.Compare(part, whole);

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

		sealed class StartsWithComparatorPartWholeCheck : IComparer<string>
		{
			private readonly static Comparer<string> _stringComparer = Comparer<string>.Default;

			public static readonly StartsWithComparator Instance = new StartsWithComparator();

			public int Compare(string part, string whole)
			{
				Assert.IsTrue(part == "Part", "#PWC0");
				Assert.IsTrue(whole == "Whole", "#PWC1");

				// let the default string comparer deal with null or when part is not smaller then whole
				if (part == null || whole == null || part.Length >= whole.Length)
					return _stringComparer.Compare(part, whole);

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
			SortedList<string, string> sl = new SortedList<string, string>(StartsWithComparator.Instance);

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
			SortedList<string, string> sl = new SortedList<string, string>(StartsWithComparatorPartWholeCheck.Instance);
			sl.Add("Part", "Value-Part");
			Assert.IsFalse(sl.ContainsKey("Whole"), "#PWC2");
		}

		[Test]
		public void NonComparatorStringCheck()
		{
			SortedList<string, string> sl = new SortedList<string, string>();

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

			Assert.IsTrue(sl.Keys[0] == "Apples", "NCSC #D1");
			Assert.IsTrue(sl.Keys[1] == "Bananas", "NCSC #D2");
			Assert.IsTrue(sl.Keys[2] == "Oranges", "NCSC #D3");
		}

		[Test]
		public void NonComparatorIntCheck()
		{
			SortedList<int, string> sl = new SortedList<int, string>();

			sl.Add(3, "Value-Oranges");
			sl.Add(2, "Value-Bananas");
			sl.Add(1, "Value-Apples");

			Assert.IsTrue(sl.Count == 3, "NCIC #A0");

			Assert.IsTrue(sl.ContainsKey(1), "NCIC #B1");
			Assert.IsTrue(sl.ContainsKey(2), "NCIC #B2");
			Assert.IsTrue(sl.ContainsKey(3), "NCIC #B3");

			Assert.IsFalse(sl.ContainsKey(11), "NCIC #C1");
			Assert.IsFalse(sl.ContainsKey(22), "NCIC #C2");
			Assert.IsFalse(sl.ContainsKey(33), "NCIC #C3");

			Assert.IsTrue(sl.Keys[0] == 1, "NCIC #D1");
			Assert.IsTrue(sl.Keys[1] == 2, "NCIC #D2");
			Assert.IsTrue(sl.Keys[2] == 3, "NCIC #D3");
		}

		[Test]
		public void ClearDoesNotTouchCapacity ()
		{
			SortedList<int, int> sl = new SortedList<int, int> ();
			for (int i = 0; i < 18; i++) {
				sl.Add (i, i);
			}
			int capacityBeforeClear = sl.Capacity;
			sl.Clear ();
			int capacityAfterClear = sl.Capacity;
			Assert.AreEqual (capacityBeforeClear, capacityAfterClear);
		}

		class Uncomparable : IComparer<double>
		{
			public int Compare (double x, double y)
			{
				throw new DivideByZeroException ();
			}
		}

		[Test]
		// Bug #4327
		public void UncomparableList ()
		{
			var list = new SortedList<double, int> (new Uncomparable ());

			list.Add (Math.PI, 1);

			try {
				list.Add (Math.E, 2);
				Assert.Fail ("UC #1");
			} catch (Exception ex) {
				Assert.That (ex, Is.TypeOf (typeof (InvalidOperationException)), "UC #2");
				Assert.IsNotNull (ex.InnerException, "UC #3");
				Assert.That (ex.InnerException, Is.TypeOf (typeof (DivideByZeroException)), "UC #4");
			}

			try {
				int a;
				list.TryGetValue (Math.E, out a);
				Assert.Fail ("UC #5");
			} catch (Exception ex) {
				Assert.That (ex, Is.TypeOf (typeof (InvalidOperationException)), "UC #6");
				Assert.IsNotNull (ex.InnerException, "UC #7");
				Assert.That (ex.InnerException, Is.TypeOf (typeof (DivideByZeroException)), "UC #8");
			}
		}

		[Test]
		public void IDictionaryNullOnNonExistingKey ()
		{
			IDictionary list = new SortedList<long, string> ();
			object val = list [1234L];
			Assert.IsNull (val);
		}
	}
}

