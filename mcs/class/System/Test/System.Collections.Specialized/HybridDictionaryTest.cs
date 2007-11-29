//
// HybridDictionaryTest.cs - NUnit Test Cases for System.Net.HybridDictionary
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Martin Willemoes Hansen
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace MonoTests.System.Collections.Specialized
{
	[TestFixture]
	public class HybridDictionaryTest {

		[Test]
		public void DefaultValues ()
		{
			HybridDictionary hd = new HybridDictionary (100);
			Assert.AreEqual (0, hd.Count, "Count");
			Assert.IsFalse (hd.IsFixedSize, "IsFixedSize");
			Assert.IsFalse (hd.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (hd.IsSynchronized, "IsSynchronized");
			Assert.AreEqual (0, hd.Keys.Count, "Keys");
			Assert.AreEqual (0, hd.Values.Count, "Values");
			Assert.AreSame (hd, hd.SyncRoot, "SyncRoot");
			Assert.IsNotNull (hd.GetEnumerator (), "GetEnumerator");
			Assert.IsNotNull ((hd as IEnumerable).GetEnumerator (), "IEnumerable.GetEnumerator");
		}

		[Test]
		public void All ()
		{
			HybridDictionary dict = new HybridDictionary (true);
			dict.Add ("CCC", "ccc");
			dict.Add ("BBB", "bbb");
			dict.Add ("fff", "fff");
			dict ["EEE"] = "eee";
			dict ["ddd"] = "ddd";
			
			Assert.AreEqual (5, dict.Count, "#1");
			Assert.AreEqual ("eee", dict["eee"], "#2");
			
			dict.Add ("CCC2", "ccc");
			dict.Add ("BBB2", "bbb");
			dict.Add ("fff2", "fff");
			dict ["EEE2"] = "eee";
			dict ["ddd2"] = "ddd";
			dict ["xxx"] = "xxx";
			dict ["yyy"] = "yyy";

			Assert.AreEqual (12, dict.Count, "#3");
			Assert.AreEqual ("eee", dict["eee"], "#4");

			dict.Remove ("eee");
			Assert.AreEqual (11, dict.Count, "Removed/Count");
			Assert.IsFalse (dict.Contains ("eee"), "Removed/Contains(xxx)");
			DictionaryEntry[] entries = new DictionaryEntry [11];
			dict.CopyTo (entries, 0);

			Assert.IsTrue (dict.Contains ("xxx"), "Contains(xxx)");
			dict.Clear ();
			Assert.AreEqual (0, dict.Count, "Cleared/Count");
			Assert.IsFalse (dict.Contains ("xxx"), "Cleared/Contains(xxx)");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void Empty () 
		{
			HybridDictionary hd = new HybridDictionary ();
			Assert.AreEqual (0, hd.Count, "Count");
			Assert.IsFalse (hd.Contains ("unexisting"), "unexisting");
			// under 1.x no exception, under 2.0 ArgumentNullException
			Assert.IsFalse (hd.Contains (null), "Contains(null)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NotEmpty () 
		{
			HybridDictionary hd = new HybridDictionary (1, false);
			hd.Add ("CCC", "ccc");
			Assert.AreEqual (1, hd.Count, "Count");
			// ArgumentNullException under all fx versions
			Assert.IsFalse (hd.Contains (null), "Contains(null)");
		}
	}
}
