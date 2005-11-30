//
// ListDictionaryTest.cs
//      - NUnit Test Cases for System.Collections.Specialized.ListDictionary.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Alon Gazit (along@mainsoft.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2003 Ximian Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace MonoTests.System.Collections.Specialized {

	[TestFixture]
	public class ListDictionaryTest {

		private void BasicTests (ListDictionary ld)
		{
			Assert.AreEqual (0, ld.Count, "Count");
			Assert.IsFalse (ld.IsFixedSize, "IsFixedSize");
			Assert.IsFalse (ld.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (ld.IsSynchronized, "IsSynchronized");
			Assert.AreEqual (0, ld.Keys.Count, "Keys");
			Assert.AreEqual (0, ld.Values.Count, "Values");
			Assert.IsNotNull (ld.SyncRoot, "SyncRoot");
			Assert.IsNotNull (ld.GetEnumerator (), "GetEnumerator");
			Assert.IsNotNull ((ld as IEnumerable).GetEnumerator (), "IEnumerable.GetEnumerator");

			ld.Add ("a", "1");
			Assert.AreEqual (1, ld.Count, "Count-1");
			Assert.IsTrue (ld.Contains ("a"), "Contains(a)");
			Assert.IsFalse (ld.Contains ("1"), "Contains(1)");

			ld.Add ("b", null);
			Assert.AreEqual (2, ld.Count, "Count-2");
			Assert.IsNull (ld["b"], "this[b]");

			DictionaryEntry[] entries = new DictionaryEntry[2];
			ld.CopyTo (entries, 0);

			ld["b"] = "2";
			Assert.AreEqual ("2", ld["b"], "this[b]2");

			ld.Remove ("b");
			Assert.AreEqual (1, ld.Count, "Count-3");
			ld.Clear ();
			Assert.AreEqual (0, ld.Count, "Count-4");
		}

		[Test]
		public void Constructor_Default ()
		{
			ListDictionary ld = new ListDictionary ();
			BasicTests (ld);
		}

		[Test]
		public void Constructor_IComparer_Null ()
		{
			ListDictionary ld = new ListDictionary (null);
			BasicTests (ld);
		}

		[Test]
		public void Constructor_IComparer ()
		{
			ListDictionary ld = new ListDictionary (new CaseInsensitiveComparer ());
			BasicTests (ld);
		}

                [Test, ExpectedException (typeof (ArgumentNullException))]
                public void CopyTo1 ()
                {
                        ListDictionary ld = new ListDictionary ();
                        ld.CopyTo (null, 0);
                }

                [Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
                public void CopyTo2 ()
                {
                        ListDictionary ld = new ListDictionary ();
                        ld.CopyTo (new int[1],-1);       
                }

                [Test, ExpectedException (typeof (ArgumentNullException))]
                public void Remove ()
                {
                        ListDictionary ld = new ListDictionary ();
                        ld.Remove (null);
                }
        }
}
