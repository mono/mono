// NewArrayListTest.cs
// 
// Unit tests for System.Collections.ArrayList
//
// Copyright (c) 2003 Thong (Tum) Nguyen [tum@veridicus.com]
//
// Released under the MIT License:
//
// http://www.opensource.org/licenses/mit-license.html
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the 
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//
// Author's comment: Source code formatting has been changed by request to match 
// Mono's formatting style.  I personally use BSD-style formatting.
// 

using System;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Collections 
{
	/// <summary>
	/// Some test cases for the new ArrayList implementation.
	/// </summary>
	[TestFixture]
	public class NewArrayListTest
		: TestCase 
	{
		private object[] c_TestData = new Object[] {0,1,2,3,4,5,6,7,8,9};

		private void VerifyContains(IList list, IList values, string message) 
		{
			if (values.Count != list.Count) 
			{
				Assertion.Assert(message, false);
			}

			for (int i = 0; i < list.Count; i++) 
			{
				if (list[i] == null && values[i] == null) 
				{
					continue;
				}

				if ((list[i] == null || values[i] == null) || !list[i].Equals(values[i])) 
				{
					Assertion.Assert(message, false);
				}
			}
		}

		private void PrivateTestSort(ArrayList arrayList) 
		{	
			Random random = new Random(1027);

			// Sort arrays of lengths up to 200

			for (int i = 1; i < 200; i++) 
			{
				for (int j = 0; j < i; j++) 
				{
					arrayList.Add(random.Next(0, 1000));
				}

				arrayList.Sort();

				for (int j = 1; j < i; j++) 
				{
					if ((int)arrayList[j] < (int)arrayList[j - 1]) 
					{
						Assertion.Fail("ArrayList.Sort()");

						return;
					}
				}

				arrayList.Clear();
			}
		}

		[Test]
		public void TestSortStandard() 
		{
			PrivateTestSort(new ArrayList());
		}

		[Test]
		public void TestSortSynchronized() 
		{
			PrivateTestSort(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestSortAdapter() 
		{
			PrivateTestSort(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestSortGetRange() 
		{
			PrivateTestSort(new ArrayList().GetRange(0, 0));
		}

		private void PrivateTestIndexOf(ArrayList arrayList) 
		{
			int x;
			
			arrayList.AddRange(c_TestData);

			for (int i = 0; i < 10; i++) 
			{			
				x = arrayList.IndexOf(i);
				Assertion.Assert("ArrayList.IndexOf(" + i + ")", x == i);
			}

			try 
			{
				arrayList.IndexOf(0, 10, 1);
				Assertion.Fail("ArrayList.IndexOf(0, 10, 1)");
			}
			catch (ArgumentOutOfRangeException) 
			{
			}

			try 
			{
				arrayList.IndexOf(0, 0, -1);
				Assertion.Fail("ArrayList.IndexOf(0, 10, 1)");
			}
			catch (ArgumentOutOfRangeException) 
			{
			}

			try 
			{
				arrayList.IndexOf(0, -1, -1);
				Assertion.Fail("ArrayList.IndexOf(0, 10, 1)");
			}
			catch (ArgumentOutOfRangeException) 
			{
			}

			try 
			{
				arrayList.IndexOf(0, 9, 10);				
				Assertion.Fail("ArrayList.IndexOf(0, 10, 1)");
			}
			catch (ArgumentOutOfRangeException) 
			{				
			}

			try 
			{
				arrayList.IndexOf(0, 0, 10);				
			}
			catch (ArgumentOutOfRangeException) 
			{
				Assertion.Fail("ArrayList.IndexOf(0, 10, 1)");
			}

			try 
			{
				arrayList.IndexOf(0, 0, 11);
				Assertion.Fail("ArrayList.IndexOf(0, 10, 1)");
			}
			catch (ArgumentOutOfRangeException) 
			{				
			}

			// LastIndexOf

			for (int i = 0; i < 10; i++) 
			{
				x = arrayList.LastIndexOf(i);

				Assertion.Assert("ArrayList.LastIndexOf(" + i + ")", x == i);
			}			

			try 
			{
				arrayList.IndexOf(0, 10, 1);

				Assertion.Fail("ArrayList.LastIndexOf(0, 10, 1)");
			}
			catch (ArgumentOutOfRangeException) 
			{
			}

			try 
			{
				arrayList.IndexOf(0, 0, -1);

				Assertion.Fail("ArrayList.LastIndexOf(0, 10, 1)");
			}
			catch (ArgumentOutOfRangeException) 
			{
			}

			try 
			{
				arrayList.LastIndexOf(0, -1, -1);

				Assertion.Fail("ArrayList.LastIndexOf(0, 10, 1)");
			}
			catch (ArgumentOutOfRangeException) 
			{
			}

			try 
			{
				arrayList.LastIndexOf(0, 9, 10);				
			}
			catch (ArgumentOutOfRangeException) 
			{
				Assertion.Fail("ArrayList.LastIndexOf(0, 10, 1)");
			}

			try 
			{
				arrayList.LastIndexOf(0, 0, 10);
				Assertion.Fail("ArrayList.LastIndexOf(0, 10, 1)");
			}
			catch (ArgumentOutOfRangeException) 
			{				
			}

			try 
			{
				arrayList.LastIndexOf(0, 0, 11);
				Assertion.Fail("ArrayList.LastIndexOf(0, 10, 1)");
			}
			catch (ArgumentOutOfRangeException) 
			{				
			}
		}

		private void PrivateTestAddRange(ArrayList arrayList) 
		{
			arrayList.AddRange(c_TestData);
			arrayList.AddRange(c_TestData);

			VerifyContains(arrayList, new object[] {0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9}, "VerifyContains");
		}

		[Test]
		public void TestAddRangeStandard() 
		{
			PrivateTestAddRange(new ArrayList());
		}

		[Test]
		public void TestAddRangeSynchronized() 
		{
			PrivateTestAddRange(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestAddRangeAdapter() 
		{
			PrivateTestAddRange(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestAddRangeGetRange() 
		{
			PrivateTestAddRange(new ArrayList().GetRange(0, 0));
		}
		
		[Test]
		public void TestIndexOfStandard() 
		{
			PrivateTestIndexOf(new ArrayList());
		}

		[Test]
		public void TestIndexOfSynchronized() 
		{
			PrivateTestIndexOf(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestIndexOfAdapter() 
		{
			PrivateTestIndexOf(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestIndexOfGetRange() 
		{
			PrivateTestIndexOf(new ArrayList().GetRange(0, 0));
		}

		[Test]
		public void TestReadOnly() 
		{
			ArrayList arrayList, readOnlyList;
			
			arrayList = new ArrayList();
			readOnlyList = ArrayList.ReadOnly(arrayList);

			arrayList.AddRange(c_TestData);

			// Make sure the readOnlyList is a wrapper and not a clone.

			arrayList.Add(10);
			Assertion.Assert("readOnlyList.Count == 11", readOnlyList.Count == 11);

			try 
			{
				readOnlyList.Add(0);
				Assertion.Fail("readOnlyList.Add(0)");
			}
			catch (NotSupportedException) 
			{
			}

			try 
			{
				readOnlyList.AddRange(c_TestData);

				Assertion.Fail("readOnlyList.AddRange(c_TestData)");
			}
			catch (NotSupportedException) 
			{
			}
			
			try 
			{
				readOnlyList.BinarySearch(1);				
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.BinarySearch(1)");
			}			

			try 
			{
				int x = readOnlyList.Capacity;
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.Capacity");
			}

			try 
			{
				readOnlyList.Clear();
				Assertion.Fail("readOnlyList.Clear()");
			}
			catch (NotSupportedException) 
			{				
			}
			
			try 
			{
				readOnlyList.Clone();				
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.Clone()");
			}			

			try 
			{
				readOnlyList.Contains(1);
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.Contains");
			}

			try 
			{
				readOnlyList.CopyTo(new object[readOnlyList.Count]);
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.CopyTo(new Array(readOnlyList.Count))");
			}

			try 
			{
				foreach (object o in readOnlyList) 
				{
					o.ToString();
				}
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.GetEnumerator()");
			}

			try 
			{
				readOnlyList.GetRange(0, 1);				
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.GetRange(0, 1)");
			}

			try 
			{
				readOnlyList.IndexOf(1);				
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.readOnlyList.IndexOf(1)");
			}

			try 
			{
				readOnlyList[0] = 0;
				Assertion.Fail("readOnlyList[0] = 0");
			}
			catch (NotSupportedException) 
			{
			}

			try 
			{
				readOnlyList.IndexOf(0);
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.IndexOf(0)");
			}

			try 
			{
				readOnlyList.InsertRange(0, new object[] {1,2});

				Assertion.Fail("readOnlyList.InsertRange(0, new object[] {1,2})");
			}
			catch (NotSupportedException) 
			{
			}

			try 
			{
				readOnlyList.LastIndexOf(1111);
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.LastIndexOf(1)");
			}

			try 
			{
				readOnlyList.Remove(1);

				Assertion.Fail("readOnlyList.Remove(1)");
			}
			catch (NotSupportedException) 
			{
			}

			try 
			{
				readOnlyList.RemoveAt(1);

				Assertion.Fail("readOnlyList.RemoveAt(1)");
			}
			catch (NotSupportedException) 
			{
			}

			try 
			{
				readOnlyList.RemoveRange(0, 1);

				Assertion.Fail("readOnlyList.RemoveRange(0, 1)");
			}
			catch (NotSupportedException) 
			{
			}

			try 
			{
				readOnlyList.Reverse();

				Assertion.Fail("readOnlyList.Reverse()");
			}
			catch (NotSupportedException) 
			{				
			}

			try 
			{
				readOnlyList.SetRange(0, new Object[] {0, 1});

				Assertion.Fail("readOnlyList.SetRange(0, new Object[] {0, 1})");
			}
			catch (NotSupportedException) 
			{				
			}

			try 
			{
				readOnlyList.Sort();

				Assertion.Fail("readOnlyList.Sort()");
			}
			catch (NotSupportedException) 
			{
			}

			try 
			{
				readOnlyList.ToArray();				
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("readOnlyList.ToArray()");
			}

			try 
			{
				readOnlyList.TrimToSize();

				Assertion.Fail("readOnlyList.TrimToSize()");
			}
			catch (NotSupportedException) 
			{
			}			
		}

		[Test]
		public void TestFixedSize() 
		{
			ArrayList arrayList, fixedSizeList;
			
			arrayList = new ArrayList();
			fixedSizeList = ArrayList.FixedSize(arrayList);

			arrayList.AddRange(c_TestData);

			// Make sure the fixedSizeList is a wrapper and not a clone.

			arrayList.Add(10);
			Assertion.Assert("fixedSizeList.Count == 11", fixedSizeList.Count == 11);

			try 
			{
				fixedSizeList.Add(0);
				Assertion.Fail("fixedSizeList.Add(0)");
			}
			catch (NotSupportedException) 
			{
			}

			try 
			{
				fixedSizeList.Remove(0);
				Assertion.Fail("fixedSizeList.Remove(0)");
			}
			catch (NotSupportedException) 
			{				
			}

			try 
			{
				fixedSizeList.RemoveAt(0);
				Assertion.Fail("fixedSizeList.RemoveAt(0)");
			}
			catch (NotSupportedException) 
			{
			}

			try 
			{
				fixedSizeList.Clear();
				Assertion.Fail("fixedSizeList.Clear()");
			}
			catch (NotSupportedException) 
			{				
			}

			try 
			{
				fixedSizeList[0] = 0;				
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("fixedSizeList[0] = 0");
			}

			try 
			{
				fixedSizeList.Clear();
				Assertion.Fail("fixedSizeList.Clear()");
			}
			catch (NotSupportedException) 
			{
			}

			try 
			{
				fixedSizeList.Contains(1);
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("fixedSizeList.Contains");
			}

			try 
			{
				int x = fixedSizeList.Count;
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("fixedSizeList.Count");
			}

			try 
			{
				fixedSizeList.GetRange(0, 1);
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("fixedSizeList.GetRange(0, 1)");
			}

			try 
			{
				fixedSizeList.IndexOf(0);
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("fixedSizeList.IndexOf(0)");
			}

			try 
			{
				fixedSizeList.InsertRange(0, new object[] {1,2});

				Assertion.Fail("fixedSizeList.InsertRange(0, new object[] {1,2})");
			}
			catch (NotSupportedException) 
			{				
			}

			try 
			{
				fixedSizeList.Reverse();				
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("fixedSizeList.Reverse()");
			}

			try 
			{
				fixedSizeList.SetRange(0, new Object[] {0, 1});
			}
			catch (NotSupportedException) 
			{				
				Assertion.Fail("fixedSizeList.SetRange(0, new Object[] {0, 1})");
			}

			try 
			{
				fixedSizeList.Sort();
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("fixedSizeList.Sort()");
			}

			try 
			{
				fixedSizeList.ToArray();				
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("fixedSizeList.ToArray()");
			}

			try 
			{
				fixedSizeList.TrimToSize();

				Assertion.Fail("fixedSizeList.TrimToSize()");
			}
			catch (NotSupportedException) 
			{				
			}

			try 
			{
				fixedSizeList.Clone();				
			}
			catch (NotSupportedException) 
			{
				Assertion.Fail("fixedSizeList.Clone()");
			}
			
			try 
			{
				fixedSizeList.AddRange(c_TestData);

				Assertion.Fail("fixedSizeList.AddRange(c_TestData)");
			}
			catch (NotSupportedException) 
			{				
			}			
		}

		private void PrivateTestClone(ArrayList arrayList) 
		{			
			ArrayList arrayList2;
						
			arrayList.AddRange(c_TestData);

			arrayList2 = (ArrayList)arrayList.Clone();

			VerifyContains(arrayList2, c_TestData, "arrayList.Clone()");
		}

		[Test]
		public void TestCloneStandard() 
		{
			PrivateTestClone(new ArrayList());
		}

		[Test]
		public void TestCloneSynchronized() 
		{
			PrivateTestClone(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestCloneAdapter() 
		{
			PrivateTestClone(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestCloneGetRange() 
		{
			PrivateTestClone(new ArrayList().GetRange(0, 0));
		}

		private void PrivateTestCopyTo(ArrayList arrayList) 
		{
			object[] array;
			
			arrayList.AddRange(c_TestData);

			array = new Object[arrayList.Count];

			arrayList.CopyTo(array);
			
			VerifyContains(array, new object[] {0,1,2,3,4,5,6,7,8,9}, "ArrayList.CopyTo(array)");

			array = new Object[3];

			arrayList.CopyTo(0, array, 0, 3);
			
			VerifyContains(array, new object[] {0,1,2}, "ArrayList.CopyTo(0, array, 0, 3)");

			array = new Object[4];

			arrayList.CopyTo(0, array, 1, 3);
			
			VerifyContains(array, new object[] {null,0, 1, 2}, "ArrayList.CopyTo(0, array, 1, 3)");

			array = new object[10];

			arrayList.CopyTo(3, array, 3, 5);

			VerifyContains(array, new object[] {null, null, null, 3, 4, 5, 6, 7, null, null}, "VerifyContains(array, ...)");
		}

		[Test]
		public void TestCopyToStandard() 
		{
			PrivateTestCopyTo(new ArrayList());
		}

		[Test]
		public void TestCopyToSynchronized() 
		{
			PrivateTestCopyTo(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestCopyToAdapter() 
		{
			PrivateTestCopyTo(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestCopyToGetRange() 
		{
			PrivateTestCopyTo(new ArrayList().GetRange(0, 0));
		}
		
		private void PrivateTestSetCapacity(ArrayList arrayList) 
		{
			int x;

			arrayList.AddRange(c_TestData);

			x = arrayList.Capacity;

			arrayList.Capacity = x * 2;

			Assert("arrayList.Capacity == x * 2", arrayList.Capacity == x * 2);

			VerifyContains(arrayList, c_TestData, "VerifyContains(arrayList, c_TestData)");
		}

		[Test]
		public void TestSetCapacity() 
		{
			PrivateTestSetCapacity(new ArrayList());
		}

		[Test]
		public void TestSetCapacitySynchronized() 
		{
			PrivateTestSetCapacity(ArrayList.Synchronized(new ArrayList()));
		}
		
		[Test]
		public void TestCapacityExpands() 
		{
			ArrayList arrayList = new ArrayList(10);

			arrayList.AddRange(c_TestData);

			Assertion.Assert("arrayList.Capacity == 10", arrayList.Capacity == 10);

			arrayList.Add(10);

			Assertion.Assert("arrayList.Capacity == 20", arrayList.Capacity == 20);

			VerifyContains(arrayList, new object[] {0,1,2,3,4,5,6,7,8,9,10}, "VerifyContains");
		}
		
		private void PrivateTestBinarySearch(ArrayList arrayList) 
		{
			// Try searching with different size lists...

			for (int x = 0; x < 10; x++) 
			{
				for (int i = 0; i < x; i++) 
				{
					arrayList.Add(i);
				}

				for (int i = 0; i < x; i++) 
				{
					int y;

					y = arrayList.BinarySearch(i);

					Assertion.Equals(y, i);
				}
			}

			arrayList.Clear();
			arrayList.Add(new object());

			try 
			{
				arrayList.BinarySearch(new object());

				Assertion.Fail("1: Binary search on object that doesn't support IComparable.");
			}
			catch (ArgumentException) 
			{				
			}
			catch (InvalidOperationException) 
			{
				// LAMESPEC: ArrayList.BinarySearch() on MS.NET throws InvalidOperationException
			}
			
			try 
			{
				arrayList.BinarySearch(1);

				Assertion.Fail("2: Binary search on incompatible object.");
			}
			catch (ArgumentException) 
			{				
			}
			catch (InvalidOperationException) 
			{
				// LAMESPEC: ArrayList.BinarySearch() on MS.NET throws InvalidOperationException
			}

			arrayList.Clear();

			for (int i = 0; i < 100; i++) 
			{
				arrayList.Add(1);
			}

			Assertion.Assert("BinarySearch should start in middle.", arrayList.BinarySearch(1) == 49);			
			Assertion.Assert("arrayList.BinarySearch(0, 0, 0, Comparer.Default)", arrayList.BinarySearch(0, 0, 0, Comparer.Default) == -1);
		}

		[Test]
		public void TestBinarySearchStandard() 
		{
			PrivateTestBinarySearch(new ArrayList());
		}

		[Test]
		public void TestBinarySearchSynchronized() 
		{
			PrivateTestBinarySearch(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestBinarySearchAdapter() 
		{
			PrivateTestBinarySearch(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestBinarySearchGetRange() 
		{
			PrivateTestBinarySearch(new ArrayList().GetRange(0, 0));
		}
		
		private void PrivateTestRemoveAt(ArrayList arrayList) 
		{
			arrayList.Add(1);
			arrayList.Add(2);
			arrayList.Add(3);
			arrayList.Add(4);
			arrayList.Add(5);

			arrayList.Remove(2);

			VerifyContains(arrayList, new object[] {1, 3, 4, 5},
				"Remove element failed.");

			arrayList.RemoveAt(0);

			VerifyContains(arrayList, new object[] {3, 4, 5},
				"RemoveAt at start failed.");

			arrayList.RemoveAt(2);

			VerifyContains(arrayList, new object[] {3, 4},
				"RemoveAt at end failed.");			
		}

		[Test]
		public void TestRemoveAtStandard() 
		{
			PrivateTestRemoveAt(new ArrayList());
		}

		[Test]
		public void TestRemoveAtSynchronized() 
		{
			PrivateTestRemoveAt(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestRemoveAtAdapter() 
		{
			PrivateTestRemoveAt(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestRemoveAtGetRange() 
		{
			PrivateTestRemoveAt(new ArrayList().GetRange(0, 0));
		}

		private void PrivateTestRemoveRange(ArrayList arrayList) 
		{
			arrayList.AddRange(c_TestData);

			arrayList.RemoveRange(0, 3);

			VerifyContains(arrayList, new object[] { 3, 4, 5, 6, 7, 8, 9 },
				"RemoveRange at start failed.");

			arrayList.RemoveRange(4, 3);

			VerifyContains(arrayList, new object[] { 3, 4, 5, 6 },
				"RemoveRange at start failed.");

			arrayList.RemoveRange(2, 1);

			VerifyContains(arrayList, new object[] { 3, 4, 6 },
				"RemoveRange in middle failed.");
		}

		[Test]
		public void TestRemoveRangeStandard() 
		{
			PrivateTestRemoveRange(new ArrayList());
		}

		[Test]
		public void TestRemoveRangeSynchronized() 
		{
			PrivateTestRemoveRange(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestRemoveRangeAdapter() 
		{
			PrivateTestRemoveRange(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestRemoveRangeGetRange() 
		{
			PrivateTestRemoveRange(new ArrayList().GetRange(0, 0));
		}

		private void PrivateTestInsert(ArrayList arrayList) 
		{
			arrayList.Add(1);
			arrayList.Add(2);
			arrayList.Add(3);
			arrayList.Add(4);
			arrayList.Insert(0, 1);

			VerifyContains(arrayList, new object[] {1, 1, 2, 3, 4}, "Insert at beginning failed.");

			arrayList.Insert(5, 5);

			VerifyContains(arrayList, new object[] {1, 1, 2, 3, 4, 5}, "Insert at end failed.");

			arrayList.Insert(3, 7);

			VerifyContains(arrayList, new object[] {1, 1, 2, 7, 3, 4, 5}, "Insert in middle failed.");
		}

		[Test]
		public void TestInsertStandard() 
		{
			PrivateTestInsert(new ArrayList());
		}

		[Test]
		public void TestInsertAdapter() 
		{
			PrivateTestInsert(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestInsertSynchronized() 
		{
			PrivateTestInsert(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestInsertGetRange() 
		{
			PrivateTestInsert(new ArrayList().GetRange(0, 0));
		}

		private void PrivateTestGetRange(ArrayList arrayList) 
		{
			ArrayList rangeList;

			arrayList.AddRange(c_TestData);

			rangeList = arrayList.GetRange(3, 5);

			Assertion.Assert("rangeList.Count == 5", rangeList.Count == 5);

			this.VerifyContains(rangeList, new object[] {3,4,5,6,7}, "1: VerifyContains(rangeList)");
			
//FIXME: If items are removed from the Range, one may not iterate over it on .NET
/*
			rangeList.Remove(7);
			
			this.VerifyContains(a2, new object[] {3,4,5,6}, "2: VerifyContains(rangeList)");

			rangeList.RemoveAt(0);

			this.VerifyContains(a3, new object[] {4,5,6}, "3: VerifyContains(rangeList)");

			rangeList.Add(7);
			rangeList.Add(6);
			rangeList.Add(3);
			rangeList.Add(11);
			
			Assertion.Assert("rangeList.LastIndexOf(6) == 4", rangeList.LastIndexOf(6) == 4);

			rangeList.Sort();

			this.VerifyContains(arrayList, new object[] {0, 1, 2, 3, 4, 5, 6, 6, 7, 11, 8, 9}, "4: VerifyContains(rangeList)");
*/
		}

		[Test]
		public void TestGetRangeStandard() 
		{
			PrivateTestGetRange(new ArrayList());
		}

		[Test]
		public void TestGetRangeAdapter() 
		{
			PrivateTestGetRange(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestGetRangeSynchronized() 
		{
			PrivateTestGetRange(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestGetRangeGetRange() 
		{
			PrivateTestGetRange(new ArrayList().GetRange(0, 0));
		}

		private void PrivateTestEnumeratorWithRange(ArrayList arrayList) 
		{			
			IEnumerator enumerator;

			arrayList.AddRange(c_TestData);

			int x;

			// Test with the range 1 - 3

			enumerator = arrayList.GetEnumerator(1, 3);
			
			x = 1;

			while (enumerator.MoveNext()) 
			{
				Assertion.Assert("enumerator.Current == x", (int)enumerator.Current == x);

				x++;
			}

			enumerator.Reset();

			x = 1;

			while (enumerator.MoveNext()) 
			{
				Assertion.Assert("enumerator.Current == x", (int)enumerator.Current == x);

				x++;
			}


			// Test with a range covering the whole list.

			enumerator = arrayList.GetEnumerator(0, arrayList.Count);
			
			x = 0;

			while (enumerator.MoveNext()) 
			{
				Assertion.Assert("enumerator.Current == x", (int)enumerator.Current == x);

				x++;
			}

			enumerator.Reset();

			x = 0;

			while (enumerator.MoveNext()) 
			{
				Assertion.Assert("enumerator.Current == x", (int)enumerator.Current == x);

				x++;
			}

			// Test with a range covering nothing.

			enumerator = arrayList.GetEnumerator(arrayList.Count, 0);

			Assertion.Assert("!enumerator.MoveNext()", !enumerator.MoveNext());

			enumerator.Reset();
			
			Assertion.Assert("!enumerator.MoveNext()", !enumerator.MoveNext());
		}

		[Test]
		public void TestEnumeratorWithRangeStandard() 
		{
			PrivateTestEnumeratorWithRange(new ArrayList());
		}

		[Test]
		public void TestEnumeratorWithRangeSynchronized() 
		{
			PrivateTestEnumeratorWithRange(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestEnumeratorWithRangeAdapter() 
		{
			PrivateTestEnumeratorWithRange(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestEnumeratorWithRangeGetRange() 
		{
			PrivateTestEnumeratorWithRange(new ArrayList().GetRange(0, 0));
		}

		private void PrivateTestEnumerator(ArrayList arrayList) 
		{
			int x = 0;

			arrayList.AddRange(c_TestData);

			x = 0;

			foreach (object o in arrayList) 
			{
				if (!o.Equals(x)) 
				{
					Assertion.Fail("Arraylist.GetEnumerator()");

					break;
				}

				x++;
			}

			IEnumerator enumerator;

			enumerator = arrayList.GetEnumerator();

			enumerator.MoveNext();

			Assert("enumerator.Current == 0", (int)enumerator.Current == 0);

			// Invalidate the enumerator.

			arrayList.Add(10);
			
			try 
			{
				// According to the spec this should still work even though the enumerator is invalid.

				Assert("enumerator.Current == 0", (int)enumerator.Current == 0);
			}
			catch (InvalidOperationException) 
			{
				Assert("enumerator.Current should not fail.", false);
			}

			try 
			{
				// This should throw an InvalidOperationException.

				enumerator.MoveNext();

				Assert("enumerator.Current should fail.", false);
			}
			catch (InvalidOperationException) 
			{
			}
		}

		[Test]
		public void TestEnumeratorStandard() 
		{
			PrivateTestEnumerator(new ArrayList());
		}

		[Test]
		public void TestEnumeratorSynchronized() 
		{
			PrivateTestEnumerator(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestEnumeratorAdapter() 
		{
			PrivateTestEnumerator(ArrayList.Adapter(new ArrayList()));
		}

		[Test]
		public void TestEnumeratorGetRange() 
		{
			PrivateTestEnumerator(new ArrayList().GetRange(0, 0));
		}
		
		private void PrivateTestReverse(ArrayList arrayList) 
		{			
			ArrayList arrayList2;
			
			for (int x = 1; x < 100; x ++) 
			{
				arrayList2 = (ArrayList)arrayList.Clone();
				
				for (int i = 0; i < x; i++) 
				{
					arrayList2.Add(i);
				}

				arrayList2.Reverse();

				bool ok = true;

				// Check that reverse did reverse the adapter.

				for (int i = 0; i < x; i++) 
				{
					if ((int)arrayList2[i] != x - i - 1) 
					{
						ok = false;

						break;
					}				
				}

				Assertion.Assert
					(
					String.Format("Reverse on arrayList failed on list with {0} items.", x),
					ok
					);
			}
		}

		[Test]
		public void TestReverseStandard() 
		{
			PrivateTestReverse(new ArrayList());
		}

		[Test]
		public void TestReverseAdapter() 
		{
			ArrayList arrayList = new ArrayList();
			ArrayList adapter = ArrayList.Adapter(arrayList);

			PrivateTestReverse(adapter);

			VerifyContains(adapter, arrayList, "Changing adapter didn't change ArrayList.");
		}

		[Test]
		public void TestReverseSynchronized() 
		{
			PrivateTestReverse(ArrayList.Synchronized(new ArrayList()));
		}

		[Test]
		public void TestReverseGetRange() 
		{
			PrivateTestReverse(new ArrayList().GetRange(0,0));
		}

		[Test]
		public void TestIterator ()
		{
			ArrayList a = new ArrayList ();
			a.Add (1);
			a.Add (2);
			a.Add (3);

			int total = 0;
			foreach (int b in a)
				total += b;
			Assert ("Count should be 6", total == 6);
		}

		[Test]
		public void TestIteratorObjects ()
		{
			ArrayList a = new ArrayList ();
			a.Add (1);
			a.Add (null);
			a.Add (3);

			int total = 0;
			int count = 0;
			bool found_null = false;
			foreach (object b in a){
				count++;
				if (b == null){
					if (found_null)
						Assert ("Should only find one null", false);
					found_null = true;
				} else {
					total += (int) b;
				}
			}
			
			Assert ("Should fine one null", found_null);
			Assert ("Total should be 4", total == 4);
			Assert ("Count should be 3", count == 3);
		}
	}
}
