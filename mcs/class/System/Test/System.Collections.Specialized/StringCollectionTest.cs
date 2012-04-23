// System.Collections.Specialized.StringCollection.cs
//
// Authors:
//   John Barnette (jbarn@httcb.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) Copyright 2001 John Barnette
//  (C) Copyright 2003 Martin Willemoes Hansen
//  Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System.Collections;
using System.Collections.Specialized;

namespace MonoTests.System.Collections.Specialized {

	[TestFixture]
	public class StringCollectionTest {

		private StringCollection sc;
		string[] strings = {
			"foo",
			"bar",
			"baz",
			"john",
			"paul",
			"george",
			"ringo"
		};
		
		[SetUp]
		public void GetReady() 
		{
			sc = new StringCollection();
			sc.AddRange(strings);
		}

		// Simple Tests
		[Test]
		public void SimpleCount() 
		{
			Assert.AreEqual(7, sc.Count);
		}
		
		[Test]
		public void SimpleIsReadOnly() 
		{
			Assert.IsFalse(sc.IsReadOnly);
		}
		
		[Test]
		public void SimpleIsSynchronized() 
		{
			Assert.IsFalse(sc.IsSynchronized);
		}
		
		[Test]
		public void SimpleItemGet() 
		{
			for(int i = 0; i < strings.Length; i++) {
				Assert.AreEqual(strings[i], sc[i]);
			}
		}
		
		[Test]
		public void SimpleItemSet() 
		{
			sc[0] = "bob";
			Assert.AreEqual("bob", sc[0]);
		}
		
		[Test]
		public void SimpleSyncRoot() 
		{
			Assert.AreEqual(sc, sc.SyncRoot);
		}
		
		[Test]
		public void SimpleAdd() 
		{
			int index = sc.Add("chuck");
			Assert.AreEqual(strings.Length, index);
			Assert.AreEqual("chuck", sc[strings.Length]);
		}
		
		[Test]
		public void SimpleAddRange() 
		{
			string[] newStrings = {
				"peter",
				"paul",
				"mary"
			};
			
			int index = sc.Count;
			sc.AddRange(newStrings);
			
			Assert.AreEqual(index + newStrings.Length, sc.Count);
			
			for (int i = 0; i+index <= sc.Count-1; i++) {
				Assert.AreEqual(newStrings[i], sc[i+index]);
			}
		}
		
		[Test]
		public void SimpleClear() 
		{
			sc.Clear();
			Assert.AreEqual(0, sc.Count);
		}
		
		[Test]
		public void SimpleContains() 
		{
			Assert.IsTrue(sc.Contains(strings[0]));
			Assert.IsFalse(sc.Contains("NOT CONTAINED"));
		}
		
		[Test]
		public void SimpleCopyTo() 
		{
			string[] copyArray = new string[sc.Count];
			sc.CopyTo(copyArray, 0);
			for (int i = 0; i < copyArray.Length; i++) {
				Assert.AreEqual(copyArray[i], sc[i]);
			}
		}
		
		[Test]
		public void SimpleGetEnumerator() 
		{
			int index = 0;
			foreach(string s in sc) {
				Assert.AreEqual(s, strings[index]);
				index++;
			}
		}
		
		[Test]
		public void SimpleIndexOf() 
		{
			Assert.AreEqual(0, sc.IndexOf(strings[0]));
		}
		
		[Test]
		public void SimpleInsert() 
		{
			int index = 3;
			int oldCount = sc.Count;
			string before  = sc[index - 1];
			string current = sc[index];
			string after   = sc[index + 1];
			string newStr  = "paco";
			
			sc.Insert(index, newStr);
			
			Assert.AreEqual(oldCount + 1, sc.Count);
			Assert.AreEqual(newStr, sc[index]);
			Assert.AreEqual(before, sc[index-1]);
			Assert.AreEqual(current, sc[index+1]);
			Assert.AreEqual(after, sc[index+2]);
		}
		
		[Test]
		public void SimpleRemove() 
		{
			int oldCount = sc.Count;
			sc.Remove(strings[0]);
			Assert.AreEqual(sc.Count + 1, oldCount);
			Assert.IsFalse(sc.Contains(strings[0]));
		}
		
		[Test]
		public void SimpleRemoveAt() 
		{
			int index = 3;
			int oldCount = sc.Count;
			string after = sc[index+1];
			
			sc.RemoveAt(index);
			Assert.AreEqual(sc.Count + 1, oldCount);
			Assert.AreEqual(after, sc[index]);
		}

		[Test]
		public void IList ()
		{
			IList list = (IList) new StringCollection ();
			Assert.AreEqual (0, list.Count, "Count-0");
			Assert.IsFalse (list.IsFixedSize, "IsFixedSize");
			Assert.IsFalse (list.IsFixedSize, "IsReadOnly");

			list.Add ("a");
			Assert.AreEqual (1, list.Count, "Count-1");
			Assert.IsTrue (list.Contains ("a"), "Contains(b)");
			Assert.IsFalse (list.Contains ("b"), "Contains(b)");

			Assert.AreEqual (0, list.IndexOf ("a"), "IndexOf(a)");
			Assert.AreEqual (-1, list.IndexOf ("b"), "IndexOf(b)");

			list.Insert (0, "b");
			Assert.AreEqual (2, list.Count, "Count-2");
			list.Remove ("b");
			Assert.AreEqual (1, list.Count, "Count-3");

			list.Add ("b");
			list.RemoveAt (0);
			Assert.AreEqual (1, list.Count, "Count-4");

			list.Clear ();
			Assert.AreEqual (0, list.Count, "Count-5");
		}

		[Test]
		public void ICollection ()
		{
			ICollection coll = (ICollection) new StringCollection ();
			Assert.AreEqual (0, coll.Count, "Count");
			Assert.IsNotNull (coll.GetEnumerator (), "GetEnumerator");
			coll.CopyTo (new string[0], 0);
			Assert.IsFalse (coll.IsSynchronized, "IsSynchronized");
			Assert.IsNotNull (coll.SyncRoot, "SyncRoot");
		}

		[Test]
		public void IEnumerable ()
		{
			IEnumerable e = (IEnumerable) new StringCollection ();
			Assert.IsNotNull (e.GetEnumerator (), "GetEnumerator");
		}
	}
}
