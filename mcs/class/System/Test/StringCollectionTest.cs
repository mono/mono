/* System.Collections.Specialized.StringCollection.cs
 * Authors:
 *   John Barnette (jbarn@httcb.net)
 *
 *  Copyright (C) 2001 John Barnette
*/

using NUnit.Framework;

namespace Ximian.Mono.Tests.System.Collections.Specialized {
	public class StringCollectionTest : TestCase {

		public static ITest Suite {
			get {
				return new TestSuite(typeof (StringCollectionTest));
			}
		}

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
		
		public StringCollectionTest() : base("Ximian.Mono.Tests.System.Collections.Specialized.StringCollectionTest testsuite") {}
		public StringCollectionTest(string name) : base(name) {}
		
		protected override void SetUp() {
			sc = new StringCollection();
			sc.AddRange(strings);
		}

		// Simple Tests
		
		public void TestSimpleCount() {
			Assert(sc.Count == 7);
		}
		
		public void TestSimpleIsReadOnly() {
			Assert(!sc.IsReadOnly);
		}
		
		public void TestSimpleIsSynchronized() {
			Assert(!sc.IsSynchronized);
		}
		
		public void TestSimpleItemGet() {
			for(int i = 0; i < strings.Length; i++) {
				Assert(strings[i].Equals(sc[i]));
			}
		}
		
		public void TestSimpleItemSet() {
			sc[0] = "bob";
			Assert(sc[0].Equals("bob"));
		}
		
		public void TestSimpleSyncRoot() {
			Assert(sc.Equals(sc.SyncRoot));
		}
		
		public void TestSimpleAdd() {
			int index = sc.Add("chuck");
			Assert(index == strings.Length);
			Assert(sc[strings.Length].Equals("chuck"));
			
		}
		
		public void TestSimpleAddRange() {
			string[] newStrings = {
				"peter",
				"paul",
				"mary"
			};
			
			int index = sc.Count;
			sc.AddRange(newStrings);
			
			Assert(sc.Count == index + newStrings.Length);
			
			for (int i = 0; i+index <= sc.Count-1; i++) {
				Assert(newStrings[i].Equals(sc[i+index]));
			}
		}
		
		public void TestSimpleClear() {
			sc.Clear();
			Assert(sc.Count == 0);
		}
		
		public void TestSimpleContains() {
			Assert(sc.Contains(strings[0]));
			Assert(!sc.Contains("NOT CONTAINED"));
		}
		
		public void TestSimpleCopyTo() {
			string[] copyArray = new string[sc.Count];
			sc.CopyTo(copyArray, 0);
			for (int i = 0; i < copyArray.Length; i++) {
				Assert(copyArray[i] == sc[i]);
			}
		}
		
		public void TestSimpleGetEnumerator() {
			int index = 0;
			foreach(string s in sc) {
				Assert(s.Equals(strings[index]));
				index++;
			}
		}
		
		public void TestSimpleIndexOf() {
			Assert(sc.IndexOf(strings[0]) == 0);
		}
		
		public void TestSimpleInsert() {
			int index = 3;
			int oldCount = sc.Count;
			string before  = sc[index - 1];
			string current = sc[index];
			string after   = sc[index + 1];
			string newStr  = "paco";
			
			sc.Insert(index, newStr);
			
			Assert(sc.Count == oldCount + 1);
			Assert(sc[index].Equals(newStr));
			Assert(sc[index-1].Equals(before));
			Assert(sc[index+1].Equals(current));
			Assert(sc[index+2].Equals(after));
		}
		
		public void TestSimpleRemove() {
			int oldCount = sc.Count;
			sc.Remove(strings[0]);
			Assert(oldCount == sc.Count + 1);
			Assert(!sc.Contains(strings[0]));
		}
		
		public void TestSimpleRemoveAt() {
			int index = 3;
			int oldCount = sc.Count;
			string after = sc[index+1];
			
			sc.RemoveAt(index);
			Assert(oldCount == sc.Count + 1);
			Assert(sc[index].Equals(after));
		}
			
	}
}
