// 
// System.Collections.SortedListTest.cs
// 
// Author:
//   Zoltan Varga (vargaz@gmail.com)
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

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
		
	}
}

#endif
