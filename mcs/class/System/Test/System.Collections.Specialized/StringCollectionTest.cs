// System.Collections.Specialized.StringCollection.cs
//
// Authors:
//   John Barnette (jbarn@httcb.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
//  (C) Copyright 2001 John Barnette
//  (C) Copyright 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
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
			Assertion.Assert(sc.Count == 7);
		}
		
		[Test]
		public void SimpleIsReadOnly() 
		{
			Assertion.Assert(!sc.IsReadOnly);
		}
		
		[Test]
		public void SimpleIsSynchronized() 
		{
			Assertion.Assert(!sc.IsSynchronized);
		}
		
		[Test]
		public void SimpleItemGet() 
		{
			for(int i = 0; i < strings.Length; i++) {
				Assertion.Assert(strings[i].Equals(sc[i]));
			}
		}
		
		[Test]
		public void SimpleItemSet() 
		{
			sc[0] = "bob";
			Assertion.Assert(sc[0].Equals("bob"));
		}
		
		[Test]
		public void SimpleSyncRoot() 
		{
			Assertion.Assert(sc.Equals(sc.SyncRoot));
		}
		
		[Test]
		public void SimpleAdd() 
		{
			int index = sc.Add("chuck");
			Assertion.Assert(index == strings.Length);
			Assertion.Assert(sc[strings.Length].Equals("chuck"));
			
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
			
			Assertion.Assert(sc.Count == index + newStrings.Length);
			
			for (int i = 0; i+index <= sc.Count-1; i++) {
				Assertion.Assert(newStrings[i].Equals(sc[i+index]));
			}
		}
		
		[Test]
		public void SimpleClear() 
		{
			sc.Clear();
			Assertion.Assert(sc.Count == 0);
		}
		
		[Test]
		public void SimpleContains() 
		{
			Assertion.Assert(sc.Contains(strings[0]));
			Assertion.Assert(!sc.Contains("NOT CONTAINED"));
		}
		
		[Test]
		public void SimpleCopyTo() 
		{
			string[] copyArray = new string[sc.Count];
			sc.CopyTo(copyArray, 0);
			for (int i = 0; i < copyArray.Length; i++) {
				Assertion.Assert(copyArray[i] == sc[i]);
			}
		}
		
		[Test]
		public void SimpleGetEnumerator() 
		{
			int index = 0;
			foreach(string s in sc) {
				Assertion.Assert(s.Equals(strings[index]));
				index++;
			}
		}
		
		[Test]
		public void SimpleIndexOf() 
		{
			Assertion.Assert(sc.IndexOf(strings[0]) == 0);
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
			
			Assertion.Assert(sc.Count == oldCount + 1);
			Assertion.Assert(sc[index].Equals(newStr));
			Assertion.Assert(sc[index-1].Equals(before));
			Assertion.Assert(sc[index+1].Equals(current));
			Assertion.Assert(sc[index+2].Equals(after));
		}
		
		[Test]
		public void SimpleRemove() 
		{
			int oldCount = sc.Count;
			sc.Remove(strings[0]);
			Assertion.Assert(oldCount == sc.Count + 1);
			Assertion.Assert(!sc.Contains(strings[0]));
		}
		
		[Test]
		public void SimpleRemoveAt() 
		{
			int index = 3;
			int oldCount = sc.Count;
			string after = sc[index+1];
			
			sc.RemoveAt(index);
			Assertion.Assert(oldCount == sc.Count + 1);
			Assertion.Assert(sc[index].Equals(after));
		}
	}
}
