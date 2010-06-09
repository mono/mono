// KeyedCollectionTest.cs - NUnit Test Cases for System.Collections.ObjectModel.KeyedCollection
//
// Carlo Kok (ck@carlo-kok.com)
//
// (C) Carlo Kok
// 

#if NET_2_0
using NUnit.Framework;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonoTests.System.Collections.ObjectModel
{

	[TestFixture]
	public class KeyedCollectionTest 
	{
		private class CaseInsensitiveComparer : IEqualityComparer<string>
		{
			public CaseInsensitiveComparer () { }

			#region IEqualityComparer<string> Members

			public bool Equals (string x, string y)
			{
				return String.Compare (x, y, true,
					CultureInfo.InvariantCulture) == 0;
			}

			public int GetHashCode (string obj)
			{
				return obj.ToUpper (CultureInfo.InvariantCulture).GetHashCode ();
			}
			#endregion
		}

		private class StrKeyCollection : KeyedCollection<string, string>
		{
			public StrKeyCollection (
				IEqualityComparer<string> comparer,
				int dictionaryCreationThreshold):
				base (comparer, dictionaryCreationThreshold)
			{
			}

			protected override string GetKeyForItem (string item)
			{
				return "Key:" + item;
			}

			public IDictionary<string, string> GetDictionary()
			{
				return Dictionary;
			}
		}

		public KeyedCollectionTest ()
		{
		}

		[Test]
		public void TestDelete ()
		{
			StrKeyCollection collection =
				new StrKeyCollection (EqualityComparer<string>.Default, 2);
			collection.Add ("One"); // Key:First
			collection.Add ("Two"); // Key:Two
			Assert.IsTrue (collection.Remove ("Key:One"));
			collection.Add ("Four"); // Key:Four
			collection.Insert (2, "Three"); // Key:Three
			Assert.IsTrue (collection.Remove ("Key:Three"));

			Assert.IsFalse (collection.Remove ("Unknown"));

			Assert.AreEqual (collection.GetDictionary ().Count, 2);


			Assert.AreEqual (collection.Count, 2, "Collection count not equal to 2");
			// check if all items are ordered correctly

			Assert.AreEqual (collection [0], "Two");
			Assert.AreEqual (collection [1], "Four");

			Assert.AreEqual (collection ["Key:Two"], "Two");
			Assert.AreEqual (collection ["Key:Four"], "Four");

			try {
				collection ["Key:One"].ToString();
				Assert.Fail ("Unknown key should fail");
			} catch (KeyNotFoundException e) {
				e.ToString(); // avoid warning
				// oke
			}

			try {
				collection ["Key:One"].ToString();
				Assert.Fail ("Unknown key should fail");
			} catch (KeyNotFoundException e) {
				e.ToString (); // avoid warning
				// oke
			}
		}

		[Test]
		public void Insert_InvalidIndexDoesNotInsert ()
		{
			StrKeyCollection collection =
				new StrKeyCollection(EqualityComparer<string>.Default, 0);
			InsertInvalidIndex (collection, -1);
			InsertInvalidIndex (collection, 1);
		}

		void InsertInvalidIndex (StrKeyCollection collection, int index)
		{
			try {
				collection.Insert (index, "One");
				Assert.Fail ("Invalid index should fail.");
			}
			catch (ArgumentOutOfRangeException) {
			}
			catch (Exception e) {
				Assert.Fail ("Invalid exception type: expected ArgumentOutOfRangeException, got " + e.GetType ().Name);
			}

			IDictionary<string, string> dict = collection.GetDictionary ();
			if (dict != null) {
				Assert.AreEqual (0, dict.Count);
				Assert.IsFalse (dict.ContainsKey ("Key:One"));
			}
		}

		[Test]
		public void Insert_DuplicateKey ()
		{
			StrKeyCollection collection =
				new StrKeyCollection(EqualityComparer<string>.Default, 0);
			collection.Add ("One");
			try {
				collection.Add ("One");
				Assert.Fail ("Duplicate keys should throw ArgumentException.");
			}
			catch (ArgumentException) {
			}
			catch (Exception e) {
				Assert.Fail ("Invalid exception type: expected ArgumentOutOfRangeException, got " + e.GetType ().Name);
			}
			Assert.AreEqual (1, collection.Count);
			Assert.AreEqual (1, collection.GetDictionary ().Count);
		}

		[Test]
		public void Insert_DuplicateKey_NoDictionary ()
		{
			StrKeyCollection collection =
				new StrKeyCollection(EqualityComparer<string>.Default, 4);
			collection.Add ("One");
			try {
				collection.Add ("One");
				Assert.Fail ("Duplicate keys should throw ArgumentException.");
			}
			catch (ArgumentException) {
			}
			catch (Exception e) {
				Assert.Fail ("Invalid exception type: expected ArgumentOutOfRangeException, got " + e.GetType ().Name);
			}
			Assert.AreEqual (1, collection.Count);
			Assert.AreEqual (null, collection.GetDictionary ());
		}

		[Test]
		public void TestInsert ()
		{
			StrKeyCollection collection =
				new StrKeyCollection(EqualityComparer<string>.Default, 2);

			Assert.IsNull (collection.GetDictionary (), 
				"Dictionary created too early"); // There can't be a dictionary yet

			collection.Add ("One"); // Key:First

			Assert.IsNull (collection.GetDictionary(),
				"Dictionary created too early"); // There can't be a dictionary yet

			collection.Add ("Two"); // Key:Two

			Assert.IsNull (collection.GetDictionary (),
				"Dictionary created too early"); // There can't be a dictionary yet

			collection.Add ("Four"); // Key:Four

			Assert.IsNotNull(collection.GetDictionary (),
				"Dictionary created too late"); // There must be a dictionary 

			collection.Insert (2, "Three"); // Key:Three

			Assert.AreEqual (collection.Count, 4,
				"Collection count not equal to 4");
			// check if all items are ordered correctly

			Assert.AreEqual (collection [0], "One");
			Assert.AreEqual (collection [1], "Two");
			Assert.AreEqual (collection [2], "Three");
			Assert.AreEqual (collection [3], "Four");

			Assert.AreEqual (collection ["Key:One"], "One");
			Assert.AreEqual (collection ["Key:Two"], "Two");
			Assert.AreEqual (collection ["Key:Three"], "Three");
			Assert.AreEqual (collection ["Key:Four"], "Four");

			Assert.AreEqual (collection.GetDictionary ().Count, 4);

			try {
				collection ["UnkownKey"].ToString();
				Assert.Fail ("Unknown key should fail");
			} catch(KeyNotFoundException e) {
				e.ToString(); // avoid warning
				// oke
			}
		}
	}
}
#endif
