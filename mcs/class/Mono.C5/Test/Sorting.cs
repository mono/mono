/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using C5;
using NUnit.Framework;
using SCG = System.Collections.Generic;

namespace C5UnitTests.SortingTests
{
	[TestFixture]
	public class SortRandom
	{
		IC ic;

		Random ran;

		int[] a;

		int length;


		[SetUp]
		public void Init()
		{
			ic = new IC();
			ran = new Random(3456);
			length = 100000;
			a = new int[length];
			for (int i = 0; i < length; i++)
				a[i] = ran.Next();
		}


		[Test]
		public void HeapSort()
		{
			Sorting.HeapSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[Test]
		public void IntroSort()
		{
			Sorting.IntroSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[Test]
		public void InsertionSort()
		{
			length = 1000;
			Sorting.InsertionSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);

			Sorting.InsertionSort<int>(a, length, 2 * length, ic);
			for (int i = length + 1; i < 2 * length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[TearDown]
		public void Dispose() { ic = null; }
	}



	[TestFixture]
	public class SortRandomDuplicates
	{
		IC ic;

		Random ran;

		int[] a;

		int length;


		[SetUp]
		public void Init()
		{
			ic = new IC();
			ran = new Random(3456);
			length = 100000;
			a = new int[length];
			for (int i = 0; i < length; i++)
				a[i] = ran.Next(3, 23);
		}


		[Test]
		public void HeapSort()
		{
			Sorting.HeapSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[Test]
		public void IntroSort()
		{
			Sorting.IntroSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[Test]
		public void InsertionSort()
		{
			length = 1000;
			Sorting.InsertionSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);

			Sorting.InsertionSort<int>(a, length, 2 * length, ic);
			for (int i = length + 1; i < 2 * length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[TearDown]
		public void Dispose() { ic = null; a = null; ran = null; }
	}



	[TestFixture]
	public class SortIncreasing
	{
		IC ic;

		int[] a;

		int length;


		[SetUp]
		public void Init()
		{
			ic = new IC();
			length = 100000;
			a = new int[length];
			for (int i = 0; i < length; i++)
				a[i] = i;
		}


		[Test]
		public void HeapSort()
		{
			Sorting.HeapSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[Test]
		public void IntroSort()
		{
			Sorting.IntroSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[Test]
		public void InsertionSort()
		{
			length = 1000;
			Sorting.InsertionSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);

			Sorting.InsertionSort<int>(a, length, 2 * length, ic);
			for (int i = length + 1; i < 2 * length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[TearDown]
		public void Dispose() { ic = null; a = null; }
	}



	[TestFixture]
	public class SortDecreasing
	{
		IC ic;

		int[] a;

		int length;


		[SetUp]
		public void Init()
		{
			ic = new IC();
			length = 100000;
			a = new int[length];
			for (int i = 0; i < length; i++)
				a[i] = -i;
		}


		[Test]
		public void HeapSort()
		{
			Sorting.HeapSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[Test]
		public void IntroSort()
		{
			Sorting.IntroSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[Test]
		public void InsertionSort()
		{
			length = 1000;
			Sorting.InsertionSort<int>(a, 0, length, ic);
			for (int i = 1; i < length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);

			Sorting.InsertionSort<int>(a, length, 2 * length, ic);
			for (int i = length + 1; i < 2 * length; i++)
				Assert.IsTrue(a[i - 1] <= a[i], "Inversion at " + i);
		}


		[TearDown]
		public void Dispose() { ic = null; a = null; }
	}
}