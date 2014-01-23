//
// OrderedDictionaryTest.cs -
//	Unit tests for System.Collections.Specialized.OrderedDictionary
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if !MOBILE

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Collections.Specialized {

	[TestFixture]
        public class OrderedDictionaryTest {

		private void Common (OrderedDictionary od)
		{
			Assert.IsNotNull (od.GetEnumerator (), "GetEnumerator");
			Assert.AreEqual (0, od.Count, "Count-0");
			Assert.IsFalse (od.IsReadOnly, "IsReadOnly");
			od.Add ("a", "1");
			Assert.AreEqual (1, od.Count, "Count-1");
			od["a"] = "11";
			Assert.AreEqual ("11", od["a"], "this[string]");
			od[0] = "111";
			Assert.AreEqual ("111", od[0], "this[int]");

			DictionaryEntry[] array = new DictionaryEntry[2];
			od.CopyTo (array, 1);

			Assert.AreEqual ("111", ((DictionaryEntry)array[1]).Value, "CopyTo");
			Assert.AreEqual (1, od.Keys.Count, "Keys");
			Assert.AreEqual (1, od.Values.Count, "Values");
			Assert.IsTrue (od.Contains ("a"), "Contains(a)");
			Assert.IsFalse (od.Contains ("111"), "Contains(111)");

			od.Insert (0, "b", "2");
			Assert.AreEqual (2, od.Count, "Count-2");
			od.Add ("c", "3");
			Assert.AreEqual (3, od.Count, "Count-3");

			OrderedDictionary ro = od.AsReadOnly ();

			od.RemoveAt (2);
			Assert.AreEqual (2, od.Count, "Count-4");
			Assert.IsFalse (od.Contains ("c"), "Contains(c)");

			od.Remove ("b");
			Assert.AreEqual (1, od.Count, "Count-5");
			Assert.IsFalse (od.Contains ("b"), "Contains(b)");

			od.Clear ();
			Assert.AreEqual (0, od.Count, "Count-6");

			Assert.IsTrue (ro.IsReadOnly, "IsReadOnly-2");
			// it's a read-only reference
			Assert.AreEqual (0, od.Count, "Count-7");
		}

		[Test]
		public void Constructor_Default ()
		{
			OrderedDictionary od = new OrderedDictionary ();
			Common (od);
		}

		[Test]
		public void Constructor_Int ()
		{
			OrderedDictionary od = new OrderedDictionary ();
			Common (od);
		}

		[Test]
		// [ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_IntNegative ()
		{
			new OrderedDictionary (-1);
		}

		[Test]
		public void Constructor_IEqualityComparer ()
		{
			OrderedDictionary od = new OrderedDictionary (new EqualityComparer ());
			Common (od);
		}

		[Test]
		public void Constructor_Int_IEqualityComparer ()
		{
			OrderedDictionary od = new OrderedDictionary (5, new EqualityComparer ());
			Common (od);
		}

		[Test]
		public void Constructor_NoCase_IEqualityComparer ()
		{
			OrderedDictionary od = new OrderedDictionary (StringComparer.InvariantCultureIgnoreCase);
			od ["Original_PhotoID"] = null;
			od ["original_PhotoID"] = 12;
			Assert.AreEqual (1, od.Count);
		}

		[Test]
		// [ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_IntNegative_IEqualityComparer ()
		{
			new OrderedDictionary (-1, new EqualityComparer ());
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadOnly_This_String ()
		{
			OrderedDictionary od = new OrderedDictionary ().AsReadOnly ();
			od["a"] = 1;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadOnly_This_int ()
		{
			OrderedDictionary od = new OrderedDictionary ().AsReadOnly ();
			od[0] = 1;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadOnly_Add ()
		{
			OrderedDictionary od = new OrderedDictionary ().AsReadOnly ();
			od.Add ("a", "1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadOnly_Clear ()
		{
			OrderedDictionary od = new OrderedDictionary ().AsReadOnly ();
			od.Clear ();
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadOnly_Insert ()
		{
			OrderedDictionary od = new OrderedDictionary ().AsReadOnly ();
			od.Insert (0, "a", "1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadOnly_Remove ()
		{
			OrderedDictionary od = new OrderedDictionary ().AsReadOnly ();
			od.Remove ("a");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadOnly_RemoveAt ()
		{
			OrderedDictionary od = new OrderedDictionary ().AsReadOnly ();
			od.RemoveAt (0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObjectData_Null ()
		{
			OrderedDictionary coll = new OrderedDictionary ();
			coll.GetObjectData (null, new StreamingContext ());
		}

		[Test]
		public void GetObjectData ()
		{
			OrderedDictionary coll = new OrderedDictionary (99);
			coll.Add ("a", "1");

			SerializationInfo si = new SerializationInfo (typeof (OrderedDictionary), new FormatterConverter ());
			coll.GetObjectData (si, new StreamingContext ());
			foreach (SerializationEntry se in si) {
				switch (se.Name) {
				case "KeyComparer":
					Assert.IsNull (se.Value, se.Name);
					break;
				case "ReadOnly":
					Assert.IsFalse ((bool) se.Value, se.Name);
					break;
				case "InitialCapacity":
					Assert.AreEqual (99, se.Value, se.Name);
					break;
				case "ArrayList":
					Assert.AreEqual ("1", ((DictionaryEntry)((object[]) se.Value)[0]).Value, se.Name);
					break;
				default:
					string msg = String.Format ("Unexpected {0} information of type {1} with value '{2}'.",
						se.Name, se.ObjectType, se.Value);
					Assert.Fail (msg);
					break;
				}
			}
		}

		[Test]
		public void GetObjectData_IEqualityComparer ()
		{
			EqualityComparer comparer = new EqualityComparer ();
			OrderedDictionary coll = new OrderedDictionary (comparer);
			coll.Add ("a", "1");
			coll.Add ("b", "2");
			coll = coll.AsReadOnly ();

			SerializationInfo si = new SerializationInfo (typeof (OrderedDictionary), new FormatterConverter ());
			coll.GetObjectData (si, new StreamingContext ());
			foreach (SerializationEntry se in si) {
				switch (se.Name) {
				case "KeyComparer":
					Assert.AreSame (comparer, se.Value, se.Name);
					break;
				case "ReadOnly":
					Assert.IsTrue ((bool) se.Value, se.Name);
					break;
				case "InitialCapacity":
					Assert.AreEqual (0, se.Value, se.Name);
					break;
				case "ArrayList":
					Assert.AreEqual (2, ((object[]) se.Value).Length, se.Name);
					break;
				default:
					string msg = String.Format ("Unexpected {0} information of type {1} with value '{2}'.",
						se.Name, se.ObjectType, se.Value);
					Assert.Fail (msg);
					break;
				}
			}
		}
	}
}

#endif
