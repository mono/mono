//
// MonoTests.System.ArraySortArgChecks
//
// Authors:
//      Juraj Skripsky  (js@hotfeet.ch)
//
// Copyright (C) 2009 Juraj Skripsky (js@hotfeet.ch)
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

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;


namespace MonoTests.System {
	class SomeIncomparable {}
	class SomeComparable : IComparable {
		int IComparable.CompareTo (object other) {
			return 0;
		}
	}

	[TestFixture]
	public class ArraySortArgChecks {

		[Test]
		public void Check_ArgumentNullException() {
			try {
				Array.Sort (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort (null, (Array)null);
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort (null, (IComparer)null);
				Assert.Fail ("#3");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort (null, 0, 0);
				Assert.Fail ("#4");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort (null, null, null);
				Assert.Fail ("#5");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort (null, null, 0, 0);
				Assert.Fail ("#6");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort (null, 0, 0, null);
				Assert.Fail ("#7");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort (null, null, 0, 0, null);
				Assert.Fail ("#8");
			} catch (ArgumentNullException) {}
	
			try {
				Array.Sort<object> (null);
				Assert.Fail ("#9");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort<object, object> (null, null);
				Assert.Fail ("#10");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort<object> (null, (IComparer<object>)null);
				Assert.Fail ("#11");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort<object, object> (null, null, null);
				Assert.Fail ("#12");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort<object> (null, 0, 0);
				Assert.Fail ("#13");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort<object, object> (null, null, 0, 0);
				Assert.Fail ("#14");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort<object> (null, 0, 0, null);
				Assert.Fail ("#15");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort<object, object> (null, null, 0, 0, null);
				Assert.Fail ("#16");
			} catch (ArgumentNullException) {}
			
			try {
				Array.Sort<object> (null, new Comparison<object>(ObjComparison));
				Assert.Fail ("#17");
			} catch (ArgumentNullException) {}
		}	
		
		public static int ObjComparison (object o1, object o2) {
			return 0;
		}
	
	
		[Test]
		public void Check_ArgumentException() {
			object[] arr = new object[] {1, 2, 3, 4, 5};
	
			try {
				Array.Sort (arr, 1, 5);
				Assert.Fail ("#1");
			} catch (ArgumentException) {}
			
			try {
				Array.Sort (arr, null, 1, 5);
				Assert.Fail ("#2");
			} catch (ArgumentException) {}
			
			try {
				Array.Sort (arr, 1, 5, null);
				Assert.Fail ("#3");
			} catch (ArgumentException) {}
			
			try {
				Array.Sort (arr, null, 1, 5, null);
				Assert.Fail ("#4");
			} catch (ArgumentException) {}
			
			try {
				Array.Sort<object> (arr, 1, 5);
				Assert.Fail ("#5");
			} catch (ArgumentException) {}
			
			try {
				Array.Sort<object, object> (arr, null, 1, 5);
				Assert.Fail ("#6");
			} catch (ArgumentException) {}
			
			try {
				Array.Sort<object> (arr, 1, 5, null);
				Assert.Fail ("#7");
			} catch (ArgumentException) {}
			
			try {
				Array.Sort<object, object> (arr, null, 1, 5, null);
				Assert.Fail ("#8");
			} catch (ArgumentException) {}
		}
	
		[Test]
		public void Check_ArgumentOurOfRangeException() {
			object[] arr = new object[] {1, 2, 3, 4, 5};
			try {
				Array.Sort (arr, -1, 1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {}
			
			try {
				Array.Sort (arr, null, -1, 1);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {}
			
			try {
				Array.Sort (arr, -1, 1, null);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {}
			
			try {
				Array.Sort (arr, null, -1, 1, null);
				Assert.Fail ("#4");
			} catch (ArgumentOutOfRangeException) {}

			try {
				Array.Sort (arr, 0, -1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {}

			try {
				Array.Sort (arr, null, 0, -1);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {}

			try {
				Array.Sort (arr, 0, -1, null);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {}

			try {
				Array.Sort (arr, null, 0, -1, null);
				Assert.Fail ("#4");
			} catch (ArgumentOutOfRangeException) {}
	
			try {
				Array.Sort<object> (arr, -1, 1);
				Assert.Fail ("#5");
			} catch (ArgumentOutOfRangeException) {}
			
			try {
				Array.Sort<object, object> (arr, null, -1, 1);
				Assert.Fail ("#6");
			} catch (ArgumentOutOfRangeException) {}
			
			try {
				Array.Sort<object> (arr, -1, 1, null);
				Assert.Fail ("#7");
			} catch (ArgumentOutOfRangeException) {}
			
			try {
				Array.Sort<object, object> (arr, null, -1, 1, null);
				Assert.Fail ("#8");
			} catch (ArgumentOutOfRangeException) {}
						
			try {
				Array.Sort<object> (arr, 0, -1);
				Assert.Fail ("#5");
			} catch (ArgumentOutOfRangeException) {}
			
			try {
				Array.Sort<object, object> (arr, null, 0, -1);
				Assert.Fail ("#6");
			} catch (ArgumentOutOfRangeException) {}
			
			try {
				Array.Sort<object> (arr, 0, -1, null);
				Assert.Fail ("#7");
			} catch (ArgumentOutOfRangeException) {}
			
			try {
				Array.Sort<object, object> (arr, null, 0, -1, null);
				Assert.Fail ("#8");
			} catch (ArgumentOutOfRangeException) {}
		}
	
		[Test]
		public void Check_RankException() {
			object[,] arr = new object[2,2];
	
			try {
				Array.Sort (arr);
				Assert.Fail ("#1");
			} catch (RankException) {}
			
			try {
				Array.Sort (arr, (Array)null);
				Assert.Fail ("#2");
			} catch (RankException) {}
			
			try {
				Array.Sort (arr, (IComparer)null);
				Assert.Fail ("#3");
			} catch (RankException) {}
			
			try {
				Array.Sort (arr, 0, 0);
				Assert.Fail ("#4");
			} catch (RankException) {}
			
			try {
				Array.Sort (arr, null, null);
				Assert.Fail ("#5");
			} catch (RankException) {}
			
			try {
				Array.Sort (arr, null, 0, 0);
				Assert.Fail ("#6");
			} catch (RankException) {}
			
			try {
				Array.Sort (arr, 0, 0, null);
				Assert.Fail ("#7");
			} catch (RankException) {}
			
			try {
				Array.Sort (arr, null, 0, 0, null);
				Assert.Fail ("#8");
			} catch (RankException) {}
		}
	
		[Test]
		public void Check_NoInvalidOperationException ()
		{
			Array arr = new object[] {new SomeComparable (), new SomeIncomparable (), new SomeComparable ()};
	
			Array.Sort (arr);
			
			Array.Sort (arr, (Array)null);
			
			Array.Sort (arr, (IComparer)null);
			
			Array.Sort (arr, 0, 3);
			
			Array.Sort (arr, null, null);
			
			Array.Sort (arr, null, 0, 3);
			
			Array.Sort (arr, 0, 3, null);
			
			Array.Sort (arr, null, 0, 3, null);
		}

		[Test]
		public void Check_NoInvalidOperationException_Generic ()
		{
			object[] arr = new object[] {new SomeComparable (), new SomeIncomparable (), new SomeComparable ()};

			Array.Sort<object> (arr);
			
			Array.Sort<object, object> (arr, null);
			
			Array.Sort<object> (arr, (IComparer<object>)null);
			
			Array.Sort<object, object> (arr, null, null);
			
			Array.Sort<object> (arr, 0, 3);
			
			Array.Sort<object, object> (arr, null, 0, 3);
			
			Array.Sort<object> (arr, 0, 3, null);
			
			Array.Sort<object, object> (arr, null, 0, 3, null);
		}
	}		
}
