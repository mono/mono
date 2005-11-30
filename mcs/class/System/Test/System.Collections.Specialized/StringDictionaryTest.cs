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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Collections.Specialized {

	[TestFixture]
        public class StringDictionaryTest {

		[Test]
		public void Empty ()
		{
			StringDictionary sd = new StringDictionary ();
			Assert.AreEqual (0, sd.Count, "Count");
			Assert.IsFalse (sd.IsSynchronized, "IsSynchronized");
			Assert.AreEqual (0, sd.Keys.Count, "Keys");
			Assert.AreEqual (0, sd.Values.Count, "Values");
			Assert.IsNotNull (sd.SyncRoot, "SyncRoot");
			Assert.IsFalse (sd.ContainsKey ("a"), "ContainsKey");
			Assert.IsFalse (sd.ContainsValue ("1"), "ContainsValue");
			sd.CopyTo (new DictionaryEntry[0], 0);
			Assert.IsNotNull (sd.GetEnumerator (), "GetEnumerator");
			sd.Remove ("a"); // doesn't exists
			sd.Clear ();
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void This_Null ()
		{
			StringDictionary sd = new StringDictionary ();
			sd[null] = "1";
		}

		[Test]
		public void This_Empty ()
		{
			StringDictionary sd = new StringDictionary ();
			sd[String.Empty] = null;
			Assert.IsNull (sd[String.Empty], "this[String.Empty]");
			Assert.AreEqual (1, sd.Count, "Count-1");
			Assert.IsTrue (sd.ContainsKey (String.Empty), "ContainsKey");
			Assert.IsTrue (sd.ContainsValue (null), "ContainsValue");
		}

		[Test]
		public void SomeElements ()
		{
			StringDictionary sd = new StringDictionary ();
			for (int i = 0; i < 10; i++)
				sd.Add (i.ToString (), (i * 10).ToString ());
			Assert.AreEqual ("10", sd["1"], "this[1]");
			Assert.AreEqual (10, sd.Count, "Count-10");
			Assert.AreEqual (10, sd.Keys.Count, "Keys");
			Assert.AreEqual (10, sd.Values.Count, "Values");
			Assert.IsTrue (sd.ContainsKey ("2"), "ContainsKey");
			Assert.IsTrue (sd.ContainsValue ("20"), "ContainsValue");
			DictionaryEntry[] array = new DictionaryEntry[10];
			sd.CopyTo (array, 0);
			sd.Remove ("1");
			Assert.AreEqual (9, sd.Count, "Count-9");
			sd.Clear ();
			Assert.AreEqual (0, sd.Count, "Count-0");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Add_NullKey ()
		{
			StringDictionary sd = new StringDictionary ();
			sd.Add (null, "a");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void ContainsKey_Null ()
		{
			StringDictionary sd = new StringDictionary ();
			sd.ContainsKey (null);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Remove_Null ()
		{
			StringDictionary sd = new StringDictionary ();
			sd.Remove (null);
		}
	}
}
