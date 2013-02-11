//
// MonoTests.System.Collections.Generic.Test.ListTest
//
// Authors:
//      David Waite (mass@akuma.org)
//      Andres G. Aragoneses (andres.aragoneses@7digital.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 David Waite (mass@akuma.org)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
// Copyright 2012 7digital Ltd (http://www.7digital.com).
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
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Collections.Generic {

	class GenericComparer<T> : IComparer<T> {

		private bool called = false;

		public bool Called {
			get {
				bool result = called;
				called = false;
				return called;
			}
		}

		public int Compare (T x, T y)
		{
			called = true;
			return 0;
		}
	}

	[TestFixture]
	public class ListTest
	{
		static byte [] _serializedList = new byte [] {
			0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x00,
			0x7e, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x43, 0x6f, 0x6c,
			0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x73, 0x2e, 0x47, 0x65,
			0x6e, 0x65, 0x72, 0x69, 0x63, 0x2e, 0x4c, 0x69, 0x73, 0x74, 0x60,
			0x31, 0x5b, 0x5b, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x49,
			0x6e, 0x74, 0x33, 0x32, 0x2c, 0x20, 0x6d, 0x73, 0x63, 0x6f, 0x72,
			0x6c, 0x69, 0x62, 0x2c, 0x20, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6f,
			0x6e, 0x3d, 0x32, 0x2e, 0x30, 0x2e, 0x30, 0x2e, 0x30, 0x2c, 0x20,
			0x43, 0x75, 0x6c, 0x74, 0x75, 0x72, 0x65, 0x3d, 0x6e, 0x65, 0x75,
			0x74, 0x72, 0x61, 0x6c, 0x2c, 0x20, 0x50, 0x75, 0x62, 0x6c, 0x69,
			0x63, 0x4b, 0x65, 0x79, 0x54, 0x6f, 0x6b, 0x65, 0x6e, 0x3d, 0x62,
			0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36, 0x31, 0x39, 0x33, 0x34,
			0x65, 0x30, 0x38, 0x39, 0x5d, 0x5d, 0x03, 0x00, 0x00, 0x00, 0x06,
			0x5f, 0x69, 0x74, 0x65, 0x6d, 0x73, 0x05, 0x5f, 0x73, 0x69, 0x7a,
			0x65, 0x08, 0x5f, 0x76, 0x65, 0x72, 0x73, 0x69, 0x6f, 0x6e, 0x07,
			0x00, 0x00, 0x08, 0x08, 0x08, 0x09, 0x02, 0x00, 0x00, 0x00, 0x03,
			0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x0f, 0x02, 0x00, 0x00,
			0x00, 0x04, 0x00, 0x00, 0x00, 0x08, 0x05, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x0b };
		int [] _list1_contents;
		List <int> _list1;

		[SetUp]
		public void SetUp ()
		{
			// FIXME arrays currently do not support generic collection
			// interfaces
			_list1_contents = new int [] { 55, 50, 22, 80, 56, 52, 40, 63 };
			// _list1 = new List <int> (_list1_contents);
			
			_list1 = new List <int> (8);
			foreach (int i in _list1_contents)
				_list1.Add (i);
		}

		[Test]  // This was for bug #74980
		public void InsertTest ()
		{
			List <string> test = new List <string> ();
			test.Insert (0, "a");
			test.Insert (0, "b");
			test.Insert (1, "c");

			Assert.AreEqual (3, test.Count);
			Assert.AreEqual ("b", test [0]);
			Assert.AreEqual ("c", test [1]);
			Assert.AreEqual ("a", test [2]);
		}

		[Test]
		public void InsertRangeTest ()
		{
			int count = _list1.Count;
			// FIXME arrays currently do not support generic collection 
			// interfaces
			int [] items = {1, 2, 3};
			// List <int> newRange = new List <int> (items);
			List <int> newRange = new List <int> (3);
			foreach (int i in items)
				newRange.Add (i);
			_list1.InsertRange (1, newRange);
			Assert.AreEqual (count + 3, _list1.Count);
			Assert.AreEqual (55, _list1 [0]);
			Assert.AreEqual (1, _list1 [1]);
			Assert.AreEqual (2, _list1 [2]);
			Assert.AreEqual (3, _list1 [3]);
			Assert.AreEqual (50, _list1 [4]);

			newRange = new List <int> ();
			List <int> li = new List <int> ();
			li.Add (1);
			newRange.InsertRange (0, li);
			newRange.InsertRange (newRange.Count, li);
			Assert.AreEqual (2, newRange.Count);
		}
		
		[Test]
		public void InsertSelfTest()
		{
			List <int> range = new List <int> (5);
			for (int i = 0; i < 5; ++ i)
				range.Add (i);
			
			range.InsertRange(2, range);
			Assert.AreEqual (10, range.Count);
			Assert.AreEqual (0, range [0]);
			Assert.AreEqual (1, range [1]);
			Assert.AreEqual (0, range [2]);
			Assert.AreEqual (1, range [3]);
			Assert.AreEqual (2, range [4]);
			Assert.AreEqual (3, range [5]);
			Assert.AreEqual (4, range [6]);
			Assert.AreEqual (2, range [7]);
			Assert.AreEqual (3, range [8]);
			Assert.AreEqual (4, range [9]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void InsertRangeNullTest ()
		{
			IEnumerable <int> n = null;
			_list1.InsertRange (0, n);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InsertRangeNegativeIndexTest ()
		{
			_list1.InsertRange (-1, _list1);
		}

		[Test]
		public void IndexOfTest ()
		{
			List <int> l = new List <int> ();

			l.Add (100);
			l.Add (200);

			Assert.AreEqual (1, l.IndexOf (200), "Could not find value");
		}
		
		[Test, ExpectedException(typeof (ArgumentException))]
		public void IList_InsertInvalidType1 ()
		{
			IList list = _list1 as IList;
			list.Insert(0, new object());
		}

		[Test, ExpectedException(typeof (ArgumentException))]
		public void IList_InsertInvalidType2 ()
		{
			IList list = _list1 as IList;
			list.Insert(0, null);
		}
		
		[Test, ExpectedException(typeof (ArgumentException))]
		public void IList_AddInvalidType1()
		{
			IList list = _list1 as IList;
			list.Add(new object());
		}

		[Test, ExpectedException(typeof (ArgumentException))]
		public void IList_AddInvalidType2()
		{
			IList list = _list1 as IList;
			list.Add(null);
		}
		
		[Test]
		public void IList_RemoveInvalidType()
		{
			IList list = _list1 as IList;
			int nCount = list.Count;
			list.Remove(new object());
			Assert.AreEqual(nCount, list.Count);

			list.Remove(null);
			Assert.AreEqual(nCount, list.Count);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IndexOfOutOfRangeTest ()
		{
			List <int> l = new List <int> (4);
			l.IndexOf (0, 0, 4);
		}

		[Test]
		public void GetRangeTest ()
		{
			List <int> r = _list1.GetRange (2, 4);
			Assert.AreEqual (4, r.Count);
			Assert.AreEqual (22, r [0]);
			Assert.AreEqual (80, r [1]);
			Assert.AreEqual (56, r [2]);
			Assert.AreEqual (52, r [3]);
		}

		[Test]
		public void EnumeratorTest ()
		{
			List <int>.Enumerator e = _list1.GetEnumerator ();
			for (int i = 0; i < _list1_contents.Length; i++)
			{
				Assert.IsTrue (e.MoveNext ());
				Assert.AreEqual (_list1_contents [i], e.Current);
			}
			Assert.IsFalse (e.MoveNext ());
		}

		[Test]
		public void ConstructWithSizeTest ()
		{
			List <object> l_1 = new List <object> (1);
			List <object> l_2 = new List <object> (50);
			List <object> l_3 = new List <object> (0);

			Assert.AreEqual (1, l_1.Capacity);
			Assert.AreEqual (50, l_2.Capacity);
			Assert.AreEqual (0, l_3.Capacity);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructWithInvalidSizeTest ()
		{
			List <int> l = new List <int> (-1);
		}

		[Test]
		public void ConstructWithCollectionTest ()
		{
			List <int> l1 = new List <int> (_list1);
			Assert.AreEqual (_list1.Count, l1.Count);
			Assert.AreEqual (l1.Count, l1.Capacity);
			for (int i = 0; i < l1.Count; i++)
				Assert.AreEqual (_list1 [i], l1 [i]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ConstructWithInvalidCollectionTest ()
		{
			List <int> n = null;
			List <int> l1 = new List <int> (n);
		}

		[Test]
		public void AddTest ()
		{
			int count = _list1.Count;
			_list1.Add (-1);
			Assert.AreEqual (count + 1, _list1.Count);
			Assert.AreEqual (-1, _list1 [_list1.Count - 1]);
		}

		[Test]
		public void AddRangeTest ()
		{
			int count = _list1.Count;
			// FIXME arrays currently do not support generic collection
			// interfaces
			int [] range = { -1, -2, -3 };
			List <int> tmp = new List <int> (3);
			foreach (int i in range)
				tmp.Add (i);
			// _list1.AddRange (range);
			_list1.AddRange (tmp);
			
			Assert.AreEqual (count + 3, _list1.Count);
			Assert.AreEqual (-1, _list1 [_list1.Count - 3]);
			Assert.AreEqual (-2, _list1 [_list1.Count - 2]);
			Assert.AreEqual (-3, _list1 [_list1.Count - 1]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void AddNullRangeTest ()
		{
			int [] n = null;
			_list1.AddRange (n);
		}

		[Test]
		public void BinarySearchTest ()
		{
			List <int> l = new List <int> (_list1);
			l.Sort ();
			Assert.AreEqual (0, l.BinarySearch (22));
			Assert.AreEqual (-2, l.BinarySearch (23));
			Assert.AreEqual (- (l.Count + 1), l.BinarySearch (int.MaxValue));
		}

#if !NET_4_0 // FIXME: the blob contains the 2.0 mscorlib version

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void SerializeTest ()
		{
			List <int> list = new List <int> ();
			list.Add (5);
			list.Add (0);
			list.Add (7);

#if TARGET_JVM
			BinaryFormatter bf = (BinaryFormatter)vmw.@internal.remoting.BinaryFormatterUtils.CreateBinaryFormatter (false);
#else
			BinaryFormatter bf = new BinaryFormatter ();
#endif // TARGET_JVM
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, list);

			byte [] buffer = new byte [ms.Length];
			ms.Position = 0;
			ms.Read (buffer, 0, buffer.Length);

			Assert.AreEqual (_serializedList, buffer);
		}

#endif

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void DeserializeTest ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Write (_serializedList, 0, _serializedList.Length);
			ms.Position = 0;

#if TARGET_JVM
			BinaryFormatter bf = (BinaryFormatter)vmw.@internal.remoting.BinaryFormatterUtils.CreateBinaryFormatter (false);
#else
			BinaryFormatter bf = new BinaryFormatter ();
#endif // TARGET_JVM
			List<int> list = (List<int>) bf.Deserialize (ms);
			Assert.AreEqual (3, list.Count, "#1");
			Assert.AreEqual (5, list [0], "#2");
			Assert.AreEqual (0, list [1], "#3");
			Assert.AreEqual (7, list [2], "#4");
		}

		[Test]
		public void SortTest ()
		{
			List <int> l = new List <int> (_list1);
			l.Sort ();
			Assert.AreEqual (_list1.Count, l.Count);
			Assert.AreEqual (22, l [0]);
			int minimum = 22;
			foreach (int i in l)
			{
				Assert.IsTrue (minimum <= i);
				minimum = i;
			}
		}

		[Test]
		public void ClearTest ()
		{
			int capacity = _list1.Capacity;
			_list1.Clear ();
			Assert.AreEqual (0, _list1.Count);
			Assert.AreEqual (capacity, _list1.Capacity);
		}

		[Test]
		public void ContainsTest ()
		{
			Assert.IsTrue (_list1.Contains (22));
			Assert.IsFalse (_list1.Contains (23));
		}

		private string StringConvert (int i)
		{
			return i.ToString ();
		}
		
		[Test]
		public void ConvertAllTest ()
		{
			List <string> s = _list1.ConvertAll ( (Converter <int, string>)StringConvert);
			Assert.AreEqual (_list1.Count, s.Count);
			Assert.AreEqual ("55", s [0]);
		}

		[Test]
		public void CopyToTest ()
		{
			int [] a = new int [2];
			_list1.CopyTo (1, a, 0, 2);
			Assert.AreEqual (50, a [0]);
			Assert.AreEqual (22, a [1]);

			int [] b = new int [_list1.Count + 1];
			b [_list1.Count] = 555;
			_list1.CopyTo (b);
			Assert.AreEqual (55, b [0]);
			Assert.AreEqual (555, b [_list1.Count]);

			b [0] = 888;
			_list1.CopyTo (b, 1);
			Assert.AreEqual (888, b [0]);
			Assert.AreEqual (55, b [1]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void CopyToNullTest ()
		{
			int [] a = null;
			_list1.CopyTo (0, a, 0, 0);
		}

		static bool FindMultipleOfThree (int i)
		{
			return (i % 3) == 0;
		}

		static bool FindMultipleOfFour (int i)
		{
			return (i % 4) == 0;
		}

		static bool FindMultipleOfTwelve (int i)
		{
			return (i % 12) == 0;
		}

		[Test]
		public void FindTest ()
		{
			int i = _list1.Find (FindMultipleOfThree);
			Assert.AreEqual (63, i);

			i = _list1.Find (FindMultipleOfTwelve);
			Assert.AreEqual (default (int), i);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void FindNullTest ()
		{
			int i = _list1.Find (null);
		}

		[Test]
		public void FindAllSmallTest ()
		{
			List <int> findings = _list1.FindAll (FindMultipleOfFour);
			Assert.AreEqual (4, findings.Count);
			Assert.AreEqual (80, findings [0]);
			Assert.AreEqual (56, findings [1]);
			Assert.AreEqual (52, findings [2]);
			Assert.AreEqual (40, findings [3]);

			findings = _list1.FindAll (FindMultipleOfTwelve);
			Assert.IsNotNull (findings);
			Assert.AreEqual (0, findings.Count);
		}
		
		[Test]
		public void FindAllMediumTest ()
		{
			List <int> integers = new List <int> (10000);
			for (int i = 1; i <= 10000; i++)
				integers.Add (i);
			
			List <int> results = integers.FindAll (FindMultipleOfFour);
			
			Assert.IsNotNull (results);
			Assert.AreEqual (2500, results.Count);
			
			results = integers.FindAll (FindMultipleOfTwelve);
			
			Assert.IsNotNull (results);
			Assert.AreEqual (833, results.Count);
		}
		
		[Test]
		public void FindAllLargeTest ()
		{
			List <int> integers = new List <int> (70000);
			for (int i = 1; i <= 80000; i++)
				integers.Add (i);
			
			List <int> results = integers.FindAll (FindMultipleOfFour);
			
			Assert.IsNotNull (results);
			Assert.AreEqual (20000, results.Count);
			
			results = integers.FindAll (FindMultipleOfTwelve);
			
			Assert.IsNotNull (results);
			Assert.AreEqual (6666, results.Count);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void FindAllNullTest ()
		{
			List <int> findings = _list1.FindAll (null);
		}

		[Test]
		public void FindIndexTest ()
		{
			int i = _list1.FindIndex (FindMultipleOfThree);
			Assert.AreEqual (7, i);

			i = _list1.FindIndex (FindMultipleOfTwelve);
			Assert.AreEqual (-1, i);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void FindIndexNullTest ()
		{
			int i = _list1.FindIndex (null);
		}

		[Test]
		public void FindLastTest ()
		{
			int i = _list1.FindLast (FindMultipleOfFour);
			Assert.AreEqual (40, i);

			i = _list1.FindLast (FindMultipleOfTwelve);
			Assert.AreEqual (default (int), i);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void FindLastNullTest ()
		{
			int i = _list1.FindLast (null);
		}

		// FIXME currently generates Invalid IL Code error
		/*
		[Test]
		public void ForEachTest ()
		{
			int i = 0;
			_list1.ForEach (delegate (int j) { i += j; });

			Assert.AreEqual (418, i);
		}
		*/
		[Test]
		public void FindLastIndexTest ()
		{
			int i = _list1.FindLastIndex (FindMultipleOfFour);
			Assert.AreEqual (6, i);

			i = _list1.FindLastIndex (5, FindMultipleOfFour);
			Assert.AreEqual (5, i);

			i = _list1.FindIndex (FindMultipleOfTwelve);
			Assert.AreEqual (-1, i);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void FindLastIndexNullTest ()
		{
			int i = _list1.FindLastIndex (null);
		}

		[Test]
		public void RemoveTest ()
		{
			int count = _list1.Count;
			bool result = _list1.Remove (22);
			Assert.IsTrue (result);
			Assert.AreEqual (count - 1, _list1.Count);

			Assert.AreEqual (-1, _list1.IndexOf (22));

			result = _list1.Remove (0);
			Assert.IsFalse (result);
		}

		[Test]
		public void RemoveAllTest ()
		{
			int count = _list1.Count;
			int removedCount = _list1.RemoveAll (FindMultipleOfFour);
			Assert.AreEqual (4, removedCount);
			Assert.AreEqual (count - 4, _list1.Count);

			removedCount = _list1.RemoveAll (FindMultipleOfTwelve);
			Assert.AreEqual (0, removedCount);
			Assert.AreEqual (count - 4, _list1.Count);
		}

		[Test]
		public void RemoveAtTest ()
		{
			int count = _list1.Count;
			_list1.RemoveAt (0);
			Assert.AreEqual (count - 1, _list1.Count);
			Assert.AreEqual (50, _list1 [0]);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RemoveOutOfRangeTest ()
		{
			_list1.RemoveAt (_list1.Count);
		}

		[Test]
		public void RemoveRangeTest ()
		{
			int count = _list1.Count;
			_list1.RemoveRange (1, 2);
			Assert.AreEqual (count - 2, _list1.Count);
			Assert.AreEqual (55, _list1 [0]);
			Assert.AreEqual (80, _list1 [1]);

			_list1.RemoveRange (0, 0);
			Assert.AreEqual (count - 2, _list1.Count);
		}

		[Test]
		public void RemoveRangeFromEmptyListTest ()
		{
			List<int> l = new List<int> ();
			l.RemoveRange (0, 0);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void RemoveRangeOutOfRangeTest ()
		{
			_list1.RemoveRange (1, _list1.Count);
		}

		[Test]
		public void ReverseTest ()
		{
			int count = _list1.Count;
			_list1.Reverse ();
			Assert.AreEqual (count, _list1.Count);

			Assert.AreEqual (63, _list1 [0]);
			Assert.AreEqual (55, _list1 [count - 1]);

			_list1.Reverse (0, 2);

			Assert.AreEqual (40, _list1 [0]);
			Assert.AreEqual (63, _list1 [1]);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ReverseOutOfRangeTest ()
		{
			_list1.Reverse (1, _list1.Count);
		}

		[Test]
		public void ToArrayTest ()
		{
			int [] copiedContents = _list1.ToArray ();
			Assert.IsFalse (ReferenceEquals (copiedContents, _list1_contents));

			Assert.AreEqual (_list1.Count, copiedContents.Length);
			Assert.AreEqual (_list1 [0], copiedContents [0]);
		}

		[Test]
		public void TrimExcessTest ()
		{
			List <string> l = new List <string> ();
			l.Add ("foo");

			Assert.IsTrue (l.Count < l.Capacity);
			l.TrimExcess ();
			Assert.AreEqual (l.Count, l.Capacity);
		}

		bool IsPositive (int i)
		{
			return i >= 0;
		}

		[Test]
		public void TrueForAllTest ()
		{
			Assert.IsFalse (_list1.TrueForAll (FindMultipleOfFour));
			Assert.IsTrue (_list1.TrueForAll (IsPositive));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void TrueForAllNullTest ()
		{
			_list1.TrueForAll (null);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CapacityOutOfRangeTest ()
		{
			_list1.Capacity = _list1.Count - 1;
		}

		[Test]
		public void BinarySearch_EmptyList ()
		{
			GenericComparer<int> comparer = new GenericComparer<int> ();
			List<int> l = new List<int> ();
			Assert.AreEqual (-1, l.BinarySearch (0, comparer), "BinarySearch");
			// bug 77030 - the comparer isn't called for an empty array/list
			Assert.IsFalse (comparer.Called, "Called");
		}

		[Test]
		public void BinarySearch2_EmptyList ()
		{
			GenericComparer<int> comparer = new GenericComparer<int> ();
			List<int> l = new List<int> ();
			Assert.AreEqual (-1, l.BinarySearch (0, 0, 0, comparer), "BinarySearch");
			// bug 77030 - the comparer isn't called for an empty array/list
			Assert.IsFalse (comparer.Called, "Called");
		}

		[Test]
		public void AddRange_Bug77019 ()
		{
			List<int> l = new List<int> ();
			Dictionary<string, int> d = new Dictionary<string, int> ();
			l.AddRange (d.Values);
			Assert.AreEqual (0, l.Count, "Count");
		}

		[Test]
		public void VersionCheck_Add ()
		{
			List<int> list = new List<int> ();
			IEnumerator enumerator = list.GetEnumerator ();
			list.Add (5);

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			enumerator.MoveNext ();
		}

		[Test]
		public void VersionCheck_AddRange ()
		{
			List<int> list = new List<int> ();
			IEnumerator enumerator = list.GetEnumerator ();
			list.AddRange (new int [] { 5, 7 });

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			enumerator.MoveNext ();
		}

		[Test]
		public void VersionCheck_Clear ()
		{
			List<int> list = new List<int> ();
			IEnumerator enumerator = list.GetEnumerator ();
			list.Clear ();

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			enumerator.MoveNext ();
		}

		[Test]
		public void VersionCheck_Insert ()
		{
			List<int> list = new List<int> ();
			IEnumerator enumerator = list.GetEnumerator ();
			list.Insert (0, 7);

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			enumerator.MoveNext ();
		}

		[Test]
		public void VersionCheck_InsertRange ()
		{
			List<int> list = new List<int> ();
			IEnumerator enumerator = list.GetEnumerator ();
			list.InsertRange (0, new int [] { 5, 7 });

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			enumerator.MoveNext ();
		}

		[Test]
		public void VersionCheck_Remove ()
		{
			List<int> list = new List<int> ();
			list.Add (5);
			IEnumerator enumerator = list.GetEnumerator ();
			// version number is not incremented if item does not exist in list
			list.Remove (7);
			enumerator.MoveNext ();
			list.Remove (5);

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			enumerator.MoveNext ();
		}

		[Test]
		public void VersionCheck_RemoveAll ()
		{
			List<int> list = new List<int> ();
			list.Add (5);
			IEnumerator enumerator = list.GetEnumerator ();
			// version is not incremented if there are no items to remove
			list.RemoveAll (FindMultipleOfFour);
			enumerator.MoveNext ();
			list.Add (4);

			enumerator = list.GetEnumerator ();
			list.RemoveAll (FindMultipleOfFour);

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			enumerator.MoveNext ();
		}

		[Test]
		public void VersionCheck_RemoveAt ()
		{
			List<int> list = new List<int> ();
			list.Add (5);
			IEnumerator enumerator = list.GetEnumerator ();
			list.RemoveAt (0);

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			enumerator.MoveNext ();
		}

		[Test]
		public void VersionCheck_RemoveRange ()
		{
			List<int> list = new List<int> ();
			list.Add (5);
			IEnumerator enumerator = list.GetEnumerator ();
			// version is not incremented if count is zero
			list.RemoveRange (0, 0);
			enumerator.MoveNext ();
			enumerator.Reset ();
			list.RemoveRange (0, 1);

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			enumerator.MoveNext ();
		}

		[Test, ExpectedException (typeof (InvalidOperationException))] // #699182
		public void VersionCheck_Indexer ()
		{
			var list = new List<int> () { 0, 2, 3 };
			var enumerator = list.GetEnumerator ();

			list [0] = 1;

			enumerator.MoveNext ();
		}

		[Test]
		public void VersionCheck_Reverse ()
		{
			List<int> list = new List<int> ();
			IEnumerator enumerator = list.GetEnumerator ();
			list.Reverse ();

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#A2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			list.Reverse (0, 0);

			try {
				enumerator.MoveNext ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException) {
			}

			try {
				enumerator.Reset ();
				Assert.Fail ("#B2");
			} catch (InvalidOperationException) {
			}

			enumerator = list.GetEnumerator ();
			enumerator.MoveNext ();
		}

		class SortTestComparer: IComparer<string> {

			public int Compare (string s1, string s2)
			{
				return String.Compare (s1, s2);
			}
		}

		[Test]
		public void Sort_Bug76361 ()
		{
			SortTestComparer comparer = new SortTestComparer ();
			List<string> l = new List<string> ();
			l.Add ("foo");
			l.Add ("bar");
			l.Sort (comparer);
			Assert.AreEqual ("bar", l[0], "0");
			Assert.AreEqual ("foo", l[1], "1");
			Assert.AreEqual (2, l.Count, "Count");
		}

		// for bug #77039 test case
		class GenericIComparable: IComparable<GenericIComparable> {
			private int _NumberToSortOn;

			public int NumberToSortOn {
				get { return _NumberToSortOn; }
				set { _NumberToSortOn = value; }
			}

			public GenericIComparable (int val)
			{
				_NumberToSortOn = val;
			}

			public int CompareTo (GenericIComparable other)
			{
				return NumberToSortOn.CompareTo (other.NumberToSortOn);
			}
		}

		[Test]
		public void Sort_GenericIComparable_Bug77039 ()
		{
			List<GenericIComparable> l = new List<GenericIComparable> ();
			l.Add (new GenericIComparable (2));
			l.Add (new GenericIComparable (1));
			l.Add (new GenericIComparable (3));
			l.Sort ();
			Assert.AreEqual (1, l[0].NumberToSortOn, "0");
			Assert.AreEqual (2, l[1].NumberToSortOn, "1");
			Assert.AreEqual (3, l[2].NumberToSortOn, "2");
		}

		class NonGenericIComparable: IComparable {
			private int _NumberToSortOn;

			public int NumberToSortOn {
				get { return _NumberToSortOn; }
				set { _NumberToSortOn = value; }
			}

			public NonGenericIComparable (int val)
			{
				_NumberToSortOn = val;
			}

			public int CompareTo (object obj)
			{
				return NumberToSortOn.CompareTo ((obj as NonGenericIComparable).NumberToSortOn);
			}
		}

		[Test]
		public void Sort_NonGenericIComparable ()
		{
			List<NonGenericIComparable> l = new List<NonGenericIComparable> ();
			l.Add (new NonGenericIComparable (2));
			l.Add (new NonGenericIComparable (1));
			l.Add (new NonGenericIComparable (3));
			l.Sort ();
			Assert.AreEqual (1, l[0].NumberToSortOn, "0");
			Assert.AreEqual (2, l[1].NumberToSortOn, "1");
			Assert.AreEqual (3, l[2].NumberToSortOn, "2");
		}

		class NonComparable {
		}

		[Test]
		public void Sort_GenericNonIComparable ()
		{
			List<NonComparable> l = new List<NonComparable> ();
			l.Sort ();
			// no element -> no sort -> no exception
			l.Add (new NonComparable ());
			l.Sort ();
			// one element -> no sort -> no exception
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Sort_GenericNonIComparable_2 ()
		{
			List<NonComparable> l = new List<NonComparable> ();
			l.Add (new NonComparable ());
			l.Add (new NonComparable ());
			l.Sort ();
			// two element -> sort -> exception!
		}
		
		[Test]
		public void IList_Contains_InvalidType()
		{
			List<string> list = new List<string>();
			list.Add("foo");
			Assert.IsFalse (((IList)list).Contains(new object()));

			Assert.IsFalse (((IList)_list1).Contains(null));
		}
		
		[Test]
		public void IList_IndexOf_InvalidType()
		{
			List<string> list = new List<string>();
			list.Add("foo");
			Assert.AreEqual (-1, ((IList)list).IndexOf(new object()));

			Assert.AreEqual (-1, ((IList)_list1).IndexOf(null));
		}

		// for bug #77277 test case
		[Test]
		public void Test_ContainsAndIndexOf_EquatableItem ()
		{
			List<EquatableClass> list = new List<EquatableClass> ();
			EquatableClass item0 = new EquatableClass (0);
			EquatableClass item1 = new EquatableClass (1);

			list.Add (item0);
			list.Add (item1);
			list.Add (item0);

			Assert.AreEqual (true, list.Contains (item0), "#0");
			Assert.AreEqual (true, list.Contains (new EquatableClass (0)), "#1");
			Assert.AreEqual (0, list.IndexOf (item0), "#2");
			Assert.AreEqual (0, list.IndexOf (new EquatableClass (0)), "#3");
			Assert.AreEqual (2, list.LastIndexOf (item0), "#4");
			Assert.AreEqual (2, list.LastIndexOf (new EquatableClass (0)), "#5");
		}

		// for bug #81387 test case
		[Test]
		public void Test_Contains_After_Remove ()
		{
			List<int> list = new List<int> ();
            list.Add (2);

            list.Remove (2);

			Assert.AreEqual (false, list.Contains (2), "#0");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetItem_OutOfRange()
		{
			List<string> list = new List<string>();
			list[0] = "foo";
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetItem_IList_OutOfRange()
		{
			IList<string> list = new List<string>();
			list[0] = "foo";
		}

		public class EquatableClass : IEquatable<EquatableClass>
		{
			int _x;
			public EquatableClass (int x)
			{
				_x = x;
			}

			public bool Equals (EquatableClass other)
			{
				return this._x == other._x;
			}
		}

		delegate void D ();
		bool Throws (D d)
		{
			try {
				d ();
				return false;
			} catch {
				return true;
			}
		}

		[Test]
		// based on #491858, #517415
		public void Enumerator_Current ()
		{
			var e1 = new List<int>.Enumerator ();
			Assert.IsFalse (Throws (delegate { var x = e1.Current; }));

			var d = new List<int> ();
			var e2 = d.GetEnumerator ();
			Assert.IsFalse (Throws (delegate { var x = e2.Current; }));
			e2.MoveNext ();
			Assert.IsFalse (Throws (delegate { var x = e2.Current; }));
			e2.Dispose ();
			Assert.IsFalse (Throws (delegate { var x = e2.Current; }));

			var e3 = ((IEnumerable<int>) d).GetEnumerator ();
			Assert.IsFalse (Throws (delegate { var x = e3.Current; }));
			e3.MoveNext ();
			Assert.IsFalse (Throws (delegate { var x = e3.Current; }));
			e3.Dispose ();
			Assert.IsFalse (Throws (delegate { var x = e3.Current; }));

			var e4 = ((IEnumerable) d).GetEnumerator ();
			Assert.IsTrue (Throws (delegate { var x = e4.Current; }));
			e4.MoveNext ();
			Assert.IsTrue (Throws (delegate { var x = e4.Current; }));
			((IDisposable) e4).Dispose ();
			Assert.IsTrue (Throws (delegate { var x = e4.Current; }));
		}

		[Test] //bug #672907
		public void ICollectionCopyToExceptions ()
		{
			var l = new List <int> ();
			ICollection x = l;
			try {
				x.CopyTo (null, 0);
				Assert.Fail ("#1");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentNullException, "#2");
			}

			try {
				x.CopyTo (new int [10], -1);
				Assert.Fail ("#3");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentOutOfRangeException, "#4");
			}

			try {
				x.CopyTo (new int [10, 1], 0);
				Assert.Fail ("#5");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "#6");
			}

			try {
				x.CopyTo (Array.CreateInstance (typeof (int), new int [] { 10 }, new int[] { 1 }), 0);
				Assert.Fail ("#7");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "#8");
			}

			l.Add (10); l.Add (20);
			try {
				x.CopyTo (new int [1], 0);
				Assert.Fail ("#9");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "#10");
			}
		}

		[Test]
		public void LastIndexOfEmpty_2558 () {
			var l = new List<int> ();
			Assert.AreEqual (-1, l.IndexOf (-1));
		}


#region Enumerator mutability

		class Bar
		{
		}

		class Foo : IEnumerable<Bar>
		{
			Baz enumerator;

			public Foo ()
			{
				enumerator = new Baz ();
			}

			public IEnumerator<Bar> GetEnumerator ()
			{
				return enumerator;
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return enumerator;
			}
		}

		class Baz : IEnumerator<Bar>
		{
			public bool DisposeWasCalled = false;

			public void Dispose ()
			{
				DisposeWasCalled = true;
			}

			public bool MoveNext ()
			{
				return false; //assume empty collection
			}

			public void Reset ()
			{
			}

			public Bar Current
			{
				get { return null; }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}
		}

		[Test]
		public void PremiseAboutDisposeBeingCalledWhenLooping ()
		{
			Foo enumerable = new Foo ();
			Baz enumerator = enumerable.GetEnumerator () as Baz;
			Assert.IsNotNull (enumerator);
			Assert.AreEqual (false, enumerator.DisposeWasCalled);
			foreach (var element in enumerable) ; //sic
			Assert.AreEqual (true, enumerator.DisposeWasCalled);
		}

		[Test]
		public void TwoEnumeratorsOfTwoDifferentListsAreDifferent ()
		{
			var twoThree = new List<int> { 2, 3 };
			var oneTwo = new List<int> { 2, 4 };
			Assert.IsFalse (oneTwo.GetEnumerator ().Equals (twoThree.GetEnumerator ()));
		}

		[Test]
		public void TwoEnumeratorsOfTwoDifferentListsWithSameElementsAreDifferent ()
		{
			var twoThree = new List<int> { 2, 3 };
			var anotherTwoThree = new List<int> { 2, 3 };
			Assert.IsFalse(twoThree.GetEnumerator ().Equals (anotherTwoThree.GetEnumerator ()));
		}

		[Test]
		public void EnumeratorIsSameInSameListAfterSubsequentCalls ()
		{
			var enumerable = new List<Bar> ();
			var enumerator = enumerable.GetEnumerator ();
			var enumerator2 = enumerable.GetEnumerator ();

			Assert.IsFalse (ReferenceEquals (enumerator2, enumerator)); //because they are value-types

			Assert.IsTrue (enumerator2.Equals (enumerator));
		}


		[Test] // was bug in Mono 2.10.9
		public void EnumeratorIsStillSameInSubsequentCallsEvenHavingADisposalInBetween ()
		{
			var enumerable = new List<Bar> ();
			var enumerator = enumerable.GetEnumerator ();
			enumerator.Dispose ();
			var enumerator2 = enumerable.GetEnumerator ();

			Assert.IsFalse (ReferenceEquals (enumerator2, enumerator)); //because they are value-types

			Assert.IsTrue (enumerator2.Equals (enumerator));
		}

		[Test]
		public void EnumeratorIsObviouslyDifferentAfterListChanges ()
		{
			var enumerable = new List<Bar> ();
			var enumerator = enumerable.GetEnumerator ();
			enumerable.Add (new Bar ());
			var enumerator2 = enumerable.GetEnumerator ();

			Assert.IsFalse (ReferenceEquals (enumerator2, enumerator)); //because they are value-types

			Assert.IsFalse (enumerator2.Equals (enumerator));
		}

		[Test] // was bug in Mono 2.10.9
		public void DotNetDoesntThrowObjectDisposedExceptionAfterSubsequentDisposes()
		{
			var enumerable = new List<Bar> ();
			var enumerator = enumerable.GetEnumerator ();
			Assert.AreEqual (false, enumerator.MoveNext ());
			enumerator.Dispose();
			Assert.AreEqual (false, enumerator.MoveNext ());
		}
#endregion


	}
}
#endif

